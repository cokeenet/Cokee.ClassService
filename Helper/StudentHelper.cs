using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

using Cokee.ClassService.Shared;

using Newtonsoft.Json;

using Serilog;

using Wpf.Ui.Common;

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
                    return ControlAppearance.Transparent;

                case 1:
                    return ControlAppearance.Info;

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
        public static List<Student> RandomHistory = new List<Student>();
        public int Count = 1;
        public bool AllowMLang = true, AllowExist;
        public SexCombo SexLimit = SexCombo.None;
        public Easter Easter = Easter.None;

        public RandomEventArgs(int num = 1, bool allowMLang = true, bool allowExist = false, SexCombo sexLimit = SexCombo.None, Easter easter = Easter.None)
        {
            Count = num;
            AllowMLang = allowMLang;
            AllowExist = allowExist;
            SexLimit = sexLimit;
            Easter = easter;
        }

        public override string ToString()
        {
            return $"Count{Count}|MLang{AllowMLang}|Exist{AllowExist}|Sex{SexLimit}|Easter{Easter}";
        }
    }

    public static class StudentExtensions
    {
        public static async Task<List<Student>> Load()
        {
         //  var a = await new ApiClient().GetStudents(0);
          // return a.ToList();
            
            if (!File.Exists(Catalog.STU_FILE)) return new List<Student>();
            var a = JsonConvert.DeserializeObject<List<Student>>(File.ReadAllText(Catalog.STU_FILE));
            if (a != null)
            {
                a.Sort((s1, s2) => s2.Role.CompareTo(s1.Role));
                return a;
            }

            return new List<Student>();
        }

        public static async Task<List<Student>> Save(this List<Student> students)
        {
            students.Sort((s1, s2) => s2.Role.CompareTo(s1.Role));
            foreach (var item in students)
            {
                item.ID = null;
                if (item.QQ != null && item.QQ.ToString().Length >= 5)
                    item.HeadPicUrl = $"https://q.qlogo.cn/g?b=qq&nk={item.QQ}&s=100";
                else item.HeadPicUrl = "/Resources/head.jpg";
               // var a = await new ApiClient().CreateStudentAsync(JsonConvert.SerializeObject(item));
              //  Log.Information(a);
            }

           if (!Directory.Exists(Catalog.CONFIG_DIR)) Directory.CreateDirectory(Catalog.CONFIG_DIR);
            File.WriteAllText(Catalog.STU_FILE, JsonConvert.SerializeObject(students));
            return students;
        }

        public static async Task<List<Student>> Random(RandomEventArgs args)
        {
            List<Student> randoms = new List<Student>();

            List<Student> students = await Load();
            int i = 1;

            #region Easter

            /*try
            {
                if (Easter == "1")
                    randoms.Add(students.Find(t => t.Name == Encoding.UTF8.GetString(Convert.FromBase64String("6Zer5a6d5oCh"))));
                else if (Easter == "2")
                    randoms.Add(students.Find(t => t.Name == Encoding.UTF8.GetString(Convert.FromBase64String("57+f5pix6IiS"))));
            }
            catch (Exception)
            {
            }*/

            #endregion Easter

            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (randoms.Count < args.Count)
            {
                var a = students[new Random().Next(students.Count)];
                if (!args.AllowExist && (randoms.Exists(f => f.Name == a.Name) || RandomEventArgs.RandomHistory.Exists(f => f.Name == a.Name)) && args.Count <= students.Count) continue;
                if (!args.AllowMLang && a.IsMinorLang && args.Count <= students.Count) continue;
                if (args.SexLimit == SexCombo.Boy && a.Sex == Sex.Girl) continue;
                if (args.SexLimit == SexCombo.Girl && a.Sex == Sex.Boy) continue;
                randoms.Add(a);
                i++;
                if (sw.ElapsedMilliseconds >= 3000) { Catalog.ShowInfo("抽取超时."); break; }
            }

            #region easter

            /*if (Easter == "1")
            {
                randoms.RemoveAll(t => t.Name == Encoding.UTF8.GetString(Convert.FromBase64String("57+f5pix6IiS")));
                Easter = "0";
                goto ranStart;
            }*/

            #endregion easter

            //randoms = Catalog.RandomizeList(randoms);
            RandomEventArgs.RandomHistory = RandomEventArgs.RandomHistory.Union(randoms).ToList();
            if (RandomEventArgs.RandomHistory.Count >= students.Count) RandomEventArgs.RandomHistory.Clear();

            return randoms;
        }
    }
}