using InventoryProcessor.Models;
using static InventoryProcessor.Models.DomainConstants;

namespace InventoryProcessor.Services;

public class InventoryProcessorService : IInventoryProcessor
{
    private static readonly (string[] Keywords, string Category, string Prefix)[] CategoryRules =
    {
        (Keywords.Electronics, Categories.Electronics, Prefixes.Electronics),
        (Keywords.Peripherals, Categories.Peripherals, Prefixes.Peripherals),
        (Keywords.Furniture,   Categories.Furniture,   Prefixes.Furniture),
        (Keywords.Accessories, Categories.Accessories, Prefixes.Accessories),
    };

    public ProcessResult Process(InventoryFile inventory)
    {
        var result = new ProcessResult();
        var log = result.LogMessages;

        var dupIds = inventory.Products
            .GroupBy(p => p.Id)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (dupIds.Any())
        {
            string ids = string.Join(", ", dupIds);
            result.ValidationErrors.Add(string.Format(ProcessMessages.Errors.DupIdsFmt, ids));
            log.Add(string.Format(ProcessMessages.Log.DupIdsAbortedFmt, ids));
            result.Success = false;
            return result;
        }

        var originals = inventory.Products
            .ToDictionary(p => p.Id, p => (p.Category, p.Status, p.Sku));

        ApplyCategorization(inventory.Products, log);
        ApplyStatusRecalculation(inventory.Products, log);
        ApplySkuGeneration(inventory.Products, log);
        DetectInvalidSkuPrefixes(inventory.Products, log);
        ApplyPriceCheck(inventory.Products, log, result.ValidationErrors);

        result.ValidationErrors.AddRange(Validate(inventory));
        result.Success = result.ValidationErrors.Count == 0;

        result.TotalProducts = inventory.Products.Count;
        result.ModifiedProducts = inventory.Products.Count(p =>
        {
            var orig = originals[p.Id];
            return orig.Category != p.Category || orig.Status != p.Status || orig.Sku != p.Sku;
        });
        result.CriticalCount   = inventory.Products.Count(p => p.Status == Status.Critical);
        result.ReorderCount    = inventory.Products.Count(p => p.Status == Status.Reorder);
        result.OutOfStockCount = inventory.Products.Count(p => p.Status == Status.OutOfStock);
        result.OkCount         = inventory.Products.Count(p => p.Status == Status.Ok);

        if (result.Success)
            log.Insert(0, ProcessMessages.Log.ProcessSucceeded);
        else
        {
            log.Insert(0, ProcessMessages.Log.ProcessFailed);
            foreach (var err in result.ValidationErrors)
                log.Add(string.Format(ProcessMessages.Log.ErrorLineFmt, err));
        }

        return result;
    }

    // -------------------------------------------------------
    // Rule 3.1 — Auto-categorization
    // -------------------------------------------------------
    private static void ApplyCategorization(List<Product> products, List<string> log)
    {
        foreach (var product in products)
        {
            if (!string.IsNullOrWhiteSpace(product.Category)) continue;

            string nameLower = product.Name.ToLowerInvariant();
            string? matched = null;

            foreach (var rule in CategoryRules)
            {
                if (rule.Keywords.Any(k => nameLower.Contains(k)))
                {
                    matched = rule.Category;
                    break;
                }
            }

            if (matched is not null)
            {
                log.Add(string.Format(ProcessMessages.Log.CategoryAssignedFmt, product.Name, matched));
                product.Category = matched;
            }
            else
            {
                product.Category = Categories.Uncategorized;
                log.Add(string.Format(ProcessMessages.Log.CategoryNoMatchFmt, product.Name, Categories.Uncategorized));
            }
        }
    }

    // -------------------------------------------------------
    // Rule 3.2 — Status recalculation
    // -------------------------------------------------------
    private static void ApplyStatusRecalculation(List<Product> products, List<string> log)
    {
        foreach (var product in products)
        {
            string newStatus = product.Stock switch
            {
                0                                         => Status.OutOfStock,
                > 0 when product.Stock < product.MinStock => Status.Critical,
                _ when product.Stock == product.MinStock  => Status.Reorder,
                _                                         => Status.Ok
            };

            if (product.Status != newStatus)
            {
                log.Add(string.Format(ProcessMessages.Log.StatusChangedFmt, product.Name, product.Status ?? "null", newStatus));
                product.Status = newStatus;
            }
        }
    }

