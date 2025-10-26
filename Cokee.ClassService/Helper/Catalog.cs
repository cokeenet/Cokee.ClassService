using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
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

using WpfScreenHelper;

using static Vanara.PInvoke.ComCtl32;

using Point = System.Windows.Point;

namespace Cokee.ClassService.Helper
{
    #region 通知数据模型（实现属性变更通知）

    /// <summary>
    /// 通知数据模型，用于UI层显示提示/错误信息
    /// </summary>
    public class Notification : INotifyPropertyChanged
    {
        private string? _title;
        private string? _content;
        private InfoBarSeverity _severity;
        private double _animX;

        /// <summary>
        /// 通知标题
        /// </summary>
        public string? Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        /// <summary>
        /// 通知内容
        /// </summary>
        public string? Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        /// <summary>
        /// 通知类型（Info/Error/Success/Warning，影响UI样式）
        /// </summary>
        public InfoBarSeverity Severity
        {
            get => _severity;
            set => SetProperty(ref _severity, value);
        }

        /// <summary>
        /// 动画X轴位置（用于通知弹出/收回动画）
        /// </summary>
        public double AnimX
        {
            get => _animX;
            set => SetProperty(ref _animX, value);
        }

        #region INotifyPropertyChanged 实现

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 触发属性变更事件
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 设置属性值并触发变更事件（通用方法）
        /// </summary>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }

    #endregion

    #region 全局工具类（应用级通用功能）

    /// <summary>
    /// 全局工具类，封装路径配置、窗口管理、异常处理、文件操作等通用功能
    /// </summary>
    public static class Catalog
    {
        #region 路径配置（静态常量+动态更新）

        // 默认基础路径（D盘），可通过UpdatePath切换
        public static string CONFIG_DISK = @"D:\";
        public static string CONFIG_DIR = $@"{CONFIG_DISK}CokeeTech\CokeeClass";
        public static string BACKUP_FILE_DIR = $@"{CONFIG_DIR}\Files";
        public static string INK_DIR = $@"{CONFIG_DIR}\ink";
        public static string SCRSHOT_DIR = $@"{CONFIG_DIR}\ScreenShots";
        public static string SCHEDULE_FILE = $@"{CONFIG_DIR}\schedule.json";
        public static string CLASSES_DIR = $@"{CONFIG_DIR}\Classes";
        public static string STU_FILE = $@"{CONFIG_DIR}\students.json";
        public static string SETTINGS_FILE = $@"{CONFIG_DIR}\config.json";

        #endregion

        #region 全局状态与配置

        /// <summary>
        /// 窗口样式类型（0：全屏，1：工作区大小）
        /// </summary>
        public static int WindowType;

        /// <summary>
        /// 是否为屏保模式
        /// </summary>
        public static bool IsScrSave = false;

        /// <summary>
        /// 全局动画缓动函数（统一动画效果）
        /// </summary>
        public static IEasingFunction easingFunction = new BackEase { EasingMode = EasingMode.EaseInOut };

        /// <summary>
        /// 应用配置（从本地文件加载）
        /// </summary>
        public static AppSettings settings = AppSettingsExtensions.LoadSettings();

        /// <summary>
        /// 主窗口实例（懒加载，避免启动时UI未初始化的问题）
        /// </summary>
        public static MainWindow? MainWindow => Application.Current?.MainWindow as MainWindow;

        /// <summary>
        /// 当前登录用户信息
        /// </summary>
        public static User? user = null;

        /// <summary>
        /// API请求客户端实例
        /// </summary>
        public static ApiClient apiClient = new ApiClient();

        /// <summary>
        /// 桌面背景窗口实例
        /// </summary>
        public static DesktopWindow? desktopWindow;

        /// <summary>
        /// 应用版本号（从程序集信息读取）
        /// </summary>
        public static Version? Version => Assembly.GetExecutingAssembly().GetName().Version;

        #endregion

        #region 错误与通知显示（新增ShowError方法）

        /// <summary>
        /// 显示错误通知（独立方法，便捷调用）
        /// </summary>
        /// <param name="title">错误标题</param>
        /// <param name="content">错误详情</param>
        public static void ShowError(string title, string content)
        {
            ShowNotification(title, content, InfoBarSeverity.Error);
        }

        /// <summary>
        /// 显示普通通知（通用方法，支持不同类型）
        /// </summary>
        /// <param name="title">通知标题</param>
        /// <param name="content">通知内容</param>
        /// <param name="severity">通知类型</param>
        public static void ShowInfo(string? title = "", string content = "", InfoBarSeverity severity = InfoBarSeverity.Informational)
        {
            ShowNotification(title, content, severity);
        }

