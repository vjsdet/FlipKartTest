namespace FrameworkSupport
{
    public interface IEnvironmentHelper
    {
        string DisplayEnvironment { get; }

        string Environment { get; }

        bool SafeBool(string setting, bool defaultValue);

        int SafeParse(string setting, int defaultValue);

        long SafeParse(string setting, long defaultValue);

        string GetSetting(string key);

        string GetConnectionString(string key);

        CommandLineArgsHelper CLHelper { get; set; }

        public string ApplicationPhysicalPath { get; }

        bool IsUsingICUMode();
    }
}