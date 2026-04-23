namespace InventoryProcessor.UI;

internal static class UIColors
{
    internal static class Toolbar
    {
        internal static readonly Color Background = Color.FromArgb(240, 240, 240);
        internal static readonly Color BtnLoad    = Color.FromArgb(0, 120, 215);
        internal static readonly Color BtnProcess = Color.FromArgb(16, 124, 16);
        internal static readonly Color BtnExport  = Color.FromArgb(136, 0, 21);
        internal static readonly Color BtnText    = Color.White;
    }

    internal static class InfoPanel
    {
        internal static readonly Color Background = Color.FromArgb(225, 235, 250);
        internal static readonly Color Warehouse  = Color.FromArgb(0, 60, 130);
        internal static readonly Color Generated  = Color.Gray;
    }

    internal static class SummaryPanel
    {
        internal static readonly Color Background = Color.FromArgb(230, 245, 230);
        internal static readonly Color Default    = Color.FromArgb(30, 30, 30);
        internal static readonly Color Critical   = Color.FromArgb(180, 0, 0);
        internal static readonly Color Reorder    = Color.FromArgb(180, 100, 0);
        internal static readonly Color Ok         = Color.FromArgb(0, 120, 0);
    }

    internal static class Grid
    {
        internal static readonly Color Background   = Color.White;
        internal static readonly Color Lines        = Color.FromArgb(220, 220, 220);
        internal static readonly Color RowCritical  = Color.FromArgb(255, 220, 220);
        internal static readonly Color RowReorder   = Color.FromArgb(255, 240, 200);
        internal static readonly Color RowOk        = Color.White;
    }

    internal static class Log
    {
        internal static readonly Color Background = Color.FromArgb(20, 20, 20);
        internal static readonly Color Default    = Color.LightGreen;
        internal static readonly Color FileEvent  = Color.Cyan;
        internal static readonly Color Success    = Color.LightGreen;
        internal static readonly Color Error      = Color.Tomato;
        internal static readonly Color Warning    = Color.Orange;
        internal static readonly Color Header     = Color.White;
    }
}
