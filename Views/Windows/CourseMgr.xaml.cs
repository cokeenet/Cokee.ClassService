using System.Windows;

namespace Cokee.ClassService.Views.Windows
{
    /// <summary>
    /// CourseMgr.xaml 的交互逻辑
    /// </summary>

    public partial class CourseMgr : Window
    {
        private bool isClosing;

        public CourseMgr()
        {
            InitializeComponent();
            Closing += (a, b) => isClosing = true;
        }

        private void CoursesManage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue == false && !isClosing) Close();
        }
    }
}