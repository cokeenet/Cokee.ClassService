using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

using Serilog;

using File = System.IO.File;

namespace Cokee.ClassService.Helper
{
    public class PicDirectoryInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public int Version { get; set; }
        public int Files { get; set; }
        public string FilesLength { get; set; }
    }
    public class TaskInfo
    {
        public string NowName { get; set; }
        public int Persent { get; set; }
        public int Version { get; set; }
        public int TotalFiles { get; set; }
        public int RestFiles { get; set; }
        public int Speed { get; set; }
        public string ETA { get; set; }
        public TaskInfo(PicDirectoryInfo info, int copiedFiles, int speedpersec)
        {
            NowName = info.Name;
            Persent = Convert.ToInt32(copiedFiles / (decimal)info.Files * 100);
            Version = info.Version;
            TotalFiles = info.Files;
            RestFiles = info.Files - copiedFiles;
            Speed = speedpersec; if (Speed <= 0) Speed = 1;
            ETA = TimeSpan.FromMilliseconds((RestFiles / Speed) * 1000).ToString();
        }
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
                    if (picBackgroundWorker.IsBusy) Catalog.ShowInfo("Task Already running.");
                    else picBackgroundWorker.RunWorkerAsync(copydisk);
                }
            }
        }
        public void CancleTask()
        {
            if (picBackgroundWorker.IsBusy) { Catalog.ShowInfo("Try to cancel task.");picBackgroundWorker.CancelAsync(); }
            else Catalog.ShowInfo("Task Not running now.");

        }

        public void StartAgent()
        {
            service = new CapService();
            service.Start();
        }

        public void StopAgent()
        {
            service?.Dispose();
            service = null;
        }
        public void DoCapAction()
        {
            if (service == null)
                service.isfast = true;
            service?.CapAction();
        }
        public DateTime? GetLastCapTime()
        {
            return service?.lastCapTime;
        }

        public List<PicDirectoryInfo> EnumPicDirs(string disk = "D:\\")
        {

            List<PicDirectoryInfo> list = new List<PicDirectoryInfo>();
            foreach (string dir in Directory.GetDirectories($"{disk}CokeeDP\\Cache"))
            {
                DirectoryInfo dirinfo = new DirectoryInfo(dir);
                if (dirinfo.Name != "2024") list.Add(new PicDirectoryInfo { Path = dir, Name = dirinfo.Name, Version = 1, Files = dirinfo.GetFiles().Length, FilesLength = FileSystemHelper.DirHelper.CalcDirBytes(dir) });
                var dirs = Directory.GetDirectories(dir);

                if (dirs?.Length > 0)
                {
                    foreach (string subdir in dirs)
                    {
                        DirectoryInfo subdirinfo = new DirectoryInfo(subdir);
                        if (subdirinfo.Name != "2024")
                            list.Add(new PicDirectoryInfo { Path = subdir, Name = subdirinfo.Name, Version = 2, Files = subdirinfo.GetFiles().Length, FilesLength = FileSystemHelper.DirHelper.CalcDirBytes(subdir) });
                    }
                }
            }
            return list;
        }

        private void BackgroundWorker1_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            Catalog.UpdateProgress(e.ProgressPercentage, true, (TaskInfo?)e.UserState);
        }
        private void BackgroundWorker1_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            sw.Stop();

            if (e.Error != null)
                Catalog.HandleException(e.Error, $"Debug日志({sw.Elapsed.TotalMinutes:F2}min)");
            else if (e.Cancelled)
                Catalog.ShowInfo($"Debug日志 Cancelled:{e.Cancelled} ({sw.Elapsed.TotalMinutes:F2}min)", $"Result:{e.Result?.ToString()}");
            else
                Catalog.ShowInfo($"Debug日志 Completed. ({sw.Elapsed.TotalMinutes:F2}min)", $"Result:{e.Result?.ToString()}");
            Catalog.UpdateProgress(100, false);
            sw.Reset();
        }
        int existed = 0;
        private void BackgroundWorker1_DoWork(object? sender, DoWorkEventArgs e)
        {
            string? copyDisk = (string?)e.Argument;
            sw.Restart();
            Log.Information($"PicBackgroundWorker Started.");
            copieddirs = 0;
            existed = 0;
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
                FileSystemHelper.DirHelper.MakeExist(cpTo);
                num = 1;
                foreach (string file in Directory.GetFiles(item.Path))
                {
                    FileInfo f = new FileInfo(file);
                    if (!File.Exists($"{cpTo}\\{f.Name}"))
                        f.CopyTo($"{cpTo}\\{f.Name}");
                    else existed++;
                    num++;
                    copieditems++;
                    var time = (int)sw.Elapsed.TotalSeconds;
                    if (existed >= copieditems) existed = 0;
                    if (time <= 0) time = 1;
                    try
                    {
                        picBackgroundWorker.ReportProgress(Convert.ToInt32(num / (decimal)item.Files * 100), new TaskInfo(item, (int)num, (copieditems - existed) / time));
                    }
                    catch { }
                    if (e.Cancel) break;
                }
                if (e.Cancel) break;
                copieddirs++;
                Log.Information("Done.");
                e.Result = $"{copieddirs} dirs,copied {copieditems - existed} items";
            }
            Log.Information($"All Done. IsCancelled:{e.Cancel}");
            e.Result = $"{copieddirs} dirs,copied {copieditems - existed} items";
        }
    }
}