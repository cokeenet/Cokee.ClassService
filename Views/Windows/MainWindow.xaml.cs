using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Cokee.ClassService.Views.Windows;
namespace Cokee.ClassService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isDragging=false;
        private Point startPoint;

        public MainWindow()
        {
            InitializeComponent();
            this.Width = SystemParameters.WorkArea.Width;
            this.Height = SystemParameters.WorkArea.Height;
            this.Top = SystemParameters.WorkArea.Top;
            this.Left = SystemParameters.WorkArea.Left;
        }

        private void MouseDown(object sender, MouseButtonEventArgs e)
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
        private void Click(object sender, MouseButtonEventArgs e)
        {
            if (!cardPopup.IsOpen) cardPopup.IsOpen = true;
            else cardPopup.IsOpen = false;
        }
        private void StuMgr(object sender, RoutedEventArgs e)
        {
            new StudentMgr().Show();
        }

        private void MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void StartInk(object sender, RoutedEventArgs e)
        {
            if (!inkcanvas.IsEnabled) inkcanvas.IsEnabled = true;
            else inkcanvas.IsEnabled = false;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
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

        }
    }
}
