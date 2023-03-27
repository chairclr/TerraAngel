using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TerraAngel;

internal class UpdateChecker
{
    private static string GitHubVersionFile => "https://raw.githubusercontent.com/CEO-Chair/TerraAngel/master/TerraAngelPatches/VERSION";

    public static Version? NextUpdateVersion;

    public static async Task<bool> IsUpdateAvailableAsync()
    {
        if (ClientLoader.TerraAngelVersion is null)
        {
            return false;
        }

        using HttpClient httpClient = new HttpClient();

        try
        {
            NextUpdateVersion = Version.Parse(await httpClient.GetStringAsync(GitHubVersionFile));

            return true;
        }
        catch
        {
            return false;
        }
    }
}
