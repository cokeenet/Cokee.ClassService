using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using File = System.IO.File;

namespace Cokee.ClassService.Helper
{
    public class PicDirectoryInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public int Version { get; set; }
        public int Files { get; set; }
    }

    public class CapServiceHost
    {
        public BackgroundWorker picBackgroundWorker = new BackgroundWorker();
        public Stopwatch sw = new Stopwatch();
        public int copieditems = 0, copieddirs = 0;
        public static CapService? service;

        public CapServiceHost()
        {
            picBackgroundWorker.WorkerReportsProgress = true;
            picBackgroundWorker.WorkerSupportsCancellation = true;
            picBackgroundWorker.DoWork += BackgroundWorker1_DoWork;
            picBackgroundWorker.ProgressChanged += BackgroundWorker1_ProgressChanged;
            picBackgroundWorker.RunWorkerCompleted += BackgroundWorker1_RunWorkerCompleted;
        }

        public void StartTask(string copydisk)
        {
            if (!string.IsNullOrEmpty(copydisk))
            {
                if (Directory.Exists(copydisk) && File.Exists(copydisk + "picDisk"))
                {
                    picBackgroundWorker.RunWorkerAsync(copydisk);
                }
            }
        }

        public void StartAgent()
        {
            service = new CapService();
            service.Start();
        }

        public void StopAgent()
        {
            service?.Stop();
            service?.Dispose();
        }

        public List<PicDirectoryInfo> EnumPicDirs(string disk = "D:\\")
        {
            List<PicDirectoryInfo> list = new List<PicDirectoryInfo>();
            foreach (string dir in Directory.GetDirectories($"{disk}CokeeDP\\Cache"))
            {
                DirectoryInfo dirinfo = new DirectoryInfo(dir);
                if (!dirinfo.Name.Contains("-")) list.Add(new PicDirectoryInfo { Path = dir, Name = dirinfo.Name, Version = 1, Files = dirinfo.GetFiles().Length });
                var dirs = Directory.GetDirectories(dir);

                if (dirs?.Length > 0)
                {
                    foreach (string subdir in dirs)
                    {
                        DirectoryInfo subdirinfo = new DirectoryInfo(dir);
                        if (dirinfo.Name != "2024") list.Add(new PicDirectoryInfo { Path = subdir, Name = subdirinfo.Name, Version = 2, Files = subdirinfo.GetFiles().Length });
                    }
                }
            }
            return list;
        }

        private void BackgroundWorker1_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            Catalog.UpdateProgress(e.ProgressPercentage, true, $"log{e.UserState}");
        }

        private void BackgroundWorker1_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            sw.Stop();
            if (e.Error != null)
                Catalog.ShowInfo($"Debug日志 threw Exception. ({sw.Elapsed.Seconds}s)", $"Exception:{e.Error.Message}{e.Error?.ToString()}");
            else if (e.Cancelled)
                Catalog.ShowInfo($"Debug日志 Cancelled:{e.Cancelled} ({sw.Elapsed.Seconds}s)", $"Exception:{e.Error?.ToString()}");
            else
                Catalog.ShowInfo($"Debug日志 Completed. ({sw.Elapsed.Seconds}s)", $"Result:{e.Result?.ToString()}");
            Catalog.UpdateProgress(100, false);
        }

        private void BackgroundWorker1_DoWork(object? sender, DoWorkEventArgs e)
        {
            string? copyDisk = (string?)e.Argument;
            Log.Information($"PicBackgroundWorker Started.");
            copieddirs = 0;
            copieditems = 0;
            if (!File.Exists(copyDisk + "picDisk")) throw new FileNotFoundException("Non copydisk.");
            foreach (var item in EnumPicDirs())
            {
                Log.Information($"Found v{item.Version} pic dir {item.Name} with {item.Files} pics.");
                decimal num = 0;
                string? cpTo = null;
                switch (item.Version)
                {
                    case 1:
                        cpTo = $"{copyDisk}CokeeDP\\Cache\\{item.Name}";
                        break;
                    case 2:
                        cpTo = $"{copyDisk}CokeeDP\\Cache\\2024\\{item.Name}";
                        break;
                }
                DirHelper.MakeExist(cpTo);
                copieddirs++; num = 0;
                foreach (string file in Directory.GetFiles(item.Path))
                {
                    FileInfo f = new FileInfo(file);
                    if (!File.Exists($"{cpTo}\\{f.Name}"))
                        f.CopyTo($"{cpTo}\\{f.Name}");
                    num++;
                    picBackgroundWorker.ReportProgress(Convert.ToInt32(num / (decimal)item.Files * 100), item.Name);
                }
                Log.Information("Done.");
            }


            Log.Information("All Done.");
            e.Result = $"{copieddirs} dirs,{copieditems} items";
        }
    }
}
