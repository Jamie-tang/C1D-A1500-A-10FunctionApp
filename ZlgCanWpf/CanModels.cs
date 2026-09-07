using System.Globalization;

namespace ZlgCanWpf;

public enum CanDriverKind
{
    ZlgCan,
    ControlCan
}

public sealed record CanDeviceOption(
    string Name,
    uint DeviceType,
    uint ChannelCount,
    bool SupportsCanFd,
    bool SupportsRangeFilter,
    bool SupportsTermination,
    CanDriverKind DriverKind = CanDriverKind.ZlgCan)
{
    public override string ToString() => Name;
}

public sealed class CanFrameRecord
{
    public DateTime ReceivedAt { get; init; }
    public ulong HardwareTimestampUs { get; init; }
    public uint RawId { get; init; }
    public byte[] Data { get; init; } = Array.Empty<byte>();
    public string Direction { get; init; } = "RX";
    public string FrameType { get; init; } = "CAN";
    public string IdText { get; init; } = string.Empty;
    public string Format { get; init; } = string.Empty;
    public string FrameKind { get; init; } = string.Empty;
    public int Length { get; init; }
    public string DataText { get; init; } = string.Empty;

    public static string FormatData(ReadOnlySpan<byte> data)
    {
        return string.Join(" ", data.ToArray().Select(value => value.ToString("X2", CultureInfo.InvariantCulture)));
    }
}

public sealed class CalibrationValueRecord
{
    public string GroupName { get; init; } = string.Empty;
    public string TypeText { get; init; } = string.Empty;
    public int Index { get; init; }
    public string FieldName { get; init; } = string.Empty;
    public ushort Value { get; init; }
    public string ValueText { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}

public sealed record CanChannelOptions(
    uint ChannelIndex,
    bool UseCanFd,
    uint ArbitrationBitrate,
    uint DataBitrate,
    bool CanFdNonIso,
    bool ListenOnly,
    bool EnableTermination,
    int FilterMode,
    uint FilterStart,
    uint FilterEnd);

public sealed record CanTransmitRequest(
    uint Id,
    bool Extended,
    bool Remote,
    bool UseCanFd,
    bool BitRateSwitch,
    uint TransmitType,
    byte[] Data);
