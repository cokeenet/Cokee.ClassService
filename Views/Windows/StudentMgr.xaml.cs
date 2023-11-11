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
        public event EventHandler<bool>? RandomEvent;

        public event EventHandler<bool>? AddMuitlStuEvent;

        public event EventHandler<bool>? AddStuEvent;

        public StudentMgr()
        {
            InitializeComponent();
        }

        private void AddMuitlStu(object sender, RoutedEventArgs e)
        {
            AddMuitlStuEvent?.Invoke(this, true);
        }

        private void Random(object sender, RoutedEventArgs e)
        {
            RandomEvent?.Invoke(this, true);
        }

        private void ClassSetting(object sender, RoutedEventArgs e)
        {
        }

        private void AddStu(object sender, RoutedEventArgs e)
        {
            AddStuEvent?.Invoke(this, true);
        }
    }
}