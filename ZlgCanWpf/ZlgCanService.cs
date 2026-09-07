using System.Runtime.InteropServices;
using System.Text;

namespace ZlgCanWpf;

public sealed class ZlgCanService : IAsyncDisposable
{
    private const uint MaxReceiveFrames = 100;

    private readonly object _syncRoot = new();
    private IntPtr _deviceHandle;
    private IntPtr _channelHandle;
    private uint _controlDeviceType;
    private uint _controlDeviceIndex;
    private uint _controlChannelIndex;
    private bool _controlDeviceOpen;
    private bool _controlChannelInitialized;
    private CancellationTokenSource? _receiveCancellation;
    private Task? _receiveTask;
    private CanDeviceOption? _deviceOption;
    private bool _channelUsesCanFd;

    public event EventHandler<CanFrameRecord>? FrameReceived;
    public event EventHandler<Exception>? ReceiveFaulted;

    public bool IsDeviceOpen => _controlDeviceOpen || (_deviceHandle != IntPtr.Zero);
    public bool IsChannelInitialized => _controlChannelInitialized || (_channelHandle != IntPtr.Zero);
    public bool IsChannelStarted => _receiveTask is { IsCompleted: false };

    private bool UsesControlCan => _deviceOption?.DriverKind == CanDriverKind.ControlCan;

    public string OpenDevice(CanDeviceOption option, uint deviceIndex)
    {
        lock (_syncRoot)
        {
            if (IsDeviceOpen)
            {
                throw new InvalidOperationException("设备已经打开，请先关闭当前设备。");
            }

            if (option.DriverKind == CanDriverKind.ControlCan)
            {
                if (ControlCanNative.VCI_OpenDevice(option.DeviceType, deviceIndex, 0) != ControlCanNative.StatusOk)
                {
                    throw new InvalidOperationException("打开创芯CAN设备失败，请检查设备连接、驱动和设备索引。");
                }

                _controlDeviceType = option.DeviceType;
                _controlDeviceIndex = deviceIndex;
                _controlDeviceOpen = true;
                _deviceOption = option;
                return $"{option.Name}  设备索引:{deviceIndex}  通道数:{option.ChannelCount}";
            }

            _deviceHandle = ZlgCanNative.ZCAN_OpenDevice(option.DeviceType, deviceIndex, 0);
            if (_deviceHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("打开CAN设备失败，请检查设备连接、驱动和设备索引。");
            }

            _deviceOption = option;
            return ReadDeviceDescription();
        }
    }

    public void InitializeChannel(CanChannelOptions options)
    {
        lock (_syncRoot)
        {
            EnsureDeviceOpen();
            if (IsChannelInitialized)
            {
                throw new InvalidOperationException("通道已经初始化，请先复位当前通道。");
            }

            if (UsesControlCan)
            {
                InitializeControlCan(options);
                return;
            }

            if (options.UseCanFd)
            {
                SetValue(options.ChannelIndex, "canfd_abit_baud_rate", options.ArbitrationBitrate.ToString());
                SetValue(options.ChannelIndex, "canfd_dbit_baud_rate", options.DataBitrate.ToString());
                if ((_deviceOption is { SupportsTermination: true } deviceOption)
                    && (deviceOption.DeviceType != ZlgCanNative.UsbCanFd800U))
                {
                    SetValue(options.ChannelIndex, "canfd_standard", options.CanFdNonIso ? "1" : "0");
                }
            }
            else
            {
                SetValue(options.ChannelIndex, "baud_rate", options.ArbitrationBitrate.ToString());
            }

            ZlgCanNative.ChannelInitConfig config;
            if (options.UseCanFd)
            {
                config = new ZlgCanNative.ChannelInitConfig
                {
                    CanType = ZlgCanNative.TypeCanFd,
                    CanFd = new ZlgCanNative.CanFdConfig
                    {
                        AccCode = 0,
                        AccMask = uint.MaxValue,
                        Filter = 0,
                        Mode = options.ListenOnly ? (byte)1 : (byte)0
                    }
                };
            }
            else
            {
                config = new ZlgCanNative.ChannelInitConfig
                {
                    CanType = ZlgCanNative.TypeCan,
                    Can = new ZlgCanNative.ClassicCanConfig
                    {
                        AccCode = 0,
                        AccMask = uint.MaxValue,
                        Filter = 0,
                        Mode = options.ListenOnly ? (byte)1 : (byte)0
                    }
                };
            }

            IntPtr configPointer = Marshal.AllocHGlobal(Marshal.SizeOf<ZlgCanNative.ChannelInitConfig>());
            try
            {
                Marshal.StructureToPtr(config, configPointer, false);
                _channelHandle = ZlgCanNative.ZCAN_InitCAN(_deviceHandle, options.ChannelIndex, configPointer);
            }
            finally
            {
                Marshal.FreeHGlobal(configPointer);
            }

            if (_channelHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("初始化CAN通道失败，请检查通道号和波特率配置。");
            }

            _channelUsesCanFd = options.UseCanFd;

            if ((_deviceOption?.SupportsTermination == true) && options.EnableTermination)
            {
                SetValue(options.ChannelIndex, "initenal_resistance", "1");
            }

            if (_deviceOption?.SupportsRangeFilter == true)
            {
                ConfigureFilter(options);
            }
        }
    }

