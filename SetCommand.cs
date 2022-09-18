using System.CommandLine;

namespace WallpaperFetch
{
    static class SetCommand
    {
        public static Command Create(Func<string?, string?, string?, bool, Task> handler)
        {
            var setCommand = new Command("set", "Sets a new wallpaper");
            var categoryOption =
                new Option<string>(new[] { "--category", "-c" }, "Set the category of the wallpaper")
                {
                    IsRequired = false
                };
            var nameOption = new Option<string>(new[] { "--name", "-n" },
                "The name of the wallpaper in selected category. If missing, a random image will be selected")
            {
                IsRequired = false,
            };
            var sourceOption =
                new Option<string>(new[] { "--source", "-s" },
                    "Set the source of the wallpaper image. This overrides the default source in the settings.")
                {
                    IsRequired = false
                };
            var skipOption =
                new Option<bool>(new[] { "--only-once-per-day", "--once" },
                    "Check if the wallpaper has already been updated today. If that's true, then do not update the wallpaper again.")
                {
                    IsRequired = false
                };

            setCommand.Add(categoryOption);
            setCommand.Add(nameOption);
            setCommand.Add(sourceOption);
            setCommand.Add(skipOption);

            setCommand.SetHandler(handler, sourceOption, categoryOption, nameOption, skipOption);
            return setCommand;
        }
    }
}