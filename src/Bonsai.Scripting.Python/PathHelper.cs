using System.IO;

namespace Bonsai.Scripting.Python
{
    static class PathHelper
    {
        static bool IsDirectorySeparator(char c)
        {
            return c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar;
        }

        static bool IsRoot(string path)
        {
            return !string.IsNullOrEmpty(path) &&
                    Path.IsPathRooted(path) &&
                    path.Length == Path.GetPathRoot(path).Length;
        }

        internal static string TrimEndingDirectorySeparator(string path)
        {
            if (!string.IsNullOrEmpty(path) && IsDirectorySeparator(path[path.Length - 1]) && !IsRoot(path))
            {
                path = path.Substring(0, path.Length - 1);
            }

            return path;
        }
    }
}
