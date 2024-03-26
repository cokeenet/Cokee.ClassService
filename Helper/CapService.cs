using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

using AForge.Video;
using AForge.Video.DirectShow;

using Serilog;

using Timer = System.Timers.Timer;

namespace Cokee.ClassService.Helper
{
    public class CapService : IDisposable
    {
        private string disk = "D:\\";
        private int camIndex, res;
        private string copyPath = null;
        private VideoCaptureDevice captureDevice;
        private Timer CapTimer = new Timer(10 * 1000);
        private Timer ClearTimer = new Timer(72 * 60 * 60 * 1000);
        public bool debugDesktop = false, isCls = false;
        public DateTime? lastCapTime = null;
        private string path = "CokeeDP\\Cache", configPath = "logs\\v2";
        public Stopwatch sw = new Stopwatch();

        public event EventHandler<string> CapStartEvent, CapDoneEvent;

        public void Start()
        {
            if (!Directory.Exists(disk)) disk = @"C:\";
            configPath = disk + configPath;
            path = disk + path;
            WriteInfo($"Service started");
            try
            {
                //res = Convert.ToInt32(ReadTxtConfig("res", "2"));
                camIndex = Convert.ToInt32(ReadTxtConfig("camIndex", "0"));
                copyPath = ReadTxtConfig("copyPath");
                //var a = DateTime.Parse(ReadTxtConfig("expDate", "2099-01-01"));
                //if (DateTime.Now.Subtract(a).Minutes >= 0) { WriteInfo("EXP!!!"); Environment.Exit(0); return; }
            }
            catch (Exception ex) { WriteInfo(ex.ToString()); }
            WriteInfo($"Timer Interval:{CapTimer.Interval / 1000}s IsEnabled:{CapTimer.Enabled}");
            SetTimer(CapTimer, CapTimer_Elapsed);
            SetTimer(ClearTimer, ClearTimer_Elapsed);
            if (copyPath != null) WriteInfo("CopyPath Loaded.");
            else WriteInfo("Failed to load copyPath.");
            copyPath = "C:\\Users\\seewo\\OneDrive - Cokee Technologies";
            CapAction();
            new Task(CleanOutdated).Start();
            //Console.ReadKey(); //debug
        }

        protected void OnStop()
        {
            CapTimer.Stop();
            WriteInfo("Service stopped");
            //Environment.Exit(0);
        }

        public void Dispose()
        {
            GC.Collect();
        }

        public void SetTimer(Timer a, ElapsedEventHandler handler, bool dontstart = false)
        {
            a.Elapsed += handler;
            a.AutoReset = true;
            a.Enabled = true;
            if (!dontstart) a.Start();
        }

        private void CapTimer_Elapsed(object sender, ElapsedEventArgs e) => CapAction();

        private void ClearTimer_Elapsed(object sender, ElapsedEventArgs e) => new Task(CleanOutdated).Start();

