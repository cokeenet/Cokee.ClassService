using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Cokee.ClassService.Helper;
using Cokee.ClassService.Views.Windows;

using Serilog;

using Wpf.Ui.Controls;

using MessageBox = System.Windows.MessageBox;

namespace Cokee.ClassService.Views.Pages
{
    /// <summary>
    /// StudentList.xaml 的交互逻辑
    /// </summary>
    public partial class StudentList : UiPage
    {
        private ObservableCollection<Student> students = new ObservableCollection<Student>();
        private int RightClickCount = 0;

        public StudentList()
        {
            try
            {
                InitializeComponent();
                Application.Current.Windows.OfType<StudentMgr>().FirstOrDefault().RandomEvent += StudentList_RandomEvent;
                studentInfo.EditStudent += StudentInfo_EditStudent;
                students = new ObservableCollection<Student>(Student.Load());
                if (students != null)
                {
                    Students.ItemsSource = students;
                    stuCount.Text = $"共 {students.Count} 名学生";
                }
            }
            catch (Exception ex)
            {
                Catalog.HandleException(ex);
            }
        }

        private void StudentList_RandomEvent(object? sender, bool e) => Catalog.ToggleControlVisible(randomcontrol);

        public void SaveData()
        {
            students = new ObservableCollection<Student>(Student.Save(students.ToList()));
            Students.ItemsSource = students;
            Catalog.ShowInfo("数据已保存.");
        }

        private async void RandomStart(object sender, RandomEventArgs e)
        {
            randomres.ItemsSource = Student.Random(e);
            Catalog.ToggleControlVisible(randomres);
        }

        private void Card_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Card card = sender as Card;
            if (card.Tag is Student && e.ChangedButton != MouseButton.Right)
            {
                Catalog.ToggleControlVisible(studentInfo);
                studentInfo.DataContext = card.Tag;
            }
        }

        private void StudentInfo_EditStudent(object? sender, Student e)
        {
            int index = students.ToList().FindIndex(f => f.ID == e.ID);
            if (index != -1) students[index] = e;
            else students.Add(e);
            SaveData();
        }

        private void Card_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            RightClickCount++;
            if (RightClickCount >= 10)
            {
                RightClickCount = 0;
                Student stu = (sender as Card).Tag as Student;
                if (MessageBox.Show("确定删除？", "确定删除？\n删除记录将保存.", MessageBoxButton.YesNoCancel) == MessageBoxResult.Yes)
                {
                    Log.Warning($"Tried to DELETE Student {stu.Name} ID {stu.ID}");
                    students.Remove(stu);
                    SaveData();
                }
            }
            else Catalog.ShowInfo($"{RightClickCount}", $"Click More {10 - RightClickCount} Times to Delete It.");
        }
    }
}