using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Cokee.ClassService.Helper
{
    public static class AnimationHelper
    {
        public static void ApplyOptAnimation(Label ele, string text)
        {
            if (ele == null) return;
            DoubleAnimation anim1 = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.5));
            DoubleAnimation anim2 = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
            anim1.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
            anim2.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn };
            anim1.Completed += async (a, b) =>
            {
                //await Task.Delay(500);
                ele.Content = text;
                ele.BeginAnimation(Label.OpacityProperty, anim2);
            };
            ele.BeginAnimation(Label.OpacityProperty, anim1);
        }
        public static void ApplyOptAnimation(TextBlock ele, string text)
        {
            if (ele == null) return;
            DoubleAnimation anim1 = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.5));
            DoubleAnimation anim2 = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
            anim1.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
            anim2.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn };
            anim1.Completed += async (a, b) =>
            {
                //await Task.Delay(500);
                ele.Text = text;
                ele.BeginAnimation(TextBlock.OpacityProperty, anim2);
            };
            ele.BeginAnimation(TextBlock.OpacityProperty, anim1);
        }
        public static void ApplyOptAnimation(FrameworkElement ele)
        {
            if (ele == null) return;
            DoubleAnimation anim1 = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(0.5));
            DoubleAnimation anim2 = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.5));
            anim1.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
            anim2.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn };
            anim1.Completed += async (a, b) =>
            {
                
                ele.BeginAnimation(UIElement.OpacityProperty, anim2);
            };
            ele.BeginAnimation(UIElement.OpacityProperty, anim1);
        }
    }
}
