using System.Windows.Controls;

using Wpf.Ui.Controls;

namespace Cokee.ClassService.Views.Pages
{
    /// <summary>
    /// QuickFix.xaml 的交互逻辑
    /// </summary

    public partial class FloatNotePage : Page
    {
        public FloatNotePage()
        {
            InitializeComponent();
            infBtn.Click += (a, b) =>
            {
                content.FontSize += 5;
            };
            defBtn.Click += (a, b) =>
            {
                if (content.FontSize >= 5) content.FontSize -= 5;
            };
        }
    }
}