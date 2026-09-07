using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace FaultCodeAscParser;

/// <summary>ASC 文件中的一帧 CAN 报文，同时提供界面显示所需的格式化字段。</summary>
public sealed class AscFrame : INotifyPropertyChanged
{
    private double _timestamp;
    private int _channel;
    private uint _canId;
    private bool _isExtended;
    private string _direction;
    private byte[] _data;
    private int _sourceLine;
    private DateTime _actualTimestamp;

    public AscFrame(double timestamp, int channel, uint canId, bool isExtended, string direction, byte[] data,
        int sourceLine, DateTime actualTimestamp)
    {
        _timestamp = timestamp;
        _channel = channel;
        _canId = canId;
        _isExtended = isExtended;
        _direction = direction;
        _data = data;
        _sourceLine = sourceLine;
        _actualTimestamp = actualTimestamp;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public double Timestamp
    {
        get => _timestamp;
        set => SetField(ref _timestamp, value);
    }

    public int Channel
    {
        get => _channel;
        set => SetField(ref _channel, value);
    }

    public uint CanId
    {
        get => _canId;
        set
        {
            if (!SetField(ref _canId, value))
                return;

            OnPropertyChanged(nameof(CanIdText));
        }
    }

    public bool IsExtended
    {
        get => _isExtended;
        set
        {
            if (!SetField(ref _isExtended, value))
                return;

            OnPropertyChanged(nameof(CanIdText));
        }
    }

    public string Direction
    {
        get => _direction;
        set => SetField(ref _direction, value);
    }

    public byte[] Data
    {
        get => _data;
        set
        {
            if (ReferenceEquals(_data, value))
                return;

            _data = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DataLength));
            OnPropertyChanged(nameof(DataText));
            OnPropertyChanged(nameof(Data0Text));
        }
    }

    public int SourceLine
    {
        get => _sourceLine;
        set => SetField(ref _sourceLine, value);
    }

    public DateTime ActualTimestamp
    {
        get => _actualTimestamp;
        set
        {
            if (!SetField(ref _actualTimestamp, value))
                return;

            OnPropertyChanged(nameof(TimestampText));
        }
    }

    /// <summary>解析报文时已按“第一行 date 基准 + 当前报文相对秒”计算的实际时间，固定使用24小时制。</summary>
    public string TimestampText
    {
        get => ActualTimestamp.ToString("yyyy-MM-dd HH:mm:ss.fff");
        set
        {
            if (DateTime.TryParseExact(value, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var actualTimestamp))
            {
                ActualTimestamp = actualTimestamp;
            }
        }
    }

    /// <summary>按标准帧或扩展帧格式显示 CAN ID。</summary>
    public string CanIdText
    {
        get => IsExtended ? $"0x{CanId:X8}" : $"0x{CanId:X3}";
        set
        {
            var text = value.Trim();
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                text = text[2..];

            if (uint.TryParse(text, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var canId))
                CanId = canId;
        }
    }

    /// <summary>CAN 数据长度；由 data0~data7 编辑内容自动计算。</summary>
    public int DataLength => Data.Length;

    /// <summary>以 data0~data7 的顺序显示实际报文数据。</summary>
    public string DataText
    {
        get => Data.Length == 0 ? "" : string.Join(" ", Data.Select(value => value.ToString("X2")));
        set
        {
            var fields = value.Split([' ', '\t', ',', ';'], StringSplitOptions.RemoveEmptyEntries);
            if (fields.Length > 8)
                return;

            var data = new byte[fields.Length];
            for (var index = 0; index < fields.Length; index++)
            {
                var field = fields[index];
                if (field.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    field = field[2..];

                if (!byte.TryParse(field, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out data[index]))
                    return;
            }

            Data = data;
        }
    }

    /// <summary>显示首字节，便于定位固件 CAN 调试页选择字。</summary>
    public string Data0Text
    {
        get => Data.Length == 0 ? "--" : $"0x{Data[0]:X2}";
        set
        {
            var text = value.Trim();
            if (text.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                text = text[2..];

            if (!byte.TryParse(text, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var data0))
                return;

            var data = Data.Length == 0 ? new byte[1] : Data.ToArray();
            data[0] = data0;
            Data = data;
        }
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

/// <summary>ASC 文件异步读取完成后的解析结果。</summary>
public sealed record AscParseResult(IReadOnlyList<AscFrame> Frames, int IgnoredLineCount, DateTime InitialTime);

/// <summary>右侧故障位表的一行：bit 位、故障含义及当前是否置位。</summary>
public sealed record FaultVisualRow(int Bit, string FaultName, bool IsSet);

/// <summary>已解码的一条 PDF 故障信息。</summary>
public sealed class FaultOccurrence
{
    public FaultOccurrence(double timestamp, int channel, uint canId, string direction, byte selector, string domain,
        int pdfNumber, string faultName, string state)
    {
        Timestamp = timestamp;
        Channel = channel;
        CanId = canId;
        Direction = direction;
        Selector = selector;
        Domain = domain;
        PdfNumber = pdfNumber;
        FaultName = faultName;
        State = state;
    }

    public double Timestamp { get; set; }
    public int Channel { get; set; }
    public uint CanId { get; set; }
    public string Direction { get; set; }
    public byte Selector { get; set; }
    public string Domain { get; set; }
    public int PdfNumber { get; set; }
    public string FaultName { get; set; }
    public string State { get; set; }

    /// <summary>仅“首次故障”记录保留固件原始 u16FirstCode，其他故障项显示“--”。</summary>
    public string FirstFaultCodeText { get; set; } = "--";
}
