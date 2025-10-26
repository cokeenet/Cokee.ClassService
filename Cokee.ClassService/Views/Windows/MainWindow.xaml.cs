using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using Cokee.ClassService.Helper;
using Cokee.ClassService.Shared;
using Cokee.ClassService.Views.Windows;

using iNKORE.UI.WPF.Modern;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
using iNKORE.UI.WPF.Modern.Controls;

using Microsoft.Win32;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Serilog;
using Serilog.Events;

using Application = System.Windows.Application;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using MsExcel = Microsoft.Office.Interop.Excel;
using MsPpt = Microsoft.Office.Interop.PowerPoint;
using MsWord = Microsoft.Office.Interop.Word;
using Point = System.Windows.Point;
using Timer = System.Timers.Timer;

namespace Cokee.ClassService
{
    /// <summary>
    /// 主窗口交互逻辑
    /// 2025/10/25---Cokee---主窗口代码AI整理后
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 字段定义

        // 拖拽相关字段
        private bool _isDragging;
        private Point _startPoint;
        private Point _mouseDownControlPosition;

        // 数据模型与定时器
        public Schedule Schedule;
        private readonly Timer _secondTimer = new Timer(1000);
        private readonly Timer _picTimer = new Timer(120000);
        private Task _checkOfficeTask;

        // Office应用程序对象
        public MsPpt.Application PptApplication;
        public MsWord.Application WordApplication;
        public MsExcel.Application ExcelApplication;

        // 通知集合
        public ObservableCollection<Notification> Notifications { get; set; }

