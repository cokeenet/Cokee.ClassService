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
        public Student stu;
        public event EventHandler<Student> EditStudent;
        public StudentInfo()
        {
            InitializeComponent();
            //if(DataContext!=null&&DataContext is Student) stu = DataContext as Student;
        }
        private void Confirm(object sender, RoutedEventArgs e)
        {
            
            this.Visibility = Visibility.Collapsed;
        }

        private void Cancel(object sender, RoutedEventArgs e) => this.Visibility = Visibility.Collapsed;

        private void ComboBoxSelect(object sender, SelectionChangedEventArgs e)
        {
           /* ComboBox combo = sender as ComboBox;
            if(combo != null&&combo.SelectedIndex!=-1)
            switch (combo.Tag.ToString())
            {
                case "Sex":
                    stu.Sex = combo.SelectedIndex;
                    break;
                case "Role":
                    stu.Role = combo.SelectedIndex;
                    break;
            }*/
            
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox text = sender as TextBox;
          //  stu.RoleStr = text.Text;
        }

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

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DatePicker datePicker = sender as DatePicker;
            //if(datePicker.SelectedDate!=null) stu.BirthDay = (DateTime)datePicker.SelectedDate;
        }

        private void ToggleSwitch_Click(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            //stu.IsMinorLang = (bool)toggleSwitch.IsChecked;
        }
    }
}