        /// <summary>
        /// 通知显示核心逻辑（私有，统一实现）
        /// </summary>
        private static void ShowNotification(string? title, string content, InfoBarSeverity severity)
        {
            // 确保在UI线程执行（避免跨线程操作UI控件）
            Application.Current?.Dispatcher.Invoke(() =>
            {
                var mainWindow = MainWindow;
                if (mainWindow == null || mainWindow.Notifications == null)
                    return;

                // 创建新通知实例
                var notification = new Notification
                {
                    Title = string.IsNullOrEmpty(title) ? GetDefaultTitle(severity) : title,
                    Content = content,
                    Severity = severity
                };
                mainWindow.Notifications.Add(notification);

                // 3.1秒后自动移除通知（动画总时长：弹出0.6s + 停留2s + 收回0.5s）
                var timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(3.1),
                };
                timer.Tick += (s, e) =>
                {
                    if (mainWindow.Notifications.Contains(notification))
                        mainWindow.Notifications.Remove(notification);
                    timer.Stop();
                };
                timer.Start();
            });
        }

        /// <summary>
        /// 获取通知默认标题（根据类型自动生成）
        /// </summary>
        private static string GetDefaultTitle(InfoBarSeverity severity)
        {
            return severity switch
            {
                InfoBarSeverity.Error => "错误",
                InfoBarSeverity.Warning => "警告",
                InfoBarSeverity.Success => "成功",
                _ => "提示"
            };
        }

        #endregion

        #region 异常处理（统一日志+错误显示）

        /// <summary>
        /// 统一处理异常（记录日志+Sentry上报+UI错误提示）
        /// </summary>
        /// <param name="ex">异常实例</param>
        /// <param name="moduleName">发生异常的模块（如"文件备份"）</param>
        /// <param name="isSlient">是否静默处理（不显示UI提示）</param>
        public static async void HandleException(Exception ex, string moduleName = "", bool isSlient = false)
        {
            if (ex == null) return;

            // 1. 记录日志（线程安全，无需UI调度）
            Log.Error(ex, $"[{moduleName}] 发生异常");
            SentrySdk.CaptureException(ex);

            // 2. 显示UI错误提示（需UI线程）
            if (!isSlient)
            {
                await Application.Current?.Dispatcher.InvokeAsync(() =>
                {
                    // 截取异常信息（避免内容过长导致UI显示异常）
                    string shortExpContent = ex.ToString();
                    if (shortExpContent.Length > 200)
                        shortExpContent = $"{shortExpContent.Substring(0, 200)}...";

                    ShowError(
                        title: string.IsNullOrEmpty(moduleName) ? "操作失败" : $"{moduleName}失败",
                        content: shortExpContent
                    );
                });
            }
        }

        #endregion

        #region 路径管理（动态更新+目录校验）

        /// <summary>
        /// 更新基础配置路径（切换存储磁盘）
        /// </summary>
        /// <param name="diskPath">目标磁盘路径（如"D:\\"）</param>
        public static void UpdatePath(string diskPath = "D:\\")
        {
            // 校验磁盘是否存在
            if (!Directory.Exists(diskPath))
            {
                ShowError("路径无效", $"磁盘 {diskPath} 不存在，无法更新配置路径");
                return;
            }

            // 更新所有关联路径
            CONFIG_DISK = diskPath;
            CONFIG_DIR = $@"{CONFIG_DISK}CokeeTech\CokeeClass";
            BACKUP_FILE_DIR = $@"{CONFIG_DIR}\Files";
            INK_DIR = $@"{CONFIG_DIR}\ink";
            SCRSHOT_DIR = $@"{CONFIG_DIR}\ScreenShots";
            SCHEDULE_FILE = $@"{CONFIG_DIR}\schedule.json";
            STU_FILE = $@"{CONFIG_DIR}\students.json";
            SETTINGS_FILE = $@"{CONFIG_DIR}\config.json";

            // 确保配置根目录存在
            if (!Directory.Exists(CONFIG_DIR))
            {
                Directory.CreateDirectory(CONFIG_DIR);
                ShowInfo("配置目录已创建", CONFIG_DIR);
            }
        }

        #endregion

        #region 应用更新（预留逻辑）

        /// <summary>
        /// 检查应用更新（原AutoUpdater逻辑，预留扩展）
        /// </summary>
        public static void CheckUpdate()
        {
            try
            {
                /* 如需启用更新，取消注释并配置XML更新源
                AutoUpdater.ShowSkipButton = false;
                AutoUpdater.ShowRemindLaterButton = true;
                AutoUpdater.RemindLaterAt = 15;
                AutoUpdater.RemindLaterTimeSpan = RemindLaterFormat.Minutes;
                AutoUpdater.Start("https://gitee.com/cokee/classservice/raw/master/class_update.xml");
                */
            }
            catch (Exception ex)
            {
                HandleException(ex, "检查更新", isSlient: true);
            }
        }

