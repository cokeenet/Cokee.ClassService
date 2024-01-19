using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;

namespace Cokee.ClassService.Helper
{
    // Token: 0x02000007 RID: 7
    public class PerformanceDesktopTransparentWindow : Window
    {
        // Token: 0x0600001E RID: 30 RVA: 0x00002438 File Offset: 0x00000638
        public PerformanceDesktopTransparentWindow()
        {
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            WindowChrome.SetWindowChrome(this, new WindowChrome
            {
                GlassFrameThickness = WindowChrome.GlassFrameCompleteThickness,
                CaptionHeight = 0.0
            });
            FrameworkElementFactory frameworkElementFactory = new FrameworkElementFactory(typeof(Border));
            frameworkElementFactory.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(BackgroundProperty));
            FrameworkElementFactory frameworkElementFactory2 = new FrameworkElementFactory(typeof(ContentPresenter));
            frameworkElementFactory2.SetValue(ClipToBoundsProperty, true);
            frameworkElementFactory.AppendChild(frameworkElementFactory2);
            Template = new ControlTemplate
            {
                TargetType = typeof(Window),
                VisualTree = frameworkElementFactory
            };
            //this._dwmEnabled = Win32.Dwmapi.DwmIsCompositionEnabled();
            if (_dwmEnabled)
            {
                _hwnd = new WindowInteropHelper(this).EnsureHandle();
                Loaded += PerformanceDesktopTransparentWindow_Loaded;
                Background = Brushes.Transparent;
                return;
            }
            AllowsTransparency = true;
            //base.Background = BrushCreator.GetOrCreate("#01000000");
            _hwnd = new WindowInteropHelper(this).EnsureHandle();
        }

        // Token: 0x0600001F RID: 31 RVA: 0x00002557 File Offset: 0x00000757
        private void PerformanceDesktopTransparentWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ((HwndSource)PresentationSource.FromVisual(this)).AddHook(delegate (IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
            {
                if (msg == 124 && (long)wParam == -20L)
                {
                    STYLESTRUCT stylestruct = (STYLESTRUCT)Marshal.PtrToStructure(lParam, typeof(STYLESTRUCT));
                    stylestruct.styleNew |= 524288;
                    Marshal.StructureToPtr(stylestruct, lParam, false);
                    handled = true;
                }
                return IntPtr.Zero;
            });
        }

        // Token: 0x06000020 RID: 32 RVA: 0x00002588 File Offset: 0x00000788
        public void SetTransparentHitThrough()
        {
            if (_dwmEnabled)
            {
                // Win32.User32.SetWindowLongPtr(this._hwnd, Win32.GetWindowLongFields.GWL_EXSTYLE, (IntPtr)((int)((long)Win32.User32.GetWindowLongPtr(this._hwnd, Win32.GetWindowLongFields.GWL_EXSTYLE) | 32L)));
                return;
            }
            Background = Brushes.Transparent;
        }

        // Token: 0x06000021 RID: 33 RVA: 0x000025C8 File Offset: 0x000007C8
        public void SetTransparentNotHitThrough()
        {
            if (_dwmEnabled)
            {
                // Win32.User32.SetWindowLongPtr(this._hwnd, Win32.GetWindowLongFields.GWL_EXSTYLE, (IntPtr)((int)((long)Win32.User32.GetWindowLongPtr(this._hwnd, Win32.GetWindowLongFields.GWL_EXSTYLE) & -33L)));
            }
            //base.Background = BrushCreator.GetOrCreate("#0100000");
        }

        // Token: 0x04000008 RID: 8
        private readonly bool _dwmEnabled;

        // Token: 0x04000009 RID: 9
        private readonly IntPtr _hwnd;

        // Token: 0x0200000A RID: 10
        private struct STYLESTRUCT
        {
            // Token: 0x0400000F RID: 15
            public int styleOld;

            // Token: 0x04000010 RID: 16
            public int styleNew;
        }
    }
}