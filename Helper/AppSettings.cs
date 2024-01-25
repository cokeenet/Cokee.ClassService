using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;

namespace Cokee.ClassService.Helper
{
    public class AppSettings
    {
        public bool MultiTouchEnable { get; set; } = true;
        public bool OfficeFunctionEnable { get; set; } = true;
        public bool EraseByPointEnable { get; set; } = true;
        public bool UseMemberAvatar { get; set; }
        public bool SideCardEnable { get; set; } = true;
        public bool AgentEnable { get; set; } = true;
        private bool _FitCurveEnable { get; set; }

        public bool FitCurveEnable
        {
            get { return _FitCurveEnable; }
            set
            {
                if (value != _FitCurveEnable)
                {
                    _FitCurveEnable = value;
                    Catalog.MainWindow.inkcanvas.DefaultDrawingAttributes.FitToCurve = value;
                }
            }
        }

        public string FileWatcherFilter { get; set; } = "*.*";

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
                        Catalog.MainWindow.IntiFileWatcher();
                    }
                    else
                    {
                        Catalog.MainWindow.desktopWatcher.EnableRaisingEvents = false;
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
                Log.Error("Error while loading settings:" + e);
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
                Log.Error("Error while saving settings: " + e);
            }
        }
    }
}