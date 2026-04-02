using Guna.UI2.WinForms;
using LyXel.Helpers;
using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace LyXel
{
    public partial class Form1
    {
        private void RefrescarListaPerfiles()
        {
            if (_lstPerfiles == null) return;
            _lstPerfiles.Items.Clear();
            foreach (var nombre in perfilManager.ListarPerfiles())
                _lstPerfiles.Items.Add(nombre);
        }

        private string ValidarNombrePerfil(string nombre, string nombreActual)
        {
            if (string.IsNullOrWhiteSpace(nombre)) return "El nombre no puede estar vacío";
            if (nombre.Length > 30) return "Máximo 30 caracteres";
            if (Regex.IsMatch(nombre, @"[\/\\:\*\?""<>\|]"))
                return "Caracteres no permitidos: / \\ : * ? \" < > |";
            if (nombre != nombreActual)
            {
                var existentes = perfilManager.ListarPerfiles();
                if (existentes.Contains(nombre))
                    return $"Ya existe un perfil con el nombre '{nombre}'";
            }
            return "";
        }

        private string FormatearValoresPerfil(ScrcpyConfig cfg)
        {
            if (cfg == null) return "Sin datos";
            return $"Video: {(cfg.Video ? "✓" : "✗")}  |  FPS: {cfg.Fps}  |  Bitrate: {cfg.Bitrate} Mb  |  Codec: {cfg.VideoCodec}\n" +
                   $"Audio: {(cfg.Audio ? "✓" : "✗")}  |  Codec: {cfg.AudioCodec ?? "opus"}  |  Bitrate: {cfg.AudioBitrate} Kbps  |  Doble: {(cfg.AudioDoble ? "✓" : "✗")}\n" +
                   $"Fullscreen: {(cfg.Fullscreen ? "✓" : "✗")}  |  Max Size: {cfg.MaxSize}  |  MOD: {cfg.ShortcutMod}\n" +
                   $"WiFi: {(cfg.UsarWifi ? $"{cfg.WifiIp}:{cfg.WifiPuerto}" : "✗")}  |  OTG: {(cfg.ModoOtg ? "✓" : "✗")}\n" +
                   $"Stay Awake: {(cfg.StayAwake ? "✓" : "✗")}  |  Screen Off: {(cfg.TurnScreenOff ? "✓" : "✗")}\n" +
                   $"Input Mode: {(cfg.InputMode ?? "uhid").ToUpper()}";
        }

        private void CargarPerfilEnApp(ScrcpyConfig cfg)
        {
            if (cfg == null) return;
            _video = cfg.Video;
            _audio = cfg.Audio;
            _audioDoble = cfg.AudioDoble;
            _audioCodec = cfg.AudioCodec ?? "opus";
            _audioBitrate = cfg.AudioBitrate;
            _fps = cfg.Fps;
            _bitrate = cfg.Bitrate;
            _maxSize = cfg.MaxSize;
            _windowWidth = cfg.WindowWidth;
            _windowHeight = cfg.WindowHeight;
            _videoCodec = cfg.VideoCodec;
            _videoBuffer = cfg.VideoBuffer;
            _audioBuffer = cfg.AudioBuffer;
            _disableScreensaver = cfg.DisableScreensaver;
            _stayAwake = cfg.StayAwake;
            _turnScreenOff = cfg.TurnScreenOff;
            _shortcutMod = cfg.ShortcutMod;
            _fullscreen = cfg.Fullscreen;
            // Estos campos son temporales de sesión, no los cargo del perfil
            _modoOtg = false; // OTG siempre arranca apagado, independientemente del perfil
            // _otgSerial viene de config.ini, no del perfil
            // WiFi tampoco se carga del perfil, lo determina _wifiConectado y la IP activa
            _dpi = cfg.Dpi;
            _printFps = cfg.PrintFps;
            _forwardAllClicks = cfg.ForwardAllClicks;
            _mostrarFlotante = cfg.MostrarFlotante;
            _wmSizeActivo = cfg.WmSizeActivo;
            _wmSizeValor = cfg.WmSizeValor ?? "";
            _useAdvancedEncoder = cfg.UseAdvancedEncoder;
            _videoEncoder = cfg.VideoEncoder;
            _inputMode = cfg.InputMode ?? "uhid";
            _pointerSpeed = cfg.PointerSpeed;
            LimpiarIndicadorCambios();
        }

        private void ExportarPerfilSeleccionado(string nombre)
        {
            using var dlg = new SaveFileDialog()
            {
                Title = "Exportar Perfil",
                Filter = "Archivos INI (*.ini)|*.ini",
                FileName = $"perfil_{nombre}.ini",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            };
            if (dlg.ShowDialog() != DialogResult.OK) return;
            var (exito, error) = perfilManager.ExportarPerfil(nombre, dlg.FileName);
            MessageBox.Show(
                exito ? $"Perfil '{nombre}' exportado correctamente." : $"Error al exportar:\n{error}",
                exito ? "✓ Exportado" : "Error", MessageBoxButtons.OK,
                exito ? MessageBoxIcon.Information : MessageBoxIcon.Error);
        }

        // Página de perfiles: lista izquierda + panel de detalle derecho

        private void LoadPerfilesPage()
        {
            var panelIzq = new Panel()
            {
                Left = S(30),
                Top = S(20),
                Width = S(240),
                Height = contentPanel.Height - 40,
                BackColor = bgCard,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
            };

            var panelDer = new Panel()
            {
                Left = S(286),
                Top = S(20),
                Width = contentPanel.Width - S(316),
                Height = contentPanel.Height - 40,
                BackColor = bgCard,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            _panelDetalle = panelDer;

            panelIzq.Controls.Add(new Label()
            {
                Text = "Perfiles Guardados",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = textPrimary,
                Left = S(16),
                Top = S(16),
                AutoSize = true
            });

            _lstPerfiles = new ListBox()
            {
                Left = S(12),
                Top = S(48),
                Width = panelIzq.Width - S(24),
                Height = panelIzq.Height - S(140),
                BackColor = AppTheme.BgPrimary,
                ForeColor = textPrimary,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9.5f),
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = S(28),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            _lstPerfiles.DrawItem += (s, e) =>
            {
                if (e.Index < 0) return;
                bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                e.Graphics.FillRectangle(
                    new System.Drawing.SolidBrush(selected
                        ? AppTheme.AccentDark
                        : AppTheme.BgPrimary),
                    e.Bounds);
                e.Graphics.DrawString(
                    _lstPerfiles.Items[e.Index].ToString(),
                    e.Font ?? new Font("Segoe UI", 9.5f),
                    new System.Drawing.SolidBrush(AppTheme.TextPrimary),
                    e.Bounds.X + 8, e.Bounds.Y + 4);
            };

            var btnNuevoPerfil = new Guna2Button()
            {
                Text = "Nuevo Perfil",
                Left = S(12),
                Top = panelIzq.Height - S(84),
                Width = panelIzq.Width - S(24),
                Height = S(36),
                Font = new Font("Segoe UI", 9f),
                FillColor = accentColor,
                ForeColor = Color.White,
                BorderRadius = 4,
                ImageSize = new Size(S(18), S(18)),
                ImageAlign = HorizontalAlignment.Left,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            btnNuevoPerfil.Image = IconMap.AddProfile;

            var btnImportar = new Guna2Button()
            {
                Text = "Importar",
                Left = S(12),
                Top = panelIzq.Height - S(42),
                Width = (panelIzq.Width - S(30)) / 2,
                Height = S(32),
                Font = new Font("Segoe UI", 8.5f),
                FillColor = AppTheme.BtnSecondary,
                ForeColor = textSecondary,
                BorderColor = AppTheme.BorderNeutral,
                BorderThickness = 1,
                BorderRadius = 4,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left,
                ImageSize = new Size(S(18), S(18)),
                ImageAlign = HorizontalAlignment.Left
            };
            btnImportar.Image = IconMap.ImportExport;

            var btnExportarLista = new Guna2Button()
            {
                Text = "Exportar",
                Left = S(18) + (panelIzq.Width - S(30)) / 2,
                Top = panelIzq.Height - S(42),
                Width = (panelIzq.Width - S(30)) / 2,
                Height = S(32),
                Font = new Font("Segoe UI", 8.5f),
                FillColor = AppTheme.BtnSecondary,
                ForeColor = textSecondary,
                BorderColor = AppTheme.BorderNeutral,
                BorderThickness = 1,
                BorderRadius = 4,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                ImageSize = new Size(S(18), S(18)),
                ImageAlign = HorizontalAlignment.Left
            };
            btnExportarLista.Image = IconMap.ImportExport;

            RefrescarListaPerfiles();
            if (!string.IsNullOrEmpty(_perfilSeleccionado) &&
                _lstPerfiles.Items.Contains(_perfilSeleccionado))
                _lstPerfiles.SelectedItem = _perfilSeleccionado;

            _lstPerfiles.SelectedIndexChanged += (s, e) =>
            {
                if (_lstPerfiles.SelectedItem == null) return;
                string nombre = _lstPerfiles.SelectedItem.ToString();
                _perfilSeleccionado = nombre;
                MostrarDetallePerfil(nombre);
            };

            btnNuevoPerfil.Click += (s, e) => CrearNuevoPerfil();

            btnImportar.Click += (s, e) =>
            {
                using var dlg = new OpenFileDialog()
                {
                    Title = "Importar Perfil",
                    Filter = "Archivos INI (*.ini)|*.ini",
                    InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
                };
                if (dlg.ShowDialog() != DialogResult.OK) return;
                var (exito, nombre, error) = perfilManager.ImportarDesdeArchivo(dlg.FileName);
                if (exito)
                {
                    RefrescarListaPerfiles();
                    _lstPerfiles.SelectedItem = nombre;
                    MessageBox.Show($"Perfil '{nombre}' importado.", "✓ Importado",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                    MessageBox.Show($"Error:\n{error}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            btnExportarLista.Click += (s, e) =>
            {
                if (string.IsNullOrEmpty(_perfilSeleccionado))
                {
                    MessageBox.Show("Selecciona un perfil para exportar.", "Sin selección",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                ExportarPerfilSeleccionado(_perfilSeleccionado);
            };

            panelIzq.Controls.AddRange(new Control[] { _lstPerfiles, btnNuevoPerfil, btnImportar, btnExportarLista });
            MostrarPlaceholderDetalle();
            contentPanel.Controls.AddRange(new Control[] { panelIzq, panelDer });
            if (!string.IsNullOrEmpty(_perfilSeleccionado))
                MostrarDetallePerfil(_perfilSeleccionado);
        }

        private void MostrarPlaceholderDetalle()
        {
            if (_panelDetalle == null) return;
            _panelDetalle.Controls.Clear();
            _panelDetalle.Controls.Add(new Label()
            {
                Text = "← Selecciona un perfil\n   o crea uno nuevo",
                Font = new Font("Segoe UI", 13f),
                ForeColor = textSecondary,
                AutoSize = true,
                Left = S(40),
                Top = _panelDetalle.Height / 2 - 30,
                Anchor = AnchorStyles.None
            });
        }

        private void MostrarDetallePerfil(string nombre)
        {
            if (_panelDetalle == null) return;
            _panelDetalle.Controls.Clear();

            var perfil = perfilManager.ObtenerPerfil(nombre);
            if (perfil == null) { MostrarPlaceholderDetalle(); return; }

            _panelDetalle.Controls.Add(new Label()
            {
                Text = "Nombre del Perfil",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = textPrimary,
                Left = S(24),
                Top = S(20),
                AutoSize = true
            });

            var txtNombre = new Guna2TextBox()
            {
                Left = S(24),
                Top = S(48),
                Width = S(300),
                Height = S(36),
                Text = nombre,
                Font = new Font("Segoe UI", 10f),
                FillColor = AppTheme.BgCard,
                ForeColor = textPrimary,
                BorderColor = AppTheme.BorderNeutral,
                BorderRadius = 4
            };

            var lblNombreError = new Label()
            {
                Text = "",
                Font = new Font("Segoe UI", 8f),
                ForeColor = AppTheme.Error,
                Left = S(24),
                Top = S(88),
                AutoSize = true
            };

            _panelDetalle.Controls.Add(new Label()
            {
                Text = "Valores del Perfil",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = textPrimary,
                Left = S(24),
                Top = S(106),
                AutoSize = true
            });

            var panelValores = new Panel()
            {
                Left = S(24),
                Top = S(128),
                Width = _panelDetalle.Width - S(48),
                Height = S(154),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            void FillValoresPanel(Panel panel, ScrcpyConfig cfg)
            {
                panel.Controls.Clear();
                if (cfg == null) return;
                var rows = new (string name, string val, Color valColor, bool showClear)[]
                {
                    ("Video",      cfg.Video ? "Activo" : "Inactivo",                       cfg.Video  ? AppTheme.Success : AppTheme.Error, !cfg.Video),
                    ("Audio",      cfg.Audio ? "Activo" : "Inactivo",                       cfg.Audio  ? AppTheme.Success : AppTheme.Error, !cfg.Audio),
                    ("FPS",        cfg.Fps.ToString(),                                       AppTheme.AccentLight,                  false),
                    ("Bitrate",    $"{cfg.Bitrate} Mb",                                      AppTheme.AccentLight,                  false),
                    ("Codec",      cfg.VideoCodec ?? "h264",                                 AppTheme.AccentLight,                  false),
                    ("Fullscreen", cfg.Fullscreen ? "Sí" : "No",                            cfg.Fullscreen ? AppTheme.Success : AppTheme.Error, !cfg.Fullscreen),
                    ("Max Size",   cfg.MaxSize > 0 ? cfg.MaxSize.ToString() : "Auto",       AppTheme.AccentLight,                  false),
                    ("MOD",        cfg.ShortcutMod ?? "lalt",                                AppTheme.AccentLight,                  false),
                    ("WiFi",       cfg.UsarWifi ? $"{cfg.WifiIp}:{cfg.WifiPuerto}" : "No",  cfg.UsarWifi ? accentColor : AppTheme.Error,    !cfg.UsarWifi),
                    ("OTG",        cfg.ModoOtg ? "Sí" : "No",                               cfg.ModoOtg ? AppTheme.Success : AppTheme.Error, !cfg.ModoOtg),
                    ("Stay Awake", cfg.StayAwake ? "Sí" : "No",                             cfg.StayAwake ? AppTheme.Success : AppTheme.Error, !cfg.StayAwake),
                    ("Screen Off", cfg.TurnScreenOff ? "Sí" : "No",                         cfg.TurnScreenOff ? AppTheme.Success : AppTheme.Error, !cfg.TurnScreenOff),
                    ("Input Mode", (cfg.InputMode ?? "uhid").ToUpper(),                      AppTheme.AccentLight,                  false),
                };
                int leftCount = 7;
                int colW = panel.Width / 2;
                int rowH = S(22);
                for (int i = 0; i < rows.Length; i++)
                {
                    bool isRight = i >= leftCount;
                    int colX = isRight ? colW : 0;
                    int rowY = (isRight ? i - leftCount : i) * rowH;
                    var (name, val, valColor, showClear) = rows[i];
                    Image? ico = showClear ? IconMap.Clear : IconMap.Check;
                    panel.Controls.Add(new PictureBox()
                    {
                        Width = S(12), Height = S(12),
                        Left = colX, Top = rowY + (rowH - S(12)) / 2,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Image = ico,
                        BackColor = Color.Transparent
                    });
                    panel.Controls.Add(new Label()
                    {
                        Text = name + ":",
                        Font = new Font("Segoe UI", 8f),
                        ForeColor = textSecondary,
                        Left = colX + S(16), Top = rowY,
                        Width = S(62), Height = rowH,
                        AutoSize = false,
                        TextAlign = ContentAlignment.MiddleLeft
                    });
                    panel.Controls.Add(new Label()
                    {
                        Text = val,
                        Font = new Font("Segoe UI", 8f),
                        ForeColor = valColor,
                        Left = colX + S(78), Top = rowY,
                        Width = colW - S(82), Height = rowH,
                        AutoSize = false,
                        TextAlign = ContentAlignment.MiddleLeft
                    });
                }
            }

            FillValoresPanel(panelValores, perfil);

            var btnCargar = new Guna2Button()
            {
                Text = "  Cargar en App",
                Left = S(24),
                Top = S(300),
                Width = S(160),
                Height = S(38),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                FillColor = accentColor,
                ForeColor = Color.White,
                BorderRadius = 4,
                ImageSize = new Size(S(18), S(18)),
                ImageAlign = HorizontalAlignment.Left
            };
            btnCargar.Image = IconMap.Upload;

            var btnGuardar = new Guna2Button()
            {
                Text = "  Guardar Cambios",
                Left = S(194),
                Top = S(300),
                Width = S(170),
                Height = S(38),
                Font = new Font("Segoe UI", 9.5f),
                FillColor = AppTheme.BtnSecondary,
                ForeColor = textSecondary,
                BorderColor = AppTheme.BorderNeutral,
                BorderThickness = 1,
                BorderRadius = 4,
                ImageSize = new Size(S(18), S(18)),
                ImageAlign = HorizontalAlignment.Left
            };
            btnGuardar.Image = IconMap.Save2;

            var btnEliminar = new Guna2Button()
            {
                Text = "  Eliminar",
                Left = S(24),
                Top = S(348),
                Width = S(130),
                Height = S(34),
                Font = new Font("Segoe UI", 9f),
                FillColor = AppTheme.BtnDanger,
                ForeColor = Color.White,
                BorderRadius = 4,
                ImageSize = new Size(S(18), S(18)),
                ImageAlign = HorizontalAlignment.Left
            };
            btnEliminar.Image = IconMap.Delete;

            var btnExportar = new Guna2Button()
            {
                Text = "  Exportar este Perfil",
                Left = S(164),
                Top = S(348),
                Width = S(190),
                Height = S(34),
                Font = new Font("Segoe UI", 9f),
                FillColor = AppTheme.BtnSecondary,
                ForeColor = textSecondary,
                BorderColor = AppTheme.BorderNeutral,
                BorderThickness = 1,
                BorderRadius = 4,
                ImageSize = new Size(S(18), S(18)),
                ImageAlign = HorizontalAlignment.Left
            };
            btnExportar.Image = IconMap.ImportExport;

            var lblAccionStatus = new Label()
            {
                Text = "",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = AppTheme.Success,
                Left = S(24),
                Top = S(392),
                AutoSize = true
            };

            btnCargar.Click += (s, e) =>
            {
                var confirm = MessageBox.Show(
                    $"¿Cargar el perfil '{nombre}' en la app?\n\n" +
                    "Esto reemplazará la configuración actual.",
                    "Cargar Perfil", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (confirm != DialogResult.Yes) return;
                CargarPerfilEnApp(perfil);
                _perfilSeleccionado = nombre;
                GuardarConfigTema();
                lblAccionStatus.Text = $"✓ Perfil '{nombre}' cargado y guardado como activo";
                lblAccionStatus.ForeColor = AppTheme.Success;
            };

            btnGuardar.Click += (s, e) =>
            {
                string nuevoNombre = txtNombre.Text.Trim();
                string errorNombre = ValidarNombrePerfil(nuevoNombre, nombre);
                if (!string.IsNullOrEmpty(errorNombre))
                {
                    lblNombreError.Text = errorNombre;
                    txtNombre.BorderColor = AppTheme.Error;
                    return;
                }
                lblNombreError.Text = "";
                txtNombre.BorderColor = AppTheme.BorderNeutral;

                if (nuevoNombre != nombre)
                {
                    var (exitoRename, errorRename) = perfilManager.RenombrarPerfil(nombre, nuevoNombre);
                    if (!exitoRename) { lblNombreError.Text = errorRename; return; }
                    nombre = nuevoNombre;
                    _perfilSeleccionado = nuevoNombre;
                }

                var config = ObtenerConfigActual();
                var (exito, error) = perfilManager.GuardarConfigEnPerfil(nombre, config);
                if (exito)
                {
                    RefrescarListaPerfiles();
                    if (_lstPerfiles != null) _lstPerfiles.SelectedItem = nombre;
                    FillValoresPanel(panelValores, perfilManager.ObtenerPerfil(nombre));
                    lblAccionStatus.Text = $"✓ Perfil '{nombre}' guardado";
                    lblAccionStatus.ForeColor = AppTheme.Success;
                    LimpiarIndicadorCambios();
                }
                else
                {
                    lblAccionStatus.Text = $"✗ Error: {error}";
                    lblAccionStatus.ForeColor = AppTheme.Error;
                }
            };

            btnEliminar.Click += (s, e) =>
            {
                var confirm = MessageBox.Show(
                    $"¿Eliminar el perfil '{nombre}'?\n\nEsta acción no se puede deshacer.",
                    "Eliminar Perfil", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (confirm != DialogResult.Yes) return;
                var (exito, error) = perfilManager.EliminarPerfil(nombre);
                if (exito)
                {
                    _perfilSeleccionado = "";
                    RefrescarListaPerfiles();
                    MostrarPlaceholderDetalle();
                }
                else
                    MessageBox.Show($"No se pudo eliminar:\n{error}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            btnExportar.Click += (s, e) => ExportarPerfilSeleccionado(nombre);

            txtNombre.TextChanged += (s, e) =>
            {
                lblNombreError.Text = "";
                txtNombre.BorderColor = AppTheme.BorderNeutral;
            };

            _panelDetalle.Controls.AddRange(new Control[]
            {
                txtNombre, lblNombreError, panelValores,
                btnCargar, btnGuardar, btnEliminar, btnExportar, lblAccionStatus
            });
        }

        private void CrearNuevoPerfil()
        {
            var dlg = new Form()
            {
                Text = "Nuevo Perfil",
                Size = new Size(380, 180),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = bgCard
            };

            dlg.Controls.Add(new Label()
            {
                Text = "Nombre del nuevo perfil:",
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = textPrimary,
                Left = 20,
                Top = 20,
                AutoSize = true
            });

            var txtNuevo = new Guna2TextBox()
            {
                Left = 20,
                Top = 44,
                Width = 320,
                Height = 34,
                PlaceholderText = "Ej: Perfil Gaming, Alta Calidad...",
                Font = new Font("Segoe UI", 9.5f),
                FillColor = AppTheme.BgCard,
                ForeColor = textPrimary,
                BorderColor = AppTheme.BorderNeutral,
                BorderRadius = 4,
                MaxLength = 30
            };

            var lblError = new Label()
            {
                Text = "",
                Font = new Font("Segoe UI", 8f),
                ForeColor = AppTheme.Error,
                Left = 20,
                Top = 82,
                AutoSize = true
            };

            var btnOk = new Guna2Button()
            {
                Text = "Crear",
                Left = 220,
                Top = 100,
                Width = 120,
                Height = 34,
                Font = new Font("Segoe UI", 9.5f),
                FillColor = accentColor,
                ForeColor = Color.White,
                BorderRadius = 4
            };

            var btnCancelar = new Guna2Button()
            {
                Text = "Cancelar",
                Left = 20,
                Top = 100,
                Width = 100,
                Height = 34,
                Font = new Font("Segoe UI", 9.5f),
                FillColor = AppTheme.BtnSecondary,
                ForeColor = textSecondary,
                BorderColor = AppTheme.BorderNeutral,
                BorderThickness = 1,
                BorderRadius = 4
            };

            btnOk.Click += (s, e) =>
            {
                string nombre = txtNuevo.Text.Trim();
                string errorNombre = ValidarNombrePerfil(nombre, null);
                if (!string.IsNullOrEmpty(errorNombre))
                {
                    lblError.Text = errorNombre;
                    txtNuevo.BorderColor = AppTheme.Error;
                    return;
                }
                var config = ObtenerConfigActual();
                var (exito, error) = perfilManager.AgregarPerfil(nombre, config);
                if (exito) { _perfilSeleccionado = nombre; dlg.DialogResult = DialogResult.OK; dlg.Close(); }
                else lblError.Text = error;
            };

            btnCancelar.Click += (s, e) => { dlg.DialogResult = DialogResult.Cancel; dlg.Close(); };
            txtNuevo.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) btnOk.PerformClick();
                if (e.KeyCode == Keys.Escape) btnCancelar.PerformClick();
            };

            dlg.Controls.AddRange(new Control[] { txtNuevo, lblError, btnOk, btnCancelar });
            dlg.AcceptButton = null;

            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                RefrescarListaPerfiles();
                if (_lstPerfiles != null)
                    _lstPerfiles.SelectedItem = _perfilSeleccionado;
            }
        }
    }
}
