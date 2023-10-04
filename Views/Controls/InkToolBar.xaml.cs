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
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Cokee.ClassService.Views.Controls;
using MSO = Microsoft.Office.Interop.PowerPoint;
using Wpf.Ui.Common;
using Wpf.Ui.Controls;

using Button = Wpf.Ui.Controls.Button;
using System.Windows.Threading;
using Wpf.Ui.Mvvm.Services;
using Wpf.Ui.Appearance;

namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// InkToolBar.xaml 的交互逻辑
    /// </summary>
    public partial class InkToolBar : UserControl
    {
        public static readonly DependencyProperty InkCanvasProperty =
     DependencyProperty.Register("inkCanvas", typeof(InkCanvas), typeof(InkToolBar), new PropertyMetadata(null));
        public MSO.Application pptApplication = null;
        public InkCanvas inkCanvas
        {
            get { return (InkCanvas)GetValue(InkCanvasProperty); }
            set { SetValue(InkCanvasProperty, value); }
        }
        public bool isPPT=false;
        public InkToolBar()
        {
            InitializeComponent();
            
            if(inkCanvas!=null)
            {
                inkCanvas.EraserShape = new RectangleStylusShape(500, 1000);
            }
            this.IsVisibleChanged += (a,b) => {
                if ((bool)b.NewValue && !isPPT) { SetCursorMode(1); Theme.Apply(ThemeType.Light); }
                else { SetCursorMode(0); Theme.Apply(ThemeType.Dark); }
                };
        }
        public void SetCursorMode(int mode)
        {
            Application.Current.Dispatcher.Invoke(() => {
                if (mode == 0)
                {
                    SetBtnState(curBtn);
                    inkCanvas.IsEnabled = false;
                    inkCanvas.Background.Opacity = 0;
                }
                else if (mode == 1)
                {
                    inkCanvas.IsEnabled = true;
                    inkCanvas.Background.Opacity = 0.01;
                    SetBtnState(penBtn);
                    inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
                }
            }, DispatcherPriority.Normal);
            
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => {
                var btn = (Button)sender;
            switch (btn.Tag.ToString())
            {
                case "Cursor":
                    SetBtnState(curBtn);
                    inkCanvas.IsEnabled = false;
                    inkCanvas.Background.Opacity = 0;
                    break;
                case "Pen":
                    inkCanvas.IsEnabled = true;
                    inkCanvas.Background.Opacity = 0.01;
                    //if (penBtn.Appearance == ControlAppearance.Primary) penMenu.IsOpen = true;
                    SetBtnState(penBtn);
                    inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
                    break;
                case "Eraser":
                    SetBtnState(eraserBtn);
                    inkCanvas.IsEnabled = true;
                    inkCanvas.Background.Opacity = 0.01;
                    inkCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
                    break;
                case "Back":
                    if(inkCanvas.Strokes.Count>1) inkCanvas.Strokes.RemoveAt(inkCanvas.Strokes.Count-1);
                    break;
                case "More":
                    break;
                case "Exit":
                    inkCanvas.IsEnabled = false;
                    inkCanvas.Strokes.Clear();
                    inkCanvas.Background.Opacity = 0;
                    Visibility = Visibility.Collapsed;
                    if (isPPT && pptApplication != null&& pptApplication.SlideShowWindows[1]!=null) pptApplication.SlideShowWindows[1].View.Exit();
                   break;
            }
            },DispatcherPriority.Normal);
        }
        private void SetBtnState(Button btn)
        {
            Application.Current.Dispatcher.Invoke(() => {
                foreach (Button button in mainGrid.Children.OfType<Button>())
            {
                button.Appearance = ControlAppearance.Secondary;
            }
            btn.Appearance = ControlAppearance.Primary;
            }, DispatcherPriority.Normal);
        }

        private void ListView_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void ClearScr(object sender, MouseButtonEventArgs e) => inkCanvas.Strokes.Clear();
    }
}
