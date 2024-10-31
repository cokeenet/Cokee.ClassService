using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        public string Name { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        [JsonIgnore]
        public bool IsChecked { get; set; }

        public Lesson()
        {
        }
    }

    public class Schedule
    {
        // 使用 Dictionary 存储每天的课程
        public Dictionary<DayOfWeek, List<Lesson>> LessonsByDay { get; } = new Dictionary<DayOfWeek, List<Lesson>>();

        public IEnumerable<Lesson> Lessons => LessonsByDay.Values.SelectMany(x => x);

        public List<Lesson> this[DayOfWeek day]
        {
            get => LessonsByDay.GetValueOrDefault(day, new List<Lesson>());
            set => LessonsByDay[day] = value;
        }

        public Schedule()
        {
            // 使用构造函数初始化，而不是 foreach 循环
            LessonsByDay = Enum.GetValues(typeof(DayOfWeek))
                              .Cast<DayOfWeek>()
                              .ToDictionary(day => day, day => new List<Lesson>());
        }
    }

    public static class ScheduleExt
    {
        // 使用异步方法保存到JSON文件
        public static async Task SaveToJsonAsync(this Schedule schedule)
        {
            if (schedule == null) return;
            var json = JsonConvert.SerializeObject(schedule, Formatting.Indented);
            await File.WriteAllTextAsync(Catalog.SCHEDULE_FILE, json);
        }

        // 加载JSON文件的异步方法
        public static async Task<Schedule> LoadFromJsonAsync()
        {
            if (File.Exists(Catalog.SCHEDULE_FILE))
            {
                var json = await File.ReadAllTextAsync(Catalog.SCHEDULE_FILE);
                return JsonConvert.DeserializeObject<Schedule>(json) ?? new Schedule();
            }
            return new Schedule();
        }

        // 同步方法作为异步方法的包装，以保持向后兼容
        // public static void SaveToJson(this Schedule schedule) => SaveToJsonAsync(schedule).GetAwaiter().GetResult();

        //public static Schedule LoadFromJson() => LoadFromJsonAsync().GetAwaiter().GetResult();

        public static CourseStatus GetNowCourse(this Schedule schedule)
        {
            if (schedule == null || schedule.LessonsByDay == null)
                return new CourseStatus(CourseNowStatus.NoCoursesScheduled);

            DayOfWeek today = DayOfWeek.Monday; // 假设今天是星期一，实际应使用 DateTime.Now.DayOfWeek
            List<Lesson> coursesToday = schedule[today];

            var now = DateTime.Now;
            Lesson? currentCourse = null;
            Lesson? upcomingCourse = null;
            CourseNowStatus status = CourseNowStatus.NoCoursesScheduled;

            foreach (var lesson in coursesToday)
            {
                if (now.TimeOfDay >= lesson.StartTime && now.TimeOfDay <= lesson.EndTime)
                {
                    currentCourse = lesson;
                    status = CourseNowStatus.InProgress;
                    break;
                }
                else if (now.TimeOfDay < lesson.StartTime && status == CourseNowStatus.NoCoursesScheduled)
                {
                    if (upcomingCourse == null && lesson.StartTime > now.TimeOfDay)
                    {
                        upcomingCourse = lesson;
                        status = CourseNowStatus.Upcoming;
                    }
                }
            }

            // 如果当前没有课程，且找到了下一节课，则当前状态为课间休息
            if (status == CourseNowStatus.NoCoursesScheduled && upcomingCourse != null)
            {
                status = CourseNowStatus.OnBreak;
            }

            return new CourseStatus(status, currentCourse, upcomingCourse);
        }
    }

    public record CourseStatus(CourseNowStatus NowStatus, Lesson? Now = null, Lesson? Next = null);

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