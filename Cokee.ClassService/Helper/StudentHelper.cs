using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

using Cokee.ClassService.Shared;

using Sex = Cokee.ClassService.Shared.Sex;
using Student = Cokee.ClassService.Shared.Student;

namespace Cokee.ClassService.Helper
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? rolestr = value as string;

            if (!string.IsNullOrEmpty(rolestr))
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
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
                    return Colors.Transparent;
                // return ControlAppearance.Transparent;

                case 1:
                    return Colors.CornflowerBlue;
                //return ControlAppearance.Info;

                case 2:
                    return Colors.ForestGreen;
                //return ControlAppearance.Success;

                case 3:
                    return Colors.DarkRed;
                //return ControlAppearance.Danger;

                default:
                    return Colors.Transparent;
                    //return ControlAppearance.Info;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class SexToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Sex sex)
            {
                return (int)sex;
            }
            return Binding.DoNothing; // or return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return (Sex)intValue;
            }
            return Binding.DoNothing; // or return null;
        }
    }

    public class HeadPicUrlConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.ToString() == "/Resources/head.jpg" || value.ToString() == "default")
            {
                return "/Resources/head.jpg";
            }
            else
            {
                // 返回HeadPicUrl值，表示不需要转换
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RandomEventArgs
    {
        public int Count = 1;
        public bool AllowMLang = true, AllowExist;
        public SexCombo SexLimit = SexCombo.None;

        public RandomEventArgs(int num = 1, bool allowMLang = true, bool allowExist = false, SexCombo sexLimit = SexCombo.None)
        {
            Count = num;
            AllowMLang = allowMLang;
            AllowExist = allowExist;
            SexLimit = sexLimit;
        }

        public override string ToString()
        {
            return $"Count{Count}|MLang{AllowMLang}|Exist{AllowExist}|Sex{SexLimit}";
        }
    }

    public static class StudentExtensions
    {
        public static List<Student> RandomHistory = new List<Student>();

        public static Class CreateSimpleClass(List<Student>? stu = null)
        {
            return new Class()
            {
                Students = stu,
                Name = "",
                SchoolName = "阜阳师范大学教育集团",
                Grade = GradeType.High2,
                ID = 0,
                CreatedTime = new DateTime(2022, 09, 01)
            };
        }

        public static async Task<Class> Load()
        {
            //  var a = await new ApiClient().GetStudents(0);
            // return a.ToList();
            FileSystemHelper.DirHelper.MakeExist(Catalog.CLASSES_DIR);
            //var list = Directory.GetFiles(Catalog.CLASSES_DIR, "*.json");
            List<Student>? stu = new List<Student>();
            //if (list.Length == 0)
            //{
            //OLD Mode
            if (File.Exists(Catalog.STU_FILE))
            {
                stu = JsonSerializer.Deserialize<List<Student>>(File.ReadAllText(Catalog.STU_FILE));
                return CreateSimpleClass(stu);
            }
            else return new Class();
            //}
            //var a = JsonSerializer.Deserialize<List<Student>>(File.ReadAllText(list[0]));
            /*if (a != null)
            {
                a.Sort((s1, s2) => s2.Role.CompareTo(s1.Role));
                return CreateSimpleClass(a);
            }
            else return CreateSimpleClass();*/
        }

        public static async Task<Class> Save(this List<Student> students)
        {
            students.Sort((s1, s2) => s2.Role.CompareTo(s1.Role));
            int i = 0;
            foreach (var item in students)
            {
                i++;
                item.ID = i;
                if (item.QQ != null && item.QQ.ToString().Length >= 5)
                    item.HeadPicUrl = $"https://q.qlogo.cn/g?b=qq&nk={item.QQ}&s=100";
                else item.HeadPicUrl = "default";
                // var a = await new ApiClient().CreateStudentAsync(JsonConvert.SerializeObject(item));
                //  Log.Information(a);
            }
            FileSystemHelper.DirHelper.MakeExist(Catalog.CLASSES_DIR);
            File.WriteAllText(Catalog.STU_FILE, JsonSerializer.Serialize(students));
            return CreateSimpleClass(students);
        }

        public static async Task<List<Student>> GetRandom(Class c, RandomEventArgs args)
        {
            List<Student> randoms = new List<Student>();
            List<Student> students = new List<Student>(c.Students);
            int i = 1;
            Random random = new Random();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            if (students.Count <= 0) return null;
                while (randoms.Count < args.Count)
                {
                    if (sw.ElapsedMilliseconds >= 3000) { Catalog.ShowInfo($"抽取超时.({sw.Elapsed.TotalSeconds}s)"); break; }

                    var a = students[random.Next(students.Count)];
                    if (!args.AllowExist && randoms.Exists(f => f.Name == a.Name)) continue;
                    if (!args.AllowExist && RandomHistory.Exists(f => f.Name == a.Name)) continue;
                    if (!args.AllowMLang && a.IsMinorLang && args.Count <= students.Count) continue;
                    if (args.SexLimit == SexCombo.Boy && a.Sex == Sex.Girl) continue;
                    if (args.SexLimit == SexCombo.Girl && a.Sex == Sex.Boy) continue;
                    randoms.Add(a);
                    i++;
                }

            //randoms = Catalog.RandomizeList(randoms);
            RandomHistory = RandomHistory.Union(randoms).ToList();
            if (RandomHistory.Count >= students.Count) { Catalog.ShowInfo($"已抽列表溢出。({RandomHistory.Count}个)", "已自动清除。"); RandomHistory.Clear(); }
            return randoms;
        }
    }
}