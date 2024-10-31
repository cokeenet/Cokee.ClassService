using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;

using Vanara.PInvoke;

using static Vanara.PInvoke.User32;

namespace Cokee.ClassService.Helper
{
    public partial class PerformanceTransparentWin : Window
    {
        private static nint _hwnd;
        static class BrushCreator
        {
            /// <summary>
            /// 尝试从缓存获取或创建颜色笔刷
            /// </summary>
            /// <param name="color">对应的字符串颜色</param>
            /// <returns>已经被 Freeze 的颜色笔刷</returns>
            public static SolidColorBrush GetOrCreate(string color)
            {
                if (!color.StartsWith("#"))
                {
                    throw new ArgumentException($"输入的{nameof(color)}不是有效颜色，需要使用 # 开始");
                    // 如果不使用 # 开始将会在 ConvertFromString 出现异常
                }

                if (TryGetBrush(color, out var brushValue))
                {
                    return (SolidColorBrush)brushValue;
                }

                object convertColor;

                try
                {
                    convertColor = ColorConverter.ConvertFromString(color);
                }
                catch (FormatException)
                {
                    // 因为在 ConvertFromString 会抛出的是 令牌无效 难以知道是为什么传入的不对
                    throw new ArgumentException($"输入的{nameof(color)}不是有效颜色");
                }

                if (convertColor == null)
                {
                    throw new ArgumentException($"输入的{nameof(color)}不是有效颜色");
                }

                var brush = new SolidColorBrush((Color)convertColor);
                if (TryFreeze(brush))
                {
                    BrushCacheList.Add(color, new WeakReference<Brush>(brush));
                }

                return brush;
            }

            private static Dictionary<string, WeakReference<Brush>> BrushCacheList { get; } =
                new Dictionary<string, WeakReference<Brush>>();

            private static bool TryGetBrush(string key, out Brush brush)
            {
                if (BrushCacheList.TryGetValue(key, out var brushValue))
                {
                    if (brushValue.TryGetTarget(out brush))
                    {
                        return true;
                    }
                    else
                    {
                        // 被回收的资源
                        BrushCacheList.Remove(key);
                    }
                }

                brush = null;
                return false;
            }

            private static bool TryFreeze(Freezable freezable)
            {
                if (freezable.CanFreeze)
                {
                    freezable.Freeze();
                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// 创建高性能透明桌面窗口
        /// </summary>
        public PerformanceTransparentWin()
        {
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;

            Stylus.SetIsFlicksEnabled(this, false);
            Stylus.SetIsPressAndHoldEnabled(this, false);
            Stylus.SetIsTapFeedbackEnabled(this, false);
            Stylus.SetIsTouchFeedbackEnabled(this, false);

            WindowChrome.SetWindowChrome(this,
                new WindowChrome { GlassFrameThickness = WindowChrome.GlassFrameCompleteThickness, CaptionHeight = 0, CornerRadius = new CornerRadius(0), ResizeBorderThickness = new Thickness(0) });

            var visualTree = new FrameworkElementFactory(typeof(Border));
            visualTree.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Window.BackgroundProperty));
            var childVisualTree = new FrameworkElementFactory(typeof(ContentPresenter));
            childVisualTree.SetValue(UIElement.ClipToBoundsProperty, true);
            visualTree.AppendChild(childVisualTree);

            Template = new ControlTemplate
            {
                TargetType = typeof(Window),
                VisualTree = visualTree,
            };

            var _dwmEnabled = Dwmapi.DwmIsCompositionEnabled();
            if (_dwmEnabled)
            {
                _hwnd = new WindowInteropHelper(this).EnsureHandle();
                Loaded += PerformanceDesktopTransparentWindow_Loaded;
                Loaded += (_, _) => Install();
                Closed += (_, _) => _hookId?.Close(); 
                Background = Brushes.Transparent;
            }
            else
            {
                AllowsTransparency = true;
                Background = BrushCreator.GetOrCreate("#0100000");
                _hwnd = new WindowInteropHelper(this).EnsureHandle();
            }
        }
        private void PerformanceDesktopTransparentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Add WS_EX_LAYERED style
            ((HwndSource)PresentationSource.FromVisual(this)).AddHook(
                (nint hwnd, int msg, nint wParam, nint lParam, ref bool handled) =>
                {
                    if (msg == (int)User32.WindowMessage.WM_STYLECHANGING &&
                        wParam == (long)User32.WindowLongFlags.GWL_EXSTYLE)
                    {
                        var styleStruct = (STYLESTRUCT)Marshal.PtrToStructure(lParam, typeof(STYLESTRUCT));
                        styleStruct.styleNew |= User32.WindowStylesEx.WS_EX_LAYERED;
                        Marshal.StructureToPtr(styleStruct, lParam, false);
                        handled = true;
                    }
                    return IntPtr.Zero;
                });
        }
        /*
                /// <summary>
                /// 设置点击穿透到后面透明的窗口
                /// </summary>
                public void SetTransparentHitThrough()
                {
                    if (_dwmEnabled)
                    {
                        Win32.User32.SetWindowLongPtr(_hwnd, Win32.GetWindowLongFields.GWL_EXSTYLE,
                            (IntPtr)(int)((long)Win32.User32.GetWindowLongPtr(_hwnd, Win32.GetWindowLongFields.GWL_EXSTYLE) | (long)Win32.ExtendedWindowStyles.WS_EX_TRANSPARENT));
                    }
                    else
                    {
                        Background = Brushes.Transparent;
                    }
                }

                /// <summary>
                /// 设置点击命中，不会穿透到后面的窗口
                /// </summary>
                public void SetTransparentNotHitThrough()
                {
                    if (_dwmEnabled)
                    {
                        Win32.User32.SetWindowLongPtr(_hwnd, Win32.GetWindowLongFields.GWL_EXSTYLE,
                            (IntPtr)(int)((long)Win32.User32.GetWindowLongPtr(_hwnd, Win32.GetWindowLongFields.GWL_EXSTYLE) & ~(long)Win32.ExtendedWindowStyles.WS_EX_TRANSPARENT));
                    }
                    else
                    {
                        Background = BrushCreator.GetOrCreate("#0100000");
                    }
                }*/
        private static nint CurrentExStyle => User32.GetWindowLongPtr(_hwnd, User32.WindowLongFlags.GWL_EXSTYLE);

