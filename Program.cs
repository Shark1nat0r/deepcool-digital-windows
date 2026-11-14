using System.Security.Principal;

namespace DeepCool_Display;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        if (args.Length == 0 || args[0] != "--scheduled")
        {
            if (!IsAdministrator())
            {
                MessageBox.Show("This program must be run as administrator to read CPU temperature, power and frequency.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
        }

        TrayApplicationContext context;
        try
        {
            context = new TrayApplicationContext();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "DeepCool Display", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        Application.Run(context);
    }

    private static bool IsAdministrator()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }
}