    public void StartChannel()
    {
        lock (_syncRoot)
        {
            EnsureChannelInitialized();
            if (IsChannelStarted)
            {
                return;
            }

            if (UsesControlCan)
            {
                if (ControlCanNative.VCI_StartCAN(_controlDeviceType, _controlDeviceIndex,
                    _controlChannelIndex) != ControlCanNative.StatusOk)
                {
                    throw new InvalidOperationException("启动创芯CAN通道失败。");
                }
            }
            else if (ZlgCanNative.ZCAN_StartCAN(_channelHandle) != ZlgCanNative.StatusOk)
            {
                throw new InvalidOperationException("启动CAN通道失败。");
            }

            _receiveCancellation = new CancellationTokenSource();
            CancellationToken token = _receiveCancellation.Token;
            _receiveTask = Task.Run(() => ReceiveLoopAsync(token), token);
        }
    }

    public async Task ResetChannelAsync()
    {
        await StopReceiveAsync().ConfigureAwait(false);

        lock (_syncRoot)
        {
            if (UsesControlCan)
            {
                if (!_controlChannelInitialized)
                {
                    return;
                }

                if (ControlCanNative.VCI_ResetCAN(_controlDeviceType, _controlDeviceIndex,
                    _controlChannelIndex) != ControlCanNative.StatusOk)
                {
                    throw new InvalidOperationException("复位创芯CAN通道失败。");
                }

                _controlChannelInitialized = false;
                return;
            }

            if (_channelHandle == IntPtr.Zero)
            {
                return;
            }

            if (ZlgCanNative.ZCAN_ResetCAN(_channelHandle) != ZlgCanNative.StatusOk)
            {
                throw new InvalidOperationException("复位CAN通道失败。");
            }

            _channelHandle = IntPtr.Zero;
            _channelUsesCanFd = false;
        }
    }

    public void ClearReceiveBuffer()
    {
        lock (_syncRoot)
        {
            EnsureChannelInitialized();
            if (UsesControlCan)
            {
                if (ControlCanNative.VCI_ClearBuffer(_controlDeviceType, _controlDeviceIndex,
                    _controlChannelIndex) != ControlCanNative.StatusOk)
                {
                    throw new InvalidOperationException("清空创芯CAN接收缓冲区失败。");
                }

                return;
            }

            if (ZlgCanNative.ZCAN_ClearBuffer(_channelHandle) != ZlgCanNative.StatusOk)
            {
                throw new InvalidOperationException("清空CAN接收缓冲区失败。");
            }
        }
    }

    public uint Send(CanTransmitRequest request)
    {
        lock (_syncRoot)
        {
            EnsureChannelInitialized();
            if (!IsChannelStarted)
            {
                throw new InvalidOperationException("CAN通道尚未启动。");
            }

            if (UsesControlCan)
            {
                return SendControlClassicCan(request);
            }

            return request.UseCanFd ? SendCanFd(request) : SendClassicCan(request);
        }
    }

