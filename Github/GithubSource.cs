using Dahomey.Json.Attributes;

namespace WallpaperFetch.Github;

[JsonDiscriminator(DiscriminatorValue)]
class GithubSource : ImageSource
{
    public const string DiscriminatorValue = "github";

    public string RepositoryName { get; set; } = String.Empty;

    public string BasePath { get; set; } = String.Empty;

    public bool IgnoreCategory { get; set; } = false;
}
