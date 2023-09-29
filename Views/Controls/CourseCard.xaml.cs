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
        public void Show(string courseName,string nextCourseName)
        {
            this.Visibility = Visibility.Visible;
            title.Content = $"{DateTime.Now.ToString("hh:mm")} 下一节:{nextCourseName}";
            subtitle.Content = $"本节课程: {courseName} 结束";
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
