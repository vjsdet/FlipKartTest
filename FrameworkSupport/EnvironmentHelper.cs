using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Threading;

namespace FrameworkSupport
{
    public class EnvironmentHelper : IEnvironmentHelper
    {
        public enum AzureConnectionStringType
        {
            SqlDatabases = 0,
            SQLServer = 1,
            MySQL = 2,
            Custom = 3
        }

        private NameValueCollection _appSettings;
        private NameValueCollection _connectionStrings;
        private IConfiguration _fullConfiguration;

        private delegate void ThreadQuery(out int threads, out int ioThreads);

        private Dictionary<AzureConnectionStringType, string> connStringPrefixes = new Dictionary<AzureConnectionStringType, string>()
        {
            { AzureConnectionStringType.SqlDatabases, "SQLAZURECONNSTR_" },
            { AzureConnectionStringType.SQLServer, "SQLCONNSTR_" },
            { AzureConnectionStringType.MySQL, "MYSQLCONNSTR_" },
            { AzureConnectionStringType.Custom, "CUSTOMCONNSTR_" },
        };

        public IConfiguration FullConfiguration
        { get { return _fullConfiguration; } }

        public string ConnectionStringTypeToPrefix(AzureConnectionStringType t) => connStringPrefixes[t];

        /// <summary>
        /// Initialize the helper with your collection
        /// </summary>
        /// <param name="appSettings">Your AppSettings as defined in Environment vars, CLI args or appSettings.json</param>
        public EnvironmentHelper(NameValueCollection appSettings) : this(appSettings, new NameValueCollection(), null) { }

        /// Initialize the helper with your collections
        /// </summary>
        /// <param name="appSettings">Your AppSettings as defined in Environment vars, CLI args or appSettings.json</param>
        public EnvironmentHelper(NameValueCollection appSettings, NameValueCollection connectionStrings, IConfiguration mainConfig)
        {
            _appSettings = appSettings;
            _connectionStrings = connectionStrings;
            _fullConfiguration = mainConfig;
        }

        /// <summary>
        /// For .NET Core update - not sure if this is possible to share between workers and web. To ponder.
        /// </summary>
        public string ApplicationPhysicalPath
        {
            get
            {
                string path = Directory.GetCurrentDirectory();

                return path;
            }
        }

        public string Environment
        {
            get
            {
                return GetSetting("Environment") ?? "NotSet";
            }
        }

        public string DisplayEnvironment
        {
            get
            {
                return GetSetting("DisplayEnvironment") ?? Environment;
            }
        }

        public bool SafeBool(string setting, bool defaultValue)
        {
            bool result;
            return bool.TryParse(GetSetting(setting), out result) ? result : defaultValue;
        }

        /// <summary>
        /// Get a setting value parsed into its numeric equivalent, or a default
        /// </summary>
        /// <param name="setting">The name of the settings (note: not the value itself)</param>
        /// <param name="defaultValue">A default value if the setting is missing</param>
        /// <returns>The value</returns>
        public long SafeParse(string setting, long defaultValue)
        {
            long result;
            return long.TryParse(GetSetting(setting), out result) ? result : defaultValue;
        }

        public int SafeParse(string setting, int defaultValue)
        {
            return (int)SafeParse(setting, (long)defaultValue);
        }

        public uint SafeParseUint(string setting, uint defaultValue)
        {
            return (uint)SafeParse(setting, defaultValue);
        }

        /// <summary>
        /// Helper class available for CLI apps, otherwise null
        /// </summary>
        public CommandLineArgsHelper CLHelper { get; set; }

        private static (int threads, int ioThreads) GetThreadInfo(ThreadQuery q)
        {
            int threads, ioThreads;
            q(out threads, out ioThreads);

            return (threads, ioThreads);
        }

        public bool IsUsingICUMode()
        {
            SortVersion sortVersion = CultureInfo.InvariantCulture.CompareInfo.Version;
            byte[] bytes = sortVersion.SortId.ToByteArray();
            int version = (bytes[3] << 24) | (bytes[2] << 16) | (bytes[1] << 8) | bytes[0];
            return version != 0 && version == sortVersion.FullVersion;
        }

        public static (string threads, string ioThreads) ThreadEnvironmentDescription
        {
            get
            {
                var min = GetThreadInfo(ThreadPool.GetMinThreads);
                var max = GetThreadInfo(ThreadPool.GetMaxThreads);
                var avail = GetThreadInfo(ThreadPool.GetAvailableThreads);

                return (threads: $"MinThreads = {min.threads}, MaxThreads = {max.threads}, Available = {avail.threads}",
                    ioThreads: $"MinIoThreads = {min.ioThreads}, MaxIoThreads = {max.ioThreads}, AvailableIo = {avail.ioThreads}");
            }
        }

        /// <summary>
        /// Get an app setting for any environment. Precedence: 1) command line, 2) Azure setting (APPSETTING_ prefixed), 3) normal env variable,
        /// 4) app.config or web.config file
        /// </summary>
        /// <param name="key"></param>
        /// <returns>The setting if found, or null</returns>
        public string GetSetting(string key)
        {
            string result = ((CLHelper?.ContainsKey(key) ?? false) ? CLHelper[key] : null)
                ?? System.Environment.GetEnvironmentVariable($"APPSETTING_{key}")
                ?? System.Environment.GetEnvironmentVariable($"DOTNET_{key}")
                ?? System.Environment.GetEnvironmentVariable(key)
                ?? _appSettings[key];

            return result;
        }

