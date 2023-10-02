using System;
using System.Windows;
using Newtonsoft.Json;
using Wpf.Ui.Controls;

namespace Cokee.ClassService.Views.Windows
{
    /// <summary>
    /// CourseMgr.xaml 的交互逻辑
    /// </summary>


    public partial class CourseMgr : UiWindow
    {
        public CourseMgr()
        {
            InitializeComponent();
        }

        private void CoursesManage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false) Close();
        }
    }
}