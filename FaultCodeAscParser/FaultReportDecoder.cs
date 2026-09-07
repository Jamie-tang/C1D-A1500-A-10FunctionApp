namespace FaultCodeAscParser;

/// <summary>按固件定时主动上传及 Can_DebugDataAckSub() 的 0x10~0x14 页面协议解析故障报码。</summary>
public static class FaultReportDecoder
{
    /// <summary>从 ASC 帧解码所有置位的实时、锁存和首次故障码。</summary>
    public static IReadOnlyList<FaultOccurrence> Decode(AscFrame frame)
    {
        if (frame.Data.Length == 0)
            return Array.Empty<FaultOccurrence>();

        return frame.Data[0] switch
        {
            0x10 when frame.Data.Length >= 8 => DecodeCharger(frame),
            0x11 when frame.Data.Length >= 8 => DecodeBitmap(frame, "电池", 1, 4, "实时"),
            0x12 when frame.Data.Length >= 7 => DecodeBitmap(frame, "电池", 1, 4, "锁存"),
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

        // data6、data7 为小端首故障码：data6 是 PDF 项目编号，data7 是故障域号。
        var pdfNumber = frame.Data[6];
        var domain = FaultCatalog.GetDomain(frame.Data[7]);
        var firstCode = (ushort)(frame.Data[6] | (frame.Data[7] << 8));
        if (pdfNumber != 0)
        {
            result.Add(new FaultOccurrence(frame.Timestamp, frame.Channel, frame.CanId, frame.Direction,
                0x10, domain, pdfNumber, FaultCatalog.GetName(domain, pdfNumber), "首次故障")
            {
                // 保留原始 0xDDNN，便于与固件 FaultReport.u16FirstCode 直接对照。
                FirstFaultCodeText = $"0x{firstCode:X4}"
            });
        }

        return result;
    }

    /// <summary>将指定字节范围的置位 bit 转换为 PDF 编号；bit0 对应 PDF 编号1。</summary>
    private static IReadOnlyList<FaultOccurrence> DecodeBitmap(AscFrame frame, string domain, int dataOffset, int byteCount, string state)
    {
        var result = new List<FaultOccurrence>();
        for (var byteIndex = 0; byteIndex < byteCount; byteIndex++)
        {
            var value = frame.Data[dataOffset + byteIndex];
            for (var bit = 0; bit < 8; bit++)
            {
                if ((value & (1 << bit)) == 0)
                    continue;

                var pdfNumber = byteIndex * 8 + bit + 1;
                result.Add(new FaultOccurrence(frame.Timestamp, frame.Channel, frame.CanId, frame.Direction,
                    frame.Data[0], domain, pdfNumber, FaultCatalog.GetName(domain, pdfNumber), state));
            }
        }

        return result;
    }

}