using Guna.UI2.WinForms;
using LyXel.Helpers;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace LyXel
{
    public partial class Form1
    {
        // Página Acerca de: encabezado, descripción, descargas y créditos

        private void LoadAcercaPage()
        {
            var purpleLight = AppTheme.AccentLighter;

            // Card 1: encabezado con logo, nombre y botones de redes sociales
            var cardHeader = new Panel()
            {
                Left = S(30),
                Top = S(20),
                Width = contentPanel.Width - S(60),
                Height = S(228),
                BackColor = bgCard,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // PictureBox del logo: 110×110, centrado verticalmente respecto al bloque de texto
            var picBox = new PictureBox()
            {
                Left = cardHeader.Width - S(132),
                Top = S(27),
                Width = S(110),
                Height = S(110),
                BackColor = AppTheme.BtnSecondary,
                SizeMode = PictureBoxSizeMode.Zoom,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png");
            if (File.Exists(logoPath))
                picBox.Image = Image.FromFile(logoPath);
            else
                picBox.Paint += (s, e) => e.Graphics.DrawString("logo.png",
                    new Font("Segoe UI", 7f), new SolidBrush(textSecondary),
                    new RectangleF(0, 0, picBox.Width, picBox.Height),
                    new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });

            // Convierto el ícono oscuro a blanco preservando el canal alfa para los botones de redes
            static Image? TintWhite(Image? source)
            {
                if (source == null) return null;
                var bmp = new Bitmap(source.Width, source.Height);
                using var g = Graphics.FromImage(bmp);
                var cm = new System.Drawing.Imaging.ColorMatrix(new float[][]
                {
                    new float[] { 0, 0, 0, 0, 0 },
                    new float[] { 0, 0, 0, 0, 0 },
                    new float[] { 0, 0, 0, 0, 0 },
                    new float[] { 0, 0, 0, 1, 0 },
                    new float[] { 1, 1, 1, 0, 1 },
                });
                var ia = new System.Drawing.Imaging.ImageAttributes();
                ia.SetColorMatrix(cm);
                g.DrawImage(source, new Rectangle(0, 0, bmp.Width, bmp.Height),
                    0, 0, source.Width, source.Height, GraphicsUnit.Pixel, ia);
                return bmp;
            }

            var btnSocialBase = Color.FromArgb(60, 30, 40);
            Guna2Button BtnOutline(string text, int left, int top, int width, string url, Image? icon)
            {
                var b = new Guna2Button()
                {
                    Text = text, Width = width, Height = S(36), Left = left, Top = top,
                    Font = new Font("Segoe UI", 9f),
                    FillColor = btnSocialBase, ForeColor = purpleLight,
                    BorderColor = purpleLight, BorderThickness = 1, BorderRadius = 6,
                    ImageSize = new Size(S(18), S(18)),
                    ImageAlign = HorizontalAlignment.Left
                };
                b.Image = TintWhite(icon);
                b.MouseEnter += (_, __) => b.FillColor = AppTheme.AccentLight;
                b.MouseLeave += (_, __) => b.FillColor = btnSocialBase;
                b.Click += (_, __) => System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo() { FileName = url, UseShellExecute = true });
                return b;
            }

            cardHeader.Controls.AddRange(new Control[]
            {
                picBox,
                new Label() { Text = "Edición principal", Font = new Font("Segoe UI", 7.5f), ForeColor = textSecondary, Left = S(24), Top = S(14), AutoSize = true },
                new Label() { Text = "LyXel", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = textPrimary, Left = S(24), Top = S(30), AutoSize = true },
                new Label() { Text = $"LyXel Build — {ObtenerVersionApp()}", Font = new Font("Segoe UI", 9.5f), ForeColor = accentColor, Left = S(24), Top = S(60), AutoSize = true },
                new Label() { Text = "Desarrollado por Dario (@nks_array)", Font = new Font("Segoe UI", 9f), ForeColor = textSecondary, Left = S(24), Top = S(84), AutoSize = true },
                new Label() { Text = "\"Eliminando a todos los que supieran la historia, según ellos, mejor para el mundo que no haya memoria.\"", Font = new Font("Segoe UI", 9f, FontStyle.Italic), ForeColor = purpleLight, Left = S(24), Top = S(107), Width = cardHeader.Width - S(158), AutoSize = false, Height = S(20), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right },
                new Label() { Text = "Versión principal del launcher.", Font = new Font("Segoe UI", 8.5f), ForeColor = textSecondary, Left = S(24), Top = S(132), AutoSize = true },
                // Fila de 4 botones sociales, cada uno separado S(12) del anterior
                BtnOutline("  TikTok",   S(24),  S(172), S(118), "https://www.tiktok.com/@nks_array", IconMap.TikTok),
                BtnOutline("  Discord",  S(154), S(172), S(118), "https://discord.gg/CU5quVNyun",     IconMap.Discord),
                BtnOutline("  YouTube",  S(284), S(172), S(118), "https://www.youtube.com/@Nks_v1",   IconMap.YouTube),
                BtnOutline("  Ko-fi",    S(414), S(172), S(118), "https://ko-fi.com/nks_array",       IconMap.Kofi),
            });

            // Card 2: descripción del proyecto con altura dinámica
            // Uso TextRenderer.MeasureText porque lblProyecto sin parent devuelve una sola línea
            int cardW = contentPanel.Width - S(60);
            int lblMaxW = cardW - S(48);
            string proyectoText =
                "LyXel es una herramienta desarrollada para simplificar y mejorar la experiencia de usar scrcpy en PC, permitiendo conectar el teléfono y utilizar periféricos como teclado y mouse de forma más cómoda y controlada.\n" +
                "Está orientada principalmente al ámbito gaming, integrando opciones que facilitan la personalización y adaptación según el dispositivo y las preferencias del usuario.\n" +
                "El proyecto ha sido desarrollado de forma independiente, con enfoque en la funcionalidad, la estabilidad y la mejora continua, apoyándose en el uso real y el feedback de la comunidad.\n" +
                "Actualmente, LyXel establece una identidad propia: una interfaz más limpia, consistente y neutral, dejando atrás elementos genéricos o referencias externas para consolidarse como una herramienta clara, reconocible y bien definida.";
            var proyectoFont = new Font("Segoe UI", 9f);
            int textH = TextRenderer.MeasureText(proyectoText, proyectoFont,
                new Size(lblMaxW, int.MaxValue),
                TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl).Height;

            var cardProyecto = CreateCard("Acerca del Proyecto", S(30), cardHeader.Bottom + S(15),
                S(52) + textH + S(24));
            var lblProyecto = new Label()
            {
                Text = proyectoText,
                Font = proyectoFont,
                ForeColor = textSecondary,
                Left = S(24),
                Top = S(52),
                Width = lblMaxW,
                Height = textH,
                AutoSize = false
            };
            cardProyecto.Controls.Add(lblProyecto);

            // Card 3: descargas oficiales con badge de versión
            var cardDescargas = CreateCard("Descargas oficiales", S(30), cardProyecto.Bottom + S(15), S(170));

            // Badge de versión actual, solo informativo
            var lblBadge = new Label()
            {
                Text = $"Versión actual: {ObtenerVersionApp()}",
                Font = new Font("Segoe UI", 8f),
                ForeColor = purpleLight,
                BackColor = AppTheme.BgTabActive,
                AutoSize = false,
                Width = S(165),
                Height = S(22),
                TextAlign = ContentAlignment.MiddleCenter,
                Top = S(22),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Cursor = Cursors.Default
            };
            lblBadge.Left = cardDescargas.Width - S(24) - S(165);

            var picVerified = new PictureBox()
            {
                Width = S(16),
                Height = S(16),
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = IconMap.Verified,
                BackColor = Color.Transparent,
                Top = S(22) + (S(22) - S(16)) / 2,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            picVerified.Left = cardDescargas.Width - S(24) - S(165) - S(4) - S(16);

            var lblDescDescargas = new Label()
            {
                Text = "Aquí se encuentran todas las versiones oficiales del launcher, incluyendo las más recientes y versiones anteriores.\n" +
                       "Se recomienda utilizar siempre la versión más reciente para evitar errores y contar con las últimas mejoras y funciones.\n" +
                       "Descarga únicamente desde los enlaces oficiales para asegurar que obtienes la versión correcta, sin modificaciones ni intermediarios.",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = textPrimary,
                Left = S(24),
                Top = S(52),
                AutoSize = true,
                MaximumSize = new Size(cardDescargas.Width - S(48), 0)
            };

            var btnDescargas = new Guna2Button()
            {
                Text = "  Ver todas las versiones",
                Width = S(222),
                Height = S(38),
                Left = S(24),
                Top = S(115),
                Font = new Font("Segoe UI", 9.5f),
                FillColor = accentColor,
                ForeColor = Color.White,
                BorderRadius = 6,
                ImageSize = new Size(S(18), S(18)),
                ImageAlign = HorizontalAlignment.Left
            };
            btnDescargas.Image = IconMap.Download;
            btnDescargas.Click += (s, e) => System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo() { FileName = "https://app.mediafire.com/folder/mbedorlh3gugg", UseShellExecute = true });

            cardDescargas.Controls.AddRange(new Control[] { picVerified, lblBadge, lblDescDescargas, btnDescargas });

            // Card 4: créditos de dependencias usadas
            var cardCreditos = CreateCard("Créditos", S(30), cardDescargas.Bottom + S(15), S(168));
            var creditRows = new (string name, string license)[]
            {
                ("scrcpy (Genymobile)",        "Apache License 2.0"),
                ("ADB (Android Debug Bridge)", "Google"),
                ("Guna UI2",                   "Guna Systems"),
                ("ini-parser",                 "MIT License"),
            };
            int rowTop = S(52);
            foreach (var (name, license) in creditRows)
            {
                cardCreditos.Controls.Add(new Label()
                {
                    Text = name,
                    Font = new Font("Segoe UI", 9f),
                    ForeColor = textPrimary,
                    Left = S(24),
                    Top = rowTop,
                    AutoSize = true
                });
                var lblLic = new Label()
                {
                    Text = license,
                    Font = new Font("Segoe UI", 9f),
                    ForeColor = textSecondary,
                    Top = rowTop,
                    Width = S(180),
                    Height = S(22),
                    TextAlign = ContentAlignment.MiddleRight,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                lblLic.Left = cardCreditos.Width - S(24) - S(180);
                cardCreditos.Controls.Add(lblLic);
                rowTop += S(32);
            }

            contentPanel.Controls.AddRange(new Control[] { cardHeader, cardProyecto, cardDescargas, cardCreditos });
        }

    }
}
