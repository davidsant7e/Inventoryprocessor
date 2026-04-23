namespace InventoryProcessor.Models;

public static class DomainConstants
{
    public static class Categories
    {
        public const string Electronics   = "Electrónica";
        public const string Peripherals   = "Periféricos";
        public const string Furniture     = "Mobiliario";
        public const string Accessories   = "Accesorios";
        public const string Uncategorized = "Sin categoría";
    }

    public static class Prefixes
    {
        public const string Electronics  = "ELC";
        public const string Peripherals  = "PER";
        public const string Furniture    = "MOB";
        public const string Accessories  = "ACC";
        public const string Uncategorized = "SIN";
    }

    public static class Keywords
    {
        public static readonly string[] Electronics = { "laptop", "monitor" };
        public static readonly string[] Peripherals = { "teclado", "mouse", "auricular", "webcam" };
        public static readonly string[] Furniture   = { "silla", "mesa" };
        public static readonly string[] Accessories = { "cable", "hub" };
    }

    public static class Status
    {
        public const string Ok         = "ok";
        public const string Critical   = "critical";
        public const string Reorder    = "reorder";
        public const string OutOfStock = "out_of_stock";
    }

    public static class AppUI
    {
        public const string WindowTitle = "Inventory Processor";

        public static class Buttons
        {
            public const string Load    = "📂  Cargar JSON";
            public const string Process = "⚙  Procesar";
            public const string Export  = "💾  Exportar JSON";
        }

        public static class Labels
        {
            public const string Log             = "Log de procesamiento:";
            public const string WarehousePrefix = "📦  ";
            public const string GeneratedPrefix = "Generado: ";
        }

        public static class Summary
        {
            public const string TotalDefault    = "Total: —";
            public const string ModifiedDefault = "Modificados: —";
            public const string CriticalDefault = "Críticos: —";
            public const string ReorderDefault  = "Reorder: —";
            public const string OkDefault       = "OK: —";

            public const string TotalFmt    = "Total: {0}";
            public const string ModifiedFmt = "Modificados: {0}";
            public const string CriticalFmt = "Críticos: {0}";
            public const string ReorderFmt  = "Reorder: {0}";
            public const string OkFmt       = "OK: {0}";
        }

        public static class GridCols
        {
            public static class Col
            {
                public const string Id        = "Id";
                public const string Name      = "Name";
                public const string Category  = "Category";
                public const string Stock     = "Stock";
                public const string MinStock  = "MinStock";
                public const string UnitPrice = "UnitPrice";
                public const string Status    = "Status";
                public const string Sku       = "Sku";
            }

            public static class Header
            {
                public const string Id        = "ID";
                public const string Name      = "Nombre";
                public const string Category  = "Categoría";
                public const string Stock     = "Stock";
                public const string MinStock  = "Min. Stock";
                public const string UnitPrice = "Precio";
                public const string Status    = "Status";
                public const string Sku       = "SKU";
            }
        }

        public static class Dialogs
        {
            public const string OpenFilter       = "JSON files (*.json)|*.json|All files (*.*)|*.*";
            public const string OpenTitle        = "Seleccionar archivo de inventario";
            public const string SaveFilter       = "JSON files (*.json)|*.json";
            public const string SaveDefaultFile  = "output.json";
            public const string SaveTitle        = "Guardar inventario procesado";
            public const string ErrorLoadTitle   = "Error al cargar";
            public const string ErrorExportTitle = "Error al exportar";
            public const string ExportOkMessage  = "Archivo exportado correctamente.";
            public const string ExportOkTitle    = "Exportación exitosa";
        }

        public static class LogMsg
        {
            public const string Header          = "=== INICIANDO PROCESAMIENTO ===";
            public const string FileLoadedFmt   = "Archivo cargado: {0}  ({1} productos)";
            public const string FileExportedFmt = "✔ Archivo exportado: {0}";
            public const string PrefixSuccess   = "✔";
            public const string PrefixError     = "✖";
            public const string PrefixErrorLine = "  ERROR";
            public const string PrefixWarning   = "ADVERTENCIA";
        }
    }

    public static class ProcessMessages
    {
        public static class Log
        {
            public const string DupIdsAbortedFmt     = "✖ IDs duplicados detectados ({0}) — procesamiento abortado.";
            public const string ProcessSucceeded     = "✔ Procesamiento exitoso. Validación pasada.";
            public const string ProcessFailed        = "✖ Validación fallida. Revise los errores antes de exportar.";
            public const string ErrorLineFmt         = "  ERROR: {0}";

            public const string CategoryAssignedFmt  = "[Categoría] '{0}': null → '{1}'";
            public const string CategoryNoMatchFmt   = "[Categoría] ADVERTENCIA: '{0}' no coincide con ninguna regla → '{1}'";

            public const string StatusChangedFmt     = "[Status] '{0}': '{1}' → '{2}'";

            public const string SkuEmptyFmt          = "[SKU] '{0}' (id={1}): SKU vacío — se reasignará.";
            public const string SkuNoCategoryFmt     = "[SKU] ADVERTENCIA: '{0}' (id={1}) sin categoría — se usará prefijo '{2}'.";
            public const string SkuDuplicateFmt      = "[SKU] Duplicado detectado: '{0}' tenía SKU '{1}' (id mayor) → se reasignará";
            public const string SkuAssignedFmt       = "[SKU] '{0}': null → '{1}'";
            public const string SkuUnknownPrefixFmt  = "[SKU] ADVERTENCIA: '{0}' tiene prefijo desconocido '{1}' en SKU '{2}' — no corresponde a ninguna categoría válida.";
            public const string SkuWrongPrefixFmt    = "[SKU] ADVERTENCIA: '{0}' (categoría '{1}') tiene SKU '{2}' con prefijo '{3}' que pertenece a '{4}'. Se esperaba '{5}'.";

            public const string PriceInvalidFmt      = "[Precio] ADVERTENCIA: '{0}' (id={1}) tiene precio <= 0.";
        }

        public static class Errors
        {
            public const string DupIdsFmt            = "IDs duplicados: {0}";
            public const string EmptyCategoryFmt     = "Producto '{0}' (id={1}) tiene categoría nula o vacía.";
            public const string StatusMismatchFmt    = "Producto '{0}' (id={1}): status '{2}' no coincide con stock/minStock (esperado: '{3}').";
            public const string NullSkuFmt           = "Producto '{0}' (id={1}) tiene SKU nulo.";
            public const string NegativeStockFmt     = "Producto '{0}' (id={1}) tiene stock negativo.";
            public const string DupSkuFmt            = "SKU duplicado '{0}' en categoría '{1}'.";
            public const string UnknownSkuPrefixFmt  = "Producto '{0}' (id={1}) tiene SKU '{2}' con prefijo desconocido '{3}'.";
            public const string WrongSkuPrefixFmt    = "Producto '{0}' (id={1}) tiene SKU '{2}' con prefijo '{3}' (categoría '{4}') pero su categoría es '{5}' (esperado '{6}').";
            public const string InvalidPriceFmt      = "Producto '{0}' (id={1}) tiene precio <= 0.";
        }
    }
}
