using System.Runtime.InteropServices;

namespace ZlgCanWpf;

internal static class ControlCanNative
{
    internal const uint StatusOk = 1;

    [StructLayout(LayoutKind.Sequential)]
    internal struct InitConfig
    {
        internal uint AccCode;
        internal uint AccMask;
        internal uint Reserved;
        internal byte Filter;
        internal byte Timing0;
        internal byte Timing1;
        internal byte Mode;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct CanObject
    {
        internal uint Id;
        internal uint Timestamp;
        internal byte TimeFlag;
        internal byte SendType;
        internal byte RemoteFlag;
        internal byte ExternFlag;
        internal byte DataLength;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        internal byte[] Data;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        internal byte[] Reserved;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ErrorInfo
    {
        internal uint ErrorCode;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        internal byte[] PassiveErrorData;
        internal byte ArbitrationLostErrorData;
    }

    [DllImport("ControlCAN.dll", CallingConvention = CallingConvention.StdCall)]
    internal static extern uint VCI_OpenDevice(uint deviceType, uint deviceIndex, uint reserved);

    [DllImport("ControlCAN.dll", CallingConvention = CallingConvention.StdCall)]
    internal static extern uint VCI_CloseDevice(uint deviceType, uint deviceIndex);

    [DllImport("ControlCAN.dll", CallingConvention = CallingConvention.StdCall)]
    internal static extern uint VCI_InitCAN(uint deviceType, uint deviceIndex, uint channelIndex,
        ref InitConfig config);

    [DllImport("ControlCAN.dll", CallingConvention = CallingConvention.StdCall)]
    internal static extern uint VCI_StartCAN(uint deviceType, uint deviceIndex, uint channelIndex);

    [DllImport("ControlCAN.dll", CallingConvention = CallingConvention.StdCall)]
    internal static extern uint VCI_ResetCAN(uint deviceType, uint deviceIndex, uint channelIndex);

    [DllImport("ControlCAN.dll", CallingConvention = CallingConvention.StdCall)]
    internal static extern uint VCI_ClearBuffer(uint deviceType, uint deviceIndex, uint channelIndex);

    [DllImport("ControlCAN.dll", CallingConvention = CallingConvention.StdCall)]
    internal static extern uint VCI_GetReceiveNum(uint deviceType, uint deviceIndex, uint channelIndex);

    [DllImport("ControlCAN.dll", CallingConvention = CallingConvention.StdCall)]
    internal static extern uint VCI_Transmit(uint deviceType, uint deviceIndex, uint channelIndex,
        IntPtr send, uint length);

    [DllImport("ControlCAN.dll", CallingConvention = CallingConvention.StdCall)]
    internal static extern uint VCI_Receive(uint deviceType, uint deviceIndex, uint channelIndex,
        IntPtr receive, uint length, int waitTime);

    [DllImport("ControlCAN.dll", CallingConvention = CallingConvention.StdCall)]
    internal static extern uint VCI_ReadErrInfo(uint deviceType, uint deviceIndex, uint channelIndex,
        IntPtr errorInfo);

    internal static bool TryGetBitTiming(uint bitrate, out byte timing0, out byte timing1)
    {
        timing1 = 0x1C;
        switch (bitrate)
        {
            case 1_000_000: timing0 = 0x00; timing1 = 0x14; return true;
            case 800_000: timing0 = 0x00; timing1 = 0x16; return true;
            case 500_000: timing0 = 0x00; return true;
            case 250_000: timing0 = 0x01; return true;
            case 125_000: timing0 = 0x03; return true;
            case 100_000: timing0 = 0x04; return true;
            case 50_000: timing0 = 0x09; return true;
            case 20_000: timing0 = 0x18; return true;
            case 10_000: timing0 = 0x31; return true;
            case 5_000: timing0 = 0x63; return true;
            default: timing0 = 0; return false;
        }
    }
}
