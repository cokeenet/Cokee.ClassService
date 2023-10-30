using System;
using System.IO;
using System.Text.Json;
using System.Windows.Media.Animation;
using Serilog;
using Wpf.Ui.Appearance;

namespace Cokee.ClassService.Helper
{
    public class AppSettings
    {
        public bool MultiTouchEnable { get; set; } = true;
        public bool PPTFunctionEnable { get; set; } = true;
        public bool EraseByPointEnable { get; set; } = false;
        public bool UseMemberAvatar { get; set; } = false;
        private bool _darkModeEnable = true;
        public bool DarkModeEnable
        {
            get { return _darkModeEnable; }
            set
            {
                if (value != _darkModeEnable)
                {
                    _darkModeEnable = value;
                    if (value) 
                    {
                        Environment.Exit(0);
                        //Theme.Apply(ThemeType.Dark); this.SaveSettings();
                    }
                    else
                    {
                        //Theme.Apply(ThemeType.Light); this.SaveSettings();
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
                var dir = Catalog.SETTINGS_FILE_NAME.Split("config.json")[0];
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                if (!File.Exists(Catalog.SETTINGS_FILE_NAME)) SaveSettings(new AppSettings());
                var content = File.ReadAllText(Catalog.SETTINGS_FILE_NAME);
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
                var dir = Catalog.SETTINGS_FILE_NAME.Split("config.json")[0];
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(Catalog.SETTINGS_FILE_NAME, content);
            }
            catch (Exception e)
            {
                Log.Error($"Error while saving settings: " + e.ToString());
            }
        }
    }
}
