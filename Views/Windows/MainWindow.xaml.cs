using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
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
using Cokee.ClassService.Views.Controls;
using Cokee.ClassService.Views.Windows;

using Microsoft.Win32;

using Wpf.Ui.Common;
using Wpf.Ui.Mvvm.Services;

using MSO = Microsoft.Office.Interop.PowerPoint;
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

        private event EventHandler<bool>? RandomEvent;

        private Timer secondTimer = new Timer(1000);
        private Timer picTimer = new Timer(120000);
        public MSO.Application? pptApplication = null;

        //StrokeCollection[] strokes=new StrokeCollection[101];
        public int page = 0;

        private Schedule schedule = Schedule.LoadFromJson();
        public SnackbarService snackbarService = new SnackbarService();

        public MainWindow()
        {
            InitializeComponent();
            Catalog.SetWindowStyle(1);
            SystemEvents.DisplaySettingsChanged += (a, b) => { Catalog.SetWindowStyle(Catalog.WindowType); transT.X = -10; transT.Y = -100; };
            secondTimer.Elapsed += SecondTimer_Elapsed;
            secondTimer.Start();
            picTimer.Elapsed += PicTimer_Elapsed;
            picTimer.Start();
            snackbarService.SetSnackbarControl(snackbar);
            snackbarService.Timeout = 4000;
            inkTool.inkCanvas = inkcanvas;
            //inkcanvas.StrokeCollected += Inkcanvas_StrokeCollected;
            VerStr.Text = $"CokeeClass 版本{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(4)}";

            Win32Func.SetParent(new WindowInteropHelper(this).Handle, Win32Func.programHandle);
            if (Catalog.appSettings.MultiTouchEnable)
            {
                inkcanvas.StylusDown += MainWindow_StylusDown;
                inkcanvas.StylusMove += MainWindow_StylusMove;
                inkcanvas.StylusUp += MainWindow_StylusUp;
                inkcanvas.TouchDown += MainWindow_TouchDown;
            }
            inkcanvas.StrokeCollected += inkCanvas_StrokeCollected;
            /*if (!Catalog.appSettings.DarkModeEnable) Theme.Apply(ThemeType.Light);
            else Theme.Apply(ThemeType.Dark);*/
            /*var videoDevices = MultimediaUtil.VideoInputNames;// 获取所有视频设备
            string videoName = videoDevices[0];// 选择第一个*/
        }

        private void PicTimer_Elapsed(object? sender = null, ElapsedEventArgs e = null)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
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
                            var a = Student.LoadFromFile(Catalog.STU_FILE);
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

        private void Inkcanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                /*if (page >= 0 && page <= 101)
                {
                    strokes[page] = inkcanvas.Strokes;
                }*/
            }));
        }

        public void PptUp(object sender = null, RoutedEventArgs e = null)
        {
            try
            {
                new Thread(new ThreadStart(() =>
                {
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
                time.Text = Schedule.GetShortTimeStr(DateTime.Now);
                //PicTimer_Elapsed();
            }));
            //Course a, b;
            // var status = Schedule.GetNowCourse(schedule, out a, out b);
            //  if (status == CourseNowStatus.EndOfLesson || status == CourseNowStatus.Upcoming) { courseCard.Show(status, a, b); StartAnimation(10, 3600); }
            if (ProcessHelper.HasPowerPointProcess() && pptApplication == null && Catalog.appSettings.PPTFunctionEnable)
            {
                pptApplication = (MSO.Application)MarshalForCore.GetActiveObject("PowerPoint.Application");
                if (pptApplication != null)
                {
                    Catalog.ShowInfo("成功捕获PPT程序对象", pptApplication.Name + "/版本:" + pptApplication.Version + "/PC:" + pptApplication.ProductCode);
                    pptApplication.PresentationClose += PptApplication_PresentationClose;
                    pptApplication.SlideShowBegin += PptApplication_SlideShowBegin;
                    pptApplication.SlideShowNextSlide += PptApplication_SlideShowNextSlide;
                    pptApplication.SlideShowEnd += PptApplication_SlideShowEnd;
                    pptApplication.AfterPresentationOpen += PptApplication_AfterPresentationOpen;
                    if (pptApplication.SlideShowWindows.Count >= 1)
                    {
                        PptApplication_SlideShowBegin(pptApplication.SlideShowWindows[1]);
                    }
                }

                if (pptApplication == null) return;
            }
        }

        private void PptApplication_AfterPresentationOpen(MSO.Presentation Pres)
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
            }), DispatcherPriority.Normal);
        }

        private void PptApplication_SlideShowEnd(MSO.Presentation Pres)
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
            }), DispatcherPriority.Normal);
        }

        private void PptApplication_SlideShowNextSlide(MSO.SlideShowWindow Wn)
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

        private void PptApplication_PresentationClose(MSO.Presentation Pres)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                page = 0;
                inkcanvas.Strokes.Clear();
                pptControls.Visibility = Visibility.Collapsed;
                pptApplication.PresentationClose -= PptApplication_PresentationClose;
                pptApplication.SlideShowBegin -= PptApplication_SlideShowBegin;
                pptApplication.SlideShowNextSlide -= PptApplication_SlideShowNextSlide;
                pptApplication.SlideShowEnd -= PptApplication_SlideShowEnd;
                pptApplication.AfterPresentationOpen -= PptApplication_AfterPresentationOpen;
                Marshal.ReleaseComObject(pptApplication);
                pptApplication = null;
                inkTool.isPPT = false;
                Catalog.ShowInfo("尝试释放pptApplication对象");
                IconAnimation(false, SymbolRegular.Presenter24);
            }), DispatcherPriority.Normal);
        }

        private void PptApplication_SlideShowBegin(MSO.SlideShowWindow Wn)
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
            if (Application.Current.Windows.OfType<StudentMgr>().FirstOrDefault() == null) new StudentMgr().Show();
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
                    IconAnimation(true, SymbolRegular.Pen32);
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            hwndSource.AddHook(new HwndSourceHook(usbCard.WndProc));//挂钩
        }

        private void CourseMgr(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Windows.OfType<CourseMgr>().FirstOrDefault() == null) new CourseMgr().Show();
        }

        private void AddFloatCard(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Windows.OfType<FloatNote>().FirstOrDefault() == null) new FloatNote().Show();
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Windows.OfType<Settings>().FirstOrDefault() == null) new Settings().Show();
        }

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

        private void QuickFix(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Windows.OfType<QuickFix>().FirstOrDefault() == null) new QuickFix().Show();
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (inkTool.isPPT)
            {
                if (e.Key == Key.PageDown || e.Key == Key.Down) PptDown();
                else if (e.Key == Key.PageUp || e.Key == Key.Up) PptUp();
            }
        }

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
            else
            {
                TouchDownPointsList[e.TouchDevice.Id] = InkCanvasEditingMode.None;
                inkcanvas.EditingMode = InkCanvasEditingMode.None;
            }
        }

        private void MainWindow_StylusDown(object sender, StylusDownEventArgs e)
        {
            TouchDownPointsList[e.StylusDevice.Id] = InkCanvasEditingMode.None;
        }

        private void inkCanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            try
            {
                // 检查是否是压感笔书写
                foreach (StylusPoint stylusPoint in e.Stroke.StylusPoints)
                {
                    if (stylusPoint.PressureFactor != 0.5 && stylusPoint.PressureFactor != 0)
                    {
                        return;
                    }
                }

                double GetPointSpeed(Point point1, Point point2, Point point3)
                {
                    return (Math.Sqrt((point1.X - point2.X) * (point1.X - point2.X) + (point1.Y - point2.Y) * (point1.Y - point2.Y))
                        + Math.Sqrt((point3.X - point2.X) * (point3.X - point2.X) + (point3.Y - point2.Y) * (point3.Y - point2.Y)))
                        / 20;
                }
                try
                {
                    if (e.Stroke.StylusPoints.Count > 3)
                    {
                        Random random = new Random();
                        double _speed = GetPointSpeed(e.Stroke.StylusPoints[random.Next(0, e.Stroke.StylusPoints.Count - 1)].ToPoint(), e.Stroke.StylusPoints[random.Next(0, e.Stroke.StylusPoints.Count - 1)].ToPoint(), e.Stroke.StylusPoints[random.Next(0, e.Stroke.StylusPoints.Count - 1)].ToPoint());
                    }
                }
                catch { }

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
            catch { }
        }

        private void MainWindow_StylusUp(object sender, StylusEventArgs e)
        {
            try
            {
                inkcanvas.Strokes.Add(GetStrokeVisual(e.StylusDevice.Id).Stroke);
                inkcanvas.Children.Remove(GetVisualCanvas(e.StylusDevice.Id));
                inkCanvas_StrokeCollected(inkcanvas, new InkCanvasStrokeCollectedEventArgs(GetStrokeVisual(e.StylusDevice.Id).Stroke));
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
            catch { }
        }

        private void MainWindow_StylusMove(object sender, StylusEventArgs e)
        {
            try
            {
                if (GetTouchDownPointsList(e.StylusDevice.Id) != InkCanvasEditingMode.None) return;
                try
                {
                    if (e.StylusDevice.StylusButtons[1].StylusButtonState == StylusButtonState.Down) return;
                }
                catch { }
                var strokeVisual = GetStrokeVisual(e.StylusDevice.Id);
                var stylusPointCollection = e.GetStylusPoints(this);
                foreach (var stylusPoint in stylusPointCollection)
                {
                    strokeVisual.Add(new StylusPoint(stylusPoint.X, stylusPoint.Y, stylusPoint.PressureFactor));
                }

                strokeVisual.Redraw();
            }
            catch { }
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

        private Dictionary<int, InkCanvasEditingMode> TouchDownPointsList { get; } = new Dictionary<int, InkCanvasEditingMode>();
        private Dictionary<int, StrokeVisual> StrokeVisualList { get; } = new Dictionary<int, StrokeVisual>();
        private Dictionary<int, VisualCanvas> VisualCanvasList { get; } = new Dictionary<int, VisualCanvas>();

        #endregion Multi-Touch
    }
}