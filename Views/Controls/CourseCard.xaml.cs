using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

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

        public void Show(CourseStatus status, bool needHide = true)
        {
            Visibility = Visibility.Visible;

            switch (status.NowStatus)
            {
                case CourseNowStatus.EndOfLesson:
                    title.Text = $"{DateTime.Now.ToString("hh:mm")} 下课辣";
                    subtitle.Text = $"下一节: {status.Next.Name}";
                    break;

                case CourseNowStatus.Upcoming:
                    title.Text = $"{DateTime.Now.ToString("hh:mm")} {status.Now.Name} 上课辣";
                    subtitle.Text = $"下课时间 {status.Now.EndTime?.ToString("hh:mm")}";
                    break;

                case CourseNowStatus.OnBreak:
                    title.Text = $"{DateTime.Now.ToString("hh:mm")}";
                    subtitle.Text = $"课间休息 下一节: {status.Next.Name}";
                    break;

                case CourseNowStatus.InProgress:
                    title.Text = $"{DateTime.Now.ToString("hh:mm")} 当前课程:{status.Now.Name}";
                    subtitle.Text = "pupupu";
                    break;

                case CourseNowStatus.NoCoursesScheduled:
                    title.Text = $"{DateTime.Now.ToString("hh:mm")}";
                    subtitle.Text = "没课了";
                    break;
            }
            if (!needHide)
            {
                DoubleAnimation doubleAnimation = new DoubleAnimation(330, 0, TimeSpan.FromSeconds(1));
                doubleAnimation.EasingFunction = Catalog.easingFunction;
                doubleAnimation.Completed += async (a, b) =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(10));
                    DoubleAnimation doubleAnimation = new DoubleAnimation(0, 330, TimeSpan.FromSeconds(1));
                    doubleAnimation.EasingFunction = Catalog.easingFunction;
                    doubleAnimation.Completed += (a, b) => Visibility = Visibility.Collapsed;
                    transT.BeginAnimation(TranslateTransform.XProperty, doubleAnimation);
                };
                transT.BeginAnimation(TranslateTransform.XProperty, doubleAnimation);
            }
        }
    }
}