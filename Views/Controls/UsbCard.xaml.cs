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

using Cokee.ClassService.Helper;
using Serilog;

namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// UsbCard.xaml 的交互逻辑
    /// </summary>
    public partial class UsbCard : UserControl
    {
        public string disk;
        private BackgroundWorker backgroundWorker1 = new BackgroundWorker();

        public UsbCard()
        {
            InitializeComponent();
            try
            {
                backgroundWorker1.WorkerReportsProgress = true;
                backgroundWorker1.WorkerSupportsCancellation = true;
                backgroundWorker1.DoWork += BackgroundWorker1_DoWork;
                backgroundWorker1.ProgressChanged += BackgroundWorker1_ProgressChanged;
                backgroundWorker1.RunWorkerCompleted += (a, b) => { Catalog.ShowInfo($"Copied Done. Cancelled{b.Cancelled} Res{b.Result}", $"Err{b.Error?.ToString()}");Catalog.UpdateProgress(100, false); };
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
            anim2.Completed += (a, b) => Catalog.ToggleControlVisible(this);
            anim2.EasingFunction = Catalog.easingFunction;
            if (!isUnplug)
            {
                Visibility = Visibility.Visible;
                tranUsb.BeginAnimation(TranslateTransform.XProperty, anim1);
                string volumeLabel = $"{t.VolumeLabel}({t.Name})";

                disk = t.Name;
                diskName.Text = volumeLabel;
                diskInfo.Text = $"{FileSize.Format(t.TotalFreeSpace, "{0:0.0}")}/{FileSize.Format(t.TotalSize, "{0:0.0}")}";

                if (File.Exists(disk + "picDisk") && File.Exists(disk + "autoCopy")) SymbolIcon_MouseRightButtonDown(null, null);
                await Task.Delay(15000);
                ShowUsbCard(true);
            }
            else if (isUnplug)
            {
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

        private void SymbolIcon_MouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (File.Exists(disk + "picDisk"))
            {
                Catalog.ShowInfo($"Start to move pics. Isbusy: {backgroundWorker1.IsBusy}");
                copyDisk = disk;
                if (backgroundWorker1.IsBusy != true)
                {
                    backgroundWorker1.RunWorkerAsync();
                }
            }
            else Catalog.ShowInfo("nonTag");
        }

        private void BackgroundWorker1_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            Catalog.UpdateProgress(e.ProgressPercentage, true, "picCopy");
        }

        private void BackgroundWorker1_DoWork(object? sender, DoWorkEventArgs e)
        { 
            foreach (string dir in Directory.GetDirectories("D:\\CokeeDP\\Cache"))
            {
                DirectoryInfo dirinfo = new DirectoryInfo(dir);
                var files = Directory.GetFiles(dir);
                var cpTo = copyDisk + $"CokeeDP\\Cache\\{dirinfo.Name}";
                int num = 0;
                if(!Directory.Exists(cpTo))Directory.CreateDirectory(cpTo);
                foreach (string file in files)
                {
                    FileInfo f = new FileInfo(file);
                    if (File.Exists($"{cpTo}\\{f.Name}")) continue;
                    f.CopyTo($"{cpTo}\\{f.Name}");
                    num++;
                    Log.Information($"{dirinfo.Name}:{num}/{files.Length}");
                    backgroundWorker1.ReportProgress((num / files.Length) * 100);
                }
            }
        }
    }
}