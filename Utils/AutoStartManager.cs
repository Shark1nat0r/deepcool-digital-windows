using System.Diagnostics;
using Microsoft.Win32.TaskScheduler;

namespace DeepCool_Display.Utils;

public static class AutoStartManager
{
    private const string TaskName = "DeepCool Display";
    internal const string ScheduledArgument = "--scheduled";

    public static void SetAutoStart(bool enable)
    {
        using var ts = new TaskService();
        if (enable)
        {
            var td = ts.NewTask();
            td.Principal.RunLevel = TaskRunLevel.Highest;
            td.Triggers.Add(new LogonTrigger());
            ApplyAppSettings(td);

            ts.RootFolder.RegisterTaskDefinition(TaskName, td);
        }
        else
        {
            ts.RootFolder.DeleteTask(TaskName, false);
        }
    }

    /// <summary>
    /// Returns whether autostart is enabled and, if so, updates the task in place when it no longer
    /// matches this executable or uses outdated settings, e.g. after the exe was moved or renamed.
    /// The task's principal and triggers are kept. A failed repair doesn't change the returned state.
    /// </summary>
    public static bool SyncTask()
    {
        using var ts = new TaskService();
        var task = ts.GetTask(TaskName);
        if (task is not { Enabled: true })
        {
            return false;
        }

        try
        {
            var td = task.Definition;
            var before = td.XmlText;
            ApplyAppSettings(td);
            if (td.XmlText != before)
            {
                task.RegisterChanges();
            }
        }
        catch
        {
            // The task is still enabled even if it couldn't be repaired
        }

        return true;
    }

    private static void ApplyAppSettings(TaskDefinition td)
    {
        td.Actions.Clear();
        td.Actions.Add(new ExecAction(Application.ExecutablePath, ScheduledArgument, Application.StartupPath));

        // The task is the app itself: it must start on battery, keep running indefinitely
        // and not run at the below-normal priority Task Scheduler uses by default
        td.Settings.DisallowStartIfOnBatteries = false;
        td.Settings.StopIfGoingOnBatteries = false;
        td.Settings.ExecutionTimeLimit = TimeSpan.Zero;
        td.Settings.Priority = ProcessPriorityClass.Normal;
    }
}