        public async void CapAction()
        {
            try
            {
                int hour = DateTime.Now.Hour;
                int minute = DateTime.Now.Minute;
                if ((hour >= 22 && minute >= 45) || hour <= 5) { WriteInfo("[CapFunc]Not cap time."); return; }
                /*if (lastCapTime.HasValue)
                {
                    if (DateTime.Now.Subtract(lastCapTime.Value).Minutes <= 1)
                        WriteInfo($"[CapFunc]No-need to cap at this time. lastTime: {lastCapTime.Value.ToString("HH-mm-ss")}"); return;
                }*/
                WriteInfo("Start to cap.");
                if (captureDevice == null)
                {
                    var a = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                    WriteInfo($"Found {a.Count} CAMS.");
                    for (int i = 0; i < a.Count; i++)
                    {
                        WriteInfo($"CAM[{i}]: {a[i].Name}   Moniker:  {a[i].MonikerString}");
                    }
                    captureDevice = new VideoCaptureDevice(a[camIndex].MonikerString);
                    WriteInfo($"Selected CAM: {a[camIndex].Name}   Moniker:  {a[camIndex].MonikerString}");
                    var resolutionList = captureDevice.VideoCapabilities;
                    if (resolutionList.Length > 0)
                    {
                        for (int j = 0; j < resolutionList.Length; j++)
                        {
                            WriteInfo($"[{j}] Available Resolution: W{resolutionList[j].FrameSize.Width} xH{resolutionList[j].FrameSize.Height} FPS:{resolutionList[j].AverageFrameRate}");
                        }
                        if (res == -1) captureDevice.VideoResolution = FindMaxResolution(resolutionList);
                        else captureDevice.VideoResolution = resolutionList[res];

                        WriteInfo("Selected Resolution: W" + captureDevice.VideoResolution.FrameSize.Width + "xH" + captureDevice.VideoResolution.FrameSize.Height + " FPS:" + captureDevice.VideoResolution.AverageFrameRate);
                    }
                }
                else
                {
                    WriteInfo("Selected Resolution: W" + captureDevice.VideoResolution.FrameSize.Width + "xH" + captureDevice.VideoResolution.FrameSize.Height + " FPS:" + captureDevice.VideoResolution.AverageFrameRate);
                }
                sw.Restart();
                CapStartEvent.Invoke(this, $"Res{res} Cam{camIndex}");
                captureDevice.NewFrame += CaptureDevice_NewFrame;
                captureDevice.VideoSourceError += CaptureDevice_VideoSourceError;
                captureDevice.Start();
                await Task.Delay(1000);
                captureDevice.SignalToStop();
                WriteInfo($"NewFrame Event Registed. Sw:{sw.Elapsed.TotalSeconds}s");
                captureDevice.WaitForStop();
                //4:3 8.0mp 3264*2448max!
            }
            catch (Exception e)
            {
                WriteInfo(e.ToString());
            }
        }

        private void CaptureDevice_VideoSourceError(object sender, VideoSourceErrorEventArgs eventArgs)
        {
            WriteInfo($"Event:VideoSourceError. {eventArgs.Description}");
        }

        public VideoCapabilities FindMaxResolution(VideoCapabilities[] capabilities)
        {
            int[] h = new int[100], w = new int[100];
            int j = 0;
            foreach (var item in capabilities)
            {
                w.SetValue(item.FrameSize.Width, j);
                h.SetValue(item.FrameSize.Height, j);
                j++;
            }
            for (int i = 0; i < capabilities.Length; i++)
            {
                //WriteInfo($"W{w[i]}MAX{w.Max()} | H{h[i]} MAX{h.Max()})");
                if (w[i] == w.Max()) return capabilities.ElementAt(i);
            }
            WriteInfo("[FindMaxRes]Can't find!Use the last resolution instead.");
            return capabilities.Last();
        }

        private void CaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                if (lastCapTime.HasValue)
                {
                    var a = DateTime.Now.Subtract(lastCapTime.Value);
                    if (a.TotalMilliseconds < CapTimer.Interval)
                    { /*WriteInfo($"[CapEvent]Too fast to cap. Skip. Countdown:{a.Seconds}s Timer:{CapTimer.Interval}ms");*/
                        return;
                    }
                }
                Bitmap bitmap;
                captureDevice.NewFrame -= null;
                captureDevice.SignalToStop();
                string fileName = DateTime.Now.ToString("MMdd-HH-mm-ss") + ".png";
                string partPath = $"{DateTime.Now.Year}\\{DateTime.Now.ToString("MM-dd")}";
                string fullPath = $"{path}\\{partPath}\\{fileName}";
                if (debugDesktop) fullPath = @"C:\Users\seewo\aa\" + fileName;
                if (!Directory.Exists(Path.GetDirectoryName(fullPath))) Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                if (File.Exists(fullPath)) { WriteInfo("[CapEvent]Warning:Existing file. Skip cap."); return; }
                try
                {
                    bitmap = eventArgs.Frame;
                    bitmap.Save(fullPath, ImageFormat.Png);
                }
                catch (Exception ex) { WriteInfo(ex.ToString()); }

                WriteInfo($"Caped: {fileName} IsCamRunning:{captureDevice.IsRunning} FramesReceiced: {captureDevice.FramesReceived} FileInfo:{new FileInfo(fullPath).Length} bytes");

