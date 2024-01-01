using Serilog;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;

using Wpf.Ui.Animations;
using Wpf.Ui.Common;
using Wpf.Ui.Mvvm.Services;

using Application = System.Windows.Application;
using MsExcel = Microsoft.Office.Interop.Excel;
using MsPpt = Microsoft.Office.Interop.PowerPoint;
using MsWord = Microsoft.Office.Interop.Word;

namespace Cokee.ClassService.Helper
{
    public class Catalog
    {
        public static string CONFIG_DISK = @$"D:\";
        public static string CONFIG_DIR = @$"{CONFIG_DISK}CokeeTech\CokeeClass";
        public static string BACKUP_FILE_DIR = @$"{CONFIG_DIR}\Files";
        public static string INK_DIR = @$"{CONFIG_DIR}\ink";
        public static string SCRSHOT_DIR = @$"{CONFIG_DIR}\ScreenShots";
        public static string SCHEDULE_FILE = @$"{CONFIG_DIR}\schedule.json";
        public static string STU_FILE = @$"{CONFIG_DIR}\students.json";
        public static string SETTINGS_FILE = @$"{CONFIG_DIR}\config.json";
        public static int WindowType = 0;
        public static bool isScrSave = false;
        public static IEasingFunction easingFunction = new CubicEase() { EasingMode = EasingMode.EaseInOut };

        // public static MainWindow mainWindow = App.Current.MainWindow as MainWindow;
        public static AppSettings settings = AppSettingsExtensions.LoadSettings();

        public static SnackbarService? GlobalSnackbarService;

        public static void HandleException(Exception ex, string str = "")
        {
            string shortExpInfo = ex.ToString();
            if (shortExpInfo.Length >= 201) shortExpInfo = string.Concat(ex.ToString().Substring(0, 200), "...");
            Application.Current.Dispatcher.Invoke(async () =>
            {
                if (GlobalSnackbarService != null) if (GlobalSnackbarService.GetSnackbarControl() != null)
                        await GlobalSnackbarService.ShowAsync($"{str}发生错误", shortExpInfo, SymbolRegular.Warning24, ControlAppearance.Danger);
            });
        }

        public static void UpdatePath(string disk = "D:\\")
        {
            if (Directory.Exists(disk))
            {
                CONFIG_DISK = disk;
                CONFIG_DIR = @$"{CONFIG_DISK}CokeeTech\CokeeClass";
                BACKUP_FILE_DIR = @$"{CONFIG_DIR}\Files";
                INK_DIR = @$"{CONFIG_DIR}\ink";
                SCRSHOT_DIR = @$"{CONFIG_DIR}\ScreenShots";
                SCHEDULE_FILE = @$"{CONFIG_DIR}\schedule.json";
                STU_FILE = @$"{CONFIG_DIR}\students.json";
                SETTINGS_FILE = @$"{CONFIG_DIR}\config.json";
                if (!Directory.Exists(Catalog.CONFIG_DIR))
                {
                    Directory.CreateDirectory(Catalog.CONFIG_DIR);
                }
            }
        }

        public static void ExitPPTShow()
        {
            Application.Current.Dispatcher.Invoke(() =>
           {
               MainWindow mainWindow = Application.Current.MainWindow as MainWindow;
               if (mainWindow != null)
               {
                   if (mainWindow.inkTool.isPPT && mainWindow.pptApplication != null && mainWindow.pptApplication.SlideShowWindows[1] != null) mainWindow.pptApplication.SlideShowWindows[1].View.Exit();
                   mainWindow.IconAnimation(true);
               }
           });
        }

        public static void ShowInfo(string? title = "", string? content = "")
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                if (title == null) title = "";
                if(content==null)content = "";
                if (GlobalSnackbarService != null) if (GlobalSnackbarService.GetSnackbarControl() != null)
                    {
                        Log.Information($"Snack消息:{title} {content}");
                        await GlobalSnackbarService.ShowAsync(title, content, SymbolRegular.Info28, ControlAppearance.Light);
                    }
            }, DispatcherPriority.Background);
        }

        public static void CreateWindow<T>(bool allowMulti = false) where T : Window, new()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var win = Application.Current.Windows.OfType<T>().FirstOrDefault();
                if (win == null || allowMulti)
                {
                    win = new T();
                    win.Show();
                }
                else { win.Activate(); Catalog.ShowInfo("窗口在任务栏上!!", "就在底下"); }
            });
        }

        public static void BackupFile(string filePath, string fileName, bool isFullyDownloaded = true)
        {
            new Thread(new ThreadStart(() =>
            {
                try
                {
                    Catalog.ShowInfo($"尝试备份文件。", $"{filePath}");
                    if (File.Exists(filePath) && isFullyDownloaded)
                    {
                        var a = new FileInfo(filePath);
                        if (!Directory.Exists($"{BACKUP_FILE_DIR}\\{DateTime.Now.ToString("yyyy-MM")}")) Directory.CreateDirectory($"{BACKUP_FILE_DIR}\\{DateTime.Now.ToString("yyyy-MM")}");
                        var backupPath = $"{BACKUP_FILE_DIR}\\{DateTime.Now.ToString("yyyy-MM")}\\{fileName}";
                        if (File.Exists(backupPath) && new FileInfo(backupPath).Length != a.Length) backupPath = $"{BACKUP_FILE_DIR}\\{DateTime.Now.ToString("yyyy-MM")}\\1_{fileName}";
                        a.CopyTo(backupPath, true);
                    }
                    else Catalog.ShowInfo($"文件不存在或未下载。");
                }
                catch (Exception ex)
                {
                    HandleException(ex, "FileCopy");
                }
            })).Start();
        }

        public static void ReleaseCOMObject(object o, string type = "ComObject")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Catalog.ShowInfo($"尝试释放 {type} 对象");
                try { Marshal.FinalReleaseComObject(o); }
                catch { }
                o = null;
            }, DispatcherPriority.Normal);
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

        public static void ToggleControlVisible(UIElement uIElement, bool IsForceShow = false)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (uIElement.Visibility == Visibility.Collapsed || IsForceShow)
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
                (list[n], list[k]) = (list[k], list[n]);
            }
            return list;
        }
    }
}