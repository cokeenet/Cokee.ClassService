using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Cokee.ClassService.Shared;

namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// StudentsViewer.xaml 的交互逻辑
    /// </summary>
    ///
    public partial class StudentsViewer : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource), typeof(IEnumerable), typeof(StudentsViewer), new PropertyMetadata(null));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(
        nameof(Columns), typeof(int), typeof(StudentsViewer), new PropertyMetadata(null));

        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        public EventHandler<Student>? StudentClick, StudentRightClick;

        public StudentsViewer()
        {
            InitializeComponent();
        }

        private void Card_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Border card = sender as Border;
            if (card.Tag is Student && e.ChangedButton != MouseButton.Right)
            {
                StudentClick?.Invoke(sender, (Student)((Border)sender).Tag);
            }
        }

        private void Card_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            StudentRightClick?.Invoke(sender, (Student)((Border)sender).Tag);
        }
    }
}