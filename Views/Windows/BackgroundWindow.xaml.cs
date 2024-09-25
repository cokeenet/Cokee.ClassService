using Cokee.ClassService.Helper;
using Cokee.ClassService.Shared;
using Cokee.ClassService.Views.Windows;

using iNKORE.UI.WPF.Modern;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
using iNKORE.UI.WPF.Modern.Controls;

using Microsoft.Win32;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Serilog;
using Serilog.Events;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

using Application = System.Windows.Application;
using Control = System.Windows.Controls.Control;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using MsExcel = Microsoft.Office.Interop.Excel;
using MsPpt = Microsoft.Office.Interop.PowerPoint;
using MsWord = Microsoft.Office.Interop.Word;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Windows.Point;
using Timer = System.Timers.Timer;

namespace Cokee.ClassService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class BackgroundWindow : Window
    {
        public BackgroundWindow()
        {
            InitializeComponent();
            Win32Helper.SendMsgToProgman();
            Win32Helper.SetParent(new WindowInteropHelper(this).Handle, Win32Helper.programHandle);
            //Catalog.ShowInfo(Win32Helper.programHandle.ToString());
        }

        private void DisplaySettingsChanged(object? sender=null, EventArgs? e=null)
        {
            this.Width = SystemParameters.FullPrimaryScreenWidth;
            this.Height = SystemParameters.FullPrimaryScreenHeight;
            this.Top = 0;
            this.Left = 0;
            UpdateLayout();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(async () =>
            {
                DisplaySettingsChanged();
                SystemEvents.DisplaySettingsChanged += DisplaySettingsChanged;
                DpiChanged += DisplaySettingsChanged;
                SizeChanged += DisplaySettingsChanged;
            }), DispatcherPriority.Normal);
        }

        private async void SecondTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                // time.Text = DateTime.Now.ToString("HH:mm:ss");
                // time1.Text = DateTime.Now.ToString("HH:mm:ss");
                //longDate.Text = DateTime.Now.ToString("yyyy年MM月dd日 ddd");
            }, DispatcherPriority.Background);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Log.Information("Program Closing.");
            e.Cancel = true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Log.Information($"BgWun Closed {e.ToString()}");
        }

       /* protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            Win32Helper.SetToolWindow(this);
        }*/

        private void BackWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
        }
    }
}