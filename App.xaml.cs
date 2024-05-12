using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Cokee.ClassService.Helper;
using Bugsnag;
using Serilog;

namespace Cokee.ClassService
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Client bugsnag = new Bugsnag.Client("dbeed1f3604f8067ee4a8c5c0c578ae3");

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
            bugsnag.SessionTracking.CreateSession();
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
                    if (Process.GetProcessesByName("Cokee.ClassService").Length >= 2) Shutdown();
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