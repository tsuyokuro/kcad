
/**
 * TODO Consoleの利用について
 * 
 * プロジェクト->KCadのプロパティー->アプリケーソンタブ
 * 出力の種類をコンソールアプリケーションにするとデバッグ実行時も
 * コンソールに出力されるようになる
 * コンソールの使用を止めるときは、出力の種類を Windowsアプリケーションもどすこと
 *
 * 
 * Visual studio 2017 15.8.4では、Windowsアプリケーションのまま、普通にコンソールに出力される
 * Visual studio 2017 15.9.4では、またこの手順が必要になった
 **/

#define USE_CONSOLE
//#define USE_CONSOL_INPUT

using Plotter.Serializer;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace KCad
{
    public partial class App : Application
    {
        public DebugInputThread InputThread = null;

        public MySplashWindow SplashWindow = null;

        TaskScheduler mMainThreadScheduler;

#if USE_CONSOLE
        public const bool UseConsole = true;
#else
        public const bool UseConsole = false;
#endif

        public static App GetCurrent()
        {
            return (App)Current;
        }

        public static TaskScheduler MainThreadScheduler
        {
            get
            {
                return GetCurrent().mMainThreadScheduler;
            }
        }

        private static bool NowExceptionHandling = false;

        public App()
        {
            //CultureInfo ci = new CultureInfo("en-US");
            //Thread.CurrentThread.CurrentCulture = ci;
            //Thread.CurrentThread.CurrentUICulture = ci;

            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += CurrentDomain_UnhandledException;

            System.Windows.Forms.Application.ThreadException += Application_ThreadException;
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

        public static void ThrowException(object e)
        {
            new Task(() =>
            {
                GetCurrent().HandleException(e);
            }
            ).Start(GetCurrent().mMainThreadScheduler);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void HandleException(object e)
        {
            if (NowExceptionHandling)
            {
                return;
            }

            NowExceptionHandling = true;

            if (!ShowExceptionDialg(e.ToString()))
            {
                Shutdown();
            }

            NowExceptionHandling = false;
        }

        public static bool ShowExceptionDialg(string text)
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

            Stopwatch sw = new Stopwatch();
                       
            sw.Start();

            // MessagePack for C# は、初回の実行が遅いので、起動時にダミーを実行して
            // 紛れさせる
            MpInitializer.Init();

            MainWindow = new MainWindow();
            MainWindow.Show();

            sw.Stop();

            Console.WriteLine("MainWindow startup. Start up time: " + sw.ElapsedMilliseconds.ToString());

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
