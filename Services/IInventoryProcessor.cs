using InventoryProcessor.Models;

namespace InventoryProcessor.Services;

public interface IInventoryProcessor
{
    ProcessResult Process(InventoryFile inventory);
    List<string> Validate(InventoryFile inventory);
}
