using System;
using System.IO;
using System.Linq;
using Python.Runtime;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Bonsai.Scripting.Python
{
    static class EnvironmentHelper
    {
        public static string GetPythonDLL(string pythonVersion)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (string.IsNullOrEmpty(pythonVersion))
                {
                    pythonVersion = "3";
                }
                pythonVersion = pythonVersion.Replace(".", "");
                return $"python{pythonVersion}";
            }
            else 
            {
                return $"libpython{pythonVersion}.so";

            }
        }

        public static bool GetIncludeSystemSitePackages(string path)
        {
            bool includeGlobalPackages = true;

            var configFileName = !string.IsNullOrEmpty(path) ? Path.Combine(path, "pyvenv.cfg") : null;
            if (File.Exists(configFileName))
            {
                using var configReader = new StreamReader(File.OpenRead(configFileName));
                while (!configReader.EndOfStream)
                {
                    var line = configReader.ReadLine();
                    if (line.StartsWith("include-system-site-packages"))
                    {
                        includeGlobalPackages = bool.Parse(line.Split('=')[1].Trim()) ;
                        break;
                    }
                }
            }

            return includeGlobalPackages;
        }

        public static string GetPythonHome(string path)
        {
            string pythonHome = null;

            var configFileName = !string.IsNullOrEmpty(path) ? Path.Combine(path, "pyvenv.cfg") : null;
            if (File.Exists(configFileName))
            {
                using var configReader = new StreamReader(File.OpenRead(configFileName));
                while (!configReader.EndOfStream)
                {
                    var line = configReader.ReadLine();
                    if (line.StartsWith("home"))
                    {
                        var parts = line.Split('=');
                        pythonHome = parts[parts.Length - 1].Trim();
                        break;
                    }
                }
            }
            else
            {
                string pythonExecutableName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "python.exe" : "python3";

                var systemPath = Environment.GetEnvironmentVariable("PATH");
                var paths = systemPath.Split(Path.PathSeparator);

                var pythonExecutableHome = paths.Select(p => Path.Combine(p, pythonExecutableName))
                                    .FirstOrDefault(File.Exists);

                if (pythonExecutableHome != null)
                {
                    pythonHome = Path.GetDirectoryName(pythonExecutableHome);
                }

            }
            return pythonHome;
        }

        public static string GetPythonVersion(string path, string pythonHome)
        {
            string version = "0.0";

            var configFileName = !string.IsNullOrEmpty(path) ? Path.Combine(path, "pyvenv.cfg") : null;
            if (File.Exists(configFileName))
            {
                using var configReader = new StreamReader(File.OpenRead(configFileName));
                while (!configReader.EndOfStream)
                {
                    var line = configReader.ReadLine();
                    if (line.StartsWith("version"))
                    {
                        var parts = line.Split('=')[1].Trim().Split('.');
                        version = $"{parts[0]}.{parts[1]}";
                        break;
                    }
                }
            }
            else
            {
                var pythonExecutableRegex = new Regex(@"python3(\d+)?(\.\d+)?", RegexOptions.IgnoreCase);

                var filesAndDirs = Directory.EnumerateFileSystemEntries(pythonHome);
                foreach (var entry in filesAndDirs)
                {
                    var name = Path.GetFileName(entry);
                    var match = pythonExecutableRegex.Match(name);
                    if (match.Success)
                    {
                        var matchedVersion = match.Value.Replace("python", "").Replace("Python", "");
                        if (String.Compare(matchedVersion, version) > 0)
                        {
                            version = matchedVersion;
                        }
                    }
                }                
            }
            return version != "0.0" ? version : null;
        }

        public static string GetPythonPath(string pythonHome, string path, string pythonVersion)
        {
            string sitePackages = null;
            string basePath = null;

            if (!string.IsNullOrEmpty(path))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    sitePackages = Path.Combine(path, "Lib", "site-packages");
                }
                else
                {
                    sitePackages = string.Join(Path.PathSeparator.ToString(), Path.Combine(path, "lib", $"python{pythonVersion}", "site-packages"), 
                        Path.Combine(path, "lib64", $"python{pythonVersion}", "site-packages"));
                }
            }

            string[] basePaths = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // var pythonZip = Path.Combine(pythonHome, Path.ChangeExtension(Runtime.PythonDLL, ".zip"));
                var pythonDLLs = Path.Combine(pythonHome, "DLLs");
                var pythonLib = Path.Combine(pythonHome, "Lib");
                bool includeSystemSitePackages = GetIncludeSystemSitePackages(path);
                var pythonPackages = includeSystemSitePackages ? Path.Combine(pythonLib, "site-packages") : null;
                // var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                basePaths = new string[] { 
                    pythonDLLs, 
                    pythonLib,
                    pythonPackages
                };
            }
            else
            {
                var pythonMajorVersion = pythonVersion.Split('.')[0];
                var pythonLib = Path.Combine(Path.GetDirectoryName(pythonHome), "lib", $"python{pythonVersion}");
                bool includeSystemSitePackages = GetIncludeSystemSitePackages(path);
                var pythonPackages = includeSystemSitePackages ? Path.Combine(Path.GetDirectoryName(pythonHome), "lib", $"python{pythonMajorVersion}", "dist-packages") : null;
                // var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

                basePaths = new string[] {
                    pythonLib,
                    pythonPackages
                };
            }
            
            basePath = string.Join(Path.PathSeparator.ToString(), basePaths.Where(p => p != null));

            string[] paths = {
                basePath,
                sitePackages
            };

            var pythonPath = string.Join(Path.PathSeparator.ToString(), paths.Where(p => p != null));

            return pythonPath;
        }

        public static void SetEnvironmentPath(string pythonHome)
        {
                var systemPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process).TrimEnd(Path.PathSeparator);
                if (!systemPath.Split(Path.PathSeparator).Contains(pythonHome, StringComparer.OrdinalIgnoreCase))
                {
                    systemPath = string.IsNullOrEmpty(systemPath) ? pythonHome : pythonHome + Path.PathSeparator + systemPath;
                }
                Environment.SetEnvironmentVariable("PATH", systemPath, EnvironmentVariableTarget.Process);
        }
    }
}
