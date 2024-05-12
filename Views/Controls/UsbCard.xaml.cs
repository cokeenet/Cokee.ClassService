using Cokee.ClassService.Helper;

using Serilog;

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// UsbCard.xaml 的交互逻辑
    /// </summary>
    public partial class UsbCard : UserControl
    {
        public string disk;
        private BackgroundWorker picBackgroundWorker = new BackgroundWorker();
        private Stopwatch sw = new Stopwatch();

        public UsbCard()
        {
            InitializeComponent();
            try
            {
                if (DesignerProperties.GetIsInDesignMode(this)) return;
                picBackgroundWorker.WorkerReportsProgress = true;
                picBackgroundWorker.WorkerSupportsCancellation = true;
                picBackgroundWorker.DoWork += BackgroundWorker1_DoWork;
                picBackgroundWorker.ProgressChanged += BackgroundWorker1_ProgressChanged;
                picBackgroundWorker.RunWorkerCompleted += BackgroundWorker1_RunWorkerCompleted;
                if (Catalog.IsScrSave) return;
                EnumDrive();
            }
            catch (Exception ex)
            {
                Catalog.HandleException(ex, "UsbCard");
            }
        }

        public void EnumDrive()
        {
            if (Catalog.IsScrSave) return;
            DriveInfo[] s = DriveInfo.GetDrives();
            s.Any(t =>
            {
                if (t.DriveType == DriveType.Removable)
                {
                    ShowUsbCard(false, t);
                    return true;
                }
                return false;
            });
        }

        private async void ShowUsbCard(bool isUnplug, DriveInfo t = null)
        {
            DoubleAnimation anim2 = new DoubleAnimation(0, 368, TimeSpan.FromSeconds(1));
            DoubleAnimation anim1 = new DoubleAnimation(368, 0, TimeSpan.FromSeconds(1));
            anim2.Completed += (a, b) => Visibility = Visibility.Collapsed;
            anim2.EasingFunction = Catalog.easingFunction;
            anim2.EasingFunction = Catalog.easingFunction;
            if (!isUnplug)
            {
                Visibility = Visibility.Visible;
                tranUsb.BeginAnimation(TranslateTransform.XProperty, anim1);
                disk = t.Name;
                try
                {
                    diskName.Text = $"{t.VolumeLabel}({t.Name})";
                    diskInfo.Text = $"{FileSize.Format(t.TotalFreeSpace, "{0:0.0}")}/{FileSize.Format(t.TotalSize, "{0:0.0}")}";
                    if (File.Exists(disk + "picDisk") && File.Exists(disk + "autoCopy")) SymbolIcon_MouseRightButtonDown(null, null);
                }
                catch
                {
                    diskName.Text = "U盘(未知盘符)";
                }

                await Task.Delay(15000);
                ShowUsbCard(true);
            }
            else if (isUnplug)
            {
                if (Visibility == Visibility.Collapsed) return;
                Visibility = Visibility.Visible;
                tranUsb.BeginAnimation(TranslateTransform.XProperty, anim2);
                await Task.Delay(1000);
                Visibility = Visibility.Collapsed;
            }
        }

        private void Open(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", disk);
            ShowUsbCard(true);
        }

        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            try
            {
                if (msg == WM_DEVICECHANGE)
                {
                    switch (wParam.ToInt32())
                    {
                        case DBT_DEVICEARRIVAL:
                            DriveInfo[] s = DriveInfo.GetDrives();
                            s.Any(t =>
                            {
                                if (t.DriveType == DriveType.Removable)
                                {
                                    ShowUsbCard(false, t);
                                    return true;
                                }
                                return false;
                            });
                            break;

                        case DBT_DEVICEREMOVECOMPLETE:
                            ShowUsbCard(true);
                            break;
                    }
                }
                return IntPtr.Zero;
            }
            catch { return IntPtr.Zero; }
        }

        public const int DBT_DEVICEARRIVAL = 0x8000;  //设备可用
        public const int DBT_DEVICEREMOVECOMPLETE = 0x8004; //设备被删除
        public const int FILE_SHARE_READ = 0x1;
        public const int FILE_SHARE_WRITE = 0x2;
        public const uint GENERIC_READ = 0x80000000;
        public const int GENERIC_WRITE = 0x40000000;
        public const int IOCTL_STORAGE_EJECT_MEDIA = 0x2d4808;
        public const int WM_DEVICECHANGE = 0x219;

        private void ExitUsbDrive(object sender, RoutedEventArgs e)
        {
            string filename = @"\\.\" + disk.Remove(2);
            //打开设备，得到设备的句柄handle.
            IntPtr handle = CreateFile(filename, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, 0x3, 0, IntPtr.Zero);
            // 向目标设备发送设备控制码。IOCTL_STORAGE_EJECT_MEDIA-弹出U盘
            uint byteReturned;
            bool result = DeviceIoControl(handle, IOCTL_STORAGE_EJECT_MEDIA, IntPtr.Zero, 0, IntPtr.Zero, 0, out byteReturned, IntPtr.Zero);
            if (!result) Catalog.ShowInfo("U盘退出失败", "请检查程序占用，关闭已打开的文件夹.");
            else ShowUsbCard(true);
        }

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateFile(
         string lpFileName,
         uint dwDesireAccess,
         uint dwShareMode,
         IntPtr SecondTimerurityAttributes,
         uint dwCreationDisposition,
         uint dwFlagsAndAttributes,
         IntPtr hTemplateFile);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            uint nInBufferSize,
            IntPtr lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped
        );

        private string copyDisk;
        private int copieditems = 0, copieddirs = 0;

        private void SymbolIcon_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (File.Exists(disk + "picDisk"))
            {
                copyDisk = disk;
                if (picBackgroundWorker.IsBusy != true)
                {
                    picBackgroundWorker.RunWorkerAsync();
                    sw.Restart();
                }
                else if (picBackgroundWorker.IsBusy == true)
                {
                    picBackgroundWorker.CancelAsync();
                    Catalog.ShowInfo($"Try to stop.");
                }
            }
            else Catalog.ShowInfo("nonTag");
        }

        private void BackgroundWorker1_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            Catalog.UpdateProgress(e.ProgressPercentage, true, $"log{e.UserState}");
        }

        private void BackgroundWorker1_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            sw.Stop();
            if (e.Error != null)
                Catalog.ShowInfo($"Debug日志 threw Exception. ({sw.Elapsed.Seconds}s)", $"Exception:{e.Error.ToString()}");
            else if (e.Cancelled)
                Catalog.ShowInfo($"Debug日志 Cancelled:{e.Cancelled} ({sw.Elapsed.Seconds}s)", $"Exception:{e.Error?.ToString()}");
            else
                Catalog.ShowInfo($"Debug日志 Completed. ({sw.Elapsed.Seconds}s)", $"Result:{e.Result?.ToString()}");
            Catalog.UpdateProgress(100, false);
        }

        private void BackgroundWorker1_DoWork(object? sender, DoWorkEventArgs e)
        {
            Log.Information("正在生成程序Debug日志");
            copieddirs = 0;
            copieditems = 0;
            foreach (string dir in Directory.GetDirectories("D:\\CokeeDP\\Cache"))
            {
                DirectoryInfo dirinfo = new DirectoryInfo(dir);
                var files = Directory.GetFiles(dir);
                var dirs = Directory.GetDirectories(dir);
                if (dirs != null)
                {
                    if (dirs.Length > 0)
                    {
                        foreach (string subdir in dirs)
                        {
                            var subinfo = new DirectoryInfo(subdir);
                            decimal num = 0;
                            var cpSubTo = $"{copyDisk}\\CokeeDP\\Cache\\{dirinfo.Name}\\{subinfo.Name}";
                            DirHelper.MakeExist(cpSubTo);
                            var subfiles = Directory.GetFiles(subdir);
                            Log.Information($"Found v2 pic dir {subinfo.Name} with {subfiles.Length} pics.");
                            foreach (var item in subfiles)
                            {
                                FileInfo f = new FileInfo(item);
                                if (!File.Exists($"{cpSubTo}\\{f.Name}"))
                                    f.CopyTo($"{cpSubTo}\\{f.Name}");
                                copieditems++;
                                num++;
                                picBackgroundWorker.ReportProgress(Convert.ToInt32(num / (decimal)subfiles.Length * 100), "v2" + subinfo.Name);
                            }
                            copieddirs++;
                            Log.Information("Done.");
                        }
                    }
                }
                var cpTo = copyDisk + $"CokeeDP\\Cache\\{dirinfo.Name}";
                DirHelper.MakeExist(cpTo);
                decimal num1 = 0;
                Log.Information($"Found v1 pic dir {dirinfo.Name} with {files.Length} pics.");
                foreach (string file in files)
                {
                    FileInfo f = new FileInfo(file);
                    if (!File.Exists($"{cpTo}\\{f.Name}"))
                        f.CopyTo($"{cpTo}\\{f.Name}");
                    num1++;
                    // Log.Information($"{dirinfo.Name}:{num}/{files.Length}");
                    picBackgroundWorker.ReportProgress(Convert.ToInt32(num1 / (decimal)files.Length * 100), dirinfo.Name);
                }
                Log.Information("Done.");
                e.Result = $"{copieddirs} dirs,{copieditems} items";
            }
        }
    }
}