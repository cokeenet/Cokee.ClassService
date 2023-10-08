using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Threading;

using Wpf.Ui.Appearance;
using Wpf.Ui.Common;

using Button = Wpf.Ui.Controls.Button;
using MSO = Microsoft.Office.Interop.PowerPoint;

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
        public bool isPPT = false;
        public InkToolBar()
        {
            InitializeComponent();
            /*if (inkCanvas != null)
            {
                
            }
            this.IsVisibleChanged += (a, b) =>
            {
                if ((bool)b.NewValue && !isPPT)
                {
                    SetCursorMode(1);
                }
                else
                {
                    SetCursorMode(0);
                }
            };*/
        }
        public void SetCursorMode(int mode)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (inkCanvas == null) return;
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
            Application.Current.Dispatcher.Invoke(() =>
            {
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
                        if (inkCanvas.Strokes.Count > 1) inkCanvas.Strokes.RemoveAt(inkCanvas.Strokes.Count - 1);
                        break;
                    case "More":
                        break;
                    case "Exit":
                        inkCanvas.IsEnabled = false;
                        inkCanvas.Strokes.Clear();
                        inkCanvas.Background.Opacity = 0;
                        Visibility = Visibility.Collapsed;
                        if (isPPT && pptApplication != null && pptApplication.SlideShowWindows[1] != null) pptApplication.SlideShowWindows[1].View.Exit();
                        break;
                }
            }, DispatcherPriority.Normal);
        }
        private void SetBtnState(Button btn)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
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
