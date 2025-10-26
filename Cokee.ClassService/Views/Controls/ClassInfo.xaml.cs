using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

using Cokee.ClassService.Helper;
using Cokee.ClassService.Shared;
using Cokee.ClassService.Views.Windows;

using Microsoft.Win32;

namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// StudentInfo.xaml 的交互逻辑
    /// </summary>
    ///
    public partial class ClassInfo : UserControl
    {
        public event EventHandler<Student>? EditStudent;

        public ClassInfo()
        {
            InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this)) return;
            this.Loaded += (a, b) =>
            {
                Application.Current.Windows.OfType<StudentMgr>().FirstOrDefault().AddStuEvent += AddStuEvent;
            };
            this.IsVisibleChanged += (a, b) => scrollviewer?.ScrollToHome();
            //this.Unloaded += (a, b) => Application.Current.Windows.OfType<StudentMgr>().FirstOrDefault().AddStuEvent -= AddStuEvent;
        }

        private void AddStuEvent(object? sender, bool e)
        {
            DataContext = new Student();
            Catalog.ToggleControlVisible(this, true);
        }

        private void Confirm(object sender, RoutedEventArgs e)
        {
            Catalog.ToggleControlVisible(this);
            if (DataContext != null) EditStudent?.Invoke(this, DataContext as Student);
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            this.DataContext = null;
            Catalog.ToggleControlVisible(this);
        }

       
    }
}