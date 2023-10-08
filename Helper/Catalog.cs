using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

using Wpf.Ui.Common;
using Wpf.Ui.Mvvm.Services;
using Wpf.Ui.Animations;
namespace Cokee.ClassService.Helper
{
    public class Catalog
    {
        public const string CONFIG_DIR = @"D:\Program Files (x86)\CokeeTech\CokeeClass\";
        public const string INK_DIR = "D:\\Program Files (x86)\\CokeeTech\\CokeeClass\\ink";
        public const string SCHEDULE_FILE = @"D:\Program Files (x86)\CokeeTech\CokeeClass\schedule.json";
        public const string STU_FILE = "D:\\Program Files (x86)\\CokeeTech\\CokeeClass\\students.json";
        public static int WindowType = 0;
        public static SnackbarService GlobalSnackbarService { get; set; } = ((MainWindow)Application.Current.MainWindow).snackbarService;
        public static void HandleException(Exception ex, string str = "")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                GlobalSnackbarService.Show($"{str}发生错误", ex.ToString().Substring(30) + "...", SymbolRegular.Warning32);
            });
        }
        public static void ShowInfo(string title = "", string content = "")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                GlobalSnackbarService.Show(title, content, SymbolRegular.Info12);
            });
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
                Catalog.WindowType = type;
                if (mainWindow == null) return;
                if (type == 0)
                {
                    mainWindow.Width = SystemParameters.PrimaryScreenWidth;
                    mainWindow.Height = SystemParameters.PrimaryScreenHeight;
                    mainWindow.Top = 0;
                    mainWindow.Left = 0;
                }
                else
                {
                    mainWindow.Width = SystemParameters.WorkArea.Width;
                    mainWindow.Height = SystemParameters.WorkArea.Height;
                    mainWindow.Top = 0;
                    mainWindow.Left = 0;
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
                    Transitions.ApplyTransition(uIElement, TransitionType.FadeInWithSlide, 200);
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
