using System.Net.Http.Json;

namespace WallpaperFetch.Github
{
    class GithubImageSourceHandler : IImageSourceHandler
    {
        private const string USER_AGENT =
            "Mozilla/5.0 (Linux; Android 7.0; VFD 610) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.79 Mobile Safari/537.36";

        public Task<byte[]> GetImage(ImageSource source, string category, string? imageName)
        {
            return GetImage((GithubSource)source, category, imageName);
        }

        public async Task<byte[]> GetImage(GithubSource source, string category, string? imageName)
        {
            var url = $"https://api.github.com/repos/{source.RepositoryName}/git/trees/master?recursive=1";

            var wallpapers = await GetWallpapers(url, source.BasePath, category);

            var selectedWallpaper = string.IsNullOrWhiteSpace(imageName)
                ? wallpapers[Random.Shared.Next(wallpapers.Count)]
                : wallpapers.Single((w) => w.Name == imageName);

            return await GetBlobContent(selectedWallpaper.BlobContent);
        }

        static async Task<IReadOnlyList<WallpaperImage>> GetWallpapers(string url, string basePath, string category)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent",
                USER_AGENT);

            var response = await client.GetFromJsonAsync<Response>(url) ?? new Response()
            {
                tree = Array.Empty<BlobReference>()
            };

            var namePrefix = $"{basePath}/{category}/";

            return response.tree
                .Where((b) => b.type == "blob" &&
                              b.path.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase))
                .Select((b) => new WallpaperImage()
                {
                    BlobContent = new Uri(b.url),
                    Category = category,
                    Name = b.path.Substring(namePrefix.Length)
                })
                .ToList();
        }

        public static async Task<byte[]> GetBlobContent(Uri blobUri)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent",
                USER_AGENT);

            var response = await client.GetFromJsonAsync<Blob>(blobUri) ??
                           throw new Exception("Failed to decode response");

            return Convert.FromBase64String(response.content);
        }


        private class Response
        {
            public string sha { get; set; }
            public string url { get; set; }
            public BlobReference[] tree { get; set; }
            public bool truncated { get; set; }
        }

        private class BlobReference
        {
            public string path { get; set; }
            public string mode { get; set; }
            public string type { get; set; }
            public string sha { get; set; }
            public int size { get; set; }
            public string url { get; set; }
        }

        private class Blob
        {
            public string sha { get; set; }
            public string node_id { get; set; }
            public int size { get; set; }
            public string url { get; set; }
            public string content { get; set; }
            public string encoding { get; set; }
        }

        private readonly struct WallpaperImage
        {
            public string Category { get; init; }
            public string Name { get; init; }
            public Uri BlobContent { get; init; }
        }
    }
}