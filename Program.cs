using System.CommandLine;
using WallpaperFetch.Github;
using Task = System.Threading.Tasks.Task;

namespace WallpaperFetch
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var setCommand = SetCommand.Create(SetWallpaperAsync);
            var cronCommand = CronCommand.Create(InstallCron, UninstallCron);

            var rootCommand = new RootCommand("Sets (random) wallpapers once or regularly using a cron job")
                { setCommand, cronCommand };

            await rootCommand.InvokeAsync(args);
        }

        private static async Task UninstallCron()
        {
            var cron = new Cron();
            var p = new Profile();
            await p.UpdateOrCreateProfile().ConfigureAwait(false);

            cron.Uninstall();
        }

        private static async Task InstallCron()
        {
            var cron = new Cron();
            var p = new Profile();
            await p.UpdateOrCreateProfile().ConfigureAwait(false);

            cron.Install(p.ApplicationFullPath);
        }

        public static async Task SetWallpaperAsync(string? sourceName, string? category, string? imageName,
            bool onlyOncePerDay)
        {
            var p = new Profile();
            await p.UpdateOrCreateProfile().ConfigureAwait(false);

            var settings = Settings.LoadFromFile(p.SettingsFileFullPath);
            var history = new History(p.HistoryFolderFullPath);

            if (onlyOncePerDay && history.HasEntryForToday())
            {
                await Console.Error.WriteLineAsync("The wallpaper has already been updated today. Skip execution...");
                return;
            }

            var source = settings.GetSourceByName(sourceName ?? settings.DefaultSourceName ??
                throw new Exception("The default source name is missing."));

            var handler = GetHandlerFromSource(source);

            var selectedCategory = category ??
                                   settings.DefaultCategory ?? throw new Exception("The default category is missing.");

            var imageData =
                await handler.GetImage(
                    source,
                    selectedCategory,
                    imageName);

            await history.SaveAsync(imageData);

            await Wallpaper.SetDesktopWallpaper(imageData, Wallpaper.Style.Fill);
        }

        private static IImageSourceHandler GetHandlerFromSource(ImageSource source)
        {
            return source.Type switch
            {
                GithubSource.DiscriminatorValue => new GithubImageSourceHandler(),
                _ => throw new Exception($"Unknown source '{source.Name}'")
            };
        }
    }
}