using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows.Data;

using Newtonsoft.Json;
namespace Cokee.ClassService.Helper
{
    public class TimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan)
            {
                return timeSpan.ToString(@"hh\:mm");
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue)
            {
                if (TimeSpan.TryParse(strValue, out TimeSpan timeSpan))
                {
                    return timeSpan;
                }
            }
            return TimeSpan.Zero;
        }
    }
    public class Course
    {
        public string Name { get; set; } = "";
        public int DayOfWeek { get; set; } = 0;//0-6
        public TimeSpan StartTime { get; set; }  = TimeSpan.Zero;
        public TimeSpan EndTime { get; set; } = TimeSpan.Zero;
        [JsonIgnore]
        public bool IsChecked { get; set; } = false;
        public Course(string name="",int dayOfWeek=0)
        {
            Name = name;
            DayOfWeek = dayOfWeek;
        }   
    }

    public class Schedule
    {
        public List<Course> Courses { get; set; }
        public static void SaveToJson(Schedule schedule, string filePath)
        {
            var json = JsonConvert.SerializeObject(schedule, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        // 从 JSON 文件加载 Schedule 对象
        public static Schedule LoadFromJson(string filePath)
        {
            if (!File.Exists(filePath)) return null;
            var json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<Schedule>(json);
        }
        // 获取指定星期几的课程列表
        public static List<Course> GetCourses(Schedule schedule,int dayOfWeek)
        {
            var courses = new List<Course>();
            if (schedule == null) return courses;
            foreach (var course in schedule.Courses)
            {
                if ((int)course.DayOfWeek == dayOfWeek)
                {
                    courses.Add(course);
                }
            }
            return courses;
        }
        public static void SetCourses(Schedule schedule, int dayOfWeek, List<Course> courses)
        {
            var course = new List<Course>();
            if (schedule == null) return;
            foreach (var cours in schedule.Courses)
            {
                if ((int)cours.DayOfWeek == dayOfWeek)
                {
                    course.Add(cours);
                }
            }
        }
        public static CourseNowStatus GetNowCourse(Schedule schedule, out Course course,out Course nextCourse)
        {
            course = null; // 初始化 course
            nextCourse = null;
            if (schedule == null) return CourseNowStatus.NoCoursesScheduled;
            DateTime now = DateTime.Now;
            var coursesToday = Schedule.GetCourses(schedule,(int)now.DayOfWeek);
            // 遍历课程列表，查找当前时间所在的课程
            foreach (var c in coursesToday)
            {
                if (now.TimeOfDay >= c.StartTime && now.TimeOfDay <= c.EndTime)
                {
                    course = c; // 找到当前课程
                    nextCourse = coursesToday[coursesToday.IndexOf(c) + 1];
                    if (now.TimeOfDay == c.StartTime)
                    {
                        return CourseNowStatus.Upcoming; // 上课时间点
                    }
                    else if (now.TimeOfDay == c.EndTime)
                    {
                        return CourseNowStatus.EndOfLesson; // 下课时间点
                    }
                    else
                    {
                        return CourseNowStatus.InProgress; // 正在上课
                    }
                }
                else if (now.TimeOfDay < c.StartTime)
                {
                    return CourseNowStatus.OnBreak; // 正在休息
                }
            }
            // 如果没有当前课程，则返回没有课程了
            return CourseNowStatus.NoCoursesScheduled;
        }

    }
    public enum CourseNowStatus
    {
        /// <summary>
        /// 上课时间点
        /// </summary>
        Upcoming,

        /// <summary>
        /// 下课时间点
        /// </summary>
        EndOfLesson,

        /// <summary>
        /// 正在上课
        /// </summary>
        InProgress,

        /// <summary>
        /// 正在休息
        /// </summary>
        OnBreak,

        /// <summary>
        /// 没有课程了
        /// </summary>
        NoCoursesScheduled
    }


}
