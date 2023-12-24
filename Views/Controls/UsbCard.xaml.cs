using System;
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

namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// UsbCard.xaml 的交互逻辑
    /// </summary>
    public partial class UsbCard : UserControl
    {
        public string disk;

        public UsbCard()
        {
            InitializeComponent();
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
                this.Visibility = Visibility.Visible;
                tranUsb.BeginAnimation(TranslateTransform.XProperty, anim1);
                string volumeLabel = string.IsNullOrEmpty(t.VolumeLabel) ? t.Name : t.VolumeLabel;

                disk = t.Name;
                diskName.Text = volumeLabel;
                diskInfo.Text = $"{FileSize.Format(t.TotalFreeSpace, "{0:0.0}")}/{FileSize.Format(t.TotalSize, "{0:0.0}")}";
                await Task.Delay(15000);
                ShowUsbCard(true);
            }
            else if (isUnplug)
            {
                tranUsb.BeginAnimation(TranslateTransform.XProperty, anim2);
                await Task.Delay(1000);
                this.Visibility = Visibility.Collapsed;
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

                        default:
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
    }
}