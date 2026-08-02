using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using System.Text;

namespace Usb.Events.Models.Linux;

[SupportedOSPlatform("linux")]
internal sealed class ProcMounts
{
    public async IAsyncEnumerable<MountEntry> ReadAsync()
    {
        await foreach (string line in File.ReadLinesAsync("/proc/mounts"))
        {
            string[] fields = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

            if (fields.Length < 6)
            {
                continue;
            }

            yield return new MountEntry(
                new DeviceNode(DecodeOctalEscapes(fields[0])),
                new MountPoint(DecodeOctalEscapes(fields[1])),
                fields[2],
                fields[3],
                int.TryParse(fields[4], out int dumpFrequency) ? dumpFrequency : 0,
                int.TryParse(fields[5], out int passNumber) ? passNumber : 0);
        }
    }

    private static string DecodeOctalEscapes(string value)
    {
        if (!value.Contains('\\'))
        {
            return value;
        }

        var builder = new StringBuilder(value.Length);

        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];

            if (c == '\\')
            {
                int result = 0;
                int count = 0;

                while (i + 1 < value.Length && count < 3)
                {
                    char digit = value[i + 1];

                    if (digit is < '0' or > '7')
                    {
                        break;
                    }

                    result = result * 8 + digit - '0';
                    count++;
                    i++;
                }

                if (count > 0)
                {
                    builder.Append((char)result);
                }
                else
                {
                    builder.Append(c);
                }
            }
            else
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }
}
