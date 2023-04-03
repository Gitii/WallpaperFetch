using Microsoft.Win32.TaskScheduler;

namespace WallpaperFetch;

class Cron
{
    private const string CRON_NAME = "WallpaperFetch";

    public void Uninstall()
    {
        TaskService.Instance.RootFolder.DeleteTask(CRON_NAME, false);
    }

    public void Install(string appPath)
    {
        Uninstall();

        TaskDefinition td = TaskService.Instance.NewTask();
        td.RegistrationInfo.Description = "Regularly set a new wallpaper";

        td.Triggers.Add(new DailyTrigger() { StartBoundary = DateTime.Today.AddHours(9) });
        td.Triggers.Add(
            new LogonTrigger()
            {
                UserId = System.Security.Principal.WindowsIdentity.GetCurrent().Name
            }
        );

        // Create an action that will launch Notepad whenever the trigger fires
        td.Actions.Add(appPath, "set --only-once-per-day");

        // Register the task in the root folder of the local machine
        TaskService.Instance.RootFolder.RegisterTaskDefinition(CRON_NAME, td);
    }
}
