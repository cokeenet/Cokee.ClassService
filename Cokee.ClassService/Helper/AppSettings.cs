using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

using Cokee.ClassService.Shared;

using Serilog;

namespace Cokee.ClassService.Helper
{
    /// <summary>
    /// 应用程序配置类，存储全局设置项
    /// Ai润色202510250109--bycokee
    /// </summary>
    public class AppSettings
    {
        #region 基础配置项

        /// <summary>
        /// 是否启用多点触控
        /// </summary>
        public bool MultiTouchEnable { get; set; } = true;

        /// <summary>
        /// 是否启用Office功能（PPT批注等）
        /// </summary>
        public bool OfficeFunctionEnable { get; set; } = true;

        /// <summary>
        /// 是否启用点擦除模式（橡皮擦）
        /// </summary>
        public bool EraseByPointEnable { get; set; } = true;

        /// <summary>
        /// 是否使用成员头像
        /// </summary>
        public bool UseMemberAvatar { get; set; } = false;

        /// <summary>
        /// 是否启用侧边功能卡片
        /// </summary>
        public bool SideCardEnable { get; set; } = true;

        /// <summary>
        /// 是否启用代理功能
        /// </summary>
        public bool AgentEnable { get; set; } = false;

        /// <summary>
        /// 登录状态
        /// </summary>
        public string LoginState { get; set; } = string.Empty;

        /// <summary>
        /// 倒计时日期
        /// </summary>
        public DateTime? CountDownDate { get; set; }

        /// <summary>
        /// 倒计时名称
        /// </summary>
        public string CountDownName { get; set; } = string.Empty;

        /// <summary>
        /// 是否启用桌面背景窗口
        /// </summary>
        public bool DesktopBgWin { get; set; } = true;

        /// <summary>
        /// 文件监控器过滤规则（默认监控所有文件）
        /// </summary>
        public string FileWatcherFilter { get; set; } = "*.*";

        #endregion

        #region 文件监控器配置（带状态联动）

        /// <summary>
        /// 文件监控器启用状态（私有字段，通过属性控制）
        /// </summary>
        [JsonIgnore]
        private bool _fileWatcherEnable = false;

        /// <summary>
        /// 是否启用文件监控器（设置时自动联动启动/停止监控）
        /// </summary>
        public bool FileWatcherEnable
        {
            get => _fileWatcherEnable;
            set
            {
                if (value == _fileWatcherEnable) return;

                _fileWatcherEnable = value;
                var mainWindow = Catalog.MainWindow;

                if (mainWindow == null) return;

                // 根据状态启动或停止文件监控
                if (value)
                {
                    mainWindow.InitFileWatcher();
                }
                else
                {
                    mainWindow._desktopWatcher.EnableRaisingEvents = false;
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// AppSettings的扩展方法类，提供配置加载与保存功能
    /// </summary>
    public static class AppSettingsExtensions
    {
        /// <summary>
        /// 从配置文件加载应用设置
        /// </summary>
        /// <returns>加载后的AppSettings实例</returns>
        public static AppSettings LoadSettings()
        {
            try
            {
                // 确保配置目录存在
                string configDir = Path.GetDirectoryName(Catalog.SETTINGS_FILE);
                FileSystemHelper.DirHelper.MakeExist(configDir);

                // 配置文件不存在时，创建默认配置
                if (!File.Exists(Catalog.SETTINGS_FILE))
                {
                    new AppSettings().Save();
                }

                // 读取并反序列化配置文件
                string configContent = File.ReadAllText(Catalog.SETTINGS_FILE);
                return JsonSerializer.Deserialize<AppSettings>(configContent) ?? new AppSettings();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "加载配置文件时发生错误");
                return new AppSettings(); // 异常时返回默认配置
            }
        }

        /// <summary>
        /// 将应用设置保存到配置文件
        /// </summary>
        /// <param name="settings">要保存的AppSettings实例</param>
        public static void Save(this AppSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            try
            {
                // 序列化配置（带缩进，便于人工编辑）
                JsonSerializerOptions options = new()
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull // 忽略null值字段
                };
                string configContent = JsonSerializer.Serialize(settings, options);

                // 确保配置目录存在并写入文件
                string configDir = Path.GetDirectoryName(Catalog.SETTINGS_FILE);
                FileSystemHelper.DirHelper.MakeExist(configDir);
                File.WriteAllText(Catalog.SETTINGS_FILE, configContent);

                // 显示保存成功通知
                Catalog.ShowInfo("配置保存成功", "应用设置已更新并保存");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "保存配置文件时发生错误");
            }
        }
    }
}