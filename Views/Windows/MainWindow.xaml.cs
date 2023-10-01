using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;

using Cokee.ClassService.Helper;
using Cokee.ClassService.Views.Controls;
using Cokee.ClassService.Views.Pages;
using Cokee.ClassService.Views.Windows;

using Newtonsoft.Json;

using Serilog;
using Serilog.Sink.AppCenter;

using Wpf.Ui.Mvvm.Services;

using MSO = Microsoft.Office.Interop.PowerPoint;
using Point = System.Windows.Point;

namespace Cokee.ClassService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isDragging = false, isPPT = false;
        private Point startPoint, _mouseDownControlPosition;
        public event EventHandler<bool> RandomEvent;
        private Timer secondTimer = new Timer(1000);
        public static MSO.Application pptApplication = null;
        public static MSO.Presentation presentation = null;
        public static MSO.Slides slides = null;
        private int slidescount;
        public static MSO.Slide slide = null;
        List<StrokeCollection> inkPages = new List<StrokeCollection>();
        Schedule schedule = Schedule.LoadFromJson(Catalog.SCHEDULE_FILE);
        public SnackbarService snackbarService = new SnackbarService();
        public MainWindow()
        {
            InitializeComponent();
            Log.Logger = new LoggerConfiguration()
               .WriteTo.File("log.txt",
              outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
               .WriteTo.AppCenterSink(null, Serilog.Events.LogEventLevel.Information, AppCenterTarget.ExceptionsAsCrashes)
               .WriteTo.RichTextBox(richTextBox)
               .CreateLogger();
            this.Width = SystemParameters.WorkArea.Width;
            this.Height = SystemParameters.WorkArea.Height;
            this.Top = SystemParameters.WorkArea.Top;
            this.Left = SystemParameters.WorkArea.Left;
            secondTimer.Elapsed += SecondTimer_Elapsed;
            secondTimer.Start();
            snackbarService.SetSnackbarControl(snackbar);
        }
        private void PptUp(object sender, RoutedEventArgs e)
        {
            if (isPPT)
            {

            }
        }
        private void PptDown(object sender, RoutedEventArgs e)
        {
            if (isPPT)
            {

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
            var status = Schedule.GetNowCourse(schedule,out a,out b);
            if (status != CourseNowStatus.InProgress || status != CourseNowStatus.OnBreak || status != CourseNowStatus.NoCoursesScheduled) courseCard.Show(status, a, b);
            /*if (ProcessHelper.HasPowerPointProcess())
            {
                Type comType = Type.GetTypeFromProgID("PowerPoint.Application");
                pptApplication = (MSO.Application)Activator.CreateInstance(comType);

                if (pptApplication != null)
                {
                    //获得演示文稿对象
                    presentation = pptApplication.ActivePresentation;
                    pptApplication.PresentationClose += PptApplication_PresentationClose;
                    pptApplication.SlideShowBegin += PptApp_SlideShowBegin;
                    pptApplication.SlideShowNextSlide += PptApplication_SlideShowNextSlide;
                    pptApplication.SlideShowEnd += PptApp_SlideShowEnd;
                    // 获得幻灯片对象集合
                    slides = presentation.Slides;
                    // 获得幻灯片的数量
                    slidescount = slides.Count;
                    // 获得当前选中的幻灯片
                    try
                    {
                        // 在普通视图下这种方式可以获得当前选中的幻灯片对象
                        // 然而在阅读模式下，这种方式会出现异常
                        slide = slides[pptApplication.ActiveWindow.Selection.SlideRange.SlideNumber];
                    }
                    catch
                    {
                        // 在阅读模式下出现异常时，通过下面的方式来获得当前选中的幻灯片对象
                        slide = pptApplication.SlideShowWindows[1].View.Slide;
                    }
                }

                if (pptApplication == null) return;
            }*/
        }
        private void MouseUp(object sender, MouseButtonEventArgs e)
        {
            StartAnimation();
            isDragging = false;
            floatGrid.ReleaseMouseCapture();
            if (!cardPopup.IsOpen) cardPopup.IsOpen = true;
            else cardPopup.IsOpen = false;
        }
        private void StartAnimation(int time=2,int angle=180)
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
                snackbarService.Show($"{pos.ToString()}");
                var dp = pos - startPoint;
                if (pos.X >= SystemParameters.FullPrimaryScreenWidth - 10 || pos.Y >= SystemParameters.FullPrimaryScreenHeight - 10) { isDragging = false; floatGrid.ReleaseMouseCapture(); transT.X = -10; transT.Y = -100;return; }
                transT.X = _mouseDownControlPosition.X + dp.X;
                transT.Y = _mouseDownControlPosition.Y + dp.Y;
            }
        }
        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
        private void StuMgr(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Windows.OfType<StudentMgr>().FirstOrDefault() == null) new StudentMgr().Show();
        }
        private void StartInk(object sender, RoutedEventArgs e)
        {
            if (inkcanvas.IsEnabled == false)
            {
                this.Width = SystemParameters.FullPrimaryScreenWidth;
                this.Height = SystemParameters.FullPrimaryScreenHeight;
                this.Top = 0;
                this.Left = 0;
                inkcanvas.IsEnabled = true;
                inkTool.Visibility = Visibility.Visible;
                inkBg.Opacity = 0.01;
            }
            else
            {
                this.Width = SystemParameters.WorkArea.Width;
                this.Height = SystemParameters.WorkArea.Height;
                this.Top = SystemParameters.WorkArea.Top;
                this.Left = SystemParameters.WorkArea.Left;
                inkcanvas.IsEnabled = false;
                inkTool.Visibility = Visibility.Collapsed;
                inkBg.Opacity = 0;
            }
        }

        private void ShowTime(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
        }

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

        private void PostNote(object sender, RoutedEventArgs e)
        {
            if (postNote.Visibility == Visibility.Collapsed) postNote.Visibility = Visibility.Visible;
            else postNote.Visibility = Visibility.Collapsed;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (volcd.Visibility == Visibility.Collapsed) volcd.Visibility = Visibility.Visible;
            else volcd.Visibility = Visibility.Collapsed;
        }

        private void ShowRandom(object sender, RoutedEventArgs e) => rancor.Visibility = Visibility.Visible;

        private void RandomControl_StartRandom(object sender, string e)
        {
            List<Student> students = new List<Student>();
            Student.LoadFromFile(Catalog.STU_FILE);
            ranres.ItemsSource = Student.Random(e, students);
            ranres.Visibility = Visibility.Visible;
        }
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            hwndSource.AddHook(new HwndSourceHook(usbCard.WndProc));//挂钩
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
