using Cokee.ClassService.Helper;
using Cokee.ClassService.Views.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;

using Wpf.Ui.Common;
using Wpf.Ui.Controls;

using Clipboard = Wpf.Ui.Common.Clipboard;
using MessageBox = System.Windows.MessageBox;
using Path = System.IO.Path;

namespace Cokee.ClassService.Views.Pages
{
    
    /// <summary>
    /// StudentList.xaml 的交互逻辑
    /// </summary>
    public partial class StudentList : UiPage
    {
        List<Student> students = new List<Student>();
        public StudentList()
        {
            try
            {
                InitializeComponent();
                Application.Current.Windows.OfType<StudentMgr>().FirstOrDefault().RandomEvent += StudentList_RandomEvent;
                studentInfo.EditStudent += StudentInfo_EditStudent;
                students = Student.LoadFromFile(Catalog.STU_FILE);
                students.Sort((s1, s2) => s2.Role.CompareTo(s1.Role));
                Students.ItemsSource = students;
            }
            catch (Exception ex)
            {
                Catalog.ShowWarn(ex);
            }
        }

       

        private void StudentList_RandomEvent(object? sender, bool e) => randomcontrol.Visibility = Visibility.Visible;

        public void SaveData()
        {
            
            Student.SaveToFile(students);
            Catalog.ShowInfo("数据已保存.");
        }
        private void RandomStart(object sender, string e)
        {
            randomres.ItemsSource = Student.Random(e,students);
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
            int index = students.FindIndex(f => f.ID == e.ID);
            if (index!=-1)
            {
                students[index] = e;
                SaveData();
                //Catalog.ShowInfo("saved.");
            }
        }
    }
}
