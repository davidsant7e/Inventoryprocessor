using System.Text.Json.Serialization;

namespace InventoryProcessor.Models;

public class InventoryFile
{
    [JsonPropertyName("warehouse")]
    public string Warehouse { get; set; } = string.Empty;

    [JsonPropertyName("generatedAt")]
    public string GeneratedAt { get; set; } = string.Empty;

    [JsonPropertyName("products")]
    public List<Product> Products { get; set; } = new();
}
