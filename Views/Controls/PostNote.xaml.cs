using Cokee.ClassService.Views.Pages;
using Newtonsoft.Json;
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
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;
using Button = Wpf.Ui.Controls.Button;
using Clipboard = Wpf.Ui.Common.Clipboard;
using MessageBox = System.Windows.MessageBox;

namespace CokeeDP.Views.Controls
{
    /// <summary>
    /// PostNote.xaml 的交互逻辑
    /// </summary>
    public partial class PostNote : UserControl
    {
        public bool IsEraser=false;
        public Student stu=null;
        public string stud = "";
        List<Student> students = new List<Student>();
        public PostNote()
        {
            InitializeComponent();
            string DATA_FILE = "D:\\Program Files (x86)\\CokeeTech\\CokeeClass\\students.json";
            if (File.Exists(DATA_FILE))
            {
                List<Student> students = new List<Student>();
                List<string> str=new List<string>();    
                students = JsonConvert.DeserializeObject<List<Student>>(File.ReadAllText(DATA_FILE));
                foreach (var item in students)
                {
                    str.Add(item.Name);
                }
                atu.ItemsSource = str;
                //MessageBox.Show(str.Count.ToString());
            }
        }

        private void Pen(object sender, RoutedEventArgs e)
        {
            if (IsEraser) 
            {
                IsEraser = false;
                ink.EditingMode = InkCanvasEditingMode.Ink;
                Button btn = sender as Button;
                btn.Appearance = ControlAppearance.Primary;
                era.Appearance = ControlAppearance.Secondary;
            }
        }

        private void save(object sender, RoutedEventArgs e)
        {
            //  if (stu == null) return;
            try
            {


                FileStream fs = new FileStream(@$"D:\Program Files (x86)\CokeeTech\CokeeDP\ink\{stud}.ink", FileMode.OpenOrCreate);
                ink.Strokes.Save(fs);
                fs.Close();
                MessageBox.Show("saved.");
                ink.Strokes=new StrokeCollection();
                atu.Text = null;
                //this.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Clipboard.SetText(ex.ToString());
                MessageBox.Show(ex.ToString()); 
            }
        }

        private void eraser(object sender, RoutedEventArgs e)
        {
            if (!IsEraser)
            {
                IsEraser = true;
                ink.EditingMode = InkCanvasEditingMode.EraseByStroke;
                Button btn = sender as Button;
                btn.Appearance = ControlAppearance.Primary;
                pen.Appearance= ControlAppearance.Secondary;
            }
        }

        private void Atu_sc(object sender, RoutedEventArgs e)
        {
            AutoSuggestBox atu=sender as AutoSuggestBox;
            atu.Text=atu.Text.Trim();
            //MessageBox.Show(atu.Text.Trim());
            stud=atu.Text.Trim();
           /* foreach (Student item in students)
            {
                MessageBox.Show(atu.Text.Trim());
                if (atu.Text.Trim() == item.Name)
                { 
                    stu=item;
                    name.Content = item.Name;
                }
            }*/
        }


    }
}
