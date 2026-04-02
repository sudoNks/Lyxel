using System.Drawing;
using System.IO;
using System.Reflection;

namespace LyXel.Helpers
{
    internal static class IconHelper
    {
        /// <summary>
        /// Carga un icono embebido por nombre (sin extensión), p.ej. "ic_home".
        /// Devuelve null si el recurso no existe.
        /// </summary>
        public static Image? Get(string name)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = $"LyXel.Assets.Icons.{name}.png";
            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) return null;
            // Copio a MemoryStream para no depender del stream del assembly que se cierra
            var ms = new MemoryStream();
            stream.CopyTo(ms);
            ms.Position = 0;
            return new Bitmap(ms);
        }
    }
}
