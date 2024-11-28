using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Cokee.ClassService.Helper;
using Lierda.WPFHelper;
using Sentry;
using Sentry.Profiling;

namespace Cokee.ClassService
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        LierdaCracker cracker = new LierdaCracker();
        protected override void OnStartup(StartupEventArgs e)
        {
            //cracker.Cracker();
            base.OnStartup(e);
            try
            {
                if (!Directory.Exists("D:\\")) Catalog.UpdatePath("C:\\");
                FileSystemHelper.DirHelper.MakeExist(Catalog.CONFIG_DIR);
            }
            catch (Exception ex)
            {
                Catalog.HandleException(ex);
            }
            SentrySdk.Init(options =>
            {
                options.Dsn = "https://4a520052947bfc810435d96ee91ad2b9@o4507629156630528.ingest.us.sentry.io/4507629162725376";
                options.Debug = false;
                options.TracesSampleRate = 1.0;
                options.ProfilesSampleRate = 1.0; 
                options.AutoSessionTracking = true;
                options.AddIntegration(new ProfilingIntegration(
                    //TimeSpan.FromMilliseconds(500)
                ));
                options.IsGlobalModeEnabled = true;
                options.Distribution=App.Current.
            });
            Timeline.DesiredFrameRateProperty.OverrideMetadata(
                typeof(Timeline),
                new FrameworkPropertyMetadata { DefaultValue = 120 }
            );
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 0)
            {
                if (args.Contains("-scrsave")) Catalog.IsScrSave = true;
                else if (!args.Contains("-m"))
                {
                    //if (Process.GetProcessesByName("Cokee.ClassService").Length >= 3) Shutdown();
                }
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception? ex = e.ExceptionObject as Exception;
            if (ex == null) ex = new Exception("Null异常。");
            Catalog.HandleException(ex, "未捕获的异常! 尝试重启程序.");
            Process.Start(System.Windows.Forms.Application.ExecutablePath);
            Environment.Exit(ex.HResult);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Catalog.HandleException(e.Exception);
            e.Handled = true;
        }
    }
}