using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

using Serilog;

using Wpf.Ui.Appearance;

namespace Cokee.ClassService.Helper
{
    public class AppSettings
    {
        public bool MultiTouchEnable { get; set; } = true;
        public bool PPTFunctionEnable { get; set; } = true;
        public bool EraseByPointEnable { get; set; } = true;
        public bool UseMemberAvatar { get; set; } = false;

        [JsonIgnore]
        private bool _FileWatcherEnable { get; set; } = true;

        public bool FileWatcherEnable
        {
            get { return _FileWatcherEnable; }
            set
            {
                if (value != _FileWatcherEnable)
                {
                    _FileWatcherEnable = value;
                    if (value)
                    {
                        (App.Current.MainWindow as MainWindow).IntiFileWatcher();
                    }
                    else
                    {
                        (App.Current.MainWindow as MainWindow).desktopWatcher.EnableRaisingEvents = false;
                    }
                }
            }
        }

        public string FileWatcherFilter { get; set; } = "*.*";

        [JsonIgnore]
        private bool _DarkModeEnable { get; set; } = true;

        public bool DarkModeEnable
        {
            get { return _DarkModeEnable; }
            set
            {
                if (value != _DarkModeEnable)
                {
                    _DarkModeEnable = value;
                    if (value)
                    {
                        foreach (Window item in App.Current.Windows)
                        {
                            Theme.RemoveDarkThemeFromWindow(item);
                        }
                    }
                }
            }
        }
    }

    public static class AppSettingsExtensions
    {
        public static AppSettings LoadSettings()
        {
            try
            {
                var dir = Catalog.SETTINGS_FILE.Split("config.json")[0];
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                if (!File.Exists(Catalog.SETTINGS_FILE)) SaveSettings(new AppSettings());
                var content = File.ReadAllText(Catalog.SETTINGS_FILE);
                return JsonSerializer.Deserialize<AppSettings>(content);
            }
            catch (Exception e)
            {
                Log.Error($"Error while loading settings:" + e.ToString());
                return new AppSettings();
            }
        }

        public static void SaveSettings(this AppSettings settings)
        {
            var content = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });

            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(Catalog.SETTINGS_FILE))) Directory.CreateDirectory(Path.GetDirectoryName(Catalog.SETTINGS_FILE));
                File.WriteAllText(Catalog.SETTINGS_FILE, content);
                Catalog.ShowInfo("数据已保存.");
            }
            catch (Exception e)
            {
                Log.Error($"Error while saving settings: " + e.ToString());
            }
        }
    }
}