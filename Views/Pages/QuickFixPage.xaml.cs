using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Timers;
using System.Windows.Controls;

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
            ResUsage =/* "CPU: " + process.TotalProcessorTime.ToString() +*/ " MEM: " + process.WorkingSet64.ToString();
            try
            {
                Path=process.MainModule.FileName;
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(Path).ToBitmap();
            }
            catch 
            {
                Path = "null";
                Icon = null;
            }

            // 提取图标
            
        }
    }

    public partial class QuickFixPage : UiPage
    {
        Timer timer = new Timer(1000);
        ObservableCollection<ProcessInfo> processList = new ObservableCollection<ProcessInfo>();
        Process[] processes = Process.GetProcesses();
        public QuickFixPage()
        {
            InitializeComponent();
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
            processes = Process.GetProcesses();
            foreach (Process process in processes)
            {
                processList.Add(new ProcessInfo(process));
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
            if(ProcessView.SelectedItem is ProcessInfo)
            {
                processes[ProcessView.SelectedIndex].Kill(true);
            }
        }
    }
}
