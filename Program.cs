using System.CommandLine;
using WallpaperFetch.Github;
using Task = System.Threading.Tasks.Task;

namespace WallpaperFetch;

internal class Program
{
    static async Task Main(string[] args)
    {
        var setCommand = SetCommand.Create(SetWallpaperAsync);
        var cronCommand = CronCommand.Create(InstallCronAsync, UninstallCronAsync);

        var rootCommand = new RootCommand(
            "Sets (random) wallpapers once or regularly using a cron job"
        )
        {
            setCommand,
            cronCommand
        };

        await rootCommand.InvokeAsync(args).ConfigureAwait(false);
        return;
    }

    private static async Task UninstallCronAsync()
    {
        var cron = new Cron();
        var p = new Profile();
        await p.UpdateOrCreateProfileAsync().ConfigureAwait(false);

        cron.Uninstall();
    }

    private static async Task InstallCronAsync()
    {
        var cron = new Cron();
        var p = new Profile();
        await p.UpdateOrCreateProfileAsync().ConfigureAwait(false);

        cron.Install(p.ApplicationFullPath);
    }

    public static async Task SetWallpaperAsync(
        string? sourceName,
        string? category,
        string? imageName,
        bool onlyOncePerDay
    )
    {
        var p = new Profile();
        await p.UpdateOrCreateProfileAsync().ConfigureAwait(false);

        var settings = Settings.LoadFromFile(p.SettingsFileFullPath);
        var history = new History(p.HistoryFolderFullPath);

        if (onlyOncePerDay && history.HasEntryForToday())
        {
            await Console.Error
                .WriteLineAsync("The wallpaper has already been updated today. Skip execution...")
                .ConfigureAwait(false);
            return;
        }

        var source = settings.GetSourceByName(
            sourceName
                ?? settings.DefaultSourceName
                ?? throw new Exception("The default source name is missing.")
        );

        var handler = GetHandlerFromSource(source);

        var selectedCategory =
            category
            ?? settings.DefaultCategory
            ?? throw new Exception("The default category is missing.");

        var imageData = await handler
            .GetImageAsync(source, selectedCategory, imageName)
            .ConfigureAwait(false);

        history.Cleanup(settings.MaxHistoryLength);

        await history.SaveAsync(imageData).ConfigureAwait(false);

        await Wallpaper
            .SetLockScreenWallpaperAsync(imageData)
            .ConfigureAwait(false);

        await Wallpaper
            .SetDesktopWallpaperAsync(imageData, Wallpaper.Style.Fill)
            .ConfigureAwait(false);
    }

    private static IImageSourceHandler GetHandlerFromSource(ImageSource source)
    {
        return source.Type switch
        {
            GithubSource.DiscriminatorValue => new GithubImageSourceHandler(),
            _ => throw new Exception($"Unknown source '{source.Type}' ({source.Name})")
        };
    }
}
