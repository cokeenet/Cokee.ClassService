using System;
using System.Diagnostics;
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
            base.OnStartup(e);
            try
            {
                if (!Directory.Exists("D:\\")) Catalog.UpdatePath("C:\\");
                if (!Directory.Exists(Catalog.CONFIG_DIR))
                {
                    Directory.CreateDirectory(Catalog.CONFIG_DIR);
                }
            }
            catch (Exception ex)
            {
                Catalog.HandleException(ex);
            }
            AppCenter.Start("3f56f1de-dc29-4a8f-9350-81820e32da71",
                  typeof(Analytics), typeof(Crashes));
            Timeline.DesiredFrameRateProperty.OverrideMetadata(
                typeof(Timeline),
                new FrameworkPropertyMetadata { DefaultValue = 120 }
            );
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            this.Exit += (a, b) =>
            {
               // Catalog.MainWindow.agent.Kill();
            };
            
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Accent.ApplySystemAccent();
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
            Log.Error(ex, "AppDomain异常");
            Catalog.HandleException(ex, "未捕获的异常! 尝试重启程序.");
            Process.Start(System.Windows.Forms.Application.ExecutablePath, "-m");
            Shutdown();
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error(e.Exception, "发生错误");
            Catalog.HandleException(e.Exception);

            e.Handled = true;
        }
    }
}