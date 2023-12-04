using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

using Cokee.ClassService.Helper;
using Cokee.ClassService.Views.Controls;

using Wpf.Ui.Controls;

namespace Cokee.ClassService.Views.Windows
{
    /// <summary>
    /// CourseMgr.xaml 的交互逻辑
    /// </summary>

    public partial class Sticky : UiWindow
    {
        public Sticky()
        {
            InitializeComponent();
            try
            {
                LoadSticky();
            }
            catch (Exception e)
            {
                
            }
        }

        public void LoadSticky()
        {
            List<StickyItem> list = new List<StickyItem>();
            var dir = new DirectoryInfo(Catalog.INK_DIR);
            foreach (FileInfo item in dir.GetFiles("*.ink"))
            {
                list.Add(new StickyItem(item.Name));
            }
            Sclview.Visibility = Visibility.Visible;
            Stickys.ItemsSource = list;
        }

        private void NavigationItem_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}