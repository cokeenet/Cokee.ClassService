using AutoUpdaterDotNET;

using Cokee.ClassService.Helper;

using Serilog;

using System;
using System.Diagnostics;
using System.Windows;

using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

using Button = Wpf.Ui.Controls.Button;

namespace Cokee.ClassService.Views.Pages
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class Settings : UiPage
    {
        public Settings()
        {
            try
            {
                InitializeComponent();
                if (!Wpf.Ui.Appearance.Background.IsSupported(BackgroundType.Mica)) micaInfo.IsOpen = true;
                else micaInfo.IsOpen = false;
                this.IsVisibleChanged += (a, b) =>
                {
                    this.DataContext = Catalog.appSettings;

                    if (!(bool)b.NewValue) SaveData();
                };
            }
            catch (Exception ex)
            {
                Catalog.HandleException(ex);
            }
        }

        public void SaveData()
        {
            Catalog.appSettings.SaveSettings();
            Catalog.ShowInfo("数据已保存.");
        }

        private void CheckUpdate(object sender, RoutedEventArgs e)
        {
            AutoUpdater.ShowSkipButton = false;
            AutoUpdater.ShowRemindLaterButton = true;
            AutoUpdater.RemindLaterAt = 5;
            AutoUpdater.RemindLaterTimeSpan = RemindLaterFormat.Minutes;
            AutoUpdater.RunUpdateAsAdmin = false;
            AutoUpdater.Start("https://gitee.com/cokee/classservice/raw/master/class_update.xml");
        }

        private void Restart(object sender, RoutedEventArgs e)
        {
            Process.Start(System.Windows.Forms.Application.ExecutablePath, "-restart");
            Application.Current.Shutdown();
        }

        private void ReleaseObj(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = App.Current.MainWindow as MainWindow;
            switch (((Button)sender).Tag)
            {
                case "0":
                    mainWindow.wordApplication = null;
                    break;

                case "1":
                    mainWindow.excelApplication = null;
                    break;

                case "2":
                    mainWindow.pptApplication = null;
                    break;

                default:
                    break;
            }
        }
    }
}