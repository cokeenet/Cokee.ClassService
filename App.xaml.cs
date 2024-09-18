using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

using Cokee.ClassService.Helper;

using Sentry;
using Sentry.Profiling;

using Serilog;

namespace Cokee.ClassService
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            try
            {
                if (!Directory.Exists("D:\\")) Catalog.UpdatePath("C:\\");
                DirHelper.MakeExist(Catalog.CONFIG_DIR);
            }
            catch (Exception ex)
            {
                Catalog.HandleException(ex);
            }
            /*SentrySdk.Init(o =>
            {
                // Tells which project in Sentry to send events to:
                o.Dsn = "https://4a520052947bfc810435d96ee91ad2b9@o4507629156630528.ingest.us.sentry.io/4507629162725376";
                // When configuring for the first time, to see what the SDK is doing:
                o.Debug = false;
                // Set TracesSampleRate to 1.0 to capture 100% of transactions for performance monitoring.
                // We recommend adjusting this value in production.
                o.TracesSampleRate = 1.0;
                // Sample rate for profiling, applied on top of othe TracesSampleRate,
                // e.g. 0.2 means we want to profile 20 % of the captured transactions.
                // We recommend adjusting this value in production.
                o.ProfilesSampleRate = 1.0;
                // Requires NuGet package: Sentry.Profiling
                // Note: By default, the profiler is initialized asynchronously. This can
                // be tuned by passing a desired initialization timeout to the constructor.
                o.AddIntegration(new ProfilingIntegration(
                    // During startup, wait up to 500ms to profile the app startup code.
                    // This could make launching the app a bit slower so comment it out if you
                    // prefer profiling to start asynchronously
                    TimeSpan.FromMilliseconds(500)
                ));
            });*/
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
            // Process.Start(System.Windows.Forms.Application.ExecutablePath);
            Environment.Exit(ex.HResult);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Catalog.HandleException(e.Exception);
            e.Handled = true;
        }
    }
}