                captureDevice.SignalToStop();
                WriteInfo($"Try to stop CAM. IsRunning:{captureDevice.IsRunning}");
                //_ = UploadFileAsync(fullPath);
                string fullCopyPath = $"{copyPath}\\CokeeDP\\Cache\\{partPath}\\{fileName}";
                if (Directory.Exists(copyPath))
                {
                    if (!Directory.Exists(Path.GetDirectoryName(fullCopyPath))) Directory.CreateDirectory(Path.GetDirectoryName(fullCopyPath));
                    File.Copy(fullPath, fullCopyPath);
                    //WriteInfo($"{fullCopyPath}");
                    WriteInfo($"Try to copy file. IsCamRunning:{captureDevice.IsRunning}");
                }
                else WriteInfo($"Can't find copy folder.");
                lastCapTime = DateTime.Now;
                WriteInfo($"Done. Sw:{sw.Elapsed.TotalSeconds}s");
                CapDoneEvent.Invoke(this, $"{sw.Elapsed.TotalSeconds}|{fullPath}");
                sw.Stop();
                //ipcClient.Send("");
                //captureDevice.SignalToStop();
            }
            catch (Exception e)
            {
                WriteInfo(e.ToString());
            }
        }

        public void CleanOutdated()
        {
            try
            {
                WriteInfo($"Start to clean outdated resources.");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                DirectoryInfo dir = new DirectoryInfo(path + $"\\{DateTime.Now.Year}");
                var a = dir.GetDirectories();
                WriteInfo($"Year {DateTime.Now.Year}: Found {a.Length} Data Dirs.");
                foreach (var item in a)
                {
                    DateTime b;
                    long CountLength = 0;
                    if (DateTime.TryParse(item.Name, out b))

                        if (b.Month != DateTime.Now.Month || (DateTime.Now.Day - b.Day) >= 5)
                        {
                            WriteInfo($"Found outdated dir:{item.Name}. Trying to delete it.");
                            item.Delete(true);
                        }
                    foreach (var file in item.GetFiles())
                    {
                        CountLength += file.Length / 1000000;
                    }
                    WriteInfo($"Dir: {item.Name} Space: {CountLength}M");
                    if (CountLength >= 4000 && item.Name != DateTime.Now.ToString("MM-dd"))
                    {
                        WriteInfo($"Dir: {item.Name} Space: {CountLength}M! Outdated.");
                        item.Delete(true);
                        continue;
                    }
                    if (CountLength >= 10000)
                    {
                        WriteInfo($"Dir: {item.Name} Space: {CountLength}M. MUST!!DELETE!!!");
                        item.Delete(true);
                    }
                }
                var logs = Directory.GetFiles(configPath);
                WriteInfo($"Found {logs.Length} Agent Logs.");
                foreach (var log in logs)
                {
                    DateTime b;
                    if (DateTime.TryParse(log.Replace(".txt", ""), out b))
                        if (b.Month != DateTime.Now.Month || (DateTime.Now.Day - b.Day) >= 7)
                        {
                            WriteInfo($"Found outdated log:{log}. Trying to delete it.");
                            File.Delete(log);
                        }
                }
            }
            catch (Exception ex)
            {
                WriteInfo(ex.ToString());
            }
        }

        public void WriteInfo(string info)
        {
            Log.Information($"Agent|{info}");
            //Console.WriteLine(info);
            //var dir = @"D:\logs\v2";
            if (!Directory.Exists(configPath)) Directory.CreateDirectory(configPath);
            using (FileStream stream = new FileStream(configPath + "\\" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt", FileMode.Append))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.WriteLine($"{DateTime.Now} | {info}");
                }
            }
        }

        public string ReadTxtConfig(string configName, string defaultStr = "")
        {
            try
            {
                string fullPath = configPath + "\\" + configName;
                if (File.Exists(fullPath))
                {
                    return File.ReadAllText(fullPath);
                }
                else { File.WriteAllText(fullPath, defaultStr); return defaultStr; }
            }
            catch (Exception ex)
            {
                WriteInfo(ex.ToString());
                return defaultStr;
            }
        }
    }
}