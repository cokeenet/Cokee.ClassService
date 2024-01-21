using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Serilog;
using Wpf.Ui.Animations;
using Wpf.Ui.Common;
using Wpf.Ui.Mvvm.Services;
using MsExcel = Microsoft.Office.Interop.Excel;
using MsPpt = Microsoft.Office.Interop.PowerPoint;
using MsWord = Microsoft.Office.Interop.Word;

namespace Cokee.ClassService.Helper
{
    public static class Catalog
    {
        public static string CONFIG_DISK = @"D:\";
        public static string CONFIG_DIR = @$"{CONFIG_DISK}CokeeTech\CokeeClass";
        public static string BACKUP_FILE_DIR = @$"{CONFIG_DIR}\Files";
        public static string INK_DIR = @$"{CONFIG_DIR}\ink";
        public static string SCRSHOT_DIR = @$"{CONFIG_DIR}\ScreenShots";
        public static string SCHEDULE_FILE = @$"{CONFIG_DIR}\schedule.json";
        public static string STU_FILE = @$"{CONFIG_DIR}\students.json";
        public static string SETTINGS_FILE = @$"{CONFIG_DIR}\config.json";
        public static int WindowType;
        public static bool IsScrSave = false;
        public static IEasingFunction easingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut };

        // public static MainWindow mainWindow = App.Current.MainWindow as MainWindow;
        public static AppSettings settings = AppSettingsExtensions.LoadSettings();

        public static MainWindow? MainWindow = Application.Current.MainWindow as MainWindow;
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
                if (!Directory.Exists(CONFIG_DIR))
                {
                    Directory.CreateDirectory(CONFIG_DIR);
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
                   if (MainWindow.inkTool.isPPT && mainWindow.pptApplication != null && mainWindow.pptApplication.SlideShowWindows[1] != null) mainWindow.pptApplication.SlideShowWindows[1].View.Exit();
                   mainWindow.IconAnimation(true);
               }
           });
        }

        public static void ShowInfo(string? title = "", string? content = "",ControlAppearance appearance=ControlAppearance.Light,SymbolRegular symbol=SymbolRegular.Info28)
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                title ??= "";
                content ??= "";
                Log.Information($"Snack消息:{title} {content}");
                if (GlobalSnackbarService?.GetSnackbarControl() != null)
                {
                    await GlobalSnackbarService.ShowAsync(title, content, symbol,appearance);
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
                //else { win.Activate(); Catalog.ShowInfo("窗口在任务栏上!!", "就在底下"); }
            });
        }

        public static void BackupFile(string filePath, string fileName, bool isFullyDownloaded = true)
        {
            Task.Run(() =>
            {
                try
                {
                    ShowInfo("尝试备份文件。", $"{filePath}");
                    if (File.Exists(filePath) && isFullyDownloaded)
                    {
                        var a = new FileInfo(filePath);
                        if (!Directory.Exists($"{BACKUP_FILE_DIR}\\{DateTime.Now:yyyy-MM}")) Directory.CreateDirectory($"{BACKUP_FILE_DIR}\\{DateTime.Now:yyyy-MM}");
                        var backupPath = $"{BACKUP_FILE_DIR}\\{DateTime.Now:yyyy-MM}\\{fileName}";
                        if (File.Exists(backupPath) && new FileInfo(backupPath).Length != a.Length) backupPath = $"{BACKUP_FILE_DIR}\\{DateTime.Now:yyyy-MM}\\1_{fileName}";
                        a.CopyTo(backupPath, true);
                    }
                    else ShowInfo("文件不存在。");
                }
                catch (Exception ex)
                {
                    HandleException(ex, "FileCopyTask");
                }
            }).Start();
        }

        public static void ReleaseComObject(object? o, string type = "COM")
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ShowInfo($"尝试释放 {type} 对象");
                if(o==null)return;
                try { Marshal.FinalReleaseComObject(o); }
                catch(Exception ex) { HandleException(ex,"释放COM对象");}
                o = null;
            }, DispatcherPriority.Normal);
        }

        public static void SetWindowStyle(int type = 0)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                WindowType = type;
                if (MainWindow == null) return;
                if (type == 0)
                {
                    MainWindow.Width = SystemParameters.PrimaryScreenWidth;
                    MainWindow.Height = SystemParameters.PrimaryScreenHeight;
                    MainWindow.Top = 0;
                    MainWindow.Left = 0;
                }
                else
                {
                    MainWindow.Width = SystemParameters.WorkArea.Width;
                    MainWindow.Height = SystemParameters.WorkArea.Height;
                    MainWindow.Top = 0;
                    MainWindow.Left = 0;
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
                    var fadeOutAnimation = new DoubleAnimation
                    {
                        From = 1.0,
                        To = 0.0,
                        Duration = new Duration(TimeSpan.FromSeconds(0.2)) // 设置动画持续时间
                    };
                    // 创建一个淡出故事板，并将动画应用于控件的透明度属性
                    var fadeOutStoryboard = new Storyboard();
                    fadeOutStoryboard.Children.Add(fadeOutAnimation);
                    Storyboard.SetTarget(fadeOutAnimation, uIElement);
                    Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath(UIElement.OpacityProperty));

                    // 淡出动画完成时将控件设置为不可见
                    fadeOutStoryboard.Completed += (a,b) =>
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
            var random = new Random();
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