using System.Text.Json;
using System.Text.Json.Serialization;
using Dahomey.Json;
using Dahomey.Json.Serialization.Conventions;
using Json.Schema;
using Json.Schema.Generation;
using WallpaperFetch.Github;

namespace WallpaperFetch;

class Settings
{
    [JsonPropertyName("$schema")]
    public string? SchemaPath { get; set; } = null;

    public List<ImageSource> Sources { get; set; } = new List<ImageSource>();

    public string? DefaultSourceName { get; set; }

    public string? DefaultCategory { get; set; }

    public int MaxHistoryLength { get; set; } = 5;

    public static Settings LoadFromFile(string path)
    {
        return JsonSerializer.Deserialize<Settings>(File.ReadAllText(path), GetSerializerSettings())
            ?? throw new Exception("Failed to deserialize settings");
    }

    private static JsonSerializerOptions GetSerializerSettings()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            MaxDepth = 0,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true,
        };
        options.SetupExtensions();

        DiscriminatorConventionRegistry registry = options.GetDiscriminatorConventionRegistry();
        registry.ClearConventions();
        registry.RegisterConvention(new DefaultDiscriminatorConvention<string>(options));
        registry.RegisterType<GithubSource>();
        return options;
    }

    public static void SaveSchema(string path)
    {
        var schema = new JsonSchemaBuilder().FromType<Settings>().Build();

        File.WriteAllText(path, JsonSerializer.Serialize(schema));
    }

    public static void SaveScaffold(string path, string? schemaFilePath)
    {
        var defaultSettings = new Settings
        {
            Sources = new List<ImageSource>
            {
                new GithubSource()
                {
                    RepositoryName = "makccr/wallpapers",
                    BasePath = "wallpapers",
                    Name = "makccr",
                    Type = GithubSource.DiscriminatorValue
                }
            },
            DefaultSourceName = "makccr",
            DefaultCategory = "space",
            SchemaPath = schemaFilePath
        };

        File.WriteAllText(path, JsonSerializer.Serialize(defaultSettings, GetSerializerSettings()));
    }

    public ImageSource GetSourceByName(string sourceName)
    {
        return Sources.FirstOrDefault((s) => s.Name == sourceName)
            ?? throw new Exception($"A source with name '{sourceName}' doesn't exist.");
    }
}
