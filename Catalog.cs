using System.Windows;

using Wpf.Ui.Mvvm.Services;

namespace Cokee.ClassService
{
    public class Catalog
    {
        public const string CONFIG_DIR = @"D:\Program Files (x86)\CokeeTech\CokeeClass\";
        public const string SCHEDULE_FILE = @"D:\Program Files (x86)\CokeeTech\CokeeClass\schedule.json";
        public static SnackbarService GlobalSnackbarService { get; set; }=((MainWindow)Application.Current.MainWindow).snackbarService;
    
    }
}
