using System.Diagnostics;

internal class Helper
{
    public static string GetCurrentDirectory()
    {

        var exePath = Process.GetCurrentProcess().MainModule.FileName;
        var exeFolder = exePath.Substring(0, exePath.LastIndexOf("\\"));

        return exeFolder;
    }
}
