using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Newtonsoft.Json;
namespace Cokee.ClassService.Helper
{

    public class Course
    {
        public string Name { get; set; } = "";
        public int DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    public class Schedule
    {
        public List<Course> Courses { get; set; }
        public void SaveToJson(string filePath)
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
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
        public List<Course> GetCourses(int dayOfWeek)
        {
            var courses = new List<Course>();
            foreach (var course in Courses)
            {
                if ((int)course.DayOfWeek == dayOfWeek)
                {
                    courses.Add(course);
                }
            }
            return courses;
        }
        public static CourseNowStatus GetNowCourse(Schedule schedule, out Course course,out Course nextCourse)
        {
            DateTime now = DateTime.Now;
            var coursesToday = schedule.GetCourses((int)now.DayOfWeek);
            course = null; // 初始化 course
            nextCourse = null;
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
