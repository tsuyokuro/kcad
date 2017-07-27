
/**
 * TODO
 * 
 * プロジェクト->KCadのプロパティー->アプリケーソンタブ
 * 出力の種類をコンソールアプリケーションにするとデバッグ実行時も
 * コンソールに出力されるようになる
 * コンソールの使用を止めるときは、出力の種類を Windowsアプリケーションもどすこと
 *
 * 
 **/

#define USE_CONSOLE


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
    public partial class App : Application
    {
        public DebugInputThread InputThread = null;

        public MySplashWindow SplashWindow = null;

        public static App GetCurrent()
        {
            return (App)Application.Current;
        }


        [STAThread]
        override protected void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

#if USE_CONSOLE
            NativeMethods.AllocConsole();

            InputThread = new DebugInputThread();
            InputThread.start();
#endif
            SplashWindow = new MySplashWindow();
            SplashWindow.Show();

            OpenTK.Toolkit.Init();

            this.MainWindow = new MainWindow();
            this.MainWindow.Show();

            SplashWindow.Close();
            SplashWindow = null;
        }

        protected override void OnExit(ExitEventArgs e)
        {
#if USE_CONSOLE
            NativeMethods.FreeConsole();
#endif
            base.OnExit(e);
        }
    }

#if USE_CONSOLE
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
#endif
}
