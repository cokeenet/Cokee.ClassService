using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.AppCenter;
using Serilog.Sink.AppCenter;
using Serilog;
using System.Windows.Threading;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace Cokee.ClassService
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("log.txt",
               outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.AppCenterSink(null, Serilog.Events.LogEventLevel.Information, AppCenterTarget.ExceptionsAsCrashes, "3f56f1de-dc29-4a8f-9350-81820e32da71")
                .CreateLogger();
            AppCenter.Start("3f56f1de-dc29-4a8f-9350-81820e32da71",
                  typeof(Analytics), typeof(Crashes));
            base.OnStartup(e);
            this.DispatcherUnhandledException += App_DispatcherUnhandledException; ;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error(e.Exception,"发生错误");
            Crashes.TrackError(e.Exception);
            MessageBox.Show(e.Exception.ToString(), "Bug Tracked! 发生错误");
            e.Handled = true;
        }
    }
}
