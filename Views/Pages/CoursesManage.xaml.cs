using Cokee.ClassService.Helper;
using Cokee.ClassService.Views.Windows;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Cokee.ClassService.Views.Pages
{
    public partial class CoursesManage : Page
    {
        private Schedule schedule = Schedule.LoadFromJson();
        private ObservableCollection<Lesson> dayCourses = new ObservableCollection<Lesson>();
        private int lastIndex = 0;

        public CoursesManage()
        {
            InitializeComponent();
            if (!DesignerProperties.GetIsInDesignMode(this)) Loaded += (a, b) =>
            {
                dayCourses = new ObservableCollection<Lesson>(schedule.Monday);
                courseControl.DataContext = dayCourses;
            };
        }

        private void Confirm(object sender, RoutedEventArgs e)
        {
            Schedule.SaveToJson(schedule);
            App.Current.Windows.Cast<CourseMgr>().FirstOrDefault().Close();
        }

        private void Cancel(object sender, RoutedEventArgs e) => Catalog.ToggleControlVisible(this);

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (comboBox.SelectedIndex)
            {
                case 0:
                    dayCourses = new ObservableCollection<Lesson>(schedule.Monday);
                    break;

                case 1:
                    dayCourses = new ObservableCollection<Lesson>(schedule.Tuesday);
                    break;

                case 2:
                    dayCourses = new ObservableCollection<Lesson>(schedule.Wendesday);
                    break;

                case 3:
                    dayCourses = new ObservableCollection<Lesson>(schedule.Thursday);
                    break;

                case 4:
                    dayCourses = new ObservableCollection<Lesson>(schedule.Friday);
                    break;

                case 5:
                    dayCourses = new ObservableCollection<Lesson>(schedule.Saturday);
                    break;

                case 6:
                    dayCourses = new ObservableCollection<Lesson>(schedule.Sunday);
                    break;

                default:
                    break;
            }
            lastIndex = comboBox.SelectedIndex;

            if (courseControl != null && dayCourses != null) courseControl.ItemsSource = dayCourses;
        }

        private void AddCourse(object sender, RoutedEventArgs e)
        {
            var a = new Lesson("1");
            dayCourses.Add(a);
            courseControl.ItemsSource = dayCourses;
        }

        private void MoveUp(object sender, RoutedEventArgs e)
        {
            int c = dayCourses.ToList().FindIndex(t => t.IsChecked);
            if (c != -1)
            {
                Lesson a, b;
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
                Lesson a, b;
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

        private void comboBox_Selected(object sender, RoutedEventArgs e)
        {
        }
    }
}