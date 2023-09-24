using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

using Cokee.ClassService.Views.Pages;
using Cokee.ClassService.Views.Windows;

using CokeeClass.Views.Controls;

using NAudio.CoreAudioApi;

using Newtonsoft.Json;

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
        /* MSOffice.Application pptApp = new MSOffice.Application();
         SlideShowView pptView;
         List<StrokeCollection> strokes = new List<StrokeCollection>();*/
        public MainWindow()
        {
            InitializeComponent();
            /*pptApp.SlideShowBegin += PptApp_SlideShowBegin;
            pptApp.SlideShowOnNext += PptApp_SlideShowOnNext;
            pptApp.SlideShowOnPrevious += PptApp_SlideShowOnPrevious;
            pptApp.SlideShowEnd += PptApp_SlideShowEnd;*/
            this.Width = SystemParameters.WorkArea.Width;
            this.Height = SystemParameters.WorkArea.Height;
            this.Top = SystemParameters.WorkArea.Top;
            this.Left = SystemParameters.WorkArea.Left;
            secondTimer.Elapsed += SecondTimer_Elapsed;
            secondTimer.Start();
        }
        /*private void PptUp(object sender, RoutedEventArgs e)
        {
            if (isPPT)
            {
                pptView.Previous();
            }
        }

        private void PptDown(object sender, RoutedEventArgs e)
        {
            if (isPPT)
            {
                pptView.Next();
            }
        }
        private void PptApp_SlideShowEnd(Presentation Pres)
        {
            pptControls.Visibility = Visibility.Collapsed;
            pptView = null;
            isPPT = false;
        }

        private void PptApp_SlideShowOnPrevious(SlideShowWindow Wn)
        {
            pptPage.Content = $"{Wn.View.CurrentShowPosition.ToString()}/{Wn.Presentation.Slides.Count}";
        }

        private void PptApp_SlideShowOnNext(SlideShowWindow Wn)
        {
            pptPage.Content = $"{Wn.View.CurrentShowPosition.ToString()}/{Wn.Presentation.Slides.Count}";
        }

        private void PptApp_SlideShowBegin(SlideShowWindow Wn)
        {
            isPPT = true;
            pptControls.Visibility = Visibility.Visible;
            StartInk(null, null);
            pptView = Wn.View;
            pptPage.Content=$"{Wn.View.CurrentShowPosition.ToString()}/{Wn.Presentation.Slides.Count}";
        }*/

        private void SecondTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                time.Text = DateTime.Now.ToString("HH:mm");
            }));
        }
        private void MouseUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            floatGrid.ReleaseMouseCapture();
            DoubleAnimation doubleAnimation = new DoubleAnimation();
            doubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(2));
            doubleAnimation.EasingFunction = new CircleEase();
            doubleAnimation.From = 0;
            doubleAnimation.To = 360;
            rotateT.BeginAnimation(RotateTransform.AngleProperty, doubleAnimation);
            if (!cardPopup.IsOpen) cardPopup.IsOpen = true;
            else cardPopup.IsOpen = false;
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
                var dp = pos - startPoint;
                //var transform = c.RenderTransform as TranslateTransform;
                //if (_mouseDownControlPosition.X + dp.X <= 0 || _mouseDownControlPosition.Y + dp.Y <= 0) MouseUp(null,null);
                transT.X = _mouseDownControlPosition.X + dp.X;
                transT.Y = _mouseDownControlPosition.Y + dp.Y;
            }
        }
        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
        private void StuMgr(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Application.Current.Windows.OfType<StudentMgr>().FirstOrDefault() == null) new StudentMgr().Show();
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
                string DATA_DIR = "D:\\Program Files (x86)\\CokeeTech\\CokeeClass\\ink";
                var dir = new DirectoryInfo(DATA_DIR);
                foreach (FileInfo item in dir.GetFiles("*.ink"))
                {
                    list.Add(new StickyItem(item.Name.Replace(".ink", "")));
                }
                Sclview.Visibility = Visibility.Visible;
                //MessageBox.Show(list[50].Name);
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
            const string DATA_FILE = "D:\\Program Files (x86)\\CokeeTech\\CokeeClass\\students.json";
            List<Student> students = new List<Student>();
            if (File.Exists(DATA_FILE)) students = JsonConvert.DeserializeObject<List<Student>>(File.ReadAllText(DATA_FILE));
            else { Directory.CreateDirectory(Path.GetDirectoryName(DATA_FILE)); File.Create(DATA_FILE); }
            string Num = e.Split("|")[0], AllowMLang = e.Split("|")[1], AllowGirl = e.Split("|")[2], AllowExist = e.Split("|")[3];
            List<Student> randoms = new List<Student>();
            int i = 1;
            while (i <= Convert.ToInt32(Num))
            {
                var a = students[new Random().Next(students.Count)];
                if (randoms.Exists(f => f.Name == a.Name) && AllowExist == "0" && Convert.ToInt32(Num) <= students.Count) continue;
                if (AllowMLang == "0" && a.IsMinorLang) continue;
                else if (AllowGirl == "0" && a.Sex == 0) continue;
                else
                {
                    randoms.Add(a);
                    i++;
                }
            }
            ranres.ItemsSource = randoms;
            ranres.Visibility = Visibility.Visible;
        }


    }
}