        /// <summary>
        /// Get a connection string for any web environment. Precedence: 1) normal env variable, 2) local config file
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetConnectionString(string key)
        {
            return ((CLHelper?.ContainsKey(key) ?? false) ? CLHelper[key] : null)
                ?? System.Environment.GetEnvironmentVariable(key)
                ?? _connectionStrings[key];
        }

        /// <summary>
        /// Get a connection string for worker environment. Precedence: 1) Azure setting (type prefixed), 2) normal env variable,
        /// 3) local config file
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetConnectionString(AzureConnectionStringType type, string key)
        {
            // going to be some more work here I think
            return System.Environment.GetEnvironmentVariable($"{ConnectionStringTypeToPrefix(type)}{key}") ?? GetConnectionString(key);
        }

        public static string GetConnectionStringFromAppSetting(string key)
        {
            // Load ConnectionStrings from config
            IConfiguration config = EnvironmentHelperUtils.GetCurrentConfiguration();

            EnvironmentHelper environment = new EnvironmentHelper(new NameValueCollection(), new NameValueCollection(), config);

            NameValueCollection connStrings = EnvironmentHelperUtils.GetConnectionStrings(config);
            environment._connectionStrings = connStrings;

            return environment.GetConnectionString(key);
        }

        public static EnvironmentHelper GetEnvironment(IConfiguration config = null)
        {
            // Load ConnectionStrings from config
            config ??= EnvironmentHelperUtils.GetCurrentConfiguration();
            NameValueCollection appSettings = EnvironmentHelperUtils.GetAppSettings(config);
            NameValueCollection connStrings = EnvironmentHelperUtils.GetConnectionStrings(config);

            return new EnvironmentHelper(appSettings, connStrings, config);
        }
    }

    public static class EnvironmentHelperUtils
    {
        private static MemoryCache _cache = MemoryCache.Default;

        public static NameValueCollection ToNameValueCollection(this ConnectionStringSettingsCollection settings)
        {
            NameValueCollection result = new NameValueCollection();
            foreach (ConnectionStringSettings s in settings)
            {
                result.Add(s.Name, s.ConnectionString);
            }
            return result;
        }

        public static NameValueCollection ToNameValueCollection(this IConfigurationSection section, string sectionKeyName)
        {
            Func<NameValueCollection> buildCollection = () =>
            {
                NameValueCollection result = new NameValueCollection();
                IEnumerable<KeyValuePair<string, string>> settings = section.AsEnumerable();
                if (settings.Count() > 0)
                {
                    string sectionName = settings.First().Key.Split(":")[0];
                    foreach (KeyValuePair<string, string> setting in settings)
                    {
                        if (sectionName == setting.Key)
                        {
                            continue; // Ignore section container, only add setting fields
                        }

                        // dotnet core setting format => "SectionName:SubsectionName:SettingName"
                        string settingName = setting.Key.Split($"{sectionName}:")[1];
                        result.Add(settingName, setting.Value);
                    }
                }
                return result;
            };

            string cacheKey = $"EnvironmentHelper-ToNameValueCollection-{sectionKeyName}";

            return GetOrAddItem(cacheKey, DateTime.UtcNow.AddMinutes(60), buildCollection);
        }

        public static IConfiguration GetCurrentConfiguration(string configName = "", bool useCache = true)
        {
            Func<IConfiguration> buildConfig = () =>
            {
                IConfigurationBuilder builder = configName != null && File.Exists($"{Directory.GetCurrentDirectory()}/appsettings.{configName}.json")
                    ? new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json")
                        .AddJsonFile($"appsettings.{configName}.json")
                        .AddEnvironmentVariables()
                    : new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .AddEnvironmentVariables();
                // Load Environment-specific appSettings.json by adding the environment name as an Environment Variable named 'DOTNET_ENVIRONMENT'

                var configuration = builder.Build();
                return configuration;
            };

            if (!useCache)
            {
                return buildConfig.Invoke();
            }

            string cacheKey = $"EnvironmentHelper-GetCurrentConfiguration{configName}";

            return GetOrAddItem(cacheKey, DateTime.UtcNow.AddMinutes(60), buildConfig);
        }

        private static NameValueCollection ReadSectionFromConfig(IConfiguration config, string name)
        {
            NameValueCollection settings = config.GetSection(name).ToNameValueCollection(name);
            return settings;
        }

        public static NameValueCollection GetAppSettings(IConfiguration config) => ReadSectionFromConfig(config, "AppSettings");

        public static NameValueCollection GetConnectionStrings(IConfiguration config) => ReadSectionFromConfig(config, "ConnectionStrings");

        private static T GetOrAddItem<T>(string key, DateTimeOffset endDate, Func<T> valueFactory)
        {
            var newValue = new Lazy<T>(valueFactory);
            var oldValue = _cache.AddOrGetExisting(key, newValue, endDate) as Lazy<T>;
            try
            {
                return (oldValue ?? newValue).Value;
            }
            catch
            {
                _cache.Remove(key);
                throw;
            }
        }
    }
}