using System;
using System.Collections.Generic;
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

    public class Lesson
    {
        public string Name { get; set; } = "";
        public TimeSpan StartTime { get; set; } = TimeSpan.Zero;
        public TimeSpan EndTime { get; set; } = TimeSpan.Zero;

        [JsonIgnore]
        public bool IsChecked { get; set; } = false;

        public Lesson(string name = "")
        {
            Name = name;
        }
    }

    public class Schedule
    {
        public List<Lesson> Monday = new List<Lesson>();
        public List<Lesson> Tuesday = new List<Lesson>();
        public List<Lesson> Wendesday = new List<Lesson>();
        public List<Lesson> Thursday = new List<Lesson>();
        public List<Lesson> Friday = new List<Lesson>();
        public List<Lesson> Saturday = new List<Lesson>();
        public List<Lesson> Sunday = new List<Lesson>();
        public List<Lesson> LessonList = new List<Lesson>();

        public static void SaveToJson(Schedule schedule)
        {
            var json = JsonConvert.SerializeObject(schedule, Formatting.Indented);
            File.WriteAllText(Catalog.SCHEDULE_FILE, json);
        }

        // 从 JSON 文件加载 Schedule 对象
        public static Schedule LoadFromJson()
        {
            if (!File.Exists(Catalog.SCHEDULE_FILE)) return new Schedule();
            var json = File.ReadAllText(Catalog.SCHEDULE_FILE);
            var a = JsonConvert.DeserializeObject<Schedule>(json);
            if (a != null) return a;
            return new Schedule();
        }

        public static string GetShortTimeStr(DateTime t)
        {
            if (t.Hour == 17) if (t.Minute is >= 18 and <= 22) return "";
            return DateTime.Now.ToString("HH:mm");
        }

        public static CourseStatus GetNowCourse(Schedule schedule)
        {
            Lesson? course = null, nextCourse = null;
            DateTime now = DateTime.Now;
            CourseNowStatus status = CourseNowStatus.NoCoursesScheduled;
            List<Lesson>? coursesToday = new List<Lesson>();
            if (schedule == null) return new CourseStatus(status);
            switch (now.DayOfWeek)
            {
                case DayOfWeek.Sunday:
                    coursesToday = schedule.Sunday;
                    break;

                case DayOfWeek.Monday:
                    coursesToday = schedule.Monday;
                    break;

                case DayOfWeek.Tuesday:
                    coursesToday = schedule.Tuesday; ;
                    break;

                case DayOfWeek.Wednesday:
                    coursesToday = schedule.Wendesday;
                    break;

                case DayOfWeek.Thursday:
                    coursesToday = schedule.Thursday;
                    break;

                case DayOfWeek.Friday:
                    coursesToday = schedule.Friday;
                    break;

                case DayOfWeek.Saturday:
                    coursesToday = schedule.Saturday;
                    break;

                default:
                    break;
            }
            if (coursesToday != null)
            // 遍历课程列表，查找当前时间所在的课程
            {
                foreach (var c in coursesToday)
                {
                    if (now.TimeOfDay >= c.StartTime && now.TimeOfDay <= c.EndTime)
                    {
                        course = c; // 找到当前课程
                        nextCourse = coursesToday[coursesToday.IndexOf(c) + 1];
                        if (now.TimeOfDay == c.StartTime)
                        {
                            status = CourseNowStatus.Upcoming; // 上课时间点
                        }
                        else if (now.TimeOfDay == c.EndTime)
                        {
                            status = CourseNowStatus.EndOfLesson; // 下课时间点
                        }
                        else
                        {
                            status = CourseNowStatus.InProgress; // 正在上课
                        }
                    }
                    else if (now.TimeOfDay < c.StartTime)
                    {
                        status = CourseNowStatus.OnBreak; // 正在休息
                    }
                }
                // 如果没有当前课程，则返回没有课程了
                return new CourseStatus(status, course, nextCourse);
            }
            status = CourseNowStatus.NoCoursesScheduled;
            return new CourseStatus(status);
        }
    }

    public class CourseStatus
    {
        public CourseNowStatus nowStatus;
        public Lesson? now, next;

        public CourseStatus(CourseNowStatus nowStatus, Lesson? now = null, Lesson? next = null)
        {
            this.nowStatus = nowStatus;
            this.now = now;
            this.next = next;
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