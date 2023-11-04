using Cokee.ClassService.Helper;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Timers;

using Wpf.Ui.Controls;

namespace Cokee.ClassService.Views.Pages
{
    /// <summary>
    /// QuickFix.xaml 的交互逻辑
    /// </summary>
    public class ProcessInfo
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string ResUsage { get; set; }
        public Bitmap Icon { get; set; }
        public string Path { get; set; }

        public ProcessInfo(Process process)
        {
            Name = process.ProcessName;
            if (process.Responding)
                Status = "正常运行";
            else
                Status = "未响应";
            ResUsage =/* "CPU: " + process.TotalProcessorTime.ToString() +*/ " MEM: " + FileSize.Format(process.WorkingSet64);
            Path = process.MainModule.FileName;
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Path).ToBitmap();

            // 提取图标

        }
    }

    public partial class QuickFixPage : UiPage
    {
        Timer timer = new Timer(1000);
        List<ProcessInfo> processList = new List<ProcessInfo>();
        Process[] processes = Process.GetProcesses();
        public QuickFixPage()
        {
            InitializeComponent();
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
            processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                try { if (process.MainModule != null) processList.Add(new ProcessInfo(process)); }
                catch { continue; }

            }

            ProcessView.ItemsSource = processList;
        }
        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                processList.Add(new ProcessInfo(process));
            }

            ProcessView.ItemsSource = processList;
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (ProcessView.SelectedItem is ProcessInfo)
            {
                processes[ProcessView.SelectedIndex].Kill(true);
            }
        }

        private void Button_Click_1(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}
