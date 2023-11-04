using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

using Serilog;

namespace Cokee.ClassService.Helper
{
    public static class Win32Func
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
        public static IntPtr programHandle = IntPtr.Zero;
        public static void SendMsgToProgman()
        {
            // 桌面窗口句柄，在外部定义，用于后面将我们自己的窗口作为子窗口放入
            programHandle = Win32Func.FindWindow("Progman", null);

            IntPtr result = IntPtr.Zero;
            // 向 Program Manager 窗口发送消息 0x52c 的一个消息，超时设置为2秒
            Win32Func.SendMessageTimeout(programHandle, 0x52c, IntPtr.Zero, IntPtr.Zero, 0, 2, result);

            // 遍历顶级窗口
            Win32Func.EnumWindows((hwnd, lParam) =>
            {
                // 找到第一个 WorkerW 窗口，此窗口中有子窗口 SHELLDLL_DefView，所以先找子窗口
                if (Win32Func.FindWindowEx(hwnd, IntPtr.Zero, "SHELLDLL_DefView", null) != IntPtr.Zero)
                {
                    // 找到当前第一个 WorkerW 窗口的，后一个窗口，及第二个 WorkerW 窗口。
                    IntPtr tempHwnd = Win32Func.FindWindowEx(IntPtr.Zero, hwnd, "WorkerW", null);

                    // 隐藏第二个 WorkerW 窗口
                    Win32Func.ShowWindow(tempHwnd, 0);
                }
                return true;
            }, IntPtr.Zero);
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
    static class ProcessHelper
    {
        public static bool HasPowerPointProcess()
        {
            Process[] processes = Process.GetProcesses();
            //Log.Information(processes.Length.ToString());
            foreach (var item in processes)
            {
                //Log.Information(item.ProcessName);
                if (item.ProcessName.Contains("powerpnt") || item.ProcessName.Contains("wpp") || item.ProcessName.Contains("POWERPNT"))
                return true;
            }
            return false;
            /*bool result = false;
            IntPtr intPtr = IntPtr.Zero;
            try
            {
                ProcessHelper.PROCESSENTRY32 processentry = new ProcessHelper.PROCESSENTRY32
                {
                    dwSize = (uint)Marshal.SizeOf(typeof(ProcessHelper.PROCESSENTRY32))
                };
                intPtr = ProcessHelper.CreateToolhelp32Snapshot(2U, 0U);
                if (!ProcessHelper.Process32First(intPtr, ref processentry))
                {
                    throw new ApplicationException(string.Format("Failed with win32 error code {0}", Marshal.GetLastWin32Error()));
                }
                for (; ; )
                {
                    string a = processentry.szExeFile.ToLower();
                    if (a == "POWERPNT.exe" || a == "powerpnt.exe" || a == "wpp.exe")
                    {
                        break;
                    }
                    if (!ProcessHelper.Process32Next(intPtr, ref processentry))
                    {
                        goto Block_5;
                    }
                }
                result = true;
            Block_5:;
            }
            catch (Exception innerException)
            {
                throw new ApplicationException("Can't get the process.", innerException);
            }
            finally
            {
                ProcessHelper.CloseHandle(intPtr);
            }
            return result;*/
        }

        public static void TryKillWppProcess()
        {
            Process[] processesByName = Process.GetProcessesByName("wpp");
            if (processesByName.Length == 1)
            {
                Process[] array = processesByName;
                for (int i = 0; i < array.Length; i++)
                {
                    array[i].Kill();
                }
            }
        }
    }
}
