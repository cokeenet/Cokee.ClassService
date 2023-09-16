using Cokee.ClassService.Views.Windows;
using CokeeClass.Views.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Cokee.ClassService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isDragging = false;
        private Point startPoint;
        private Timer secondTimer = new Timer(1000);
        public MainWindow()
        {
            InitializeComponent();
            this.Width = SystemParameters.WorkArea.Width;
            this.Height = SystemParameters.WorkArea.Height;
            this.Top = SystemParameters.WorkArea.Top;
            this.Left = SystemParameters.WorkArea.Left;
            secondTimer.Elapsed += SecondTimer_Elapsed;
            secondTimer.Start();
        }

        private void SecondTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() => 
            {
                time.Text = DateTime.Now.ToString("hh:mm");
            }));
        }

        private void mouseDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            startPoint = e.GetPosition(sender as Grid);
        }
        private void mouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                var grid = sender as Grid;
                var currentPosition = e.GetPosition(grid);
                double offsetX = currentPosition.X - startPoint.X;
                double offsetY = currentPosition.Y - startPoint.Y;

                // 更新位移变换
                transT.X += offsetX;
                transT.Y += offsetY;

                // 更新起始点
                startPoint = currentPosition;
            }
        }
        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
        private void StuMgr(object sender, RoutedEventArgs e)
        {
            new StudentMgr().Show();
        }

        private void MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!cardPopup.IsOpen) cardPopup.IsOpen = true;
            else cardPopup.IsOpen = false;
        }

        private void StartInk(object sender, RoutedEventArgs e)
        {
            if (inkGrid.Visibility==Visibility.Collapsed)
            {
                inkGrid.Visibility = Visibility.Visible;

            }
            else 
            {
                inkGrid.Visibility = Visibility.Collapsed;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            List<StickyItem> list = new List<StickyItem>();
            if (Sclview.Visibility == Visibility.Collapsed)
            {
                string DATA_DIR = "D:\\Program Files (x86)\\CokeeTech\\CokeeDP\\ink";
                var dir = new DirectoryInfo(DATA_DIR);
                foreach (FileInfo item in dir.GetFiles())
                {
                    list.Add(new StickyItem(item.Name.Replace(".ink","")));
                }
                Sclview.Visibility = Visibility.Visible;
                MessageBox.Show(list[50].Name);
                Stickys.ItemsSource = list;
            }
            else Sclview.Visibility = Visibility.Collapsed;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (postNote.Visibility == Visibility.Collapsed) postNote.Visibility = Visibility.Visible;
            else postNote.Visibility = Visibility.Collapsed;
        }
    }
}
