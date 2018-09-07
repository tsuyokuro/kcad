
/**
 * TODO Consoleの利用について
 * 
 * プロジェクト->KCadのプロパティー->アプリケーソンタブ
 * 出力の種類をコンソールアプリケーションにするとデバッグ実行時も
 * コンソールに出力されるようになる
 * コンソールの使用を止めるときは、出力の種類を Windowsアプリケーションもどすこと
 *
 * 
 **/

#define USE_CONSOLE
//#define USE_CONSOL_INPUT

using Plotter.Serializer;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

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

        TaskScheduler mMainThreadScheduler;

#if USE_CONSOLE
        public const bool UseConsole = true;
#else
        public const bool UseConsole = false;
#endif

        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += CurrentDomain_UnhandledException;

            System.Windows.Forms.Application.ThreadException += Application_ThreadException;

            // MessagePack for C# は、初回の実行が遅いので、起動時にダミーを実行して
            // 紛れさせる
            MpInitializer.Init();
        }

        private void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            HandleException(e.Exception);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleException(e.ExceptionObject);
        }

        // UI ThreadのException
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            HandleException(e.Exception);
        }

        // 別ThreadのException
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            new Task(() =>
            {
                HandleException(e.Exception);
            }
            ).Start(mMainThreadScheduler);
        }

        private void HandleException(object e)
        {
            //SaveData();
            if (!ShowExceptionDialg(e.ToString()))
            {
                Shutdown();
            }
        }

        private void SaveData()
        {
            //KCad.MainWindow mw = (KCad.MainWindow)this.MainWindow;
            //mw.EmergencySave(@".\EmergencySave.txt");
        }

        private bool ShowExceptionDialg(string text)
        {
            EceptionDialog dlg = new EceptionDialog();

            dlg.text.Text = text;
            bool? result = dlg.ShowDialog();

            if (result == null) return false;

            return result.Value;
        }

        [STAThread]
        override protected void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            mMainThreadScheduler = TaskScheduler.FromCurrentSynchronizationContext();


            OpenTK.Toolkit.Init();

#if USE_CONSOLE
            NativeMethods.AllocConsole();
            Console.WriteLine("App OnStartup USE_CONSOLE");
#endif

#if USE_CONSOL_INPUT
            InputThread = new DebugInputThread();
            InputThread.start();
#endif

            SplashWindow = new MySplashWindow();
            SplashWindow.Show();

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
