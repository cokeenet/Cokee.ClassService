using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

using Serilog;

namespace Cokee.ClassService.Helper
{
    internal static class ProcessHelper
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
