using AutoUpdaterDotNET;
using Cokee.ClassService.Helper;
using Cokee.ClassService.Shared;
using Cokee.ClassService.Views.Windows;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Events;
using Serilog.Sink.AppCenter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Wpf.Ui.Appearance;
using Wpf.Ui.Common;
using Wpf.Ui.Mvvm.Services;
using Application = System.Windows.Application;
using Control = System.Windows.Controls.Control;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using MsExcel = Microsoft.Office.Interop.Excel;
using MsPpt = Microsoft.Office.Interop.PowerPoint;
using MsWord = Microsoft.Office.Interop.Word;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Windows.Point;
using Timer = System.Timers.Timer;

namespace Cokee.ClassService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isDragging;
        private Point startPoint, _mouseDownControlPosition;

        private CapService service;

        //private event EventHandler<bool>? RandomEvent;
        private Timer secondTimer = new Timer(1000);

        private Timer picTimer = new Timer(120000);
        public MsPpt.Application? pptApplication;
        public MsWord.Application? wordApplication;
        public MsExcel.Application? excelApplication;

        public FileSystemWatcher desktopWatcher = new FileSystemWatcher(
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), Catalog.settings.FileWatcherFilter);

        private StrokeCollection[] strokes = new StrokeCollection[101];
        public int page;

        private Schedule schedule = Schedule.LoadFromJson();
        private Version? version = Assembly.GetExecutingAssembly().GetName().Version;
        public SnackbarService snackbarService = new SnackbarService();
        private Task CheckOfficeTask;

        public MainWindow()
        {
            InitializeComponent();
            Catalog.MainWindow = this;
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("log.txt",
                    outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.AppCenterSink(null, LogEventLevel.Error)
                .WriteTo.RichTextBox(richTextBox, LogEventLevel.Verbose)
                .CreateLogger();
            Catalog.GlobalSnackbarService = snackbarService;
            snackbarService.SetSnackbarControl(snackbar);
            snackbarService.Timeout = 4000;
            inkTool.inkCanvas = inkcanvas;
            VerStr.Text =
                $"CokeeClass 版本{version?.ToString(4)}";
        }

        private void DisplaySettingsChanged(object? sender, EventArgs e)
        {
            Catalog.SetWindowStyle(Catalog.WindowType);
            transT.X = -10;
            transT.Y = -100;
            UpdateLayout();
            Accent.ApplySystemAccent();
        }

        public void IntiAgent()
        {
            service = new CapService();
            service.CapStartEvent += CapStart;
            service.CapDoneEvent += CapDone;
            service.Start();
        }

        private void CapDone(object? sender, string e)
        {
            //IconAnimation(false, SymbolRegular.Balloon20, new SolidColorBrush() { Color = Colors.ForestGreen }, 5000);
        }

        private void CapStart(object? sender, string e)
        {
            //IconAnimation(false, SymbolRegular.Add24, new SolidColorBrush() { Color = Colors.IndianRed }, 3000);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Catalog.SetWindowStyle(1);
                SystemEvents.DisplaySettingsChanged += DisplaySettingsChanged;
                richTextBox.TextChanged += (sender, e) =>
                {
                    if (richTextBox.Document.Blocks.Count > 100)
                    {
                        while (richTextBox.Document.Blocks.Count > 100)
                        {
                            richTextBox.Document.Blocks.Remove(richTextBox.Document.Blocks.FirstBlock);
                        }
                    }
                };

                DpiChanged += DisplaySettingsChanged;
                SizeChanged += DisplaySettingsChanged;
                secondTimer.Elapsed += SecondTimer_Elapsed;
                secondTimer.Start();
                picTimer.Elapsed += PicTimer_Elapsed;
                picTimer.Start();
                longDate.Text = DateTime.Now.ToString("yyyy年MM月dd日 ddd");

                CheckOfficeTask = new Task(CheckOffice);
                if (!Catalog.IsScrSave)
                {
                    AutoUpdater.Start("https://gitee.com/cokee/classservice/raw/master/class_update.xml");
                    HwndSource? hwndSource = PresentationSource.FromVisual(this) as HwndSource;
                    hwndSource.AddHook(usbCard.WndProc);
                    if (Catalog.settings.AgentEnable) IntiAgent();
                    if (Catalog.settings.FileWatcherEnable) IntiFileWatcher();
                    Catalog.CheckUpdate();
                }
                else
                {
                    nameBadge.Visibility = Visibility.Visible;
                    nameBadge.Content = "屏保模式";
                }

                if (Catalog.settings.MultiTouchEnable)
                {
                    inkcanvas.StylusDown += MainWindow_StylusDown;
                    inkcanvas.StylusMove += MainWindow_StylusMove;
                    inkcanvas.StylusUp += MainWindow_StylusUp;
                    inkcanvas.TouchDown += MainWindow_TouchDown;
                }

                inkcanvas.StrokeCollected += inkcanvas_StrokeCollected;
                timeMachine.OnRedoStateChanged += TimeMachine_OnRedoStateChanged;
                timeMachine.OnUndoStateChanged += TimeMachine_OnUndoStateChanged;
                inkcanvas.Strokes.StrokesChanged += StrokesOnStrokesChanged;
                GetCalendarInfo();
                CheckBirthDay();
            }), DispatcherPriority.Normal);
        }

        private void MonitorOff(object sender, RoutedEventArgs e)
        {
            //关闭显示器
            Win32Helper.SendMessage(new WindowInteropHelper(this).Handle, Win32Helper.WM_SYSCOMMAND,
                Win32Helper.SC_MONITORPOWER, 2);

            //打开显示器
            //Win32Helper.SendMessage(this.Handle, WM_SYSCOMMAND, SC_MONITORPOWER, -1);
        }

        private void Debug_RightBtn(object sender, MouseButtonEventArgs e)
        {
            Catalog.ToggleControlVisible(logview);
        }

        public void IntiFileWatcher()
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Catalog.ShowInfo("FileWatcher初始化", $"类型 {desktopWatcher.NotifyFilter} 作用路径 {desktopWatcher.Path}");
                desktopWatcher.NotifyFilter = NotifyFilters.LastWrite;
                desktopWatcher.Changed += DesktopWatcher_Changed;
                desktopWatcher.Error += (a, b) =>
                {
                    desktopWatcher.EnableRaisingEvents = false;
                    Catalog.HandleException(b.GetException(), "FileWatcher");
                };
                desktopWatcher.Created += DesktopWatcher_Changed;
                desktopWatcher.Renamed += DesktopWatcher_Changed;
                desktopWatcher.EnableRaisingEvents = true;
            }), DispatcherPriority.Background);
        }

        private void DesktopWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!e.Name.Contains(".lnk") && !e.Name.Contains(".tmp") && !e.Name.Contains("~$") &&
                    e.Name.Contains("."))
                {
                    Catalog.ShowInfo($"桌面文件变动 Type:{e.ChangeType.ToString()}", e.FullPath);
                    if (e.ChangeType != WatcherChangeTypes.Deleted) Catalog.BackupFile(e.FullPath, e.Name);
                }
            }), DispatcherPriority.Normal);
        }

        private void PicTimer_Elapsed(object? sender = null, ElapsedEventArgs e = null)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                string url = $"pack://application:,,,/Resources/HeadPics/{new Random().Next(8)}.jpg";
                head.Source = new BitmapImage(new Uri(url));

                StartAnimation(3, 3600);
            }));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

        private void SecondTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                time.Text = DateTime.Now.ToString("HH:mm:ss");
                time1.Text = DateTime.Now.ToString("HH:mm:ss");
                //longDate.Text = DateTime.Now.ToString("yyyy年MM月dd日 ddd");
                var status = Schedule.GetNowCourse(schedule);
                if (status.nowStatus == CourseNowStatus.EndOfLesson || status.nowStatus == CourseNowStatus.Upcoming)
                {
                    courseCard.Show(status);
                    StartAnimation(10, 3600);
                }

                if (Catalog.settings.OfficeFunctionEnable)
                {
                    if (CheckOfficeTask.Status == TaskStatus.Created) CheckOfficeTask.Start();
                    if (CheckOfficeTask.IsCompleted || CheckOfficeTask.Status == TaskStatus.Canceled ||
                        CheckOfficeTask.Status == TaskStatus.Faulted)
                    {
                        //Log.Information($"CheckOfficeTask Status:{CheckOfficeTask.Status}");
                        CheckOfficeTask = new Task(CheckOffice);
                        CheckOfficeTask.Start();
                    }
                }
            }, DispatcherPriority.Background);
        }

        public void GetCalendarInfo()
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var client = new HttpClient();
                var json = await client.GetStringAsync(
                    $"https://opendata.baidu.com/api.php?tn=wisetpl&format=json&resource_id=39043&query={DateTime.Now.Year}年{DateTime.Now.Month}月");
                var dt = JsonConvert.DeserializeObject<JObject>(json);
                var fes = "";
                if (dt != null)
                    foreach (var item in dt["data"][0]["almanac"])
                    {
                        if (item["month"].ToString() == DateTime.Now.Month.ToString() &&
                            item["day"].ToString() == DateTime.Now.Day.ToString()) fes = item["term"].ToString();
                    }

                longDate.Text = $"{DateTime.Now:yyyy年MM月dd日 ddd} {fes}";
                sw.Stop();
                Log.Information($"获取节假日信息用时:{sw.Elapsed.TotalSeconds}s");
            }, DispatcherPriority.Background);
        }

        public void ToggleCard(bool isForceShow = false)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                DoubleAnimation anim2 = new DoubleAnimation(0, 300, TimeSpan.FromSeconds(1));
                DoubleAnimation anim1 = new DoubleAnimation(300, 0, TimeSpan.FromSeconds(1));
                anim2.Completed += (a, b) => sideCard.Visibility = Visibility.Collapsed;
                anim1.EasingFunction = Catalog.easingFunction;
                anim2.EasingFunction = Catalog.easingFunction;
                if (sideCard.Visibility == Visibility.Collapsed || isForceShow)
                {
                    sideCard.Visibility = Visibility.Visible;
                    cardtran.BeginAnimation(TranslateTransform.XProperty, anim1);
                    anim1.Completed += (a, b) =>
                    {
                        Point floatGridTopLeft = floatGrid.PointToScreen(new Point(0, 0));
                        Point sideCardTopLeft = sideCard.PointToScreen(new Point(0, 0));

                        bool isFullyInside = new Rect(
                            sideCardTopLeft.X,
                            sideCardTopLeft.Y,
                            sideCard.ActualWidth,
                            sideCard.ActualHeight
                        ).Contains(floatGridTopLeft);
                        //Catalog.ShowInfo(isFullyInside.ToString());
                        if (isFullyInside) transT.Y = 0;
                    };
                    //transT.Y = 0;
                }
                else
                {
                    cardtran.BeginAnimation(TranslateTransform.XProperty, anim2);
                    //transT.Y = -100;
                }
            }), DispatcherPriority.Background);
        }

        private void StartAnimation(int time = 2, int angle = 180)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var doubleAnimation = new DoubleAnimation
                {
                    Duration = new Duration(TimeSpan.FromSeconds(time)),
                    EasingFunction = Catalog.easingFunction,
                    //doubleAnimation.From = 0;
                    // doubleAnimation.To = 360;
                    By = angle
                };
                rotateT.BeginAnimation(RotateTransform.AngleProperty, doubleAnimation);
            }), DispatcherPriority.Background);
        }

        public async void IconAnimation(bool isHide = false, SymbolRegular symbol = SymbolRegular.Empty,
            SolidColorBrush bgc = null, int autoHideTime = 0)
        {
            await Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
            {
                var doubleAnimation = new DoubleAnimation
                {
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                    EasingFunction = Catalog.easingFunction
                };
                if (bgc != null) iconE.Fill = bgc;
                else iconE.Fill = new SolidColorBrush() { Color = Colors.White };
                if (symbol != SymbolRegular.Empty) icon.Symbol = symbol;
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
                if (autoHideTime != 0)
                {
                    await Task.Delay(autoHideTime).ContinueWith(t =>
                    {
                        doubleAnimation.From = 1;
                        doubleAnimation.To = 0;
                        iconTrans.BeginAnimation(ScaleTransform.ScaleXProperty, doubleAnimation);
                        iconTrans.BeginAnimation(ScaleTransform.ScaleYProperty, doubleAnimation);
                    });
                }
            }));
        }

        private Stopwatch floatStopwatch = new Stopwatch();

        private void FloatGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                floatStopwatch.Restart();
                isDragging = true;
                startPoint = e.GetPosition(this);
                _mouseDownControlPosition = new Point(transT.X, transT.Y);
                floatGrid.CaptureMouse();
            }));
        }

        private void FloatGrid_MouseMove(object sender, MouseEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (isDragging && floatStopwatch.ElapsedMilliseconds >= 100)
                {
                    var c = sender as Control;
                    var pos = e.GetPosition(this);
                    var dp = pos - startPoint;
                    if (pos.X >= SystemParameters.FullPrimaryScreenWidth - 10 ||
                        pos.Y >= SystemParameters.FullPrimaryScreenHeight - 10)
                    {
                        isDragging = false;
                        floatGrid.ReleaseMouseCapture();
                        transT.X = -10;
                        transT.Y = -100;
                        return;
                    }

                    transT.X = _mouseDownControlPosition.X + dp.X;
                    transT.Y = _mouseDownControlPosition.Y + dp.Y;
                }
            });
        }

        private void FloatGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                floatStopwatch.Stop();
                StartAnimation();
                isDragging = false;
                floatGrid.ReleaseMouseCapture();
                // Catalog.ShowInfo(floatStopwatch.ElapsedMilliseconds.ToString());
                if (floatStopwatch.ElapsedMilliseconds > 200) return;
                if (Catalog.settings.SideCardEnable) ToggleCard();
                else cardPopup.IsOpen = !cardPopup.IsOpen;
            }), DispatcherPriority.Background);
        }

        private void StuMgr(object sender, RoutedEventArgs e) => Catalog.CreateWindow<StudentMgr>();

        private void StartInk(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                timeMachine.ClearStrokeHistory();
                if (inkTool.Visibility == Visibility.Collapsed || inkTool.isPPT)
                {
                    if (inkTool.isPPT) inkTool.SetCursorMode(0);
                    else inkTool.SetCursorMode(1);
                    Catalog.SetWindowStyle();
                    inkTool.Visibility = Visibility.Visible;
                    IconAnimation(false, SymbolRegular.Pen32);
                }
                else
                {
                    Catalog.SetWindowStyle(1);
                    inkTool.Visibility = Visibility.Collapsed;
                    IconAnimation(true);
                }
            }));
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Log.Information("Program Closing.");
            e.Cancel = true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Log.Information("Program Closed");
        }

        public void CheckBirthDay()
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
            {
                List<Student> students = await StudentExtensions.Load();
                Student? nearest = null;
                int type = 0;
                foreach (var person in students)
                {
                    if (!person.BirthDay.HasValue) continue;
                    string shortBirthStr = person.BirthDay.Value.ToString("MM-dd");

                    if (DateTime.Now.ToString("MM-dd") == shortBirthStr)
                    {
                        nearest = person;
                        type = 1;
                        break;
                    }

                    if (DateTime.Now.AddDays(1).ToString("MM-dd") == shortBirthStr)
                    {
                        nearest = person;
                        type = 2;
                    }
                }

                if (nearest != null)
                {
                    if (type == 1)
                    {
                        birth.IsOpen = true;
                        birth.Message = $"🎉 今天是 {nearest.Name} 的生日！";
                    }
                    else if (type == 2)
                    {
                        birth.IsOpen = true;
                        birth.Message = $"🎉 明天是 {nearest.Name} 的生日！";
                    }
                    else birth.IsOpen = false;
                }
                else birth.IsOpen = false;
            }));
        }

        private void ShowStickys(object sender, RoutedEventArgs e) => Catalog.CreateWindow<Sticky>();

        public void PostNote(object sender, RoutedEventArgs e) => Catalog.ToggleControlVisible(postNote);

        private void VolumeCard(object sender, RoutedEventArgs e)
        {
            cardPopup.IsOpen = false;
            Catalog.ToggleControlVisible(volcd);
        }

        private void ShowRandom(object sender, RoutedEventArgs e) => Catalog.ToggleControlVisible(rancor);

        private async void RandomControl_StartRandom(object sender, RandomEventArgs e)
        {
            var a = await StudentExtensions.Random(e);
            ranres.ItemsSource = a;
            Catalog.ToggleControlVisible(ranres);
        }

        private void CourseMgr(object sender, RoutedEventArgs e) => Catalog.CreateWindow<CourseMgr>();

        private void AddFloatCard(object sender, RoutedEventArgs e) => Catalog.CreateWindow<FloatNote>(true);

        private void OpenSettings(object sender, RoutedEventArgs e) => Catalog.CreateWindow<Settings>();

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            SetToolWindow();
        }

        public void SetToolWindow()
        {
            const int WS_EX_TOOLWINDOW = 0x80;
            // 获取窗口句柄
            IntPtr hwnd = new WindowInteropHelper(this).Handle;

            // 获取当前窗口样式
            int currentStyle = Win32Helper.GetWindowLong(hwnd, -20); // -20 表示 GWL_EXSTYLE

            // 设置窗口样式，去掉 WS_EX_APPWINDOW，添加 WS_EX_TOOLWINDOW
            int newStyle = (currentStyle & ~0x00000040) | WS_EX_TOOLWINDOW;

            // 更新窗口样式
            Win32Helper.SetWindowLong(hwnd, -20, newStyle);
        }

        private void ScreenShot(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                cardPopup.IsOpen = false;
                if (sideCard.Visibility != Visibility.Collapsed) ToggleCard();
                Rectangle rc = SystemInformation.VirtualScreen;
                var bitmap = new Bitmap(rc.Width, rc.Height, PixelFormat.Format32bppArgb);

                using (Graphics memoryGrahics = Graphics.FromImage(bitmap))
                {
                    memoryGrahics.CopyFromScreen(rc.X, rc.Y, 0, 0, rc.Size, CopyPixelOperation.SourceCopy);
                }

                var savePath =
                    $@"{Catalog.SCRSHOT_DIR}\{DateTime.Now:yyyy-MM-dd}\{DateTime.Now:HH-mm-ss)}.png";
                if (!Directory.Exists(Path.GetDirectoryName(savePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                }

                bitmap.Save(savePath, ImageFormat.Png);
                Catalog.ShowInfo("成功保存截图", "路径:" + savePath);
            }));
        }

        private void Button_MouseRightButtonDown(object sender, MouseButtonEventArgs e) => App.Current.Shutdown();

        private void QuickFix(object sender, RoutedEventArgs e) => Catalog.ToggleControlVisible(logview);

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (!inkTool.isPPT) return;
            if (e.Key == Key.PageDown || e.Key == Key.Down) PptDown();
            else if (e.Key == Key.PageUp || e.Key == Key.Up) PptUp();
        }

        #region OfficeObj

        public void PptUp(object? sender = null, RoutedEventArgs? e = null)
        {
            try
            {
                new Thread(() =>
                {
                    if (pptApplication == null) throw new NullReferenceException("ppt对象不存在。");
                    pptApplication.SlideShowWindows[1].Activate();
                    pptApplication.SlideShowWindows[1].View.Previous();
                }).Start();
            }
            catch
            {
                pptControls.Visibility = Visibility.Collapsed;
                inkTool.isPPT = false;
            }
        }

        public void PptDown(object? sender = null, RoutedEventArgs? e = null)
        {
            try
            {
                new Thread(() =>
                {
                    if (pptApplication == null) throw new NullReferenceException("ppt对象不存在。");
                    pptApplication.SlideShowWindows[1].Activate();
                    pptApplication.SlideShowWindows[1].View.Next();
                }).Start();
            }
            catch
            {
                pptControls.Visibility = Visibility.Collapsed;
                inkTool.isPPT = false;
            }
        }

        private void CheckOffice()
        {
            try
            {
                //Log.Information($"CheckOffice Started. TaskStatus:{CheckOfficeTask.Status}");
                if (ProcessHelper.HasPowerPointProcess() && pptApplication == null)
                {
                    pptApplication = (MsPpt.Application)MarshalForCore.GetActiveObject("PowerPoint.Application");

                    if (pptApplication != null)
                    {
                        Catalog.ShowInfo("成功捕获PPT程序对象",
                            pptApplication.Name + "/版本:" + pptApplication.Version + "/PC:" +
                            pptApplication.ProductCode, ControlAppearance.Success);
                        if (!pptApplication.Name.Contains("Microsoft"))
                            Catalog.ShowInfo("警告:不推荐使用WPS。", "高分辨率下WPS无法播放视频。", ControlAppearance.Caution);
                        pptApplication.PresentationClose += PptApplication_PresentationClose;
                        pptApplication.SlideShowBegin += PptApplication_SlideShowBegin;
                        pptApplication.SlideShowNextSlide += PptApplication_SlideShowNextSlide;
                        pptApplication.SlideShowEnd += PptApplication_SlideShowEnd;
                        if (pptApplication.SlideShowWindows.Count >= 1)
                        {
                            PptApplication_SlideShowBegin(pptApplication.SlideShowWindows[1]);
                        }

                        if (pptApplication.Presentations.Count >= 1)
                        {
                            foreach (MsPpt.Presentation pres in pptApplication.Presentations)
                            {
                                Catalog.BackupFile(pres.FullName, pres.Name, pres.IsFullyDownloaded);
                            }
                        }
                    }
                }

                if (ProcessHelper.HasWordProcess() && wordApplication == null && Catalog.settings.FileWatcherEnable)
                {
                    wordApplication = (MsWord.Application)MarshalForCore.GetActiveObject("Word.Application");
                    if (wordApplication != null)
                    {
                        Catalog.ShowInfo("成功捕获Word程序对象",
                            wordApplication.Name + "/版本:" + wordApplication.Version + "/PC:" +
                            wordApplication.ProductCode(), ControlAppearance.Success);
                        wordApplication.DocumentOpen += Doc => { Catalog.BackupFile(Doc.FullName, Doc.Name); };
                        wordApplication.DocumentBeforeClose += (MsWord.Document Doc, ref bool Cancel) =>
                        {
                            Catalog.ShowInfo($"尝试释放 Word 对象");
                            if (wordApplication == null) return;
                            try
                            {
                                Marshal.FinalReleaseComObject(wordApplication);
                            }
                            catch (Exception ex)
                            {
                                Catalog.HandleException(ex, "释放COM对象");
                            }

                            wordApplication = null;
                        };
                        if (wordApplication.Documents.Count > 0)
                        {
                            foreach (MsWord.Document item in wordApplication.Documents)
                            {
                                Catalog.BackupFile(item.FullName, item.Name);
                            }
                        }
                    }
                }

                if (ProcessHelper.HasExcelProcess() && excelApplication == null && Catalog.settings.FileWatcherEnable)
                {
                    excelApplication = (MsExcel.Application)MarshalForCore.GetActiveObject("Excel.Application");
                    if (excelApplication != null)
                    {
                        Catalog.ShowInfo("成功捕获Excel程序对象",
                            excelApplication.Name + "/版本:" + excelApplication.Version + "/PC:" +
                            excelApplication.ProductCode, ControlAppearance.Success);
                        excelApplication.WorkbookOpen += Workbook =>
                        {
                            Catalog.BackupFile(Workbook.FullName, Workbook.Name);
                        };
                        excelApplication.WorkbookBeforeClose += (MsExcel.Workbook Wb, ref bool Cancel) =>
                        {
                            Catalog.ShowInfo($"尝试释放 Excel 对象");
                            if (excelApplication == null) return;
                            try
                            {
                                Marshal.FinalReleaseComObject(excelApplication);
                            }
                            catch (Exception ex)
                            {
                                Catalog.HandleException(ex, "释放COM对象");
                            }

                            excelApplication = null;
                        };
                        if (excelApplication.Workbooks.Count > 0)
                        {
                            foreach (MsExcel.Workbook item in excelApplication.Workbooks)
                            {
                                Catalog.BackupFile(item.FullName, item.Name);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Catalog.HandleException(ex, "COM对象线程");
            }
        }

        private void PptApplication_SlideShowEnd(MsPpt.Presentation Pres)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                page = 0;
                ClearStrokes(true);
                pptControls.Visibility = Visibility.Collapsed;
                Catalog.SetWindowStyle(1);
                inkcanvas.IsEnabled = false;
                inkTool.Visibility = Visibility.Collapsed;
                inkcanvas.Background.Opacity = 0;
                inkTool.isPPT = false;
                Catalog.ShowInfo("放映结束.");
                //IconAnimation(true);
            }), DispatcherPriority.Background);
        }

        private void PptApplication_SlideShowNextSlide(MsPpt.SlideShowWindow Wn)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!inkTool.isPPT) return;
                page = Wn.View.CurrentShowPosition;
                ClearStrokes(true);
                pptPage.Text = $"{Wn.View.CurrentShowPosition}/{Wn.Presentation.Slides.Count}";
                pptPage1.Text = $"{Wn.View.CurrentShowPosition}/{Wn.Presentation.Slides.Count}";
                //if (strokes[page]!=null)inkcanvas.Strokes = strokes[page];
            }), DispatcherPriority.Normal);
        }

        private void PptApplication_PresentationClose(MsPpt.Presentation Pres)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                page = 0;
                pptControls.Visibility = Visibility.Collapsed;
                inkTool.isPPT = false;
                Catalog.ShowInfo($"尝试释放 PPT 对象");
                if (pptApplication == null) return;
                try
                {
                    Marshal.FinalReleaseComObject(pptApplication);
                }
                catch (Exception ex)
                {
                    Catalog.HandleException(ex, "释放COM对象");
                }

                pptApplication = null;
                //IconAnimation(true);
            }), DispatcherPriority.Background);
        }

        private void PptApplication_SlideShowBegin(MsPpt.SlideShowWindow Wn)
        {
            inkTool.isPPT = true;
            //memoryStreams = new MemoryStream[Wn.Presentation.Slides.Count + 2];
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Catalog.ShowInfo("放映已开始.");
                StartInk(null, null);
                inkTool.SetCursorMode(0);
                inkcanvas.Background.Opacity = 0;
                pptControls.Visibility = Visibility.Visible;
                pptPage.Text = $"{Wn.View.CurrentShowPosition}/{Wn.Presentation.Slides.Count}";
                pptPage1.Text = $"{Wn.View.CurrentShowPosition}/{Wn.Presentation.Slides.Count}";
                page = Wn.View.CurrentShowPosition;
                if (pptApplication.Presentations.Count >= 1)
                {
                    foreach (MsPpt.Presentation Pres in pptApplication.Presentations)
                    {
                        Catalog.BackupFile(Pres.FullName, Pres.Name, Pres.IsFullyDownloaded);
                    }
                }
            }), DispatcherPriority.Background);
        }

        #endregion OfficeObj

        #region Multi-Touch

        private void MainWindow_TouchDown(object sender, TouchEventArgs e)
        {
            double boundWidth = e.GetTouchPoint(null).Bounds.Width;
            if (boundWidth > 20)
            {
                inkcanvas.EraserShape = new EllipseStylusShape(boundWidth, boundWidth);
                TouchDownPointsList[e.TouchDevice.Id] = InkCanvasEditingMode.EraseByPoint;
                inkcanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
            }
            else if (!inkTool.isEraser)
            {
                TouchDownPointsList[e.TouchDevice.Id] = InkCanvasEditingMode.None;
                inkcanvas.EditingMode = InkCanvasEditingMode.None;
            }
        }

        private void MainWindow_StylusDown(object sender, StylusDownEventArgs e)
        {
            TouchDownPointsList[e.StylusDevice.Id] = InkCanvasEditingMode.None;
        }

        private void inkcanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            try
            {
                // 检查是否是压感笔书写
                if (e.Stroke.StylusPoints.Any(stylusPoint =>
                        stylusPoint.PressureFactor != 0.5 && stylusPoint.PressureFactor != 0))
                {
                    return;
                }

                double GetPointSpeed(Point point1, Point point2, Point point3)
                {
                    return (Math.Sqrt((point1.X - point2.X) * (point1.X - point2.X) +
                                      (point1.Y - point2.Y) * (point1.Y - point2.Y))
                            + Math.Sqrt((point3.X - point2.X) * (point3.X - point2.X) +
                                        (point3.Y - point2.Y) * (point3.Y - point2.Y)))
                           / 20;
                }

                try
                {
                    if (e.Stroke.StylusPoints.Count > 3)
                    {
                        Random random = new Random();
                        double _speed = GetPointSpeed(
                            e.Stroke.StylusPoints[random.Next(0, e.Stroke.StylusPoints.Count - 1)].ToPoint(),
                            e.Stroke.StylusPoints[random.Next(0, e.Stroke.StylusPoints.Count - 1)].ToPoint(),
                            e.Stroke.StylusPoints[random.Next(0, e.Stroke.StylusPoints.Count - 1)].ToPoint());
                    }
                }
                catch
                {
                }

                try
                {
                    StylusPointCollection stylusPoints = new StylusPointCollection();
                    int n = e.Stroke.StylusPoints.Count - 1;
                    double pressure = 0.1;
                    int x = 10;
                    if (n == 1) return;
                    if (n >= x)
                    {
                        for (int i = 0; i < n - x; i++)
                        {
                            StylusPoint point = new StylusPoint();

                            point.PressureFactor = (float)0.5;
                            point.X = e.Stroke.StylusPoints[i].X;
                            point.Y = e.Stroke.StylusPoints[i].Y;
                            stylusPoints.Add(point);
                        }

                        for (int i = n - x; i <= n; i++)
                        {
                            StylusPoint point = new StylusPoint();

                            point.PressureFactor = (float)((0.5 - pressure) * (n - i) / x + pressure);
                            point.X = e.Stroke.StylusPoints[i].X;
                            point.Y = e.Stroke.StylusPoints[i].Y;
                            stylusPoints.Add(point);
                        }
                    }
                    else
                    {
                        for (int i = 0; i <= n; i++)
                        {
                            StylusPoint point = new StylusPoint();

                            point.PressureFactor = (float)(0.4 * (n - i) / n + pressure);
                            point.X = e.Stroke.StylusPoints[i].X;
                            point.Y = e.Stroke.StylusPoints[i].Y;
                            stylusPoints.Add(point);
                        }
                    }

                    e.Stroke.StylusPoints = stylusPoints;
                }
                catch
                {
                }
            }
            catch
            {
            }
        }

        private void MainWindow_StylusUp(object sender, StylusEventArgs e)
        {
            try
            {
                if (!inkTool.isEraser)
                {
                    inkcanvas.Strokes.Add(GetStrokeVisual(e.StylusDevice.Id).Stroke);
                    inkcanvas.Children.Remove(GetVisualCanvas(e.StylusDevice.Id));
                    inkcanvas_StrokeCollected(inkcanvas,
                        new InkCanvasStrokeCollectedEventArgs(GetStrokeVisual(e.StylusDevice.Id).Stroke));
                }
            }
            catch (Exception ex)
            {
            }

            try
            {
                StrokeVisualList.Remove(e.StylusDevice.Id);
                VisualCanvasList.Remove(e.StylusDevice.Id);
                TouchDownPointsList.Remove(e.StylusDevice.Id);
                if (StrokeVisualList.Count == 0 || VisualCanvasList.Count == 0 || TouchDownPointsList.Count == 0)
                {
                    inkcanvas.Children.Clear();
                    StrokeVisualList.Clear();
                    VisualCanvasList.Clear();
                    TouchDownPointsList.Clear();
                }
            }
            catch
            {
            }
        }

        private void MainWindow_StylusMove(object sender, StylusEventArgs e)
        {
            try
            {
                if (inkTool.isEraser) return;
                if (GetTouchDownPointsList(e.StylusDevice.Id) != InkCanvasEditingMode.None) return;
                try
                {
                    if (e.StylusDevice.StylusButtons[1].StylusButtonState == StylusButtonState.Down) return;
                }
                catch
                {
                }

                var strokeVisual = GetStrokeVisual(e.StylusDevice.Id);
                var stylusPointCollection = e.GetStylusPoints(this);
                foreach (var stylusPoint in stylusPointCollection)
                {
                    strokeVisual.Add(new StylusPoint(stylusPoint.X, stylusPoint.Y, stylusPoint.PressureFactor));
                }

                strokeVisual.ReDraw();
            }
            catch
            {
            }
        }

        private StrokeVisual GetStrokeVisual(int id)
        {
            if (StrokeVisualList.TryGetValue(id, out var visual))
            {
                return visual;
            }

            var strokeVisual = new StrokeVisual(inkcanvas.DefaultDrawingAttributes.Clone());
            StrokeVisualList[id] = strokeVisual;
            StrokeVisualList[id] = strokeVisual;
            var visualCanvas = new VisualCanvas(strokeVisual);
            VisualCanvasList[id] = visualCanvas;
            inkcanvas.Children.Add(visualCanvas);

            return strokeVisual;
        }

        private VisualCanvas GetVisualCanvas(int id)
        {
            if (VisualCanvasList.TryGetValue(id, out var visualCanvas))
            {
                return visualCanvas;
            }

            return null;
        }

        private InkCanvasEditingMode GetTouchDownPointsList(int id)
        {
            if (TouchDownPointsList.TryGetValue(id, out var inkCanvasEditingMode))
            {
                return inkCanvasEditingMode;
            }

            return inkcanvas.EditingMode;
        }

        private Dictionary<int, InkCanvasEditingMode> TouchDownPointsList { get; } =
            new Dictionary<int, InkCanvasEditingMode>();

        private Dictionary<int, StrokeVisual> StrokeVisualList { get; } = new Dictionary<int, StrokeVisual>();
        private Dictionary<int, VisualCanvas> VisualCanvasList { get; } = new Dictionary<int, VisualCanvas>();

        #endregion Multi-Touch

        private StrokeCollection[] strokeCollections = new StrokeCollection[101];
        private bool[] whiteboadLastModeIsRedo = new bool[101];
        private StrokeCollection lastTouchDownStrokeCollection = new StrokeCollection();

        private int CurrentWhiteboardIndex = 1;
        private int WhiteboardTotalCount = 1;
        private TimeMachineHistory[][] TimeMachineHistories = new TimeMachineHistory[101][]; //最多99页，0用来存储非白板时的墨迹以便还原

        private void SaveStrokes(bool isBackupMain = false)
        {
            if (isBackupMain)
            {
                var timeMachineHistory = timeMachine.ExportTimeMachineHistory();
                TimeMachineHistories[0] = timeMachineHistory;
                timeMachine.ClearStrokeHistory();
            }
            else
            {
                var timeMachineHistory = timeMachine.ExportTimeMachineHistory();
                TimeMachineHistories[CurrentWhiteboardIndex] = timeMachineHistory;
                timeMachine.ClearStrokeHistory();
            }
        }

        public void ClearStrokes(bool isErasedByCode)
        {
            _currentCommitType = CommitReason.ClearingCanvas;
            if (isErasedByCode) _currentCommitType = CommitReason.CodeInput;
            inkcanvas.Strokes.Clear();
            _currentCommitType = CommitReason.UserInput;
        }

        private void RestoreStrokes(bool isBackupMain = false)
        {
            try
            {
                if (TimeMachineHistories[CurrentWhiteboardIndex] == null) return; //防止白板打开后不居中
                if (isBackupMain)
                {
                    _currentCommitType = CommitReason.CodeInput;
                    timeMachine.ImportTimeMachineHistory(TimeMachineHistories[0]);
                    foreach (var item in TimeMachineHistories[0])
                    {
                        if (item.CommitType == TimeMachineHistoryType.UserInput)
                        {
                            if (!item.StrokeHasBeenCleared)
                            {
                                foreach (var strokes in item.CurrentStroke)
                                {
                                    if (!inkcanvas.Strokes.Contains(strokes))
                                        inkcanvas.Strokes.Add(strokes);
                                }
                            }
                            else
                            {
                                foreach (var strokes in item.CurrentStroke)
                                {
                                    if (inkcanvas.Strokes.Contains(strokes))
                                        inkcanvas.Strokes.Remove(strokes);
                                }
                            }
                        }
                        else if (item.CommitType == TimeMachineHistoryType.ShapeRecognition)
                        {
                            if (item.StrokeHasBeenCleared)
                            {
                                foreach (var strokes in item.CurrentStroke)
                                {
                                    if (inkcanvas.Strokes.Contains(strokes))
                                        inkcanvas.Strokes.Remove(strokes);
                                }

                                foreach (var strokes in item.ReplacedStroke)
                                {
                                    if (!inkcanvas.Strokes.Contains(strokes))
                                        inkcanvas.Strokes.Add(strokes);
                                }
                            }
                            else
                            {
                                foreach (var strokes in item.CurrentStroke)
                                {
                                    if (!inkcanvas.Strokes.Contains(strokes))
                                        inkcanvas.Strokes.Add(strokes);
                                }

                                foreach (var strokes in item.ReplacedStroke)
                                {
                                    if (inkcanvas.Strokes.Contains(strokes))
                                        inkcanvas.Strokes.Remove(strokes);
                                }
                            }
                        }
                        else if (item.CommitType == TimeMachineHistoryType.Rotate)
                        {
                            if (item.StrokeHasBeenCleared)
                            {
                                foreach (var strokes in item.CurrentStroke)
                                {
                                    if (inkcanvas.Strokes.Contains(strokes))
                                        inkcanvas.Strokes.Remove(strokes);
                                }

                                foreach (var strokes in item.ReplacedStroke)
                                {
                                    if (!inkcanvas.Strokes.Contains(strokes))
                                        inkcanvas.Strokes.Add(strokes);
                                }
                            }
                            else
                            {
                                foreach (var strokes in item.CurrentStroke)
                                {
                                    if (!inkcanvas.Strokes.Contains(strokes))
                                        inkcanvas.Strokes.Add(strokes);
                                }

                                foreach (var strokes in item.ReplacedStroke)
                                {
                                    if (inkcanvas.Strokes.Contains(strokes))
                                        inkcanvas.Strokes.Remove(strokes);
                                }
                            }
                        }
                        else if (item.CommitType == TimeMachineHistoryType.Clear)
                        {
                            if (!item.StrokeHasBeenCleared)
                            {
                                if (item.CurrentStroke != null)
                                {
                                    foreach (var currentStroke in item.CurrentStroke)
                                    {
                                        if (!inkcanvas.Strokes.Contains(currentStroke))
                                            inkcanvas.Strokes.Add(currentStroke);
                                    }
                                }

                                if (item.ReplacedStroke != null)
                                {
                                    foreach (var replacedStroke in item.ReplacedStroke)
                                    {
                                        if (inkcanvas.Strokes.Contains(replacedStroke))
                                            inkcanvas.Strokes.Remove(replacedStroke);
                                    }
                                }
                            }
                            else
                            {
                                if (item.ReplacedStroke != null)
                                {
                                    foreach (var replacedStroke in item.ReplacedStroke)
                                    {
                                        if (!inkcanvas.Strokes.Contains(replacedStroke))
                                            inkcanvas.Strokes.Add(replacedStroke);
                                    }
                                }

                                if (item.CurrentStroke != null)
                                {
                                    foreach (var currentStroke in item.CurrentStroke)
                                    {
                                        if (inkcanvas.Strokes.Contains(currentStroke))
                                            inkcanvas.Strokes.Remove(currentStroke);
                                    }
                                }
                            }
                        }

                        _currentCommitType = CommitReason.UserInput;
                    }
                }
                else
                {
                    _currentCommitType = CommitReason.CodeInput;
                    timeMachine.ImportTimeMachineHistory(TimeMachineHistories[CurrentWhiteboardIndex]);
                    foreach (var item in TimeMachineHistories[CurrentWhiteboardIndex])
                    {
                        if (item.CommitType == TimeMachineHistoryType.UserInput)
                        {
                            if (!item.StrokeHasBeenCleared)
                            {
                                foreach (var strokes in item.CurrentStroke)
                                {
                                    if (!inkcanvas.Strokes.Contains(strokes))
                                        inkcanvas.Strokes.Add(strokes);
                                }
                            }
                            else
                            {
                                foreach (var strokes in item.CurrentStroke)
                                {
                                    if (inkcanvas.Strokes.Contains(strokes))
                                        inkcanvas.Strokes.Remove(strokes);
                                }
                            }
                        }
                        else if (item.CommitType == TimeMachineHistoryType.ShapeRecognition)
                        {
                            if (item.StrokeHasBeenCleared)
                            {
                                foreach (var strokes in item.CurrentStroke)
                                {
                                    if (inkcanvas.Strokes.Contains(strokes))
                                        inkcanvas.Strokes.Remove(strokes);
                                }

                                foreach (var strokes in item.ReplacedStroke)
                                {
                                    if (!inkcanvas.Strokes.Contains(strokes))
                                        inkcanvas.Strokes.Add(strokes);
                                }
                            }
                            else
                            {
                                foreach (var strokes in item.CurrentStroke)
                                {
                                    if (!inkcanvas.Strokes.Contains(strokes))
                                        inkcanvas.Strokes.Add(strokes);
                                }

                                foreach (var strokes in item.ReplacedStroke)
                                {
                                    if (inkcanvas.Strokes.Contains(strokes))
                                        inkcanvas.Strokes.Remove(strokes);
                                }
                            }
                        }
                        else if (item.CommitType == TimeMachineHistoryType.Rotate)
                        {
                            if (item.StrokeHasBeenCleared)
                            {
                                foreach (var strokes in item.CurrentStroke)
                                {
                                    if (inkcanvas.Strokes.Contains(strokes))
                                        inkcanvas.Strokes.Remove(strokes);
                                }

                                foreach (var strokes in item.ReplacedStroke)
                                {
                                    if (!inkcanvas.Strokes.Contains(strokes))
                                        inkcanvas.Strokes.Add(strokes);
                                }
                            }
                            else
                            {
                                foreach (var strokes in item.CurrentStroke)
                                {
                                    if (!inkcanvas.Strokes.Contains(strokes))
                                        inkcanvas.Strokes.Add(strokes);
                                }

                                foreach (var strokes in item.ReplacedStroke)
                                {
                                    if (inkcanvas.Strokes.Contains(strokes))
                                        inkcanvas.Strokes.Remove(strokes);
                                }
                            }
                        }
                        else if (item.CommitType == TimeMachineHistoryType.Clear)
                        {
                            if (!item.StrokeHasBeenCleared)
                            {
                                if (item.CurrentStroke != null)
                                {
                                    foreach (var currentStroke in item.CurrentStroke)
                                    {
                                        if (!inkcanvas.Strokes.Contains(currentStroke))
                                            inkcanvas.Strokes.Add(currentStroke);
                                    }
                                }

                                if (item.ReplacedStroke != null)
                                {
                                    foreach (var replacedStroke in item.ReplacedStroke)
                                    {
                                        if (inkcanvas.Strokes.Contains(replacedStroke))
                                            inkcanvas.Strokes.Remove(replacedStroke);
                                    }
                                }
                            }
                            else
                            {
                                if (item.ReplacedStroke != null)
                                {
                                    foreach (var replacedStroke in item.ReplacedStroke)
                                    {
                                        if (!inkcanvas.Strokes.Contains(replacedStroke))
                                            inkcanvas.Strokes.Add(replacedStroke);
                                    }
                                }

                                if (item.CurrentStroke != null)
                                {
                                    foreach (var currentStroke in item.CurrentStroke)
                                    {
                                        if (inkcanvas.Strokes.Contains(currentStroke))
                                            inkcanvas.Strokes.Remove(currentStroke);
                                    }
                                }
                            }
                        }
                    }

                    _currentCommitType = CommitReason.UserInput;
                }
            }
            catch
            {
            }
        }

        #region TimeMachine

        public enum CommitReason
        {
            UserInput,
            CodeInput,
            ShapeDrawing,
            ShapeRecognition,
            ClearingCanvas,
            Rotate
        }

        public CommitReason _currentCommitType = CommitReason.UserInput;
        private bool IsEraseByPoint => inkcanvas.EditingMode == InkCanvasEditingMode.EraseByPoint;
        private StrokeCollection ReplacedStroke;
        private StrokeCollection AddedStroke;
        private StrokeCollection CuboidStrokeCollection;
        public TimeMachine timeMachine = new TimeMachine();

        private void TimeMachine_OnUndoStateChanged(bool status)
        {
            var result = status ? Visibility.Visible : Visibility.Collapsed;
            inkTool.backBtn.Visibility = result;
            inkTool.backBtn.IsEnabled = status;
        }

        private void TimeMachine_OnRedoStateChanged(bool status)
        {
            var result = status ? Visibility.Visible : Visibility.Collapsed;
            inkTool.redoBtn.Visibility = result;
            inkTool.redoBtn.IsEnabled = status;
        }

        private void QuickFix(object sender, MouseButtonEventArgs e)
        {
            Catalog.CreateWindow<UserLogin>();
        }

        private void StrokesOnStrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            if (_currentCommitType == CommitReason.CodeInput || _currentCommitType == CommitReason.ShapeDrawing) return;
            if (_currentCommitType == CommitReason.Rotate)
            {
                timeMachine.CommitStrokeRotateHistory(e.Removed, e.Added);
                return;
            }

            if ((e.Added.Count != 0 || e.Removed.Count != 0) && IsEraseByPoint)
            {
                if (AddedStroke == null) AddedStroke = new StrokeCollection();
                if (ReplacedStroke == null) ReplacedStroke = new StrokeCollection();
                AddedStroke.Add(e.Added);
                ReplacedStroke.Add(e.Removed);
                return;
            }

            if (e.Added.Count != 0)
            {
                if (_currentCommitType == CommitReason.ShapeRecognition)
                {
                    timeMachine.CommitStrokeShapeHistory(ReplacedStroke, e.Added);
                    ReplacedStroke = null;
                    return;
                }

                timeMachine.CommitStrokeUserInputHistory(e.Added);
                return;
            }

            if (e.Removed.Count != 0)
            {
                if (_currentCommitType == CommitReason.ShapeRecognition)
                {
                    ReplacedStroke = e.Removed;
                    return;
                }

                if (!IsEraseByPoint || _currentCommitType == CommitReason.ClearingCanvas)
                {
                    timeMachine.CommitStrokeEraseHistory(e.Removed);
                }
            }
        }

        #endregion TimeMachine
    }
}