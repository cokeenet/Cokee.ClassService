using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using Cokee.ClassService.Helper;

using InkCanvasForClass.IccInkCanvas;
using InkCanvasForClass.IccInkCanvas.Settings;

using iNKORE.UI.WPF.Modern;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
using iNKORE.UI.WPF.Modern.Controls;

namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// InkToolBar.xaml 的交互逻辑
    /// </summary>
    public partial class InkToolBar : UserControl
    {
        public IccBoard? iccBoard;
        public bool isPPT = false, isWhiteBoard, isEraser;

        public InkToolBar()
        {
            InitializeComponent();
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                if (iccBoard != null)
                {
                    iccBoard.BoardSettings.NibHeightChanged += (a, b) =>
                    {
                        penSlider.Value = iccBoard.BoardSettings.NibHeight;
                    };
                    moreCard.DataContext = Catalog.settings;
                    iccBoard.ActiveEditingModeChanged += (a, b) =>
                    {
                        switch (iccBoard.EditingMode)
                        {
                            case EditingMode.NoneWithHitTest:
                                SetBtnState(curBtn);
                                break;
                            case EditingMode.Select:
                                SetBtnState(curBtn);
                                break;
                            case EditingMode.Writing:
                                SetBtnState(penBtn);
                                break;
                            case EditingMode.GeometryErasing:
                                SetBtnState(eraserBtn);
                                break;
                            case EditingMode.AreaErasing:
                                SetBtnState(eraserBtn);
                                break;
                        }
                    };
                }
                IsVisibleChanged += (a, b) =>
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
            Dispatcher.Invoke(() =>
            {
                if (iccBoard == null) return;
                if (mode == 0)
                {
                    SetBtnState(curBtn);
                    iccBoard.EditingMode = EditingMode.NoneWithHitTest;
                }
                else if (mode == 1)
                {
                    SetBtnState(penBtn);
                    iccBoard.EditingMode = EditingMode.Writing;
                }
            }, DispatcherPriority.Normal);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (iccBoard != null)
                iccBoard.BoardSettings.NibWidth = e.NewValue;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var btn = (Button)sender;
                if (iccBoard == null) return;
                switch (btn.Tag.ToString())
                {
                    case "Cursor":
                        SetBtnState(curBtn);
                        isEraser = false;
                        iccBoard.EditingMode = EditingMode.NoneWithHitTest;
                        break;

                    case "Pen":
                        if (colorFlyout.IsOpen) colorFlyout.Hide();
                        else if (penBtn.Style == this.FindResource(ThemeKeys.AccentButtonStyleKey)) colorFlyout.ShowAt(penBtn);
                        isEraser = false;
                        SetBtnState(penBtn);
                        iccBoard.EditingMode = EditingMode.Writing;
                        break;

                    case "Eraser":
                        SetBtnState(eraserBtn);
                        isEraser = true;
                        iccBoard.EditingMode = EditingMode.GeometryErasing;
                        break;

                    case "Back":
                        try
                        {
                            iccBoard?.Undo();
                        }
                        catch (Exception ex)
                        {
                            Catalog.HandleException(ex, "TimeMachine");
                        }
                        break;

                    case "Redo":
                        try
                        {
                            iccBoard.Redo();
                        }
                        catch (Exception ex)
                        {
                            Catalog.HandleException(ex, "TimeMachine");
                        }
                        break;

                    case "More":
                        //moreMenu.IsOpen = !moreMenu.IsOpen;
                        break;

                    case "Select":
                        SetBtnState(null);
                        iccBoard.EditingMode = EditingMode.Select;
                        break;

                    case "Exit":
                        ReleaseInk();
                        if (Catalog.MainWindow != null)
                        {
                            if (Catalog.MainWindow.inkTool.isPPT && Catalog.MainWindow.pptApplication != null && Catalog.MainWindow.pptApplication.SlideShowWindows[1] != null) Catalog.MainWindow.pptApplication.SlideShowWindows[1].View.Exit();
                            //mainWindow.IconAnimation(true);
                        }
                        Catalog.SetWindowStyle(1);
                        break;
                }
            }, DispatcherPriority.Normal);
        }

        public void ReleaseInk()
        {
            Visibility = Visibility.Collapsed;
            //iccBoard.Undo()
        }

        private void SetBtnState(Button? btn)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (Button button in mainGrid.Children.OfType<Button>())
                {
                    button.Style = (Style)this.FindResource(ThemeKeys.DefaultButtonStyleKey);
                }
                if (btn != null) btn.Style = (Style)this.FindResource(ThemeKeys.AccentButtonStyleKey);
            }, DispatcherPriority.Normal);
        }

        private void ClearScr(object sender, MouseButtonEventArgs e) => iccBoard.CurrentPageItem.InkCanvas.Strokes.Clear();

        private void ColorBtn(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null && sender is Button)
            {
                iccBoard.BoardSettings.NibColor = (button.Background as SolidColorBrush).Color;
                foreach (var item in colorGrid.Children)
                {
                    if (item is Button)
                    {
                        Button a = (Button)item;
                        a.Content = null;
                    }
                }
                button.Content = new FontIcon(FluentSystemIcons.CheckmarkCircle_48_Regular);
            }
        }

        private void OnToggleSwitch(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggle = sender as ToggleSwitch;
            bool En = (bool)toggle.IsEnabled;
            if (toggle != null)
            {
                switch (toggle.Tag.ToString())
                {
                    case "WhiteBoard":
                        SolidColorBrush s1 = new(Color.FromRgb(0x0E, 0x25, 0x1D));
                         s1.Opacity = 1;SolidColorBrush s2 = new(Colors.White);
                       
                        s2.Opacity = 0.01;
                        if (En) { iccBoard.Background = s1; isWhiteBoard = true; }
                        else { iccBoard.Background = s2; isWhiteBoard = false; }
                        break;

                    case "EraseByShape":
                        if (En)
                        {
                            Catalog.settings.EraseByPointEnable = true;
                            Catalog.settings.Save();
                        }
                        else
                        {
                            Catalog.settings.EraseByPointEnable = false;
                            Catalog.settings.Save();
                        }
                        break;
                }
            }
        }
    }
}