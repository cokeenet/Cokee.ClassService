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

namespace CokeeDP.Views.Controls
{
    /// <summary>
    /// PostNote.xaml 的交互逻辑
    /// </summary>
    public partial class StickyNote : UserControl
    {
    
        public static readonly DependencyProperty NameProperty =
      DependencyProperty.Register("StudentName", typeof(string), typeof(StickyNote), new PropertyMetadata(0));
        public StickyNote()
        {
            InitializeComponent();
            string DATA_DIR = "D:\\Program Files (x86)\\CokeeTech\\CokeeDP\\ink";
            if (File.Exists(DATA_DIR + $"\\0.ink"))
            {
                FileStream fs = new FileStream(DATA_DIR + $"\\0.ink", FileMode.Open);
                ink.Strokes = new StrokeCollection(fs);
                fs.Close();
            }
        }


    }
}
