using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace FaultCodeAscParser;

/// <summary>解析 Vector ASC 文本日志中常见的经典 CAN 报文格式。</summary>
public static partial class AscParser
{
    /*
     * 典型格式：0.123456 1 18FF50E5x Rx d 8 10 01 02 03 04 05 06 07
     * 仅处理标准 ASC 数据帧；注释、统计行、错误帧和不带数据的行会自动跳过。
     */
    [GeneratedRegex(@"^\s*(?<timestamp>\d+(?:\.\d+)?)\s+(?<channel>\d+)\s+(?<canid>[0-9A-Fa-f]+)(?<extended>x?)\s+(?<direction>Rx|Tx)\s+(?:\w+\s+)?d\s+(?<dlc>\d+)(?<bytes>(?:\s+[0-9A-Fa-f]{2})*)\s*$", RegexOptions.CultureInvariant)]
    private static partial Regex FrameRegex();

    /* Vector ASC 文件头的常见形式：date Wed Apr 30 03:40:56.191 pm 2014。 */
    [GeneratedRegex(@"^\s*date\s+(?<date>.+?)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex InitialTimeRegex();

    /// <summary>
    /// 异步读取 ASC 文件，按文件字节读取进度回调百分比，避免大文件读取期间界面无响应。
    /// 第一行 date 字段是整个文件的时间基准，后续报文的相对秒时间戳均在其基础上累加。
    /// </summary>
    public static async Task<AscParseResult> ParseAsync(string path, IProgress<int>? progress = null, CancellationToken cancellationToken = default)
    {
        var frames = new List<AscFrame>();
        var ignoredLineCount = 0;
        var lineNumber = 0;
        var lastProgress = -1;

        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 4096, FileOptions.SequentialScan);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true,
            bufferSize: 4096, leaveOpen: false);
        var fileLength = stream.Length;

        /*
         * 第一行必须是时间基准。先完成该行解析，后续每一条 CAN 报文均传入同一个基准时间。
         * 不再允许未获取基准时间时以相对时间显示，避免出现错误的报文时间。
         */
        var firstLine = await reader.ReadLineAsync(cancellationToken);
        if (firstLine is null)
            throw new InvalidDataException("ASC 文件为空，未找到第一行 date 时间基准。");

        lineNumber = 1;
        var initialTimeMatch = InitialTimeRegex().Match(firstLine.TrimStart('﻿'));
        if (!initialTimeMatch.Success || !TryParseInitialTime(initialTimeMatch.Groups["date"].Value.Trim(), out var initialTime))
            throw new InvalidDataException("ASC 第一行不是可识别的 date 时间基准，无法计算报文实际时间。");

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            cancellationToken.ThrowIfCancellationRequested();
            lineNumber++;
            ParseLine(line, lineNumber, initialTime, frames, ref ignoredLineCount);

            // StreamReader 存在内部缓冲，进度是近似值；读取完成后始终主动报告 100%。
            var currentProgress = fileLength == 0 ? 100 : (int)(stream.Position * 100L / fileLength);
            if (currentProgress > lastProgress)
            {
                lastProgress = currentProgress;
                progress?.Report(Math.Min(currentProgress, 99));
            }
        }

        progress?.Report(100);
        return new AscParseResult(frames, ignoredLineCount, initialTime);
    }

    /// <summary>解析一行 ASC 内容并将有效 CAN 数据帧加入结果列表。</summary>
    private static void ParseLine(string line, int lineNumber, DateTime initialTime, ICollection<AscFrame> frames,
        ref int ignoredLineCount)
    {
        var match = FrameRegex().Match(line);
        if (!match.Success)
        {
            ignoredLineCount++;
            return;
        }

        if (!double.TryParse(match.Groups["timestamp"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var timestamp)
            || !int.TryParse(match.Groups["channel"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var channel)
            || !uint.TryParse(match.Groups["canid"].Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var canId)
            || !int.TryParse(match.Groups["dlc"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var dlc))
        {
            ignoredLineCount++;
            return;
        }

        var byteTexts = match.Groups["bytes"].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var data = new List<byte>();
        foreach (var byteText in byteTexts)
        {
            if (byte.TryParse(byteText, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var value))
                data.Add(value);
        }

        // ASC 的 DLC 可能大于实际可用字节数，防止不完整行进入后续故障解码。
        if (data.Count < Math.Min(dlc, 8))
        {
            ignoredLineCount++;
            return;
        }

        /* 第四行及后续每条报文均在此处以第一行 date 为基准累加相对秒时间戳。 */
        var actualTimestamp = initialTime.AddSeconds(timestamp);
        frames.Add(new AscFrame(timestamp, channel, canId, match.Groups["extended"].Success,
            match.Groups["direction"].Value, data.Take(Math.Min(dlc, data.Count)).ToArray(), lineNumber, actualTimestamp));
    }

    /// <summary>解析 ASC 第一行 date 时间；以月份、日期、时分秒和年份为准，不依赖可能错误的星期字段。</summary>
    private static bool TryParseInitialTime(string text, out DateTime initialTime)
    {
        var enUs = CultureInfo.GetCultureInfo("en-US");
        var formats = new[]
        {
            "MMM dd hh:mm:ss tt yyyy",
            "MMM dd hh:mm:ss.fff tt yyyy",
            "MMM dd HH:mm:ss yyyy",
            "MMM dd HH:mm:ss.fff yyyy"
        };

        /*
         * 部分 ASC 文件的星期缩写可能与实际日期不一致，例如“Fri Aug 27 ... 2026”。
         * DateTime 会校验星期与日期的匹配关系而拒绝该文件，因此主动丢弃第一段星期字段。
         */
        var firstSpaceIndex = text.IndexOf(' ');
        var timeText = firstSpaceIndex > 0 ? text[(firstSpaceIndex + 1)..].TrimStart() : text;

        return DateTime.TryParseExact(timeText, formats, enUs, DateTimeStyles.AllowWhiteSpaces, out initialTime)
            || DateTime.TryParse(timeText, enUs, DateTimeStyles.AllowWhiteSpaces, out initialTime)
            || DateTime.TryParse(timeText, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out initialTime);
    }
}