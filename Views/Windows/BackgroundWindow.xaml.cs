using Cokee.ClassService.Helper;

using Microsoft.Win32;

using Serilog;

using System;
using System.ComponentModel;
using System.Timers;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Timer = System.Timers.Timer;

namespace Cokee.ClassService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class BackgroundWindow : Window
    {
        public Timer secTimer =new Timer(1000);
        public BackgroundWindow()
        {
            InitializeComponent();
            Win32Helper.SendMsgToProgman();
            Win32Helper.SetParent(new WindowInteropHelper(this).Handle, Win32Helper.programHandle);
            //Catalog.ShowInfo(Win32Helper.programHandle.ToString());
        }

        private void DisplaySettingsChanged(object? sender = null, EventArgs? e = null)
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
                secTimer.Elapsed += SecondTimer_Elapsed;
                secTimer.Start();
            }), DispatcherPriority.Normal);
        }
        public DateTime cd = new DateTime(2025, 06, 07);
        private async void SecondTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                Time.Text = DateTime.Now.ToString("HH:mm:ss");
                Countdown.Text = $"{Catalog.settings.CountDownDate.Subtract(DateTime.Now).Days} 天";
                longTime.Text = DateTime.Now.ToString("yyyy年MM月dd日 ddd");
            }, DispatcherPriority.Background);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            //Log.Information("Program Closing.");
            e.Cancel = true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //Log.Information($"BgWun Closed {e.ToString()}");
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