    public ZlgCanError ReadChannelError()
    {
        lock (_syncRoot)
        {
            EnsureChannelInitialized();
            if (UsesControlCan)
            {
                return ReadControlCanError();
            }

            IntPtr pointer = Marshal.AllocHGlobal(Marshal.SizeOf<ZlgCanNative.ChannelErrorInfo>());
            try
            {
                if (ZlgCanNative.ZCAN_ReadChannelErrInfo(_channelHandle, pointer) != ZlgCanNative.StatusOk)
                {
                    throw new InvalidOperationException("读取CAN通道错误信息失败。");
                }

                ZlgCanNative.ChannelErrorInfo info = Marshal.PtrToStructure<ZlgCanNative.ChannelErrorInfo>(pointer);
                return new ZlgCanError(
                    info.ErrorCode,
                    info.PassiveErrorData ?? Array.Empty<byte>(),
                    info.ArbitrationLostErrorData);
            }
            finally
            {
                Marshal.FreeHGlobal(pointer);
            }
        }
    }

    public async Task CloseDeviceAsync()
    {
        await StopReceiveAsync().ConfigureAwait(false);

        lock (_syncRoot)
        {
            if (UsesControlCan)
            {
                if (_controlChannelInitialized)
                {
                    ControlCanNative.VCI_ResetCAN(_controlDeviceType, _controlDeviceIndex,
                        _controlChannelIndex);
                    _controlChannelInitialized = false;
                }

                if (_controlDeviceOpen)
                {
                    ControlCanNative.VCI_CloseDevice(_controlDeviceType, _controlDeviceIndex);
                    _controlDeviceOpen = false;
                }
            }
            else
            {
                if (_channelHandle != IntPtr.Zero)
                {
                    ZlgCanNative.ZCAN_ResetCAN(_channelHandle);
                    _channelHandle = IntPtr.Zero;
                }

                if (_deviceHandle != IntPtr.Zero)
                {
                    ZlgCanNative.ZCAN_CloseDevice(_deviceHandle);
                    _deviceHandle = IntPtr.Zero;
                }
            }

            _deviceOption = null;
            _channelUsesCanFd = false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await CloseDeviceAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    private void InitializeControlCan(CanChannelOptions options)
    {
        if (options.UseCanFd)
        {
            throw new InvalidOperationException("创芯CAN设备不支持CAN FD。");
        }

        if (!ControlCanNative.TryGetBitTiming(options.ArbitrationBitrate,
            out byte timing0, out byte timing1))
        {
            throw new InvalidOperationException("创芯CAN设备不支持当前仲裁域波特率。");
        }

        ControlCanNative.InitConfig config = new()
        {
            AccCode = 0,
            AccMask = uint.MaxValue,
            Filter = 1,
            Timing0 = timing0,
            Timing1 = timing1,
            Mode = options.ListenOnly ? (byte)1 : (byte)0
        };

        if (ControlCanNative.VCI_InitCAN(_controlDeviceType, _controlDeviceIndex,
            options.ChannelIndex, ref config) != ControlCanNative.StatusOk)
        {
            throw new InvalidOperationException("初始化创芯CAN通道失败，请检查通道号和波特率配置。");
        }

        _controlChannelIndex = options.ChannelIndex;
        _controlChannelInitialized = true;
        _channelUsesCanFd = false;
    }

    private uint SendControlClassicCan(CanTransmitRequest request)
    {
        if (request.UseCanFd)
        {
            throw new ArgumentException("创芯CAN设备不支持CAN FD。", nameof(request));
        }

        if (request.Data.Length > 8)
        {
            throw new ArgumentException("经典CAN数据长度不能超过8字节。", nameof(request));
        }

        uint maxId = request.Extended ? 0x1FFFFFFFU : 0x7FFU;
        if (request.Id > maxId)
        {
            throw new ArgumentOutOfRangeException(nameof(request), request.Extended
                ? "扩展帧ID必须在0x00000000～0x1FFFFFFF范围内。"
                : "标准帧ID必须在0x000～0x7FF范围内。");
        }

        ControlCanNative.CanObject frame = new()
        {
            Id = request.Id,
            SendType = 0,
            RemoteFlag = request.Remote ? (byte)1 : (byte)0,
            ExternFlag = request.Extended ? (byte)1 : (byte)0,
            DataLength = (byte)request.Data.Length,
            Data = CopyToFixedLength(request.Data, 8),
            Reserved = new byte[3]
        };

        IntPtr pointer = Marshal.AllocHGlobal(Marshal.SizeOf<ControlCanNative.CanObject>());
        try
        {
            Marshal.StructureToPtr(frame, pointer, false);
            return ControlCanNative.VCI_Transmit(_controlDeviceType, _controlDeviceIndex,
                _controlChannelIndex, pointer, 1);
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    private ZlgCanError ReadControlCanError()
    {
        IntPtr pointer = Marshal.AllocHGlobal(Marshal.SizeOf<ControlCanNative.ErrorInfo>());
        try
        {
            if (ControlCanNative.VCI_ReadErrInfo(_controlDeviceType, _controlDeviceIndex,
                _controlChannelIndex, pointer) != ControlCanNative.StatusOk)
            {
                throw new InvalidOperationException("读取创芯CAN通道错误信息失败。");
            }

            ControlCanNative.ErrorInfo info = Marshal.PtrToStructure<ControlCanNative.ErrorInfo>(pointer);
            return new ZlgCanError(info.ErrorCode, info.PassiveErrorData ?? Array.Empty<byte>(),
                info.ArbitrationLostErrorData);
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    private uint SendClassicCan(CanTransmitRequest request)
    {
        if (request.Data.Length > 8)
        {
            throw new ArgumentException("经典CAN数据长度不能超过8字节。", nameof(request));
        }

        ZlgCanNative.TransmitData transmit = new()
        {
            Frame = new ZlgCanNative.ClassicCanFrame
            {
                CanId = MakeCanId(request),
                DataLength = (byte)request.Data.Length,
                Data = CopyToFixedLength(request.Data, 8)
            },
            TransmitType = request.TransmitType
        };

        return TransmitStructure(transmit, ZlgCanNative.ZCAN_Transmit);
    }

    private uint SendCanFd(CanTransmitRequest request)
    {
        if (!_channelUsesCanFd)
        {
            throw new InvalidOperationException("当前通道不是CAN FD通道。");
        }

        if (request.Remote)
        {
            throw new ArgumentException("CAN FD不支持远程帧。", nameof(request));
        }

        if (request.Data.Length > 64)
        {
            throw new ArgumentException("CAN FD数据长度不能超过64字节。", nameof(request));
        }

        ZlgCanNative.TransmitFdData transmit = new()
        {
            Frame = new ZlgCanNative.CanFdFrame
            {
                CanId = MakeCanId(request),
                Length = (byte)request.Data.Length,
                Flags = request.BitRateSwitch ? ZlgCanNative.CanFdBrs : (byte)0,
                Data = CopyToFixedLength(request.Data, 64)
            },
            TransmitType = request.TransmitType
        };

        return TransmitStructure(transmit, ZlgCanNative.ZCAN_TransmitFD);
    }

    private uint TransmitStructure<T>(T transmit, Func<IntPtr, IntPtr, uint, uint> transmitFunction)
        where T : struct
    {
        IntPtr pointer = Marshal.AllocHGlobal(Marshal.SizeOf<T>());
        try
        {
            Marshal.StructureToPtr(transmit, pointer, false);
            return transmitFunction(_channelHandle, pointer, 1);
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                bool received;
                if (UsesControlCan)
                {
                    received = ReceiveControlClassicFrames();
                }
                else
                {
                    received = ReceiveClassicFrames();
                    if (_channelUsesCanFd)
                    {
                        received |= ReceiveCanFdFrames();
                    }
                }

                if (!received)
                {
                    await Task.Delay(2, token).ConfigureAwait(false);
                }
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            ReceiveFaulted?.Invoke(this, exception);
        }
    }

    private bool ReceiveControlClassicFrames()
    {
        if (!_controlChannelInitialized)
        {
            return false;
        }

        uint pending = ControlCanNative.VCI_GetReceiveNum(_controlDeviceType, _controlDeviceIndex,
            _controlChannelIndex);
        if (pending == uint.MaxValue)
        {
            return false;
        }

        uint receiveCount = Math.Min(pending, MaxReceiveFrames);
        if (receiveCount == 0)
        {
            return false;
        }

        int structureSize = Marshal.SizeOf<ControlCanNative.CanObject>();
        IntPtr pointer = Marshal.AllocHGlobal(checked(structureSize * (int)receiveCount));
        try
        {
            uint received = ControlCanNative.VCI_Receive(_controlDeviceType, _controlDeviceIndex,
                _controlChannelIndex, pointer, receiveCount, 0);
            if (received > receiveCount)
            {
                throw new InvalidOperationException("创芯CAN接收接口返回无效帧数量。");
            }

            for (uint index = 0; index < received; index++)
            {
                IntPtr itemPointer = IntPtr.Add(pointer, checked((int)index * structureSize));
                ControlCanNative.CanObject item = Marshal.PtrToStructure<ControlCanNative.CanObject>(itemPointer);
                uint rawCanId = item.Id;
                if (item.ExternFlag != 0)
                {
                    rawCanId |= ZlgCanNative.CanEffFlag;
                }

                if (item.RemoteFlag != 0)
                {
                    rawCanId |= ZlgCanNative.CanRtrFlag;
                }

                PublishFrame(rawCanId, item.DataLength, item.Data, item.Timestamp, false, 0);
            }

            return received > 0;
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    private bool ReceiveClassicFrames()
    {
        IntPtr channelHandle = _channelHandle;
        if (channelHandle == IntPtr.Zero)
        {
            return false;
        }

        uint pending = ZlgCanNative.ZCAN_GetReceiveNum(channelHandle, ZlgCanNative.TypeCan);
        uint receiveCount = Math.Min(pending, MaxReceiveFrames);
        if (receiveCount == 0)
        {
            return false;
        }

        int structureSize = Marshal.SizeOf<ZlgCanNative.ReceiveData>();
        IntPtr pointer = Marshal.AllocHGlobal(checked(structureSize * (int)receiveCount));
        try
        {
            uint received = ZlgCanNative.ZCAN_Receive(channelHandle, pointer, receiveCount, 0);
            for (uint index = 0; index < received; index++)
            {
                IntPtr itemPointer = IntPtr.Add(pointer, checked((int)index * structureSize));
                ZlgCanNative.ReceiveData item = Marshal.PtrToStructure<ZlgCanNative.ReceiveData>(itemPointer);
                PublishFrame(item.Frame.CanId, item.Frame.DataLength, item.Frame.Data, item.Timestamp, false, 0);
            }

            return received > 0;
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    private bool ReceiveCanFdFrames()
    {
        IntPtr channelHandle = _channelHandle;
        if (channelHandle == IntPtr.Zero)
        {
            return false;
        }

        uint pending = ZlgCanNative.ZCAN_GetReceiveNum(channelHandle, ZlgCanNative.TypeCanFd);
        uint receiveCount = Math.Min(pending, MaxReceiveFrames);
        if (receiveCount == 0)
        {
            return false;
        }

        int structureSize = Marshal.SizeOf<ZlgCanNative.ReceiveFdData>();
        IntPtr pointer = Marshal.AllocHGlobal(checked(structureSize * (int)receiveCount));
        try
        {
            uint received = ZlgCanNative.ZCAN_ReceiveFD(channelHandle, pointer, receiveCount, 0);
            for (uint index = 0; index < received; index++)
            {
                IntPtr itemPointer = IntPtr.Add(pointer, checked((int)index * structureSize));
                ZlgCanNative.ReceiveFdData item = Marshal.PtrToStructure<ZlgCanNative.ReceiveFdData>(itemPointer);
                PublishFrame(item.Frame.CanId, item.Frame.Length, item.Frame.Data, item.Timestamp, true, item.Frame.Flags);
            }

            return received > 0;
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    private void PublishFrame(uint rawCanId, byte length, byte[]? buffer, ulong timestamp, bool canFd, byte flags)
    {
        int safeLength = Math.Min(length, (byte)(buffer?.Length ?? 0));
        byte[] data = safeLength == 0 ? Array.Empty<byte>() : buffer![..safeLength];
        uint id = rawCanId & ZlgCanNative.CanIdMask;

        FrameReceived?.Invoke(this, new CanFrameRecord
        {
            ReceivedAt = DateTime.Now,
            HardwareTimestampUs = timestamp,
            RawId = id,
            Data = data,
            Direction = "RX",
            FrameType = canFd ? ((flags & ZlgCanNative.CanFdBrs) != 0 ? "CAN FD+BRS" : "CAN FD") : "CAN",
            IdText = (rawCanId & ZlgCanNative.CanEffFlag) != 0 ? $"0x{id:X8}" : $"0x{id:X3}",
            Format = (rawCanId & ZlgCanNative.CanEffFlag) != 0 ? "扩展帧" : "标准帧",
            FrameKind = (rawCanId & ZlgCanNative.CanErrFlag) != 0
                ? "错误帧"
                : (rawCanId & ZlgCanNative.CanRtrFlag) != 0 ? "远程帧" : "数据帧",
            Length = safeLength,
            DataText = CanFrameRecord.FormatData(data)
        });
    }

    private async Task StopReceiveAsync()
    {
        CancellationTokenSource? cancellation;
        Task? task;
        lock (_syncRoot)
        {
            cancellation = _receiveCancellation;
            task = _receiveTask;
            _receiveCancellation = null;
            _receiveTask = null;
        }

        if (cancellation is null)
        {
            return;
        }

        cancellation.Cancel();
        try
        {
            if (task is not null)
            {
                await task.ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            cancellation.Dispose();
        }
    }

    private void ConfigureFilter(CanChannelOptions options)
    {
        SetValue(options.ChannelIndex, "filter_clear", "0");
        if (options.FilterMode == 2)
        {
            return;
        }

        SetValue(options.ChannelIndex, "filter_mode", options.FilterMode.ToString());
        SetValue(options.ChannelIndex, "filter_start", $"0x{options.FilterStart:X}");
        SetValue(options.ChannelIndex, "filter_end", $"0x{options.FilterEnd:X}");
        SetValue(options.ChannelIndex, "filter_ack", "0");
    }

    private void SetValue(uint channelIndex, string propertyName, string value)
    {
        string path = $"{channelIndex}/{propertyName}";
        if (ZlgCanNative.ZCAN_SetValue(_deviceHandle, path, Encoding.ASCII.GetBytes(value)) != ZlgCanNative.StatusOk)
        {
            throw new InvalidOperationException($"设置CAN参数失败：{path} = {value}");
        }
    }

    private string ReadDeviceDescription()
    {
        IntPtr pointer = Marshal.AllocHGlobal(Marshal.SizeOf<ZlgCanNative.DeviceInfo>());
        try
        {
            if (ZlgCanNative.ZCAN_GetDeviceInf(_deviceHandle, pointer) != ZlgCanNative.StatusOk)
            {
                return "设备已打开";
            }

            ZlgCanNative.DeviceInfo info = Marshal.PtrToStructure<ZlgCanNative.DeviceInfo>(pointer);
            string hardwareType = DecodeNullTerminated(info.HardwareType);
            string serialNumber = DecodeNullTerminated(info.SerialNumber);
            return $"{hardwareType}  SN:{serialNumber}  通道数:{info.CanChannelCount}";
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    private static string DecodeNullTerminated(byte[]? value)
    {
        if (value is null || value.Length == 0)
        {
            return string.Empty;
        }

        int length = Array.IndexOf(value, (byte)0);
        if (length < 0)
        {
            length = value.Length;
        }

        return Encoding.ASCII.GetString(value, 0, length).Trim();
    }

    private static uint MakeCanId(CanTransmitRequest request)
    {
        uint maxId = request.Extended ? 0x1FFFFFFFU : 0x7FFU;
        if (request.Id > maxId)
        {
            throw new ArgumentOutOfRangeException(nameof(request), request.Extended
                ? "扩展帧ID必须在0x00000000～0x1FFFFFFF范围内。"
                : "标准帧ID必须在0x000～0x7FF范围内。");
        }

        uint rawId = request.Id;
        if (request.Extended)
        {
            rawId |= ZlgCanNative.CanEffFlag;
        }

        if (request.Remote)
        {
            rawId |= ZlgCanNative.CanRtrFlag;
        }

        return rawId;
    }

    private static byte[] CopyToFixedLength(byte[] source, int length)
    {
        byte[] destination = new byte[length];
        Array.Copy(source, destination, source.Length);
        return destination;
    }

    private void EnsureDeviceOpen()
    {
        if (!IsDeviceOpen)
        {
            throw new InvalidOperationException("请先打开CAN设备。");
        }
    }

    private void EnsureChannelInitialized()
    {
        EnsureDeviceOpen();
        if (!IsChannelInitialized)
        {
            throw new InvalidOperationException("请先初始化CAN通道。");
        }
    }
}

public sealed record ZlgCanError(uint ErrorCode, byte[] PassiveErrorData, byte ArbitrationLostErrorData);
