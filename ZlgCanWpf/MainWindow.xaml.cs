using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text.Json;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace ZlgCanWpf;

public partial class MainWindow : Window
{
    private const int MaxVisibleFrames = 5000;
    private const ushort SetPsuRunId = 0x082B;
    private const ushort FactoryModeId = 0x1F2D;
    private const ushort AgingModeId = 0x1F2E;
    private const ushort CalibrationId = 0x1F2C;
    private const ushort FlashCalibrationReadId = 0x1F3C;
    private const ushort CalibrationFlagId = 0x1F2B;
    private const ushort DebugDataId = 0x1F1C;
    private const byte MeasurementDebugIndex = 0x0A;
    private const byte TemperatureDebugIndex = 0x0D;
    private const byte MasterCanAddress = 0x10;
    private const int CalibrationResponseTimeoutMs = 500;
    private static readonly string ChannelSettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "ZlgCanWpf",
        "channel-settings.json");

    private static readonly string[] VoltageCalibrationFields =
    {
        "增益0", "增益1", "偏移0", "偏移1", "限制值", "CRC"
    };

    private static readonly string[] CurrentCalibrationFields =
    {
        "增益0", "增益1", "增益2", "偏移0", "偏移1",
        "偏移2", "限制值0", "限制值1", "CRC", "保留值"
    };

    private static readonly CalibrationDefinition[] CalibrationDefinitions =
    {
        new(0x51, "输入电压", VoltageCalibrationFields),
        new(0x52, "输入电压反馈", VoltageCalibrationFields),
        new(0x53, "输出电压", VoltageCalibrationFields),
        new(0x54, "输出电压反馈", VoltageCalibrationFields),
        new(0x55, "电池电压", VoltageCalibrationFields),
        new(0x56, "输出电流", CurrentCalibrationFields),
        new(0x57, "输出电流反馈", CurrentCalibrationFields),
        new(0x58, "输入电流", CurrentCalibrationFields),
        new(0x59, "输入电流反馈", CurrentCalibrationFields)
    };

    private static readonly string[] CalibrationFlagNames =
    {
        "输入电压", "输入电压反馈", "输出电压", "输出电压反馈", "电池电压",
        "输出电流", "输出电流反馈", "输入电流", "输入电流反馈"
    };

    private static readonly uint[] ClassicBitrates =
    {
        1_000_000, 800_000, 500_000, 250_000, 125_000,
        100_000, 50_000, 20_000, 10_000, 5_000
    };

    private static readonly uint[] CanFdDataBitrates =
    {
        8_000_000, 5_000_000, 4_000_000, 2_000_000,
        1_000_000, 800_000, 500_000, 250_000, 125_000, 100_000
    };

    private readonly ZlgCanService _service = new();
    private readonly DispatcherTimer _telemetryTimer = new();
    private readonly ICollectionView _frameView;
    private readonly object _calibrationResponseLock = new();
    private readonly object _calibrationFlagResponseLock = new();
    private TaskCompletionSource<ushort>? _pendingCalibrationResponse;
    private ushort _pendingCalibrationResponseCommandId;
    private TaskCompletionSource<uint>? _pendingCalibrationFlagResponse;
    private byte _pendingCalibrationType;
    private byte _pendingCalibrationIndex;
    private byte _calibrationTargetAddress;
    private byte _calibrationFlagTargetAddress;
    private byte _telemetryTargetAddress;
    private long _receiveCount;
    private long _transmitCount;
    private uint? _frameIdFilter;
    private bool _requestTemperatureData;
    private bool _calibrationReadInProgress;
    private bool _calibrationFlagReadInProgress;
    private bool _commandSequenceInProgress;
    private bool _closing;

    public ObservableCollection<CanFrameRecord> Frames { get; } = new();
    public ObservableCollection<CalibrationValueRecord> CalibrationValues { get; } = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        /* 帧ID筛选仅作用于报文显示视图，Frames中继续保留全部原始报文。 */
        _frameView = CollectionViewSource.GetDefaultView(Frames);
        _frameView.Filter = FilterFrame;

        ProtocolComboBox.SelectedIndex = 0;
        DeviceTypeComboBox.ItemsSource = CreateDeviceOptions();
        DeviceTypeComboBox.SelectedIndex = 6;
        WorkModeComboBox.SelectedIndex = 0;
        FilterModeComboBox.SelectedIndex = 2;
        TransmitTypeComboBox.SelectedIndex = 0;

        ArbitrationBitrateComboBox.ItemsSource = ClassicBitrates;
        ArbitrationBitrateComboBox.SelectedItem = 125_000U;
        DataBitrateComboBox.ItemsSource = CanFdDataBitrates;
        DataBitrateComboBox.SelectedItem = 2_000_000U;
        LoadChannelSettings();

        _service.FrameReceived += Service_FrameReceived;
        _service.ReceiveFaulted += Service_ReceiveFaulted;

        _telemetryTimer.Interval = TimeSpan.FromSeconds(1);
        _telemetryTimer.Tick += TelemetryTimer_Tick;
        _telemetryTimer.Start();

        UpdateControls();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Rect workArea = SystemParameters.WorkArea;
        double availableWidth = Math.Max(MinWidth, workArea.Width - 16);
        double availableHeight = Math.Max(MinHeight, workArea.Height - 16);

        /* 启动时按当前显示器工作区设置窗口，避免任务栏和高DPI缩放遮挡底部报文区。 */
        Width = Math.Min(1280, availableWidth);
        Height = Math.Min(820, availableHeight);
        Left = workArea.Left + ((workArea.Width - Width) / 2);
        Top = workArea.Top + ((workArea.Height - Height) / 2);

        Dispatcher.BeginInvoke(new Action(ApplyStartupLayoutScale), DispatcherPriority.Loaded);
    }

    private void ApplyStartupLayoutScale()
    {
        MainContentGrid.LayoutTransform = Transform.Identity;
        MainContentGrid.UpdateLayout();

        double availableHeight = Math.Max(1, ActualHeight - 46);
        if (MainContentGrid.ActualHeight <= availableHeight)
        {
            return;
        }

        /* 低分辨率或高缩放显示器下统一缩小布局，最低保留72%，再小则使用滚动条。 */
        double scale = Math.Max(0.72, availableHeight / MainContentGrid.ActualHeight);
        MainContentGrid.LayoutTransform = new ScaleTransform(scale, scale);
    }

    private static IReadOnlyList<CanDeviceOption> CreateDeviceOptions()
    {
        return new[]
        {
            new CanDeviceOption("USBCAN-1", ZlgCanNative.UsbCan1, 1, false, false, false),
            new CanDeviceOption("USBCAN-2", ZlgCanNative.UsbCan2, 2, false, false, false),
            new CanDeviceOption("PCI-9820I", ZlgCanNative.Pci9820I, 2, false, false, false),
            new CanDeviceOption("USBCAN-E-U", ZlgCanNative.UsbCanEU, 1, false, true, false),
            new CanDeviceOption("USBCAN-2E-U", ZlgCanNative.UsbCan2EU, 2, false, true, false),
            new CanDeviceOption("USBCAN-4E-U", ZlgCanNative.UsbCan4EU, 4, false, false, false),
            new CanDeviceOption("USBCANFD-200U", ZlgCanNative.UsbCanFd200U, 2, true, true, true),
            new CanDeviceOption("USBCANFD-400U", ZlgCanNative.UsbCanFd400U, 4, true, true, true),
            new CanDeviceOption("USBCANFD-800U", ZlgCanNative.UsbCanFd800U, 8, true, true, true),
            new CanDeviceOption("USBCANFD-100U", ZlgCanNative.UsbCanFd100U, 1, true, true, true),
            new CanDeviceOption("USBCANFD-MINI", ZlgCanNative.UsbCanFdMini, 1, true, true, true),
            new CanDeviceOption("创芯 USBCAN", 3, 1, false, false, false, CanDriverKind.ControlCan),
            new CanDeviceOption("创芯 USBCAN2", 4, 2, false, false, false, CanDriverKind.ControlCan)
        };
    }

    private void LoadChannelSettings()
    {
        try
        {
            if (!File.Exists(ChannelSettingsPath))
            {
                return;
            }

            ChannelSettings? settings = JsonSerializer.Deserialize<ChannelSettings>(
                File.ReadAllText(ChannelSettingsPath));
            if (settings is null)
            {
                return;
            }

            CanDeviceOption? device = DeviceTypeComboBox.Items
                .OfType<CanDeviceOption>()
                .FirstOrDefault(item => (item.DeviceType == settings.DeviceType)
                    && (item.DriverKind == settings.DriverKind));
            if (device is not null)
            {
                DeviceTypeComboBox.SelectedItem = device;
            }

            DeviceIndexTextBox.Text = settings.DeviceIndex;
            RestoreComboBoxSelectedIndex(ChannelComboBox, settings.ChannelIndex);
            RestoreComboBoxSelectedIndex(WorkModeComboBox, settings.WorkModeIndex);
            RestoreComboBoxSelectedIndex(FilterModeComboBox, settings.FilterModeIndex);
            ArbitrationBitrateComboBox.Text = settings.ArbitrationBitrate;
            DataBitrateComboBox.Text = settings.DataBitrate;
            CanFdNonIsoCheckBox.IsChecked = settings.CanFdNonIso;
            TerminationCheckBox.IsChecked = settings.EnableTermination;
            FilterStartTextBox.Text = settings.FilterStart;
            FilterEndTextBox.Text = settings.FilterEnd;
        }
        catch
        {
            /* 用户配置文件缺失或内容无效时继续使用界面默认参数。 */
        }
    }

    private void SaveChannelSettings()
    {
        try
        {
            CanDeviceOption? device = DeviceTypeComboBox.SelectedItem as CanDeviceOption;
            if (device is null)
            {
                return;
            }

            ChannelSettings settings = new()
            {
                DeviceType = device.DeviceType,
                DriverKind = device.DriverKind,
                DeviceIndex = DeviceIndexTextBox.Text,
                ChannelIndex = ChannelComboBox.SelectedIndex,
                WorkModeIndex = WorkModeComboBox.SelectedIndex,
                FilterModeIndex = FilterModeComboBox.SelectedIndex,
                ArbitrationBitrate = ArbitrationBitrateComboBox.Text,
                DataBitrate = DataBitrateComboBox.Text,
                CanFdNonIso = CanFdNonIsoCheckBox.IsChecked == true,
                EnableTermination = TerminationCheckBox.IsChecked == true,
                FilterStart = FilterStartTextBox.Text,
                FilterEnd = FilterEndTextBox.Text
            };

            string? directory = Path.GetDirectoryName(ChannelSettingsPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(ChannelSettingsPath, JsonSerializer.Serialize(settings));
        }
        catch
        {
            /* 保存失败不影响设备关闭和程序退出。 */
        }
    }

    private static void RestoreComboBoxSelectedIndex(ComboBox comboBox, int index)
    {
        if ((index >= 0) && (index < comboBox.Items.Count))
        {
            comboBox.SelectedIndex = index;
        }
    }

    private void DeviceTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DeviceTypeComboBox.SelectedItem is not CanDeviceOption option)
        {
            return;
        }

        ChannelComboBox.ItemsSource = Enumerable.Range(0, checked((int)option.ChannelCount));
        ChannelComboBox.SelectedIndex = 0;

        ProtocolComboBox.SelectedIndex = option.SupportsCanFd ? 1 : 0;

        if (!option.SupportsRangeFilter)
        {
            FilterModeComboBox.SelectedIndex = 2;
        }

        if (!option.SupportsTermination)
        {
            TerminationCheckBox.IsChecked = false;
        }

        if (!option.SupportsCanFd)
        {
            SendCanFdCheckBox.IsChecked = false;
        }

        UpdateControls();
    }

    private void ProtocolComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        bool useCanFd = ProtocolComboBox.SelectedIndex == 1;
        DataBitrateComboBox.IsEnabled = useCanFd;
        CanFdNonIsoCheckBox.IsEnabled = useCanFd;
        UpdateControls();
    }

    private void FilterModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateControls();
    }

    private void SendCanFdCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        bool canFd = SendCanFdCheckBox.IsChecked == true;
        BrsCheckBox.IsEnabled = canFd;
        RemoteFrameCheckBox.IsEnabled = !canFd;
        if (canFd)
        {
            RemoteFrameCheckBox.IsChecked = false;
        }
    }

    private void OpenButton_Click(object sender, RoutedEventArgs e)
    {
        RunUiAction(() =>
        {
            CanDeviceOption option = GetSelectedDevice();
            uint deviceIndex = ParseUnsigned(DeviceIndexTextBox.Text, "设备索引");
            string description = _service.OpenDevice(option, deviceIndex);
            SetStatus($"设备已打开：{description}");
            UpdateControls();
        });
    }

    private void InitializeButton_Click(object sender, RoutedEventArgs e)
    {
        RunUiAction(() =>
        {
            CanDeviceOption device = GetSelectedDevice();
            bool useCanFd = device.SupportsCanFd;

            CanChannelOptions options = new(
                checked((uint)ChannelComboBox.SelectedIndex),
                useCanFd,
                ParseUnsigned(ArbitrationBitrateComboBox.Text, "仲裁域波特率"),
                ParseUnsigned(DataBitrateComboBox.Text, "数据域波特率"),
                CanFdNonIsoCheckBox.IsChecked == true,
                WorkModeComboBox.SelectedIndex == 1,
                TerminationCheckBox.IsChecked == true,
                FilterModeComboBox.SelectedIndex,
                ParseCanId(FilterStartTextBox.Text, "过滤起始ID"),
                ParseCanId(FilterEndTextBox.Text, "过滤结束ID"));

            if (options.FilterStart > options.FilterEnd)
            {
                throw new InvalidOperationException("过滤起始ID不能大于结束ID。");
            }

            if (options.FilterMode == 0 && options.FilterEnd > 0x7FF)
            {
                throw new InvalidOperationException("标准帧过滤ID必须在0x000～0x7FF范围内。");
            }

            _service.InitializeChannel(options);
            SetStatus($"通道{options.ChannelIndex}初始化成功");
            UpdateControls();
        });
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        RunUiAction(() =>
        {
            _service.StartChannel();
            SetStatus("CAN通道已启动，正在接收报文");
            UpdateControls();
        });
    }

    private async void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        await RunUiActionAsync(async () =>
        {
            await _service.ResetChannelAsync();
            SetStatus("CAN通道已复位");
            UpdateControls();
        });
    }

    private async void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        await RunUiActionAsync(async () =>
        {
            await _service.CloseDeviceAsync();
            SetStatus("CAN设备已关闭");
            UpdateControls();
        });
    }

    private async void SendOutputReferenceButton_Click(object sender, RoutedEventArgs e)
    {
        await RunUiActionAsync(async () =>
        {
            _commandSequenceInProgress = true;
            UpdateControls();
            try
            {
                byte psuAddress = ParsePsuAddress();
                ushort voltage = ParseHundredthValue(OutputVoltageTextBox.Text, "输出电压");
                ushort current = ParseHundredthValue(OutputCurrentTextBox.Text, "输出电流");
                byte[] data =
                {
                    0x00,
                    0x00,
                    (byte)(voltage & 0xFF),
                    (byte)(voltage >> 8),
                    (byte)(current & 0xFF),
                    (byte)(current >> 8),
                    0xFF,
                    0xFF
                };

                /* 调压调流前，先进入工厂模式和校准模式。
                 * 固件只在自动模式切换到被控模式时接收新的电压、电流参考值，
                 * 因此随后先发送0x00恢复自动模式，再发送0x01下发本次设定。 */
                SendChargerCommand(FactoryModeId, psuAddress,
                    new byte[] { 0x11, 0x13, 0x17, 0x19 },
                    "正在进入工厂模式", false);
                await Task.Delay(100);

                SendChargerCommand(CalibrationId, psuAddress,
                    new byte[] { 0x21 },
                    "正在进入校准模式", false);
                await Task.Delay(100);

                SendChargerCommand(SetPsuRunId, psuAddress, data,
                    "正在切换到自动模式", false);
                await Task.Delay(100);

                data[0] = 0x01;
                SendChargerCommand(SetPsuRunId, psuAddress, data,
                    $"已进入工厂和校准模式，并发送0x082B调压调流："
                        + $"{voltage / 100M:F2}V，{current / 100M:F2}A");
            }
            finally
            {
                _commandSequenceInProgress = false;
                UpdateControls();
            }
        });
    }

    private async void EnterParallelButton_Click(object sender, RoutedEventArgs e)
    {
        await RunUiActionAsync(async () =>
        {
            _commandSequenceInProgress = true;
            UpdateControls();
            try
            {
                byte mergedAddress = ParsePsuAddress();
                byte floatingAddress = ParseFloatingPsuAddress();
                ValidateParallelAddressPair(mergedAddress, floatingAddress);

                byte[] data =
                {
                    0x06,
                    0xFF,
                    0xFF,
                    0x7F,
                    0xFF,
                    0x7F,
                    0xFF,
                    floatingAddress
                };

                /* 主控需要向合并路和悬空路发送相同的并联命令，data7均填写悬空路地址。 */
                SendChargerCommand(SetPsuRunId, mergedAddress, data,
                    "正在向合并路发送进入并联指令", false);
                await Task.Delay(100);
                SendChargerCommand(SetPsuRunId, floatingAddress, data,
                    $"已发送进入并联指令：合并路0x{mergedAddress:X2}，悬空路0x{floatingAddress:X2}");
            }
            finally
            {
                _commandSequenceInProgress = false;
                UpdateControls();
            }
        });
    }

    private void ExitParallelButton_Click(object sender, RoutedEventArgs e)
    {
        RunUiAction(() =>
        {
            byte mergedAddress = ParsePsuAddress();
            byte floatingAddress = ParseFloatingPsuAddress();
            ValidateParallelAddressPair(mergedAddress, floatingAddress);

            /* 向悬空路发送自动运行：悬空路退出并联并自动运行，合并路仅取消并机运行。 */
            byte[] data =
            {
                0x00,
                0xFF,
                0xFF,
                0x7F,
                0xFF,
                0x7F,
                0xFF,
                0xFF
            };
            SendChargerCommand(SetPsuRunId, floatingAddress, data,
                $"已发送退出并联指令：悬空路0x{floatingAddress:X2}转自动运行，"
                    + $"合并路0x{mergedAddress:X2}保持原运行状态");
        });
    }

    private void EnterFactoryButton_Click(object sender, RoutedEventArgs e)
    {
        RunUiAction(() => SendChargerCommand(FactoryModeId, ParsePsuAddress(),
            new byte[] { 0x11, 0x13, 0x17, 0x19 }, "已发送进入工厂模式指令"));
    }

    private void EnterCalibrationButton_Click(object sender, RoutedEventArgs e)
    {
        RunUiAction(() => SendChargerCommand(CalibrationId, ParsePsuAddress(),
            new byte[] { 0x21 }, "已发送进入校准模式指令"));
    }

    private void ExitCalibrationButton_Click(object sender, RoutedEventArgs e)
    {
        RunUiAction(() => SendChargerCommand(CalibrationId, ParsePsuAddress(),
            new byte[] { 0xFF }, "已发送退出校准和工厂模式指令"));
    }

    private void EnterAgingButton_Click(object sender, RoutedEventArgs e)
    {
        RunUiAction(() => SendChargerCommand(AgingModeId, ParsePsuAddress(),
            new byte[] { 0x55, 0xAA }, "已发送进入老化模式指令"));
    }

    private void ExitAgingButton_Click(object sender, RoutedEventArgs e)
    {
        RunUiAction(() => SendChargerCommand(AgingModeId, ParsePsuAddress(),
            new byte[] { 0xAA, 0x55 }, "已发送退出老化模式指令"));
    }

    private async void ReadAllCalibrationButton_Click(object sender, RoutedEventArgs e)
    {
        await RunUiActionAsync(ReadAllCalibrationValuesAsync);
    }

    private async void ReadCalibrationFlagButton_Click(object sender, RoutedEventArgs e)
    {
        await RunUiActionAsync(ReadCalibrationFlagAsync);
    }

    private void SaveCalibrationFileButton_Click(object sender, RoutedEventArgs e)
    {
        RunUiAction(SaveCalibrationFile);
    }

    private async void WriteCalibrationFileButton_Click(object sender, RoutedEventArgs e)
    {
        OpenFileDialog dialog = new()
        {
            Filter = "校准数据文件 (*.json)|*.json|所有文件 (*.*)|*.*",
            Title = "选择要写入充电器Flash的校准数据文件"
        };
        if (dialog.ShowDialog(this) == true)
        {
            await RunUiActionAsync(() => WriteCalibrationFileAsync(dialog.FileName));
        }
    }

    private void SaveCalibrationFile()
    {
        List<CalibrationBackupItem> items = GetCompleteCalibrationItems(CalibrationValues);
        byte sourceAddress = ParsePsuAddress();
        SaveFileDialog dialog = new()
        {
            Filter = "校准数据文件 (*.json)|*.json|所有文件 (*.*)|*.*",
            FileName = $"Calibration_0x{sourceAddress:X2}_{DateTime.Now:yyyyMMdd_HHmmss}.json",
            Title = "保存充电器校准数据"
        };
        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        CalibrationBackupFile backup = new()
        {
            SourceAddress = sourceAddress,
            CreatedAt = DateTime.Now,
            Items = items
        };
        File.WriteAllText(dialog.FileName, JsonSerializer.Serialize(backup,
            new JsonSerializerOptions { WriteIndented = true }));
        SetStatus($"已保存{items.Count}项校准数据：{dialog.FileName}");
    }

    private async Task WriteCalibrationFileAsync(string fileName)
    {
        CalibrationBackupFile? backup = JsonSerializer.Deserialize<CalibrationBackupFile>(File.ReadAllText(fileName));
        if ((backup is null) || (backup.Format != CalibrationBackupFile.FormatName) || (backup.Version != 1))
        {
            throw new FormatException("校准数据文件格式或版本不正确。");
        }

        List<CalibrationBackupItem> items = GetCompleteCalibrationItems(backup.Items);
        ValidateCalibrationCrc(items);
        _commandSequenceInProgress = true;
        _calibrationTargetAddress = ParsePsuAddress();
        UpdateControls();
        try
        {
            SendChargerCommand(FactoryModeId, _calibrationTargetAddress,
                new byte[] { 0x11, 0x13, 0x17, 0x19 },
                "正在进入工厂模式并准备写入Flash校准数据", false);
            await Task.Delay(100);

            SendChargerCommand(CalibrationId, _calibrationTargetAddress,
                new byte[] { 0x21 }, "正在进入校准模式", false);
            await Task.Delay(100);

            List<CalibrationBackupItem> writableItems = items
                .Where(item => item.Type <= 0x57)
                .ToList();
            foreach (CalibrationBackupItem item in writableItems)
            {
                await WriteCalibrationWordAsync(item);
                await Task.Delay(10);
            }

            /* 输入电流0x58和输入电流反馈0x59无需校准，不写入Flash。 */
            /* 所有需校准数据已逐帧回显确认后，发送Store命令写入Flash。 */
            await WriteCalibrationWordAsync(new CalibrationBackupItem(0xFF, 0x60, 0x5220));
            await Task.Delay(1000);

            /* 固件Store完成后会重新从Flash加载校准值，重新读取并逐项核对。 */
            await ReadAllCalibrationValuesAsync();
            List<CalibrationBackupItem> readbackItems = GetCompleteCalibrationItems(CalibrationValues)
                .Where(item => item.Type <= 0x57)
                .ToList();
            if (!writableItems.SequenceEqual(readbackItems))
            {
                throw new InvalidOperationException("Flash校准数据回读校验不一致，未退出校准和工厂模式。");
            }

            SendChargerCommand(CalibrationId, _calibrationTargetAddress,
                new byte[] { 0xFF }, "Flash校准数据写入并回读校验成功，已退出校准和工厂模式");
        }
        finally
        {
            _commandSequenceInProgress = false;
            UpdateControls();
        }
    }

    private async Task WriteCalibrationWordAsync(CalibrationBackupItem item)
    {
        TaskCompletionSource<ushort> response = new(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (_calibrationResponseLock)
        {
            _pendingCalibrationType = item.Type;
            _pendingCalibrationIndex = item.Index;
            _pendingCalibrationResponseCommandId = CalibrationId;
            _pendingCalibrationResponse = response;
        }

        try
        {
            SendChargerCommand(CalibrationId, _calibrationTargetAddress,
                new byte[] { item.Type, item.Index, (byte)item.Value, (byte)(item.Value >> 8) },
                $"正在写入校准类型0x{item.Type:X2}，索引{item.Index}", false);

            Task completed = await Task.WhenAny(response.Task, Task.Delay(1000));
            if (completed != response.Task)
            {
                throw new TimeoutException($"写入校准类型0x{item.Type:X2}，索引{item.Index}时未收到回显。");
            }

            if (await response.Task != item.Value)
            {
                throw new InvalidOperationException($"校准类型0x{item.Type:X2}，索引{item.Index}回显值不匹配。");
            }
        }
        finally
        {
            lock (_calibrationResponseLock)
            {
                if (ReferenceEquals(_pendingCalibrationResponse, response))
                {
                    _pendingCalibrationResponse = null;
                    _pendingCalibrationResponseCommandId = 0;
                }
            }
        }
    }

    private static List<CalibrationBackupItem> GetCompleteCalibrationItems(
        IEnumerable<CalibrationValueRecord> records)
    {
        List<CalibrationBackupItem> items = new();
        foreach (CalibrationDefinition definition in CalibrationDefinitions)
        {
            for (byte index = 0; index < definition.Fields.Length; index++)
            {
                string typeText = $"0x{definition.Type:X2}";
                List<CalibrationValueRecord> matches = records.Where(item => (item.TypeText == typeText)
                    && (item.Index == index) && (item.Status == "读取成功")).ToList();
                if (matches.Count != 1)
                {
                    throw new InvalidOperationException("请先成功读取完整的70项Flash校准数据，再保存到文件。");
                }

                items.Add(new CalibrationBackupItem(definition.Type, index, matches[0].Value));
            }
        }

        return items;
    }

    private static List<CalibrationBackupItem> GetCompleteCalibrationItems(
        IEnumerable<CalibrationBackupItem> source)
    {
        Dictionary<(byte Type, byte Index), CalibrationBackupItem> itemMap = new();
        foreach (CalibrationBackupItem item in source)
        {
            if (!itemMap.TryAdd((item.Type, item.Index), item))
            {
                throw new FormatException($"校准数据文件中类型0x{item.Type:X2}、索引{item.Index}重复。");
            }
        }

        List<CalibrationBackupItem> items = new();
        foreach (CalibrationDefinition definition in CalibrationDefinitions)
        {
            for (byte index = 0; index < definition.Fields.Length; index++)
            {
                if (!itemMap.TryGetValue((definition.Type, index), out CalibrationBackupItem? item))
                {
                    throw new FormatException($"校准数据文件缺少类型0x{definition.Type:X2}、索引{index}。");
                }

                items.Add(item!);
            }
        }

        if (itemMap.Count != items.Count)
        {
            throw new FormatException("校准数据文件包含未定义的校准类型或索引。");
        }

        return items;
    }

    private static void ValidateCalibrationCrc(IReadOnlyCollection<CalibrationBackupItem> items)
    {
        foreach (CalibrationDefinition definition in CalibrationDefinitions.Where(item => item.Type <= 0x57))
        {
            byte[] data = new byte[definition.Fields.Length * sizeof(ushort)];
            for (byte index = 0; index < definition.Fields.Length; index++)
            {
                CalibrationBackupItem item = items.Single(item => (item.Type == definition.Type)
                    && (item.Index == index));
                data[index * 2] = (byte)item.Value;
                data[index * 2 + 1] = (byte)(item.Value >> 8);
            }

            int crcHighByteIndex = definition.Fields.Length == VoltageCalibrationFields.Length ? 11 : 17;
            if ((data[crcHighByteIndex - 1] != 0x18)
                || (data[crcHighByteIndex] != CalculateCalibrationCrc(data, crcHighByteIndex)))
            {
                throw new FormatException($"校准类型0x{definition.Type:X2}的CRC无效，不能写入Flash。");
            }
        }
    }

    private static byte CalculateCalibrationCrc(byte[] data, int length)
    {
        byte crc = 0;
        for (int index = 0; index < length; index++)
        {
            crc ^= data[index];
            for (int bit = 0; bit < 8; bit++)
            {
                crc = (crc & 0x80) != 0
                    ? (byte)((crc << 1) ^ 0x07)
                    : (byte)(crc << 1);
            }
        }

        return crc;
    }

    private async void ClearCalibrationDataButton_Click(object sender, RoutedEventArgs e)
    {
        await RunUiActionAsync(ClearCalibrationDataAsync);
    }

    private async Task ClearCalibrationDataAsync()
    {
        _commandSequenceInProgress = true;
        UpdateControls();
        try
        {
            byte psuAddress = ParsePsuAddress();

            /* 清除校准数据前按固件既有流程先进入工厂模式和校准模式。 */
            SendChargerCommand(FactoryModeId, psuAddress,
                new byte[] { 0x11, 0x13, 0x17, 0x19 },
                "正在进入工厂模式并准备清除Flash校准数据", false);
            await Task.Delay(100);

            SendChargerCommand(CalibrationId, psuAddress,
                new byte[] { 0x21 }, "正在进入校准模式", false);
            await Task.Delay(100);

            /* 0x1F2C清除校准数据：data0=0xF0，data1=0x60，data2=0x20，data3=0x52。 */
            SendChargerCommand(CalibrationId, psuAddress,
                new byte[] { 0xF0, 0x60, 0x20, 0x52 },
                "已发送清除Flash校准数据指令");
        }
        finally
        {
            _commandSequenceInProgress = false;
            UpdateControls();
        }
    }

    private async Task ReadCalibrationFlagAsync()
    {
        if (_calibrationFlagReadInProgress)
        {
            throw new InvalidOperationException("正在读取校准标志位，请等待本次读取完成。");
        }

        _calibrationFlagReadInProgress = true;
        _calibrationFlagTargetAddress = ParsePsuAddress();
        CalibrationFlagValueTextBlock.Text = "读取中...";
        CalibrationFlagDetailsTextBlock.Text = "正在等待充电器返回0x1F2B校准标志位。";
        UpdateControls();

        TaskCompletionSource<uint> response = new(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (_calibrationFlagResponseLock)
        {
            _pendingCalibrationFlagResponse = response;
        }

        try
        {
            /* 0x1F2B校准标志位读取请求：DLC=1，data0=0x04。 */
            SendChargerCommand(CalibrationFlagId, _calibrationFlagTargetAddress,
                new byte[] { 0x04 }, "正在读取校准标志位", false);

            Task completed = await Task.WhenAny(response.Task, Task.Delay(CalibrationResponseTimeoutMs));
            if (completed != response.Task)
            {
                CalibrationFlagValueTextBlock.Text = "响应超时";
                CalibrationFlagDetailsTextBlock.Text = "未收到0x1F2B10xx应答，请检查电源地址和CAN通信。";
                throw new TimeoutException("未收到0x1F2B校准标志位响应，请检查电源地址和CAN通信。");
            }

            uint flags = await response.Task;
            UpdateCalibrationFlagDisplay(flags);
            SetStatus($"校准标志位读取成功：0x{flags:X8}");
        }
        finally
        {
            lock (_calibrationFlagResponseLock)
            {
                if (ReferenceEquals(_pendingCalibrationFlagResponse, response))
                {
                    _pendingCalibrationFlagResponse = null;
                }
            }

            _calibrationFlagReadInProgress = false;
            UpdateControls();
        }
    }

    private async Task ReadAllCalibrationValuesAsync()
    {
        if (_calibrationReadInProgress)
        {
            throw new InvalidOperationException("正在读取Flash校准值，请等待本次读取完成。");
        }

        _calibrationReadInProgress = true;
        _calibrationTargetAddress = ParsePsuAddress();
        CalibrationValues.Clear();
        UpdateControls();

        int successCount = 0;
        int consecutiveTimeouts = 0;
        try
        {
            /* 0x1F3C直接读取Flash加载且当前实际生效的校准数据，不改变工厂或校准模式。 */
            foreach (CalibrationDefinition definition in CalibrationDefinitions)
            {
                for (byte index = 0; index < definition.Fields.Length; index++)
                {
                    TaskCompletionSource<ushort> response = new(TaskCreationOptions.RunContinuationsAsynchronously);
                    lock (_calibrationResponseLock)
                    {
                        _pendingCalibrationType = definition.Type;
                        _pendingCalibrationIndex = index;
                        _pendingCalibrationResponseCommandId = FlashCalibrationReadId;
                        _pendingCalibrationResponse = response;
                    }

                    SendChargerCommand(FlashCalibrationReadId, _calibrationTargetAddress,
                        new[] { definition.Type, index },
                        $"正在通过0x1F3C读取{definition.Name}校准值，索引{index}", false);

                    Task completed = await Task.WhenAny(response.Task, Task.Delay(CalibrationResponseTimeoutMs));
                    if (completed == response.Task)
                    {
                        ushort value = await response.Task;
                        CalibrationValues.Add(new CalibrationValueRecord
                        {
                            GroupName = definition.Name,
                            TypeText = $"0x{definition.Type:X2}",
                            Index = index,
                            FieldName = definition.Fields[index],
                            Value = value,
                            ValueText = $"0x{value:X4} ({value})",
                            Status = "读取成功"
                        });
                        successCount++;
                        consecutiveTimeouts = 0;
                    }
                    else
                    {
                        CalibrationValues.Add(new CalibrationValueRecord
                        {
                            GroupName = definition.Name,
                            TypeText = $"0x{definition.Type:X2}",
                            Index = index,
                            FieldName = definition.Fields[index],
                            ValueText = "--",
                            Status = "响应超时"
                        });
                        consecutiveTimeouts++;
                    }

                    lock (_calibrationResponseLock)
                    {
                        if (ReferenceEquals(_pendingCalibrationResponse, response))
                        {
                            _pendingCalibrationResponse = null;
                        }
                    }

                    if (consecutiveTimeouts >= 3)
                    {
                        throw new TimeoutException("连续3次未收到校准值响应，请检查电源地址和CAN通信。");
                    }

                    await Task.Delay(10);
                }
            }

            SetStatus($"0x1F3C Flash校准值读取完成，共读取{successCount}项");
        }
        finally
        {
            lock (_calibrationResponseLock)
            {
                _pendingCalibrationResponse = null;
            }

            _calibrationReadInProgress = false;
            UpdateControls();
        }
    }

    private void SendButton_Click(object sender, RoutedEventArgs e)
    {
        RunUiAction(() =>
        {
            bool useCanFd = SendCanFdCheckBox.IsChecked == true;
            byte[] data = ParseDataBytes(SendDataTextBox.Text);
            uint transmitType = ParseTransmitType();
            CanTransmitRequest request = new(
                ParseCanId(SendIdTextBox.Text, "帧ID"),
                ExtendedFrameCheckBox.IsChecked == true,
                RemoteFrameCheckBox.IsChecked == true,
                useCanFd,
                BrsCheckBox.IsChecked == true,
                transmitType,
                data);

            uint sent = _service.Send(request);
            if (sent != 1)
            {
                throw new InvalidOperationException($"CAN报文发送失败，驱动返回发送数量：{sent}");
            }

            _transmitCount++;
            Frames.Add(new CanFrameRecord
            {
                ReceivedAt = DateTime.Now,
                RawId = request.Id,
                Data = request.Data,
                Direction = "TX",
                FrameType = useCanFd ? (request.BitRateSwitch ? "CAN FD+BRS" : "CAN FD") : "CAN",
                IdText = request.Extended ? $"0x{request.Id:X8}" : $"0x{request.Id:X3}",
                Format = request.Extended ? "扩展帧" : "标准帧",
                FrameKind = request.Remote ? "远程帧" : "数据帧",
                Length = request.Data.Length,
                DataText = CanFrameRecord.FormatData(request.Data)
            });
            TrimFrameList();
            UpdateCounter();
            SetStatus($"已发送1帧，ID={request.Id:X}");
        });
    }

    private void ReadErrorButton_Click(object sender, RoutedEventArgs e)
    {
        RunUiAction(() =>
        {
            ZlgCanError error = _service.ReadChannelError();
            string passiveData = CanFrameRecord.FormatData(error.PassiveErrorData);
            SetStatus($"错误码=0x{error.ErrorCode:X8}，被动错误={passiveData}，仲裁丢失=0x{error.ArbitrationLostErrorData:X2}");
        });
    }

    private void ApplyFrameIdFilterButton_Click(object sender, RoutedEventArgs e)
    {
        RunUiAction(() =>
        {
            uint frameId = ParseCanId(FrameIdFilterTextBox.Text, "筛选帧ID");
            if (frameId > 0x1FFFFFFF)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(frameId),
                    "筛选帧ID必须在0x00000000～0x1FFFFFFF范围内。");
            }

            _frameIdFilter = frameId;
            _frameView.Refresh();
            SetStatus($"已按帧ID 0x{frameId:X8}筛选报文");
        });
    }

    private void ClearFrameIdFilterButton_Click(object sender, RoutedEventArgs e)
    {
        _frameIdFilter = null;
        FrameIdFilterTextBox.Clear();
        _frameView.Refresh();
        SetStatus("已清除帧ID筛选");
    }

    private bool FilterFrame(object item)
    {
        return item is CanFrameRecord frame
            && (!_frameIdFilter.HasValue || (frame.RawId == _frameIdFilter.Value));
    }

    private void ClearHardwareBufferButton_Click(object sender, RoutedEventArgs e)
    {
        RunUiAction(() =>
        {
            _service.ClearReceiveBuffer();
            SetStatus("硬件接收缓冲区已清空");
        });
    }

    private void ClearListButton_Click(object sender, RoutedEventArgs e)
    {
        Frames.Clear();
        _receiveCount = 0;
        _transmitCount = 0;
        UpdateCounter();
        SetStatus("报文列表已清空");
    }

    private void TelemetryTimer_Tick(object? sender, EventArgs e)
    {
        if (!_service.IsChannelStarted
            || _calibrationReadInProgress
            || _calibrationFlagReadInProgress
            || _commandSequenceInProgress)
        {
            return;
        }

        try
        {
            byte debugIndex = _requestTemperatureData
                ? TemperatureDebugIndex
                : MeasurementDebugIndex;

            _telemetryTargetAddress = ParsePsuAddress();
            SendChargerCommand(DebugDataId, _telemetryTargetAddress,
                new byte[] { debugIndex }, string.Empty, false, false);
            _requestTemperatureData = !_requestTemperatureData;
        }
        catch (Exception exception)
        {
            TelemetryUpdateTimeTextBlock.Text = $"实时数据请求失败：{exception.Message}";
        }
    }

    private void Service_FrameReceived(object? sender, CanFrameRecord frame)
    {
        TryCompleteCalibrationResponse(frame);
        TryCompleteCalibrationFlagResponse(frame);

        Dispatcher.BeginInvoke(new Action(() =>
        {
            TryUpdateChargerMeasurements(frame);
            Frames.Add(frame);
            _receiveCount++;
            TrimFrameList();
            UpdateCounter();
        }));
    }

    private void TryUpdateChargerMeasurements(CanFrameRecord frame)
    {
        if (frame.Data.Length < 7)
        {
            return;
        }

        uint commandId = (frame.RawId >> 16) & 0xFFFF;
        byte targetAddress = (byte)((frame.RawId >> 8) & 0xFF);
        byte sourceAddress = (byte)(frame.RawId & 0xFF);
        if ((commandId != DebugDataId)
            || (targetAddress != MasterCanAddress)
            || (sourceAddress != _telemetryTargetAddress))
        {
            return;
        }

        if (frame.Data[0] == MeasurementDebugIndex)
        {
            ushort inputVoltage = ReadUInt16LittleEndian(frame.Data, 1);
            ushort outputVoltage = ReadUInt16LittleEndian(frame.Data, 3);
            ushort outputCurrent = ReadUInt16LittleEndian(frame.Data, 5);

            InputVoltageValueTextBlock.Text = $"{inputVoltage / 100M:F2}";
            OutputVoltageValueTextBlock.Text = $"{outputVoltage / 100M:F2}";
            OutputCurrentValueTextBlock.Text = $"{outputCurrent / 100M:F2}";
        }
        else if (frame.Data[0] == TemperatureDebugIndex)
        {
            short heatsinkTemperature = ReadInt16LittleEndian(frame.Data, 5);

            HeatsinkTemperatureValueTextBlock.Text = $"{heatsinkTemperature}";
        }
        else
        {
            return;
        }

        TelemetryAddressTextBlock.Text = $"0x{sourceAddress:X2}";
        TelemetryUpdateTimeTextBlock.Text = $"最后更新：{DateTime.Now:HH:mm:ss}";
    }

    private static ushort ReadUInt16LittleEndian(byte[] data, int index)
    {
        return (ushort)(data[index] | (data[index + 1] << 8));
    }

    private static short ReadInt16LittleEndian(byte[] data, int index)
    {
        return unchecked((short)ReadUInt16LittleEndian(data, index));
    }

    private void Service_ReceiveFaulted(object? sender, Exception exception)
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            SetStatus($"接收线程异常：{exception.Message}");
            UpdateControls();
        }));
    }

    private void Window_Closing(object? sender, CancelEventArgs e)
    {
        if (_closing)
        {
            return;
        }

        SaveChannelSettings();
        _closing = true;
        _telemetryTimer.Stop();
        try
        {
            _service.CloseDeviceAsync().GetAwaiter().GetResult();
        }
        catch
        {
            // 窗口退出时尽力释放设备，避免关闭流程被驱动异常阻断。
        }
    }

    private void SendChargerCommand(ushort commandId, byte psuAddress, byte[] data,
        string status, bool updateStatus = true, bool addToFrameList = true)
    {
        uint canId = ((uint)commandId << 16) | ((uint)psuAddress << 8) | MasterCanAddress;
        CanTransmitRequest request = new(
            canId,
            true,
            false,
            false,
            false,
            ParseTransmitType(),
            data);

        uint sent = _service.Send(request);
        if (sent != 1)
        {
            throw new InvalidOperationException($"CAN报文发送失败，驱动返回发送数量：{sent}");
        }

        _transmitCount++;
        if (addToFrameList)
        {
            Frames.Add(new CanFrameRecord
            {
                ReceivedAt = DateTime.Now,
                RawId = request.Id,
                Data = request.Data,
                Direction = "TX",
                FrameType = "CAN",
                IdText = $"0x{request.Id:X8}",
                Format = "扩展帧",
                FrameKind = "数据帧",
                Length = request.Data.Length,
                DataText = CanFrameRecord.FormatData(request.Data)
            });
            TrimFrameList();
        }

        UpdateCounter();
        if (updateStatus)
        {
            SetStatus(status);
        }
    }

    private void TryCompleteCalibrationFlagResponse(CanFrameRecord frame)
    {
        if (frame.Data.Length < 4)
        {
            return;
        }

        uint commandId = (frame.RawId >> 16) & 0xFFFF;
        byte targetAddress = (byte)((frame.RawId >> 8) & 0xFF);
        byte sourceAddress = (byte)(frame.RawId & 0xFF);
        if ((commandId != CalibrationFlagId)
            || (targetAddress != MasterCanAddress)
            || (sourceAddress != _calibrationFlagTargetAddress))
        {
            return;
        }

        TaskCompletionSource<uint>? response;
        lock (_calibrationFlagResponseLock)
        {
            response = _pendingCalibrationFlagResponse;
        }

        if (response is null)
        {
            return;
        }

        uint flags = frame.Data[0]
            | ((uint)frame.Data[1] << 8)
            | ((uint)frame.Data[2] << 16)
            | ((uint)frame.Data[3] << 24);
        response.TrySetResult(flags);
    }

    private void UpdateCalibrationFlagDisplay(uint flags)
    {
        CalibrationFlagValueTextBlock.Text = $"0x{flags:X8}";
        CalibrationFlagDetailsTextBlock.Text = string.Join("　",
            CalibrationFlagNames.Select((name, bit) =>
                $"bit{bit} {name}:{(((flags >> bit) & 0x01) != 0 ? "有效" : "无效")}"));
    }

    private void TryCompleteCalibrationResponse(CanFrameRecord frame)
    {
        if (frame.Data.Length < 4)
        {
            return;
        }

        uint commandId = (frame.RawId >> 16) & 0xFFFF;
        byte targetAddress = (byte)((frame.RawId >> 8) & 0xFF);
        byte sourceAddress = (byte)(frame.RawId & 0xFF);
        if ((targetAddress != MasterCanAddress)
            || (sourceAddress != _calibrationTargetAddress))
        {
            return;
        }

        TaskCompletionSource<ushort>? response;
        lock (_calibrationResponseLock)
        {
            if ((_pendingCalibrationResponse is null)
                || (commandId != _pendingCalibrationResponseCommandId)
                || (frame.Data[0] != _pendingCalibrationType)
                || (frame.Data[1] != _pendingCalibrationIndex))
            {
                return;
            }

            response = _pendingCalibrationResponse;
        }

        ushort value = (ushort)(frame.Data[2] | (frame.Data[3] << 8));
        response.TrySetResult(value);
    }

    private byte ParsePsuAddress()
    {
        return ParsePsuAddress(PsuAddressTextBox.Text, "目标/合并路地址");
    }

    private byte ParseFloatingPsuAddress()
    {
        return ParsePsuAddress(FloatingPsuAddressTextBox.Text, "悬空路地址");
    }

    private static byte ParsePsuAddress(string text, string fieldName)
    {
        uint address = ParseCanId(text, fieldName);
        if ((address < 0x80) || (address > 0xBF))
        {
            throw new ArgumentOutOfRangeException(nameof(address), $"{fieldName}必须在0x80～0xBF范围内。");
        }

        return (byte)address;
    }

    private static void ValidateParallelAddressPair(byte mergedAddress, byte floatingAddress)
    {
        bool isLabPair = ((mergedAddress == 0x80) && (floatingAddress == 0xBF))
            || ((mergedAddress == 0xBF) && (floatingAddress == 0x80));
        bool isNormalPair = (byte)(floatingAddress ^ 0x01) == mergedAddress;
        if (!isLabPair && !isNormalPair)
        {
            throw new InvalidOperationException(
                "合并路和悬空路地址不匹配。正式版本地址需要异或0x01配对，实验室支持0x80与0xBF配对。");
        }
    }

    private static ushort ParseHundredthValue(string text, string fieldName)
    {
        bool parsed = decimal.TryParse(text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out decimal value)
            || decimal.TryParse(text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out value);
        if (!parsed || (value < 0) || (value > 327.67M))
        {
            throw new FormatException($"{fieldName}必须是0～327.67范围内的数值。");
        }

        return checked((ushort)decimal.Round(value * 100M, 0, MidpointRounding.AwayFromZero));
    }

    private CanDeviceOption GetSelectedDevice()
    {
        return DeviceTypeComboBox.SelectedItem as CanDeviceOption
            ?? throw new InvalidOperationException("请选择CAN设备类型。");
    }

    private uint ParseTransmitType()
    {
        if (TransmitTypeComboBox.SelectedItem is ComboBoxItem item
            && uint.TryParse(item.Tag?.ToString(), out uint transmitType))
        {
            return transmitType;
        }

        return 0;
    }

    private static byte[] ParseDataBytes(string text)
    {
        string[] fields = text.Split(new[] { ' ', ',', ';', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        byte[] data = new byte[fields.Length];
        for (int index = 0; index < fields.Length; index++)
        {
            string field = fields[index].StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? fields[index][2..]
                : fields[index];
            if (!byte.TryParse(field, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out data[index]))
            {
                throw new FormatException($"数据字节“{fields[index]}”不是有效的十六进制数。");
            }
        }

        return data;
    }

    private static uint ParseCanId(string text, string fieldName)
    {
        string value = text.Trim();
        NumberStyles style = NumberStyles.Integer;
        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
        {
            value = value[2..];
            style = NumberStyles.HexNumber;
        }

        if (!uint.TryParse(value, style, CultureInfo.InvariantCulture, out uint result))
        {
            throw new FormatException($"{fieldName}格式错误。");
        }

        return result;
    }

    private static uint ParseUnsigned(string text, string fieldName)
    {
        if (!uint.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out uint result))
        {
            throw new FormatException($"{fieldName}必须是非负整数。");
        }

        return result;
    }

    private void TrimFrameList()
    {
        while (Frames.Count > MaxVisibleFrames)
        {
            Frames.RemoveAt(0);
        }
    }

    private void UpdateCounter()
    {
        CounterTextBlock.Text = $"接收: {_receiveCount}  发送: {_transmitCount}  列表: {Frames.Count}";
    }

    private bool HasCompleteCalibrationValues()
    {
        try
        {
            GetCompleteCalibrationItems(CalibrationValues);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void UpdateControls()
    {
        bool deviceOpen = _service.IsDeviceOpen;
        bool initialized = _service.IsChannelInitialized;
        bool started = _service.IsChannelStarted;
        CanDeviceOption? selectedDevice = DeviceTypeComboBox.SelectedItem as CanDeviceOption;
        bool configurationEnabled = !initialized;

        OpenButton.IsEnabled = !deviceOpen;
        InitializeButton.IsEnabled = deviceOpen && !initialized;
        StartButton.IsEnabled = initialized && !started;
        ResetButton.IsEnabled = initialized;
        CloseButton.IsEnabled = deviceOpen;
        DeviceTypeComboBox.IsEnabled = !deviceOpen;
        DeviceIndexTextBox.IsEnabled = !deviceOpen;
        ChannelComboBox.IsEnabled = configurationEnabled;
        ProtocolComboBox.IsEnabled = false;
        WorkModeComboBox.IsEnabled = configurationEnabled;
        ArbitrationBitrateComboBox.IsEnabled = configurationEnabled;
        DataBitrateComboBox.IsEnabled = configurationEnabled && ProtocolComboBox.SelectedIndex == 1;
        CanFdNonIsoCheckBox.IsEnabled = configurationEnabled && ProtocolComboBox.SelectedIndex == 1;
        FilterModeComboBox.IsEnabled = configurationEnabled && selectedDevice?.SupportsRangeFilter == true;
        FilterStartTextBox.IsEnabled = FilterModeComboBox.IsEnabled && FilterModeComboBox.SelectedIndex != 2;
        FilterEndTextBox.IsEnabled = FilterStartTextBox.IsEnabled;
        TerminationCheckBox.IsEnabled = configurationEnabled && selectedDevice?.SupportsTermination == true;

        bool chargerCommandEnabled = started
            && !_calibrationReadInProgress
            && !_calibrationFlagReadInProgress
            && !_commandSequenceInProgress;
        PsuAddressTextBox.IsEnabled = !_calibrationReadInProgress
            && !_calibrationFlagReadInProgress
            && !_commandSequenceInProgress;
        FloatingPsuAddressTextBox.IsEnabled = !_calibrationReadInProgress
            && !_calibrationFlagReadInProgress
            && !_commandSequenceInProgress;
        OutputVoltageTextBox.IsEnabled = chargerCommandEnabled;
        OutputCurrentTextBox.IsEnabled = chargerCommandEnabled;
        SendOutputReferenceButton.IsEnabled = chargerCommandEnabled;
        EnterFactoryButton.IsEnabled = chargerCommandEnabled;
        EnterCalibrationButton.IsEnabled = chargerCommandEnabled;
        ExitCalibrationButton.IsEnabled = chargerCommandEnabled;
        EnterAgingButton.IsEnabled = chargerCommandEnabled;
        ExitAgingButton.IsEnabled = chargerCommandEnabled;
        EnterParallelButton.IsEnabled = chargerCommandEnabled;
        ExitParallelButton.IsEnabled = chargerCommandEnabled;
        ReadAllCalibrationButton.IsEnabled = chargerCommandEnabled;
        ReadCalibrationFlagButton.IsEnabled = chargerCommandEnabled;
        SaveCalibrationFileButton.IsEnabled = HasCompleteCalibrationValues();
        WriteCalibrationFileButton.IsEnabled = chargerCommandEnabled;
        ClearCalibrationDataButton.IsEnabled = chargerCommandEnabled;
    }

    private void SetStatus(string text)
    {
        StatusTextBlock.Text = $"{DateTime.Now:HH:mm:ss}  {text}";
    }

    private void RunUiAction(Action action)
    {
        try
        {
            action();
        }
        catch (Exception exception)
        {
            SetStatus(exception.Message);
            MessageBox.Show(this, exception.Message, "ZLG CAN", MessageBoxButton.OK, MessageBoxImage.Warning);
            UpdateControls();
        }
    }

    private async Task RunUiActionAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception exception)
        {
            SetStatus(exception.Message);
            MessageBox.Show(this, exception.Message, "ZLG CAN", MessageBoxButton.OK, MessageBoxImage.Warning);
            UpdateControls();
        }
    }

    private sealed record CalibrationDefinition(byte Type, string Name, string[] Fields);

    private sealed record CalibrationBackupItem(byte Type, byte Index, ushort Value);

    private sealed class CalibrationBackupFile
    {
        public const string FormatName = "ZlgCanWpf.CalibrationBackup";

        public string Format { get; init; } = FormatName;
        public int Version { get; init; } = 1;
        public DateTime CreatedAt { get; init; }
        public byte SourceAddress { get; init; }
        public List<CalibrationBackupItem> Items { get; init; } = new();
    }

    private sealed class ChannelSettings
    {
        public uint DeviceType { get; init; }
        public CanDriverKind DriverKind { get; init; }
        public string DeviceIndex { get; init; } = "0";
        public int ChannelIndex { get; init; }
        public int WorkModeIndex { get; init; }
        public int FilterModeIndex { get; init; } = 2;
        public string ArbitrationBitrate { get; init; } = "125000";
        public string DataBitrate { get; init; } = "2000000";
        public bool CanFdNonIso { get; init; }
        public bool EnableTermination { get; init; }
        public string FilterStart { get; init; } = "0x00000000";
        public string FilterEnd { get; init; } = "0x1FFFFFFF";
    }
}
