using System.Windows;

using Wpf.Ui.Controls;

namespace Cokee.ClassService.Views.Windows
{
    /// <summary>
    /// CourseMgr.xaml 的交互逻辑
    /// </summary>

    public partial class UserLogin : UiWindow
    {
        private bool isClosing;

        public UserLogin()
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