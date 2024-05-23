using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

using AutoUpdaterDotNET;

//using AutoUpdaterDotNET;

using Cokee.ClassService.Shared;

using iNKORE.UI.WPF.Modern.Controls;

using Serilog;

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
        public static string CLASSES_DIR = @$"{CONFIG_DIR}\Classes";
        public static string STU_FILE = @$"{CONFIG_DIR}\students.json";
        public static string SETTINGS_FILE = @$"{CONFIG_DIR}\config.json";
        public static int WindowType;
        public static bool IsScrSave = false;
        public static IEasingFunction easingFunction = new BackEase { EasingMode = EasingMode.EaseInOut };
        public static AppSettings settings = AppSettingsExtensions.LoadSettings();
        public static MainWindow? MainWindow = Application.Current.MainWindow as MainWindow;
        public static User? user = null;
        public static ApiClient apiClient = new ApiClient();

        public static async void HandleException(Exception ex, string str = "",bool isSlient=false)
        {
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                Log.Error(ex, "发生错误");
                App.bugsnag.Notify(ex);
                string shortExpInfo = ex.ToString();
                if (shortExpInfo.Length >= 201) shortExpInfo = string.Concat(ex.ToString().Substring(0, 200), "...");
                if (MainWindow == null||isSlient) return;
                ShowInfo($"{str}发生错误", shortExpInfo, InfoBarSeverity.Error);
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

        public static void TryLoginFromCache()
        {
            if (!string.IsNullOrEmpty(settings.LoginState))
            {
            }
        }

        public static void CheckUpdate()
        {
            try
            {
                AutoUpdater.ShowSkipButton = false;
                AutoUpdater.ShowRemindLaterButton = true;
                AutoUpdater.RemindLaterAt = 15;
                AutoUpdater.RemindLaterTimeSpan = RemindLaterFormat.Minutes;
                AutoUpdater.Start("https://gitee.com/cokee/classservice/raw/master/class_update.xml");
            }
            catch
            {
            }
        }

        public static async void ExitPPTShow()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
           {
               if (MainWindow != null)
               {
                   if (MainWindow.inkTool.isPPT && MainWindow.pptApplication != null && MainWindow.pptApplication.SlideShowWindows[1] != null) MainWindow.pptApplication.SlideShowWindows[1].View.Exit();
                   //mainWindow.IconAnimation(true);
               }
           });
        }

        public static async void ShowInfo(string? title = "", string content = "  ", InfoBarSeverity severity = InfoBarSeverity.Informational)
        {
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                Log.Information($"Snack消息:{title} {content}");
                if (MainWindow == null) return;
                MainWindow.infobar.Title = title;
                MainWindow.infobar.Message = content;
                MainWindow.infobar.Severity = severity;
                DoubleAnimation anim2 = new DoubleAnimation(0, MainWindow.infobar.ActualHeight + 200, TimeSpan.FromSeconds(1));
                DoubleAnimation anim1 = new DoubleAnimation(MainWindow.infobar.ActualHeight + 200, 0, TimeSpan.FromSeconds(1));
                anim2.Completed += (a, b) => MainWindow.infobar.IsOpen = false;
                anim1.EasingFunction = Catalog.easingFunction;
                anim2.EasingFunction = Catalog.easingFunction;
                MainWindow.infobar.IsOpen = true;
                MainWindow.infobarTran.BeginAnimation(TranslateTransform.YProperty, anim1);
                await Task.Delay(5000);
                MainWindow.infobar.IsOpen = true;
                MainWindow.infobarTran.BeginAnimation(TranslateTransform.YProperty, anim2);
            }, DispatcherPriority.Background);
        }

        public static async void CreateWindow<T>(bool allowMulti = false) where T : Window, new()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var win = Application.Current.Windows.OfType<T>().FirstOrDefault();
                if (win == null || allowMulti)
                {
                    win = new T();
                    win.Show();
                }
                else { win.Activate(); Catalog.ShowInfo("窗口已开启"); }
            });
        }

        public static void BackupFile(string filePath, string fileName, bool isFullyDownloaded = true)
        {
            new Thread(() =>
            {
                try
                {
                    ShowInfo("尝试备份文件。", $"{filePath}");
                    if (File.Exists(filePath) && isFullyDownloaded)
                    {
                        var a = new FileInfo(filePath);
                        DirHelper.MakeExist($"{BACKUP_FILE_DIR}\\{DateTime.Now:yyyy-MM}");
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

        public static async void UpdateProgress(int progress, bool isvisible = true, string? taskname = null)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MainWindow.progress.Value = progress;
                MainWindow.tipsText.Text = $"{taskname}:{progress}%";
                if (!isvisible || progress == 100)
                {
                    MainWindow.progress.IsActive = false;
                    MainWindow.tipsBorder.Visibility = Visibility.Collapsed;
                }
                else
                {
                    MainWindow.progress.IsActive = true;
                    MainWindow.tipsBorder.Visibility = Visibility.Visible;
                }
            }, DispatcherPriority.Background);
        }

        public static async void ReleaseComObject(object? o, string type = "COM")
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ShowInfo($"尝试释放 {type} 对象");
                if (o == null) return;
                try { Marshal.FinalReleaseComObject(o); }
                catch (Exception ex) { HandleException(ex, "释放COM对象"); }
                o = null;
            }, DispatcherPriority.Normal);
        }

        public static async void SetWindowStyle(int type = 0)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
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

        public static async void ToggleControlVisible(UIElement uIElement, bool IsForceShow = false)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (uIElement.Visibility == Visibility.Collapsed || IsForceShow)
                {
                    uIElement.Visibility = Visibility.Visible;
                    uIElement.Opacity = 0.0;
                    var fadeOutAnimation = new DoubleAnimation
                    {
                        From = 0.0,
                        To = 1.0,
                        Duration = new Duration(TimeSpan.FromSeconds(0.2)) // 设置动画持续时间
                    };
                    // 创建一个淡出故事板，并将动画应用于控件的透明度属性
                    var fadeItStoryboard = new Storyboard();
                    fadeItStoryboard.Children.Add(fadeOutAnimation);
                    Storyboard.SetTarget(fadeOutAnimation, uIElement);
                    Storyboard.SetTargetProperty(fadeOutAnimation, new PropertyPath(UIElement.OpacityProperty));
                    fadeItStoryboard.Completed += (a, b) =>
                    {
                        uIElement.Visibility = Visibility.Visible;
                        uIElement.Opacity = 1.0;
                    };

                    // 启动淡出动画
                    fadeItStoryboard.Begin();
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
                    fadeOutStoryboard.Completed += (a, b) =>
                    {
                        uIElement.Visibility = Visibility.Collapsed;
                        uIElement.Opacity = 1.0;
                    };

                    // 启动淡出动画
                    fadeOutStoryboard.Begin();
                }
            });
        }

        public static async Task<List<T>> RandomizeList<T>(List<T> list)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var random = new Random();
                int n = list.Count;
                while (n > 1)
                {
                    n--;
                    int k = random.Next(n + 1);
                    (list[n], list[k]) = (list[k], list[n]);
                }
            });
            return list;
        }
    }
}