        #endregion

        #region 窗口管理（创建+样式+显隐控制）

        /// <summary>
        /// 创建窗口实例（支持单例模式，避免重复打开）
        /// </summary>
        /// <typeparam name="T">窗口类型（需继承Window且有无参构造）</typeparam>
        /// <param name="allowMulti">是否允许多实例</param>
        public static async void CreateWindow<T>(bool allowMulti = false) where T : Window, new()
        {
            await Application.Current?.Dispatcher.InvokeAsync(() =>
            {
                if (Application.Current == null) return;

                // 查找已打开的窗口
                var existingWindow = Application.Current.Windows.OfType<T>().FirstOrDefault();
                if (existingWindow == null || allowMulti)
                {
                    // 新建窗口并显示
                    var newWindow = new T();
                    newWindow.Show();
                }
                else
                {
                    // 激活已存在的窗口（最小化则恢复）
                    if (existingWindow.WindowState == WindowState.Minimized)
                        existingWindow.WindowState = WindowState.Normal;
                    existingWindow.Activate();
                    ShowInfo("窗口已开启", "请在任务栏中查找已打开的窗口");
                }
            });
        }

        /// <summary>
        /// 设置主窗口样式（全屏/工作区大小）
        /// </summary>
        /// <param name="type">0：全屏，1：工作区大小</param>
        public static async void SetWindowStyle(int type = 0)
        {
            await Application.Current?.Dispatcher.InvokeAsync(() =>
            {
                var mainWindow = MainWindow;
                if (mainWindow == null) return;

                WindowType = type;
                if (type == 0)
                {
                    // 全屏模式（覆盖整个屏幕，包含任务栏）
                    mainWindow.Width = SystemParameters.PrimaryScreenWidth;
                    mainWindow.Height = SystemParameters.PrimaryScreenHeight;
                }
                else
                {
                    // 工作区模式（排除任务栏等系统区域）
                    mainWindow.Width = SystemParameters.WorkArea.Width;
                    mainWindow.Height = SystemParameters.WorkArea.Height;
                }
                // 重置窗口位置到左上角
                mainWindow.Top = 0;
                mainWindow.Left = 0;
            });
        }

        /// <summary>
        /// 切换控件可见性（带动画效果，提升UI体验）
        /// </summary>
        /// <param name="uIElement">目标控件</param>
        /// <param name="isForceShow">是否强制显示（忽略当前状态）</param>
        public static async void ToggleControlVisible(UIElement uIElement, bool isForceShow = false)
        {
            if (uIElement == null) return;

            await Application.Current?.Dispatcher.InvokeAsync(() =>
            {
                bool needShow = isForceShow || uIElement.Visibility == Visibility.Collapsed;

                if (needShow)
                {
                    // 显示动画：从透明到不透明
                    uIElement.Visibility = Visibility.Visible;
                    var fadeInAnim = new DoubleAnimation
                    {
                        From = 0.0,
                        To = 1.0,
                        Duration = TimeSpan.FromSeconds(0.2)
                    };
                    uIElement.BeginAnimation(UIElement.OpacityProperty, fadeInAnim);
                }
                else
                {
                    // 隐藏动画：从不透明到透明，完成后设为Collapsed
                    var fadeOutAnim = new DoubleAnimation
                    {
                        From = 1.0,
                        To = 0.0,
                        Duration = TimeSpan.FromSeconds(0.2)
                    };
                    fadeOutAnim.Completed += (s, e) =>
                    {
                        uIElement.Visibility = Visibility.Collapsed;
                        uIElement.Opacity = 1.0; // 重置透明度，避免下次显示异常
                    };
                    uIElement.BeginAnimation(UIElement.OpacityProperty, fadeOutAnim);
                }
            });
        }

        #endregion

