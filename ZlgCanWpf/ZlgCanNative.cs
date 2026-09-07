using System.Runtime.InteropServices;

namespace ZlgCanWpf;

internal static class ZlgCanNative
{
    internal const uint StatusOk = 1;
    internal const byte TypeCan = 0;
    internal const byte TypeCanFd = 1;

    internal const uint CanEffFlag = 0x80000000U;
    internal const uint CanRtrFlag = 0x40000000U;
    internal const uint CanErrFlag = 0x20000000U;
    internal const uint CanIdMask = 0x1FFFFFFFU;
    internal const byte CanFdBrs = 0x01;

    internal const uint UsbCan1 = 3;
    internal const uint UsbCan2 = 4;
    internal const uint Pci9820I = 16;
    internal const uint UsbCanEU = 20;
    internal const uint UsbCan2EU = 21;
    internal const uint UsbCan4EU = 31;
    internal const uint UsbCanFd200U = 41;
    internal const uint UsbCanFd100U = 42;
    internal const uint UsbCanFdMini = 43;
    internal const uint UsbCanFd800U = 59;
    internal const uint UsbCanFd400U = 76;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ClassicCanConfig
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
    internal struct CanFdConfig
    {
        internal uint AccCode;
        internal uint AccMask;
        internal uint ArbitrationTiming;
        internal uint DataTiming;
        internal uint Brp;
        internal byte Filter;
        internal byte Mode;
        internal ushort Padding;
        internal uint Reserved;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct ChannelInitConfig
    {
        [FieldOffset(0)] internal uint CanType;
        [FieldOffset(4)] internal ClassicCanConfig Can;
        [FieldOffset(4)] internal CanFdConfig CanFd;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ClassicCanFrame
    {
        internal uint CanId;
        internal byte DataLength;
        internal byte Padding;
        internal byte Reserved0;
        internal byte Reserved1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        internal byte[] Data;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct CanFdFrame
    {
        internal uint CanId;
        internal byte Length;
        internal byte Flags;
        internal byte Reserved0;
        internal byte Reserved1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        internal byte[] Data;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TransmitData
    {
        internal ClassicCanFrame Frame;
        internal uint TransmitType;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ReceiveData
    {
        internal ClassicCanFrame Frame;
        internal ulong Timestamp;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct TransmitFdData
    {
        internal CanFdFrame Frame;
        internal uint TransmitType;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ReceiveFdData
    {
        internal CanFdFrame Frame;
        internal ulong Timestamp;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct ChannelErrorInfo
    {
        internal uint ErrorCode;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        internal byte[] PassiveErrorData;
        internal byte ArbitrationLostErrorData;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DeviceInfo
    {
        internal ushort HardwareVersion;
        internal ushort FirmwareVersion;
        internal ushort DriverVersion;
        internal ushort InterfaceVersion;
        internal ushort IrqNumber;
        internal byte CanChannelCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        internal byte[] SerialNumber;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        internal byte[] HardwareType;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        internal ushort[] Reserved;
    }

    [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
    internal static extern IntPtr ZCAN_OpenDevice(uint deviceType, uint deviceIndex, uint reserved);

    [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
    internal static extern uint ZCAN_CloseDevice(IntPtr deviceHandle);

    [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
    internal static extern IntPtr ZCAN_InitCAN(IntPtr deviceHandle, uint channelIndex, IntPtr initConfig);

    [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
    internal static extern uint ZCAN_GetDeviceInf(IntPtr deviceHandle, IntPtr deviceInfo);

    [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
    internal static extern uint ZCAN_SetValue(IntPtr deviceHandle, string path, byte[] value);

    [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
    internal static extern uint ZCAN_StartCAN(IntPtr channelHandle);

    [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
    internal static extern uint ZCAN_ResetCAN(IntPtr channelHandle);

    [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
    internal static extern uint ZCAN_ClearBuffer(IntPtr channelHandle);

    [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
    internal static extern uint ZCAN_Transmit(IntPtr channelHandle, IntPtr transmitData, uint length);

    [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
    internal static extern uint ZCAN_TransmitFD(IntPtr channelHandle, IntPtr transmitData, uint length);

    [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
    internal static extern uint ZCAN_GetReceiveNum(IntPtr channelHandle, byte type);

    [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
    internal static extern uint ZCAN_Receive(IntPtr channelHandle, IntPtr data, uint length, int waitTime);

    [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
    internal static extern uint ZCAN_ReceiveFD(IntPtr channelHandle, IntPtr data, uint length, int waitTime);

    [DllImport("zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
    internal static extern uint ZCAN_ReadChannelErrInfo(IntPtr channelHandle, IntPtr errorInfo);
}
