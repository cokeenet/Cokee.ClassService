using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Cokee.ClassService.Helper;
using Cokee.ClassService.Views.Pages;

using Microsoft.Win32;

using Newtonsoft.Json;

using Wpf.Ui.Controls;

using TextBox = Wpf.Ui.Controls.TextBox;

namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// StudentInfo.xaml 的交互逻辑
    /// </summary>
    public partial class StudentInfo : UserControl
    {
        public event EventHandler<Student> EditStudent;
        public StudentInfo()
        {
            InitializeComponent();
        }
        private void Confirm(object sender, RoutedEventArgs e)
        {
            
            this.Visibility = Visibility.Collapsed;
            if(DataContext!=null) EditStudent.Invoke(this, DataContext as Student);
        }

        private void Cancel(object sender, RoutedEventArgs e) => this.Visibility = Visibility.Collapsed;

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "图像文件|*.jpg;*.jpeg;*.png";
            openFileDialog.Title = "选择头像";
            openFileDialog.CheckFileExists = true; 
            openFileDialog.CheckPathExists=true;
            if((bool)openFileDialog.ShowDialog())
            {
                //if(File.Exists(openFileDialog.FileName))
            }
        }
    }
}