    // -------------------------------------------------------
    // Rule 3.3 — SKU generation
    // -------------------------------------------------------
    private static void ApplySkuGeneration(List<Product> products, List<string> log)
    {
        foreach (var p in products.Where(p => p.Sku != null && string.IsNullOrWhiteSpace(p.Sku)))
        {
            log.Add(string.Format(ProcessMessages.Log.SkuEmptyFmt, p.Name, p.Id));
            p.Sku = null;
        }

        var byCategory = products
            .GroupBy(p => p.Category ?? Categories.Uncategorized)
            .ToList();

        foreach (var group in byCategory)
        {
            if (group.Key == Categories.Uncategorized)
            {
                foreach (var p in group.Where(p => p.Sku == null))
                    log.Add(string.Format(ProcessMessages.Log.SkuNoCategoryFmt, p.Name, p.Id, Prefixes.Uncategorized));
            }

            string prefix = GetPrefix(group.Key);

            var skuGroups = group
                .Where(p => p.Sku != null)
                .GroupBy(p => p.Sku!)
                .Where(g => g.Count() > 1)
                .ToList();

            foreach (var dupGroup in skuGroups)
            {
                foreach (var dup in dupGroup.OrderBy(p => p.Id).Skip(1))
                {
                    log.Add(string.Format(ProcessMessages.Log.SkuDuplicateFmt, dup.Name, dup.Sku));
                    dup.Sku = null;
                }
            }

            int lastNumber = group
                .Where(p => p.Sku != null && p.Sku.StartsWith(prefix + "-"))
                .Select(p =>
                {
                    string numPart = p.Sku!.Substring(prefix.Length + 1);
                    return int.TryParse(numPart, out int n) ? n : 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            foreach (var product in group.Where(p => p.Sku == null).OrderBy(p => p.Id))
            {
                product.Sku = $"{prefix}-{++lastNumber:D4}";
                log.Add(string.Format(ProcessMessages.Log.SkuAssignedFmt, product.Name, product.Sku));
            }
        }
    }

    // -------------------------------------------------------
    // Rule 3.4 — Detect SKUs with invalid prefix for category
    // -------------------------------------------------------
    private static void DetectInvalidSkuPrefixes(List<Product> products, List<string> log)
    {
        var validPrefixes = CategoryRules.Select(r => r.Prefix)
            .Append(Prefixes.Uncategorized)
            .ToHashSet();

        foreach (var product in products)
        {
            if (string.IsNullOrWhiteSpace(product.Sku)) continue;

            string skuPrefix = product.Sku.Contains('-')
                ? product.Sku.Split('-')[0].ToUpperInvariant()
                : product.Sku.ToUpperInvariant();

            string expectedPrefix = GetPrefix(product.Category ?? Categories.Uncategorized);

            if (!validPrefixes.Contains(skuPrefix))
            {
                log.Add(string.Format(ProcessMessages.Log.SkuUnknownPrefixFmt, product.Name, skuPrefix, product.Sku));
                continue;
            }

            if (skuPrefix != expectedPrefix)
            {
                string? wrongCategory = CategoryRules.FirstOrDefault(r => r.Prefix == skuPrefix).Category;
                log.Add(string.Format(ProcessMessages.Log.SkuWrongPrefixFmt, product.Name, product.Category, product.Sku, skuPrefix, wrongCategory, expectedPrefix));
            }
        }
    }

    // -------------------------------------------------------
    // Validation before export
    // -------------------------------------------------------
    public List<string> Validate(InventoryFile inventory)
    {
        var errors = new List<string>();
        var products = inventory.Products;

        var dupIds = products.GroupBy(p => p.Id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (dupIds.Any())
            errors.Add(string.Format(ProcessMessages.Errors.DupIdsFmt, string.Join(", ", dupIds)));

        foreach (var p in products)
        {
            if (string.IsNullOrWhiteSpace(p.Category))
                errors.Add(string.Format(ProcessMessages.Errors.EmptyCategoryFmt, p.Name, p.Id));

            string expected = p.Stock switch
            {
                0                             => Status.OutOfStock,
                > 0 when p.Stock < p.MinStock => Status.Critical,
                _ when p.Stock == p.MinStock  => Status.Reorder,
                _                             => Status.Ok
            };
            if (p.Status != expected)
                errors.Add(string.Format(ProcessMessages.Errors.StatusMismatchFmt, p.Name, p.Id, p.Status, expected));

            if (string.IsNullOrWhiteSpace(p.Sku))
                errors.Add(string.Format(ProcessMessages.Errors.NullSkuFmt, p.Name, p.Id));

            if (p.Stock < 0)
                errors.Add(string.Format(ProcessMessages.Errors.NegativeStockFmt, p.Name, p.Id));
        }

        var skuDups = products
            .Where(p => !string.IsNullOrWhiteSpace(p.Sku))
            .GroupBy(p => new { p.Category, p.Sku })
            .Where(g => g.Count() > 1)
            .ToList();

        foreach (var dup in skuDups)
            errors.Add(string.Format(ProcessMessages.Errors.DupSkuFmt, dup.Key.Sku, dup.Key.Category));

        var validPrefixes = CategoryRules.Select(r => r.Prefix)
            .Append(Prefixes.Uncategorized)
            .ToHashSet();
        foreach (var p in products.Where(p => !string.IsNullOrWhiteSpace(p.Sku)))
        {
            string skuPrefix = p.Sku!.Contains('-')
                ? p.Sku.Split('-')[0].ToUpperInvariant()
                : p.Sku.ToUpperInvariant();

            if (!validPrefixes.Contains(skuPrefix))
            {
                errors.Add(string.Format(ProcessMessages.Errors.UnknownSkuPrefixFmt, p.Name, p.Id, p.Sku, skuPrefix));
                continue;
            }

            string expectedPrefix = GetPrefix(p.Category ?? Categories.Uncategorized);
            if (skuPrefix != expectedPrefix)
            {
                string? wrongCategory = CategoryRules.FirstOrDefault(r => r.Prefix == skuPrefix).Category;
                errors.Add(string.Format(ProcessMessages.Errors.WrongSkuPrefixFmt, p.Name, p.Id, p.Sku, skuPrefix, wrongCategory, p.Category, expectedPrefix));
            }
        }

        return errors;
    }

    private static void ApplyPriceCheck(List<Product> products, List<string> log, List<string> errors)
    {
        foreach (var p in products.Where(p => p.UnitPrice <= 0))
        {
            log.Add(string.Format(ProcessMessages.Log.PriceInvalidFmt, p.Name, p.Id));
            errors.Add(string.Format(ProcessMessages.Errors.InvalidPriceFmt, p.Name, p.Id));
        }
    }

    private static string GetPrefix(string category) =>
        CategoryRules.FirstOrDefault(r => r.Category == category).Prefix ?? Prefixes.Uncategorized;

}
