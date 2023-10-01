using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

using Wpf.Ui.Common;
using Newtonsoft.Json;
using System.IO;

namespace Cokee.ClassService.Helper
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

            switch (role)
            {
                case 0:
                    return ControlAppearance.Transparent;
                case 1:
                    return ControlAppearance.Secondary;
                case 2:
                    return ControlAppearance.Success;
                case 3:
                    return ControlAppearance.Danger;
                default: return ControlAppearance.Info;
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
        public int Sex { get; set; }//0 girl 1 boy
        public string Name { get; set; }
        public int Score { get; set; }
        public DateTime? BirthDay { get; set; }//can be delete
        public string? RoleStr { get; set; }
        public int Role { get; set; } //0-3
        public string? Desc { get; set; }
        public string? QQ { get; set; }
        public bool IsMinorLang { get; set; }
        public string HeadPicUrl { get; set; } = "/Resources/head.jpg";
        public Student(string name, int sex, DateTime birth, bool isMinorLang = false)
        {
            ID = new Random().Next(9000000);
            Sex = sex;
            Name = name;
            BirthDay = birth;
            IsMinorLang = isMinorLang;

        }
        public static List<Student> LoadFromFile(string path)
        {
            if (!File.Exists(path)) return null;
            return JsonConvert.DeserializeObject<List<Student>>(File.ReadAllText(path));
        }
        public static void SaveToFile(List<Student> students)
        {
            if(!Directory.Exists(Catalog.CONFIG_DIR))Directory.CreateDirectory(Catalog.CONFIG_DIR);
            File.WriteAllText(Catalog.STU_FILE, JsonConvert.SerializeObject(students));
        }
        public static List<Student> Random(string e,List<Student> students)
        {
            string Num = e.Split("|")[0], AllowMLang = e.Split("|")[1], AllowGirl = e.Split("|")[2], AllowExist = e.Split("|")[3];
            List<Student> randoms = new List<Student>();
            int i = 1;
            while (i <= Convert.ToInt32(Num))
            {
                var a = students[new Random().Next(students.Count)];
                if (randoms.Exists(f => f.Name == a.Name) && AllowExist == "0" && Convert.ToInt32(Num) <= students.Count) continue;
                if (AllowMLang == "0" && a.IsMinorLang) continue;
                else if (AllowGirl == "0" && a.Sex == 0) continue;
                else
                {
                    randoms.Add(a);
                    i++;
                }
            }
            return randoms;
        }
    }
}
