using System.Drawing;

namespace LyXel
{
    /// <summary>
    /// Paleta de colores centralizada de LyXel.
    /// Todos los Color.FromArgb del proyecto deben referenciar esta clase.
    /// </summary>
    public static class AppTheme
    {
        // Fondos
        public static Color BgPrimary      => Color.FromArgb(13, 13, 13);      // #0d0d0d fondo general
        public static Color BgSecondary    => Color.FromArgb(17, 13, 15);      // #110d0f sidebar
        public static Color BgCard         => Color.FromArgb(26, 16, 20);      // #1a1014 cards
        public static Color BgDark         => Color.FromArgb(8, 6, 7);         // más oscuro que primary
        public static Color BgDarkMid      => Color.FromArgb(18, 11, 14);      // terminal / diag inner
        public static Color BgDarkMid2     => Color.FromArgb(30, 18, 23);      // trackbar fill
        public static Color BgTabActive    => Color.FromArgb(55, 20, 35);      // tab activo

        // Acento Dark Amaranth
        public static Color Accent         => Color.FromArgb(109, 26, 54);     // #6d1a36 Dark Amaranth
        public static Color AccentDark     => Color.FromArgb(80, 15, 35);      // más oscuro
        public static Color AccentDeep     => Color.FromArgb(60, 10, 25);      // profundo (btn mostrar)
        public static Color AccentLight    => Color.FromArgb(200, 80, 110);    // más claro
        public static Color AccentLighter  => Color.FromArgb(190, 70, 110);    // muy claro
        public static Color AccentPale     => Color.FromArgb(240, 160, 190);   // pálido (texto btn)
        public static Color AccentText     => Color.FromArgb(220, 150, 175);   // texto sobre fondo acento
        public static Color AccentSubtle   => Color.FromArgb(160, 90, 115);    // amaranth-gris sutil
        public static Color AccentMuted    => Color.FromArgb(150, 80, 105);    // amaranth-gris apagado

        // Botones
        public static Color BtnSecondary   => Color.FromArgb(50, 22, 33);      // botón secundario
        public static Color BtnInactive    => Color.FromArgb(55, 25, 37);      // botón inactivo
        public static Color BtnNavActive   => Color.FromArgb(65, 22, 40);      // nav seleccionado
        public static Color BtnNavIdle     => Color.FromArgb(42, 16, 26);      // nav idle/deselected
        public static Color BtnDisabled    => Color.FromArgb(55, 50, 52);      // botón deshabilitado
        public static Color BtnDanger      => Color.FromArgb(180, 40, 40);     // botón peligro / eliminar
        public static Color BtnDangerDark  => Color.FromArgb(160, 30, 30);     // detener scrcpy
        public static Color BtnWarning     => Color.FromArgb(180, 80, 0);      // acción advertencia

        // Texto
        public static Color TextPrimary    => Color.FromArgb(238, 238, 238);   // #eeeeee texto principal
        public static Color TextSecondary  => Color.FromArgb(170, 170, 170);   // #aaaaaa texto secundario
        public static Color TextMuted      => Color.FromArgb(102, 102, 102);   // #666666 texto tenue
        public static Color TextDisabled   => Color.FromArgb(80, 80, 80);      // texto deshabilitado
        public static Color TextDimmer     => Color.FromArgb(130, 130, 130);   // texto muy tenue
        public static Color TextTertiary   => Color.FromArgb(140, 140, 140);   // texto terciario
        public static Color TextModerate   => Color.FromArgb(160, 160, 160);   // texto moderado
        public static Color TextLight      => Color.FromArgb(190, 190, 190);   // texto claro
        public static Color TextLighter    => Color.FromArgb(210, 210, 210);   // texto más claro
        public static Color TextError      => Color.FromArgb(200, 80, 100);    // rojo-ish (no activo)
        public static Color TextGreenTerm  => Color.FromArgb(0, 220, 0);       // texto terminal verde

        // Bordes
        public static Color BorderNeutral  => Color.FromArgb(42, 26, 32);      // #2a1a20 borde neutro
        public static Color BorderSecondary => Color.FromArgb(80, 30, 50);     // borde amaranth
        public static Color BorderSecondary2 => Color.FromArgb(90, 30, 55);    // borde amaranth v2

        // Estado
        public static Color Success        => Color.FromArgb(16, 124, 16);     // verde éxito
        public static Color SuccessBright  => Color.FromArgb(0, 200, 0);       // verde brillante WiFi
        public static Color SuccessOtg     => Color.FromArgb(0, 200, 80);      // verde OTG activo
        public static Color Error          => Color.FromArgb(220, 50, 50);     // rojo error
        public static Color Warning        => Color.FromArgb(255, 167, 38);    // naranja advertencia
        public static Color WarningText    => Color.FromArgb(200, 140, 40);    // naranja texto sutil
        public static Color WarningOrange  => Color.FromArgb(255, 140, 0);     // naranja codec label
        public static Color Info           => Color.FromArgb(33, 150, 243);    // azul info WiFi

        // Toast
        public static Color ToastBgSuccess => Color.FromArgb(15, 70, 15);
        public static Color ToastBgWarning => Color.FromArgb(90, 60, 10);
        public static Color ToastBgError   => Color.FromArgb(90, 20, 30);
        public static Color ToastBgInfo    => Color.FromArgb(20, 45, 75);
        public static Color ToastBarSuccess => Color.FromArgb(16, 185, 16);
        public static Color ToastBarInfo   => Color.FromArgb(0, 120, 212);

        // Semi-transparentes
        public static Color AccentHoverBg  => Color.FromArgb(30, 109, 26, 54);   // alpha bg acento
        public static Color AccentBorderPen => Color.FromArgb(150, 109, 26, 54); // alpha borde acento
        public static Color WhiteBorderPen  => Color.FromArgb(60, 255, 255, 255); // borde blanco alfa
    }
}
