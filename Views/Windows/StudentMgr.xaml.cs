using System;
using System.Windows;
using Newtonsoft.Json;
using Wpf.Ui.Controls;

namespace Cokee.ClassService.Views.Windows
{
    /// <summary>
    /// StudentMgr.xaml 的交互逻辑
    /// </summary>
    

    public partial class StudentMgr : UiWindow
    {
        public event EventHandler<bool> RandomEvent;
        public StudentMgr()
        {
            InitializeComponent();
        }

        private void ClassSetting(object sender, RoutedEventArgs e)
        {

        }

        private void Random(object sender, RoutedEventArgs e)
        {
            RandomEvent?.Invoke(this, true);
        }

        private void NavigationItem_Click_2(object sender, RoutedEventArgs e)
        {

        }
    }
}