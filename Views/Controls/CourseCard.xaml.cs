using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Cokee.ClassService.Helper;

namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// CourseCard.xaml 的交互逻辑
    /// </summary>
    public partial class CourseCard : UserControl
    {
        public CourseCard()
        {
            InitializeComponent();
        }
        public void Show(CourseNowStatus nowStatus,Course? course,Course? nextCourse)
        {
            this.Visibility = Visibility.Visible;
            
            switch(nowStatus)
            {
                case CourseNowStatus.EndOfLesson:
                    title.Text = $"{DateTime.Now.ToString("hh:mm")} 下课辣";
                    subtitle.Text = $"下一节: {nextCourse.Name}";
                    break;
                case CourseNowStatus.Upcoming:
                    title.Text = $"{DateTime.Now.ToString("hh:mm")} {course.Name} 上课辣";
                    subtitle.Text = $"下课时间 {course.EndTime.ToString("hh:mm")}";
                    break;
                case CourseNowStatus.OnBreak:
                    title.Text = $"{DateTime.Now.ToString("hh:mm")}";
                    subtitle.Text = $"课间休息 下一节: {course.Name}";
                    break;
                case CourseNowStatus.InProgress:
                    title.Text = $"{DateTime.Now.ToString("hh:mm")} 当前课程:{course.Name}";
                    subtitle.Text = $"pupupu";
                    break;
                case CourseNowStatus.NoCoursesScheduled:
                    title.Text = $"{DateTime.Now.ToString("hh:mm")}";
                    subtitle.Text = $"今天没有课了嗷";
                    break;
            }
            
            DoubleAnimation doubleAnimation = new DoubleAnimation(330,0,TimeSpan.FromSeconds(1));
            doubleAnimation.EasingFunction = new CircleEase();
            doubleAnimation.Completed += async (a, b) => 
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                DoubleAnimation doubleAnimation = new DoubleAnimation(0, 330, TimeSpan.FromSeconds(1));
                doubleAnimation.EasingFunction = new CircleEase();
                doubleAnimation.Completed += (a, b) => this.Visibility=Visibility.Collapsed;
                transT.BeginAnimation(TranslateTransform.XProperty, doubleAnimation);
            };
            transT.BeginAnimation(TranslateTransform.XProperty, doubleAnimation);
        }
    }
}
