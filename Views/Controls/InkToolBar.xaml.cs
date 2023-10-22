using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using Cokee.ClassService.Helper;

using Wpf.Ui.Common;
using Wpf.Ui.Controls;

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
            if (!DesignerHelper.IsInDesignMode)
            {
                
                if (inkCanvas != null)
                {
                    inkCanvas.DefaultDrawingAttributesReplaced += (a, b) =>
                    {
                        penSlider.Value = b.NewDrawingAttributes.Width;
                    };
                    inkCanvas.EraserShape = new RectangleStylusShape(50, 100);
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
                };
            }
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
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (inkCanvas != null)
            {
                inkCanvas.DefaultDrawingAttributes.Height = e.NewValue;
                inkCanvas.DefaultDrawingAttributes.Width = e.NewValue;
            }

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
                        if (penMenu.IsOpen) penMenu.IsOpen = false;
                        else if (penBtn.Appearance==ControlAppearance.Primary) penMenu.IsOpen = true;
                        inkCanvas.IsEnabled = true;
                        inkCanvas.Background.Opacity = 0.01;
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
                        if (moreMenu.IsOpen) moreMenu.IsOpen = false;
                        else moreMenu.IsOpen = true;
                        break;
                    case "Exit":
                        inkCanvas.IsEnabled = false;
                        inkCanvas.Strokes.Clear();
                        inkCanvas.Background.Opacity = 0;
                        Visibility = Visibility.Collapsed;
                        Catalog.ExitPPTShow();
                        Catalog.SetWindowStyle(1);
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

        private void ColorBtn(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null && sender is Button)
            {
                inkCanvas.DefaultDrawingAttributes.Color = (button.Background as SolidColorBrush).Color;
                foreach (var item in colorGrid.Children)
                {
                    if (item is Button)
                    {
                        Button a = (Button)item;
                        a.Icon = SymbolRegular.Empty;
                    }
                }
                button.Icon = SymbolRegular.CheckmarkCircle48;
            }
        }

        private void OnToggleSwitch(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggle = sender as ToggleSwitch;
            bool En = (bool)toggle.IsChecked;
            if(toggle != null)
            {
                switch (toggle.Tag.ToString())
                {
                    case "WhiteBoard":
                        SolidColorBrush s1 = new SolidColorBrush(Colors.DarkOliveGreen);
                        SolidColorBrush s2 = new SolidColorBrush(Colors.White);
                        s1.Opacity = 1;
                        s2.Opacity = 0.01;
                        if (En) inkCanvas.Background = s1;
                        else inkCanvas.Background = s2;
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
