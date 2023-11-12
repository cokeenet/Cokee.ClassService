using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Cokee.ClassService.Helper;

namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// RandomControl.xaml 的交互逻辑
    /// </summary>
    public partial class RandomControl : UserControl
    {
        public int Number = 1, AllowMLang = 1, SexLimit = 1, AllowExist = 0, Easter = 0, tmc = 0;

        public event EventHandler<string> StartRandom;

        public RandomControl()
        {
            InitializeComponent();
            this.IsVisibleChanged += (a, b) =>
            {
                Easter = 0; title.FontSize = 18; tmc = 0;
                title.FontWeight = FontWeights.Normal;
            };
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

        private void CancelBtn(object sender, RoutedEventArgs e) => Catalog.ToggleControlVisible(this);

        private void numbox_TC(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                try
                {
                    var a = Convert.ToInt32(textBox.Text);
                    if (a >= 0) Number = a;
                }
                catch (Exception)
                {
                    textBox.Text = "1";
                    Number = 1;
                }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e) => AllowExist = 1;

        private void ComboBox_Selected(object sender, SelectionChangedEventArgs e)
        {
            var a = (ComboBox)sender;
            if (a.SelectedIndex != -1) SexLimit = a.SelectedIndex;
        }

        private void tm(object sender, TouchEventArgs e)
        {
            tmc++;
            if (tmc == 8) { tmc = 0; EasterEgg(); }
        }

        private void EasterEgg(object sender = null, MouseButtonEventArgs e = null)
        {
            //if (File.Exists(Catalog.CONFIG_DIR + $"\\eggs\\{DateTime.Now.ToString("yyyy-MM-dd")}")) { Easter = 0; return; }
            Easter++;
            if (Easter == 1)
            {
                title.FontWeight = FontWeights.Light;
            }
            else if (Easter == 2)
            {
                title.FontSize = 19;
            }
            else
            {
                title.FontSize = 18;
                title.FontWeight = FontWeights.Normal;
                Easter = 0;
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e) => AllowExist = 0;

        private void ConfirmBtn(object sender, RoutedEventArgs e)
        {
            if (Number <= 0) { Number = 0; numbox.Text = "0"; }
            Catalog.ToggleControlVisible(this);
            StartRandom?.Invoke(this, $"{Number}|{AllowMLang}|{SexLimit}|{AllowExist}|{Easter}");
            //if (Easter != 0) { File.Create(Catalog.CONFIG_DIR + $"\\eggs\\{DateTime.Now.ToString("yyyy-MM-dd")}");Easter = 0; }
        }

        private void MLang_UC(object sender, RoutedEventArgs e) => AllowMLang = 1;
    }
}