using System.Configuration;

namespace DeepCool_Display.Properties
{
    internal sealed partial class Settings : ApplicationSettingsBase
    {
        private static readonly Settings _default = (Settings)Synchronized(new Settings());

        public static Settings Default => _default;

        [UserScopedSetting]
        [DefaultSettingValue("false")]
        public bool ShowCpuUsage
        {
            get => (bool)this["ShowCpuUsage"];
            set => this["ShowCpuUsage"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("false")]
        public bool UseFahrenheit
        {
            get => (bool)this["UseFahrenheit"];
            set => this["UseFahrenheit"] = value;
        }

        [UserScopedSetting]
        [DefaultSettingValue("1")]
        public int UpdateIntervalSeconds
        {
            get => (int)this["UpdateIntervalSeconds"];
            set => this["UpdateIntervalSeconds"] = value;
        }
    }
}