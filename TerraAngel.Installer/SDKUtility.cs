using System.Diagnostics;
using System.Text;

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

    public static void PublishProject(string projectPath, string outputDir)
    {
        StringBuilder builder = new StringBuilder();

        string logFile = Path.Combine(outputDir, $"build-log-{DateTime.UtcNow.Ticks}.txt");

        if (Directory.Exists(outputDir))
        {
            Directory.Delete(outputDir, true);
        }

        Directory.CreateDirectory(outputDir);

        try
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"publish {projectPath} --ucr --nologo -v d -o {outputDir} -p:PublishSingleFile=true",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();


            string? line;
            while ((line = process.StandardOutput.ReadLine()) is not null)
            {
                builder.AppendLine(line);
            }

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                Console.WriteLine("An error occured during build process");

                Console.WriteLine($"Open an issue on GitHub with the build-log.txt file (located at: {logFile})");
            }
        }
        catch (Exception ex)
        {

            Console.WriteLine("An error occured during build process:");
            Console.WriteLine(ex.ToString());
        }

        File.WriteAllText(logFile, builder.ToString());

    }
}
