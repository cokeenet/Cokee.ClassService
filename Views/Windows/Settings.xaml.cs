using System.Windows;

using Cokee.ClassService.Helper;

namespace Cokee.ClassService.Views.Windows
{
    /// <summary>
    /// CourseMgr.xaml 的交互逻辑
    /// </summary>

    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            Closing += (a, b) =>
            {
                Catalog.settings.Save();
            };
        }
    }
}