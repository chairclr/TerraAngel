using System.Diagnostics;

namespace TerraAngel.Installer;

internal class SDKUtility
{
    public static List<string> GetDotnetSDKList()
    {
        try
        {
            List<string> outputLines = new List<string>();

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = "--list-sdks",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();

            string? line;
            while ((line = process.StandardOutput.ReadLine()) is not null)
            {
                outputLines.Add(line);
            }

            process.WaitForExit();

            return outputLines;
        }
        catch
        {
            return new List<string>();
        }
    }
}
