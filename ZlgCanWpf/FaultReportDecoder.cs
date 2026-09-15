namespace ZlgCanWpf;

/// <summary>按固件 0x1F1C10xx～0x1F2010xx 与 data0=0x10～0x14 页协议解析故障报码。</summary>
public static class FaultReportDecoder
{
    /// <summary>判断扩展 CAN 帧的 ID、data0 页面及 DLC 是否符合固件故障报码协议。</summary>
    public static bool IsFaultReportFrame(AscFrame frame)
    {
        if (!frame.IsExtended || frame.Data.Length == 0)
        {
            return false;
        }

        byte page = frame.Data[0];
        int requiredLength = page switch
        {
            0x10 or 0x11 or 0x12 => 8,
            0x13 => 5,
            0x14 => 7,
            _ => int.MaxValue
        };
        if (frame.Data.Length < requiredLength)
        {
            return false;
        }

        /* xx 是故障报码来源电源地址；调试应答与周期上传都使用同一 data0 页面定义。 */
        uint psuAddress = frame.CanId & 0xFFU;
        if (psuAddress is < 0x80U or > 0x8BU)
        {
            return false;
        }

        const uint debugReplyBaseId = 0x1F1C1000U;
        uint debugReplyId = debugReplyBaseId | psuAddress;
        uint periodicUploadId = debugReplyId + ((uint)(page - 0x10) * 0x10000U);
        return frame.CanId == debugReplyId || frame.CanId == periodicUploadId;
    }

    /// <summary>从一帧有效故障报码解码所有实时、锁存和首次故障项。</summary>
    public static IReadOnlyList<FaultOccurrence> Decode(AscFrame frame)
    {
        if (frame.Data.Length == 0)
        {
            return Array.Empty<FaultOccurrence>();
        }

        return frame.Data[0] switch
        {
            0x10 when frame.Data.Length >= 8 => DecodeCharger(frame),
            0x11 when frame.Data.Length >= 8 => DecodeBitmap(frame, "电池", 1, 4, "实时"),
            0x12 when frame.Data.Length >= 8 => DecodeBitmap(frame, "电池", 1, 4, "锁存"),
            0x13 when frame.Data.Length >= 5 => DecodeBitmap(frame, "RS485握手", 1, 1, "实时")
                .Concat(DecodeBitmap(frame, "RS485握手", 2, 1, "锁存")).ToArray(),
            0x14 when frame.Data.Length >= 7 => DecodeBitmap(frame, "RS485周期查询", 1, 2, "实时")
                .Concat(DecodeBitmap(frame, "RS485周期查询", 3, 2, "锁存")).ToArray(),
            _ => Array.Empty<FaultOccurrence>()
        };
    }

    private static IReadOnlyList<FaultOccurrence> DecodeCharger(AscFrame frame)
    {
        var result = DecodeBitmap(frame, "充电器", 1, 3, "实时").ToList();
        result.AddRange(DecodeBitmap(frame, "充电器", 4, 2, "锁存"));

        /* 固件 firstCode = 0xDDNN：data7 为故障域，data6 为零基 bit index，bit0 合法。 */
        int firstBitIndex = frame.Data[6];
        string domain = FaultCatalog.GetDomain(frame.Data[7]);
        ushort firstCode = (ushort)(frame.Data[6] | (frame.Data[7] << 8));
        if (firstCode != 0)
        {
            int pdfNumber = firstBitIndex + 1;
            result.Add(new FaultOccurrence(frame.Timestamp, frame.Channel, frame.CanId, frame.Direction,
                0x10, domain, pdfNumber, FaultCatalog.GetName(domain, pdfNumber), "首次故障")
            {
                FirstFaultCodeText = $"0x{firstCode:X4}"
            });
        }

        return result;
    }

    private static IReadOnlyList<FaultOccurrence> DecodeBitmap(AscFrame frame, string domain,
        int dataOffset, int byteCount, string state)
    {
        var result = new List<FaultOccurrence>();
        for (int byteIndex = 0; byteIndex < byteCount; byteIndex++)
        {
            byte value = frame.Data[dataOffset + byteIndex];
            for (int bit = 0; bit < 8; bit++)
            {
                if ((value & (1 << bit)) == 0)
                {
                    continue;
                }

                int pdfNumber = byteIndex * 8 + bit + 1;
                result.Add(new FaultOccurrence(frame.Timestamp, frame.Channel, frame.CanId, frame.Direction,
                    frame.Data[0], domain, pdfNumber, FaultCatalog.GetName(domain, pdfNumber), state));
            }
        }

        return result;
    }
}
