using System.IO;

namespace Bonsai.Scripting.Python
{
    internal class EnvironmentConfig
    {
        private EnvironmentConfig()
        {
        }

        public EnvironmentConfig(string pythonHome, string pythonVersion)
        {
            Path = pythonHome;
            PythonHome = pythonHome;
            PythonVersion = pythonVersion;
            IncludeSystemSitePackages = true;
        }

        public string Path { get; private set; }

        public string PythonHome { get; private set; }

        public string PythonVersion { get; private set; }

        public bool IncludeSystemSitePackages { get; private set; }

        public static EnvironmentConfig FromConfigFile(string configFileName)
        {
            var config = new EnvironmentConfig
            {
                Path = System.IO.Path.GetDirectoryName(configFileName)
            };
            using var configReader = new StreamReader(File.OpenRead(configFileName));
            while (!configReader.EndOfStream)
            {
                var line = configReader.ReadLine();

                if (line.StartsWith("home"))
                {
                    config.PythonHome = GetConfigValue(line);
                }
                else if (line.StartsWith("include-system-site-packages"))
                {
                    config.IncludeSystemSitePackages = bool.Parse(GetConfigValue(line));
                }
                else if (line.StartsWith("version"))
                {
                    var pythonVersion = GetConfigValue(line);
                    if (!string.IsNullOrEmpty(pythonVersion))
                    {
                        pythonVersion = pythonVersion.Substring(0, pythonVersion.LastIndexOf('.'));
                    }
                    config.PythonVersion = pythonVersion;
                }
            }

            return config;
        }

        private static string GetConfigValue(string line)
        {
            var parts = line.Split('=');
            return parts.Length > 1 ? parts[1].Trim() : string.Empty;
        }
    }
}
