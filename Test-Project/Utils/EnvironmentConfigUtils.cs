using FrameworkSupport;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_Project.Utils
{
    public static class EnvironmentConfigUtils
    {
        public const string ConfigNameFile = "appsettings.json";

        public static EnvironmentHelper GetEnvironmentForConfig()
        {
            string configName = string.Empty;
            if (File.Exists(Path.Join(AppDomain.CurrentDomain.BaseDirectory, ConfigNameFile)))
            {
                // Read the running config name from file env.config created on the PostBuild task
                configName = File.ReadAllText(Path.Join(AppDomain.CurrentDomain.BaseDirectory, ConfigNameFile)).Trim();
            }

            IConfiguration config = EnvironmentHelperUtils.GetCurrentConfiguration(configName, useCache: false);
            NameValueCollection appSettings = EnvironmentHelperUtils.GetAppSettings(config);
            NameValueCollection connectionStrings = EnvironmentHelperUtils.GetConnectionStrings(config);

            return new EnvironmentHelper(appSettings, connectionStrings, config);
        }
    }
}
