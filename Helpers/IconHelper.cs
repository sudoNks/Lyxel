using System.Drawing;
using System.IO;
using System.Reflection;

namespace MobiladorStex.Helpers
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
            string resourceName = $"MobiladorStex.Assets.Icons.{name}.png";
            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) return null;
            // Copiar a MemoryStream para no depender del stream original
            var ms = new MemoryStream();
            stream.CopyTo(ms);
            ms.Position = 0;
            return new Bitmap(ms);
        }
    }
}
