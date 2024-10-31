using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using Cokee.ClassService.Helper;

namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// PostNote.xaml 的交互逻辑
    /// </summary>
    public class StickyItem
    {
        public string Name { get; set; }

        public StickyItem(string _name)
        {
            Name = _name;
        }
    }

    public partial class StickyNote : UserControl
    {
        public StickyNote()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            string stu = e.NewValue.ToString();
            name.Text = stu;
            string INK_FILE = @$"{Catalog.INK_DIR}\{stu}";
            if (File.Exists(INK_FILE))
            {
                FileStream fs = new FileStream(INK_FILE, FileMode.Open);
                ink.Strokes = new StrokeCollection(fs);
                fs.Close();
            }
        }
    }
}