        public static void SetTransparentHitThrough() =>
                    User32.SetWindowLong(_hwnd, User32.WindowLongFlags.GWL_EXSTYLE, CurrentExStyle | (nint)User32.WindowStylesEx.WS_EX_TRANSPARENT);

        public static void SetTransparentNotHitThrough() =>
            User32.SetWindowLong(_hwnd, User32.WindowLongFlags.GWL_EXSTYLE, CurrentExStyle & ~(nint)User32.WindowStylesEx.WS_EX_TRANSPARENT);

        private static User32.SafeHHOOK? _hookId;
        public static void Install()
        {
            var moduleHandle = Kernel32.GetModuleHandle();

            _hookId = User32.SetWindowsHookEx(User32.HookType.WH_MOUSE_LL, Hook, moduleHandle, 0);
            if (_hookId == nint.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        private static bool CurrentTransparent = false;
        private static double CurrentDpi => WpfScreenHelper.Screen.FromWindow(Application.Current.MainWindow).ScaleFactor;
        private static IntPtr Hook(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0)
                return User32.CallNextHookEx(_hookId, nCode, wParam, lParam);

            var obj = Marshal.PtrToStructure(lParam, typeof(User32.MSLLHOOKSTRUCT));
            if (obj is not User32.MSLLHOOKSTRUCT info)
                return User32.CallNextHookEx(_hookId, nCode, wParam, lParam);

            // Check position
            var win = Application.Current.MainWindow;
            var relativePoint = new Point(info.pt.X / CurrentDpi - win.Left, info.pt.Y / CurrentDpi - win.Top);
            var test = VisualTreeHelper.HitTest((Grid)win.Content, relativePoint);
            if (test != null)
            {
                if (CurrentTransparent == true)
                {
                    SetTransparentNotHitThrough();
                }
                CurrentTransparent = false;
            }
            else
            {
                if (CurrentTransparent == false)
                {
                    SetTransparentHitThrough();
                }
                CurrentTransparent = true;
            }


            return User32.CallNextHookEx(_hookId, nCode, wParam, lParam);
        }
    }
    [StructLayout(LayoutKind.Sequential)]

    public struct STYLESTRUCT
    {
        public User32.WindowStylesEx styleOld;
        public User32.WindowStylesEx styleNew;
    }

}
