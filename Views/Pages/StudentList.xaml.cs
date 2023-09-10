using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shapes;

using Cokee.ClassService.Views.Windows;

using Newtonsoft.Json;

using Wpf.Ui.Controls;

using Path = System.IO.Path;

namespace Cokee.ClassService.Views.Pages
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string rolestr = value as string;

            if (!string.IsNullOrEmpty(rolestr))
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class LevelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int role = (int)value;

            if (role == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class Student
    {
        public int ID { get; set; }
        public int Sex { get; set; }
        public string Name { get; set; }
        public int Score { get; set; }
        public DateTime BirthDay { get; set; }
        public string? RoleStr { get; set; }
        public int Role { get;set; }
        public string? Desc { get; set; }
        public bool IsMinorLang { get; set; }
        public Student(string name, int sex, DateTime birth, bool isMinorLang = false)
        {
            ID = new Random().Next(9000000);
            Sex = sex;
            Name = name;
            BirthDay = birth;
            IsMinorLang = isMinorLang;
        }
        public List<Student> LoadFromFile(string path)
        {
            string json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<List<Student>>(json);
        }
        public void SaveToFile(string path, List<Student> students)
        {
            string json = JsonConvert.SerializeObject(students);
            File.WriteAllText(path, json);
        }
    }
    /// <summary>
    /// StudentList.xaml 的交互逻辑
    /// </summary>
    public partial class StudentList : UiPage
    {
        public const string DATA_FILE = "D:\\Program Files (x86)\\CokeeTech\\CokeeClass\\students.json";
        List<Student> students = new List<Student>();
        public StudentList()
        {
            InitializeComponent();
            Application.Current.Windows.OfType<StudentMgr>().FirstOrDefault().RandomEvent += StudentList_RandomEvent;
            if (File.Exists(DATA_FILE)) students = JsonConvert.DeserializeObject<List<Student>>(File.ReadAllText(DATA_FILE));
            else { Directory.CreateDirectory(Path.GetDirectoryName(DATA_FILE)); File.Create(DATA_FILE); }
            Students.ItemsSource = students;
        }

        private void StudentList_RandomEvent(object? sender, bool e) => randomcontrol.Visibility = Visibility.Visible;

        private void RandomStart(object sender, string e)
        {
            string Num = e.Split("|")[0], AllowMLang = e.Split("|")[1], AllowGirl = e.Split("|")[2];
            List<Student> randoms = new List<Student>();
            int i = 1;
            while (i<=Convert.ToInt32(Num))
            {
                var a = students[new Random().Next(students.Count)];
                if (AllowMLang == "0" && a.IsMinorLang) continue;
                else if (AllowGirl == "0" && a.Sex == 0) continue;
                else
                { 
                    randoms.Add(a);
                    i++;
                }
            }
            randomres.ItemsSource=randoms;
            randomres.Visibility = Visibility.Visible;
        }
    }
}
