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
                MessageBox.Show(ex.ToString());
                Clipboard.SetText(ex.ToString());
            }
        }

       

        private void StudentList_RandomEvent(object? sender, bool e) => randomcontrol.Visibility = Visibility.Visible;

        public void SaveData()
        {
            students.Sort((s1, s2) => s2.Role.CompareTo(s1.Role));
            foreach (var item in students)
            {
                if (item.QQ != null&&item.QQ.Length >= 6) 
                    item.HeadPicUrl = $"https://q.qlogo.cn/g?b=qq&nk={item.QQ}&s=100";
            }
            string json = JsonConvert.SerializeObject(students);
            File.WriteAllText(Catalog.STU_FILE, json);
            MessageBox.Show("数据已保存.");
        }
        private void RandomStart(object sender, string e)
        {
            string Num = e.Split("|")[0], AllowMLang = e.Split("|")[1], AllowGirl = e.Split("|")[2],AllowExist= e.Split("|")[3];
            List<Student> randoms = new List<Student>();
            int i = 1;
            while (i <= Convert.ToInt32(Num))
            {
                var a = students[new Random().Next(students.Count)];
                if (randoms.Exists(f => f.Name == a.Name) && AllowExist == "0") continue;
                if (AllowMLang == "0" && a.IsMinorLang) continue;
                else if (AllowGirl == "0" && a.Sex == 0) continue;
                else
                {
                    randoms.Add(a);
                    i++;
                }
            }
            randomres.ItemsSource = randoms;
            randomres.Visibility = Visibility.Visible;
        }

        private void Card_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Card card = sender as Card;
            if (card.Tag is Student)
            {
                studentInfo.Visibility = Visibility.Visible;
                studentInfo.DataContext = card.Tag;
            }
        }
        private void StudentInfo_EditStudent(object? sender, Student e)
        {
            Student stu1 = null;
            //MessageBox.Show(e.RoleStr.ToString());
            int index = students.FindIndex(f => f.ID == e.ID);
            if (index!=-1)
            {
                students[index] = e;
                SaveData();
                //MessageBox.Show("saved.");
            }
        }
    }
}
