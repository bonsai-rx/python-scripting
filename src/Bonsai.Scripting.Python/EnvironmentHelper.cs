using System;
using System.IO;
using System.Runtime.InteropServices;
using Python.Runtime;

namespace Bonsai.Scripting.Python
{
    static class EnvironmentHelper
    {
        public static string GetPythonDLL(string pythonVersion)
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? $"python{pythonVersion.Replace(".", string.Empty)}.dll"
                : $"libpython{pythonVersion}.so";
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

        public static string GetVirtualEnvironmentPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = Environment.GetEnvironmentVariable("VIRTUAL_ENV", EnvironmentVariableTarget.Process);
                if (path == null) return path;
            }

            return Path.GetFullPath(path);
        }

        public static string GetPythonHome(string path, out string pythonVersion)
        {
            var pythonHome = path;
            pythonVersion = string.Empty;
            var configFileName = Path.Combine(path, "pyvenv.cfg");
            if (File.Exists(configFileName))
            {
                using var configReader = new StreamReader(File.OpenRead(configFileName));
                while (!configReader.EndOfStream)
                {
                    var line = configReader.ReadLine();
                    static string GetConfigValue(string line)
                    {
                        var parts = line.Split('=');
                        return parts.Length > 1 ? parts[1].Trim() : string.Empty;
                    }

                    if (line.StartsWith("home"))
                    {
                        pythonHome = GetConfigValue(line);
                    }
                    else if (line.StartsWith("version"))
                    {
                        pythonVersion = GetConfigValue(line);
                        if (!string.IsNullOrEmpty(pythonVersion))
                        {
                            pythonVersion = pythonVersion.Substring(0, pythonVersion.LastIndexOf('.'));
                        }
                    }
                }
            }

            return pythonHome;
        }

        public static string GetPythonPath(string pythonHome, string pythonVersion, string path)
        {
            string sitePackages;
            var basePath = PythonEngine.PythonPath;
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (string.IsNullOrEmpty(basePath))
                {
                    var pythonZip = Path.Combine(pythonHome, Path.ChangeExtension(Runtime.PythonDLL, ".zip"));
                    var pythonDLLs = Path.Combine(pythonHome, "DLLs");
                    var pythonLib = Path.Combine(pythonHome, "Lib");
                    basePath = string.Join(Path.PathSeparator.ToString(), pythonZip, pythonDLLs, pythonLib, baseDirectory);
                }

                sitePackages = Path.Combine(path, "Lib", "site-packages");
            }
            else
            {
                if (string.IsNullOrEmpty(basePath))
                {
                    var pythonBase = Path.GetDirectoryName(pythonHome);
                    pythonBase = Path.Combine(pythonBase, "lib", $"python{pythonVersion}");
                    var pythonLibDynload = Path.Combine(pythonBase, "lib-dynload");
                    basePath = string.Join(Path.PathSeparator.ToString(), pythonBase, pythonLibDynload, baseDirectory);
                }

                sitePackages = Path.Combine(path, "lib", $"python{pythonVersion}", "site-packages");
            }

            return $"{basePath}{Path.PathSeparator}{path}{Path.PathSeparator}{sitePackages}";
        }
    }
}
