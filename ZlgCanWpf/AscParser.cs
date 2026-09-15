using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ZlgCanWpf;

/// <summary>解析 Vector ASC 文本日志中常见的经典 CAN 报文格式。</summary>
public static partial class AscParser
{
    /* 典型格式：0.123456 1 18FF50E5x Rx d 8 10 01 02 03 04 05 06 07。 */
    [GeneratedRegex(@"^\s*(?<timestamp>\d+(?:\.\d+)?)\s+(?<channel>\d+)\s+(?<canid>[0-9A-Fa-f]+)(?<extended>x?)\s+(?<direction>Rx|Tx)\s+(?:\w+\s+)?d\s+(?<dlc>\d+)(?<bytes>(?:\s+[0-9A-Fa-f]{2})*)\s*$", RegexOptions.CultureInvariant)]
    private static partial Regex FrameRegex();

    [GeneratedRegex(@"^\s*date\s+(?<date>.+?)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex InitialTimeRegex();

    /// <summary>
    /// 异步读取 ASC 文件。第一行 date 是实际时间基准；后续每帧相对秒时间戳均在此基础上累加。
    /// </summary>
    public static async Task<AscParseResult> ParseAsync(string path, IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var frames = new List<AscFrame>();
        int ignoredLineCount = 0;
        int lineNumber = 0;
        int lastProgress = -1;

        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 4096, FileOptions.SequentialScan);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true,
            bufferSize: 4096, leaveOpen: false);
        long fileLength = stream.Length;

        string? firstLine = await reader.ReadLineAsync(cancellationToken);
        if (firstLine is null)
        {
            throw new InvalidDataException("ASC 文件为空，未找到第一行 date 时间基准。");
        }

        lineNumber = 1;
        Match initialTimeMatch = InitialTimeRegex().Match(firstLine.TrimStart('﻿'));
        if (!initialTimeMatch.Success
            || !TryParseInitialTime(initialTimeMatch.Groups["date"].Value.Trim(), out DateTime initialTime))
        {
            throw new InvalidDataException("ASC 第一行不是可识别的 date 时间基准，无法计算报文实际时间。");
        }

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            cancellationToken.ThrowIfCancellationRequested();
            lineNumber++;
            ParseLine(line, lineNumber, initialTime, frames, ref ignoredLineCount);

            int currentProgress = fileLength == 0 ? 100 : (int)(stream.Position * 100L / fileLength);
            if (currentProgress > lastProgress)
            {
                lastProgress = currentProgress;
                progress?.Report(Math.Min(currentProgress, 99));
            }
        }

        progress?.Report(100);
        return new AscParseResult(frames, ignoredLineCount, initialTime);
    }

    private static void ParseLine(string line, int lineNumber, DateTime initialTime,
        ICollection<AscFrame> frames, ref int ignoredLineCount)
    {
        Match match = FrameRegex().Match(line);
        if (!match.Success
            || !double.TryParse(match.Groups["timestamp"].Value, NumberStyles.Float,
                CultureInfo.InvariantCulture, out double timestamp)
            || !int.TryParse(match.Groups["channel"].Value, NumberStyles.Integer,
                CultureInfo.InvariantCulture, out int channel)
            || !uint.TryParse(match.Groups["canid"].Value, NumberStyles.HexNumber,
                CultureInfo.InvariantCulture, out uint canId)
            || !int.TryParse(match.Groups["dlc"].Value, NumberStyles.Integer,
                CultureInfo.InvariantCulture, out int dlc))
        {
            ignoredLineCount++;
            return;
        }

        var data = new List<byte>();
        foreach (string byteText in match.Groups["bytes"].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (byte.TryParse(byteText, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte value))
            {
                data.Add(value);
            }
        }

        if (data.Count < Math.Min(dlc, 8))
        {
            ignoredLineCount++;
            return;
        }

        frames.Add(new AscFrame(timestamp, channel, canId, match.Groups["extended"].Success,
            match.Groups["direction"].Value, data.Take(Math.Min(dlc, data.Count)).ToArray(), lineNumber,
            initialTime.AddSeconds(timestamp)));
    }

    private static bool TryParseInitialTime(string text, out DateTime initialTime)
    {
        CultureInfo enUs = CultureInfo.GetCultureInfo("en-US");
        string[] formats =
        {
            "MMM dd hh:mm:ss tt yyyy", "MMM dd hh:mm:ss.fff tt yyyy",
            "MMM dd HH:mm:ss yyyy", "MMM dd HH:mm:ss.fff yyyy"
        };

        /* ASC 第一段星期缩写可能与日期不一致，主动丢弃以保证后续 date 能正确解析。 */
        int firstSpaceIndex = text.IndexOf(' ');
        string timeText = firstSpaceIndex > 0 ? text[(firstSpaceIndex + 1)..].TrimStart() : text;
        return DateTime.TryParseExact(timeText, formats, enUs, DateTimeStyles.AllowWhiteSpaces, out initialTime)
            || DateTime.TryParse(timeText, enUs, DateTimeStyles.AllowWhiteSpaces, out initialTime)
            || DateTime.TryParse(timeText, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces,
                out initialTime);
    }
}
