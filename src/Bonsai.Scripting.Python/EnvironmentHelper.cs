using System;
using System.IO;
using System.Linq;
using Python.Runtime;
using System.Runtime.InteropServices;

namespace Bonsai.Scripting.Python
{
    static class EnvironmentHelper
    {
        public static string GetPythonDLL(string pythonHome, string path)
        {
            string searchPath = pythonHome;
            string searchPattern;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                pythonVersion = pythonVersion.Replace(".", "");
                searchPattern = $"python{pythonVersion}*";
            }
            else 
            {
                searchPath = Path.Combine(searchPath, $"../lib/python{pythonVersion}");
                searchPattern = $"libpython{pythonVersion}.so";
            }

            Console.WriteLine($"Search path: {searchPath}");
            Console.WriteLine($"Search pattern: {searchPattern}");

            return Directory
                .EnumerateFiles(searchPath, searchPattern, SearchOption.AllDirectories)
                .Select(Path.GetFileNameWithoutExtension)
                .Where(match => match.StartsWith($"python{pythonVersion}") || match.StartsWith($"libpython{pythonVersion}"))
                .Select(match => match.Replace(".", string.Empty))
                .FirstOrDefault();
            // return Directory
            //     .EnumerateFiles(pythonHome, searchPattern: "python3?*.*")
            //     .Select(Path.GetFileNameWithoutExtension)
            //     .Where(match => match.Length > "python3".Length)
            //     .Select(match => match.Replace(".", string.Empty))
            //     .FirstOrDefault();
        }

        public static void SetRuntimePath(string pythonHome)
        {
            var path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process).TrimEnd(Path.PathSeparator);
            path = string.IsNullOrEmpty(path) ? pythonHome : pythonHome + Path.PathSeparator + path;
            Environment.SetEnvironmentVariable("PATH", path, EnvironmentVariableTarget.Process);
        }

        public static string GetPythonHome(string path)
        {
            var configFileName = Path.Combine(path, "pyvenv.cfg");
            if (File.Exists(configFileName))
            {
                using var configReader = new StreamReader(File.OpenRead(configFileName));
                while (!configReader.EndOfStream)
                {
                    var line = configReader.ReadLine();
                    if (line.StartsWith("home"))
                    {
                        var parts = line.Split('=');
                        return parts[parts.Length - 1].Trim();
                    }
                }
            }
            return path;
        }

        public static string GetPythonVersionFromVenv(string path)
        {
            var configFileName = Path.Combine(path, "pyvenv.cfg");
            if (File.Exists(configFileName))
            {
                using var configReader = new StreamReader(File.OpenRead(configFileName));
                while (!configReader.EndOfStream)
                {
                    var line = configReader.ReadLine();
                    if (line.StartsWith("version"))
                    {
                        var parts = line.Split('=')[1].Trim().Split('.');
                        return $"{parts[0]}.{parts[1]}";
                    }
                }
            }
            return "";
        }

        public static string GetPythonVersionFromPythonDir(string pythonHome)
        {
            var pythonExecutableRegex = new Regex(@"python3(\d+)?(\.\d+)?", RegexOptions.IgnoreCase);
            var highestVersion = "0.0";
            
            var filesAndDirs = Directory.EnumerateFileSystemEntries(pythonHome);
            foreach (var entry in filesAndDirs)
            {
                var name = Path.GetFileName(entry);
                var match = pythonExecutableRegex.Match(name);
                if (match.Success)
                {
                    var version = match.Value.Replace("python", "").Replace("Python", "");
                    if (String.Compare(version, highestVersion) > 0)
                    {
                        highestVersion = version;
                    }
                }
            }

            return highestVersion != "0.0" ? highestVersion : null;
        }

        public static string GetPythonPath(string pythonHome, string path)
        {
            var basePath = PythonEngine.PythonPath;
            var pythonVersion = GetPythonVersion(path);
            var sitePackages = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Path.Combine(path, "Lib", "site-packages") :
                string.Join(Path.PathSeparator.ToString(), Path.Combine(path, "lib", $"python{pythonVersion}", "site-packages"), 
                    Path.Combine(path, "lib64", $"python{pythonVersion}", "site-packages"));
            if (string.IsNullOrEmpty(PythonEngine.PythonPath))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var pythonZip = Path.Combine(pythonHome, Path.ChangeExtension(Runtime.PythonDLL, ".zip"));
                    var pythonDLLs = Path.Combine(pythonHome, "DLLs");
                    var pythonLib = Path.Combine(pythonHome, "Lib");
                    var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    basePath = string.Join(Path.PathSeparator.ToString(), pythonZip, pythonDLLs, pythonLib, baseDirectory);
                }
                else
                {
                    var pythonLib = Path.Combine(pythonHome, $"../lib/python{pythonVersion}");
                    var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    basePath = string.Join(Path.PathSeparator.ToString(), pythonLib, baseDirectory);
                }
            }

            // var sitePackages = Path.Combine(path, "Lib", "site-packages");
            // var newPath = $"{basePath}{Path.PathSeparator}{path}{Path.PathSeparator}{sitePackages}";

            var newPath = string.Join(
                Path.PathSeparator.ToString(), 
                path,
                basePath,
                sitePackages
            );
            return newPath;
        }
    }
}
