using Guna.UI2.WinForms;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System;

namespace MobiladorStex
{
    public partial class Form1
    {
        private void LoadConexionPage()
        {
            _cargandoPagina = true;
            try
            {

                // ── CARD: Estado ADB ──────────────────────────────────────
                var cardAdb = CreateCard("Estado de Conexión ADB", S(30), S(20), S(140));

                var lblAdbEstado = new Label()
                {
                    Text = "●  Verificando...",
                    Font = new Font("Segoe UI", 10f),
                    ForeColor = textSecondary,
                    Left = S(24),
                    Top = S(58),
                    AutoSize = true
                };

                var btnReiniciarAdb = new Guna2Button()
                {
                    Text = "🔌 Reiniciar ADB",
                    Width = S(160),
                    Height = S(36),
                    Left = S(24),
                    Top = S(92),
                    Font = new Font("Segoe UI", 9f),
                    FillColor = Color.FromArgb(55, 40, 75),
                    ForeColor = textSecondary,
                    BorderColor = Color.FromArgb(60, 60, 60),
                    BorderThickness = 1,
                    BorderRadius = 4
                };

                var btnLimpiarHuerfanas = new Guna2Button()
                {
                    Text = "🧹 Limpiar WiFi Huérfanas",
                    Width = S(200),
                    Height = S(36),
                    Left = S(194),
                    Top = S(92),
                    Font = new Font("Segoe UI", 9f),
                    FillColor = Color.FromArgb(55, 40, 75),
                    ForeColor = textSecondary,
                    BorderColor = Color.FromArgb(60, 60, 60),
                    BorderThickness = 1,
                    BorderRadius = 4
                };

                btnReiniciarAdb.Click += async (s, e) =>
                {
                    btnReiniciarAdb.Enabled = false;
                    btnReiniciarAdb.Text = "Reiniciando...";
                    lblAdbEstado.Text = "●  Reiniciando servidor ADB...";
                    lblAdbEstado.ForeColor = Color.FromArgb(255, 167, 38);
                    try
                    {
                        await adbManager.ReiniciarServidorAsync();
                        await ActualizarEstadoAdbAsync(lblAdbEstado);
                    }
                    finally
                    {
                        if (!IsDisposed)
                        {
                            btnReiniciarAdb.Text = "🔌 Reiniciar ADB";
                            btnReiniciarAdb.Enabled = true;
                        }
                    }
                };

                btnLimpiarHuerfanas.Click += async (s, e) =>
                {
                    btnLimpiarHuerfanas.Enabled = false;
                    btnLimpiarHuerfanas.Text = "Limpiando...";
                    try
                    {
                        var (exito, cantidad, mensaje) = await adbManager.LimpiarConexionesWifiAsync(true);
                        if (!IsDisposed)
                            MessageBox.Show(mensaje, exito ? "✓ Limpieza completada" : "Error", MessageBoxButtons.OK, exito ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                    }
                    finally
                    {
                        if (!IsDisposed)
                        {
                            btnLimpiarHuerfanas.Text = "🧹 Limpiar WiFi Huérfanas";
                            btnLimpiarHuerfanas.Enabled = true;
                        }
                    }
                };

                cardAdb.Controls.AddRange(new Control[] { lblAdbEstado, btnReiniciarAdb, btnLimpiarHuerfanas });

                // ── CARD: Modo OTG ────────────────────────────────────────
                bool mostrarIndicadorOtg = !string.IsNullOrEmpty(_otgSerial);
                int otgTopOffset = mostrarIndicadorOtg ? S(46) : 0;
                var cardOtg = CreateCard("Modo OTG — Teclado y Mouse", S(30), S(180), S(260) + otgTopOffset);

                bool otgBloqueadoPorWifi = _wifiConectado || _usarWifi;

                var togOtg = new Guna2ToggleSwitch()
                {
                    Left = cardOtg.Width - S(70),
                    Top = S(48) + otgTopOffset,
                    Checked = _modoOtg,
                    Enabled = !otgBloqueadoPorWifi,
                    CheckedState = { FillColor = accentColor },
                    UncheckedState = { FillColor = Color.FromArgb(60, 60, 60) },
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };

                var lblOtgActivar = new Label()
                {
                    Text = "Activar Modo OTG",
                    Font = new Font("Segoe UI", 10f),
                    ForeColor = textPrimary,
                    Left = S(24),
                    Top = S(50) + otgTopOffset,
                    AutoSize = true
                };

                if (otgBloqueadoPorWifi)
                {
                    var ttOtg = new ToolTip();
                    string ttTexto = "OTG solo funciona por USB. Desactiva WiFi primero.";
                    ttOtg.SetToolTip(lblOtgActivar, ttTexto);
                    lblOtgActivar.ForeColor = textSecondary;
                }

                const string OTG_OPCION_VACIA = "— Seleccionar dispositivo —";

                var cmbSerial = new Guna2ComboBox()
                {
                    Left = S(24),
                    Top = S(118) + otgTopOffset,
                    Width = cardOtg.Width - S(48),
                    Height = S(34),
                    Font = new Font("Segoe UI", 9f),
                    FillColor = Color.FromArgb(42, 42, 45),
                    ForeColor = textPrimary,
                    BorderColor = Color.FromArgb(60, 60, 60),
                    BorderRadius = 4,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                cmbSerial.Items.Add(OTG_OPCION_VACIA);
                cmbSerial.SelectedIndex = 0;

                cmbSerial.SelectedIndexChanged += (s, e) =>
                {
                    if (_cargandoPagina) return;
                    var item = cmbSerial.SelectedItem?.ToString() ?? "";
                    _otgSerial = (item == OTG_OPCION_VACIA || string.IsNullOrEmpty(item))
                        ? ""
                        : (item.Contains(" — ") ? item.Split(new[] { " — " }, StringSplitOptions.None)[0].Trim() : item.Trim());
                    GuardarConfigTema();
                };

                var txtOtgConsola = new Label()
                {
                    Left = S(24),
                    Top = S(162) + otgTopOffset,
                    Width = cardOtg.Width - S(48),
                    Height = S(42),
                    BackColor = Color.FromArgb(42, 42, 45),
                    ForeColor = Color.FromArgb(0, 200, 0),
                    Font = new Font("Consolas", 8f),
                    Text = "Presiona 'Detectar' para listar dispositivos",
                    AutoSize = false
                };

                var btnDetectarOtg = new Guna2Button()
                {
                    Text = "🔄 Detectar Dispositivos",
                    Width = S(200),
                    Height = S(36),
                    Left = S(24),
                    Top = S(212) + otgTopOffset,
                    Font = new Font("Segoe UI", 9f),
                    FillColor = Color.FromArgb(55, 40, 75),
                    ForeColor = textSecondary,
                    BorderColor = Color.FromArgb(60, 60, 60),
                    BorderThickness = 1,
                    BorderRadius = 4
                };

                // Rellena cmbSerial con los dispositivos ADB actuales.
                // silencioso=true solo rellena sin mostrar estado en consola.
                async Task PopularCmbSerial(bool silencioso = false)
                {
                    btnDetectarOtg.Enabled = false;
                    if (!silencioso) txtOtgConsola.Text = "Detectando...";
                    try
                    {
                        var (_, seriales, _) = await Task.Run(() => adbManager.ListarDispositivos());
                        if (IsDisposed) return;

                        _cargandoPagina = true;
                        cmbSerial.Items.Clear();
                        cmbSerial.Items.Add(OTG_OPCION_VACIA);
                        foreach (var ser in seriales)
                            cmbSerial.Items.Add(ser);

                        // Restaurar serial guardado si sigue en la lista
                        bool seleccionado = false;
                        if (!string.IsNullOrEmpty(_otgSerial))
                        {
                            for (int i = 1; i < cmbSerial.Items.Count; i++)
                            {
                                if ((cmbSerial.Items[i]?.ToString() ?? "").Trim() == _otgSerial)
                                { cmbSerial.SelectedIndex = i; seleccionado = true; break; }
                            }
                        }
                        if (!seleccionado)
                            cmbSerial.SelectedIndex = 0;

                        _cargandoPagina = false;

                        if (!silencioso)
                            txtOtgConsola.Text = seriales.Count > 0
                                ? $"✓ {seriales.Count} dispositivo(s) detectado(s)"
                                : "❌ No se detectaron dispositivos\n• Verifica USB y depuración USB habilitada";
                    }
                    finally
                    {
                        if (!IsDisposed) { _cargandoPagina = false; btnDetectarOtg.Enabled = true; }
                    }
                }

                togOtg.CheckedChanged += async (s, e) =>
                {
                    if (_cargandoPagina) return;

                    if (togOtg.Checked && (_wifiConectado || _usarWifi))
                    {
                        _cargandoPagina = true;
                        togOtg.Checked = false;
                        _cargandoPagina = false;
                        MessageBox.Show(this,
                            "El modo OTG solo funciona con cable USB.\nDesactiva WiFi primero.",
                            "OTG no disponible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    if (togOtg.Checked && !string.IsNullOrEmpty(_otgSerial))
                    {
                        // Sesión anterior detectada — mostrar opciones al usuario
                        int opcionOtg = 0;
                        using (var dlg = new Form()
                        {
                            Text = "",
                            FormBorderStyle = FormBorderStyle.None,
                            StartPosition = FormStartPosition.CenterParent,
                            BackColor = Color.FromArgb(33, 32, 35),
                            Size = new Size(460, 154),
                            ShowInTaskbar = false
                        })
                        {
                            dlg.Controls.Add(new Label()
                            {
                                Text = $"Sesión anterior: OTG (Serial: {_otgSerial}).",
                                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                                ForeColor = Color.FromArgb(238, 238, 238),
                                Left = 20, Top = 18, Width = 420, Height = 20, AutoSize = false
                            });
                            dlg.Controls.Add(new Label()
                            {
                                Text = "¿Qué deseas hacer?",
                                Font = new Font("Segoe UI", 9f),
                                ForeColor = Color.FromArgb(180, 180, 180),
                                Left = 20, Top = 42, Width = 420, Height = 18, AutoSize = false
                            });
                            var btnUsar = new Guna2Button() { Text = "Usar serial guardado", Left = 20, Top = 84, Width = 130, Height = 36, Font = new Font("Segoe UI", 9f), FillColor = Color.FromArgb(107, 47, 196), ForeColor = Color.White, BorderRadius = 4 };
                            var btnSel = new Guna2Button() { Text = "Seleccionar de la lista", Left = 162, Top = 84, Width = 138, Height = 36, Font = new Font("Segoe UI", 9f), FillColor = Color.FromArgb(55, 40, 75), ForeColor = Color.FromArgb(200, 200, 200), BorderColor = Color.FromArgb(80, 60, 110), BorderThickness = 1, BorderRadius = 4 };
                            var btnCanc = new Guna2Button() { Text = "Cancelar", Left = 312, Top = 84, Width = 130, Height = 36, Font = new Font("Segoe UI", 9f), FillColor = Color.FromArgb(42, 42, 45), ForeColor = Color.FromArgb(160, 160, 160), BorderColor = Color.FromArgb(60, 60, 60), BorderThickness = 1, BorderRadius = 4 };
                            btnUsar.Click += (bs, be) => { opcionOtg = 1; dlg.Close(); };
                            btnSel.Click += (bs, be) => { opcionOtg = 2; dlg.Close(); };
                            btnCanc.Click += (bs, be) => { dlg.Close(); };
                            dlg.Controls.AddRange(new Control[] { btnUsar, btnSel, btnCanc });
                            dlg.ShowDialog(this);
                        }
                        if (opcionOtg == 0)
                        {
                            _cargandoPagina = true;
                            togOtg.Checked = false;
                            _cargandoPagina = false;
                            return;
                        }
                        if (opcionOtg == 2)
                        {
                            _otgSerial = "";
                            GuardarConfigTema();
                            await PopularCmbSerial(silencioso: true);
                        }
                        // opcionOtg == 1: usar serial guardado — _otgSerial ya está establecido
                    }

                    _modoOtg = togOtg.Checked;
                    // OTG es temporal — no pertenece al perfil, no marca cambios pendientes
                };

                btnDetectarOtg.Click += async (s, e) => await PopularCmbSerial(silencioso: false);

                // Indicador de sesión anterior OTG
                var controlsOtg = new System.Collections.Generic.List<Control>();
                if (mostrarIndicadorOtg)
                {
                    var panelSesionOtg = new Panel()
                    {
                        Left = 24, Top = 46, Height = 34,
                        Width = cardOtg.Width - 48,
                        BackColor = Color.FromArgb(30, 107, 47, 196),
                        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                    };
                    panelSesionOtg.Paint += (ps, pe) =>
                    {
                        using var pen = new Pen(Color.FromArgb(150, 107, 47, 196), 1);
                        pe.Graphics.DrawRectangle(pen, 0, 0, panelSesionOtg.Width - 1, panelSesionOtg.Height - 1);
                    };
                    panelSesionOtg.Controls.Add(new Label()
                    {
                        Text = $"📡 Sesión anterior: OTG (Serial: {_otgSerial}) — activa el toggle para reconectar.",
                        Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                        ForeColor = Color.FromArgb(210, 190, 245),
                        Left = 8, Top = 7, Width = panelSesionOtg.Width - 16, Height = 18,
                        AutoSize = false,
                        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                    });
                    controlsOtg.Add(panelSesionOtg);
                }
                controlsOtg.AddRange(new Control[]
                {
                lblOtgActivar,
                togOtg,
                new Label() { Text = "Control total del teléfono via teclado/mouse físico.\nNo incluye transmisión de video/audio.", Font = new Font("Segoe UI", 8.5f), ForeColor = textSecondary, Left = S(24), Top = S(72) + otgTopOffset, AutoSize = true },
                new Label() { Text = "Dispositivo USB:", Font = new Font("Segoe UI", 9.5f), ForeColor = textPrimary, Left = S(24), Top = S(106) + otgTopOffset, AutoSize = true },
                cmbSerial, txtOtgConsola, btnDetectarOtg
                });
                cardOtg.Controls.AddRange(controlsOtg.ToArray());

                // Poblar el ComboBox al abrir la página (silencioso — sin diálogos ni estado en consola)
                _ = PopularCmbSerial(silencioso: true);

                // ── CARD: WiFi ────────────────────────────────────────────
                // El toggle refleja si hay una sesión WiFi activa o en progreso.
                // _usarWifi se persiste en config, pero puede quedar true sin sesión real
                // (ej: app cerrada a mitad del setup). Se recalcula desde el estado real.
                bool wifiActivoAlCargar = _wifiConectado || _puertotcpActivo;
                _usarWifi = wifiActivoAlCargar;
                bool mostrarIndicadorWifi = !string.IsNullOrEmpty(_wifiIp);
                int wifiTopOffset = mostrarIndicadorWifi ? S(46) : 0;
                var cardWifi = CreateCard("Conexión WiFi (Avanzada)", S(30), S(468) + otgTopOffset, S(392) + wifiTopOffset);

                var togWifi = new Guna2ToggleSwitch()
                {
                    Left = cardWifi.Width - S(70),
                    Top = S(70) + wifiTopOffset,
                    Checked = wifiActivoAlCargar,
                    CheckedState = { FillColor = accentColor },
                    UncheckedState = { FillColor = Color.FromArgb(60, 60, 60) },
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };

                var numPuerto = new Guna2TextBox()
                {
                    Left = S(120),
                    Top = S(126) + wifiTopOffset,
                    Width = S(110),
                    Height = S(32),
                    Text = _wifiPuerto.ToString(),
                    Font = new Font("Segoe UI", 9.5f),
                    FillColor = Color.FromArgb(42, 42, 45),
                    ForeColor = Color.FromArgb(238, 238, 238),
                    BorderColor = Color.FromArgb(80, 60, 100),
                    BorderRadius = 4,
                    MaxLength = 5
                };
                numPuerto.TextChanged += (s, e) =>
                {
                    if (int.TryParse(numPuerto.Text, out int p) && p >= 1024 && p <= 65535)
                    {
                        _wifiPuerto = p;
                        if (!_cargandoPagina) MarcarCambiosSinGuardar();
                    }
                };

                var txtIp = new Guna2TextBox()
                {
                    Left = S(180),
                    Top = S(166) + wifiTopOffset,
                    Width = S(160),
                    Height = S(34),
                    Text = _wifiIp,
                    PlaceholderText = "192.168.1.X",
                    Font = new Font("Segoe UI", 9f),
                    FillColor = Color.FromArgb(42, 42, 45),
                    ForeColor = textPrimary,
                    BorderColor = Color.FromArgb(60, 60, 60),
                    BorderRadius = 4
                };
                txtIp.TextChanged += (s, e) => { _wifiIp = txtIp.Text; if (!_cargandoPagina) MarcarCambiosSinGuardar(); };

                var btnDetectarIp = new Guna2Button()
                {
                    Text = "🔄",
                    Width = S(36),
                    Height = S(34),
                    Left = S(350),
                    Top = S(166) + wifiTopOffset,
                    Font = new Font("Segoe UI", 11f),
                    FillColor = accentColor,
                    ForeColor = Color.White,
                    BorderColor = accentColor,
                    BorderThickness = 1,
                    BorderRadius = 4,
                    Enabled = _hayDispositivo
                };

                // Estado inicial del label de status según flags persistidos
                string wifiStatusTextoInicial;
                Color wifiStatusColorInicial;
                if (_wifiConectado)
                {
                    wifiStatusTextoInicial = $"🟢 Conectado a {_wifiIp}:{_wifiPuerto}";
                    wifiStatusColorInicial = Color.FromArgb(16, 124, 16);
                }
                else if (_puertotcpActivo)
                {
                    wifiStatusTextoInicial = $"🔵 Puerto {_wifiPuerto} habilitado — Ingresa la IP y pulsa Conectar WiFi";
                    wifiStatusColorInicial = Color.FromArgb(33, 150, 243);
                }
                else if (!_hayDispositivo)
                {
                    wifiStatusTextoInicial = "⚪ Sin dispositivo — Conecta el cable USB para empezar";
                    wifiStatusColorInicial = Color.FromArgb(255, 167, 38);
                }
                else
                {
                    wifiStatusTextoInicial = "⚪ Listo — Activa el toggle WiFi para comenzar";
                    wifiStatusColorInicial = textSecondary;
                }

                var lblWifiStatus = new Label()
                {
                    Text = wifiStatusTextoInicial,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = wifiStatusColorInicial,
                    Left = S(24),
                    Top = S(210) + wifiTopOffset,
                    AutoSize = true
                };

                var btnHabilitarPuerto = new Guna2Button()
                {
                    Text = _puertotcpActivo ? "✓ Puerto Habilitado" : "③ Habilitar Puerto",
                    Width = S(170),
                    Height = S(36),
                    Left = S(24),
                    Top = S(238) + wifiTopOffset,
                    Font = new Font("Segoe UI", 9f),
                    FillColor = accentColor,
                    ForeColor = Color.White,
                    BorderRadius = 4,
                    Enabled = wifiActivoAlCargar && !_puertotcpActivo
                };

                var btnConectarWifi = new Guna2Button()
                {
                    Text = _wifiConectado ? "✓ Conectado" : "⑤ Conectar WiFi",
                    Width = S(150),
                    Height = S(36),
                    Left = S(204),
                    Top = S(238) + wifiTopOffset,
                    Font = new Font("Segoe UI", 9f),
                    FillColor = Color.FromArgb(55, 40, 75),
                    ForeColor = textSecondary,
                    BorderColor = Color.FromArgb(60, 60, 60),
                    BorderThickness = 1,
                    BorderRadius = 4,
                    Enabled = _puertotcpActivo && !_wifiConectado
                };

                var btnCerrarPuerto = new Guna2Button()
                {
                    Text = "🔒 Cerrar Puerto",
                    Width = S(160),
                    Height = S(36),
                    Left = S(24),
                    Top = S(284) + wifiTopOffset,
                    Font = new Font("Segoe UI", 9f),
                    FillColor = Color.FromArgb(55, 40, 75),
                    ForeColor = textSecondary,
                    BorderColor = Color.FromArgb(60, 60, 60),
                    BorderThickness = 1,
                    BorderRadius = 4,
                    Enabled = _puertotcpActivo
                };

                togWifi.CheckedChanged += async (s, e) =>
                {
                    if (_cargandoPagina) return;

                    // Bloquear activación si no hay dispositivo USB conectado
                    if (togWifi.Checked && !_hayDispositivo)
                    {
                        _cargandoPagina = true;
                        togWifi.Checked = false;
                        _cargandoPagina = false;
                        lblWifiStatus.Text = "⚪ Sin dispositivo — Conecta el cable USB primero";
                        lblWifiStatus.ForeColor = Color.FromArgb(255, 167, 38);
                        MessageBox.Show(this, "Necesitas conectar el teléfono por cable USB antes de activar el modo WiFi.\n\nEl cable es necesario para el primer paso de configuración.", "Cable USB requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    _usarWifi = togWifi.Checked;
                    if (_usarWifi && _modoOtg) { _modoOtg = false; MessageBox.Show(this, "WiFi es incompatible con OTG. OTG desactivado.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information); }
                    if (_usarWifi)
                    {
                        // Si hay datos de sesión anterior, ofrecer usarlos directamente
                        if (!string.IsNullOrEmpty(_wifiIp) && !_puertotcpActivo)
                        {
                            string ipMostrar = _wifiPuerto > 0 ? $"{_wifiIp}:{_wifiPuerto}" : _wifiIp;
                            // 0 = sin decisión/cancelado, 1 = usar guardados, 2 = configurar de nuevo
                            int opcionDlg = 0;
                            using (var dlg = new Form()
                            {
                                Text = "",
                                FormBorderStyle = FormBorderStyle.None,
                                StartPosition = FormStartPosition.CenterParent,
                                BackColor = Color.FromArgb(33, 32, 35),
                                Size = new Size(460, 154),
                                ShowInTaskbar = false
                            })
                            {
                                dlg.Controls.Add(new Label()
                                {
                                    Text = $"Se encontraron datos de sesión anterior (IP: {ipMostrar}).",
                                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                                    ForeColor = Color.FromArgb(238, 238, 238),
                                    Left = 20, Top = 18, Width = 400, Height = 20,
                                    AutoSize = false
                                });
                                dlg.Controls.Add(new Label()
                                {
                                    Text = "¿Qué deseas hacer?",
                                    Font = new Font("Segoe UI", 9f),
                                    ForeColor = Color.FromArgb(180, 180, 180),
                                    Left = 20, Top = 42, Width = 400, Height = 18,
                                    AutoSize = false
                                });
                                var btnUsar = new Guna2Button()
                                {
                                    Text = "Usar datos guardados",
                                    Left = 20, Top = 84, Width = 130, Height = 36,
                                    Font = new Font("Segoe UI", 9f),
                                    FillColor = Color.FromArgb(107, 47, 196),
                                    ForeColor = Color.White,
                                    BorderRadius = 4
                                };
                                var btnNuevo = new Guna2Button()
                                {
                                    Text = "Configurar de nuevo",
                                    Left = 162, Top = 84, Width = 130, Height = 36,
                                    Font = new Font("Segoe UI", 9f),
                                    FillColor = Color.FromArgb(55, 40, 75),
                                    ForeColor = Color.FromArgb(200, 200, 200),
                                    BorderColor = Color.FromArgb(80, 60, 110),
                                    BorderThickness = 1,
                                    BorderRadius = 4
                                };
                                var btnCancelar = new Guna2Button()
                                {
                                    Text = "Cancelar",
                                    Left = 304, Top = 84, Width = 130, Height = 36,
                                    Font = new Font("Segoe UI", 9f),
                                    FillColor = Color.FromArgb(42, 42, 45),
                                    ForeColor = Color.FromArgb(160, 160, 160),
                                    BorderColor = Color.FromArgb(60, 60, 60),
                                    BorderThickness = 1,
                                    BorderRadius = 4
                                };
                                btnUsar.Click += (bs, be) => { opcionDlg = 1; dlg.Close(); };
                                btnNuevo.Click += (bs, be) => { opcionDlg = 2; dlg.Close(); };
                                btnCancelar.Click += (bs, be) => { opcionDlg = 0; dlg.Close(); };
                                dlg.Controls.AddRange(new Control[] { btnUsar, btnNuevo, btnCancelar });
                                dlg.ShowDialog(this);
                            }
                            bool usarGuardados = opcionDlg == 1;
                            if (opcionDlg == 0)
                            {
                                // Cancelado — revertir el toggle sin disparar el handler
                                _cargandoPagina = true;
                                togWifi.Checked = false;
                                _usarWifi = false;
                                _cargandoPagina = false;
                                return;
                            }
                            if (usarGuardados)
                            {
                                txtIp.Text = _wifiIp;
                                _puertotcpActivo = true;
                                btnHabilitarPuerto.Text = "✓ Puerto Habilitado";
                                btnHabilitarPuerto.Enabled = false;
                                btnConectarWifi.Enabled = true;
                                // btnCerrarPuerto solo se habilita al completar la conexión WiFi (_wifiConectado = true)
                                lblWifiStatus.Text = $"🔵 Datos cargados ({ipMostrar}) — pulsa ⑤ Conectar WiFi";
                                lblWifiStatus.ForeColor = Color.FromArgb(33, 150, 243);
                                return;
                            }
                        }

                        // Flujo normal: habilitar el primer paso si el puerto aún no está activo
                        btnHabilitarPuerto.Enabled = !_puertotcpActivo;
                        lblWifiStatus.Text = _puertotcpActivo
                            ? $"🔵 Puerto {_wifiPuerto} habilitado — continúa con Conectar WiFi"
                            : "⚪ Listo — pulsa ③ Habilitar Puerto para comenzar";
                        lblWifiStatus.ForeColor = _puertotcpActivo
                            ? Color.FromArgb(33, 150, 243)
                            : textSecondary;
                    }
                    else
                    {
                        // Pequeño delay para que la animación del toggle termine antes de la operación async
                        await Task.Delay(150);
                        lblWifiStatus.Text = "⏳ Desconectando WiFi...";
                        lblWifiStatus.ForeColor = Color.FromArgb(255, 167, 38);
                        togWifi.Enabled = false;
                        await adbManager.DesconectarTodoAsync();
                        togWifi.Enabled = true;
                        _puertotcpActivo = false; _wifiConectado = false;
                        // Toggle apagado → deshabilitar todos los botones del flujo WiFi
                        btnHabilitarPuerto.Text = "③ Habilitar Puerto"; btnHabilitarPuerto.Enabled = false;
                        btnConectarWifi.Text = "⑤ Conectar WiFi"; btnConectarWifi.Enabled = false;
                        btnCerrarPuerto.Enabled = false;
                        lblWifiStatus.Text = _hayDispositivo
                            ? "⚪ WiFi desactivado — Activa el toggle para volver a conectar"
                            : "⚪ WiFi desactivado — Conecta el cable USB para continuar";
                        lblWifiStatus.ForeColor = textSecondary;
                    }
                    // WiFi es una acción de conexión, no se guarda en perfil como cambio pendiente
                };

                btnDetectarIp.Click += async (s, e) =>
                {
                    btnDetectarIp.Enabled = false;
                    lblWifiStatus.Text = "⏳ Detectando IP del teléfono...";
                    lblWifiStatus.ForeColor = Color.FromArgb(255, 167, 38);
                    var (exito, ip, _) = await adbManager.DetectarIPDispositivoAsync();
                    if (exito) { _wifiIp = ip; txtIp.Text = ip; lblWifiStatus.Text = $"✓ IP detectada: {ip} — ahora pulsa Conectar WiFi"; lblWifiStatus.ForeColor = Color.FromArgb(16, 124, 16); }
                    else { lblWifiStatus.Text = "⚠ No se pudo detectar la IP — escríbela en el campo de arriba"; lblWifiStatus.ForeColor = Color.FromArgb(255, 167, 38); }
                    btnDetectarIp.Enabled = true;
                };

                btnHabilitarPuerto.Click += async (s, e) =>
                {
                    btnHabilitarPuerto.Enabled = false;
                    btnHabilitarPuerto.Text = "Habilitando...";
                    lblWifiStatus.Text = "⏳ Habilitando puerto...";
                    lblWifiStatus.ForeColor = Color.FromArgb(255, 167, 38);

                    // Suprimir actualizaciones visuales de track-devices durante la operación.
                    // adb tcpip reinicia el daemon del dispositivo → desconexión USB transitoria →
                    // track-devices dispara OnDispositivoCambio(false) que sin este guard haría
                    // parpadear el indicador en rojo y mostraría toasts fuera de contexto.
                    _operacionWifiEnCurso = true;
                    try
                    {
                        var (exitoList, seriales, _) = await Task.Run(() => adbManager.ListarDispositivos());
                        if (IsDisposed) return;
                        if (!exitoList || seriales.Count == 0)
                        {
                            lblWifiStatus.Text = "❌ Conecta el USB primero"; lblWifiStatus.ForeColor = Color.FromArgb(220, 50, 50);
                            btnHabilitarPuerto.Text = "③ Habilitar Puerto"; btnHabilitarPuerto.Enabled = true;
                            MessageBox.Show("Conecta el teléfono por USB antes de habilitar el puerto WiFi.", "USB Requerido", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        var (exito, mensaje, error) = await adbManager.HabilitarTcpipAsync(_wifiPuerto);
                        if (IsDisposed) return;
                        if (exito)
                        {
                            _puertotcpActivo = true;
                            btnHabilitarPuerto.Text = "✓ Puerto Habilitado"; btnHabilitarPuerto.Enabled = false;
                            btnCerrarPuerto.Enabled = true;
                            // Detectar IP antes de habilitar "Conectar WiFi" para que el usuario
                            // no pueda pulsar el botón con la IP vacía. El delay da tiempo al
                            // daemon ADB del dispositivo para reiniciarse en modo TCP.
                            lblWifiStatus.Text = "⏳ Detectando IP del dispositivo...";
                            await Task.Delay(1200);
                            if (IsDisposed) return;
                            var (exitoIp, ip, _2) = await adbManager.DetectarIPDispositivoAsync();
                            if (IsDisposed) return;
                            if (exitoIp)
                            {
                                _wifiIp = ip; txtIp.Text = ip;
                                lblWifiStatus.Text = $"🔵 Puerto {_wifiPuerto} habilitado — IP: {ip} — pulsa Conectar WiFi";
                            }
                            else
                            {
                                lblWifiStatus.Text = $"🔵 Puerto {_wifiPuerto} habilitado.\n" +
                                    "La IP se detecta automáticamente. Si no aparece, usa el botón morado (🔄) para detectarla.";
                            }
                            lblWifiStatus.ForeColor = Color.FromArgb(33, 150, 243);
                            // Habilitamos "Conectar WiFi" solo cuando la detección de IP ya terminó
                            btnConectarWifi.Enabled = true;
                        }
                        else
                        {
                            lblWifiStatus.Text = "❌ Error habilitando puerto"; lblWifiStatus.ForeColor = Color.FromArgb(220, 50, 50);
                            btnHabilitarPuerto.Text = "③ Habilitar Puerto"; btnHabilitarPuerto.Enabled = true;
                        }
                    }
                    finally
                    {
                        _operacionWifiEnCurso = false;
                        if (!IsDisposed)
                        {
                            // Sincronizar el indicador de dispositivo con el estado real que
                            // track-devices pudo haber actualizado mientras estaba suprimido.
                            ActualizarIndicadorDispositivo(_hayDispositivo);
                            ActualizarBotonesScrcpy();
                        }
                    }
                };

                btnConectarWifi.Click += async (s, e) =>
                {
                    if (string.IsNullOrWhiteSpace(_wifiIp)) { MessageBox.Show(this, "Ingresa o detecta la IP del dispositivo primero.", "IP Requerida", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                    btnConectarWifi.Enabled = false; btnConectarWifi.Text = "Conectando...";
                    lblWifiStatus.Text = "⏳ Conectando via WiFi..."; lblWifiStatus.ForeColor = Color.FromArgb(255, 167, 38);
                    _operacionWifiEnCurso = true;
                    try
                    {
                        bool alcanzable = await adbManager.PingDispositivoTcpAsync(_wifiIp, _wifiPuerto);
                        if (IsDisposed) return;
                        if (!alcanzable)
                        {
                            lblWifiStatus.Text = "❌ No se pudo alcanzar el dispositivo"; lblWifiStatus.ForeColor = Color.FromArgb(220, 50, 50);
                            btnConectarWifi.Text = "⑤ Conectar WiFi"; btnConectarWifi.Enabled = true;
                            MessageBox.Show(this,
                                $"No se puede alcanzar el dispositivo en {_wifiIp}:{_wifiPuerto}.\n\n" +
                                "• Verifica que el teléfono y el PC estén en la misma red WiFi\n" +
                                "• Confirma que la IP sea correcta (usa 🔄 para detectarla)\n" +
                                "• Asegúrate de haber completado el paso ③ Habilitar Puerto con el cable conectado",
                                "Sin conexión WiFi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        var (exito, mensaje, error) = await adbManager.ConectarWifiAsync(_wifiIp, _wifiPuerto);
                        if (IsDisposed) return;
                        if (exito)
                        {
                            _wifiConectado = true;
                            _ = MonitorearUsbConWifiAsync();
                            lblWifiStatus.Text = $"🟢 Conectado a {_wifiIp}:{_wifiPuerto}"; lblWifiStatus.ForeColor = Color.FromArgb(16, 124, 16);
                            btnConectarWifi.Text = "✓ Conectado"; btnConectarWifi.Enabled = false;
                            btnCerrarPuerto.Enabled = true;
                            // Actualizar el estado ADB inmediatamente para que el label de la card
                            // refleje la nueva conexión sin necesidad de cambiar de pestaña.
                            _ = ActualizarEstadoAdbAsync(lblAdbEstado);
                            MessageBox.Show(this,
                                $"✓ El teléfono está conectado por WiFi ({_wifiIp}:{_wifiPuerto}).\n\n" +
                                "Ya puedes desconectar el cable USB con seguridad.\n" +
                                "La conexión seguirá activa mientras estén en la misma red WiFi.",
                                "✓ WiFi Conectado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            lblWifiStatus.Text = "❌ Error de conexión"; lblWifiStatus.ForeColor = Color.FromArgb(220, 50, 50);
                            btnConectarWifi.Text = "⑤ Conectar WiFi"; btnConectarWifi.Enabled = true;
                        }
                    }
                    finally
                    {
                        _operacionWifiEnCurso = false;
                        if (!IsDisposed)
                        {
                            ActualizarIndicadorDispositivo(_hayDispositivo);
                            ActualizarBotonesScrcpy();
                        }
                    }
                };

                btnCerrarPuerto.Click += async (s, e) =>
                {
                    if (MessageBox.Show(this,
                            "¿Cerrar el puerto WiFi?\n\n" +
                            "Esto desconectará la sesión WiFi actual y el dispositivo volverá a modo USB. " +
                            "Necesitarás conectar el cable USB nuevamente para seguir usando la app.",
                            "Confirmar cierre WiFi", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                    btnCerrarPuerto.Enabled = false; btnCerrarPuerto.Text = "Cerrando...";
                    lblWifiStatus.Text = "⏳ Cerrando puerto, por favor espera..."; lblWifiStatus.ForeColor = Color.FromArgb(255, 167, 38);
                    _operacionWifiEnCurso = true;
                    try
                    {
                        await adbManager.DesconectarTodoAsync();
                        if (IsDisposed) return;
                        var (exito, mensaje, error) = await adbManager.CerrarTcpipAsync();
                        if (IsDisposed) return;
                        _puertotcpActivo = false; _wifiConectado = false;
                        lblWifiStatus.Text = "⚪ Puerto cerrado — reconecta el cable USB para continuar"; lblWifiStatus.ForeColor = textSecondary;
                        btnHabilitarPuerto.Text = "③ Habilitar Puerto"; btnHabilitarPuerto.Enabled = false;
                        btnConectarWifi.Text = "⑤ Conectar WiFi"; btnConectarWifi.Enabled = false;
                        btnCerrarPuerto.Text = "🔒 Cerrar Puerto"; btnCerrarPuerto.Enabled = false;
                        MessageBox.Show(this,
                            exito ? "Puerto cerrado correctamente.\n\nSi quieres volver a conectar por WiFi, pulsa ③ Habilitar Puerto."
                                   : $"No se pudo cerrar automáticamente.\nPuedes reiniciar el teléfono para cerrar el puerto.\n\nError: {error}",
                            exito ? "✓ Puerto Cerrado" : "⚠ Advertencia",
                            MessageBoxButtons.OK,
                            exito ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
                        // Si el toggle sigue activo, restaurar el flujo para que el usuario pueda
                        // volver a habilitar el puerto sin tener que apagar y reactivar el toggle.
                        if (togWifi.Checked && _hayDispositivo)
                        {
                            btnHabilitarPuerto.Enabled = true;
                            lblWifiStatus.Text = "⚪ Puerto cerrado — pulsa ③ Habilitar Puerto para reconectar";
                            lblWifiStatus.ForeColor = textSecondary;
                        }
                    }
                    finally
                    {
                        _operacionWifiEnCurso = false;
                        if (!IsDisposed)
                        {
                            ActualizarIndicadorDispositivo(_hayDispositivo);
                            ActualizarBotonesScrcpy();
                        }
                    }
                };

                // Indicador de sesión anterior — se construye antes del AddRange para incluirlo
                Panel panelSesionWifi = null;
                if (mostrarIndicadorWifi)
                {
                    panelSesionWifi = new Panel()
                    {
                        Left = 24, Top = 46, Height = 34,
                        Width = cardWifi.Width - 48,
                        BackColor = Color.FromArgb(30, 107, 47, 196),
                        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                    };
                    panelSesionWifi.Paint += (ps, pe) =>
                    {
                        using var pen = new Pen(Color.FromArgb(150, 107, 47, 196), 1);
                        pe.Graphics.DrawRectangle(pen, 0, 0, panelSesionWifi.Width - 1, panelSesionWifi.Height - 1);
                    };
                    string ipGuardada = !string.IsNullOrEmpty(_wifiIp) ? $" · {_wifiIp}" : "";
                    panelSesionWifi.Controls.Add(new Label()
                    {
                        Text = $"📡 Sesión anterior guardada{ipGuardada} — activa el toggle para reconectar rápido.",
                        Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                        ForeColor = Color.FromArgb(210, 190, 245),
                        Left = 8, Top = 7,
                        Width = panelSesionWifi.Width - 16, Height = 18,
                        AutoSize = false,
                        Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                    });
                }

                var controlsWifi = new System.Collections.Generic.List<Control>();
                if (panelSesionWifi != null) controlsWifi.Add(panelSesionWifi);
                controlsWifi.AddRange(new Control[]
                {
                // Paso 1 — Conectar cable (informativo)
                new Label() { Text = "① Conecta el cable USB primero", Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(107, 47, 196), Left = S(24), Top = S(50) + wifiTopOffset, AutoSize = true },
                // Paso 2 — Activar WiFi
                new Label() { Text = "② Activar WiFi", Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = textPrimary, Left = S(24), Top = S(74) + wifiTopOffset, AutoSize = true },
                togWifi,
                new Label() { Text = "⚠ Solo usar en redes privadas — No en redes públicas", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(255, 167, 38), Left = S(24), Top = S(102) + wifiTopOffset, AutoSize = true },
                new Label() { Text = "Puerto:", Font = new Font("Segoe UI", 9.5f), ForeColor = textPrimary, Left = S(24), Top = S(130) + wifiTopOffset, AutoSize = true },
                numPuerto,
                // IP — se rellena automáticamente al habilitar el puerto o se introduce a mano
                new Label() { Text = "IP del dispositivo:", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(107, 47, 196), Left = S(24), Top = S(170) + wifiTopOffset, AutoSize = true },
                txtIp, btnDetectarIp,
                lblWifiStatus, btnHabilitarPuerto, btnConectarWifi, btnCerrarPuerto,
                new Label() {
                    Text = "📋 Pasos: ① Cable USB  →  ② Activar toggle  →  ③ Habilitar Puerto\n" +
                           "  → La IP se detecta sola. Si no, escríbela arriba  →  ④ Conectar WiFi\n" +
                           "  → ¡Listo! Ya puedes quitar el cable  ·  Para volver: Cerrar Puerto",
                    Font = new Font("Segoe UI", 8f),
                    ForeColor = Color.FromArgb(160, 140, 190),
                    Left = S(24), Top = S(330) + wifiTopOffset, Width = cardWifi.Width - S(48), Height = S(52),
                    AutoSize = false,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                }
                });
                cardWifi.Controls.AddRange(controlsWifi.ToArray());

                contentPanel.Controls.AddRange(new Control[] { cardAdb, cardOtg, cardWifi });

                _ = ActualizarEstadoAdbAsync(lblAdbEstado);

            }
            finally { _cargandoPagina = false; }
        }
    }
}