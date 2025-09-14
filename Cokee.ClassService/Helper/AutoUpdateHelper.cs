using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using Downloader;

using Serilog;

namespace Cokee.ClassService.Helper
{
    public class AutoUpdateHelper
    {
        public static DownloadService downloader = new DownloadService(new DownloadConfiguration()
        {
            ChunkCount = 8, // Number of file parts, default is 1
            ParallelDownload = true, // Download parts in parallel (default is false) 
            MaxTryAgainOnFailover = 5,// the maximum number of times to fail
        });
        public static async Task<string> CheckForUpdates(string? proxy = null)
        {
            try
            {
                string remoteAddress = proxy;
                remoteAddress += "https://gitee.com/cokee/classservice/raw/master/vcs.txt";
                string remoteVersion = await GetRemoteVersion(remoteAddress);

                if (remoteVersion != null)
                {
                    Version local = Assembly.GetExecutingAssembly().GetName().Version;
                    Version remote = new Version(remoteVersion);
                    if (remote > local)
                    {
                        Log.Information("AutoUpdate | New version Availble: " + remoteVersion);
                        return remoteVersion;
                    }
                    else return null;
                }
                else
                {
                    Log.Error("Failed to retrieve remote version.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AutoUpdate | Error: {ex.Message}");
                return null;
            }
        }

        public static async Task<string> GetRemoteVersion(string fileUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(fileUrl);
                    response.EnsureSuccessStatusCode();

                    return await response.Content.ReadAsStringAsync();
                }
                catch (HttpRequestException ex)
                {
                    Log.Error($"AutoUpdate | HTTP request error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Log.Error($"AutoUpdate | Error: {ex.Message}");
                }

                return null;
            }
        }

        private static string updatesFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ink Canvas Annotation", "AutoUpdate");
        private static string statusFilePath = null;

        public static async Task<bool> DownloadSetupFileAndSaveStatus(string version, string proxy = "")
        {
            try
            {
                statusFilePath = Path.Combine(updatesFolderPath, $"DownloadV{version}Status.txt");

                if (File.Exists(statusFilePath) && File.ReadAllText(statusFilePath).Trim().ToLower() == "true")
                {
                    Log.Information("AutoUpdate | Setup file already downloaded.");
                    return true;
                }

                string downloadUrl = $"{proxy}https://gitee.com/cokee/classservice/releases/download/v{version}/Ink.Canvas.Annotation.V{version}.Setup.exe";

                SaveDownloadStatus(false);
                await DownloadFile(downloadUrl, $"{updatesFolderPath}\\Ink.Canvas.Annotation.V{version}.Setup.exe");
                SaveDownloadStatus(true);

                Log.Information("AutoUpdate | Setup file successfully downloaded.");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"AutoUpdate | Error downloading and installing update: {ex.Message}");

                SaveDownloadStatus(false);
                return false;
            }
        }

        private static async Task DownloadFile(string fileUrl, string destinationPath)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(fileUrl);
                    response.EnsureSuccessStatusCode();

                    using (FileStream fileStream = File.Create(destinationPath))
                    {
                        await response.Content.CopyToAsync(fileStream);
                        fileStream.Close();
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"AutoUpdate | HTTP request error: {ex.Message}");
                    throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"AutoUpdate | Error: {ex.Message}");
                    throw;
                }
            }
        }

        private static void SaveDownloadStatus(bool isSuccess)
        {
            try
            {
                if (statusFilePath == null) return;

                string directory = Path.GetDirectoryName(statusFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(statusFilePath, isSuccess.ToString());
            }
            catch (Exception ex)
            {
                Log.Error($"AutoUpdate | Error saving download status: {ex.Message}");
            }
        }

        public static void InstallNewVersionApp(string version, bool isInSilence)
        {
            try
            {
                string setupFilePath = Path.Combine(updatesFolderPath, $"Ink.Canvas.Annotation.V{version}.Setup.exe");

                if (!File.Exists(setupFilePath))
                {
                    Log.Error($"AutoUpdate | Setup file not found: {setupFilePath}");
                    return;
                }

                string InstallCommand = $"\"{setupFilePath}\" /SILENT";
                if (isInSilence) InstallCommand += " /VERYSILENT";
                ExecuteCommandLine(InstallCommand);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Application.Current.Shutdown();
                });
            }
            catch (Exception ex)
            {
                Log.Error($"AutoUpdate | Error installing update: {ex.Message}");
            }
        }


        private static void ExecuteCommandLine(string command)
        {
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c {command}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = processStartInfo })
                {
                    process.Start();
                    Application.Current.Shutdown();
                    /*process.WaitForExit();
                    int exitCode = process.ExitCode;*/
                }
            }
            catch { }
        }

        public static void DeleteUpdatesFolder()
        {
            try
            {
                if (Directory.Exists(updatesFolderPath))
                {
                    Directory.Delete(updatesFolderPath, true);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"AutoUpdate clearing| Error deleting updates folder: {ex.Message}");
            }
        }
    }

    internal class AutoUpdateWithSilenceTimeComboBox
    {
        public static ObservableCollection<string> Hours { get; set; } = new ObservableCollection<string>();
        public static ObservableCollection<string> Minutes { get; set; } = new ObservableCollection<string>();

        public static void InitializeAutoUpdateWithSilenceTimeComboBoxOptions(ComboBox startTimeComboBox, ComboBox endTimeComboBox)
        {
            for (int hour = 0; hour <= 23; ++hour)
            {
                Hours.Add(hour.ToString("00"));
            }
            for (int minute = 0; minute <= 59; minute += 20)
            {
                Minutes.Add(minute.ToString("00"));
            }
            startTimeComboBox.ItemsSource = Hours.SelectMany(h => Minutes.Select(m => $"{h}:{m}"));
            endTimeComboBox.ItemsSource = Hours.SelectMany(h => Minutes.Select(m => $"{h}:{m}"));
        }

        public static bool CheckIsInSilencePeriod(string startTime, string endTime)
        {
            if (startTime == endTime) return true;
            DateTime currentTime = DateTime.Now;

            DateTime StartTime = DateTime.ParseExact(startTime, "HH:mm", null);
            DateTime EndTime = DateTime.ParseExact(endTime, "HH:mm", null);
            if (StartTime <= EndTime)
            { // 单日时间段
                return currentTime >= StartTime && currentTime <= EndTime;
            }
            else
            { // 跨越两天的时间段
                return currentTime >= StartTime || currentTime <= EndTime;
            }
        }
    }
}
