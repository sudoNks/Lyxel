using Guna.UI2.WinForms;
using LyXel.Helpers;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Panel = System.Windows.Forms.Panel;

namespace LyXel
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
            5 => LoadOptimizacionPage,
            6 => LoadPerfilesPage,
            7 => LoadAcercaPage,
            _ => LoadInicioPage
        };

        // Construyo toda la UI: sidebar, main panel, header y cargo la página inicial

        private void BuildUI()
        {
            this.Text = "LyXel";
            this.Size = new Size(S(1100), S(720));
            this.BackColor = bgPrimary;
            this.ForeColor = textPrimary;
            this.Font = new Font("Segoe UI", 9f);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(S(900), S(600));

            // Panel del sidebar izquierdo
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
                Text = "LyXel",
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


            navButtons = new Guna2Button[8];
            navButtons[0] = CreateNavButton("Inicio",          IconMap.Home,     0);
            navButtons[1] = CreateNavButton("Video y Audio",  IconMap.Video,    1);
            navButtons[2] = CreateNavButton("Pantalla",       IconMap.Screen,   2);
            navButtons[3] = CreateNavButton("Conexión",       IconMap.Wifi,     3);
            navButtons[4] = CreateNavButton("Opciones Extras",IconMap.Extras,   4);
            navButtons[5] = CreateNavButton("Optimización",   IconMap.Bolt,     5);
            navButtons[6] = CreateNavButton("Perfiles",       IconMap.Perfiles, 6);
            navButtons[7] = CreateNavButton("Acerca de",      IconMap.Acerca,   7);

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

            // Botón de guardado rápido, solo aparece cuando hay cambios pendientes y hay un perfil seleccionado
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
                navButtons[7],
                btnToggle, btnGuardadoRapido
            });

            // Panel principal a la derecha del sidebar
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
            navButtons[5].Click += (s, e) => LoadPage(5, "Optimización", LoadOptimizacionPage);
            navButtons[6].Click += (s, e) => LoadPage(6, "Perfiles", LoadPerfilesPage);
            navButtons[7].Click += (s, e) => LoadPage(7, "Acerca de", LoadAcercaPage);

            LoadInicioPage();
        }

        private void LoadPage(int index, string title, System.Action loadContent)
        {
            // Guardo el scroll solo si estoy volviendo a la misma página para restaurarlo
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

            // Restauro el scroll si era la misma página
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
                // lblVersion siempre visible, pero el texto cambia según el estado del sidebar
                if (sidebarExpanded)
                {
                    lblVersion.Text = ObtenerVersionApp();
                    lblVersion.Left = S(18);
                }
                else
                {
                    lblVersion.Text = "v" + (System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.3.0");
                    lblVersion.Left = S(4);
                }

                // Ajusto el botón de guardado rápido al mismo tamaño que los navButtons
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
            catch { /* animación interrumpida — no requiere feedback al usuario */ }
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
            MostrarAdvertenciaChips();
            var lbl = ObtenerLblAvisoHeader();
            if (lbl != null) lbl.Text = "⚠ Cambios sin guardar — ve a Perfiles para guardarlos";
            if (navButtons != null)
            {
                navButtons[6].Text = sidebarExpanded ? "Perfiles ●" : "";
                navButtons[6].Tag = "Perfiles ●";
            }
            // Difiero la visibilidad del botón para no interrumpir renders en curso
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
            ActualizarChipsPerfil();
            var lbl = ObtenerLblAvisoHeader();
            if (lbl != null) lbl.Text = "";
            if (navButtons != null)
            {
                navButtons[6].Text = sidebarExpanded ? "Perfiles" : "";
                navButtons[6].Tag = "Perfiles";
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

        private void PropagateWheelToContent(MouseEventArgs e)
        {
            int scrollAmount = -(e.Delta / 120) * SystemInformation.MouseWheelScrollLines * 40;
            int newY = Math.Max(0, -contentPanel.AutoScrollPosition.Y + scrollAmount);
            contentPanel.AutoScrollPosition = new System.Drawing.Point(-contentPanel.AutoScrollPosition.X, newY);
        }
    }
}
