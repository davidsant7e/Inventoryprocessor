using InventoryProcessor.Models;
using InventoryProcessor.Services;
using static InventoryProcessor.Models.DomainConstants;

namespace InventoryProcessor.UI;

public class MainForm : Form
{
    private readonly IFileService _fileService;
    private readonly IInventoryProcessor _processor;

    private InventoryFile? _currentInventory;
    private bool _processedSuccessfully = false;

    private DataGridView _grid = null!;
    private RichTextBox _logBox = null!;
    private Button _btnLoad = null!, _btnProcess = null!, _btnExport = null!;
    private Label _lblWarehouse = null!, _lblGenerated = null!;
    private Panel _summaryPanel = null!;
    private Label _lblTotal = null!, _lblModified = null!, _lblCritical = null!, _lblReorder = null!, _lblOk = null!;

    public MainForm(IFileService fileService, IInventoryProcessor processor)
    {
        _fileService = fileService;
        _processor = processor;
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        Text = AppUI.WindowTitle;
        Size = new Size(1100, 780);
        MinimumSize = new Size(900, 650);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9f);

        var toolbar = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(8, 8, 8, 0) };
        toolbar.BackColor = UIColors.Toolbar.Background;

        _btnLoad    = MakeButton(AppUI.Buttons.Load,    UIColors.Toolbar.BtnLoad);
        _btnProcess = MakeButton(AppUI.Buttons.Process, UIColors.Toolbar.BtnProcess);
        _btnExport  = MakeButton(AppUI.Buttons.Export,  UIColors.Toolbar.BtnExport);
        _btnProcess.Enabled = false;
        _btnExport.Enabled  = false;

        _btnLoad.Click    += OnLoad;
        _btnProcess.Click += OnProcess;
        _btnExport.Click  += OnExport;

        toolbar.Controls.AddRange(new Control[] { _btnLoad, _btnProcess, _btnExport });

        var infoPanel = new Panel { Dock = DockStyle.Top, Height = 28, Padding = new Padding(10, 4, 0, 0) };
        infoPanel.BackColor = UIColors.InfoPanel.Background;
        _lblWarehouse = new Label { AutoSize = true, ForeColor = UIColors.InfoPanel.Warehouse, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
        _lblGenerated = new Label { AutoSize = true, ForeColor = UIColors.InfoPanel.Generated, Left = 400 };
        infoPanel.Controls.AddRange(new Control[] { _lblWarehouse, _lblGenerated });

        _summaryPanel = new Panel { Dock = DockStyle.Top, Height = 40, Padding = new Padding(8, 6, 8, 0), Visible = false };
        _summaryPanel.BackColor = UIColors.SummaryPanel.Background;
        _lblTotal    = MakeSummaryLabel(AppUI.Summary.TotalDefault);
        _lblModified = MakeSummaryLabel(AppUI.Summary.ModifiedDefault);
        _lblCritical = MakeSummaryLabel(AppUI.Summary.CriticalDefault, UIColors.SummaryPanel.Critical);
        _lblReorder  = MakeSummaryLabel(AppUI.Summary.ReorderDefault,  UIColors.SummaryPanel.Reorder);
        _lblOk       = MakeSummaryLabel(AppUI.Summary.OkDefault,       UIColors.SummaryPanel.Ok);
        _summaryPanel.Controls.AddRange(new Control[] { _lblTotal, _lblModified, _lblCritical, _lblReorder, _lblOk });

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 440,
            Panel1MinSize = 200,
            Panel2MinSize = 120
        };

        _grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible = false,
            BorderStyle = BorderStyle.None,
            BackgroundColor = UIColors.Grid.Background,
            GridColor       = UIColors.Grid.Lines
        };
        _grid.CellValueChanged += OnGridCellChanged;
        _grid.CurrentCellDirtyStateChanged += (s, e) =>
        {
            if (_grid.IsCurrentCellDirty) _grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
        };
        _grid.DataError += (s, e) => { };
        split.Panel1.Controls.Add(_grid);

        var logLabel = new Label { Text = AppUI.Labels.Log, Dock = DockStyle.Top, Height = 20, ForeColor = UIColors.InfoPanel.Generated };
        _logBox = new RichTextBox
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            Font = new Font("Consolas", 8.5f),
            BackColor = UIColors.Log.Background,
            ForeColor = UIColors.Log.Default,
            BorderStyle = BorderStyle.None
        };
        split.Panel2.Controls.Add(_logBox);
        split.Panel2.Controls.Add(logLabel);

        Controls.Add(split);
        Controls.Add(_summaryPanel);
        Controls.Add(infoPanel);
        Controls.Add(toolbar);

        BuildGridColumns();
    }

    private void BuildGridColumns()
    {
        var categoryCol = new DataGridViewComboBoxColumn
        {
            Name       = AppUI.GridCols.Col.Category,
            HeaderText = AppUI.GridCols.Header.Category,
            FlatStyle  = FlatStyle.Flat,
            DisplayStyleForCurrentCellOnly = true
        };
        categoryCol.Items.AddRange("",
            Categories.Electronics, Categories.Peripherals,
            Categories.Furniture,   Categories.Accessories,
            Categories.Uncategorized);

        var statusCol = new DataGridViewComboBoxColumn
        {
            Name       = AppUI.GridCols.Col.Status,
            HeaderText = AppUI.GridCols.Header.Status,
            FlatStyle  = FlatStyle.Flat,
            DisplayStyleForCurrentCellOnly = true
        };
        statusCol.Items.AddRange("",
            Status.Ok, Status.Reorder, Status.Critical, Status.OutOfStock);

        _grid.Columns.Clear();
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = AppUI.GridCols.Col.Id,        HeaderText = AppUI.GridCols.Header.Id,        ReadOnly = true  });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = AppUI.GridCols.Col.Name,      HeaderText = AppUI.GridCols.Header.Name,      ReadOnly = true  });
        _grid.Columns.Add(categoryCol);
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = AppUI.GridCols.Col.Stock,     HeaderText = AppUI.GridCols.Header.Stock,     ReadOnly = true  });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = AppUI.GridCols.Col.MinStock,  HeaderText = AppUI.GridCols.Header.MinStock,  ReadOnly = true  });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = AppUI.GridCols.Col.UnitPrice, HeaderText = AppUI.GridCols.Header.UnitPrice, ReadOnly = true  });
        _grid.Columns.Add(statusCol);
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = AppUI.GridCols.Col.Sku,       HeaderText = AppUI.GridCols.Header.Sku,       ReadOnly = false });
    }

    private void RefreshGrid()
    {
        if (_currentInventory is null) return;
        _grid.Rows.Clear();
        foreach (var p in _currentInventory.Products)
        {
            int row = _grid.Rows.Add(
                p.Id, p.Name, p.Category ?? "", p.Stock, p.MinStock,
                p.UnitPrice.ToString("N0"), p.Status ?? "", p.Sku ?? ""
            );
            ColorRow(_grid.Rows[row], p.Status);
        }
    }

    private static void ColorRow(DataGridViewRow row, string? status)
    {
        row.DefaultCellStyle.BackColor = status switch
        {
            Status.Critical or Status.OutOfStock => UIColors.Grid.RowCritical,
            Status.Reorder                       => UIColors.Grid.RowReorder,
            _                                    => UIColors.Grid.RowOk
        };
    }

    private void OnLoad(object? sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog
        {
            Filter = AppUI.Dialogs.OpenFilter,
            Title  = AppUI.Dialogs.OpenTitle
        };
        if (dlg.ShowDialog() != DialogResult.OK) return;

        try
        {
            _currentInventory = _fileService.LoadFromFile(dlg.FileName);
            RefreshGrid();
            _lblWarehouse.Text = AppUI.Labels.WarehousePrefix + _currentInventory.Warehouse;
            _lblGenerated.Text = AppUI.Labels.GeneratedPrefix + _currentInventory.GeneratedAt;
            _btnProcess.Enabled      = true;
            _btnExport.Enabled       = false;
            _processedSuccessfully   = false;
            _summaryPanel.Visible    = false;
            AppendLog(string.Format(AppUI.LogMsg.FileLoadedFmt, dlg.FileName, _currentInventory.Products.Count), UIColors.Log.FileEvent);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, AppUI.Dialogs.ErrorLoadTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OnProcess(object? sender, EventArgs e)
    {
        if (_currentInventory is null) return;
        SyncGridToModel();

        _logBox.Clear();
        AppendLog(AppUI.LogMsg.Header, UIColors.Log.Header);

        var result = _processor.Process(_currentInventory);
        RefreshGrid();

        foreach (var msg in result.LogMessages)
        {
            Color color = msg.StartsWith(AppUI.LogMsg.PrefixSuccess)   ? UIColors.Log.Success
                        : msg.StartsWith(AppUI.LogMsg.PrefixError)     ? UIColors.Log.Error
                        : msg.StartsWith(AppUI.LogMsg.PrefixErrorLine) ? UIColors.Log.Error
                        : msg.Contains(AppUI.LogMsg.PrefixWarning)     ? UIColors.Log.Warning
                        : UIColors.Log.Default;
            AppendLog(msg, color);
        }

        _processedSuccessfully = result.Success;
        _btnExport.Enabled     = result.Success;

        _lblTotal.Text    = string.Format(AppUI.Summary.TotalFmt,    result.TotalProducts);
        _lblModified.Text = string.Format(AppUI.Summary.ModifiedFmt, result.ModifiedProducts);
        _lblCritical.Text = string.Format(AppUI.Summary.CriticalFmt, result.CriticalCount);
        _lblReorder.Text  = string.Format(AppUI.Summary.ReorderFmt,  result.ReorderCount);
        _lblOk.Text       = string.Format(AppUI.Summary.OkFmt,       result.OkCount);
        _summaryPanel.Visible = true;
    }

    private void OnExport(object? sender, EventArgs e)
    {
        if (_currentInventory is null || !_processedSuccessfully) return;

        using var dlg = new SaveFileDialog
        {
            Filter   = AppUI.Dialogs.SaveFilter,
            FileName = AppUI.Dialogs.SaveDefaultFile,
            Title    = AppUI.Dialogs.SaveTitle
        };
        if (dlg.ShowDialog() != DialogResult.OK) return;

        try
        {
            _fileService.SaveToFile(_currentInventory, dlg.FileName);
            AppendLog(string.Format(AppUI.LogMsg.FileExportedFmt, dlg.FileName), UIColors.Log.FileEvent);
            MessageBox.Show(AppUI.Dialogs.ExportOkMessage, AppUI.Dialogs.ExportOkTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, AppUI.Dialogs.ErrorExportTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SyncGridToModel()
    {
        if (_currentInventory is null) return;
        foreach (DataGridViewRow row in _grid.Rows)
        {
            if (row.IsNewRow) continue;
            int id = Convert.ToInt32(row.Cells[AppUI.GridCols.Col.Id].Value);
            var product = _currentInventory.Products.FirstOrDefault(p => p.Id == id);
            if (product is null) continue;
            product.Category = row.Cells[AppUI.GridCols.Col.Category].Value?.ToString();
            product.Status   = row.Cells[AppUI.GridCols.Col.Status].Value?.ToString();
            product.Sku      = row.Cells[AppUI.GridCols.Col.Sku].Value?.ToString();
        }
    }

    private void OnGridCellChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        _processedSuccessfully = false;
        _btnExport.Enabled     = false;
    }

    private void AppendLog(string text, Color color)
    {
        _logBox.SelectionStart  = _logBox.TextLength;
        _logBox.SelectionLength = 0;
        _logBox.SelectionColor  = color;
        _logBox.AppendText(text + "\n");
        _logBox.ScrollToCaret();
    }

    private static Button MakeButton(string text, Color backColor) => new()
    {
        Text      = text,
        Width     = 150,
        Height    = 34,
        Margin    = new Padding(0, 0, 8, 0),
        BackColor = backColor,
        ForeColor = UIColors.Toolbar.BtnText,
        FlatStyle = FlatStyle.Flat,
        Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
        Dock      = DockStyle.Left
    };

    private static Label MakeSummaryLabel(string text, Color? color = null) => new()
    {
        Text      = text,
        AutoSize  = true,
        Dock      = DockStyle.Left,
        ForeColor = color ?? UIColors.SummaryPanel.Default,
        Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
        Padding   = new Padding(0, 0, 20, 0)
    };
}
