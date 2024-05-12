using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

using Cokee.ClassService.Helper;
using Cokee.ClassService.Views.Windows;

namespace Cokee.ClassService.Views.Pages
{
    public class EnumToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Enum enumValue)
            {
                return enumValue.ToString();
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class CoursesManage : Page
    {
        private Schedule schedule;

        public CoursesManage()
        {
            InitializeComponent();
            comboBox.ItemsSource = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>().ToList();
            comboBox.SelectedIndex = 0;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            schedule = await ScheduleExt.LoadFromJsonAsync();
            // 初始化并设置数据上下文
            var dayOfWeek = DateTime.Now.DayOfWeek;
            CourseControl.ItemsSource = schedule[dayOfWeek];
        }

        private async void Confirm(object sender, RoutedEventArgs e)
        {
            await schedule.SaveToJsonAsync();
            App.Current.Windows.Cast<CourseMgr>().FirstOrDefault().Close();
        }

        private void Cancel(object sender, RoutedEventArgs e) => Catalog.ToggleControlVisible(this);

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 确保 ComboBox 已选中一个项
            if (comboBox.SelectedItem is DayOfWeek selectedDay)
            {
                CourseControl.ItemsSource = schedule[selectedDay];
            }
        }

        private void AddCourse(object sender, RoutedEventArgs e)
        {
            // 确保 ComboBox 已选中一个项
            if (comboBox.SelectedItem is DayOfWeek selectedDay)
            {
                // 直接从 ComboBox.SelectedItem 获取 DayOfWeek 枚举值

                // 创建一个新的课程实例
                Lesson newLesson = new()
                {
                    Name = "新课程", // 这里可以设置一个默认名称，也可以从 UI 获取
                    StartTime = TimeSpan.Zero, // 设置课程开始时间为午夜（00:00:00）
                    EndTime = TimeSpan.Zero // 设置课程结束时间为午夜（00:00:00）
                };

                // 向选定日期的课程列表中添加新课程
                schedule[selectedDay].Add(newLesson);

                // 更新 CourseControl.ItemsSource 以显示新添加的课程
                // 假设 CourseControl 是一个控件，如 ListView 或 DataGrid，它能够显示课程列表
                CourseControl.ItemsSource = schedule[selectedDay];
            }
        }

        private void MoveUp(object sender, RoutedEventArgs e)
        {
            if (comboBox.SelectedItem is DayOfWeek selectedDay)
            {
                int index = FindCheckedLessonIndex(schedule[selectedDay]);
                if (index > 0)
                {
                    // 交换课程
                    Lesson temp = schedule[selectedDay][index];
                    schedule[selectedDay][index] = schedule[selectedDay][index - 1];
                    schedule[selectedDay][index - 1] = temp;

                    // 更新界面
                    UpdateCourseControlItemsSource(selectedDay);
                }
            }
        }

        private void MoveDown(object sender, RoutedEventArgs e)
        {
            if (comboBox.SelectedItem is DayOfWeek selectedDay)
            {
                int index = FindCheckedLessonIndex(schedule[selectedDay]);
                if (index < schedule[selectedDay].Count - 1)
                {
                    // 交换课程
                    Lesson temp = schedule[selectedDay][index];
                    schedule[selectedDay][index] = schedule[selectedDay][index + 1];
                    schedule[selectedDay][index + 1] = temp;

                    // 更新界面
                    UpdateCourseControlItemsSource(selectedDay);
                }
            }
        }

        private void Delete(object sender, RoutedEventArgs e)
        {
            if (comboBox.SelectedItem is DayOfWeek selectedDay)
            {
                Lesson selectedLesson = schedule[selectedDay].FirstOrDefault(l => l.IsChecked);
                if (selectedLesson != null)
                {
                    schedule[selectedDay].Remove(selectedLesson);

                    // 更新界面
                    UpdateCourseControlItemsSource(selectedDay);
                }
            }
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox.SelectedItem is DayOfWeek selectedDay)
            {
                List<Lesson> checkedLessons = schedule[selectedDay].Where(l => l.IsChecked).ToList();
                if (checkedLessons.Count > 1)
                {
                    CheckBox clickedCheckBox = sender as CheckBox;
                    Lesson lessonToKeepChecked = clickedCheckBox?.DataContext as Lesson;
                    foreach (Lesson lesson in checkedLessons)
                    {
                        if (lesson != lessonToKeepChecked)
                        {
                            lesson.IsChecked = false;
                        }
                    }
                }

                // 更新界面
                UpdateCourseControlItemsSource(selectedDay);
            }
        }

        private int FindCheckedLessonIndex(List<Lesson> lessons)
        {
            return lessons.FindIndex(l => l.IsChecked);
        }

        private void UpdateCourseControlItemsSource(DayOfWeek day)
        {
            CourseControl.ItemsSource = null;
            CourseControl.ItemsSource = schedule[day];
        }
    }
}