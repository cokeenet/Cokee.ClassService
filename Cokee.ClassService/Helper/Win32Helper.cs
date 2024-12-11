using IWshRuntimeLibrary;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using File = System.IO.File;

namespace Cokee.ClassService.Helper
{
    public static class Win32Helper
    {// 导入用户32位DLL中的GetForegroundWindow函数
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        // 导入用户32位DLL中的GetWindowPlacement函数
        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        // 定义WINDOWPLACEMENT结构
        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }

        // 定义窗口状态常量
        public const int SW_SHOWMAXIMIZED = 3;
        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string className, string winName);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessageTimeout(IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam, uint fuFlage, uint timeout, IntPtr result);

        //查找窗口的委托 查找逻辑
        public delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc proc, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string className, string winName);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hwnd, IntPtr parentHwnd);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, uint wParam, int lParam);

        public const uint WM_SYSCOMMAND = 0x0112;

        public const uint SC_MONITORPOWER = 0xF170;

        public static IntPtr programHandle = IntPtr.Zero;
        public static bool IsForegroundMaximized()
        {
            // 获取前台窗口句柄
            IntPtr hWnd = GetForegroundWindow();

            // 创建WINDOWPLACEMENT结构
            WINDOWPLACEMENT placement = new WINDOWPLACEMENT();
            placement.length = Marshal.SizeOf(placement);

            // 获取窗口位置信息
            if (GetWindowPlacement(hWnd, ref placement))
            {
                // 检查窗口是否最大化
                bool isMaximized = (placement.showCmd == SW_SHOWMAXIMIZED);
                return isMaximized;
            }
            return false;
        }
        public static void SendMsgToProgman()
        {
            // 桌面窗口句柄，在外部定义，用于将自己的窗口作为子窗口放入
            programHandle = FindWindow("Progman", null);

            IntPtr result = IntPtr.Zero;
            // 向 Program Manager 窗口发送消息 0x52c 的一个消息，超时设置为2秒
            SendMessageTimeout(programHandle, 0x52c, IntPtr.Zero, IntPtr.Zero, 0, 2, result);

            // 遍历顶级窗口
            EnumWindows((hwnd, lParam) =>
            {
                // 找到第一个 WorkerW 窗口，此窗口中有子窗口 SHELLDLL_DefView，所以先找子窗口
                if (FindWindowEx(hwnd, IntPtr.Zero, "SHELLDLL_DefView", null) != IntPtr.Zero)
                {
                    // 找到当前第一个 WorkerW 窗口的，后一个窗口，及第二个 WorkerW 窗口。
                    IntPtr tempHwnd = FindWindowEx(IntPtr.Zero, hwnd, "WorkerW", null);

                    // 隐藏第二个 WorkerW 窗口
                    ShowWindow(tempHwnd, 0);
                }
                return true;
            }, IntPtr.Zero);
        }
        #region Bottom Window From ZongziTEK
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 SWP_NOACTIVATE = 0x0010;
        private const UInt32 SWP_SHOWWINDOW = 0x0040;
        private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        public static void SetBottom(Window window)
        {
            IntPtr hWnd = new WindowInteropHelper(window).Handle;
            SetWindowPos(hWnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOSIZE | SWP_NOMOVE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
        }
        #endregion
        public static void SetToolWindow(Window window)
        {
            const int WS_EX_TOOLWINDOW = 0x80;
            // 获取窗口句柄
            IntPtr hwnd = new WindowInteropHelper(window).Handle;

            // 获取当前窗口样式
            int currentStyle = Win32Helper.GetWindowLong(hwnd, -20); // -20 表示 GWL_EXSTYLE

            // 设置窗口样式，去掉 WS_EX_APPWINDOW，添加 WS_EX_TOOLWINDOW
            int newStyle = (currentStyle & ~0x00000040) | WS_EX_TOOLWINDOW;

            // 更新窗口样式
            Win32Helper.SetWindowLong(hwnd, -20, newStyle);
        }

        #region 开机自启

        /// <summary>
        /// 开机自启创建
        /// </summary>
        /// <param name="exeName">程序名称</param>
        /// <returns></returns>
        public static bool CreateAutoBoot()
        {
            try
            {
                string exeName = "CokeeClass";
                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\" + exeName + ".lnk");
                //设置快捷方式的目标所在的位置(源程序完整路径)
                shortcut.TargetPath = System.Windows.Forms.Application.ExecutablePath;
                //应用程序的工作目录
                //当用户没有指定一个具体的目录时，快捷方式的目标应用程序将使用该属性所指定的目录来装载或保存文件。
                shortcut.WorkingDirectory = Environment.CurrentDirectory;
                //目标应用程序窗口类型(1.Normal window普通窗口,3.Maximized最大化窗口,7.Minimized最小化)
                shortcut.WindowStyle = 1;
                //快捷方式的描述
                shortcut.Description = exeName + "_Ink";
                //设置快捷键(如果有必要的话.)
                //shortcut.Hotkey = "CTRL+ALT+D";
                shortcut.Save();
                return true;
            }
            catch (Exception) { }
            return false;
        }

        /// <summary>
        /// 开机自启删除
        /// </summary>
        /// <param name="exeName">程序名称</param>
        /// <returns></returns>
        public static bool DeleteAutoBootLnk()
        {
            try
            {
                string exeName = "CokeeClass";
                File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\" + exeName + ".lnk");
                return true;
            }
            catch
            {
            }

            return false;
        }

        public static bool GetAutoBootStatus()
        {
            try
            {
                string exeName = "CokeeClass";
                return File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\" + exeName + ".lnk");
            }
            catch
            {
            }

            return false;
        }
    }

    #endregion 开机自启






    internal static class ProcessHelper
    {
        public static bool HasPowerPointProcess()
        {
            var processes = Process.GetProcessesByName("powerpnt").Length + Process.GetProcessesByName("wpp").Length + Process.GetProcessesByName("POWERPNT").Length;
            if (processes >= 0)
                return true;
            return false;
        }

        public static bool HasWordProcess()
        {
            var processes = Process.GetProcessesByName("WINWORD").Length + Process.GetProcessesByName("wps").Length + Process.GetProcessesByName("winword").Length;
            if (processes >= 0)
                return true;
            return false;
        }

        public static bool HasExcelProcess()
        {
            var processes = Process.GetProcessesByName("excel").Length + Process.GetProcessesByName("et").Length + Process.GetProcessesByName("EXCEL").Length;
            if (processes >= 0)
                return true;
            return false;
        }

        public static void TryKillWppProcess()
        {
            Process[] processesByName = Process.GetProcessesByName("wpp");
            if (processesByName.Length == 1)
            {
                Process[] array = processesByName;
                foreach (var t in array)
                {
                    t.Kill();
                }
            }
        }
    }
}