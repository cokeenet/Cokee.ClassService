using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Cokee.ClassService.Views.Pages;
using Cokee.ClassService.Views.Windows;

using Newtonsoft.Json;

using Wpf.Ui.Common;

using Wpf.Ui.Controls;

namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// NewStudent.xaml 的交互逻辑
    /// </summary>
    public partial class NewStudent : UserControl
    {
        public const string DATA_FILE = "D:\\Program Files (x86)\\CokeeTech\\CokeeClass\\students.json";
        public NewStudent()
        {
            InitializeComponent();
            Application.Current.Windows.OfType<StudentMgr>().FirstOrDefault().AddStuEvent += (a,b)=> this.Visibility=Visibility.Visible;
        }

        private void Confirm(object sender, RoutedEventArgs e)
        {
            try
            {
                string[] lines = stutb.Text.Split("\r");
                List<Student> students = new List<Student>();
                foreach (string line in lines)
                {
                    string[] values = line.Split(' ');
                    string name = values[0];
                    int sex = 0;
                    if (values[1] == "男") sex = 1;
                    DateTime dt = new DateTime();
                    DateTime.TryParse(values[2], out dt);
                    students.Add(new Student(name, sex, dt));
                    File.WriteAllText(DATA_FILE, JsonConvert.SerializeObject(students));
                }
                this.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                
            }
        }
    }
}
