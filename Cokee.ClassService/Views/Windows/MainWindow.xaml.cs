using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
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
using Control = System.Windows.Controls.Control;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isDragging;
        private Point startPoint, _mouseDownControlPosition;
        public Schedule schedule;
        private CapService service;
        private Timer secondTimer = new Timer(1000);
        private Timer picTimer = new Timer(120000);
        public MsPpt.Application? pptApplication;
        public MsWord.Application? wordApplication;
        public MsExcel.Application? excelApplication;

        public FileSystemWatcher desktopWatcher = new FileSystemWatcher(
            Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), Catalog.settings.FileWatcherFilter);

        private Task CheckOfficeTask;

        public MainWindow()
        {
            InitializeComponent();
            Catalog.MainWindow = this;
            rancor.RandomResultControl = ranres;
            inkTool.iccBoard = IccBoard;
            VerStr.Text =
                $"CokeeClass 版本{Catalog.Version?.ToString(4)}";
        }

        private void DisplaySettingsChanged(object? sender, EventArgs e)
        {
            Catalog.SetWindowStyle(Catalog.WindowType);
            transT.X = -10;
            transT.Y = -100;
            UpdateLayout();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await Dispatcher.BeginInvoke(new Action(async () =>
            {
                //schedule = await ScheduleExt.LoadFromJsonAsync();
                Log.Logger = new LoggerConfiguration()
                .WriteTo.File($"D:\\DeviceLogs\\{DateTime.Now:yyyy-MM}\\{DateTime.Now:MM-dd}.txt",
                    outputTemplate:
                    "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .WriteTo.RichTextBox(richTextBox, LogEventLevel.Verbose)
                .CreateLogger();
                if (Catalog.settings.DesktopBgWin)
                    new DesktopWindow().Show();
                Catalog.SetWindowStyle(1);
                SystemEvents.DisplaySettingsChanged += DisplaySettingsChanged;
                DpiChanged += DisplaySettingsChanged;
                SizeChanged += DisplaySettingsChanged;
                secondTimer.Elapsed += SecondTimer_Elapsed;
                secondTimer.Start();
                picTimer.Elapsed += PicTimer_Elapsed;
                picTimer.Start();
                longDate.Text = DateTime.Now.ToString("yyyy年MM月dd日 ddd");
                if (Catalog.settings.AgentEnable) slogan.Foreground = new SolidColorBrush(Colors.Yellow);
                CheckOfficeTask = new Task(CheckOffice);
                if (!Catalog.IsScrSave)
                {
                    HwndSource? hwndSource = PresentationSource.FromVisual(this) as HwndSource;
                    hwndSource?.AddHook(usbCard.WndProc);
                    if (Catalog.settings.AgentEnable) Catalog.CapServiceHost.StartAgent();
                    if (Catalog.settings.FileWatcherEnable) IntiFileWatcher();
                    Catalog.CheckUpdate();
                }
                else
                {
                    tipsText.Visibility = Visibility.Visible;
                    tipsText.Text = "屏保模式";
                }
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

        public async void IntiFileWatcher()
        {
            await Dispatcher.InvokeAsync(new Action(() =>
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

        private async void DesktopWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            await Dispatcher.InvokeAsync(new Action(() =>
            {
                if (!e.Name.Contains(".lnk") && !e.Name.Contains(".tmp") && !e.Name.Contains("~$") &&
                    e.Name.Contains("."))
                {
                    Catalog.ShowInfo($"桌面文件变动 Type:{e.ChangeType.ToString()}", e.FullPath);
                    if (e.ChangeType != WatcherChangeTypes.Deleted) Catalog.BackupFile(e.FullPath, e.Name);
                }
            }), DispatcherPriority.Normal);
        }

        private async void PicTimer_Elapsed(object? sender = null, ElapsedEventArgs e = null)
        {
            await Dispatcher.InvokeAsync(new Action(() =>
            {
                string url = $"pack://application:,,,/Resources/HeadPics/{new Random().Next(8)}.jpg";
                head.ProfilePicture = new BitmapImage(new Uri(url));

                StartAnimation(3, 3600);
            }));
        }

        private void Timer(object sender, RoutedEventArgs e)
        {
            if (ThemeManager.Current.ApplicationTheme == ApplicationTheme.Light)
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Dark;
            else
                ThemeManager.Current.ApplicationTheme = ApplicationTheme.Light;
        }

        private void SecondTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                time.Text = DateTime.Now.ToString("HH:mm:ss");
                time1.Text = DateTime.Now.ToString("HH:mm:ss");
                var status = Win32Helper.IsForegroundMaximized();
                if (status) Catalog.SetWindowStyle(1);
                else Catalog.SetWindowStyle(0);
                if (Catalog.settings.OfficeFunctionEnable)
                {
                    if (CheckOfficeTask.Status == TaskStatus.Created) CheckOfficeTask.Start();
                    if (CheckOfficeTask.IsCompleted || CheckOfficeTask.Status == TaskStatus.Canceled || CheckOfficeTask.Status == TaskStatus.Faulted)
                    {
                        //Log.Information($"CheckOfficeTask Status:{CheckOfficeTask.Status}");
                        CheckOfficeTask = new Task(CheckOffice);
                        CheckOfficeTask.Start();
                    }
                }
            }, DispatcherPriority.Background);
        }

        public async void GetCalendarInfo()
        {
            await Dispatcher.InvokeAsync(async () =>
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var client = new HttpClient();
                var json = await client.GetStringAsync(
                    $"https://opendata.baidu.com/api.php?tn=wisetpl&format=json&resource_id=39043&query={DateTime.Now.Year}年{DateTime.Now.Month}月");
                var dt = JsonConvert.DeserializeObject<JObject>(json);
                var fes = "";
                var longTime = "";
                var suit = "";
                var avoid = "";
                if (dt != null)
                    foreach (var item in dt["data"][0]["almanac"])
                    {
                        if (item["month"].ToString() == DateTime.Now.Month.ToString() && item["day"].ToString() == DateTime.Now.Day.ToString())
                        {
                            fes = item["term"].ToString();
                            longTime = $"{item["gzYear"]}{item["animal"]}年 {item["lMonth"]}月{item["lDate"]}";
                            suit = $"今日宜:{item["suit"]}";
                            avoid = $"今日不宜:{item["avoid"]}";
                        }
                    }

                longDate.Text = $"{DateTime.Now:yyyy年MM月dd日 ddd} {fes}";
                longCHNDate.Text = longTime; //甲辰年(龙年)己巳月丙子日
                c1.Text = suit;
                c2.Text = avoid; exp.Visibility = Visibility.Visible;
                sw.Stop();
                Log.Information($"获取节假日信息用时:{sw.Elapsed.TotalSeconds}s");
            }, DispatcherPriority.Background);
        }

        public void ToggleCard(bool fasthide = false)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                DoubleAnimation anim2 = new DoubleAnimation(0, 300, TimeSpan.FromSeconds(1));
                DoubleAnimation anim1 = new DoubleAnimation(300, 0, TimeSpan.FromSeconds(1));
                anim2.Completed += (a, b) => sideCard.Visibility = Visibility.Collapsed;
                anim1.EasingFunction = Catalog.easingFunction;
                anim2.EasingFunction = Catalog.easingFunction;
                if (fasthide)
                {
                    cardtran.X = 300;
                    sideCard.Visibility = Visibility.Collapsed;
                }
                else if (sideCard.Visibility == Visibility.Visible)
                {
                    cardtran.BeginAnimation(TranslateTransform.XProperty, anim2);
                    //transT.Y = -100;
                }
                else
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
            }), DispatcherPriority.Normal);
        }

        private async void StartAnimation(int time = 2, int angle = 180)
        {
            await Dispatcher.InvokeAsync(new Action(() =>
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

        public async void IconAnimation(bool isHide, FontIconData symbol,
            SolidColorBrush bgc = null, int autoHideTime = 0)
        {
            await Dispatcher.InvokeAsync(new Action(async () =>
            {
                var doubleAnimation = new DoubleAnimation
                {
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                    EasingFunction = Catalog.easingFunction
                };
                if (bgc != null) iconE.Fill = bgc;
                else iconE.Fill = new SolidColorBrush() { Color = Colors.White };
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
            Dispatcher.InvokeAsync(new Action(() =>
            {
                floatStopwatch.Restart();
                isDragging = true;
                startPoint = e.GetPosition(this);
                _mouseDownControlPosition = new Point(transT.X, transT.Y);
                floatGrid.CaptureMouse();
            }));
        }

        private async void FloatGrid_MouseMove(object sender, MouseEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
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
            Dispatcher.Invoke(new Action(() =>
            {
                floatStopwatch.Stop();
                StartAnimation();
                isDragging = false;
                floatGrid.ReleaseMouseCapture();
                var status = schedule.GetNowCourse();
                courseCard.Show(status);
                // Catalog.ShowInfo(floatStopwatch.ElapsedMilliseconds.ToString());
                if (floatStopwatch.ElapsedMilliseconds > 200) return;
                if (Catalog.settings.SideCardEnable) ToggleCard();
                else cardPopup.IsOpen = !cardPopup.IsOpen;
            }), DispatcherPriority.Normal);
        }

        private void StuMgr(object sender, RoutedEventArgs e) => Catalog.CreateWindow<StudentMgr>();

        private async void StartInk(object sender, RoutedEventArgs e)
        {
            await Dispatcher.InvokeAsync(new Action(() =>
            {
                if (inkTool.Visibility == Visibility.Collapsed || inkTool.isPPT)
                {
                    if (inkTool.isPPT) inkTool.SetCursorMode(0);
                    else inkTool.SetCursorMode(1);
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
            }));
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Log.Information("Program Closing.");
            e.Cancel = true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Log.Information($"Program Closed {e.ToString()}");
        }

        public async void CheckBirthDay()
        {
            await Dispatcher.InvokeAsync(new Action(async () =>
            {
                var a = await StudentExtensions.Load();
                List<Student> students = new List<Student>(a.Students);
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

        private void QuickFix(object sender, RoutedEventArgs e)
        {
            new DesktopWindow().Show();

            // Catalog.CreateWindow<UserLogin>();
        }

        private void UsbDebug(object sender, MouseButtonEventArgs e)
        {
            usbCard.EnumDrive();
        }

        private void Grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!Catalog.settings.AgentEnable)
            {
                Catalog.settings.AgentEnable = true;
                Catalog.CapServiceHost.StartAgent();
                slogan.Foreground = new SolidColorBrush(Colors.Yellow);
            }
            else
            {
                Catalog.settings.AgentEnable = false;
                slogan.Foreground = new SolidColorBrush(Colors.Tomato);
                Catalog.CapServiceHost.StopAgent();
            }
            Catalog.settings.Save();
        }

        private void ShowRandom(object sender, RoutedEventArgs e) => Catalog.CreateWindow<RandomWindow>();

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

        private async void ScreenShot(object sender, RoutedEventArgs e)
        {
            await Dispatcher.InvokeAsync(new Action(() =>
            {
                cardPopup.IsOpen = false;
                if (sideCard.Visibility != Visibility.Collapsed) ToggleCard(true);
                Rectangle rc = System.Windows.Forms.SystemInformation.VirtualScreen;
                var bitmap = new Bitmap(rc.Width, rc.Height);

                using (Graphics memoryGrahics = Graphics.FromImage(bitmap))
                {
                    memoryGrahics.CopyFromScreen(rc.X, rc.Y, 0, 0, rc.Size, CopyPixelOperation.SourceCopy);
                }

                var savePath =
                    $@"{Catalog.SCRSHOT_DIR}\{DateTime.Now:yyyy-MM-dd}\{DateTime.Now:HH-mm-ss}.png";
                if (!Directory.Exists(Path.GetDirectoryName(savePath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(savePath));
                }

                bitmap.Save(savePath, ImageFormat.Png);
                Catalog.ShowInfo("成功保存截图", "路径:" + savePath);
            }));
        }

        private void Button_MouseRightButtonDown(object sender, MouseButtonEventArgs e) => App.Current.Shutdown();

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
                new Task(() =>
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
                new Task(() =>
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

        private DateTime lastOfficeErrorTime;
        private int lastOfficeErrorCount = 0;

        private void CheckOffice()
        {
            try
            {
                //Log.Information($"CheckOffice Started. TaskStatus:{CheckOfficeTask.Status}");
                if (ProcessHelper.HasPowerPointProcess())
                {
                    if (pptApplication == null)
                    {
                        pptApplication = (MsPpt.Application)MarshalForCore.GetActiveObject("PowerPoint.Application");
                        if (pptApplication != null)
                        {
                            if (pptApplication.Presentations.Count >= 1)
                            {
                                foreach (MsPpt.Presentation pres in pptApplication.Presentations)
                                {
                                    Catalog.BackupFile(pres.FullName, pres.Name);
                                }
                            }
                            else return;
                            Catalog.ShowInfo("成功捕获PPT程序对象",
                                pptApplication.Name + "/版本:" + pptApplication.Version + "/PC:" +
                                pptApplication.ProductCode, InfoBarSeverity.Success);
                            if (!pptApplication.Name.Contains("Microsoft"))
                                Catalog.ShowInfo("警告:不推荐使用WPS。", "高分辨率下WPS无法播放视频。", InfoBarSeverity.Warning);
                            pptApplication.PresentationClose += PptApplication_PresentationClose;
                            pptApplication.SlideShowBegin += PptApplication_SlideShowBegin;
                            pptApplication.SlideShowNextSlide += PptApplication_SlideShowNextSlide;
                            pptApplication.SlideShowEnd += PptApplication_SlideShowEnd;
                            pptApplication.ProtectedViewWindowOpen += PptApplication_ProtectedViewWindowOpen;
                            if (pptApplication.SlideShowWindows.Count >= 1)
                            {
                                PptApplication_SlideShowBegin(pptApplication.SlideShowWindows[1]);
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
                            wordApplication.ProductCode(), InfoBarSeverity.Success);
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
                            excelApplication.ProductCode, InfoBarSeverity.Success);
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
                if (ex.ToString().Contains("CO_E_CLASSSTRING"))
                {
                    Catalog.ShowInfo("Office未安装或COM对象未注册", "无法使用PPT批注及Office功能，已自动关闭。", InfoBarSeverity.Warning);
                    Catalog.settings.OfficeFunctionEnable = false;
                    Catalog.settings.Save();
                    return;
                }
                lastOfficeErrorCount++;
                if (lastOfficeErrorCount >= 15 && DateTime.Now.Subtract(lastOfficeErrorTime).Minutes <= 10)
                {
                    Catalog.ShowInfo("Office功能错误过多", "已自动关闭Office功能，无法正常使用PPT批注", InfoBarSeverity.Warning);
                    Catalog.settings.OfficeFunctionEnable = false;
                    Catalog.settings.Save();
                    return;
                }
                Catalog.ReleaseComObject(Catalog.MainWindow.wordApplication);
                Catalog.ReleaseComObject(Catalog.MainWindow.pptApplication);
                Catalog.ReleaseComObject(Catalog.MainWindow.excelApplication);
                lastOfficeErrorTime = DateTime.Now;
                Catalog.HandleException(ex, "Office功能");
            }
        }

        private void PptApplication_SlideShowEnd(MsPpt.Presentation Pres)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                IccBoard.CurrentPageItem.InkCanvas.Strokes.Clear();
                pptControls.Visibility = Visibility.Collapsed;
                Catalog.SetWindowStyle(1);
                inkTool.Visibility = Visibility.Collapsed;
                inkTool.isPPT = false;
                Catalog.ShowInfo("放映结束.");
            }), DispatcherPriority.Background);
        }

        private void PptApplication_SlideShowNextSlide(MsPpt.SlideShowWindow Wn)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!inkTool.isPPT) return;
                NowPageText.Text = Wn.View.CurrentShowPosition.ToString();
                TotalPageText.Text = Wn.Presentation.Slides.Count.ToString();
                IccBoard.GotoPage(Wn.View.CurrentShowPosition);
            }), DispatcherPriority.Normal);
        }

        private void PptApplication_PresentationClose(MsPpt.Presentation? Pres)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
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

        private void Button_Click(object sender, RoutedEventArgs e)=>Catalog.ToggleControlVisible(logview);
        private void PptApplication_ProtectedViewWindowOpen(MsPpt.ProtectedViewWindow Wn)
        {
            inkTool.isPPT = true;
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Catalog.ShowInfo("放映已开始.");
                StartInk(null, null);
                inkTool.SetCursorMode(0);
                pptControls.Visibility = Visibility.Visible;
                NowPageText.Text = Wn.Presentation.Slides.Count.ToString();
                TotalPageText.Text = Wn.Presentation.Slides.Count.ToString();
                while (IccBoard.IsCurrentLastPage) IccBoard?.RemovePage();
                //IccBoard.GotoPage(Wn);
                if (pptApplication?.Presentations.Count >= 1)
                {
                    foreach (MsPpt.Presentation Pres in pptApplication.Presentations)
                    {
                        Catalog.BackupFile(Pres.FullName, Pres.Name);
                    }
                }
            }), DispatcherPriority.Background);
        }
        private void PptApplication_SlideShowBegin(MsPpt.SlideShowWindow Wn)
        {
            inkTool.isPPT = true;
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Catalog.ShowInfo("放映已开始.");
                StartInk(null, null);
                inkTool.SetCursorMode(0);
                pptControls.Visibility = Visibility.Visible;
                NowPageText.Text = Wn.View.CurrentShowPosition.ToString();
                TotalPageText.Text = Wn.Presentation.Slides.Count.ToString();
                while (IccBoard.IsCurrentLastPage) IccBoard?.RemovePage();
                IccBoard.GotoPage(Wn.View.CurrentShowPosition);
                if (pptApplication?.Presentations.Count >= 1)
                {
                    foreach (MsPpt.Presentation Pres in pptApplication.Presentations)
                    {
                        Catalog.BackupFile(Pres.FullName, Pres.Name);
                    }
                }
            }), DispatcherPriority.Background);
        }

        #endregion OfficeObj
    }
}