using System;
using System.Collections.Generic;
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

using CokeeClass.Views.Controls;

using Wpf.Ui.Common;
using Wpf.Ui.Controls;

using Button = Wpf.Ui.Controls.Button;

namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// InkToolBar.xaml 的交互逻辑
    /// </summary>
    public partial class InkToolBar : UserControl
    {
        public static new readonly DependencyProperty InkCanvasProperty =
     DependencyProperty.Register("inkCanvas", typeof(InkCanvas), typeof(InkToolBar), new PropertyMetadata(null));
        public InkCanvas inkCanvas
        {
            get { return (InkCanvas)GetValue(InkCanvasProperty); }
            set { SetValue(InkCanvasProperty, value); }
        }

        public InkToolBar()
        {
            InitializeComponent();

            if(inkCanvas!=null)
            {
                inkCanvas.EraserShape = new RectangleStylusShape(1000, 1000);
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            switch (btn.Tag.ToString())
            {
                case "Cursor":
                    SetBtnState(curBtn);
                    inkCanvas.EditingMode = InkCanvasEditingMode.Select;
                    break;
                case "Pen":
                    if (penBtn.Appearance == ControlAppearance.Primary) penMenu.IsOpen = true;
                    else SetBtnState(penBtn);
                    inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
                    break;
                case "Eraser":
                    SetBtnState(eraserBtn);
                    inkCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
                    break;
                case "Back":
                    inkCanvas.Strokes.RemoveAt(inkCanvas.Strokes.Count-1);
                    break;
                case "More":
                    break;
                case "Exit":
                    inkCanvas.IsEnabled = false;
                    inkCanvas.Background.Opacity = 0;
                    this.Visibility = Visibility.Collapsed;
                    break;
            }
        }
        private void SetBtnState(Button btn)
        {
            foreach(Button button in mainGrid.Children.OfType<Button>())
            {
                button.Appearance = ControlAppearance.Secondary;
            }
            btn.Appearance = ControlAppearance.Primary;
        }

        private void ListView_Selected(object sender, RoutedEventArgs e)
        {

        }
    }
}
