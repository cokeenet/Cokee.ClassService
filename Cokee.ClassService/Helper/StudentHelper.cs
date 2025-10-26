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

namespace Cokee.ClassService.Helper
{
    #region 转换器类（实现数据绑定转换逻辑）

    /// <summary>
    /// 字符串可见性转换器：非空字符串显示，空字符串隐藏
    /// </summary>
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 修复点：将 value 转为字符串，判断是否非空且非空白
            string inputStr = value as string;
            // 使用 string.IsNullOrWhiteSpace 更严谨（排除纯空格的情况）
            return !string.IsNullOrWhiteSpace(inputStr)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 等级到颜色转换器：将角色等级转换为对应的颜色
    /// </summary>
    public class LevelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int role)
                return Colors.Transparent;

            return role switch
            {
                1 => Colors.CornflowerBlue,
                2 => Colors.ForestGreen,
                3 => Colors.DarkRed,
                _ => Colors.Transparent
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 性别与整数转换器：实现Sex枚举和整数之间的双向转换
    /// </summary>
    public class SexToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Sex sex ? (int)sex : Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int intValue && Enum.IsDefined(typeof(Sex), intValue)
                ? (Sex)intValue
                : Binding.DoNothing;
        }
    }

    /// <summary>
    /// 头像URL转换器：处理默认头像路径
    /// </summary>
    public class HeadPicUrlConverter : IValueConverter
    {
        private const string DefaultHeadPath = "/Resources/head.jpg";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var url = value?.ToString();
            return string.IsNullOrEmpty(url) || url == DefaultHeadPath || url == "default"
                ? DefaultHeadPath
                : url;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    #region 随机抽取相关类

    /// <summary>
    /// 随机抽取事件参数：包含抽取规则配置
    /// </summary>
    public class RandomEventArgs
    {
        /// <summary>
        /// 抽取人数
        /// </summary>
        public int Count { get; set; } = 1;

        /// <summary>
        /// 是否允许少数民族
        /// </summary>
        public bool AllowMLang { get; set; } = true;

        /// <summary>
        /// 是否允许重复抽取
        /// </summary>
        public bool AllowExist { get; set; }

        /// <summary>
        /// 性别限制
        /// </summary>
        public SexCombo SexLimit { get; set; } = SexCombo.None;

        public RandomEventArgs(int count = 1, bool allowMLang = true, bool allowExist = false, SexCombo sexLimit = SexCombo.None)
        {
            Count = count;
            AllowMLang = allowMLang;
            AllowExist = allowExist;
            SexLimit = sexLimit;
        }

        public override string ToString()
        {
            return $"Count{Count}|MLang{AllowMLang}|Exist{AllowExist}|Sex{SexLimit}";
        }
    }

    #endregion

    #region 学生数据扩展方法

    /// <summary>
    /// 学生数据处理扩展方法
    /// </summary>
    public static class StudentExtensions
    {
        /// <summary>
        /// 抽取历史记录
        /// </summary>
        public static List<Student> RandomHistory { get; } = new List<Student>();

        /// <summary>
        /// 创建简单班级实例
        /// </summary>
        /// <param name="students">学生列表</param>
        /// <returns>班级实例</returns>
        public static Class CreateSimpleClass(List<Student>? students = null)
        {
            return new Class
            {
                Students = students ?? new List<Student>(),
                Name = string.Empty,
                SchoolName = "阜阳师范大学教育集团",
                Grade = GradeType.High2,
                ID = 0,
                CreatedTime = new DateTime(2022, 09, 01)
            };
        }

        /// <summary>
        /// 从文件加载班级数据
        /// </summary>
        /// <returns>班级实例</returns>
        public static async Task<Class> LoadAsync()
        {
            // 确保目录存在
            FileSystemHelper.DirHelper.MakeExist(Catalog.CLASSES_DIR);

            // 检查学生数据文件是否存在
            if (File.Exists(Catalog.STU_FILE))
            {
                try
                {
                    var json = await File.ReadAllTextAsync(Catalog.STU_FILE);
                    var students = JsonSerializer.Deserialize<List<Student>>(json);
                    return CreateSimpleClass(students);
                }
                catch (Exception ex)
                {
                    Catalog.ShowError("加载学生数据失败", ex.Message);
                    return CreateSimpleClass();
                }
            }

            // 文件不存在时返回空班级
            return CreateSimpleClass();
        }

        /// <summary>
        /// 保存学生列表到文件
        /// </summary>
        /// <param name="students">学生列表</param>
        /// <returns>更新后的班级实例</returns>
        public static async Task<Class> SaveAsync(this List<Student> students)
        {
            if (students == null)
                throw new ArgumentNullException(nameof(students));

            // 排序并更新学生信息
            students.Sort((s1, s2) => s2.Role.CompareTo(s1.Role));
            for (int i = 0; i < students.Count; i++)
            {
                var student = students[i];
                student.ID = i + 1; // ID从1开始

                // 设置头像URL（QQ头像或默认头像）
                if (student.QQ.HasValue && student.QQ.ToString()!.Length >= 5)
                {
                    student.HeadPicUrl = $"https://q.qlogo.cn/g?b=qq&nk={student.QQ}&s=100";
                }
                else
                {
                    student.HeadPicUrl = "default";
                }
            }

            // 保存到文件
            try
            {
                FileSystemHelper.DirHelper.MakeExist(Catalog.CLASSES_DIR);
                var json = JsonSerializer.Serialize(students, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(Catalog.STU_FILE, json);
                Catalog.ShowInfo("学生数据已保存", $"共保存 {students.Count} 名学生信息");
            }
            catch (Exception ex)
            {
                Catalog.ShowError("保存学生数据失败", ex.Message);
            }

            return CreateSimpleClass(students);
        }

        /// <summary>
        /// 随机抽取学生
        /// </summary>
        /// <param name="classInfo">班级信息</param>
        /// <param name="args">抽取规则</param>
        /// <returns>抽取结果</returns>
        public static async Task<List<Student>?> GetRandomAsync(this Class classInfo, RandomEventArgs args)
        {
            if (classInfo?.Students == null || classInfo.Students.Count == 0)
            {
                Catalog.ShowInfo("没有学生数据可抽取");
                return null;
            }

            var students = new List<Student>(classInfo.Students);
            var randoms = new List<Student>();
            var random = new Random();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                while (randoms.Count < args.Count)
                {
                    // 超时时保护（防止无限循环）
                    if (stopwatch.ElapsedMilliseconds > 2000)
                    {
                        Catalog.ShowInfo($"抽取超时（{stopwatch.Elapsed.TotalSeconds:F1}秒）");
                        break;
                    }

                    // 随机选择学生
                    var selected = students[random.Next(students.Count)];

                    // 应用抽取规则过滤
                    if (!args.AllowExist && randoms.Any(s => s.Name == selected.Name))
                        continue;

                    if (!args.AllowExist && RandomHistory.Any(s => s.Name == selected.Name))
                        continue;

                    if (!args.AllowMLang && selected.IsMinorLang)
                        continue;

                    if (args.SexLimit == SexCombo.Boy && selected.Sex == Sex.Girl)
                        continue;

                    if (args.SexLimit == SexCombo.Girl && selected.Sex == Sex.Boy)
                        continue;

                    randoms.Add(selected);
                }

                // 更新历史记录
                RandomHistory.AddRange(randoms);
                if (RandomHistory.Count >= students.Count)
                {
                    Catalog.ShowInfo($"已抽列表溢出（{RandomHistory.Count}人）", "已自动清空历史记录");
                    RandomHistory.Clear();
                }

                return randoms;
            }
            catch (Exception ex)
            {
                Catalog.ShowError("随机抽取失败", ex.Message);
                return null;
            }
        }
    }

    #endregion
}