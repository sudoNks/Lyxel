using System.Drawing;

namespace MobiladorStex
{
    /// <summary>
    /// Paleta de colores centralizada de MobiladorSteX.
    /// Todos los Color.FromArgb del proyecto deben referenciar esta clase.
    /// </summary>
    public static class AppTheme
    {
        // ── Fondos ────────────────────────────────────────────────────
        public static Color BgPrimary      => Color.FromArgb(33, 32, 35);    // #212023 fondo general
        public static Color BgSecondary    => Color.FromArgb(26, 26, 28);    // #1a1a1c sidebar
        public static Color BgCard         => Color.FromArgb(42, 42, 45);    // #2a2a2d cards
        public static Color BgDark         => Color.FromArgb(20, 20, 20);    // combo oscuro / diag bg
        public static Color BgDarkMid      => Color.FromArgb(30, 30, 30);    // terminal / diag inner
        public static Color BgDarkMid2     => Color.FromArgb(40, 40, 40);    // trackbar fill
        public static Color BgTabActive    => Color.FromArgb(50, 30, 75);    // tab activo morado

        // ── Acento morado ─────────────────────────────────────────────
        public static Color Accent         => Color.FromArgb(107, 47, 196);  // #6b2fc4 morado brillante
        public static Color AccentDark     => Color.FromArgb(78, 28, 141);   // morado oscuro / borde
        public static Color AccentDeep     => Color.FromArgb(55, 28, 100);   // morado profundo (btn mostrar)
        public static Color AccentLight    => Color.FromArgb(147, 90, 220);  // morado claro (títulos)
        public static Color AccentLighter  => Color.FromArgb(180, 140, 220); // morado muy claro
        public static Color AccentPale     => Color.FromArgb(220, 200, 255); // morado pálido (texto btn)
        public static Color AccentText     => Color.FromArgb(210, 190, 245); // texto sobre fondo acento
        public static Color AccentSubtle   => Color.FromArgb(160, 140, 190); // morado-gris sutil
        public static Color AccentMuted    => Color.FromArgb(160, 150, 180); // morado-gris apagado

        // ── Botones ───────────────────────────────────────────────────
        public static Color BtnSecondary   => Color.FromArgb(55, 40, 75);    // botón secundario
        public static Color BtnInactive    => Color.FromArgb(60, 45, 80);    // botón inactivo / no puede
        public static Color BtnNavActive   => Color.FromArgb(60, 40, 80);    // nav seleccionado
        public static Color BtnNavIdle     => Color.FromArgb(50, 35, 70);    // nav idle/deselected
        public static Color BtnDisabled    => Color.FromArgb(60, 60, 65);    // botón deshabilitado fill
        public static Color BtnDanger      => Color.FromArgb(180, 40, 40);   // botón peligro / eliminar
        public static Color BtnDangerDark  => Color.FromArgb(160, 30, 30);   // detener scrcpy
        public static Color BtnWarning     => Color.FromArgb(180, 80, 0);    // acción advertencia

        // ── Texto ─────────────────────────────────────────────────────
        public static Color TextPrimary    => Color.FromArgb(238, 238, 238); // #eeeeee texto principal
        public static Color TextSecondary  => Color.FromArgb(120, 120, 120); // #787878 texto secundario
        public static Color TextDisabled   => Color.FromArgb(100, 100, 100); // texto deshabilitado
        public static Color TextDimmer     => Color.FromArgb(150, 150, 150); // texto muy tenue
        public static Color TextTertiary   => Color.FromArgb(160, 160, 160); // texto terciario
        public static Color TextModerate   => Color.FromArgb(180, 180, 180); // texto moderado
        public static Color TextLight      => Color.FromArgb(200, 200, 200); // texto claro
        public static Color TextLighter    => Color.FromArgb(210, 210, 210); // texto más claro
        public static Color TextError      => Color.FromArgb(200, 100, 100); // rojo-ish (no activo)
        public static Color TextGreenTerm  => Color.FromArgb(0, 220, 0);     // texto terminal verde

        // ── Bordes ────────────────────────────────────────────────────
        public static Color BorderNeutral  => Color.FromArgb(60, 60, 60);    // borde neutro
        public static Color BorderSecondary => Color.FromArgb(80, 60, 100);  // borde secundario morado
        public static Color BorderSecondary2 => Color.FromArgb(80, 60, 110); // borde secundario v2

        // ── Estado ────────────────────────────────────────────────────
        public static Color Success        => Color.FromArgb(16, 124, 16);   // verde éxito
        public static Color SuccessBright  => Color.FromArgb(0, 200, 0);     // verde brillante WiFi
        public static Color SuccessOtg     => Color.FromArgb(0, 200, 80);    // verde OTG activo
        public static Color Error          => Color.FromArgb(220, 50, 50);   // rojo error
        public static Color Warning        => Color.FromArgb(255, 167, 38);  // naranja advertencia
        public static Color WarningText    => Color.FromArgb(200, 140, 40);  // naranja texto sutil
        public static Color WarningOrange  => Color.FromArgb(255, 140, 0);   // naranja codec label
        public static Color Info           => Color.FromArgb(33, 150, 243);  // azul info WiFi

        // ── Toast ─────────────────────────────────────────────────────
        public static Color ToastBgSuccess => Color.FromArgb(20, 80, 20);
        public static Color ToastBgWarning => Color.FromArgb(90, 60, 10);
        public static Color ToastBgError   => Color.FromArgb(90, 20, 20);
        public static Color ToastBgInfo    => Color.FromArgb(25, 50, 80);
        public static Color ToastBarSuccess => Color.FromArgb(16, 185, 16);
        public static Color ToastBarInfo   => Color.FromArgb(0, 120, 212);

        // ── Semi-transparentes ────────────────────────────────────────
        public static Color AccentHoverBg  => Color.FromArgb(30, 107, 47, 196);  // alpha bg acento
        public static Color AccentBorderPen => Color.FromArgb(150, 107, 47, 196); // alpha borde acento
        public static Color WhiteBorderPen  => Color.FromArgb(60, 255, 255, 255); // borde blanco alfa
    }
}
