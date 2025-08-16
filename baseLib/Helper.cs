using System.Diagnostics;
using System.Text.RegularExpressions;

internal class Helper
{
    public static string GetCurrentDirectory()
    {

        var exePath = Process.GetCurrentProcess().MainModule.FileName;
        var exeFolder = exePath.Substring(0, exePath.LastIndexOf("\\"));

        return exeFolder;
    }
    public static string ExtractChineseBrand(string input)
    {
        // 匹配开头的连续汉字（不限于"牌"）
        Match match = Regex.Match(input, @"^([\u4e00-\u9fa5]+)");

        return match.Success ? match.Value : string.Empty;
    }
}
