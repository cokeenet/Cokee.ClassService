using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

using Cokee.ClassService.Helper;

using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

using Serilog;
using Serilog.Events;
using Serilog.Sink.AppCenter;

using Wpf.Ui.Appearance;

namespace Cokee.ClassService
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            if (!Directory.Exists("D:\\")) Catalog.CONFIG_DISK = "C:\\";
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("log.txt",
               outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.AppCenterSink(null, LogEventLevel.Error, AppCenterTarget.ExceptionsAsCrashes, "3f56f1de-dc29-4a8f-9350-81820e32da71")
               .CreateLogger();
            AppCenter.Start("3f56f1de-dc29-4a8f-9350-81820e32da71",
                  typeof(Analytics), typeof(Crashes));
            base.OnStartup(e);
            Timeline.DesiredFrameRateProperty.OverrideMetadata(
                typeof(Timeline),
                new FrameworkPropertyMetadata { DefaultValue = 120 }
            );

            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            if (!Directory.Exists(Catalog.CONFIG_DIR))
            {
                Directory.CreateDirectory(Catalog.CONFIG_DIR);
            }
            Accent.ApplySystemAccent();
            var args = Environment.GetCommandLineArgs();
            if(args.Length > 0 )
            {
                
                if (args.Contains("-scrsave")) Catalog.isScrSave = true;
               // MessageBox.Show(Catalog.isScrSave.ToString());
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            if (ex == null) ex = new Exception("Null异常。");
            Log.Error(ex, "发生错误");
            //Crashes.TrackError(ex);
            Catalog.HandleException(ex, "未预期的异常! ");
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error(e.Exception, "发生错误");
            //Crashes.TrackError(e.Exception);
            Catalog.HandleException(e.Exception, "未预期的异常! ");
            e.Handled = true;
        }
    }
}