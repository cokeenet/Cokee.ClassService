using Cokee.ClassService.Views.Controls;
using Cokee.ClassService.Views.Pages;
using Newtonsoft.Json;
using System;
using System.Collections;
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

namespace CokeeClass.Views.Controls
{
    /// <summary>
    /// PostNote.xaml 的交互逻辑
    /// </summary>
    public partial class StickyNote : UserControl
    {
    
        public static new readonly DependencyProperty NameProperty =
      DependencyProperty.Register("StudentName", typeof(string), typeof(StickyNote), new PropertyMetadata(null));

        public string StudentName
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }
        public StickyNote()
        {
            InitializeComponent();
            string DATA_DIR = "D:\\Program Files (x86)\\CokeeTech\\CokeeDP\\ink";
            if (File.Exists(DATA_DIR + $"\\ {NameProperty.Name}.ink"))
            {
                FileStream fs = new FileStream(DATA_DIR + $"\\{NameProperty.Name}.ink", FileMode.Open);
                ink.Strokes = new StrokeCollection(fs);
                fs.Close();
            }
        }


    }
}
