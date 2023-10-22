using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

using Wpf.Ui.Common;
using Wpf.Ui.Mvvm.Services;
using Wpf.Ui.Animations;
using System.Xml.Linq;

namespace Cokee.ClassService.Helper
{
    public class Catalog
    {
        public const string CONFIG_DISK = @$"D:\";
        public const string CONFIG_DIR = @$"{CONFIG_DISK}Program Files (x86)\CokeeTech\CokeeClass";
        public const string INK_DIR = @$"{CONFIG_DIR}\ink";
        public const string SCHEDULE_FILE = @$"{CONFIG_DIR}\schedule.json";
        public const string STU_FILE = @$"{CONFIG_DIR}\students.json";
        public const string SETTINGS_FILE_NAME = @$"{CONFIG_DIR}\config.json";
        public static int WindowType = 0;
       // public static MainWindow mainWindow = App.Current.MainWindow as MainWindow;
        public static AppSettings appSettings = AppSettingsExtensions.LoadSettings();
        public static SnackbarService GlobalSnackbarService { get; set; } = ((MainWindow)Application.Current.MainWindow).snackbarService;
        public static void HandleException(Exception ex, string str = "")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if(GlobalSnackbarService!=null)if(GlobalSnackbarService.GetSnackbarControl() != null)
                GlobalSnackbarService.Show($"{str}发生错误", string.Concat(ex.ToString().AsSpan(15), "..."), SymbolRegular.Warning32);
               // MessageBox.Show(ex.ToString());
            });
        }
        public static void ExitPPTShow()
        {
            Application.Current.Dispatcher.Invoke(() =>
           { 
                MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
               if(mainWindow!=null)
               if (mainWindow.inkTool.isPPT && mainWindow.pptApplication != null && mainWindow.pptApplication.SlideShowWindows[1] != null) mainWindow.pptApplication.SlideShowWindows[1].View.Exit();
           });
        }
        public static void ShowInfo(string title = "", string content = "")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (GlobalSnackbarService != null) if (GlobalSnackbarService.GetSnackbarControl() != null)
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
        public static void SetWindowStyle(int type = 0)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
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