        #region 文件操作（备份+目录处理）
        /// <summary>
        /// 备份文件到指定目录（异步线程执行，避免阻塞UI）
        /// </summary>
        /// <param name="filePath">源文件路径</param>
        /// <param name="fileName">备份后的文件名</param>
        /// <param name="isFullyDownloaded">源文件是否已完全下载</param>
        public static void BackupFile(string filePath, string fileName, bool isFullyDownloaded = true)
        {
            // 使用ThreadPool避免创建过多独立线程，提升资源利用率
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    // 显示备份开始提示
                    ShowInfo("文件备份", $"开始备份：{Path.GetFileName(filePath)}");

                    // 校验源文件有效性
                    if (!File.Exists(filePath) || !isFullyDownloaded)
                    {
                        ShowError("备份失败", "源文件不存在或未完全下载");
                        return;
                    }

                    // 获取源文件信息
                    var sourceFileInfo = new FileInfo(filePath);

                    // 按年月创建备份目录（如"Files/2024-10"），确保目录存在
                    var backupDir = $"{BACKUP_FILE_DIR}\\{DateTime.Now:yyyy-MM}";
                    FileSystemHelper.DirHelper.MakeExist(backupDir);

                    // 构建备份路径
                    var backupPath = Path.Combine(backupDir, fileName);

                    // 处理文件名冲突：若文件已存在且大小不同，则添加前缀"1_"
                    if (File.Exists(backupPath))
                    {
                        var existingFileInfo = new FileInfo(backupPath);
                        if (existingFileInfo.Length != sourceFileInfo.Length)
                        {
                            backupPath = Path.Combine(backupDir, $"1_{fileName}");
                        }
                    }

                    // 执行文件备份（允许覆盖同名文件）
                    sourceFileInfo.CopyTo(backupPath, overwrite: true);

                    // 显示备份成功提示
                    ShowInfo("备份成功", $"备份路径：{backupPath}");
                }
                catch (Exception ex)
                {
                    // 统一处理备份过程中的异常（记录日志+UI提示）
                    HandleException(ex, "文件备份");
                }
            });
        }
        #endregion
        #endregion
        /// <summary>
        /// 更新进度条与状态文本（用于耗时操作反馈）
        /// </summary>
        /// <param name="progress">当前进度（0-100）</param>
        /// <param name="isVisible">是否显示进度条</param>
        /// <param name="info">任务详情（包含ETA、速度等）</param>
        public static async void UpdateProgress(int progress, bool isVisible = true, TaskInfo? info = null)
        {
            // 校验主窗口是否可用
            if (MainWindow == null) return;

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                // 限制进度值范围（0-100）
                progress = Math.Clamp(progress, 0, 100);

                // 更新进度条数值
                MainWindow.progress.Value = progress;

                // 更新进度文本（处理info为空的默认情况）
                MainWindow.progressStr.Text = info != null
                    ? $"ETA: {info.ETA} | 速度 {info.Speed}/秒 | 剩余 {info.RestFiles}/{info.TotalFiles} 文件"
                    : $"当前进度: {progress}%";

                // 更新提示文本
                MainWindow.tipsText.Text = info != null
                    ? $"版本: {info.Version} | {info.NowName}: {progress}%"
                    : $"进度更新: {progress}%";

                // 控制进度条可见性
                bool shouldShow = isVisible && progress < 100;
                MainWindow.progress.IsActive = shouldShow;
                MainWindow.tipsBorder.Visibility = shouldShow ? Visibility.Visible : Visibility.Collapsed;
            }, DispatcherPriority.Background);
        }

        /// <summary>
        /// 释放COM对象（避免内存泄漏）
        /// </summary>
        /// <param name="comObject">COM对象实例</param>
        /// <param name="objectType">对象类型描述（用于日志）</param>
        public static async void ReleaseComObject(object? comObject, string objectType = "COM")
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ShowInfo($"资源释放", $"尝试释放 {objectType} 对象");

                // 空对象直接返回
                if (comObject == null)
                {
                    ShowInfo("释放提示", $"{objectType} 对象已为空，无需释放");
                    return;
                }

                try
                {
                    // 强制释放COM对象引用
                    Marshal.FinalReleaseComObject(comObject);
                    comObject = null; // 帮助GC回收
                    ShowInfo("释放成功", $"{objectType} 对象已释放");
                }
                catch (Exception ex)
                {
                    HandleException(ex, $"释放{objectType}对象失败");
                }
            }, DispatcherPriority.Normal);
        }
        /// <summary>
        /// 随机打乱列表顺序（Fisher-Yates洗牌算法）
        /// </summary>
        /// <typeparam name="T">列表元素类型</typeparam>
        /// <param name="list">待打乱的列表</param>
        /// <returns>打乱后的列表（原列表会被修改）</returns>
        public static async Task<List<T>> RandomizeList<T>(List<T> list)
        {
            // 空列表或单元素列表无需打乱
            if (list == null || list.Count <= 1)
                return list;

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var random = new Random();
                int n = list.Count;

                // Fisher-Yates洗牌算法（原地打乱，高效无偏）
                while (n > 1)
                {
                    n--;
                    int k = random.Next(n + 1);
                    // 交换元素（C# 7.0+ 元组语法）
                    (list[n], list[k]) = (list[k], list[n]);
                }
            });

            return list;
        }
    }
}
