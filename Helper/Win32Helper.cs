using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using IWshRuntimeLibrary;

using File = System.IO.File;

namespace Cokee.ClassService.Helper
{
    public static class Win32Helper
    {
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

        #region 开机自启

        /// <summary>
        /// 开机自启创建
        /// </summary>
        /// <param name="exeName">程序名称</param>
        /// <returns></returns>
        public static bool StartAutomaticallyCreate(string exeName)
        {
            try
            {
                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\" + exeName + ".lnk");
                //设置快捷方式的目标所在的位置(源程序完整路径)
                shortcut.TargetPath = Application.ExecutablePath;
                //应用程序的工作目录
                //当用户没有指定一个具体的目录时，快捷方式的目标应用程序将使用该属性所指定的目录来装载或保存文件。
                shortcut.WorkingDirectory = Environment.CurrentDirectory;
                //目标应用程序窗口类型(1.Normal window普通窗口,3.Maximized最大化窗口,7.Minimized最小化)
                shortcut.WindowStyle = 1;
                //快捷方式的描述
                shortcut.Description = exeName + "_Ink";
                //设置快捷键(如果有必要的话.)
                shortcut.Hotkey = "CTRL+ALT+D";
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
        public static bool StartAutomaticallyDel(string exeName)
        {
            try
            {
                File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\" + exeName + ".lnk");
                return true;
            }
            catch
            {
            }

            return false;
        }
    }

    #endregion 开机自启

    public static class DirHelper
    {
        public static bool MakeExist(string? path)
        {
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                    throw;
                }
            }
            else return true;
        }
    }

    public static class FileSize
    {
        public static string Format(long bytes, string formatString = "{0:0.00}")
        {
            int counter = 0;
            double number = bytes;

            // 最大单位就是 PB 了，而 PB 是第 5 级，从 0 开始数
            // "Bytes", "KB", "MB", "GB", "TB", "PB"
            const int maxCount = 5;

            while (Math.Round(number / 1024) >= 1)
            {
                number = number / 1024;
                counter++;

                if (counter >= maxCount)
                {
                    break;
                }
            }

            var suffix = counter switch
            {
                0 => "B",
                1 => "KB",
                2 => "MB",
                3 => "GB",
                4 => "TB",
                5 => "PB",
                // 通过 maxCount 限制了最大的值就是 5 了
                _ => throw new ArgumentException("")
            };

            return $"{string.Format(formatString, number)}{suffix}";
        }
    }

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