using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Cokee.ClassService.Helper;

using Wpf.Ui.Designer;

namespace Cokee.ClassService.Views.Pages
{
    public partial class CoursesManage : Page
    {
        private Schedule schedule = Schedule.LoadFromJson();
        private ObservableCollection<Course> dayCourses = new ObservableCollection<Course>();

        public CoursesManage()
        {
            InitializeComponent();
            if (!DesignerHelper.IsInDesignMode) Loaded += (a, b) =>
            {
                dayCourses = new ObservableCollection<Course>(schedule.Courses[0]);
                courseControl.ItemsSource = dayCourses;
            };
        }

        private void Confirm(object sender, RoutedEventArgs e)
        {
            Schedule.SaveToJson(schedule);
        }

        private void Cancel(object sender, RoutedEventArgs e) => Catalog.ToggleControlVisible(this);

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            schedule.Courses[comboBox.SelectedIndex] ??= new List<Course> { new Course() };
            dayCourses = new ObservableCollection<Course>(schedule.Courses[comboBox.SelectedIndex]);
            if (courseControl != null && dayCourses != null) courseControl.ItemsSource = dayCourses;
        }

        private void AddCourse(object sender, RoutedEventArgs e)
        {
            var a = new Course("1", comboBox.SelectedIndex);
            dayCourses.Add(a);
            courseControl.ItemsSource = dayCourses;
        }

        private void MoveUp(object sender, RoutedEventArgs e)
        {
            int c = dayCourses.ToList().FindIndex(t => t.IsChecked);
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
            int c = dayCourses.ToList().FindIndex(t => t.IsChecked);
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
            var c = dayCourses.ToList().Find(t => t.IsChecked);
            if (c != null) dayCourses.Remove(c);
            courseControl.ItemsSource = dayCourses;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var a = dayCourses.ToList().FindAll(t => t.IsChecked);
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