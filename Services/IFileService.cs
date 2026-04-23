using InventoryProcessor.Models;

namespace InventoryProcessor.Services;

public interface IFileService
{
    InventoryFile LoadFromFile(string path);
    void SaveToFile(InventoryFile inventory, string path);
}
