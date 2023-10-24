using Cokee.ClassService.Helper;
using System;

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
                this.DataContext = Catalog.appSettings;
                this.IsVisibleChanged += (a, b) => { SaveData(); };
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
