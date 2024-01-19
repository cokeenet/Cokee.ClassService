using System.Collections;
using System.Windows;
using System.Windows.Controls;
using Cokee.ClassService.Helper;

namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// RandomResult.xaml 的交互逻辑
    /// </summary>
    
    public partial class RandomResult : UserControl
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource), typeof(IEnumerable), typeof(RandomResult), new PropertyMetadata(null));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }
        public RandomResult()
        {
            InitializeComponent();
        }
        private void Confirm(object sender, RoutedEventArgs e) => Catalog.ToggleControlVisible(this);
    }
}
