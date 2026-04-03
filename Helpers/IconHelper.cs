using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace LyXel.Helpers
{
    internal static class IconHelper
    {
        private static readonly ConcurrentDictionary<string, Image?> _cache = new();

        /// <summary>
        /// Devuelve el icono embebido por nombre (sin extensión), p.ej. "ic_home".
        /// El resultado se cachea: la carga desde assembly ocurre solo una vez por nombre.
        /// </summary>
        public static Image? Get(string name) =>
            _cache.GetOrAdd(name, LoadImage);

        private static Image? LoadImage(string key)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = $"LyXel.Assets.Icons.{key}.png";
            using Stream? stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) return null;
            // Copio a MemoryStream para no depender del stream del assembly que se cierra
            var ms = new MemoryStream();
            stream.CopyTo(ms);
            ms.Position = 0;
            return new Bitmap(ms);
        }

        /// <summary>
        /// Libera todas las imágenes cacheadas y vacía el diccionario.
        /// Llamar antes de cerrar la app si se desea liberar recursos explícitamente.
        /// </summary>
        public static void ClearCache()
        {
            foreach (var img in _cache.Values)
                img?.Dispose();
            _cache.Clear();
        }
    }
}
