using System;
using System.IO;
using System.Linq;
using Python.Runtime;

namespace Bonsai.Scripting.Python
{
    static class EnvironmentHelper
    {
        public static string GetPythonDLL(string path)
        {
            return Directory
                .EnumerateFiles(path, searchPattern: "python3?*.*")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(match => match.Length > "python3".Length)
                .Select(match => match.Replace(".", string.Empty))
                .FirstOrDefault();
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

        public static string GetPythonPath(string path)
        {
            var sitePackages = Path.Combine(path, "Lib", "site-packages");
            return $"{PythonEngine.PythonPath}{Path.PathSeparator}{path}{Path.PathSeparator}{sitePackages}";
        }
    }
}
