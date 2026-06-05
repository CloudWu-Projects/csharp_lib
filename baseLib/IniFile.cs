using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace csharp_lib.baseLib
{
    /// <summary>
    /// INI文件的操作类
    /// </summary>
    public class IniFile
    {
        private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

        public byte[] Path;
        private bool bWriteIni = false;
        private readonly object _fileLock = new object();

        bool isUtf8Bom(byte[] bytes)
        {
            return (bytes.Length >= 3) && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
        }
        byte[] RemoveUtf8Bom(byte[] bytes)
        {
            return bytes[3..];
        }
        void ConvertIniFileToUTF8(string filePath)
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);

            if (isUtf8Bom(fileBytes))
            {
                byte[] utf8Bytes = RemoveUtf8Bom(fileBytes);
                string utf8Str = Encoding.UTF8.GetString(utf8Bytes);
                File.WriteAllText(filePath, utf8Str, Utf8NoBom);
            }
        }
        string sPath = "";
        public IniFile()
        {
            sPath = $"{Helper.GetCurrentDirectory()}\\config.ini";
            this.Path = getBytes(sPath);
            bWriteIni = (!System.IO.File.Exists(sPath));
            if (System.IO.File.Exists(sPath))
            {
                ConvertIniFileToUTF8(sPath);
            }
        }

        /// <summary>
        /// 写INI文件
        /// </summary>
        /// <param name="section">段落</param>
        /// <param name="key">键</param>
        /// <param name="iValue">值</param>
        public void IniWriteValue(string section, string key, string iValue)
        {
            lock (_fileLock)
            {
                var lines = LoadIniLines();
                UpsertValue(lines, section, key, iValue ?? string.Empty);
                SaveIniLines(lines);
            }
        }

        private static byte[] getBytes(string s, string encodingName = "utf-8")
        {
            return null == s ? null : Encoding.GetEncoding(encodingName).GetBytes(s);
        }

        /// <summary>
        /// 读取INI文件
        /// </summary>
        /// <param name="section">段落</param>
        /// <param name="key">键</param>
        /// <returns>返回的键值</returns>
        public string IniReadValue(string section, string key, string defaultValue = "")
        {
            if (bWriteIni)
                IniWriteValue(section, key, defaultValue);

            lock (_fileLock)
            {
                return TryReadValue(section, key, out var value) ? value : defaultValue;
            }
        }

        private List<string> LoadIniLines()
        {
            if (!File.Exists(sPath))
                return new List<string>();

            return new List<string>(File.ReadAllLines(sPath, Encoding.UTF8));
        }

        private void SaveIniLines(List<string> lines)
        {
            File.WriteAllLines(sPath, lines, Utf8NoBom);
        }

        private bool TryReadValue(string section, string key, out string value)
        {
            value = string.Empty;
            if (!File.Exists(sPath))
                return false;

            string currentSection = string.Empty;
            foreach (var rawLine in File.ReadLines(sPath, Encoding.UTF8))
            {
                var line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith(";") || line.StartsWith("#"))
                    continue;

                if (TryParseSection(line, out var parsedSection))
                {
                    currentSection = parsedSection;
                    continue;
                }

                if (!string.Equals(currentSection, section, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (TryParseKeyValue(line, out var parsedKey, out var parsedValue) &&
                    string.Equals(parsedKey, key, StringComparison.OrdinalIgnoreCase))
                {
                    value = parsedValue;
                    return true;
                }
            }

            return false;
        }

        private static void UpsertValue(List<string> lines, string section, string key, string value)
        {
            int sectionStart = -1;
            int insertIndex = lines.Count;

            for (int i = 0; i < lines.Count; i++)
            {
                var trimmed = lines[i].Trim();
                if (!TryParseSection(trimmed, out var parsedSection))
                    continue;

                if (sectionStart >= 0)
                {
                    insertIndex = i;
                    break;
                }

                if (string.Equals(parsedSection, section, StringComparison.OrdinalIgnoreCase))
                {
                    sectionStart = i;
                    insertIndex = i + 1;
                }
            }

            if (sectionStart >= 0)
            {
                for (int i = sectionStart + 1; i < lines.Count; i++)
                {
                    var trimmed = lines[i].Trim();
                    if (TryParseSection(trimmed, out _))
                    {
                        insertIndex = i;
                        break;
                    }

                    if (TryParseKeyValue(trimmed, out var parsedKey, out _) &&
                        string.Equals(parsedKey, key, StringComparison.OrdinalIgnoreCase))
                    {
                        lines[i] = BuildKeyValueLine(key, value);
                        return;
                    }
                }

                lines.Insert(insertIndex, BuildKeyValueLine(key, value));
                return;
            }

            if (lines.Count > 0 && !string.IsNullOrWhiteSpace(lines[^1]))
                lines.Add(string.Empty);

            lines.Add($"[{section}]");
            lines.Add(BuildKeyValueLine(key, value));
        }

        private static bool TryParseSection(string line, out string section)
        {
            section = string.Empty;
            if (line.Length < 3 || line[0] != '[' || line[^1] != ']')
                return false;

            section = line[1..^1].Trim();
            return section.Length > 0;
        }

        private static bool TryParseKeyValue(string line, out string key, out string value)
        {
            key = string.Empty;
            value = string.Empty;

            int index = line.IndexOf('=');
            if (index <= 0)
                return false;

            key = line[..index].Trim();
            value = line[(index + 1)..].Trim();
            return key.Length > 0;
        }

        private static string BuildKeyValueLine(string key, string value)
        {
            return $"{key}={value}";
        }
    }
}
