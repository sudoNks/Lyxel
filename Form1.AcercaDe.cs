using Guna.UI2.WinForms;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace MobiladorStex
{
    public partial class Form1
    {
        // ══════════════════════════════════════════════════════════════
        // ACERCA DE
        // ══════════════════════════════════════════════════════════════

        private void LoadAcercaPage()
        {
            var purpleLight = AppTheme.AccentLighter;

            // ── Card 1: Encabezado ────────────────────────────────────────────────
            var cardHeader = new Panel()
            {
                Left = S(30),
                Top = S(20),
                Width = contentPanel.Width - S(60),
                Height = S(228),
                BackColor = bgCard,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // Imagen: 110×110, centrada verticalmente respecto al bloque de texto (Top≈14..149 → centro≈82)
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

            // Todos los botones de redes en estilo outline (fondo transparente, borde morado)
            Guna2Button BtnOutline(string text, int left, int top, int width, string url)
            {
                var b = new Guna2Button()
                {
                    Text = text, Width = width, Height = S(36), Left = left, Top = top,
                    Font = new Font("Segoe UI", 9f),
                    FillColor = Color.Transparent, ForeColor = purpleLight,
                    BorderColor = purpleLight, BorderThickness = 1, BorderRadius = 6
                };
                b.Click += (_, __) => System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo() { FileName = url, UseShellExecute = true });
                return b;
            }

            cardHeader.Controls.AddRange(new Control[]
            {
                picBox,
                new Label() { Text = "Edición especial", Font = new Font("Segoe UI", 7.5f), ForeColor = textSecondary, Left = S(24), Top = S(14), AutoSize = true },
                new Label() { Text = "MobiladorSteX × Morrigan", Font = new Font("Segoe UI", 13f, FontStyle.Bold), ForeColor = textPrimary, Left = S(24), Top = S(30), AutoSize = true },
                new Label() { Text = $"Dreadnought Patch — {ObtenerVersionApp()}", Font = new Font("Segoe UI", 9.5f), ForeColor = accentColor, Left = S(24), Top = S(60), AutoSize = true },
                new Label() { Text = "Desarrollado por Dario (@nks_array)", Font = new Font("Segoe UI", 9f), ForeColor = textSecondary, Left = S(24), Top = S(84), AutoSize = true },
                new Label() { Text = "\"No controlas el teléfono. Controlas la distancia.\"", Font = new Font("Segoe UI", 9f, FontStyle.Italic), ForeColor = purpleLight, Left = S(24), Top = S(107), Width = cardHeader.Width - S(158), AutoSize = false, Height = S(20), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right },
                new Label() { Text = "Versión insignia de la comunidad Free Fire PC.", Font = new Font("Segoe UI", 8.5f), ForeColor = textSecondary, Left = S(24), Top = S(132), AutoSize = true },
                // Fila única de 4 botones outline, step=S(130) (S(118) ancho + S(12) gap)
                BtnOutline("TikTok — @nks_array", S(24),  S(172), S(118), "https://www.tiktok.com/@nks_array"),
                BtnOutline("💬  Discord",          S(154), S(172), S(118), "https://discord.gg/CU5quVNyun"),
                BtnOutline("▶  YouTube",           S(284), S(172), S(118), "https://www.youtube.com/@Nks_v1"),
                BtnOutline("☕  Ko-fi",             S(414), S(172), S(118), "https://ko-fi.com/nks_array"),
            });

            // ── Card 2: Acerca del Proyecto — altura dinámica según contenido ────────
            // lblProyecto.Height antes de tener parent devuelve una sola línea, por eso
            // usamos TextRenderer.MeasureText que calcula el alto real con wrapping.
            int cardW = contentPanel.Width - S(60);
            int lblMaxW = cardW - S(48);
            string proyectoText =
                "MobiladorSteX nació de un uso personal — una herramienta para llevar la experiencia " +
                "móvil a PC con la mayor fluidez y calidad posible, enfocada en usuarios que buscan " +
                "control, precisión y estabilidad.\n\n" +
                "Al ser un proyecto personal, puede presentar errores. Cualquier feedback es bienvenido " +
                "y ayuda a mejorar la herramienta para toda la comunidad.\n\n" +
                "Esta edición especial reorganiza la identidad visual del launcher, inspirada en " +
                "Morrigan — una estética más distintiva que se convierte en el emblema de la herramienta.";
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

            // ── Card 3: Descargas oficiales ────────────────────────────────────────
            var cardDescargas = CreateCard("Descargas oficiales", S(30), cardProyecto.Bottom + S(15), S(170));

            // Badge de versión — solo informativo, en la misma fila que el título de card
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

            var lblDescDescargas = new Label()
            {
                Text = "Aquí encontrarás todas las versiones oficiales del launcher — desde las más recientes " +
                       "hasta versiones anteriores. Siempre descarga desde fuentes oficiales para garantizar " +
                       "seguridad y estabilidad.",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = textPrimary,
                Left = S(24),
                Top = S(52),
                AutoSize = true,
                MaximumSize = new Size(cardDescargas.Width - S(48), 0)
            };

            var btnDescargas = new Guna2Button()
            {
                Text = "⬇  Ver todas las versiones",
                Width = S(222),
                Height = S(38),
                Left = S(24),
                Top = S(115),
                Font = new Font("Segoe UI", 9.5f),
                FillColor = accentColor,
                ForeColor = Color.White,
                BorderRadius = 6
            };
            btnDescargas.Click += (s, e) => System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo() { FileName = "https://app.mediafire.com/folder/mbedorlh3gugg", UseShellExecute = true });

            cardDescargas.Controls.AddRange(new Control[] { lblBadge, lblDescDescargas, btnDescargas });

            // ── Card 4: Créditos ──────────────────────────────────────────────────
            var cardCreditos = CreateCard("Créditos", S(30), cardDescargas.Bottom + S(15), S(168));
            var creditRows = new (string name, string license)[]
            {
                ("scrcpy — Genymobile",        "Apache 2.0"),
                ("Guna UI2 — Guna Systems",    "Librería WinForms"),
                ("ini-parser — Ricardo Amores","MIT License"),
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

        private Guna2Button CreateBtnSocial(string text, int left, int top, int width, Color fill, string url)
        {
            var btn = new Guna2Button()
            {
                Text = text,
                Width = width,
                Height = S(38),
                Left = left,
                Top = top,
                Font = new Font("Segoe UI", 9.5f),
                FillColor = fill,
                ForeColor = Color.White,
                BorderRadius = 6
            };
            btn.Click += (s, e) => System.Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo() { FileName = url, UseShellExecute = true });
            return btn;
        }
    }
}
