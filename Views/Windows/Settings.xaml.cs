using System;
using System.Windows;
using Newtonsoft.Json;
using Wpf.Ui.Controls;

namespace Cokee.ClassService.Views.Windows
{
    /// <summary>
    /// CourseMgr.xaml 的交互逻辑
    /// </summary>


    public partial class Settings : UiWindow
    {
        public Settings()
        {
            InitializeComponent();
            this.Closing+=(a,b)=> { }; 
        }


    }
}