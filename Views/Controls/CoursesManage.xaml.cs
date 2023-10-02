using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Cokee.ClassService.Helper;

namespace Cokee.ClassService.Views.Controls
{

    public partial class CoursesManage : UserControl
    {
        Schedule schedule = Schedule.LoadFromJson(Catalog.SCHEDULE_FILE);
        List<Course> courses = new List<Course>();
        public CoursesManage()
        {
            InitializeComponent();
            this.Loaded += (a, b) =>
            {
                courses = Schedule.GetCourses(schedule, 0);
                courseControl.ItemsSource = courses;
            };
        }

        private void Confirm(object sender, RoutedEventArgs e)
        {

            Catalog.ToggleControlVisible(this);
        }

        private void Cancel(object sender, RoutedEventArgs e) => Catalog.ToggleControlVisible(this);

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            courses = Schedule.GetCourses(schedule, comboBox.SelectedIndex);
            if(courseControl!=null) courseControl.ItemsSource = courses;
        }

        private void AddCourse(object sender, RoutedEventArgs e)
        {
            courses.Add(new Course(""));
        }

        private void MoveUp(object sender, RoutedEventArgs e)
        {
            int c = courses.FindIndex(t => t.IsChecked == true);
            if (c != -1)
            {
                Course a, b;
                a = courses[c];
                b = courses[c - 1];
                courses[c - 1] = a;
                courses[c] = b;
            }
        }

        private void MoveDown(object sender, RoutedEventArgs e)
        {
            int c = courses.FindIndex(t => t.IsChecked == true);
            if (c != -1)
            {
                Course a, b;
                a = courses[c];
                b = courses[c + 1];
                courses[c + 1] = a;
                courses[c] = b;
            }
        }

        private void Delete(object sender, RoutedEventArgs e)
        {
            var c = courses.Find(t => t.IsChecked == true);
            if(c!=null)courses.Remove(c);
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var a = courses.FindAll(t => t.IsChecked == true);
            if (a.Count > 1)
            {
                foreach (var item in a.Skip(1))
                {
                    item.IsChecked = false;
                }
            }
                
        }
    }
}
