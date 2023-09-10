using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
using Cokee.ClassService.Views.Pages;
namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// RandomResult.xaml 的交互逻辑
    /// </summary>
    public partial class RandomResult : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        "ItemsSource", typeof(IEnumerable), typeof(RandomResult), new PropertyMetadata(null));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        public RandomResult()
        {
            InitializeComponent();
        }
        private void Confirm(object sender, RoutedEventArgs e) => this.Visibility = Visibility.Collapsed;
    }
}
