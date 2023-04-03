using System.Net.Http.Json;

namespace WallpaperFetch.Github;

class GithubImageSourceHandler : IImageSourceHandler
{
    private const string USER_AGENT =
        "Mozilla/5.0 (Linux; Android 7.0; VFD 610) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.79 Mobile Safari/537.36";

    public Task<byte[]> GetImageAsync(ImageSource source, string category, string? imageName)
    {
        return GetImageAsync((GithubSource)source, category, imageName);
    }

    public async Task<byte[]> GetImageAsync(GithubSource source, string category, string? imageName)
    {
        var url =
            $"https://api.github.com/repos/{source.RepositoryName}/git/trees/master?recursive=1";

        var wallpapers = await GetWallpapersAsync(
                url,
                source.BasePath,
                source.IgnoreCategory ? null : category
            )
            .ConfigureAwait(false);

        var selectedWallpaper = string.IsNullOrWhiteSpace(imageName)
            ? wallpapers[Random.Shared.Next(wallpapers.Count)]
            : wallpapers.Single((w) => w.Name == imageName);

        return await GetBlobContentAsync(selectedWallpaper.BlobContent).ConfigureAwait(false);
    }

    static async Task<IReadOnlyList<WallpaperImage>> GetWallpapersAsync(
        string url,
        string basePath,
        string? category
    )
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);

        var response =
            await client.GetFromJsonAsync<Response>(url).ConfigureAwait(false)
            ?? new Response() { tree = Array.Empty<BlobReference>() };

        var namePrefix = string.IsNullOrWhiteSpace(category)
            ? $"{basePath}/"
            : $"{basePath}/{category}/";

        return response.tree
            .Where(
                (b) =>
                    b.type == "blob"
                    && b.path.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase)
            )
            .Select(
                (b) =>
                    new WallpaperImage()
                    {
                        BlobContent = new Uri(b.url),
                        Category = category ?? "",
                        Name = b.path.Substring(namePrefix.Length)
                    }
            )
            .ToList();
    }

    public static async Task<byte[]> GetBlobContentAsync(Uri blobUri)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);

        var response =
            await client.GetFromJsonAsync<Blob>(blobUri).ConfigureAwait(false)
            ?? throw new Exception("Failed to decode response");

        return Convert.FromBase64String(response.content);
    }

    private class Response
    {
        public string sha { get; set; } = String.Empty;
        public string url { get; set; } = String.Empty;
        public BlobReference[] tree { get; set; } = Array.Empty<BlobReference>();
        public bool truncated { get; set; } = false;
    }

    private class BlobReference
    {
        public string path { get; set; } = String.Empty;
        public string mode { get; set; } = String.Empty;
        public string type { get; set; } = String.Empty;
        public string sha { get; set; } = String.Empty;
        public int size { get; set; } = 0;
        public string url { get; set; } = String.Empty;
    }

    private class Blob
    {
        public string sha { get; set; } = String.Empty;
        public string node_id { get; set; } = String.Empty;
        public int size { get; set; } = 0;
        public string url { get; set; } = String.Empty;
        public string content { get; set; } = String.Empty;
        public string encoding { get; set; } = String.Empty;
    }

    private readonly struct WallpaperImage
    {
        public string Category { get; init; }
        public string Name { get; init; }
        public Uri BlobContent { get; init; }
    }
}
