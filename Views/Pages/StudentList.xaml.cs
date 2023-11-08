using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

using Cokee.ClassService.Helper;
using Cokee.ClassService.Views.Windows;

using Wpf.Ui.Controls;

namespace Cokee.ClassService.Views.Pages
{

    /// <summary>
    /// StudentList.xaml 的交互逻辑
    /// </summary>
    public partial class StudentList : UiPage
    {
        ObservableCollection<Student> students = new ObservableCollection<Student>();
        public StudentList()
        {
            try
            {
                InitializeComponent();
                Application.Current.Windows.OfType<StudentMgr>().FirstOrDefault().RandomEvent += StudentList_RandomEvent;
                studentInfo.EditStudent += StudentInfo_EditStudent;
                students = new ObservableCollection<Student>(Student.LoadFromFile(Catalog.STU_FILE));
                if (students != null)
                {
                    
                    Students.ItemsSource = students;
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
            students = new ObservableCollection<Student>(Student.SaveToFile(students.ToList()));
            Catalog.ShowInfo("数据已保存.");
        }
        private void RandomStart(object sender, string e)
        {
            randomres.ItemsSource = Student.Random(e, students.ToList());
            Catalog.ToggleControlVisible(randomres);
        }

        private void Card_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Card card = sender as Card;
            if (card.Tag is Student)
            {
                Catalog.ToggleControlVisible(studentInfo);
                studentInfo.DataContext = card.Tag;
            }
        }
        private void StudentInfo_EditStudent(object? sender, Student e)
        {
            Student stu1 = null;
            //Catalog.ShowInfo(e.RoleStr.ToString());
            int index = students.ToList().FindIndex(f => f.ID == e.ID);
            if (index != -1)
            { 
                students[index] = e;
                SaveData();
                //Catalog.ShowInfo("saved.");
            }
        }
    }
}
