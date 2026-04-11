using System.Drawing;

namespace LyXel.Helpers
{
    /// <summary>
    /// Centralizes all icon references for the project.
    /// To swap an icon, edit only the corresponding property here.
    /// </summary>
    internal static class IconMap
    {
        // Iconos de navegación del sidebar
        public static Image? Home         => IconHelper.Get("ic_home");
        public static Image? Controller   => IconHelper.Get("ic_controller");
        public static Image? Video        => IconHelper.Get("ic_video");
        public static Image? Screen       => IconHelper.Get("ic_screen");
        public static Image? Wifi         => IconHelper.Get("ic_wifi");
        public static Image? Extras       => IconHelper.Get("ic_extras");
        public static Image? Perfiles     => IconHelper.Get("ic_perfiles");
        public static Image? Acerca       => IconHelper.Get("ic_acerca");
        public static Image? Menu         => IconHelper.Get("ic_menu");

        // Controles de scrcpy
        public static Image? Power        => IconHelper.Get("ic_power");
        public static Image? PowerDark    => IconHelper.Get("ic_power_dark");

        // Acciones generales
        public static Image? Refresh      => IconHelper.Get("ic_refresh");
        public static Image? ContentCopy  => IconHelper.Get("ic_content_copy");
        public static Image? Sync         => IconHelper.Get("ic_sync");
        public static Image? Apply        => IconHelper.Get("ic_apply");
        public static Image? Reset        => IconHelper.Get("ic_reset");
        public static Image? Save         => IconHelper.Get("ic_save");
        public static Image? Save2        => IconHelper.Get("ic_save2");
        public static Image? Delete       => IconHelper.Get("ic_delete");
        public static Image? AddProfile   => IconHelper.Get("ic_add_profile");
        public static Image? More         => IconHelper.Get("ic_more");
        public static Image? Expand       => IconHelper.Get("ic_expand");

        // Flechas para el StexNumericUpDown
        public static Image? ArrowBack    => IconHelper.Get("ic_arrow_back");
        public static Image? ArrowNext    => IconHelper.Get("ic_arrow_next");

        // Indicadores de estado
        public static Image? Check        => IconHelper.Get("ic_check");
        public static Image? Verified     => IconHelper.Get("ic_verified");
        public static Image? Clear        => IconHelper.Get("ic_clear");

        // WiFi
        public static Image? Clean        => IconHelper.Get("ic_clean");
        public static Image? WifiAdd      => IconHelper.Get("ic_wifi_add");
        public static Image? WifiConnect  => IconHelper.Get("ic_wifi_connect");
        public static Image? WifiClose    => IconHelper.Get("ic_wifi_close");

        // Archivos y transferencia
        public static Image? Download     => IconHelper.Get("ic_download");
        public static Image? ImportExport => IconHelper.Get("ic_import_export");
        public static Image? Upload       => IconHelper.Get("ic_upload");

        // Diálogos
        public static Image? Done         => IconHelper.Get("ic_done");
        public static Image? Redo         => IconHelper.Get("ic_redo");
        public static Image? Bolt         => IconHelper.Get("ic_bolt");
        public static Image? History      => IconHelper.Get("ic_history");

        // Redes sociales
        public static Image? Discord      => IconHelper.Get("ic_discord");
        public static Image? TikTok       => IconHelper.Get("ic_tiktok");
        public static Image? YouTube      => IconHelper.Get("ic_youtube");
        public static Image? Kofi         => IconHelper.Get("ic_kofi");
    }
}
