using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace FaultCodeAscParser;

/// <summary>ASC 故障报码解析工具主窗口。</summary>
public partial class MainWindow : Window
{
    private IReadOnlyList<AscFrame> _allFrames = Array.Empty<AscFrame>();
    private string _fileStatusText = "请选择 ASC 文件。";

    public MainWindow()
    {
        InitializeComponent();
        ClearFaultVisualTable();
    }

    /// <summary>异步导入 ASC 文件；大文件读取期间显示进度并禁用操作控件，避免重复触发读取。</summary>
    private async void OpenAscFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "选择 Vector ASC 日志文件",
            Filter = "ASC 日志文件 (*.asc)|*.asc|所有文件 (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false
        };

        if (dialog.ShowDialog(this) != true)
            return;

        SetReadingState(true);
        _allFrames = Array.Empty<AscFrame>();
        AllFramesDataGrid.ItemsSource = null;
        FaultDataGrid.ItemsSource = null;
        FilePathTextBox.Text = dialog.FileName;
        StatusTextBlock.Text = "正在读取 ASC 文件，请稍候。";

        // Progress 在界面线程创建，后台解析时的进度回调会自动切回界面线程更新控件。
        var progress = new Progress<int>(value =>
        {
            ReadProgressBar.Value = value;
            ReadProgressTextBlock.Text = $"正在读取 ASC 文件：{value}%";
        });

        try
        {
            // 在线程池完成逐行解析，防止超大 ASC 文件的正则匹配和数据转换卡住 WPF 界面。
            var result = await Task.Run(() => AscParser.ParseAsync(dialog.FileName, progress));
            _allFrames = result.Frames;
            _fileStatusText = $"初始记录时间：{result.InitialTime:yyyy-MM-dd HH:mm:ss.fff}；已读取 {_allFrames.Count} 帧 CAN 数据，跳过 {result.IgnoredLineCount} 行非标准 ASC 数据。";
            ApplyFrameIdFilter();
        }
        catch (Exception exception)
        {
            _allFrames = Array.Empty<AscFrame>();
            AllFramesDataGrid.ItemsSource = null;
            FaultDataGrid.ItemsSource = null;
            StatusTextBlock.Text = "文件解析失败。";
            MessageBox.Show(this, exception.Message, "解析 ASC 文件失败", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            SetReadingState(false);
        }
    }

    /// <summary>切换读取状态：读取期间禁止重复导入、筛选和选择报文。</summary>
    private void SetReadingState(bool isReading)
    {
        OpenAscButton.IsEnabled = !isReading;
        FrameIdFilterTextBox.IsEnabled = !isReading;
        AllFramesDataGrid.IsEnabled = !isReading;
        FaultDataGrid.IsEnabled = !isReading;
        ReadProgressPanel.Visibility = isReading ? Visibility.Visible : Visibility.Collapsed;

        if (isReading)
        {
            ReadProgressBar.Value = 0;
            ReadProgressTextBlock.Text = "正在读取 ASC 文件：0%";
        }
    }

    /// <summary>帧 ID 输入内容变化后，按十六进制 ID 的任意片段实时筛选上方报文列表。</summary>
    private void FrameIdFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFrameIdFilter();
    }

    /// <summary>筛选 CAN ID 显示文本；输入 1810、1080 等片段均可匹配扩展帧和标准帧 ID。</summary>
    private void ApplyFrameIdFilter()
    {
        if (_allFrames.Count == 0)
        {
            AllFramesDataGrid.ItemsSource = null;
            FaultDataGrid.ItemsSource = null;
            StatusTextBlock.Text = _fileStatusText;
            return;
        }

        var keyword = FrameIdFilterTextBox.Text.Trim();
        if (keyword.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            keyword = keyword[2..];
        keyword = keyword.TrimEnd('x', 'X');

        var filteredFrames = string.IsNullOrEmpty(keyword)
            ? _allFrames
            : _allFrames.Where(frame => frame.CanId.ToString("X").Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToList();

        AllFramesDataGrid.ItemsSource = filteredFrames;
        FaultDataGrid.ItemsSource = null;
        StatusTextBlock.Text = string.IsNullOrEmpty(keyword)
            ? _fileStatusText
            : $"{_fileStatusText}；帧 ID 包含“{keyword}”的报文：{filteredFrames.Count} 帧。";
    }

    /// <summary>
    /// 仅解析用户当前选中的报文。CAN ID、data0 页面或数据长度不符合故障报码协议时，
    /// 下方表格保持为空，不打印无关信息。
    /// </summary>
    private void AllFramesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AllFramesDataGrid.SelectedItem is not AscFrame frame)
        {
            FaultDataGrid.ItemsSource = null;
            ClearFaultVisualTable();
            return;
        }

        RefreshSelectedFrameFaults(frame);
    }

    /// <summary>报文单元格编辑提交后，按修改后的 CAN ID、data0~data7 重新解析当前选中报文。</summary>
    private void AllFramesDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        if (e.EditAction != DataGridEditAction.Commit)
            return;

        // 此时绑定源尚未提交，延后到界面队列中执行，确保解析读取的是编辑后的属性值。
        Dispatcher.BeginInvoke(() =>
        {
            if (AllFramesDataGrid.SelectedItem is AscFrame frame)
                RefreshSelectedFrameFaults(frame);
        });
    }

    /// <summary>按当前报文内容刷新故障列表和右侧 bit 位表。</summary>
    private void RefreshSelectedFrameFaults(AscFrame frame)
    {
        if (!IsFaultReportFrame(frame))
        {
            FaultDataGrid.ItemsSource = null;
            ClearFaultVisualTable();
            StatusTextBlock.Text = $"第 {frame.SourceLine} 行的 CAN ID、data0 页面或数据长度不符合故障报码协议，不显示解析结果。";
            return;
        }

        var occurrences = FaultReportDecoder.Decode(frame);
        FaultDataGrid.ItemsSource = occurrences;
        RefreshFaultVisualTable(frame, occurrences);
        StatusTextBlock.Text = occurrences.Count == 0
            ? $"第 {frame.SourceLine} 行是有效的故障报码帧，但没有置位故障。"
            : $"第 {frame.SourceLine} 行解析到 {occurrences.Count} 条故障信息。";
    }

    /// <summary>将选中故障页对应故障域的全部 bit 位和故障含义显示在右侧两列表格中。</summary>
    private void RefreshFaultVisualTable(AscFrame frame, IReadOnlyList<FaultOccurrence> occurrences)
    {
        var (domain, faultCount) = frame.Data[0] switch
        {
            0x10 => ("充电器", 18),
            0x11 or 0x12 => ("电池", 32),
            0x13 => ("RS485握手", 8),
            0x14 => ("RS485周期查询", 16),
            _ => ("", 0)
        };

        var setBits = occurrences
            .Where(item => item.Domain == domain)
            .Select(item => item.PdfNumber - 1)
            .ToHashSet();

        /* bit0 对应 PDF 编号1，依次生成当前故障域的全部定义；置位 bit 在 XAML 中显示为红色。 */
        FaultVisualDataGrid.ItemsSource = Enumerable.Range(0, faultCount)
            .Select(bit => new FaultVisualRow(bit, FaultCatalog.GetName(domain, bit + 1), setBits.Contains(bit)))
            .ToList();
        FaultVisualDomainTextBlock.Text = $"当前页面：{domain}（data0 = 0x{frame.Data[0]:X2}）";
    }

    /// <summary>未选中有效故障报码时清空右侧 bit 位表。</summary>
    private void ClearFaultVisualTable()
    {
        FaultVisualDataGrid.ItemsSource = null;
        FaultVisualDomainTextBlock.Text = "请选择 data0 = 0x10 ~ 0x14 的故障报码报文。";
    }

    /// <summary>
    /// 判断报文是否符合故障报码协议。定时主动上传时，data0 0x10~0x14 分别对应
    /// 0x1F1C10xx~0x1F2010xx；同时保留 Can_DebugDataAckSub() 使用 0x1F1C10xx 回复各页面的解析。
    /// </summary>
    private static bool IsFaultReportFrame(AscFrame frame)
    {
        if (!frame.IsExtended || frame.Data.Length == 0)
            return false;

        var faultPage = frame.Data[0];
        var requiredLength = faultPage switch
        {
            0x10 or 0x11 or 0x12 => 8,
            0x13 => 5,
            0x14 => 7,
            _ => int.MaxValue
        };

        if (frame.Data.Length < requiredLength)
            return false;

        // xx 必须是电源本机地址 0x80~0x8B。
        var psuAddress = frame.CanId & 0xFFU;
        if (psuAddress is < 0x80U or > 0x8BU)
            return false;

        const uint faultReportBaseId = 0x1F1C1000U;
        var debugReplyId = faultReportBaseId | psuAddress;
        var periodicUploadId = debugReplyId + ((uint)(faultPage - 0x10U) * 0x10000U);

        return frame.CanId == periodicUploadId || frame.CanId == debugReplyId;
    }
}