using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

using Cokee.ClassService.Helper;
using Cokee.ClassService.Views.Controls;

namespace Cokee.ClassService.Views.Windows
{
    /// <summary>
    /// CourseMgr.xaml 的交互逻辑
    /// </summary>

    public partial class Sticky : Window
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
            Dispatcher.Invoke(new Action(() =>
            {
                List<StickyItem> list = new List<StickyItem>();
                var dir = new DirectoryInfo(Catalog.INK_DIR);
                var a = dir.GetFiles("*.ink");
                count.Content = $"共{a.Length}人";
                foreach (FileInfo item in a)
                {
                    list.Add(new StickyItem(item.Name));
                }
                Sclview.Visibility = Visibility.Visible;
                Stickys.ItemsSource = list;
            }));
        }

        private void NavigationItem_Click(object sender, RoutedEventArgs e)
        {
            Catalog.ToggleControlVisible(postNote);
        }
    }
}