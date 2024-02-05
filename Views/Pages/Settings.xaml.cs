using System;
using System.Diagnostics;
using System.Windows;
using AutoUpdaterDotNET;
using Cokee.ClassService.Helper;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;
using Application = System.Windows.Forms.Application;

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
                micaInfo.IsOpen = !Wpf.Ui.Appearance.Background.IsSupported(BackgroundType.Mica);
                IsVisibleChanged += (a, b) =>
                {
                    DataContext = Catalog.settings;
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
            Catalog.settings.SaveSettings();
        }

        private void CheckUpdate(object sender, RoutedEventArgs e)
        {
            Catalog.CheckUpdate();
        }

        private void Restart(object sender, RoutedEventArgs e)
        {
            Process.Start(Application.ExecutablePath, "-m");
            System.Windows.Application.Current.Shutdown();
        }

        private void ReleaseObj(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = App.Current.MainWindow as MainWindow;
            switch (((Button)sender).Tag)
            {
                case "0":
                    Catalog.ReleaseComObject(mainWindow.wordApplication, "Word");
                    mainWindow.wordApplication = null;

                    break;

                case "1":
                    Catalog.ReleaseComObject(mainWindow.excelApplication, "Excel");
                    mainWindow.excelApplication = null;
                    break;

                case "2":
                    Catalog.ReleaseComObject(mainWindow.pptApplication, "PPT");
                    mainWindow.pptApplication = null;
                    break;
            }
        }
    }
}