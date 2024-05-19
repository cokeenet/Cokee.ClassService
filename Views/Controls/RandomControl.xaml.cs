using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Cokee.ClassService.Helper;
using Cokee.ClassService.Shared;

using iNKORE.UI.WPF.Modern.Controls;

namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// RandomControl.xaml 的交互逻辑
    /// </summary>
    public partial class RandomControl : UserControl
    {
        public RandomEventArgs randomArgs = new RandomEventArgs();
        public RandomResult? RandomResultControl { get; set; }
        // public event EventHandler<RandomEventArgs>? StartRandom;

        public RandomControl()
        {
            InitializeComponent();
            IsVisibleChanged += (a, b) =>
            {
                cf.Content = $"不重复(已抽{StudentExtensions.RandomHistory.Count}个)";
                title.FontSize = 18;
                title.FontWeight = FontWeights.Normal;
                stus.ItemsSource = StudentExtensions.RandomHistory;
            };
        }

        private void AddBtn(object sender, RoutedEventArgs e)
        {
            randomArgs.Count++;
            numbox.Text = randomArgs.Count.ToString();
        }

        private void SubBtn(object sender, RoutedEventArgs e)
        {
            if (randomArgs.Count <= 0) return;
            randomArgs.Count--;
            numbox.Text = randomArgs.Count.ToString();
        }

        private void MLang_C(object sender, RoutedEventArgs e) => randomArgs.AllowMLang = false;

        private void CancelBtn(object sender, RoutedEventArgs e) => Catalog.ToggleControlVisible(this);

        private void numbox_TC(NumberBox sender, NumberBoxValueChangedEventArgs e)
        {
            if (sender != null)
            {
                try
                {
                    var a = Convert.ToInt32(sender.Text);
                    if (a is >= 0 and <= 999) randomArgs.Count = a;
                }
                catch (Exception)
                {
                    sender.Text = "1";
                    randomArgs.Count = 1;
                }
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e) => randomArgs.AllowExist = false;

        private void ComboBox_Selected(object sender, SelectionChangedEventArgs e)
        {
            var a = (ComboBox)sender;
            if (a.SelectedIndex != -1) randomArgs.SexLimit = (SexCombo)a.SelectedIndex;
        }

        private int touchCount;

        private void LabelTouchMove(object sender, TouchEventArgs e)
        {
            touchCount++;
            if (touchCount == 8) { touchCount = 0; EasterEgg(); }
        }

        private void EasterEgg(object sender = null, MouseButtonEventArgs e = null)
        {
            //if (File.Exists(Catalog.CONFIG_DIR + $"\\eggs\\{DateTime.Now.ToString("yyyy-MM-dd")}")) { Easter = 0; return; }
            /*randomArgs.Easter++;
            if (randomArgs.Easter == Easter.Z)
            {
                title.FontWeight = FontWeights.Light;
            }
            else if (randomArgs.Easter == Easter.Y)
            {
                title.FontSize = 19;
            }
            else if (randomArgs.Easter == Easter.Me)
            {
                title.FontWeight = FontWeights.Bold;
            }
            else
            {
                title.FontSize = 18;
                title.FontWeight = FontWeights.Normal;
                randomArgs.Easter = Easter.None;
            }*/
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e) => randomArgs.AllowExist = true;

        private async void ConfirmBtn(object sender, RoutedEventArgs e)
        {
            Catalog.ToggleControlVisible(this);
            var a = await StudentExtensions.GetRandom(await StudentExtensions.Load(), randomArgs);
            if (RandomResultControl != null)
            {
                RandomResultControl.ItemsSource = a;
                Catalog.ToggleControlVisible(RandomResultControl, true);
            }
            stus.ItemsSource = StudentExtensions.RandomHistory;
            //StartRandom?.Invoke(this, randomArgs);
            //if (randomArgs.Easter != Easter.None) { File.Create(Catalog.CONFIG_DIR + $"\\eggs\\{DateTime.Now.ToString("yyyy-MM-dd")}");Easter = 0; }
        }

        private void MLang_UC(object sender, RoutedEventArgs e) => randomArgs.AllowMLang = true;

        private void Cf_OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            StudentExtensions.RandomHistory.Clear();
            stus.ItemsSource = StudentExtensions.RandomHistory;
            cf.Content = "不重复(已抽0个)";
            Catalog.ShowInfo("清除成功");
        }
    }
}