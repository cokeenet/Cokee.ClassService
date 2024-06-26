using Cokee.ClassService.Helper;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            switch (btn.Tag.ToString())
            {
                case "0":
                    DriveInfo[] s = DriveInfo.GetDrives();
                    diskComboBox.ItemsSource = s;
                    break;

                case "1":
                    var d = (DriveInfo)diskComboBox.SelectedItem;

                    dirlist.ItemsSource =await Catalog.CapServiceHost.EnumPicDirs(d.Name);
                    break;

                case "2":
                    var item = (PicDirectoryInfo)dirlist.SelectedItem;
                    var di = (DriveInfo)diskComboBox.SelectedItem;
                    new Task(() =>
                    {
                        Directory.Delete(item.Path, true);
                        Catalog.ShowInfo($"Deleted v{item.Version} dir {item.Name} with {item.Files} files.");
                    }).Start();

                    dirlist.ItemsSource = await Catalog.CapServiceHost.EnumPicDirs(di.Name);
                    break;

                case "3":
                    var x = (DriveInfo)diskComboBox.SelectedItem;

                    Catalog.CapServiceHost.StartTask(x.Name);
                    break;
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DriveInfo[] s = DriveInfo.GetDrives();
            diskComboBox.ItemsSource = s;
        }
    }
}