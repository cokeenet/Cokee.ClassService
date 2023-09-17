using System;
using System.Collections.Generic;
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
using Newtonsoft.Json;

namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// StudentInfo.xaml 的交互逻辑
    /// </summary>
    public partial class StudentInfo : UserControl
    {
        public StudentInfo()
        {
            InitializeComponent();
        }
        private void Confirm(object sender, RoutedEventArgs e)
        {
            
            this.Visibility = Visibility.Collapsed;
        }

        private void Cancel(object sender, RoutedEventArgs e) => this.Visibility = Visibility.Collapsed;

        private void sexCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void ToggleSwitch_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
