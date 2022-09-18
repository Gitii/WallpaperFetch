namespace WallpaperFetch;

class Profile
{
    public string RootFolderFullPath { get; init; }

    public string HistoryFolderFullPath { get; init; }

    public string SettingsFileFullPath { get; init; }

    public string SettingsSchemaFileFullPath { get; init; }

    public string ApplicationFullPath { get; init; }

    public Profile()
    {
        var profileFolder = Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WallpaperFetch"
        );
        var historyFolder = Path.Join(profileFolder, "history");
        var profileApp = Path.Join(profileFolder, "application.exe");
        var settingsFile = Path.Join(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".wallpaper.json"
        );
        var schemaFile = Path.Join(profileFolder, "schema.json");

        RootFolderFullPath = profileFolder;
        SettingsFileFullPath = settingsFile;
        ApplicationFullPath = profileApp;
        SettingsSchemaFileFullPath = schemaFile;
        HistoryFolderFullPath = historyFolder;
    }

    public async Task UpdateOrCreateProfileAsync()
    {
        if (!Directory.Exists(RootFolderFullPath))
        {
            Directory.CreateDirectory(RootFolderFullPath);
        }

        if (!Directory.Exists(HistoryFolderFullPath))
        {
            Directory.CreateDirectory(HistoryFolderFullPath);
        }

        var isBundled = CheckIfBundled();

        if (isBundled)
        {
            var fileName =
                Environment.ProcessPath
                ?? throw new Exception("Unable to get the path to the original executable.");

            if (!fileName.Equals(ApplicationFullPath, StringComparison.OrdinalIgnoreCase))
            {
                File.Copy(fileName, ApplicationFullPath, true);
            }
        }
        else
        {
            await Console.Error
                .WriteLineAsync(
                    "The current executable hasn't been published. Skipping installation."
                )
                .ConfigureAwait(false);
        }

        Settings.SaveSchema(SettingsSchemaFileFullPath);

        if (!File.Exists(SettingsFileFullPath))
        {
            Settings.SaveScaffold(SettingsFileFullPath, SettingsSchemaFileFullPath);
        }
    }

    private bool CheckIfBundled()
    {
        var processFilePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
        if (processFilePath == null)
        {
            return true;
        }

#if DEBUG
        return false;
#else
        return true;
#endif
    }
}
