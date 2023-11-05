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
                    inkCanvas.EraserShape = new RectangleStylusShape(300, 550, 90);
                    inkCanvas.ActiveEditingModeChanged += (a, b) =>
                    {
                        if (inkCanvas.ActiveEditingMode == InkCanvasEditingMode.EraseByPoint || inkCanvas.ActiveEditingMode == InkCanvasEditingMode.EraseByStroke) isEraser = true;
                        else isEraser = false;
                    };
                    moreMenu.DataContext = Catalog.appSettings;
                    timeMachine.OnRedoStateChanged += TimeMachine_OnRedoStateChanged;
                    timeMachine.OnUndoStateChanged += TimeMachine_OnUndoStateChanged;
                    inkCanvas.Strokes.StrokesChanged += StrokesOnStrokesChanged;
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

        private StrokeCollection[] strokeCollections = new StrokeCollection[101];
        private bool[] whiteboadLastModeIsRedo = new bool[101];
        private StrokeCollection lastTouchDownStrokeCollection = new StrokeCollection();

        private int CurrentWhiteboardIndex = 1;
        private int WhiteboardTotalCount = 1;
        private TimeMachineHistory[][] TimeMachineHistories = new TimeMachineHistory[101][]; //最多99页，0用来存储非白板时的墨迹以便还原

        private void SaveStrokes(bool isBackupMain = false)
        {
            if (isBackupMain)
            {
                var timeMachineHistory = timeMachine.ExportTimeMachineHistory();
                TimeMachineHistories[0] = timeMachineHistory;
                timeMachine.ClearStrokeHistory();
            }
            else
            {
                var timeMachineHistory = timeMachine.ExportTimeMachineHistory();
                TimeMachineHistories[CurrentWhiteboardIndex] = timeMachineHistory;
                timeMachine.ClearStrokeHistory();
            }
        }

        private void ClearStrokes(bool isErasedByCode)
        {
            _currentCommitType = CommitReason.ClearingCanvas;
            if (isErasedByCode) _currentCommitType = CommitReason.CodeInput;
            inkCanvas.Strokes.Clear();
            _currentCommitType = CommitReason.UserInput;
        }

        private void RestoreStrokes(bool isBackupMain = false)
        {
            try
            {
                if (TimeMachineHistories[CurrentWhiteboardIndex] == null) return; //防止白板打开后不居中
                if (isBackupMain)
                {
                    _currentCommitType = CommitReason.CodeInput;
                    timeMachine.ImportTimeMachineHistory(TimeMachineHistories[0]);
                    foreach (var item in TimeMachineHistories[0])
                    {
                        if (item.CommitType == TimeMachineHistoryType.UserInput)
                        {
                            if (!item.StrokeHasBeenCleared)
                            {
                                foreach (var strokes in item.CurrentStroke)
                                {
                                    if (!inkCanvas.Strokes.Contains(strokes))
                                        inkCanvas.Strokes.Add(strokes);
                                }
                            }
                            else
                            {
                                foreach (var strokes in item.CurrentStroke)
                                {
                                    if (inkCanvas.Strokes.Contains(strokes))
                                        inkCanvas.Strokes.Remove(strokes);
                                }
                            }
                        }
                        else if (item.CommitType == TimeMachineHistoryType.ShapeRecognition)
                        {
                            if (item.StrokeHasBeenCleared)
                            {
                                foreach (var strokes in item.CurrentStroke)
                                {
                                    if (inkCanvas.Strokes.Contains(strokes))
                                        inkCanvas.Strokes.Remove(strokes);
                                }
                                foreach (var strokes in item.ReplacedStroke)
                                {
                                    if (!inkCanvas.Strokes.Contains(strokes))
                                        inkCanvas.Strokes.Add(strokes);
                                }
                            }
                            else
                            {
                                foreach (var strokes in item.CurrentStroke)
                                {
                                    if (!inkCanvas.Strokes.Contains(strokes))
                                        inkCanvas.Strokes.Add(strokes);
                                }
                                foreach (var strokes in item.ReplacedStroke)
                                {
                                    if (inkCanvas.Strokes.Contains(strokes))
                                        inkCanvas.Strokes.Remove(strokes);
                                }
                            }
                        }
                        else if (item.CommitType == TimeMachineHistoryType.Rotate)
                        {
                            if (item.StrokeHasBeenCleared)
                            {
                                foreach (var strokes in item.CurrentStroke)
                                {
                                    if (inkCanvas.Strokes.Contains(strokes))
                                        inkCanvas.Strokes.Remove(strokes);
                                }
                                foreach (var strokes in item.ReplacedStroke)
                                {
                                    if (!inkCanvas.Strokes.Contains(strokes))
                                        inkCanvas.Strokes.Add(strokes);
                                }
                            }
                            else
                            {
                                foreach (var strokes in item.CurrentStroke)
                                {
                                    if (!inkCanvas.Strokes.Contains(strokes))
                                        inkCanvas.Strokes.Add(strokes);
                                }
                                foreach (var strokes in item.ReplacedStroke)
                                {
                                    if (inkCanvas.Strokes.Contains(strokes))
                                        inkCanvas.Strokes.Remove(strokes);
                                }
                            }
                        }
                        else if (item.CommitType == TimeMachineHistoryType.Clear)
                        {
                            if (!item.StrokeHasBeenCleared)
                            {
                                if (item.CurrentStroke != null)
                                {
                                    foreach (var currentStroke in item.CurrentStroke)
                                    {
                                        if (!inkCanvas.Strokes.Contains(currentStroke)) inkCanvas.Strokes.Add(currentStroke);
                                    }
                                }
                                if (item.ReplacedStroke != null)
                                {
                                    foreach (var replacedStroke in item.ReplacedStroke)
                                    {
                                        if (inkCanvas.Strokes.Contains(replacedStroke)) inkCanvas.Strokes.Remove(replacedStroke);
                                    }
                                }
                            }
                            else
                            {
                                if (item.ReplacedStroke != null)
                                {
                                    foreach (var replacedStroke in item.ReplacedStroke)
                                    {
                                        if (!inkCanvas.Strokes.Contains(replacedStroke)) inkCanvas.Strokes.Add(replacedStroke);
                                    }
                                }
                                if (item.CurrentStroke != null)
                                {
                                    foreach (var currentStroke in item.CurrentStroke)
                                    {
                                        if (inkCanvas.Strokes.Contains(currentStroke)) inkCanvas.Strokes.Remove(currentStroke);
                                    }
                                }
                            }
                        }
                        _currentCommitType = CommitReason.UserInput;
                    }
                }
                else
                {
                    _currentCommitType = CommitReason.CodeInput;
                    timeMachine.ImportTimeMachineHistory(TimeMachineHistories[CurrentWhiteboardIndex]);
                    foreach (var item in TimeMachineHistories[CurrentWhiteboardIndex])
                    {
                        if (item.CommitType == TimeMachineHistoryType.UserInput)
                        {
                            if (!item.StrokeHasBeenCleared)
                            {
                                foreach (var strokes in item.CurrentStroke)
                                {
                                    if (!inkCanvas.Strokes.Contains(strokes))
                                        inkCanvas.Strokes.Add(strokes);
                                }
                            }
                            else
                            {
                                foreach (var strokes in item.CurrentStroke)
                                {
                                    if (inkCanvas.Strokes.Contains(strokes))
                                        inkCanvas.Strokes.Remove(strokes);
                                }
                            }
                        }
                        else if (item.CommitType == TimeMachineHistoryType.ShapeRecognition)
                        {
                            if (item.StrokeHasBeenCleared)
                            {
                                foreach (var strokes in item.CurrentStroke)
                                {
                                    if (inkCanvas.Strokes.Contains(strokes))
                                        inkCanvas.Strokes.Remove(strokes);
                                }
                                foreach (var strokes in item.ReplacedStroke)
                                {
                                    if (!inkCanvas.Strokes.Contains(strokes))
                                        inkCanvas.Strokes.Add(strokes);
                                }
                            }
                            else
                            {
                                foreach (var strokes in item.CurrentStroke)
                                {
                                    if (!inkCanvas.Strokes.Contains(strokes))
                                        inkCanvas.Strokes.Add(strokes);
                                }
                                foreach (var strokes in item.ReplacedStroke)
                                {
                                    if (inkCanvas.Strokes.Contains(strokes))
                                        inkCanvas.Strokes.Remove(strokes);
                                }
                            }
                        }
                        else if (item.CommitType == TimeMachineHistoryType.Rotate)
                        {
                            if (item.StrokeHasBeenCleared)
                            {
                                foreach (var strokes in item.CurrentStroke)
                                {
                                    if (inkCanvas.Strokes.Contains(strokes))
                                        inkCanvas.Strokes.Remove(strokes);
                                }
                                foreach (var strokes in item.ReplacedStroke)
                                {
                                    if (!inkCanvas.Strokes.Contains(strokes))
                                        inkCanvas.Strokes.Add(strokes);
                                }
                            }
                            else
                            {
                                foreach (var strokes in item.CurrentStroke)
                                {
                                    if (!inkCanvas.Strokes.Contains(strokes))
                                        inkCanvas.Strokes.Add(strokes);
                                }
                                foreach (var strokes in item.ReplacedStroke)
                                {
                                    if (inkCanvas.Strokes.Contains(strokes))
                                        inkCanvas.Strokes.Remove(strokes);
                                }
                            }
                        }
                        else if (item.CommitType == TimeMachineHistoryType.Clear)
                        {
                            if (!item.StrokeHasBeenCleared)
                            {
                                if (item.CurrentStroke != null)
                                {
                                    foreach (var currentStroke in item.CurrentStroke)
                                    {
                                        if (!inkCanvas.Strokes.Contains(currentStroke)) inkCanvas.Strokes.Add(currentStroke);
                                    }
                                }
                                if (item.ReplacedStroke != null)
                                {
                                    foreach (var replacedStroke in item.ReplacedStroke)
                                    {
                                        if (inkCanvas.Strokes.Contains(replacedStroke)) inkCanvas.Strokes.Remove(replacedStroke);
                                    }
                                }
                            }
                            else
                            {
                                if (item.ReplacedStroke != null)
                                {
                                    foreach (var replacedStroke in item.ReplacedStroke)
                                    {
                                        if (!inkCanvas.Strokes.Contains(replacedStroke)) inkCanvas.Strokes.Add(replacedStroke);
                                    }
                                }
                                if (item.CurrentStroke != null)
                                {
                                    foreach (var currentStroke in item.CurrentStroke)
                                    {
                                        if (inkCanvas.Strokes.Contains(currentStroke)) inkCanvas.Strokes.Remove(currentStroke);
                                    }
                                }
                            }
                        }
                    }
                    _currentCommitType = CommitReason.UserInput;
                }
            }
            catch { }
        }

        #region TimeMachine

        private enum CommitReason
        {
            UserInput,
            CodeInput,
            ShapeDrawing,
            ShapeRecognition,
            ClearingCanvas,
            Rotate
        }

        private CommitReason _currentCommitType = CommitReason.UserInput;
        private bool IsEraseByPoint => inkCanvas.EditingMode == InkCanvasEditingMode.EraseByPoint;
        private StrokeCollection ReplacedStroke;
        private StrokeCollection AddedStroke;
        private StrokeCollection CuboidStrokeCollection;
        private TimeMachine timeMachine = new TimeMachine();

        private void TimeMachine_OnUndoStateChanged(bool status)
        {
            var result = status ? Visibility.Visible : Visibility.Collapsed;
            backBtn.Visibility = result;
            backBtn.IsEnabled = status;
        }

        private void TimeMachine_OnRedoStateChanged(bool status)
        {
            var result = status ? Visibility.Visible : Visibility.Collapsed;
            redoBtn.Visibility = result;
            redoBtn.IsEnabled = status;
        }

        private void StrokesOnStrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            if (_currentCommitType == CommitReason.CodeInput || _currentCommitType == CommitReason.ShapeDrawing) return;
            if (_currentCommitType == CommitReason.Rotate)
            {
                timeMachine.CommitStrokeRotateHistory(e.Removed, e.Added);
                return;
            }
            if ((e.Added.Count != 0 || e.Removed.Count != 0) && IsEraseByPoint)
            {
                if (AddedStroke == null) AddedStroke = new StrokeCollection();
                if (ReplacedStroke == null) ReplacedStroke = new StrokeCollection();
                AddedStroke.Add(e.Added);
                ReplacedStroke.Add(e.Removed);
                return;
            }
            if (e.Added.Count != 0)
            {
                if (_currentCommitType == CommitReason.ShapeRecognition)
                {
                    timeMachine.CommitStrokeShapeHistory(ReplacedStroke, e.Added);
                    ReplacedStroke = null;
                    return;
                }
                else
                {
                    timeMachine.CommitStrokeUserInputHistory(e.Added);
                    return;
                }
            }

            if (e.Removed.Count != 0)
            {
                if (_currentCommitType == CommitReason.ShapeRecognition)
                {
                    ReplacedStroke = e.Removed;
                    return;
                }
                else if (!IsEraseByPoint || _currentCommitType == CommitReason.ClearingCanvas)
                {
                    timeMachine.CommitStrokeEraseHistory(e.Removed);
                    return;
                }
            }
        }

        #endregion TimeMachine
    }
}