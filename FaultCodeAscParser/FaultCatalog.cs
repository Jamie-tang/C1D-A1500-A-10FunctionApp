namespace FaultCodeAscParser;

/// <summary>保存固件 Fault.h 中定义的 PDF 故障编号和中文名称。</summary>
public static class FaultCatalog
{
    private static readonly string[] Battery =
    {
        "单体电压异常", "充电过流保护", "放电过流保护", "短路保护", "充电单体过温保护", "放电单体过温保护",
        "充电单体低温保护", "放电单体低温保护", "充电 MOS 故障", "放电 MOS 故障", "电池状态异常", "单体过压",
        "单体欠压", "总电压过压", "总电压欠压", "电池极限高温", "MOS 高温", "环境低温", "环境高温", "单体失效",
        "放电过流告警", "GSM 异常", "GPS 异常", "电池丢失", "热失控告警", "SOC 过低", "主动均衡异常", "加热膜功能异常",
        "充电机故障", "电池状态原始值", "写密钥失败", "电池 MOS 状态原始值"
    };

    private static readonly string[] Rs485Handshake =
    {
        "查询 ID 失败", "查询 R40 失败", "查询 R51~R52 失败", "查询 R53~R58 失败", "接收失败", "接收数据错误", "双口通信失败", "RS485 故障"
    };

    private static readonly string[] Rs485Query =
    {
        "查询 ID 失败", "查询 S0~S51 失败", "查询 S60~S90 失败", "查询 S91~S95 失败", "查询 R30 失败", "查询 R32~R33 失败",
        "查询 R41~R43 失败", "查询 R44~R46 失败", "查询 R47~R52 失败", "查询 R61~R62 失败", "查询 R80~R91 失败", "查询 S96~S125 失败",
        "接收失败", "接收数据错误", "双口通信失败", "RS485 故障"
    };

    private static readonly string[] Charger =
    {
        "输入电压正常标志异常", "输入欠压", "输入过压", "热点过温", "散热器低温", "散热器过温", "备用模块输入欠压", "门轴/门锁/消防/电池在柜有异常",
        "输出短路", "输出过压", "输出欠压", "输出过流", "风扇故障", "输出建立超时", "电池欠压", "电池过压", "充电超过6小时", "电池未在线"
    };

    /// <summary>按故障域和 PDF 编号返回中文故障名称。</summary>
    public static string GetName(string domain, int pdfNumber)
    {
        var source = domain switch
        {
            "电池" => Battery,
            "RS485握手" => Rs485Handshake,
            "RS485周期查询" => Rs485Query,
            "充电器" => Charger,
            _ => Array.Empty<string>()
        };

        return pdfNumber is > 0 && pdfNumber <= source.Length ? source[pdfNumber - 1] : "未定义故障编号";
    }

    /// <summary>由首故障码的高字节取得对应故障域。</summary>
    public static string GetDomain(byte domain) => domain switch
    {
        0x01 => "电池",
        0x02 => "RS485握手",
        0x03 => "RS485周期查询",
        0x04 => "充电器",
        _ => $"未知域(0x{domain:X2})"
    };
}