        // 文件系统监控器
        public readonly FileSystemWatcher _desktopWatcher = new FileSystemWatcher(
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), Catalog.settings.FileWatcherFilter);

        // PPT页码与笔迹映射字典
        private readonly Dictionary<int, StrokeCollection> _pageStrokesDict = new Dictionary<int, StrokeCollection>();
        private int _currentPageNumber = 1;
        private string _currentPptPath; // 当前PPT文件路径
        private string _inkSaveDirectory; // 墨迹文件保存根目录

        // 白板相关字段
        private readonly StrokeCollection[] _strokeCollections = new StrokeCollection[101];
        private readonly bool[] _whiteboardLastModeIsRedo = new bool[101];
        private readonly StrokeCollection _lastTouchDownStrokeCollection = new StrokeCollection();
        private int _currentWhiteboardIndex = 1;
        private int _whiteboardTotalCount = 1;
        private readonly TimeMachineHistory[][] _timeMachineHistories = new TimeMachineHistory[101][]; // 最多99页，0存储非白板墨迹

        // 多点触控相关字段
        private readonly Dictionary<int, InkCanvasEditingMode> _touchDownPointsList = new Dictionary<int, InkCanvasEditingMode>();
        private readonly Dictionary<int, StrokeVisual> _strokeVisualList = new Dictionary<int, StrokeVisual>();
        private readonly Dictionary<int, VisualCanvas> _visualCanvasList = new Dictionary<int, VisualCanvas>();

        // 时间机器相关字段
        public enum CommitReason
        {
            UserInput,
            CodeInput,
            ShapeDrawing,
            ShapeRecognition,
            ClearingCanvas,
            Rotate
        }
        public CommitReason CurrentCommitType = CommitReason.UserInput;
        private bool IsEraseByPoint => inkcanvas.EditingMode == InkCanvasEditingMode.EraseByPoint;
        private StrokeCollection _replacedStroke;
        private StrokeCollection _addedStroke;
        private StrokeCollection _cuboidStrokeCollection;
        public TimeMachine TimeMachine = new TimeMachine();

        #endregion

        #region 构造函数与初始化

        public MainWindow()
        {
            InitializeComponent();
            rancor.RandomResultControl = ranres;
            inkTool.inkCanvas = inkcanvas;
            Notifications = new ObservableCollection<Notification>();
            infos.ItemsSource = Notifications;
            VerStr.Text = $"CokeeClass 版本{Catalog.Version?.ToString(4)}";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _ = Dispatcher.BeginInvoke(new Action(async () =>
            {
                // 初始化日志
                Log.Logger = new LoggerConfiguration()
                    .WriteTo.File($"D:\\DeviceLogs\\{DateTime.Now:yyyy-MM}\\{DateTime.Now:MM-dd}.txt",
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                    .WriteTo.RichTextBox(richTextBox, LogEventLevel.Verbose)
                    .CreateLogger();

                // 初始化桌面窗口
                if (Catalog.settings.DesktopBgWin)
                    new DesktopWindow().Show();

                // 窗口样式设置
                Catalog.SetWindowStyle(1);
                transT.X = -10;
                transT.Y = -100;
                UpdateLayout();

                // 事件注册
                SystemEvents.DisplaySettingsChanged += DisplaySettingsChanged;
                DpiChanged += DisplaySettingsChanged;
                SizeChanged += DisplaySettingsChanged;
                _secondTimer.Elapsed += SecondTimer_Elapsed;
                _secondTimer.Start();
                _picTimer.Elapsed += PicTimer_Elapsed;
                _picTimer.Start();

                // 初始化时间显示
                longDate.Text = DateTime.Now.ToString("yyyy年MM月dd日 ddd");

                // 非屏保模式初始化
                if (!Catalog.IsScrSave)
                {
                    if (PresentationSource.FromVisual(this) is HwndSource hwndSource)
                        hwndSource.AddHook(usbCard.WndProc);

                    if (Catalog.settings.FileWatcherEnable)
                        InitFileWatcher();

                    Catalog.CheckUpdate();
                }
                else
                {
                    tipsText.Visibility = Visibility.Visible;
                    tipsText.Text = "屏保模式";
                }

                // 触控事件注册
                if (Catalog.settings.MultiTouchEnable)
                {
                    inkcanvas.StylusDown += MainWindow_StylusDown;
                    inkcanvas.StylusMove += MainWindow_StylusMove;
                    inkcanvas.StylusUp += MainWindow_StylusUp;
                    inkcanvas.TouchDown += MainWindow_TouchDown;
                }

                // 笔迹事件注册
                inkcanvas.StrokeCollected += Inkcanvas_StrokeCollected;
                TimeMachine.OnRedoStateChanged += TimeMachine_OnRedoStateChanged;
                TimeMachine.OnUndoStateChanged += TimeMachine_OnUndoStateChanged;
                inkcanvas.Strokes.StrokesChanged += StrokesOnStrokesChanged;

                // 初始化日历与生日提醒
                GetCalendarInfo();
                CheckBirthDay();

                // 自动更新事件
                AutoUpdateHelper.downloader.DownloadStarted += DownloadStarted;
                AutoUpdateHelper.downloader.ChunkDownloadProgressChanged += ChunkDownloadProgressChanged;
                AutoUpdateHelper.downloader.DownloadProgressChanged += DownloadProgressChanged;
                AutoUpdateHelper.downloader.DownloadFileCompleted += DownloadFileCompleted;
            }), DispatcherPriority.Normal);
        }

        #endregion

        #region 窗口事件处理

        private void DisplaySettingsChanged(object sender, EventArgs e)
        {
            Catalog.SetWindowStyle(Catalog.WindowType);
            transT.X = -10;
            transT.Y = -100;
            UpdateLayout();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            SetToolWindow();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Log.Information("程序正在关闭");
            e.Cancel = true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Log.Information($"程序已关闭: {e}");
        }

        #endregion

        #region 定时器事件

        private void SecondTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _ = Dispatcher.InvokeAsync(() =>
            {
                // 更新时间显示
                time.Text = DateTime.Now.ToString("HH:mm:ss");
                time1.Text = DateTime.Now.ToString("HH:mm:ss");

                // 窗口样式调整
                var isForegroundMaximized = Win32Helper.IsForegroundMaximized();
                Catalog.SetWindowStyle(isForegroundMaximized ? 1 : 0);

                // 检查Office任务状态
                if (Catalog.settings.OfficeFunctionEnable)
                {
                    if (_checkOfficeTask?.Status == TaskStatus.Created)
                        _checkOfficeTask.Start();

                    if (_checkOfficeTask?.IsCompleted ?? true ||
                        _checkOfficeTask?.Status == TaskStatus.Canceled ||
                        _checkOfficeTask?.Status == TaskStatus.Faulted)
                    {
                        _checkOfficeTask = new Task(CheckOffice);
                        _checkOfficeTask.Start();
                    }
                }
            }, DispatcherPriority.Background);
        }

        private async void PicTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                var random = new Random();
                string url = $"pack://application:,,,/Resources/HeadPics/{random.Next(8)}.jpg";
                head.ProfilePicture = new BitmapImage(new Uri(url));
                StartAnimation(3, 3600);
            });
        }

        #endregion

        #region 文件监控

        public async void InitFileWatcher()
        {
            await Dispatcher.InvokeAsync(() =>
            {
                Catalog.ShowInfo("文件监控初始化", $"类型 {_desktopWatcher.NotifyFilter} 路径 {_desktopWatcher.Path}");
                _desktopWatcher.NotifyFilter = NotifyFilters.LastWrite;
                _desktopWatcher.Changed += DesktopWatcher_Changed;
                _desktopWatcher.Error += (a, b) =>
                {
                    _desktopWatcher.EnableRaisingEvents = false;
                    Catalog.HandleException(b.GetException(), "FileWatcher");
                };
                _desktopWatcher.Created += DesktopWatcher_Changed;
                _desktopWatcher.Renamed += DesktopWatcher_Changed;
                _desktopWatcher.EnableRaisingEvents = true;
            }, DispatcherPriority.Background);
        }

        private async void DesktopWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                if (!e.Name.Contains(".lnk") && !e.Name.Contains(".tmp") &&
                    !e.Name.Contains("~$") && e.Name.Contains("."))
                {
                    Catalog.ShowInfo($"桌面文件变动 Type:{e.ChangeType}", e.FullPath);
                    if (e.ChangeType != WatcherChangeTypes.Deleted)
                        Catalog.BackupFile(e.FullPath, e.Name);
                }
            }, DispatcherPriority.Normal);
        }

        #endregion

        #region 动画与UI交互

        private async void StartAnimation(int durationSeconds = 2, int angle = 180)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                var doubleAnimation = new DoubleAnimation
                {
                    Duration = new Duration(TimeSpan.FromSeconds(durationSeconds)),
                    EasingFunction = Catalog.easingFunction,
                    By = angle
                };
                rotateT.BeginAnimation(RotateTransform.AngleProperty, doubleAnimation);
            }, DispatcherPriority.Background);
        }

        public async void IconAnimation(bool isHide, FontIconData symbol,
            SolidColorBrush bgc = null, int autoHideTime = 0)
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                var doubleAnimation = new DoubleAnimation
                {
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                    EasingFunction = Catalog.easingFunction
                };

                iconE.Fill = bgc ?? new SolidColorBrush(Colors.White);
                icon.Icon = symbol;

                if (isHide)
                {
                    doubleAnimation.From = 1;
                    doubleAnimation.To = 0;
                }
                else
                {
                    doubleAnimation.From = 0;
                    doubleAnimation.To = 1;
                }

                iconTrans.BeginAnimation(ScaleTransform.ScaleXProperty, doubleAnimation);
                iconTrans.BeginAnimation(ScaleTransform.ScaleYProperty, doubleAnimation);

                if (autoHideTime > 0)
                {
                    await Task.Delay(autoHideTime).ContinueWith(_ =>
                    {
                        doubleAnimation.From = 1;
                        doubleAnimation.To = 0;
                        iconTrans.BeginAnimation(ScaleTransform.ScaleXProperty, doubleAnimation);
                        iconTrans.BeginAnimation(ScaleTransform.ScaleYProperty, doubleAnimation);
                    });
                }
            });
        }

        private void ToggleCard(bool fastHide = false)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var showAnimation = new DoubleAnimation(300, 0, TimeSpan.FromSeconds(1))
                {
                    EasingFunction = Catalog.easingFunction
                };

                var hideAnimation = new DoubleAnimation(0, 300, TimeSpan.FromSeconds(1))
                {
                    EasingFunction = Catalog.easingFunction
                };

                hideAnimation.Completed += (a, b) => sideCard.Visibility = Visibility.Collapsed;

                if (fastHide)
                {
                    cardtran.X = 300;
                    sideCard.Visibility = Visibility.Collapsed;
                }
                else if (sideCard.Visibility == Visibility.Visible)
                {
                    cardtran.BeginAnimation(TranslateTransform.XProperty, hideAnimation);
                }
                else
                {
                    sideCard.Visibility = Visibility.Visible;
                    cardtran.BeginAnimation(TranslateTransform.XProperty, showAnimation);
                    showAnimation.Completed += (a, b) =>
                    {
                        var floatGridPos = floatGrid.PointToScreen(new Point(0, 0));
                        var sideCardPos = sideCard.PointToScreen(new Point(0, 0));
                        var isFullyInside = new Rect(
                            sideCardPos.X, sideCardPos.Y,
                            sideCard.ActualWidth, sideCard.ActualHeight
                        ).Contains(floatGridPos);

                        if (isFullyInside) transT.Y = 0;
                    };
                }
            }), DispatcherPriority.Normal);
        }

        #endregion

        #region 拖拽功能

        private readonly Stopwatch _floatStopwatch = new Stopwatch();

        private void FloatGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _ = Dispatcher.InvokeAsync(() =>
            {
                _floatStopwatch.Restart();
                _isDragging = true;
                _startPoint = e.GetPosition(this);
                _mouseDownControlPosition = new Point(transT.X, transT.Y);
                floatGrid.CaptureMouse();
            });
        }

        private async void FloatGrid_MouseMove(object sender, MouseEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                if (_isDragging && _floatStopwatch.ElapsedMilliseconds >= 100)
                {
                    var pos = e.GetPosition(this);
                    var delta = pos - _startPoint;

                    // 边界检查
                    if (pos.X >= SystemParameters.FullPrimaryScreenWidth - 10 ||
                        pos.Y >= SystemParameters.FullPrimaryScreenHeight - 10)
                    {
                        _isDragging = false;
                        floatGrid.ReleaseMouseCapture();
                        transT.X = -10;
                        transT.Y = -100;
                        return;
                    }

                    transT.X = _mouseDownControlPosition.X + delta.X;
                    transT.Y = _mouseDownControlPosition.Y + delta.Y;
                }
            });
        }

        private void FloatGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                _floatStopwatch.Stop();
                StartAnimation();
                _isDragging = false;
                floatGrid.ReleaseMouseCapture();

                // 处理点击事件
                if (_floatStopwatch.ElapsedMilliseconds > 200) return;

                if (Catalog.settings.SideCardEnable)
                    ToggleCard();
                else
                    cardPopup.IsOpen = !cardPopup.IsOpen;
            }, DispatcherPriority.Normal);
        }

        #endregion

        #region 功能按钮事件

        private void MonitorOff(object sender, RoutedEventArgs e)
        {
            // 关闭显示器
            var hwnd = new WindowInteropHelper(this).Handle;
            Win32Helper.SendMessage(hwnd, Win32Helper.WM_SYSCOMMAND, Win32Helper.SC_MONITORPOWER, 2);
        }

        private void Debug_RightBtn(object sender, MouseButtonEventArgs e)
        {
            Catalog.ToggleControlVisible(logview);
        }

        private void Timer(object sender, RoutedEventArgs e)
        {
            ThemeManager.Current.ApplicationTheme =
                ThemeManager.Current.ApplicationTheme == ApplicationTheme.Light
                    ? ApplicationTheme.Dark
                    : ApplicationTheme.Light;
        }

        private async void StartInk(object sender, RoutedEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                if (inkTool.Visibility == Visibility.Collapsed || inkTool.isPPT)
                {
                    inkTool.SetCursorMode(inkTool.isPPT ? 0 : 1);
                    Catalog.SetWindowStyle();
                    inkTool.Visibility = Visibility.Visible;
                    IconAnimation(false, FluentSystemIcons.Pen_32_Regular);
                }
                else
                {
                    Catalog.SetWindowStyle(1);
                    inkTool.Visibility = Visibility.Collapsed;
                    IconAnimation(true, FluentSystemIcons.Pen_32_Regular);
                }
            });
        }

        private void StuMgr(object sender, RoutedEventArgs e) => Catalog.CreateWindow<StudentMgr>();

        private void ShowStickys(object sender, RoutedEventArgs e) => Catalog.CreateWindow<Sticky>();

        public void PostNote(object sender, RoutedEventArgs e) { }

        private void VolumeCard(object sender, RoutedEventArgs e)
        {
            cardPopup.IsOpen = false;
            Catalog.ToggleControlVisible(volcd);
        }

        private void QuickFix(object sender, RoutedEventArgs e)
        {
            Catalog.ShowInfo("测试", "测试内容1111");
        }

        private void UsbDebug(object sender, MouseButtonEventArgs e)
        {
            usbCard.EnumDrive();
        }

        private void Grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Catalog.settings.AgentEnable = !Catalog.settings.AgentEnable;
            slogan.Foreground = Catalog.settings.AgentEnable
                ? new SolidColorBrush(Colors.Yellow)
                : new SolidColorBrush(Colors.Tomato);

            // Catalog.settings.AgentEnable ? Catalog.CapServiceHost.StartAgent() : Catalog.CapServiceHost.StopAgent();
            Catalog.settings.Save();
        }

        private void ShowRandom(object sender, RoutedEventArgs e) => Catalog.CreateWindow<RandomWindow>();

        private void CourseMgr(object sender, RoutedEventArgs e) => Catalog.CreateWindow<CourseMgr>();

        private void AddFloatCard(object sender, RoutedEventArgs e) => Catalog.CreateWindow<FloatNote>(true);

        private void OpenSettings(object sender, RoutedEventArgs e) => Catalog.CreateWindow<Settings>();

        private async void ScreenShot(object sender, RoutedEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                cardPopup.IsOpen = false;
                if (sideCard.Visibility != Visibility.Collapsed)
                    ToggleCard(true);

                // 捕获屏幕
                var screenRect = System.Windows.Forms.SystemInformation.VirtualScreen;
                using var bitmap = new Bitmap(screenRect.Width, screenRect.Height);
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(screenRect.X, screenRect.Y, 0, 0, screenRect.Size, CopyPixelOperation.SourceCopy);
                }

                // 保存截图
                var savePath = $@"{Catalog.SCRSHOT_DIR}\{DateTime.Now:yyyy-MM-dd}\{DateTime.Now:HH-mm-ss}.png";
                var dir = Path.GetDirectoryName(savePath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                bitmap.Save(savePath, ImageFormat.Png);
                Catalog.ShowInfo("截图保存成功", $"路径: {savePath}");
            });
        }

        private void Button_MouseRightButtonDown(object sender, MouseButtonEventArgs e) => App.Current.Shutdown();

        private void Button_Click(object sender, RoutedEventArgs e) => Catalog.ToggleControlVisible(logview);

        #endregion

        #region 日历与生日提醒

        public async void GetCalendarInfo()
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                try
                {
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    using var client = new HttpClient();
                    var json = await client.GetStringAsync(
                        $"https://opendata.baidu.com/api.php?tn=wisetpl&format=json&resource_id=39043&query={DateTime.Now.Year}年{DateTime.Now.Month}月");

                    var data = JsonConvert.DeserializeObject<JObject>(json);
                    var festival = "";
                    var lunarDate = "";
                    var suitable = "";
                    var avoid = "";

                    if (data?["data"]?[0]?["almanac"] is JArray almanac)
                    {
                        foreach (var item in almanac)
                        {
                            if (item["month"]?.ToString() == DateTime.Now.Month.ToString() &&
                                item["day"]?.ToString() == DateTime.Now.Day.ToString())
                            {
                                festival = item["term"]?.ToString();
                                lunarDate = $"{item["gzYear"]}{item["animal"]}年 {item["lMonth"]}月{item["lDate"]}";
                                suitable = $"今日宜:{item["suit"]}";
                                avoid = $"今日不宜:{item["avoid"]}";
                            }
                        }
                    }

                    longDate.Text = $"{DateTime.Now:yyyy年MM月dd日 ddd} {festival}";
                    longCHNDate.Text = lunarDate;
                    c1.Text = suitable;
                    c2.Text = avoid;
                    exp.Visibility = Visibility.Visible;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "获取日历信息失败");
                }

                stopwatch.Stop();
                Log.Information($"获取节假日信息用时: {stopwatch.Elapsed.TotalSeconds}s");
            }, DispatcherPriority.Background);
        }

        public async void CheckBirthDay()
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    var studentData = await StudentExtensions.LoadAsync();
                    var students = new List<Student>(studentData.Students);
                    Student nearest = null;
                    var type = 0;

                    foreach (var student in students)
                    {
                        if (!student.BirthDay.HasValue) continue;

                        var birthDate = student.BirthDay.Value.ToString("MM-dd");
                        if (DateTime.Now.ToString("MM-dd") == birthDate)
                        {
                            nearest = student;
                            type = 1;
                            break;
                        }

                        if (DateTime.Now.AddDays(1).ToString("MM-dd") == birthDate)
                        {
                            nearest = student;
                            type = 2;
                        }
                    }

                    if (nearest != null)
                    {
                        birth.IsOpen = true;
                        birth.Message = type == 1
                            ? $"🎉 今天是 {nearest.Name} 的生日！"
                            : $"🎉 明天是 {nearest.Name} 的生日！";
                    }
                    else
                    {
                        birth.IsOpen = false;
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "检查生日信息失败");
                }
            });
        }

        #endregion

        #region 键盘事件

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!inkTool.isPPT) return;

            if (e.Key == Key.PageDown || e.Key == Key.Down)
                PptDown();
            else if (e.Key == Key.PageUp || e.Key == Key.Up)
                PptUp();
        }

        #endregion

        #region Office相关功能

        public void PptUp(object sender, RoutedEventArgs e) => PptUp();
        public void PptDown(object sender, RoutedEventArgs e) => PptDown();

        public void PptUp()
        {
            try
            {
                _ = Task.Run(() =>
                {
                    if (PptApplication == null)
                        throw new NullReferenceException("PPT对象不存在");

                    PptApplication.SlideShowWindows[1].Activate();
                    PptApplication.SlideShowWindows[1].View.Previous();
                });
            }
            catch
            {
                Dispatcher.Invoke(() =>
                {
                    pptControls.Visibility = Visibility.Collapsed;
                    inkTool.isPPT = false;
                });
            }
        }

        public void PptDown()
        {
            try
            {
                _ = Task.Run(() =>
                {
                    if (PptApplication == null)
                        throw new NullReferenceException("PPT对象不存在");

                    PptApplication.SlideShowWindows[1].Activate();
                    PptApplication.SlideShowWindows[1].View.Next();
                });
            }
            catch
            {
                Dispatcher.Invoke(() =>
                {
                    pptControls.Visibility = Visibility.Collapsed;
                    inkTool.isPPT = false;
                });
            }
        }

        private DateTime _lastOfficeErrorTime;
        private int _lastOfficeErrorCount = 0;

        private void CheckOffice()
        {
            try
            {
                // 检查PowerPoint
                if (ProcessHelper.HasPowerPointProcess() && PptApplication == null)
                {
                    PptApplication = (MsPpt.Application)MarshalForCore.GetActiveObject("PowerPoint.Application");
                    if (PptApplication?.Presentations.Count >= 1)
                    {
                        foreach (MsPpt.Presentation pres in PptApplication.Presentations)
                            Catalog.BackupFile(pres.FullName, pres.Name);

                        _currentPptPath = PptApplication.Presentations[1].FullName;
                        // 初始化墨迹保存目录
                        InitInkSaveDirectory();
                        // 加载已保存的墨迹批注
                        LoadInkAnnotations();

                        Catalog.ShowInfo("成功捕获PPT程序",
                            $"{PptApplication.Name}/版本:{PptApplication.Version}", InfoBarSeverity.Success);

                        if (!PptApplication.Name.Contains("Microsoft"))
                            Catalog.ShowInfo("警告", "不推荐使用WPS，高分辨率下可能无法播放视频", InfoBarSeverity.Warning);

                        // 注册PPT事件
                        PptApplication.PresentationClose += PptApplication_PresentationClose;
                        PptApplication.SlideShowBegin += PptApplication_SlideShowBegin;
                        PptApplication.SlideShowNextSlide += PptApplication_SlideShowNextSlide;
                        PptApplication.SlideShowEnd += PptApplication_SlideShowEnd;
                        PptApplication.ProtectedViewWindowOpen += PptApplication_ProtectedViewWindowOpen;

                        if (PptApplication.SlideShowWindows.Count >= 1)
                            PptApplication_SlideShowBegin(PptApplication.SlideShowWindows[1]);
                    }
                }

                // 检查Word
                if (ProcessHelper.HasWordProcess() && WordApplication == null && Catalog.settings.FileWatcherEnable)
                {
                    WordApplication = (MsWord.Application)MarshalForCore.GetActiveObject("Word.Application");
                    if (WordApplication != null)
                    {
                        Catalog.ShowInfo("成功捕获Word程序",
                            $"{WordApplication.Name}/版本:{WordApplication.Version}", InfoBarSeverity.Success);

                        WordApplication.DocumentOpen += doc => Catalog.BackupFile(doc.FullName, doc.Name);
                        WordApplication.DocumentBeforeClose += (doc, ref cancel) =>
                        {
                            Catalog.ShowInfo("尝试释放Word对象");
                            Catalog.ReleaseComObject(WordApplication);
                            WordApplication = null;
                        };

                        if (WordApplication.Documents.Count > 0)
                        {
                            foreach (MsWord.Document doc in WordApplication.Documents)
                                Catalog.BackupFile(doc.FullName, doc.Name);
                        }
                    }
                }

                // 检查Excel
                if (ProcessHelper.HasExcelProcess() && ExcelApplication == null && Catalog.settings.FileWatcherEnable)
                {
                    ExcelApplication = (MsExcel.Application)MarshalForCore.GetActiveObject("Excel.Application");
                    if (ExcelApplication != null)
                    {
                        Catalog.ShowInfo("成功捕获Excel程序",
                            $"{ExcelApplication.Name}/版本:{ExcelApplication.Version}", InfoBarSeverity.Success);

                        ExcelApplication.WorkbookOpen += wb => Catalog.BackupFile(wb.FullName, wb.Name);
                        ExcelApplication.WorkbookBeforeClose += (wb, ref cancel) =>
                        {
                            Catalog.ShowInfo("尝试释放Excel对象");
                            Catalog.ReleaseComObject(ExcelApplication);
                            ExcelApplication = null;
                        };

                        if (ExcelApplication.Workbooks.Count > 0)
                        {
                            foreach (MsExcel.Workbook wb in ExcelApplication.Workbooks)
                                Catalog.BackupFile(wb.FullName, wb.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.ToString().Contains("CO_E_CLASSSTRING"))
                {
                    Catalog.ShowInfo("Office未安装", "无法使用PPT批注及Office功能，已自动关闭", InfoBarSeverity.Warning);
                    Catalog.settings.OfficeFunctionEnable = false;
                    Catalog.settings.Save();
                    return;
                }

                _lastOfficeErrorCount++;
                if (_lastOfficeErrorCount >= 15 &&
                    DateTime.Now.Subtract(_lastOfficeErrorTime).TotalMinutes <= 10)
                {
                    Catalog.ShowInfo("Office功能错误过多", "已自动关闭Office功能", InfoBarSeverity.Warning);
                    Catalog.settings.OfficeFunctionEnable = false;
                    Catalog.settings.Save();
                    return;
                }

                Catalog.ReleaseComObject(WordApplication);
                Catalog.ReleaseComObject(PptApplication);
                Catalog.ReleaseComObject(ExcelApplication);
                _lastOfficeErrorTime = DateTime.Now;
                Catalog.HandleException(ex, "Office功能");
            }
        }

        private void PptApplication_SlideShowEnd(MsPpt.Presentation pres)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                ClearStrokes(true);
                pptControls.Visibility = Visibility.Collapsed;
                Catalog.SetWindowStyle(1);
                inkcanvas.IsEnabled = false;
                inkTool.Visibility = Visibility.Collapsed;
                inkcanvas.Background.Opacity = 0;
                inkTool.isPPT = false;
                SaveInkAnnotations();
                Catalog.ShowInfo("PPT放映结束");
            }), DispatcherPriority.Background);
        }

        // PPT页码切换事件 - 核心修改：保存当前页笔迹并加载新页笔迹
        private void PptApplication_SlideShowNextSlide(MsPpt.SlideShowWindow wn)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!inkTool.isPPT) return;

                // 保存当前页笔迹
                SaveCurrentPageStrokes(_currentPageNumber);

                // 更新页码显示
                var newPageNumber = wn.View.CurrentShowPosition;
                NowPageText.Text = newPageNumber.ToString();
                TotalPageText.Text = wn.Presentation.Slides.Count.ToString();

                // 加载新页笔迹
                LoadPageStrokes(newPageNumber);

                // 更新当前页码记录
                _currentPageNumber = newPageNumber;
            }), DispatcherPriority.Normal);
        }

        private void PptApplication_PresentationClose(MsPpt.Presentation pres)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                pptControls.Visibility = Visibility.Collapsed;
                inkTool.isPPT = false;
                Catalog.ShowInfo("尝试释放PPT对象");
                SaveInkAnnotations();
                Catalog.ReleaseComObject(PptApplication);
                PptApplication = null;
            }), DispatcherPriority.Background);
        }

        private void PptApplication_ProtectedViewWindowOpen(MsPpt.ProtectedViewWindow wn)
        {
            inkTool.isPPT = true;
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Catalog.ShowInfo("PPT放映开始");
                StartInk(null, null);
                inkTool.SetCursorMode(0);
                inkcanvas.Background.Opacity = 0;
                pptControls.Visibility = Visibility.Visible;
                NowPageText.Text = wn.Presentation.Slides.Count.ToString();
                TotalPageText.Text = wn.Presentation.Slides.Count.ToString();

                if (PptApplication?.Presentations.Count >= 1)
                {
                    foreach (MsPpt.Presentation pres in PptApplication.Presentations)
                        Catalog.BackupFile(pres.FullName, pres.Name);
                }
            }), DispatcherPriority.Background);
        }

        private void PptApplication_SlideShowBegin(MsPpt.SlideShowWindow wn)
        {
            inkTool.isPPT = true;
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Catalog.ShowInfo("PPT放映开始");
                StartInk(null, null);
                inkTool.SetCursorMode(0);
                inkcanvas.Background.Opacity = 0;
                pptControls.Visibility = Visibility.Visible;
                NowPageText.Text = wn.View.CurrentShowPosition.ToString();
                TotalPageText.Text = wn.Presentation.Slides.Count.ToString();

                if (PptApplication?.Presentations.Count >= 1)
                {
                    foreach (MsPpt.Presentation pres in PptApplication.Presentations)
                        Catalog.BackupFile(pres.FullName, pres.Name);
                }
            }), DispatcherPriority.Background);
        }

        #endregion

        #region PPT笔迹保存与加载

        /// <summary>
        /// 保存当前页笔迹到内存
        /// </summary>
        private void SaveCurrentPageStrokes(int pageNumber)
        {
            // 深拷贝当前笔迹避免引用问题
            var currentStrokes = new StrokeCollection(inkcanvas.Strokes);

            if (_pageStrokesDict.ContainsKey(pageNumber))
                _pageStrokesDict[pageNumber] = currentStrokes;
            else
                _pageStrokesDict.Add(pageNumber, currentStrokes);
        }
        // 添加初始化墨迹保存目录的方法
        private void InitInkSaveDirectory()
        {
            if (string.IsNullOrEmpty(_currentPptPath)) return;

            // 获取PPT所在目录
            string pptDirectory = Path.GetDirectoryName(_currentPptPath);
            // 获取PPT文件名（不含扩展名）
            string pptFileName = Path.GetFileNameWithoutExtension(_currentPptPath);

            // 创建CokeeInk目录及对应PPT的子目录
            _inkSaveDirectory = Path.Combine(pptDirectory, "CokeeInk", pptFileName);
            if (!Directory.Exists(_inkSaveDirectory))
            {
                Directory.CreateDirectory(_inkSaveDirectory);
                Catalog.ShowInfo("墨迹保存目录已创建", _inkSaveDirectory);
            }
        }

        // 添加加载墨迹批注的方法
        private void LoadInkAnnotations()
        {
            if (string.IsNullOrEmpty(_inkSaveDirectory) || !Directory.Exists(_inkSaveDirectory))
                return;

            try
            {
                // 读取目录中所有墨迹文件
                string[] inkFiles = Directory.GetFiles(_inkSaveDirectory, "Page_*.ink");
                foreach (string file in inkFiles)
                {
                    // 从文件名提取页码（格式：Page_1.ink）
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    if (int.TryParse(fileName.Split('_')[1], out int pageNumber))
                    {
                        // 加载墨迹文件
                        using (var fs = new FileStream(file, FileMode.Open))
                        {
                            var strokes = new StrokeCollection(fs);
                            if (_pageStrokesDict.ContainsKey(pageNumber))
                                _pageStrokesDict[pageNumber] = strokes;
                            else
                                _pageStrokesDict.Add(pageNumber, strokes);
                        }
                        Catalog.ShowInfo($"已加载页码 {pageNumber} 的墨迹批注", file);
                    }
                }
            }
            catch (Exception ex)
            {
                Catalog.HandleException(ex, "加载墨迹批注");
            }
        }

        // 添加保存墨迹批注的方法
        private void SaveInkAnnotations()
        {
            if (string.IsNullOrEmpty(_inkSaveDirectory) || _pageStrokesDict.Count == 0)
                return;

            try
            {
                // 保存每个页码的墨迹
                foreach (var kvp in _pageStrokesDict)
                {
                    string inkFilePath = Path.Combine(_inkSaveDirectory, $"Page_{kvp.Key}.ink");
                    using (var fs = new FileStream(inkFilePath, FileMode.Create))
                    {
                        kvp.Value.Save(fs);
                    }
                    Catalog.ShowInfo($"已保存页码 {kvp.Key} 的墨迹批注", inkFilePath);
                }
                Catalog.ShowInfo("所有墨迹批注已保存", _inkSaveDirectory);
            }
            catch (Exception ex)
            {
                Catalog.HandleException(ex, "保存墨迹批注");
            }
        }

        /// <summary>
        /// 加载指定页的笔迹
        /// </summary>
        private void LoadPageStrokes(int pageNumber)
        {
            CurrentCommitType = CommitReason.ClearingCanvas;
            inkcanvas.Strokes.Clear();

            if (_pageStrokesDict.TryGetValue(pageNumber, out var savedStrokes))
            {
                // 深拷贝加载避免后续修改影响存储
                inkcanvas.Strokes.Add(new StrokeCollection(savedStrokes));
            }

            CurrentCommitType = CommitReason.UserInput;
        }

        #endregion

        #region 多点触控处理

        private void MainWindow_TouchDown(object sender, TouchEventArgs e)
        {
            var boundWidth = e.GetTouchPoint(null).Bounds.Width;
            if (boundWidth > 20)
            {
                inkcanvas.EraserShape = new EllipseStylusShape(boundWidth, boundWidth);
                _touchDownPointsList[e.TouchDevice.Id] = InkCanvasEditingMode.EraseByPoint;
                inkcanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
            }
            else if (!inkTool.isEraser)
            {
                _touchDownPointsList[e.TouchDevice.Id] = InkCanvasEditingMode.None;
                inkcanvas.EditingMode = InkCanvasEditingMode.None;
            }
        }

        private void MainWindow_StylusDown(object sender, StylusDownEventArgs e)
        {
            _touchDownPointsList[e.StylusDevice.Id] = InkCanvasEditingMode.None;
        }

        private void Inkcanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            try
            {
                // 检查是否是压感笔书写
                if (e.Stroke.StylusPoints.Any(p => p.PressureFactor != 0.5 && p.PressureFactor != 0))
                    return;

                // 计算点速度（辅助功能）
                double GetPointSpeed(Point p1, Point p2, Point p3)
                {
                    return (Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2)) +
                            Math.Sqrt(Math.Pow(p3.X - p2.X, 2) + Math.Pow(p3.Y - p2.Y, 2))) / 20;
                }

                // 速度计算（未使用）
                if (e.Stroke.StylusPoints.Count > 3)
                {
                    var random = new Random();
                    var speed = GetPointSpeed(
                        e.Stroke.StylusPoints[random.Next(e.Stroke.StylusPoints.Count - 1)].ToPoint(),
                        e.Stroke.StylusPoints[random.Next(e.Stroke.StylusPoints.Count - 1)].ToPoint(),
                        e.Stroke.StylusPoints[random.Next(e.Stroke.StylusPoints.Count - 1)].ToPoint());
                }

                // 处理笔迹点（压感调整）
                var stylusPoints = new StylusPointCollection();
                var pointCount = e.Stroke.StylusPoints.Count - 1;
                const double pressure = 0.1;
                const int segment = 10;

                if (pointCount == 1) return;

                if (pointCount >= segment)
                {
                    for (var i = 0; i < pointCount - segment; i++)
                    {
                        stylusPoints.Add(new StylusPoint(
                            e.Stroke.StylusPoints[i].X,
                            e.Stroke.StylusPoints[i].Y,
                            0.5f));
                    }

                    for (var i = pointCount - segment; i <= pointCount; i++)
                    {
                        var factor = (float)((0.5 - pressure) * (pointCount - i) / segment + pressure);
                        stylusPoints.Add(new StylusPoint(
                            e.Stroke.StylusPoints[i].X,
                            e.Stroke.StylusPoints[i].Y,
                            factor));
                    }
                }
                else
                {
                    for (var i = 0; i <= pointCount; i++)
                    {
                        var factor = (float)(0.4 * (pointCount - i) / pointCount + pressure);
                        stylusPoints.Add(new StylusPoint(
                            e.Stroke.StylusPoints[i].X,
                            e.Stroke.StylusPoints[i].Y,
                            factor));
                    }
                }

                e.Stroke.StylusPoints = stylusPoints;
            }
            catch { /* 忽略笔迹处理异常 */ }
        }

        private void MainWindow_StylusUp(object sender, StylusEventArgs e)
        {
            try
            {
                if (!inkTool.isEraser)
                {
                    inkcanvas.Strokes.Add(GetStrokeVisual(e.StylusDevice.Id).Stroke);
                    inkcanvas.Children.Remove(GetVisualCanvas(e.StylusDevice.Id));
                    Inkcanvas_StrokeCollected(inkcanvas,
                        new InkCanvasStrokeCollectedEventArgs(GetStrokeVisual(e.StylusDevice.Id).Stroke));
                }
            }
            catch { /* 忽略笔抬起事件异常 */ }

            try
            {
                _strokeVisualList.Remove(e.StylusDevice.Id);
                _visualCanvasList.Remove(e.StylusDevice.Id);
                _touchDownPointsList.Remove(e.StylusDevice.Id);

                if (_strokeVisualList.Count == 0 && _visualCanvasList.Count == 0 && _touchDownPointsList.Count == 0)
                {
                    inkcanvas.Children.Clear();
                    _strokeVisualList.Clear();
                    _visualCanvasList.Clear();
                    _touchDownPointsList.Clear();
                }
            }
            catch { /* 忽略清理异常 */ }
        }

        private void MainWindow_StylusMove(object sender, StylusEventArgs e)
        {
            try
            {
                if (inkTool.isEraser) return;
                if (GetTouchDownPointsList(e.StylusDevice.Id) != InkCanvasEditingMode.None) return;

                try
                {
                    if (e.StylusDevice.StylusButtons[1].StylusButtonState == StylusButtonState.Down)
                        return;
                }
                catch { /* 忽略笔按钮检查异常 */ }

                var strokeVisual = GetStrokeVisual(e.StylusDevice.Id);
                var points = e.GetStylusPoints(this);
                foreach (var point in points)
                {
                    strokeVisual.Add(new StylusPoint(point.X, point.Y, point.PressureFactor));
                }
                strokeVisual.ReDraw();
            }
            catch { /* 忽略笔移动事件异常 */ }
        }

        private StrokeVisual GetStrokeVisual(int id)
        {
            if (_strokeVisualList.TryGetValue(id, out var visual))
                return visual;

            var strokeVisual = new StrokeVisual(inkcanvas.DefaultDrawingAttributes.Clone());
            _strokeVisualList[id] = strokeVisual;
            var visualCanvas = new VisualCanvas(strokeVisual);
            _visualCanvasList[id] = visualCanvas;
            inkcanvas.Children.Add(visualCanvas);

            return strokeVisual;
        }

        private VisualCanvas GetVisualCanvas(int id)
        {
            _visualCanvasList.TryGetValue(id, out var canvas);
            return canvas;
        }

        private InkCanvasEditingMode GetTouchDownPointsList(int id)
        {
            _touchDownPointsList.TryGetValue(id, out var mode);
            return mode;
        }

        #endregion

        #region 白板笔迹管理

        private void SaveStrokes(bool isBackupMain = false)
        {
            if (isBackupMain)
            {
                var history = TimeMachine.ExportTimeMachineHistory();
                _timeMachineHistories[0] = history;
                TimeMachine.ClearStrokeHistory();
            }
            else
            {
                var history = TimeMachine.ExportTimeMachineHistory();
                _timeMachineHistories[_currentWhiteboardIndex] = history;
                TimeMachine.ClearStrokeHistory();
            }
        }

        public void ClearStrokes(bool isErasedByCode)
        {
            CurrentCommitType = CommitReason.ClearingCanvas;
            if (isErasedByCode)
                CurrentCommitType = CommitReason.CodeInput;

            inkcanvas.Strokes.Clear();
            CurrentCommitType = CommitReason.UserInput;
        }

        private void RestoreStrokes(bool isBackupMain = false)
        {
            try
            {
                var targetIndex = isBackupMain ? 0 : _currentWhiteboardIndex;
                if (_timeMachineHistories[targetIndex] == null) return;

                CurrentCommitType = CommitReason.CodeInput;
                TimeMachine.ImportTimeMachineHistory(_timeMachineHistories[targetIndex]);

                foreach (var item in _timeMachineHistories[targetIndex])
                {
                    switch (item.CommitType)
                    {
                        case TimeMachineHistoryType.UserInput:
                            HandleUserInputHistory(item);
                            break;
                        case TimeMachineHistoryType.ShapeRecognition:
                            HandleShapeRecognitionHistory(item);
                            break;
                        case TimeMachineHistoryType.Rotate:
                            HandleRotateHistory(item);
                            break;
                        case TimeMachineHistoryType.Clear:
                            HandleClearHistory(item);
                            break;
                    }
                }

                CurrentCommitType = CommitReason.UserInput;
            }
            catch { /* 忽略恢复异常 */ }
        }

        #region 历史记录处理辅助方法

        private void HandleUserInputHistory(TimeMachineHistory item)
        {
            if (!item.StrokeHasBeenCleared)
            {
                foreach (var stroke in item.CurrentStroke)
                {
                    if (!inkcanvas.Strokes.Contains(stroke))
                        inkcanvas.Strokes.Add(stroke);
                }
            }
            else
            {
                foreach (var stroke in item.CurrentStroke)
                {
                    if (inkcanvas.Strokes.Contains(stroke))
                        inkcanvas.Strokes.Remove(stroke);
                }
            }
        }

        private void HandleShapeRecognitionHistory(TimeMachineHistory item)
        {
            if (item.StrokeHasBeenCleared)
            {
                foreach (var stroke in item.CurrentStroke)
                {
                    if (inkcanvas.Strokes.Contains(stroke))
                        inkcanvas.Strokes.Remove(stroke);
                }
                foreach (var stroke in item.ReplacedStroke)
                {
                    if (!inkcanvas.Strokes.Contains(stroke))
                        inkcanvas.Strokes.Add(stroke);
                }
            }
            else
            {
                foreach (var stroke in item.CurrentStroke)
                {
                    if (!inkcanvas.Strokes.Contains(stroke))
                        inkcanvas.Strokes.Add(stroke);
                }
                foreach (var stroke in item.ReplacedStroke)
                {
                    if (inkcanvas.Strokes.Contains(stroke))
                        inkcanvas.Strokes.Remove(stroke);
                }
            }
        }

        private void HandleRotateHistory(TimeMachineHistory item)
        {
            if (item.StrokeHasBeenCleared)
            {
                foreach (var stroke in item.CurrentStroke)
                {
                    if (inkcanvas.Strokes.Contains(stroke))
                        inkcanvas.Strokes.Remove(stroke);
                }
                foreach (var stroke in item.ReplacedStroke)
                {
                    if (!inkcanvas.Strokes.Contains(stroke))
                        inkcanvas.Strokes.Add(stroke);
                }
            }
            else
            {
                foreach (var stroke in item.CurrentStroke)
                {
                    if (!inkcanvas.Strokes.Contains(stroke))
                        inkcanvas.Strokes.Add(stroke);
                }
                foreach (var stroke in item.ReplacedStroke)
                {
                    if (inkcanvas.Strokes.Contains(stroke))
                        inkcanvas.Strokes.Remove(stroke);
                }
            }
        }

        private void HandleClearHistory(TimeMachineHistory item)
        {
            if (!item.StrokeHasBeenCleared)
            {
                if (item.CurrentStroke != null)
                {
                    foreach (var stroke in item.CurrentStroke)
                    {
                        if (!inkcanvas.Strokes.Contains(stroke))
                            inkcanvas.Strokes.Add(stroke);
                    }
                }
                if (item.ReplacedStroke != null)
                {
                    foreach (var stroke in item.ReplacedStroke)
                    {
                        if (inkcanvas.Strokes.Contains(stroke))
                            inkcanvas.Strokes.Remove(stroke);
                    }
                }
            }
            else
            {
                if (item.ReplacedStroke != null)
                {
                    foreach (var stroke in item.ReplacedStroke)
                    {
                        if (!inkcanvas.Strokes.Contains(stroke))
                            inkcanvas.Strokes.Add(stroke);
                    }
                }
                if (item.CurrentStroke != null)
                {
                    foreach (var stroke in item.CurrentStroke)
                    {
                        if (inkcanvas.Strokes.Contains(stroke))
                            inkcanvas.Strokes.Remove(stroke);
                    }
                }
            }
        }

        #endregion

        #endregion

        #region 时间机器（撤销/重做）
        public TimeMachineHistory? UndoInk()
        {
            return TimeMachine?.Undo();
        }

        // 用于墨迹工具栏访问的公共重做方法
        public TimeMachineHistory? RedoInk()
        {
            return TimeMachine?.Redo();
        }
        private void TimeMachine_OnUndoStateChanged(bool status)
        {
            inkTool.undoBtn.Visibility = status ? Visibility.Visible : Visibility.Collapsed;
            inkTool.redoBtn.IsEnabled = status;
        }

        private void TimeMachine_OnRedoStateChanged(bool status)
        {
            inkTool.redoBtn.Visibility = status ? Visibility.Visible : Visibility.Collapsed;
            inkTool.redoBtn.IsEnabled = status;
        }

        private void StrokesOnStrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            if (CurrentCommitType == CommitReason.CodeInput || CurrentCommitType == CommitReason.ShapeDrawing)
                return;

            if (CurrentCommitType == CommitReason.Rotate)
            {
                TimeMachine.CommitStrokeRotateHistory(e.Removed, e.Added);
                return;
            }

            if ((e.Added.Count != 0 || e.Removed.Count != 0) && IsEraseByPoint)
            {
                _addedStroke ??= new StrokeCollection();
                _replacedStroke ??= new StrokeCollection();
                _addedStroke.Add(e.Added);
                _replacedStroke.Add(e.Removed);
                return;
            }

            if (e.Added.Count != 0)
            {
                if (CurrentCommitType == CommitReason.ShapeRecognition)
                {
                    TimeMachine.CommitStrokeShapeHistory(_replacedStroke, e.Added);
                    _replacedStroke = null;
                    return;
                }

                TimeMachine.CommitStrokeUserInputHistory(e.Added);
                return;
            }

            if (e.Removed.Count != 0)
            {
                if (CurrentCommitType == CommitReason.ShapeRecognition)
                {
                    _replacedStroke = e.Removed;
                    return;
                }

                if (!IsEraseByPoint || CurrentCommitType == CommitReason.ClearingCanvas)
                {
                    TimeMachine.CommitStrokeEraseHistory(e.Removed);
                }
            }
        }

        #endregion

        #region 工具方法

        public void SetToolWindow()
        {
            const int wsExToolWindow = 0x80;
            var hwnd = new WindowInteropHelper(this).Handle;
            var currentStyle = Win32Helper.GetWindowLong(hwnd, -20); // GWL_EXSTYLE
            var newStyle = (currentStyle & ~0x00000040) | wsExToolWindow;
            Win32Helper.SetWindowLong(hwnd, -20, newStyle);
        }

        #endregion

        #region 自动更新事件（未实现）

        private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void DownloadProgressChanged(object sender, Downloader.DownloadProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ChunkDownloadProgressChanged(object sender, Downloader.DownloadProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void DownloadStarted(object sender, Downloader.DownloadStartedEventArgs e)
        {
        }

        #endregion
    }
}