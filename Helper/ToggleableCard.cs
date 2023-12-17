using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using Wpf.Ui.Controls;
using System.Collections;
using System.Windows.Media.Animation;

namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// 可触摸滚动的ScrollViewer控件
    /// </summary>
    public class ToggleableCard : Card
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource), typeof(IEnumerable), typeof(RandomResult), new PropertyMetadata(null));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public ToggleableCard()
        {
        }

        public void Toggle()
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation(330, 0, TimeSpan.FromSeconds(1));
            doubleAnimation.EasingFunction = new CircleEase();
            doubleAnimation.Completed += async (a, b) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                DoubleAnimation doubleAnimation = new DoubleAnimation(0, this.ActualWidth, TimeSpan.FromSeconds(1));
                doubleAnimation.EasingFunction = new CircleEase();
                doubleAnimation.Completed += (a, b) => this.Visibility = Visibility.Collapsed;
                this.RenderTransform.BeginAnimation(TranslateTransform.XProperty, doubleAnimation);
            };
            //this.RenderTransform..BeginAnimation(TranslateTransform.XProperty, doubleAnimation);
        }
    }
}