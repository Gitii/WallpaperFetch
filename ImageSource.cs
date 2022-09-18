using System.Text.Json.Serialization;
using Dahomey.Json.Attributes;

namespace WallpaperFetch;

class ImageSource
{
    [JsonPropertyName("$type")] public string? Type { get; set; }

    [JsonRequired(RequirementPolicy.Always)]
    public string Name { get; set; } = String.Empty;
}