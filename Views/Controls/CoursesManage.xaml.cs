using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Cokee.ClassService.Helper;

using Wpf.Ui.Common;

namespace Cokee.ClassService.Views.Controls
{

    public partial class CoursesManage : UserControl
    {
        Schedule schedule = Schedule.LoadFromJson();
        List<Course> dayCourses = new List<Course>();
        public CoursesManage()
        {
            InitializeComponent();
            if(!DesignerHelper.IsInDesignMode)this.Loaded += (a, b) =>
            {
                dayCourses = schedule.Courses[0];
                courseControl.ItemsSource = dayCourses;
            };
        }

        private void Confirm(object sender, RoutedEventArgs e)
        {
            Schedule.SaveToJson(schedule);
            Catalog.ToggleControlVisible(this);
        }

        private void Cancel(object sender, RoutedEventArgs e) => Catalog.ToggleControlVisible(this);

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dayCourses = schedule.Courses[comboBox.SelectedIndex];
            if(courseControl!=null) courseControl.ItemsSource = dayCourses;
        }

        private void AddCourse(object sender, RoutedEventArgs e)
        {
        //    dayCourses.Add(new Course("语文", comboBox.SelectedIndex));
            courseControl.ItemsSource = dayCourses;
        }

        private void MoveUp(object sender, RoutedEventArgs e)
        {
            int c = dayCourses.FindIndex(t => t.IsChecked == true);
            if (c != -1)
            {
                Course a, b;
                a = dayCourses[c];
                b = dayCourses[c - 1];
                dayCourses[c - 1] = a;
                dayCourses[c] = b;
            }
            courseControl.ItemsSource = dayCourses;
        }

        private void MoveDown(object sender, RoutedEventArgs e)
        {
            int c = dayCourses.FindIndex(t => t.IsChecked == true);
            if (c != -1)
            {
                Course a, b;
                a = dayCourses[c];
                b = dayCourses[c + 1];
                dayCourses[c + 1] = a;
                dayCourses[c] = b;
            }
            courseControl.ItemsSource = dayCourses;
        }

        private void Delete(object sender, RoutedEventArgs e)
        {
            var c = dayCourses.Find(t => t.IsChecked == true);
            if(c!=null) dayCourses.Remove(c);
            courseControl.ItemsSource = dayCourses;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var a = dayCourses.FindAll(t => t.IsChecked == true);
            if (a.Count > 1)
            {
                foreach (var item in a.Skip(1))
                {
                    item.IsChecked = false;
                }
            }
            courseControl.ItemsSource = dayCourses;
        }
    }
}
