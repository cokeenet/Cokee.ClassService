using Cokee.ClassService.Helper;
using Cokee.ClassService.Views.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;

using Wpf.Ui.Appearance;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Clipboard = Wpf.Ui.Common.Clipboard;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;

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
            }
            catch (Exception ex)
            {
                Catalog.HandleException(ex);
            }
        }

        public void SaveData()
        {
            Catalog.ShowInfo("数据已保存.");
        }
        
    }
}
