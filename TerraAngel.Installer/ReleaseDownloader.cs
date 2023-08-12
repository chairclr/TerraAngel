using System.Formats.Tar;
using System.IO.Compression;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TerraAngel.Installer;

public class ReleaseDownloader
{
    private const string APILatest = "repos/chairclr/TerraAngel/releases/latest";

    public static async Task<ReleaseRoot> GetLatestRelease()
    {
        using HttpClient client = new HttpClient()
        {
            BaseAddress = new Uri("https://api.github.com/"),
        };

        client.DefaultRequestHeaders.Add("User-Agent", "TerraAngel-Installer");

        return (await client.GetFromJsonAsync<ReleaseRoot>(APILatest))!;
    }

    public static async Task<string> DownloadRelease(ReleaseRoot release, string targetDirectory)
    {
        using HttpClient client = new HttpClient();

        client.DefaultRequestHeaders.Add("User-Agent", "TerraAngel-Installer");

        using Stream stream = await client.GetStreamAsync(release.TarballUrl);
        using GZipStream gz = new GZipStream(stream, CompressionMode.Decompress);

        TarFile.ExtractToDirectory(gz, targetDirectory, true);

        return Directory.EnumerateDirectories(targetDirectory, "chairclr-TerraAngel-*", SearchOption.TopDirectoryOnly).Single();
    }

    public class ReleaseRoot
    {
        [JsonConstructor]
        public ReleaseRoot(
            string assetsUrl,
            int id,
            string tagName,
            string targetCommitish,
            string name,
            bool draft,
            bool prerelease,
            DateTime createdAt,
            DateTime publishedAt,
            string tarballUrl,
            string zipballUrl,
            string body
        )
        {
            AssetsUrl = assetsUrl;
            Id = id;
            TagName = tagName;
            TargetCommitish = targetCommitish;
            Name = name;
            Draft = draft;
            Prerelease = prerelease;
            CreatedAt = createdAt;
            PublishedAt = publishedAt;
            TarballUrl = tarballUrl;
            ZipballUrl = zipballUrl;
            Body = body;
        }

        [JsonPropertyName("assets_url")]
        public string AssetsUrl { get; }

        [JsonPropertyName("id")]
        public int Id { get; }

        [JsonPropertyName("tag_name")]
        public string TagName { get; }

        [JsonPropertyName("target_commitish")]
        public string TargetCommitish { get; }

        [JsonPropertyName("name")]
        public string Name { get; }

        [JsonPropertyName("draft")]
        public bool Draft { get; }

        [JsonPropertyName("prerelease")]
        public bool Prerelease { get; }

        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; }

        [JsonPropertyName("published_at")]
        public DateTime PublishedAt { get; }

        [JsonPropertyName("tarball_url")]
        public string TarballUrl { get; }

        [JsonPropertyName("zipball_url")]
        public string ZipballUrl { get; }

        [JsonPropertyName("body")]
        public string Body { get; }
    }
}
