using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Animation;

using Wpf.Ui.Common;
using Wpf.Ui.Mvvm.Services;

namespace Cokee.ClassService
{
    public class Catalog
    {
        public const string CONFIG_DIR = @"D:\Program Files (x86)\CokeeTech\CokeeClass\";
        public const string INK_DIR = "D:\\Program Files (x86)\\CokeeTech\\CokeeClass\\ink";
        public const string SCHEDULE_FILE = @"D:\Program Files (x86)\CokeeTech\CokeeClass\schedule.json";
        public const string STU_FILE = "D:\\Program Files (x86)\\CokeeTech\\CokeeClass\\students.json";
        public static int WindowStyle = 0;
        public static SnackbarService GlobalSnackbarService { get; set; } = ((MainWindow)Application.Current.MainWindow).snackbarService;
        public static void HandleException(Exception ex, string str = "")
        {
            GlobalSnackbarService.Show($"{str}发生错误", ex.ToString().Substring(30) + "...", SymbolRegular.Warning32);
        }
        public static void ShowInfo(string title = "", string content = "")
        {
            GlobalSnackbarService.Show(title, content, SymbolRegular.Info12);
        }
        public static void RemoveObjFromWindow(UIElement element)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (element == null) return;
                MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow.MainGrid.Children.Remove(element);
            });
        }
        public static void SetWindowStyle(Window mainWindow, int type = 0)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                WindowStyle = type;
                if (mainWindow == null) return;
                if (type == 0)
                {
                    mainWindow.Width = SystemParameters.FullPrimaryScreenWidth;
                    mainWindow.Height = SystemParameters.FullPrimaryScreenHeight;
                    mainWindow.Top = SystemParameters.WorkArea.Top;
                    mainWindow.Left = SystemParameters.WorkArea.Left;
                }
                else
                {
                    mainWindow.Width = SystemParameters.WorkArea.Width;
                    mainWindow.Height = SystemParameters.WorkArea.Height;
                    mainWindow.Top = SystemParameters.WorkArea.Top;
                    mainWindow.Left = SystemParameters.WorkArea.Left;
                }
            });
        }
        public static void ToggleControlVisible(UIElement uIElement)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (uIElement.Visibility == Visibility.Collapsed)
                {
                    uIElement.Visibility = Visibility.Visible;
                    // 创建一个淡入动画
                    DoubleAnimation fadeInAnimation = new DoubleAnimation();
                    fadeInAnimation.From = 0.0;
                    fadeInAnimation.To = 1.0;
                    fadeInAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.2)); // 设置动画持续时间

                    // 创建一个淡入故事板，并将动画应用于控件的透明度属性
                    Storyboard fadeInStoryboard = new Storyboard();
                    fadeInStoryboard.Children.Add(fadeInAnimation);
                    Storyboard.SetTarget(fadeInAnimation, uIElement);
                    Storyboard.SetTargetProperty(fadeInAnimation, new PropertyPath(UIElement.OpacityProperty));

                    // 淡入动画完成时将控件设置为可见

                    // 启动淡入动画
                    fadeInStoryboard.Begin();
                }
                else
                {
                    // 创建一个淡出动画
                    DoubleAnimation fadeOutAnimation = new DoubleAnimation();
                    fadeOutAnimation.From = 1.0;
                    fadeOutAnimation.To = 0.0;
                    fadeOutAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.2)); // 设置动画持续时间
                                                                                         // 创建一个淡出故事板，并将动画应用于控件的透明度属性
                    Storyboard fadeOutStoryboard = new Storyboard();
                    fadeOutStoryboard.Children.Add(fadeOutAnimation);
                    Storyboard.SetTarget(fadeOutAnimation, uIElement);
                    Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath(UIElement.OpacityProperty));

                    // 淡出动画完成时将控件设置为不可见
                    fadeOutStoryboard.Completed += (sender, e) =>
                    {
                        uIElement.Visibility = Visibility.Collapsed;
                        uIElement.Opacity = 1.0;
                    };

                    // 启动淡出动画
                    fadeOutStoryboard.Begin();
                }
            });
        }
        public static List<T> RandomizeList<T>(List<T> list)
        {
            Random random = new Random();
            int n = list.Count;

            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
            return list;
        }
    }
}
