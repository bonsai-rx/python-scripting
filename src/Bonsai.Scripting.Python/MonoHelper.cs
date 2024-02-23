using System;

namespace Bonsai.Scripting.Python
{
    static class MonoHelper
    {
        internal static readonly bool IsRunningOnMono = Type.GetType("Mono.Runtime") != null;

        public static string GetRealPath(string path)
        {
            var unixPath = Type.GetType("Mono.Unix.UnixPath, Mono.Posix");
            if (unixPath == null)
            {
                throw new InvalidOperationException("No compatible Mono.Posix implementation was found.");
            }

            var getRealPath = unixPath.GetMethod(nameof(GetRealPath));
            if (getRealPath == null)
            {
                throw new InvalidOperationException($"No compatible {nameof(GetRealPath)} method was found.");
            }

            return (string)getRealPath.Invoke(null, new[] { path });
        }
    }
}
