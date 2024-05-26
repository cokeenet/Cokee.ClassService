using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace Cokee.ClassService.Helper
{
    public class PicDirectoryInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public int version { get; set; }
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
                if (!Directory.Exists(copydisk))
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

        public List<PicDirectoryInfo> EnumPicDirs()
        {
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
                        { }
                    }
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