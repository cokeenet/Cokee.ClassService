using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;

using Cokee.ClassService.Helper;
using Cokee.ClassService.Views.Controls;
using Cokee.ClassService.Views.Windows;

using Microsoft.Win32;

using Wpf.Ui.Mvvm.Services;

using WPFMediaKit.DirectShow.Controls;

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
        private bool isDragging = false, isPPT = false, createdPPT = false;
        private Point startPoint, _mouseDownControlPosition;
        public event EventHandler<bool> RandomEvent;
        private Timer secondTimer = new Timer(1000);
        public static MSO.Application pptApplication = null;
        MemoryStream[] memoryStreams = new MemoryStream[50];
        public int page = 0;
        Schedule schedule = Schedule.LoadFromJson(Catalog.SCHEDULE_FILE);
        public SnackbarService snackbarService = new SnackbarService();
        public MainWindow()
        {
            InitializeComponent();
            Catalog.SetWindowStyle(this, 0);
            SystemEvents.DisplaySettingsChanged += (a, b) => { Catalog.SetWindowStyle(this, Catalog.WindowStyle); transT.X = -10; transT.Y = -100; };
            secondTimer.Elapsed += SecondTimer_Elapsed;
            secondTimer.Start();
            snackbarService.SetSnackbarControl(snackbar);
            inkcanvas.StrokeCollected += Inkcanvas_StrokeCollected;
            var videoDevices = MultimediaUtil.VideoInputNames;// 获取所有视频设备	 
            string videoName = videoDevices[0];// 选择第一个
        }

        private void Inkcanvas_StrokeCollected(object sender, InkCanvasStrokeCollectedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                //inkcanvas.Strokes.Save(memoryStreams[page]);
            }));
        }

        private void PptUp(object sender, RoutedEventArgs e)
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
            }
        }
        private void PptDown(object sender, RoutedEventArgs e)
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
            }
        }
        /*  private void PptApp_SlideShowEnd(MSO.Presentation Pres)
          {
              pptControls.Visibility = Visibility.Collapsed;
              pptView = null;
              isPPT = false;
          }

          private void PptApp_SlideShowOnPrevious(MSO.SlideShowWindow Wn)
          {
              pptPage.Content = $"{Wn.View.CurrentShowPosition}/{Wn.Presentation.Slides.Count}";
          }

          private void PptApp_SlideShowOnNext(MSO.SlideShowWindow Wn)
          {
              pptPage.Content = $"{Wn.View.CurrentShowPosition}/{Wn.Presentation.Slides.Count}";
          }

          private void PptApp_SlideShowBegin(MSO.SlideShowWindow Wn)
          {
              snackbarService.Show($"{pptApp.SlideShowWindows.Count} {pptApp.Presentations.Count}");
              if (pptApp.SlideShowWindows.Count > 0 && pptApp.Presentations.Count > 0)
              {
                  isPPT = true;
                  pptControls.Visibility = Visibility.Visible;
                  StartInk(null, null);
                  pptView = Wn.View;
                  pptPage.Content = $"{Wn.View.CurrentShowPosition.ToString()}/{Wn.Presentation.Slides.Count}";
              }
          }*/
        /* private void PptApplication_SlideShowNextSlide(MSO.SlideShowWindow Wn)
         {
             throw new NotImplementedException();
         }

         private void PptApplication_PresentationClose(MSO.Presentation Pres)
         {
             pptApplication.PresentationClose -= PptApplication_PresentationClose;
             pptApplication.SlideShowBegin -= PptApp_SlideShowBegin;
             pptApplication.SlideShowNextSlide -= PptApplication_SlideShowNextSlide;
             pptApplication.SlideShowEnd -= PptApplication_SlideShowEnd;
             pptApplication = null;
             Application.Current.Dispatcher.Invoke(() =>
             {
                 BtnPPTSlideShow.Visibility = Visibility.Collapsed;
                 BtnPPTSlideShowEnd.Visibility = Visibility.Collapsed;
             });
         }*/

        private void SecondTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                time.Text = DateTime.Now.ToString("HH:mm");
            }));
            Course a, b;
            var status = Schedule.GetNowCourse(schedule, out a, out b);
            if (status == CourseNowStatus.EndOfLesson || status == CourseNowStatus.Upcoming) { courseCard.Show(status, a, b); StartAnimation(10, 3600); }
            if (ProcessHelper.HasPowerPointProcess() && !createdPPT)
            {
                /*Type comType = Type.GetTypeFromProgID("PowerPoint.Application");
                pptApplication = (MSO.Application)Activator.CreateInstance(comType);*/
                pptApplication = (MSO.Application)MarshalForCore.GetActiveObject("PowerPoint.Application");
                if (pptApplication != null)
                {
                    createdPPT = true;

                    pptApplication.PresentationClose += (a) =>
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            inkcanvas.Strokes.Clear();
                            pptControls.Visibility = Visibility.Collapsed;
                        }));
                        Marshal.ReleaseComObject(pptApplication);
                        pptApplication = null;
                        inkTool.isPPT = false;
                        inkTool.pptApplication = null;
                        createdPPT = false;
                        isPPT = false;

                    };
                    pptApplication.SlideShowBegin += PptApplication_SlideShowBegin;
                    pptApplication.SlideShowNextSlide += (Wn) =>
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            //if(inkcanvas.Strokes.Count>0)inkcanvas.Strokes.Save(memoryStreams[page]);
                            page = Wn.View.CurrentShowPosition;
                            inkcanvas.Strokes.Clear();
                            pptPage.Text = $"{Wn.View.CurrentShowPosition}/{Wn.Presentation.Slides.Count}";
                            pptPage1.Text = $"{Wn.View.CurrentShowPosition}/{Wn.Presentation.Slides.Count}";
                            //if(memoryStreams[page].Length > 0)inkcanvas.Strokes = new StrokeCollection(memoryStreams[page]);
                        }));
                    };
                    pptApplication.SlideShowEnd += (a) =>
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            inkcanvas.Strokes.Clear();
                            pptControls.Visibility = Visibility.Collapsed;
                        }));
                        isPPT = false;
                        StartInk(null, null);
                        inkTool.isPPT = false;
                        inkTool.pptApplication = null;
                    };
                    if (pptApplication.SlideShowWindows.Count > 0 && pptApplication.Presentations.Count > 0) PptApplication_SlideShowBegin(pptApplication.SlideShowWindows[0]);
                }

                if (pptApplication == null) return;
            }
        }

        private void PptApplication_SlideShowBegin(MSO.SlideShowWindow Wn)
        {
            isPPT = true;
            inkTool.isPPT = true;
            inkTool.pptApplication = pptApplication;
            //memoryStreams = new MemoryStream[Wn.Presentation.Slides.Count + 2];
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                StartInk("cursor", null);
                pptControls.Visibility = Visibility.Visible;
                pptPage.Text = $"{Wn.View.CurrentShowPosition}/{Wn.Presentation.Slides.Count}";
            }));
        }

        private void mouseUp(object sender, MouseButtonEventArgs e)
        {
            StartAnimation();
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
        private void mouseDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            startPoint = e.GetPosition(this);
            _mouseDownControlPosition = new Point(transT.X, transT.Y);
            floatGrid.CaptureMouse();
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

                if (inkTool.Visibility == Visibility.Collapsed)
                {
                    if (isPPT)
                    {
                        inkTool.isPPT = true;
                        inkTool.pptApplication = pptApplication;
                        inkTool.SetCursorMode(0);
                    }
                    Catalog.SetWindowStyle(this, 0);
                    inkcanvas.IsEnabled = true;
                    Catalog.ToggleControlVisible(inkTool);
                    inkBg.Opacity = 0.01;
                }
                else
                {
                    Catalog.SetWindowStyle(this, 1);
                    inkcanvas.IsEnabled = false;
                    Catalog.ToggleControlVisible(inkTool);
                    inkBg.Opacity = 0;
                }
            }));
        }

        private void ShowTime(object sender, RoutedEventArgs e) => Environment.Exit(0);

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) => e.Cancel = true;

        private void Window_Closed(object sender, EventArgs e) { }
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

        private void PostNote(object sender, RoutedEventArgs e)
        {
            Catalog.ToggleControlVisible(postNote);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Catalog.ToggleControlVisible(volcd);
        }

        private void ShowRandom(object sender, RoutedEventArgs e) => Catalog.ToggleControlVisible(rancor);

        private void RandomControl_StartRandom(object sender, string e)
        {
            ranres.ItemsSource = Student.Random(e);
            Catalog.ToggleControlVisible(ranres);
        }
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

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
            MainGrid.Children.Add(new FloatCard());
        }

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            int WS_EX_TOOLWINDOW = 0x80;
            // 获取窗口句柄
            IntPtr hwnd = new WindowInteropHelper(this).Handle;

            // 获取当前窗口样式
            int currentStyle = GetWindowLong(hwnd, -20); // -20 表示 GWL_EXSTYLE

            // 设置窗口样式，去掉 WS_EX_APPWINDOW，添加 WS_EX_TOOLWINDOW
            int newStyle = (currentStyle & ~0x00000040) | WS_EX_TOOLWINDOW;

            // 更新窗口样式
            SetWindowLong(hwnd, -20, newStyle);
        }

    }
}
