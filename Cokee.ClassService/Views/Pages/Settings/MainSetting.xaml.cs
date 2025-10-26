using Cokee.ClassService.Helper;
using iNKORE.UI.WPF.Modern.Controls;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Application = System.Windows.Forms.Application;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace Cokee.ClassService.Views.Pages
{
    /// <summary>
    /// Settings.xaml 的交互逻辑
    /// </summary>
    public partial class MainSetting : Page
    {
        public MainSetting()
        {
            try
            {
                InitializeComponent();

                IsVisibleChanged += (a, b) =>
                {
                    DataContext = Catalog.settings;
                    if (Win32Helper.GetAutoBootStatus())
                    {
                        AutoBootToggleSwitch.IsEnabled = false;
                        AutoBootToggleSwitch.IsOn = true;
                    }
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
            Catalog.settings.Save();
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
                    Catalog.ReleaseComObject(mainWindow.WordApplication, "Word");
                    mainWindow.WordApplication = null;

                    break;

                case "1":
                    Catalog.ReleaseComObject(mainWindow.ExcelApplication, "Excel");
                    mainWindow.ExcelApplication = null;
                    break;

                case "2":
                    Catalog.ReleaseComObject(mainWindow.PptApplication, "PPT");
                    mainWindow.PptApplication = null;
                    break;
            }
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                if (!toggleSwitch.IsOn) Win32Helper.DeleteAutoBootLnk();
                else Win32Helper.CreateAutoBoot();
            }
        }
    }
}