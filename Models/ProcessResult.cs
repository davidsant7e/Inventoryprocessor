namespace InventoryProcessor.Models;

public class ProcessResult
{
    public bool Success { get; set; }
    public List<string> LogMessages { get; set; } = new();
    public List<string> ValidationErrors { get; set; } = new();

    // Summary counters (bonus)
    public int TotalProducts { get; set; }
    public int ModifiedProducts { get; set; }
    public int CriticalCount { get; set; }
    public int ReorderCount { get; set; }
    public int OutOfStockCount { get; set; }
    public int OkCount { get; set; }
}
