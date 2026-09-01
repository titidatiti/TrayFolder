using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using TrayFolder.Helpers;

namespace TrayFolder.ShellHost
{
    public partial class App : Application
    {
        private const uint SEM_NOGPFAULTERRORBOX = 0x0002;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SetErrorMode(SEM_NOGPFAULTERRORBOX);

            if (e.Args.Length != 3 ||
                !int.TryParse(e.Args[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out int x) ||
                !int.TryParse(e.Args[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int y))
            {
                Shutdown();
                return;
            }

            Window ownerWindow = new Window
            {
                Width = 1,
                Height = 1,
                Left = -32000,
                Top = -32000,
                Opacity = 0,
                ShowActivated = false,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None
            };

            ownerWindow.Show();
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    NativeContextMenuHelper.ShowContextMenu(e.Args[0], x, y, ownerWindow);
                }
                finally
                {
                    ownerWindow.Close();
                    Shutdown();
                }
            }), DispatcherPriority.Input);
        }

        [DllImport("kernel32.dll")]
        private static extern uint SetErrorMode(uint mode);
    }
}
