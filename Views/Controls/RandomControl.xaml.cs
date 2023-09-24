using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
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

namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// RandomControl.xaml 的交互逻辑
    /// </summary>
    public partial class RandomControl : UserControl
    {
        public int Number=1,AllowMLang=1,AllowGirl=1,AllowExist=0;
        public event EventHandler<string> StartRandom;
        public RandomControl()
        {
            InitializeComponent();
        }

        private void AddBtn(object sender, RoutedEventArgs e)
        {
            Number++;
            numbox.Text = Number.ToString();
        }

        private void SubBtn(object sender, RoutedEventArgs e)
        {
            if (Number <= 0) return;
            Number--;
            numbox.Text = Number.ToString();
        }

        private void MLang_C(object sender, RoutedEventArgs e) => AllowMLang = 0;

        private void Boy_C(object sender, RoutedEventArgs e) => AllowGirl = 0;

        private void Boy_UC(object sender, RoutedEventArgs e) => AllowGirl = 1;
        private void CancelBtn(object sender, RoutedEventArgs e) => this.Visibility = Visibility.Collapsed;

        private void numbox_TC(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null) 
            {
                var a = Convert.ToInt32(textBox.Text);
                if (a >= 0) Number = a;
            }

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e) => AllowExist = 1;

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e) => AllowExist = 0;

        private void ConfirmBtn(object sender, RoutedEventArgs e)
        {
            if (Number <= 0) { Number = 0;numbox.Text = "0"; }
            this.Visibility = Visibility.Collapsed;
            StartRandom?.Invoke(this, $"{Number}|{AllowMLang}|{AllowGirl}|{AllowExist}");
        }

        private void MLang_UC(object sender, RoutedEventArgs e) =>AllowMLang=1;
    }
}
