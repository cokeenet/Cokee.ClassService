using System;
using System.Collections;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Cokee.ClassService.Helper;


namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// 可触摸滚动的ScrollViewer控件
    /// </summary>
    public class ToggleableCard : Border
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource), typeof(IEnumerable), typeof(RandomResult), new PropertyMetadata(null));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public void Toggle()
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation(ActualWidth, 0, TimeSpan.FromSeconds(1))
                {
                    EasingFunction = Catalog.easingFunction
                };
            doubleAnimation.Completed += async (a, b) =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                DoubleAnimation doubleAnimation = new DoubleAnimation(0, ActualWidth, TimeSpan.FromSeconds(1));
                doubleAnimation.EasingFunction = Catalog.easingFunction;
                doubleAnimation.Completed += (a, b) => Visibility = Visibility.Collapsed;
                RenderTransform.BeginAnimation(TranslateTransform.XProperty, doubleAnimation);
            };
            //this.RenderTransform..BeginAnimation(TranslateTransform.XProperty, doubleAnimation);
        }
    }
}