using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System;

using Wpf.Ui.Controls;

namespace Cokee.ClassService.Views.Pages
{
    /// <summary>
    /// QuickFix.xaml 的交互逻辑
    /// </summary
    public class TextToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 将文本转换为可见性
            if (value != null && !string.IsNullOrEmpty(value.ToString()))
            {
                return Visibility.Collapsed; // 隐藏占位文本
            }
            else
            {
                return Visibility.Visible; // 显示占位文本
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class LoginPage : UiPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void Confirm(object sender, System.Windows.RoutedEventArgs e)
        {
        }

        private void Cancel(object sender, System.Windows.RoutedEventArgs e)
        {
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
        }
    }
}