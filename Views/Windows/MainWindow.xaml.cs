using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using AutoUpdaterDotNET;

using Cokee.ClassService.Helper;
using Cokee.ClassService.Views.Controls;
using Cokee.ClassService.Views.Windows;

using Microsoft.Win32;

using Wpf.Ui.Common;
using Wpf.Ui.Mvvm.Services;

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
        private bool isDragging = false;
        private Point startPoint, _mouseDownControlPosition;

        //private event EventHandler<bool>? RandomEvent;
        private Timer secondTimer = new Timer(1000);

        private Timer picTimer = new Timer(120000);
        public MsPpt.Application? pptApplication = null;
        public MsWord.Application? wordApplication = null;
        public MsExcel.Application? excelApplication = null;
        public FileSystemWatcher desktopWatcher = new FileSystemWatcher(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), Catalog.appSettings.FileWatcherFilter);

        //StrokeCollection[] strokes=new StrokeCollection[101];
        public int page = 0;

        private Schedule schedule = Schedule.LoadFromJson();
        public SnackbarService snackbarService = new SnackbarService();

        public MainWindow()
        {
            InitializeComponent();
            Catalog.GlobalSnackbarService = snackbarService;
            Catalog.SetWindowStyle(1);
            SystemEvents.DisplaySettingsChanged += DisplaySettingsChanged;
            DpiChanged += new DpiChangedEventHandler(DisplaySettingsChanged);
            secondTimer.Elapsed += SecondTimer_Elapsed;
            secondTimer.Start();
            picTimer.Elapsed += PicTimer_Elapsed;
            picTimer.Start();
            snackbarService.SetSnackbarControl(snackbar);
            snackbarService.Timeout = 4000;
            inkTool.inkCanvas = inkcanvas;
            //inkcanvas.StrokeCollected += ;
            VerStr.Text = $"CokeeClass 版本{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(4)}";

            /*if (!Catalog.appSettings.DarkModeEnable) Theme.Apply(ThemeType.Light);
            else Theme.Apply(ThemeType.Dark);*/
            /*var videoDevices = MultimediaUtil.VideoInputNames;// 获取所有视频设备
            string videoName = videoDevices[0];// 选择第一个*/
        }

        private void DisplaySettingsChanged(object? sender, EventArgs e)
        {
            Catalog.SetWindowStyle(Catalog.WindowType);
            transT.X = -10;
            transT.Y = -100;
            this.UpdateLayout();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            hwndSource.AddHook(new HwndSourceHook(usbCard.WndProc));//挂钩
            AutoUpdater.ShowSkipButton = false;
            AutoUpdater.RemindLaterAt = 5;
            AutoUpdater.RemindLaterTimeSpan = RemindLaterFormat.Minutes;
            AutoUpdater.ShowRemindLaterButton = true;
            AutoUpdater.RunUpdateAsAdmin = false;
            AutoUpdater.Start("https://gitee.com/cokee/classservice/raw/master/class_update.xml");
            if (Catalog.appSettings.FileWatcherEnable)
            {
                IntiFileWatcher();
            }
        }

        public void IntiFileWatcher()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                Catalog.ShowInfo($"初始化FileWatcher");
                desktopWatcher.NotifyFilter = NotifyFilters.LastWrite;
                desktopWatcher.Changed += DesktopWatcher_Changed;
                desktopWatcher.Error += (a, b) => { desktopWatcher.EnableRaisingEvents = false; Catalog.HandleException(b.GetException(), "FileWatcher"); };
                desktopWatcher.Created += DesktopWatcher_Changed;
                desktopWatcher.Renamed += DesktopWatcher_Changed;
                //desktopWatcher.Deleted += DesktopWatcher_Changed;
                desktopWatcher.EnableRaisingEvents = true;
            }), DispatcherPriority.Normal);
        }

        private void DesktopWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!e.Name.Contains(".lnk") && !e.Name.Contains(".tmp") && !e.Name.Contains("~$") && e.Name.Contains("."))
                {
                    Catalog.ShowInfo($"桌面文件变动 {e.ChangeType.ToString()}", e.FullPath);
                    if (e.ChangeType != WatcherChangeTypes.Deleted) Catalog.BackupFile(e.FullPath, e.Name);
                }
            }), DispatcherPriority.Normal);
        }

        private void PicTimer_Elapsed(object? sender = null, ElapsedEventArgs e = null)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!Catalog.appSettings.UseMemberAvatar)
                {
                    nameBadge.Visibility = Visibility.Collapsed;
                    string url = $"pack://application:,,,/Resources/HeadPics/{new Random().Next(8)}.jpg";
                    head.Source = new BitmapImage(new Uri(url));
                }
                else
                {
                    new Thread(new ThreadStart(() =>
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            var a = Student.Load();
                            var b = a[new Random().Next(a.Count)];
                            if (b.HeadPicUrl.StartsWith("http"))
                                head.Source = new BitmapImage(new Uri(b.HeadPicUrl));
                            else return;
                            nameBadge.Visibility = Visibility.Visible;
                            nameBadge.Content = $"{b.Name} 的头像";
                        }));
                    })).Start();
                }
                StartAnimation(3, 3600);
            }));
        }

        public void PptUp(object sender = null, RoutedEventArgs e = null)
        {
            try
            {
                new Thread(new ThreadStart(() =>
                {
                    if (pptApplication == null) throw new NullReferenceException("ppt对象不存在。");
                    pptApplication.SlideShowWindows[1].Activate();
                    pptApplication.SlideShowWindows[1].View.Previous();
                })).Start();
            }
            catch
            {
                pptControls.Visibility = Visibility.Collapsed;
                inkTool.isPPT = false;
            }
        }

        public void PptDown(object sender = null, RoutedEventArgs e = null)
        {
            try
            {
                new Thread(new ThreadStart(() =>
                {
                    if (pptApplication == null) throw new NullReferenceException("ppt对象不存在。");
                    pptApplication.SlideShowWindows[1].Activate();
                    pptApplication.SlideShowWindows[1].View.Next();
                })).Start();
            }
            catch
            {
                pptControls.Visibility = Visibility.Collapsed;
                inkTool.isPPT = false;
            }
        }

        private void SecondTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                time.Text = DateTime.Now.ToString("HH:mm:ss");
                //PicTimer_Elapsed();
                //Course a, b;
                // var status = Schedule.GetNowCourse(schedule, out a, out b);
                //  if (status == CourseNowStatus.EndOfLesson || status == CourseNowStatus.Upcoming) { courseCard.Show(status, a, b); StartAnimation(10, 3600); }
                if (ProcessHelper.HasPowerPointProcess() && pptApplication == null && Catalog.appSettings.PPTFunctionEnable)
                {
                    pptApplication = (MsPpt.Application)MarshalForCore.GetActiveObject("PowerPoint.Application");

                    if (pptApplication != null)
                    {
                        Catalog.ShowInfo("成功捕获PPT程序对象", pptApplication.Name + "/版本:" + pptApplication.Version + "/PC:" + pptApplication.ProductCode);
                        if (!pptApplication.Name.Contains("Microsoft")) Catalog.ShowInfo("警告:不推荐使用WPS。", "高分辨率下WPS无法播放视频。");
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
                            foreach (MsPpt.Presentation Pres in pptApplication.Presentations)
                            {
                                Catalog.BackupFile(Pres.FullName, Pres.Name, Pres.IsFullyDownloaded);
                            }
                        }
                    }
                    //if (pptApplication == null) return;
                }
                if (ProcessHelper.HasWordProcess() && wordApplication == null)
                {
                    wordApplication = (MsWord.Application)MarshalForCore.GetActiveObject("Word.Application");
                    if (wordApplication != null)
                    {
                        Catalog.ShowInfo("成功捕获Word程序对象", wordApplication.Name + "/版本:" + wordApplication.Version + "/PC:" + wordApplication.ProductCode());
                        if (wordApplication.Documents.Count > 0)
                        {
                            foreach (MsWord.Document item in wordApplication.Documents)
                            {
                                Catalog.BackupFile(item.FullName, item.Name);
                            }
                        }
                        wordApplication.DocumentOpen += (Doc) =>
                        {
                            Catalog.BackupFile(Doc.FullName, Doc.Name);
                        };
                        wordApplication.DocumentBeforeClose += (MsWord.Document Doc, ref bool Cancel) =>
                        {
                            Catalog.ShowInfo("尝试释放Word对象");
                            try { Marshal.ReleaseComObject(wordApplication); }
                            catch { }
                            wordApplication = null;
                        };
                    }
                }
                if (ProcessHelper.HasExcelProcess() && excelApplication == null)
                {
                    excelApplication = (MsExcel.Application)MarshalForCore.GetActiveObject("Excel.Application");
                    if (excelApplication != null)
                    {
                        Catalog.ShowInfo("成功捕获Excel程序对象", excelApplication.Name + "/版本:" + excelApplication.Version + "/PC:" + excelApplication.ProductCode);
                        if (excelApplication.Workbooks.Count > 0)
                        {
                            foreach (MsExcel.Workbook item in excelApplication.Workbooks)
                            {
                                Catalog.BackupFile(item.FullName, item.Name);
                            }
                        }
                        excelApplication.WorkbookOpen += (Workbook) =>
                        {
                            Catalog.BackupFile(Workbook.FullName, Workbook.Name);
                        };
                        excelApplication.WorkbookBeforeClose += (MsExcel.Workbook Wb, ref bool Cancel) =>
                        {
                            Catalog.ShowInfo("尝试释放Excel对象");
                            try { Marshal.ReleaseComObject(excelApplication); }
                            catch { }
                            excelApplication = null;
                        };
                    }
                }
            }), DispatcherPriority.Normal);
        }

        private void PptApplication_PresentationOpen(MsPpt.Presentation Pres)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                Catalog.ShowInfo(Pres.FullName);
            }), DispatcherPriority.Normal);
        }

        private void PptApplication_SlideShowEnd(MsPpt.Presentation Pres)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                page = 0;
                inkcanvas.Strokes.Clear();
                pptControls.Visibility = Visibility.Collapsed;
                Catalog.SetWindowStyle(1);
                inkcanvas.IsEnabled = false;
                inkTool.Visibility = Visibility.Collapsed;
                inkcanvas.Background.Opacity = 0;
                inkTool.isPPT = false;
                Catalog.ShowInfo("放映结束.");
                IconAnimation(true);
            }), DispatcherPriority.Background);
        }

        private void PptApplication_SlideShowNextSlide(MsPpt.SlideShowWindow Wn)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!inkTool.isPPT) return;
                page = Wn.View.CurrentShowPosition;
                inkcanvas.Strokes.Clear();
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
                pptApplication.PresentationClose -= PptApplication_PresentationClose;
                pptApplication.SlideShowBegin -= PptApplication_SlideShowBegin;
                pptApplication.SlideShowNextSlide -= PptApplication_SlideShowNextSlide;
                pptApplication.SlideShowEnd -= PptApplication_SlideShowEnd;
                pptApplication.PresentationOpen -= PptApplication_PresentationOpen;
                inkTool.isPPT = false;
                Catalog.ShowInfo("尝试释放PowerPoint对象");
                IconAnimation(true);
                try
                {
                    Marshal.ReleaseComObject(pptApplication);
                }
                catch
                {
                }
                pptApplication = null;
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
                        Catalog.ShowInfo($"尝试备份文件。", $"{Pres.FullName}");
                        if (File.Exists(Pres.FullName) && Pres.IsFullyDownloaded)
                        {
                            if (!Directory.Exists(Catalog.CONFIG_DIR + "\\PPTs")) Directory.CreateDirectory(Catalog.CONFIG_DIR + "\\PPTs");
                            File.Copy(Pres.FullName, Catalog.CONFIG_DIR + "\\PPTs\\" + Pres.Name, true);
                        }
                    }
                }
            }), DispatcherPriority.Normal);
        }

        private void mouseUp(object sender, MouseButtonEventArgs e)
        {
            //StartAnimation();
            IconAnimation(true);
            PicTimer_Elapsed();
            isDragging = false;
            floatGrid.ReleaseMouseCapture();
            if (cardPopup.IsOpen) cardPopup.IsOpen = false;
            else cardPopup.IsOpen = true;
        }

        private void StartAnimation(int time = 2, int angle = 180)
        {
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(time));
            doubleAnimation.EasingFunction = new CircleEase();
            //doubleAnimation.From = 0;
            // doubleAnimation.To = 360;
            doubleAnimation.By = angle;
            rotateT.BeginAnimation(RotateTransform.AngleProperty, doubleAnimation);
        }

        public async void IconAnimation(bool isHide = false, SymbolRegular symbol = SymbolRegular.Info12, int autoHideTime = 0)
        {
            await Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
            {
                DoubleAnimation doubleAnimation = new DoubleAnimation();
                doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(0.5));
                doubleAnimation.EasingFunction = new CircleEase();
                icon.Symbol = symbol;
                if (!isHide)
                {
                    doubleAnimation.From = 0;
                    doubleAnimation.To = 1;
                }
                else
                {
                    doubleAnimation.From = 1;
                    doubleAnimation.To = 0;
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

        private void mouseDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            startPoint = e.GetPosition(this);
            _mouseDownControlPosition = new Point(transT.X, transT.Y);
            floatGrid.CaptureMouse();
            IconAnimation(false, SymbolRegular.ArrowMove24);
        }

        private void mouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                var c = sender as Control;
                var pos = e.GetPosition(this);
                //snackbarService.Show($"{pos.ToString()}");
                var dp = pos - startPoint;
                if (pos.X >= SystemParameters.FullPrimaryScreenWidth - 10 || pos.Y >= SystemParameters.FullPrimaryScreenHeight - 10) { isDragging = false; floatGrid.ReleaseMouseCapture(); transT.X = -10; transT.Y = -100; return; }
                transT.X = _mouseDownControlPosition.X + dp.X;
                transT.Y = _mouseDownControlPosition.Y + dp.Y;
            }
        }

        private void StuMgr(object sender, RoutedEventArgs e)
        {
            Catalog.CreateWindow<StudentMgr>();
        }

        private void StartInk(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (inkTool.Visibility == Visibility.Collapsed || inkTool.isPPT)
                {
                    if (inkTool.isPPT) inkTool.SetCursorMode(0);
                    else inkTool.SetCursorMode(1);
                    Catalog.SetWindowStyle(0);
                    inkcanvas.IsEnabled = true;
                    inkTool.Visibility = Visibility.Visible;
                    IconAnimation(false, SymbolRegular.Pen32);
                }
                else
                {
                    Catalog.SetWindowStyle(1);
                    inkcanvas.IsEnabled = false;
                    inkTool.Visibility = Visibility.Collapsed;
                    inkcanvas.Background.Opacity = 0;
                    IconAnimation(true);
                }
            }));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => e.Cancel = true;

        private void Window_Closed(object sender, EventArgs e)
        {
        }

        private void ShowStickys(object sender, RoutedEventArgs e)
        {
            List<StickyItem> list = new List<StickyItem>();
            if (Sclview.Visibility == Visibility.Collapsed)
            {
                var dir = new DirectoryInfo(Catalog.INK_DIR);
                foreach (FileInfo item in dir.GetFiles("*.ink"))
                {
                    list.Add(new StickyItem(item.Name.Replace(".ink", "")));
                }
                Sclview.Visibility = Visibility.Visible;
                Stickys.ItemsSource = list;
            }
            else Sclview.Visibility = Visibility.Collapsed;
        }

        private void PostNote(object sender, RoutedEventArgs e) => Catalog.ToggleControlVisible(postNote);

        private void VolumeCard(object sender, RoutedEventArgs e)
        {
            cardPopup.IsOpen = false;
            Catalog.ToggleControlVisible(volcd);
        }

        private void ShowRandom(object sender, RoutedEventArgs e) => Catalog.ToggleControlVisible(rancor);

        private void RandomControl_StartRandom(object sender, string e)
        {
            ranres.ItemsSource = Student.Random(e);
            Catalog.ToggleControlVisible(ranres);
        }

        private void CourseMgr(object sender, RoutedEventArgs e) => Catalog.CreateWindow<CourseMgr>();

        private void AddFloatCard(object sender, RoutedEventArgs e) => Catalog.CreateWindow<FloatNote>();

        private void OpenSettings(object sender, RoutedEventArgs e) => Catalog.CreateWindow<Settings>();

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            SetToolWindow();
        }

        public void SetToolWindow()
        {
            int WS_EX_TOOLWINDOW = 0x80;
            // 获取窗口句柄
            IntPtr hwnd = new WindowInteropHelper(this).Handle;

            // 获取当前窗口样式
            int currentStyle = Win32Func.GetWindowLong(hwnd, -20); // -20 表示 GWL_EXSTYLE

            // 设置窗口样式，去掉 WS_EX_APPWINDOW，添加 WS_EX_TOOLWINDOW
            int newStyle = (currentStyle & ~0x00000040) | WS_EX_TOOLWINDOW;

            // 更新窗口样式
            Win32Func.SetWindowLong(hwnd, -20, newStyle);
        }

        private void inkcanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            eraser.Visibility = Visibility.Collapsed;
        }

        private void inkcanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (inkTool.isEraser)
            {
                Point mousePosition = e.GetPosition(this);
                TranslateTransform translate = (TranslateTransform)eraser.RenderTransform;
                eraserTrans.X = mousePosition.X - eraser.ActualWidth / 2;
                eraserTrans.Y = mousePosition.Y - eraser.ActualHeight / 2;
            }
            else eraser.Visibility = Visibility.Collapsed;
        }

        private void inkcanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (inkTool.isEraser && Catalog.appSettings.EraseByPointEnable)
                eraser.Visibility = Visibility.Visible;
            else eraser.Visibility = Visibility.Collapsed;
        }

        private void ScreenShot(object sender, RoutedEventArgs e)
        {
            cardPopup.IsOpen = false;
            Rectangle rc = System.Windows.Forms.SystemInformation.VirtualScreen;
            var bitmap = new Bitmap(rc.Width, rc.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics memoryGrahics = Graphics.FromImage(bitmap))
            {
                memoryGrahics.CopyFromScreen(rc.X, rc.Y, 0, 0, rc.Size, CopyPixelOperation.SourceCopy);
            }
            var savePath =
                $@"{Catalog.SCRSHOT_DIR}\{DateTime.Now.Date:yyyyMMdd}\{DateTime.Now.ToString("HH-mm-ss")}.png";
            if (!Directory.Exists(Path.GetDirectoryName(savePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            }

            bitmap.Save(savePath, ImageFormat.Png);
            Catalog.ShowInfo("成功保存截图", "路径:" + savePath);
        }

        private void Button_MouseRightButtonDown(object sender, MouseButtonEventArgs e) => Environment.Exit(0);

        private void QuickFix(object sender, RoutedEventArgs e) => Catalog.CreateWindow<QuickFix>();

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (inkTool.isPPT)
            {
                if (e.Key == Key.PageDown || e.Key == Key.Down) PptDown();
                else if (e.Key == Key.PageUp || e.Key == Key.Up) PptUp();
            }
        }
    }
}