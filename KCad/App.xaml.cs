using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace KCad
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        [STAThread]
        override protected void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            NativeMethods.AllocConsole();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            NativeMethods.FreeConsole();
            base.OnExit(e);
        }
    }

    internal static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool FreeConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool AllocConsole();
    }
}
