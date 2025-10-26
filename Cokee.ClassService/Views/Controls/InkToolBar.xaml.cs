using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

using Cokee.ClassService.Helper;

using iNKORE.UI.WPF.Modern;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
using iNKORE.UI.WPF.Modern.Controls;

namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// InkToolBar.xaml 的交互逻辑
    /// 提供墨迹批注的工具栏功能，包括画笔、橡皮擦、撤销/重做等操作
    /// AI整理---Cokee---20251025-01:07
    /// </summary>
    public partial class InkToolBar : UserControl
    {
        // 关联的墨迹画布
        public InkCanvas? inkCanvas;

        // 状态标识
        public bool isPPT = false;       // 是否在PPT模式下
        public bool isWhiteBoard = false;// 是否启用白板模式
        public bool isEraser = false;    // 是否处于橡皮擦模式

        /// <summary>
        /// 构造函数
        /// </summary>
        public InkToolBar()
        {
            InitializeComponent();

            // 设计模式下不执行初始化逻辑
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                // 绑定设置数据上下文
                moreCard.DataContext = Catalog.settings;

                // 注册可见性变化事件
                IsVisibleChanged += OnVisibilityChanged;
            }
        }

        /// <summary>
        /// 初始化墨迹画布事件（在外部设置inkCanvas后调用）
        /// </summary>
        public void InitializeInkCanvasEvents()
        {
            if (inkCanvas == null) return;

            // 画笔属性变更事件
            inkCanvas.DefaultDrawingAttributesReplaced += (s, e) =>
            {
                penSlider.Value = e.NewDrawingAttributes.Width;
            };

            // 设置橡皮擦形状
            inkCanvas.EraserShape = new RectangleStylusShape(300, 500);

            // 编辑模式变更事件（判断是否为橡皮擦模式）
            inkCanvas.ActiveEditingModeChanged += (s, e) =>
            {
                isEraser = inkCanvas.ActiveEditingMode is InkCanvasEditingMode.EraseByPoint
                        or InkCanvasEditingMode.EraseByStroke;
            };
        }

        /// <summary>
        /// 设置光标模式（启用/禁用墨迹功能）
        /// </summary>
        /// <param name="mode">0:禁用 1:启用画笔</param>
        public void SetCursorMode(int mode)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (inkCanvas == null) return;

                if (mode == 0)
                {
                    // 禁用墨迹
                    SetBtnState(curBtn);
                    inkCanvas.IsEnabled = false;
                    inkCanvas.Background.Opacity = 0;
                }
                else if (mode == 1)
                {
                    // 启用画笔
                    inkCanvas.IsEnabled = true;
                    inkCanvas.Background.Opacity = 0.01;
                    SetBtnState(penBtn);
                    inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
                }
            }, DispatcherPriority.Normal);
        }

        /// <summary>
        /// 画笔大小滑块值变更事件
        /// </summary>
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (inkCanvas != null)
            {
                inkCanvas.DefaultDrawingAttributes.Height = e.NewValue;
                inkCanvas.DefaultDrawingAttributes.Width = e.NewValue;
            }
        }

        /// <summary>
        /// 工具栏按钮点击事件
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (sender is not Button btn) return;
                var tag = btn.Tag?.ToString();
                if (string.IsNullOrEmpty(tag)) return;

                switch (tag)
                {
                    case "Cursor":
                        HandleCursorMode();
                        break;
                    case "Pen":
                        HandlePenMode();
                        break;
                    case "Eraser":
                        HandleEraserMode();
                        break;
                    case "Back":
                        HandleUndo();
                        break;
                    case "Redo":
                        HandleRedo();
                        break;
                    case "More":
                        // 更多菜单逻辑（原代码未实现）
                        break;
                    case "Select":
                        HandleSelectMode();
                        break;
                    case "Exit":
                        HandleExit();
                        break;
                }
            }, DispatcherPriority.Normal);
        }

        /// <summary>
        /// 处理光标模式（禁用墨迹或切换到选择模式）
        /// </summary>
        private void HandleCursorMode()
        {
            if (inkCanvas == null) return;

            SetBtnState(curBtn);
            isEraser = false;

            if (!isWhiteBoard)
            {
                inkCanvas.Background.Opacity = 0;
                inkCanvas.IsEnabled = false;
            }
            else
            {
                inkCanvas.EditingMode = InkCanvasEditingMode.Select;
            }
        }

        /// <summary>
        /// 处理画笔模式
        /// </summary>
        private void HandlePenMode()
        {
            if (inkCanvas == null) return;

            // 切换颜色选择面板
            if (colorFlyout.IsOpen)
                colorFlyout.Hide();
            else if (penBtn.Style == FindResource(ThemeKeys.AccentButtonStyleKey))
                colorFlyout.ShowAt(penBtn);

            // 启用画笔
            inkCanvas.IsEnabled = true;
            isEraser = false;
            if (!isWhiteBoard)
                inkCanvas.Background.Opacity = 0.01;

            SetBtnState(penBtn);
            inkCanvas.EditingMode = InkCanvasEditingMode.Ink;
        }

        /// <summary>
        /// 处理橡皮擦模式
        /// </summary>
        private void HandleEraserMode()
        {
            if (inkCanvas == null) return;

            SetBtnState(eraserBtn);
            inkCanvas.IsEnabled = true;
            isEraser = true;
            if (!isWhiteBoard)
                inkCanvas.Background.Opacity = 0.01;

            // 根据设置选择橡皮擦模式（点擦除/线擦除）
            inkCanvas.EditingMode = InkCanvasEditingMode.EraseByStroke;
        }

        /// <summary>
        /// 处理撤销操作（通过 MainWindow 公共方法）
        /// </summary>
        private void HandleUndo()
        {
            if (inkCanvas == null || Catalog.MainWindow == null) return;

            try
            {
                // 调用 MainWindow 的公共方法获取撤销历史
                var history = Catalog.MainWindow.UndoInk();
                if (history == null) return;

                Catalog.MainWindow.CurrentCommitType = MainWindow.CommitReason.CodeInput;

                // 根据历史记录执行撤销操作
                if (!history.StrokeHasBeenCleared)
                    inkCanvas.Strokes.Add(history.CurrentStroke);  // 恢复被清除的笔画
                else
                    inkCanvas.Strokes.Remove(history.CurrentStroke);  // 移除已添加的笔画

                Catalog.MainWindow.CurrentCommitType = MainWindow.CommitReason.UserInput;
            }
            catch (Exception ex)
            {
                Catalog.HandleException(ex, "撤销操作失败");
            }
        }

        /// <summary>
        /// 处理重做操作（通过 MainWindow 公共方法）
        /// </summary>
        private void HandleRedo()
        {
            if (inkCanvas == null || Catalog.MainWindow == null) return;

            try
            {
                // 调用 MainWindow 的公共方法获取重做历史
                var history = Catalog.MainWindow.RedoInk();
                if (history == null) return;

                Catalog.MainWindow.CurrentCommitType = MainWindow.CommitReason.CodeInput;

                // 根据历史记录执行重做操作
                if (history.StrokeHasBeenCleared)
                    inkCanvas.Strokes.Add(history.CurrentStroke);  // 重新添加笔画
                else
                    inkCanvas.Strokes.Remove(history.CurrentStroke);  // 重新清除笔画

                Catalog.MainWindow.CurrentCommitType = MainWindow.CommitReason.UserInput;
            }
            catch (Exception ex)
            {
                Catalog.HandleException(ex, "重做操作失败");
            }
        }

        /// <summary>
        /// 处理选择模式
        /// </summary>
        private void HandleSelectMode()
        {
            if (inkCanvas == null) return;

            SetBtnState(null);
            inkCanvas.EditingMode = InkCanvasEditingMode.Select;
        }

        /// <summary>
        /// 处理退出操作
        /// </summary>
        private void HandleExit()
        {
            ReleaseInk();

            if (Catalog.MainWindow == null) return;

            // 退出PPT放映（增加空值判断）
            if (isPPT && Catalog.MainWindow.PptApplication != null
                && Catalog.MainWindow.PptApplication.SlideShowWindows.Count > 0)
            {
                try
                {
                    Catalog.MainWindow.PptApplication.SlideShowWindows[1]?.View?.Exit();
                }
                catch (Exception ex)
                {
                    Catalog.HandleException(ex, "退出PPT放映失败");
                }
            }

            Catalog.SetWindowStyle(1);
        }

        /// <summary>
        /// 释放墨迹资源
        /// </summary>
        public void ReleaseInk()
        {
            if (inkCanvas == null) return;

            inkCanvas.IsEnabled = false;
            isEraser = false;
            Catalog.MainWindow?.ClearStrokes(true);
            inkCanvas.Background.Opacity = 0;
            Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 设置按钮状态（激活/取消激活）
        /// </summary>
        private void SetBtnState(Button? btn, string? content = null)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                // 重置所有按钮样式
                foreach (var button in mainGrid.Children.OfType<Button>())
                {
                    button.Style = (Style)FindResource(ThemeKeys.DefaultButtonStyleKey);
                }

                // 设置激活按钮样式
                if (btn != null)
                {
                    btn.Style = (Style)FindResource(ThemeKeys.AccentButtonStyleKey);
                    if (content != null)
                        btn.Content = content;
                }
            }, DispatcherPriority.Normal);
        }

        /// <summary>
        /// 清除屏幕墨迹
        /// </summary>
        private void ClearScr(object sender, MouseButtonEventArgs e)
        {
            Catalog.MainWindow?.ClearStrokes(true);
        }

        /// <summary>
        /// 颜色选择按钮点击事件
        /// </summary>
        private void ColorBtn(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || inkCanvas == null) return;

            // 设置画笔颜色
            if (button.Background is SolidColorBrush brush)
            {
                inkCanvas.DefaultDrawingAttributes.Color = brush.Color;
            }

            // 更新颜色选择按钮状态
            foreach (var item in colorGrid.Children.OfType<Button>())
            {
                item.Content = null;
            }
            button.Content = new FontIcon(FluentSystemIcons.CheckmarkCircle_48_Regular);
        }

        /// <summary>
        /// 开关控件状态变更事件
        /// </summary>
        private void OnToggleSwitch(object sender, RoutedEventArgs e)
        {
            if (sender is not ToggleSwitch toggle || inkCanvas == null) return;

            switch (toggle.Tag?.ToString())
            {
                case "WhiteBoard":
                    // 切换白板模式
                    var isWhiteBoardEnabled = toggle.IsOn;
                    inkCanvas.Background = isWhiteBoardEnabled
                        ? new SolidColorBrush(Color.FromRgb(0x0E, 0x25, 0x1D)) { Opacity = 1 }
                        : new SolidColorBrush(Colors.White) { Opacity = 0.01 };
                    isWhiteBoard = isWhiteBoardEnabled;
                    break;

                case "EraseByShape":
                    // 切换橡皮擦模式设置
                    Catalog.settings.EraseByPointEnable = toggle.IsOn;
                    Catalog.settings.Save();
                    break;
            }
        }

        /// <summary>
        /// 可见性变更事件
        /// </summary>
        private void OnVisibilityChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
            if (inkCanvas == null) return;

            // 首次加载时初始化画布事件（避免构造函数中inkCanvas为null的问题）
            if (!DesignerProperties.GetIsInDesignMode(this) && inkCanvas.DefaultDrawingAttributes != null)
            {
                InitializeInkCanvasEvents();
            }

            // 根据可见性设置模式
            if ((bool)e.NewValue && !isPPT)
            {
                SetCursorMode(1);
            }
            else
            {
                SetCursorMode(0);
            }
        }
    }
}