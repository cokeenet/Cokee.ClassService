using Cokee.ClassService.Helper;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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
            Dispatcher.BeginInvoke(() =>
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

                        dirlist.ItemsSource = Catalog.CapServiceHost.EnumPicDirs(d.Name);

                        break;

                    case "2":
                        var item = (PicDirectoryInfo)dirlist.SelectedItem;
                        var di = (DriveInfo)diskComboBox.SelectedItem;
                        new Task(() =>
                        {
                            try
                            {
                                Directory.Delete(item.Path, true);
                                Catalog.ShowInfo($"Deleted v{item.Version} dir {item.Name} with {item.Files} files.");
                                dirlist.ItemsSource = Catalog.CapServiceHost.EnumPicDirs(di.Name);
                            }
                            catch (Exception ex)
                            {
                                Catalog.HandleException(ex, "DirRemover");
                            }
                        }).Start();

                        break;

                    case "3":
                        var x = (DriveInfo)diskComboBox.SelectedItem;

                        Catalog.CapServiceHost.StartTask(x.Name);
                        break;
                    case "4":
                        var o = (PicDirectoryInfo)dirlist.SelectedItem;
                        if (File.Exists($"{o.Path}\\.lock"))
                        {
                            lockbtn.Background = new SolidColorBrush(Colors.LightGreen);
                            File.Delete($"{o.Path}\\.lock");
                        }
                        else
                        {
                            File.Create($"{o.Path}\\.lock");
                            lockbtn.Background = new SolidColorBrush(Colors.OrangeRed);
                        }
                        Catalog.ShowInfo($"Locked:{File.Exists($"{o.Path}\\.lock")}");
                        break;
                    case "5":
                        Catalog.CapServiceHost.DoCapAction();
                        break;
                }
            });
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                DriveInfo[] s = DriveInfo.GetDrives();
                diskComboBox.ItemsSource = s;
                diskComboBox.SelectedItem = s.LastOrDefault();
            });
        }



        private void selectdir(object sender, SelectionChangedEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                var o = (PicDirectoryInfo)dirlist.SelectedItem;
                if (o is null) return;
                if (!File.Exists($"{o.Path}\\.lock"))
                {
                    lockbtn.Background = new SolidColorBrush(Colors.LightGreen);
                }
                else
                {
                    lockbtn.Background = new SolidColorBrush(Colors.OrangeRed);
                }
            });
        }

        private void captime_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                captime.Text = $"ActTime:{Catalog.CapServiceHost.GetLastCapTime()?.ToString("HH:mm:ss")}";
            });
        }
    }
}