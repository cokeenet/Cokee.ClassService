using Cokee.ClassService.Helper;
using System;
using System.Windows;

using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

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
                
                this.IsVisibleChanged += (a, b) => {
                    this.DataContext = Catalog.appSettings;
                    if (!(bool)b.NewValue)SaveData();
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

    }
}
