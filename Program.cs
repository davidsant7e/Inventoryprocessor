using InventoryProcessor.Services;
using InventoryProcessor.UI;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        // Manual dependency composition - no DI framework needed
        var fileService = new FileService();
        var processor = new InventoryProcessorService();

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm(fileService, processor));
    }
}
