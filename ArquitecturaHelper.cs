using System;
using System.IO;

namespace LyXel
{
    public static class ArquitecturaHelper
    {
        private static bool? _override = null;

        public static bool ModoCompatibilidad
        {
            get => _override ?? false;
            set => _override = value;
        }

        public static string RutaScrcpy =>
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "bin", "scrcpy",
                ModoCompatibilidad ? "x86" : "x86_64",
                "scrcpy.exe");

        public static bool ScrcpyDisponible() => File.Exists(RutaScrcpy);
    }
}
