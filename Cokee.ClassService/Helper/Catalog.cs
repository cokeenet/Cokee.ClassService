//using AutoUpdaterDotNET;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

using Cokee.ClassService.Shared;

using iNKORE.UI.WPF.Modern.Controls;
using Sentry;
using Serilog;

using static Vanara.PInvoke.ComCtl32;

namespace Cokee.ClassService.Helper
{
    public class Notification : INotifyPropertyChanged
    {
        private string? _title;
        private string? _content;
        private InfoBarSeverity _severity;
        private double _animX;

        public string? Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string? Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        public InfoBarSeverity Severity
        {
            get => _severity;
            set => SetProperty(ref _severity, value);
        }

        // 用于动画的属性，表示 InfoBar 的初始 X 位置
        public double AnimX
        {
            get => _animX;
            set => SetProperty(ref _animX, value);
        }

        // 实现 INotifyPropertyChanged 接口
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // 一个帮助方法，用于设置属性并触发 PropertyChanged 事件
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
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
        public static DesktopWindow? desktopWindow; 
        public static Version? Version = Assembly.GetExecutingAssembly().GetName().Version;

        public static async void HandleException(Exception ex, string str = "", bool isSlient = false)
        {
            await Application.Current.Dispatcher.InvokeAsync(async () =>
            {
                Log.Error(ex, "发生错误");
                SentrySdk.CaptureException(ex);
                string shortExpInfo = ex.ToString();
                if (shortExpInfo.Length >= 201) shortExpInfo = string.Concat(ex.ToString().Substring(0, 200), "...");
                if (MainWindow == null || isSlient) return;
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
                /*AutoUpdater.ShowSkipButton = false;
                AutoUpdater.ShowRemindLaterButton = true;
                AutoUpdater.RemindLaterAt = 15;
                AutoUpdater.RemindLaterTimeSpan = RemindLaterFormat.Minutes;
                AutoUpdater.Start("https://gitee.com/cokee/classservice/raw/master/class_update.xml");*/
            }
            catch
            {
            }
        }

        public static void MoveTo(Point point)
        {
        }
        public static void ShowInfo(string? title = "", string content = "  ", InfoBarSeverity severity = InfoBarSeverity.Informational)
        {
            if (MainWindow == null) return;
            // 创建新的 Notification 对象
            var notification = new Notification
            {
                Title = title,
                Content = content,
                Severity = severity,
                AnimX = 350 // 设置动画的起始位置
            };

            // 将新的 Notification 添加到 ItemsControl 的数据源中
            Application.Current.Dispatcher.Invoke(() =>
            {
                // 确保 infos 控件已经初始化
                if (MainWindow?.infos.ItemsSource is ObservableCollection<Notification> notifications)
                {
                    notifications.Add(notification);
                }
                else
                {
                    // 如果 infos 控件没有绑定到 ObservableCollection，初始化一个新的集合并设置为数据源
                    var newCollection = new ObservableCollection<Notification> { notification };
                    MainWindow.infos.ItemsSource = newCollection;
                }
            });

            // 触发动画效果
            // 这里假设您已经在 XAML 中定义了动画，并在代码后面绑定了动画的触发器
            // 您可能需要根据您的具体动画逻辑来调整这部分代码
            TriggerInfoBarAnimation(notification);
        }

        private static void TriggerInfoBarAnimation(Notification notification)
        {
            // 这里只是一个示例，您需要根据您的具体动画逻辑来实现
            // 例如，您可以使用 DataTriggers 或者 Storyboards 来实现动画效果
            // 以下代码假设您有一个方法来开始动画
            // BeginAnimation("infobarTran.X", newDoubleAnimation(...));
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
                else
                {
                    if (win.WindowState == WindowState.Minimized)
                        win.WindowState = WindowState.Normal;
                    win.Activate();
                    Catalog.ShowInfo("窗口已开启");
                }
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
                        FileSystemHelper.DirHelper.MakeExist($"{BACKUP_FILE_DIR}\\{DateTime.Now:yyyy-MM}");
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

        public static async void UpdateProgress(int progress, bool isvisible = true, TaskInfo? info = null)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                MainWindow.progress.Value = progress;
                MainWindow.progressStr.Text = $"ETA:{info?.ETA} Speed {info?.Speed} PerSec Rest {info?.RestFiles} files with {info?.TotalFiles} files.";
                MainWindow.tipsText.Text = $"logv{info?.Version}{info?.NowName}:{progress}%";
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

        public static async void ToggleControlVisible(UIElement uIElement, bool IsForceShow= false)
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                //if(uIElement.Effect == null)
                //{
                //    uIElement.Effect=new DropShadowEffect() { Opacity = 0.1 };
                //}
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
        public static bool IsOutsideOfScreen(FrameworkElement target)
        {
            var hwndSource = (HwndSource)PresentationSource.FromVisual(target);
            if (hwndSource is null)
            {
                return true;
            }

            var hWnd = hwndSource.Handle;
            var targetBounds = GetPixelBoundsToScreen(target);

            var screens = System.Windows.Forms.Screen.AllScreens;
            return !screens.Any(x => x.Bounds.IntersectsWith(targetBounds));

            System.Drawing.Rectangle GetPixelBoundsToScreen(FrameworkElement visual)
            {
                var pixelBoundsToScreen = Rect.Empty;
                pixelBoundsToScreen.Union(visual.PointToScreen(new Point(0, 0)));
                pixelBoundsToScreen.Union(visual.PointToScreen(new Point(visual.ActualWidth, 0)));
                pixelBoundsToScreen.Union(visual.PointToScreen(new Point(0, visual.ActualHeight)));
                pixelBoundsToScreen.Union(visual.PointToScreen(new Point(visual.ActualWidth, visual.ActualHeight)));
                return new System.Drawing.Rectangle(
                    (int)pixelBoundsToScreen.X, (int)pixelBoundsToScreen.Y,
                    (int)pixelBoundsToScreen.Width, (int)pixelBoundsToScreen.Height);
            }
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