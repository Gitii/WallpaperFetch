using System.CommandLine.Parsing;
using System.CommandLine;

namespace WallpaperFetch
{
    static class CronCommand
    {
        public static Command Create(Func<Task> installHandler, Func<Task> uninstallHandler)
        {
            var setCommand = new Command("cron", "Installs or uninstalls a cron job for regularly set the wallpaper");
            var installOption = new Option<bool>(new[] { "--install", "-i" },
                "Install a new or update an existing a cron job")
            {
                IsRequired = false,
            };
            var uninstallOption = new Option<bool>(new[] { "--uninstall", "-u" },
                "Uninstalls an existing cron job")
            {
                IsRequired = false,
            };

            setCommand.Add(installOption);
            setCommand.Add(uninstallOption);

            setCommand.SetHandler(Handler, installOption, uninstallOption);
            setCommand.AddValidator(Validate);
            return setCommand;


            Task Handler(bool install, bool uninstall)
            {
                if (install)
                {
                    return installHandler();
                }

                if (uninstall)
                {
                    return uninstallHandler();
                }

                throw new ArgumentException("either --install or --uninstall");
            }

            void Validate(CommandResult result)
            {
                if (result.GetValueForOption(installOption))
                {
                    return;
                }
                else if (result.GetValueForOption(uninstallOption))
                {
                    return;
                }
                else
                {
                    result.ErrorMessage = "Either use --install or --uninstall option";
                }
            }
        }
    }
}