using Guna.UI2.WinForms;
using MobiladorStex.Helpers;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Panel = System.Windows.Forms.Panel;

namespace MobiladorStex
{
    public partial class Form1
    {
        private System.Action GetPageLoader(int index) => index switch
        {
            0 => LoadInicioPage,
            1 => LoadVideoPage,
            2 => LoadPantallaPage,
            3 => LoadConexionPage,
            4 => LoadExtrasPage,
            5 => LoadPerfilesPage,
            6 => LoadAcercaPage,
            _ => LoadInicioPage
        };

        // ══════════════════════════════════════════════════════════════
        // BUILD UI
        // ══════════════════════════════════════════════════════════════

        private void BuildUI()
        {
            this.Text = "MobiladorSteX";
            this.Size = new Size(S(1100), S(720));
            this.BackColor = bgPrimary;
            this.ForeColor = textPrimary;
            this.Font = new Font("Segoe UI", 9f);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(S(900), S(600));

            // ── SIDEBAR ──────────────────────────────────────────────
            sidePanel = new Panel()
            {
                Left = 0,
                Top = 0,
                Width = S(SIDEBAR_WIDTH),
                Height = this.ClientSize.Height,
                BackColor = bgSecondary,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
            };

            lblLogo = new Label()
            {
                Text = "MobiladorSteX",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = textPrimary,
                Left = S(16),
                Top = S(20),
                AutoSize = true
            };

            lblVersion = new Label()
            {
                Text = ObtenerVersionApp(),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = AppTheme.AccentLight,
                Left = S(18),
                Top = S(48),
                AutoSize = true
            };


            navButtons = new Guna2Button[7];
            navButtons[0] = CreateNavButton("Inicio",          IconMap.Home,     0);
            navButtons[1] = CreateNavButton("Video y Audio",  IconMap.Video,    1);
            navButtons[2] = CreateNavButton("Pantalla",       IconMap.Screen,   2);
            navButtons[3] = CreateNavButton("Conexión",       IconMap.Wifi,     3);
            navButtons[4] = CreateNavButton("Opciones Extras",IconMap.Extras,   4);
            navButtons[5] = CreateNavButton("Perfiles",       IconMap.Perfiles, 5);
            navButtons[6] = CreateNavButton("Acerca de",      IconMap.Acerca,   6);

            navButtons[paginaActiva].Checked = true;
            navButtons[paginaActiva].FillColor = AppTheme.BtnNavActive;
            navButtons[paginaActiva].ForeColor = Color.White;

            btnToggle = new Guna2Button()
            {
                Text = "",
                Image = IconMap.Menu,
                ImageSize = new Size(S(20), S(20)),
                Width = S(40),
                Height = S(40),
                Left = S(8),
                Top = this.ClientSize.Height - S(170),
                Font = new Font("Segoe UI", 13f),
                FillColor = Color.Transparent,
                ForeColor = AppTheme.TextDimmer,
                BorderColor = Color.Transparent,
                BorderThickness = 0,
                BorderRadius = 6,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnToggle.Click += (s, e) => ToggleSidebar();

            // Botón guardado rápido — solo visible cuando hay cambios sin guardar
            btnGuardadoRapido = new Guna2Button()
            {
                Text = "💾 Guardar",
                Width = S(200),
                Height = S(32),
                Left = S(10),
                Top = this.ClientSize.Height - S(210),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                FillColor = accentColor,
                ForeColor = Color.White,
                BorderRadius = 6,
                Visible = false,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            btnGuardadoRapido.Click += (s, e) => GuardadoRapido();

            sidePanel.Controls.AddRange(new Control[]
            {
                lblLogo, lblVersion,
                navButtons[0], navButtons[1], navButtons[2],
                navButtons[3], navButtons[4], navButtons[5], navButtons[6],
                btnToggle, btnGuardadoRapido
            });

            // ── MAIN PANEL ───────────────────────────────────────────
            mainPanel = new Panel()
            {
                Left = S(SIDEBAR_WIDTH),
                Top = 0,
                Width = this.ClientSize.Width - S(SIDEBAR_WIDTH),
                Height = this.ClientSize.Height,
                BackColor = bgPrimary,
                AutoScroll = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            var headerPanel = new Panel()
            {
                Left = 0,
                Top = 0,
                Width = mainPanel.Width,
                Height = S(70),
                BackColor = bgPrimary,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            lblTituloPagina = new Label()
            {
                Text = tituloPaginaActiva,
                Font = new Font("Segoe UI", 24f, FontStyle.Bold),
                ForeColor = textPrimary,
                Left = S(30),
                Top = S(6),
                AutoSize = true
            };

            var lblAviso = new Label()
            {
                Text = "",
                Name = "lblAvisoHeader",
                Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                ForeColor = AppTheme.Warning,
                Left = S(30),
                Top = S(52),
                AutoSize = true
            };
            headerPanel.Controls.AddRange(new Control[] { lblTituloPagina, lblAviso });

            contentPanel = new Panel()
            {
                Left = 0,
                Top = S(70),
                Width = mainPanel.Width - S(20),
                Height = mainPanel.Height - S(70),
                BackColor = bgPrimary,
                AutoScroll = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            mainPanel.Controls.AddRange(new Control[] { headerPanel, contentPanel });
            this.Controls.AddRange(new Control[] { sidePanel, mainPanel });

            navButtons[0].Click += (s, e) => LoadPage(0, "Inicio", LoadInicioPage);
            navButtons[1].Click += (s, e) => LoadPage(1, "Video y Audio", LoadVideoPage);
            navButtons[2].Click += (s, e) => LoadPage(2, "Pantalla", LoadPantallaPage);
            navButtons[3].Click += (s, e) => LoadPage(3, "Conexión", LoadConexionPage);
            navButtons[4].Click += (s, e) => LoadPage(4, "Opciones Extras", LoadExtrasPage);
            navButtons[5].Click += (s, e) => LoadPage(5, "Perfiles", LoadPerfilesPage);
            navButtons[6].Click += (s, e) => LoadPage(6, "Acerca de", LoadAcercaPage);

            LoadInicioPage();
        }

        private void LoadPage(int index, string title, System.Action loadContent)
        {
            // Guardar scroll solo si es la misma página
            int scrollY = (index == paginaActiva && contentPanel.AutoScrollPosition.Y != 0)
                ? -contentPanel.AutoScrollPosition.Y : 0;

            paginaActiva = index;
            tituloPaginaActiva = title;

            foreach (var btn in navButtons)
            {
                btn.Checked = false;
                btn.FillColor = Color.Transparent;
                btn.ForeColor = textSecondary;
            }
            navButtons[index].Checked = true;
            navButtons[index].FillColor = AppTheme.BtnNavActive;
            navButtons[index].ForeColor = Color.White;

            lblTituloPagina.Text = title;
            contentPanel.Controls.Clear();
            loadContent();

            // Restaurar scroll si era la misma página
            if (scrollY > 0)
                contentPanel.AutoScrollPosition = new System.Drawing.Point(0, scrollY);
        }

        private async void ToggleSidebar()
        {
            try
            {
                if (_sidebarAnimating) return;
                _sidebarAnimating = true;
                btnToggle.Enabled = false;

                sidebarExpanded = !sidebarExpanded;
                int targetWidth = sidebarExpanded ? S(SIDEBAR_WIDTH) : S(SIDEBAR_COLLAPSED);

                sidePanel.Width = targetWidth;
                mainPanel.Left = targetWidth;
                mainPanel.Width = this.ClientSize.Width - targetWidth;

                lblLogo.Visible = sidebarExpanded;
                // lblVersion siempre visible pero texto cambia
                if (sidebarExpanded)
                {
                    lblVersion.Text = ObtenerVersionApp();
                    lblVersion.Left = S(18);
                }
                else
                {
                    lblVersion.Text = "v" + (System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.2.0");
                    lblVersion.Left = S(4);
                }

                // Botón guardado rápido: ajustar tamaño igual que navButtons
                if (btnGuardadoRapido != null && btnGuardadoRapido.Visible)
                {
                    btnGuardadoRapido.Width = sidebarExpanded ? S(200) : S(40);
                    btnGuardadoRapido.Left = sidebarExpanded ? S(10) : S(8);
                    btnGuardadoRapido.Text = sidebarExpanded ? "💾 Guardar" : "💾";
                }

                foreach (var btn in navButtons)
                {
                    btn.Text = sidebarExpanded ? (btn.Tag?.ToString() ?? "") : "";
                    btn.Width = sidebarExpanded ? S(200) : S(SIDEBAR_COLLAPSED);
                    btn.Left = sidebarExpanded ? S(10) : S(8);
                }

                await Task.Delay(300);
                btnToggle.Enabled = true;
                _sidebarAnimating = false;
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"ToggleSidebar error: {ex.Message}"); }
        }

        private Label ObtenerLblAvisoHeader()
        {
            return mainPanel?.Controls
                .OfType<Panel>()
                .FirstOrDefault()
                ?.Controls["lblAvisoHeader"] as Label;
        }

        private void MarcarCambiosSinGuardar()
        {
            _haysCambiosSinGuardar = true;
            if (lblUltimoPerfil != null)
                lblUltimoPerfil.Text = "⚠ Cambios sin guardar — ve a Perfiles";
            var lbl = ObtenerLblAvisoHeader();
            if (lbl != null) lbl.Text = "⚠ Cambios sin guardar — ve a Perfiles para guardarlos";
            if (navButtons != null)
            {
                navButtons[5].Text = sidebarExpanded ? "Perfiles ●" : "";
                navButtons[5].Tag = "Perfiles ●";
            }
            // Diferir la visibilidad del botón para no interrumpir renders en curso
            if (btnGuardadoRapido != null && !string.IsNullOrEmpty(_perfilSeleccionado))
            {
                this.BeginInvoke(() =>
                {
                    if (btnGuardadoRapido == null) return;
                    btnGuardadoRapido.Visible = true;
                    btnGuardadoRapido.Width = sidebarExpanded ? S(200) : S(40);
                    btnGuardadoRapido.Left = sidebarExpanded ? S(10) : S(8);
                    btnGuardadoRapido.Text = sidebarExpanded ? "💾 Guardar" : "💾";
                });
            }
        }

        private void LimpiarIndicadorCambios()
        {
            _haysCambiosSinGuardar = false;
            if (lblUltimoPerfil != null)
                lblUltimoPerfil.Text = ObtenerTextoUltimoPerfil();
            var lbl = ObtenerLblAvisoHeader();
            if (lbl != null) lbl.Text = "";
            if (navButtons != null)
            {
                navButtons[5].Text = sidebarExpanded ? "Perfiles" : "";
                navButtons[5].Tag = "Perfiles";
            }
            if (btnGuardadoRapido != null)
                btnGuardadoRapido.Visible = false;
        }

        private Guna2Button CreateNavButton(string text, Image? icon, int index)
        {
            var btn = new Guna2Button()
            {
                Text = text,
                Tag = text,
                Width = S(200),
                Height = S(44),
                Left = S(10),
                Top = S(90) + (index * S(48)),
                Font = new Font("Segoe UI", 9.5f),
                FillColor = Color.Transparent,
                ForeColor = textSecondary,
                BorderColor = Color.Transparent,
                BorderThickness = 0,
                BorderRadius = 6,
                TextAlign = HorizontalAlignment.Left,
                ImageSize = new Size(S(20), S(20)),
                ImageAlign = HorizontalAlignment.Left,
                Padding = new Padding(S(8), 0, 0, 0),
                ButtonMode = Guna.UI2.WinForms.Enums.ButtonMode.RadioButton
            };
            if (icon != null) btn.Image = icon;
            btn.MouseEnter += (s, e) =>
            {
                if (!btn.Checked)
                    btn.FillColor = AppTheme.BtnNavIdle;
            };
            btn.MouseLeave += (s, e) =>
            {
                if (!btn.Checked) btn.FillColor = Color.Transparent;
            };
            return btn;
        }
    }
}
