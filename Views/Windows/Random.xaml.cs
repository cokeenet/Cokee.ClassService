using System.Windows;

namespace Cokee.ClassService.Views.Windows
{
    /// <summary>
    /// CourseMgr.xaml 的交互逻辑
    /// </summary>

    public partial class RandomWindow : Window
    {
        private bool isClosing;

        public RandomWindow()
        {
            InitializeComponent();
            rac.RandomResultControl=res;
            Closing += (a, b) => isClosing = true;
        }

        private void IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false && !isClosing) Close();
        }
    }
}