using System.Collections.Generic;
using System;
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

namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// InkToolBar.xaml 的交互逻辑
    /// </summary>
    public partial class InkToolBar : UserControl
    {
        public InkCanvas? inkCanvas;
        public bool isPPT = false, isWhiteBoard = false, isEraser = false;

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
                    inkCanvas.EraserShape = new RectangleStylusShape(3000, 5500, 90);
                    inkCanvas.ActiveEditingModeChanged += (a, b) =>
                    {
                        if (inkCanvas.ActiveEditingMode == InkCanvasEditingMode.EraseByPoint || inkCanvas.ActiveEditingMode == InkCanvasEditingMode.EraseByStroke) isEraser = true;
                        else isEraser = false;
                    };
                    moreMenu.DataContext = Catalog.appSettings;
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
                        isEraser = false;
                        if (!isWhiteBoard)
                        {
                            inkCanvas.Background.Opacity = 0;
                            inkCanvas.IsEnabled = false;
                        }
                        else inkCanvas.EditingMode = InkCanvasEditingMode.Select;
                        break;

                    case "Pen":
                        if (penMenu.IsOpen) penMenu.IsOpen = false;
                        else if (penBtn.Appearance == ControlAppearance.Primary) penMenu.IsOpen = true;
                        inkCanvas.IsEnabled = true;
                        isEraser = false;
                        if (!isWhiteBoard) inkCanvas.Background.Opacity = 0.01;
                        SetBtnState(penBtn);
                        inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
                        break;

                    case "Eraser":
                        SetBtnState(eraserBtn);
                        inkCanvas.IsEnabled = true;
                        isEraser = true;
                        if (!isWhiteBoard) inkCanvas.Background.Opacity = 0.01;
                        if (!Catalog.appSettings.EraseByPointEnable)
                            inkCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
                        else inkCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
                        break;

                    case "Back":
                        if (inkCanvas.Strokes.Count > 1) inkCanvas.Strokes.RemoveAt(inkCanvas.Strokes.Count - 1);
                        break;

                    case "Redo":
                        if (inkCanvas.Strokes.Count > 1) inkCanvas.Strokes.RemoveAt(inkCanvas.Strokes.Count - 1);
                        break;

                    case "More":
                        if (moreMenu.IsOpen) moreMenu.IsOpen = false;
                        else moreMenu.IsOpen = true;
                        break;

                    case "Select":
                        SetBtnState(null);
                        inkCanvas.EditingMode = InkCanvasEditingMode.Select;
                        break;

                    case "Exit":
                        inkCanvas.IsEnabled = false;
                        isEraser = false;
                        inkCanvas.Strokes.Clear();
                        inkCanvas.Background.Opacity = 0;
                        Visibility = Visibility.Collapsed;
                        Catalog.ExitPPTShow();
                        Catalog.SetWindowStyle(1);
                        break;
                }
            }, DispatcherPriority.Normal);
        }

        private void SetBtnState(Button? btn)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (Button button in mainGrid.Children.OfType<Button>())
                {
                    button.Appearance = ControlAppearance.Secondary;
                }
                if (btn != null) btn.Appearance = ControlAppearance.Primary;
            }, DispatcherPriority.Normal);
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
            if (toggle != null)
            {
                switch (toggle.Tag.ToString())
                {
                    case "WhiteBoard":
                        SolidColorBrush s1 = new SolidColorBrush(Color.FromRgb(0x0E, 0x25, 0x1D));
                        SolidColorBrush s2 = new SolidColorBrush(Colors.White);
                        s1.Opacity = 1;
                        s2.Opacity = 0.01;
                        if (En) { inkCanvas.Background = s1; isWhiteBoard = true; }
                        else { inkCanvas.Background = s2; isWhiteBoard = false; }
                        break;

                    case "EraseByShape":
                        if (En)
                        {
                            Catalog.appSettings.EraseByPointEnable = true;
                            Catalog.appSettings.SaveSettings();
                        }
                        else
                        {
                            Catalog.appSettings.EraseByPointEnable = false;
                            Catalog.appSettings.SaveSettings();
                        }
                        break;

                    default:
                        break;
                }
            }
        }
    }
}