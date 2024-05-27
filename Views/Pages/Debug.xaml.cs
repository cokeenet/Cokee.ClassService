using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

namespace Cokee.ClassService.Views.Pages
{
    /// <summary>
    /// Debug.xaml 的交互逻辑
    /// </summary>
    public partial class Debug : Page
    {
        public Debug()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            switch (btn.Tag.ToString())
            {
                case "1":
                   dirlist.ItemsSource = Catalog.;
                    break;

                case "2":
                    string? a = dirlist.SelectedValue?.ToString();
                    if (!string.IsNullOrEmpty(a))
                    {
                        if (a.StartsWith("v2:"))
                            Directory.Delete($"D:\\CokeeDP\\Cache\\2024\\{a.Split(":")[1]}", true);
                        else Directory.Delete($"D:\\CokeeDP\\Cache\\{a}", true);
                        Catalog.ShowInfo($"Deleted dir {a}.");
                    }
                    break;
            }
        }
    }
}