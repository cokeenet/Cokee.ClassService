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
        bool isClosing = false;
        public CourseMgr()
        {
            InitializeComponent();
            this.Closing+=(a,b)=>isClosing=true; 
        }

        private void CoursesManage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false&&!isClosing) Close();
        }
    }
}