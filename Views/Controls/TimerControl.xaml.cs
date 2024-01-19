using System.Windows;
using System.Windows.Controls;
using Cokee.ClassService.Helper;

namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// RandomControl.xaml 的交互逻辑
    /// </summary>
    public partial class TimerControl : UserControl
    {
        public TimerControl()
        {
            InitializeComponent();
        }


        private void CancelBtn(object sender, RoutedEventArgs e) => Catalog.ToggleControlVisible(this);

        private void ConfirmBtn(object sender, RoutedEventArgs e)
        {

        }
    }
}
