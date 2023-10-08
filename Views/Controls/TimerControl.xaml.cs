using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Cokee.ClassService.Helper;
namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// RandomControl.xaml 的交互逻辑
    /// </summary>
    public partial class TimerControl : UserControl
    {
        public int Number=1,AllowMLang=1,AllowGirl=1,AllowExist=0;
        public event EventHandler<string> StartRandom;
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
