using System;
using System.IO;
using System.Runtime.InteropServices;
using Python.Runtime;

namespace Bonsai.Scripting.Python
{
    static class EnvironmentHelper
    {
        public static string GetPythonDLL(EnvironmentConfig config)
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? $"python{config.PythonVersion.Replace(".", string.Empty)}.dll"
                : $"libpython{config.PythonVersion}.so";
        }

        public static void SetRuntimePath(string pythonHome)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process).TrimEnd(Path.PathSeparator);
                path = string.IsNullOrEmpty(path) ? pythonHome : pythonHome + Path.PathSeparator + path;
                Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.Process);
            }
        }

        public static string GetEnvironmentPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = Environment.GetEnvironmentVariable("VIRTUAL_ENV", EnvironmentVariableTarget.Process);
                if (path == null) return path;
            }

            return Path.GetFullPath(path);
        }

        public static EnvironmentConfig GetEnvironmentConfig(string path)
        {
            var configFileName = Path.Combine(path, "pyvenv.cfg");
            if (File.Exists(configFileName))
            {
                return EnvironmentConfig.FromConfigFile(configFileName);
            }
            else
            {
                var pythonHome = path;
                var pythonVersion = string.Empty;
                const string DefaultPythonName = "python";
                if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var baseDirectory = Directory.GetParent(path).Parent;
                    pythonHome = Path.Combine(baseDirectory.FullName, "bin");
                }

                var pythonName = Path.GetFileName(path);
                var pythonVersionIndex = pythonName.LastIndexOf(DefaultPythonName, StringComparison.OrdinalIgnoreCase);
                if (pythonVersionIndex >= 0)
                {
                    pythonVersion = pythonName.Substring(pythonVersionIndex + DefaultPythonName.Length);
                }

                return new EnvironmentConfig(pythonHome, pythonVersion);
            }
        }

        public static string GetPythonPath(EnvironmentConfig config)
        {
            string sitePackages;
            var basePath = PythonEngine.PythonPath;
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (string.IsNullOrEmpty(basePath))
                {
                    var pythonZip = Path.Combine(config.PythonHome, Path.ChangeExtension(Runtime.PythonDLL, ".zip"));
                    var pythonDLLs = Path.Combine(config.PythonHome, "DLLs");
                    var pythonLib = Path.Combine(config.PythonHome, "Lib");
                    basePath = string.Join(Path.PathSeparator.ToString(), pythonZip, pythonDLLs, pythonLib, baseDirectory);
                }

                sitePackages = Path.Combine(config.Path, "Lib", "site-packages");
                if (config.IncludeSystemSitePackages && config.Path != config.PythonHome)
                {
                    var systemSitePackages = Path.Combine(config.PythonHome, "Lib", "site-packages");
                    sitePackages = $"{sitePackages}{Path.PathSeparator}{systemSitePackages}";
                }
            }
            else
            {
                if (string.IsNullOrEmpty(basePath))
                {
                    var pythonBase = Path.GetDirectoryName(config.PythonHome);
                    pythonBase = Path.Combine(pythonBase, "lib", $"python{config.PythonVersion}");
                    var pythonLibDynload = Path.Combine(pythonBase, "lib-dynload");
                    basePath = string.Join(Path.PathSeparator.ToString(), pythonBase, pythonLibDynload, baseDirectory);
                }

                sitePackages = Path.Combine(config.Path, "lib", $"python{config.PythonVersion}", "site-packages");
                if (config.IncludeSystemSitePackages)
                {
                    var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    var localFolder = Directory.GetParent(localAppData).FullName;
                    var systemSitePackages = Path.Combine(localFolder, "lib", $"python{config.PythonVersion}", "site-packages");
                    sitePackages = $"{sitePackages}{Path.PathSeparator}{systemSitePackages}";
                }
            }

            return $"{basePath}{Path.PathSeparator}{config.Path}{Path.PathSeparator}{sitePackages}";
        }
    }
}
