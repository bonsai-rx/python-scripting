using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Bonsai.Scripting.Python
{
    static class EnvironmentHelper
    {

        private static string GetFirstFile(string path, string pattern, HashSet<string> visitedDirectories = null)
        {
            visitedDirectories ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                var dirInfo = new DirectoryInfo(path);

                // Check if the current directory is a symbolic link
                if ((dirInfo.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
                {
                    return null;
                }

                // Avoid revisiting the same directory
                if (!visitedDirectories.Add(path))
                {
                    return null;
                }

                var files = Directory.GetFiles(path, pattern, SearchOption.TopDirectoryOnly);
                if (files.Length > 0)
                {
                    return files[0]; // Return the full path of the first found file
                }

                foreach (var directory in Directory.GetDirectories(path))
                {
                    var file = GetFirstFile(directory, pattern, visitedDirectories);
                    if (!string.IsNullOrEmpty(file))
                    {
                        return file; // Return the full path of the first found file in subdirectories
                    }
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (IOException) { } // Handle exceptions related to symbolic links and access issues

            return null; // Return null if no file is found
        }
        
        public static string GetPythonDLL(string pythonHome, string pythonVersion)
        {
            string searchPath = pythonHome;
            string searchPattern;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                pythonVersion = pythonVersion.Replace(".", "");
                searchPattern = $"python{pythonVersion}.dll";
            }
            else 
            {
                searchPath = Path.GetDirectoryName(searchPath);
                searchPattern = $"libpython{pythonVersion}.so";
            }
            return GetFirstFile(searchPath, searchPattern);
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
        public static void SetRuntimePath(string pythonHome, string path, string pythonVersion)
        {
            string systemPath = Environment.GetEnvironmentVariable("PATH")!.TrimEnd(Path.PathSeparator);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                systemPath = string.IsNullOrEmpty(systemPath) ? pythonHome : pythonHome + Path.PathSeparator + systemPath;
                Environment.SetEnvironmentVariable("PATH", systemPath, EnvironmentVariableTarget.Process);
                // Environment.SetEnvironmentVariable("PYTHONPATH", string.Join(Path.PathSeparator.ToString(),
                //     path,
                //     Path.Combine(pythonHome, $"python{pythonVersion}.zip"),
                //     Path.Combine(pythonHome, "DLLs"),
                //     Path.Combine(pythonHome, "Lib"),
                //     Path.Combine(pythonHome, "Lib", "site-packages")), EnvironmentVariableTarget.Process);   
            }
            else
            {
                systemPath = string.IsNullOrEmpty(systemPath) ? Path.Combine(path, "bin") : Path.Combine(path, "bin") + Path.PathSeparator + systemPath;
                Environment.SetEnvironmentVariable("PATH", systemPath, EnvironmentVariableTarget.Process);
                Environment.SetEnvironmentVariable("PYTHONHOME", path, EnvironmentVariableTarget.Process);
                var pythonBase = Path.GetDirectoryName(pythonHome);
                Environment.SetEnvironmentVariable("PYTHONPATH", string.Join(Path.PathSeparator.ToString(),
                    path,
                    Path.Combine(pythonBase, "lib", $"python{pythonVersion}.zip"),
                    Path.Combine(pythonBase, "lib", $"python{pythonVersion}"),
                    Path.Combine(pythonBase, "lib", $"python{pythonVersion}", "lib-dynload"),
                    Path.Combine(pythonBase, "lib", $"python{pythonVersion}", "site-packages")), EnvironmentVariableTarget.Process);   
            }
        }

        public static string GetVirtualEnvironmentPath(string path)
        {
            if (!string.IsNullOrEmpty(path)) 
            {
                Environment.SetEnvironmentVariable("VIRTUAL_ENV", path);
                return path;
            }
            var venvPath = Environment.GetEnvironmentVariable("VIRTUAL_ENV", EnvironmentVariableTarget.Process);
            if (string.IsNullOrEmpty(venvPath)) return path;
            var fullVenvPath = Path.GetFullPath(venvPath);
            return fullVenvPath;
        }

        public static void SetVirtualEnvironmentPath(string path)
        {
            if (!string.IsNullOrEmpty(path)) 
            {
                return;
            }
            Environment.SetEnvironmentVariable("VIRTUAL_ENV", path);
        }
        
        public static string GetPythonPath(string pythonHome, string path, string basePath, string pythonDLL)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (string.IsNullOrEmpty(basePath))
                {
                    var pythonZip = Path.Combine(pythonHome, Path.ChangeExtension(pythonDLL, ".zip"));
                    var pythonDLLs = Path.Combine(pythonHome, "DLLs");
                    var pythonLib = Path.Combine(pythonHome, "Lib");
                    var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    basePath = string.Join(Path.PathSeparator.ToString(), pythonZip, pythonDLLs, pythonLib, baseDirectory);
                }

                var sitePackages = Path.Combine(path, "Lib", "site-packages");
                return $"{basePath}{Path.PathSeparator}{path}{Path.PathSeparator}{sitePackages}";
            } 
            else
            {
                return basePath + Path.PathSeparator + Environment.GetEnvironmentVariable("PYTHONPATH", EnvironmentVariableTarget.Process);
            }
        }
    }
}
