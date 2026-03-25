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
                var cardAdb = CreateCard("Estado de Conexión ADB", 30, 20, 140);

                var lblAdbEstado = new Label()
                {
                    Text = "●  Verificando...",
                    Font = new Font("Segoe UI", 10f),
                    ForeColor = textSecondary,
                    Left = 24,
                    Top = 58,
                    AutoSize = true
                };

                var btnReiniciarAdb = new Guna2Button()
                {
                    Text = "🔌 Reiniciar ADB",
                    Width = 160,
                    Height = 36,
                    Left = 24,
                    Top = 92,
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
                    Width = 200,
                    Height = 36,
                    Left = 194,
                    Top = 92,
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
                    await adbManager.ReiniciarServidorAsync();
                    await ActualizarEstadoAdbAsync(lblAdbEstado);
                    btnReiniciarAdb.Text = "🔌 Reiniciar ADB";
                    btnReiniciarAdb.Enabled = true;
                };

                btnLimpiarHuerfanas.Click += async (s, e) =>
                {
                    btnLimpiarHuerfanas.Enabled = false;
                    btnLimpiarHuerfanas.Text = "Limpiando...";
                    var (exito, cantidad, mensaje) = await adbManager.LimpiarConexionesWifiAsync(true);
                    MessageBox.Show(mensaje, exito ? "✓ Limpieza completada" : "Error", MessageBoxButtons.OK, exito ? MessageBoxIcon.Information : MessageBoxIcon.Error);
                    btnLimpiarHuerfanas.Text = "🧹 Limpiar WiFi Huérfanas";
                    btnLimpiarHuerfanas.Enabled = true;
                };

                cardAdb.Controls.AddRange(new Control[] { lblAdbEstado, btnReiniciarAdb, btnLimpiarHuerfanas });

                // ── CARD: Modo OTG ────────────────────────────────────────
                var cardOtg = CreateCard("Modo OTG — Teclado y Mouse", 30, 180, 260);

                var togOtg = new Guna2ToggleSwitch()
                {
                    Left = cardOtg.Width - 70,
                    Top = 48,
                    Checked = _modoOtg,
                    CheckedState = { FillColor = accentColor },
                    UncheckedState = { FillColor = Color.FromArgb(60, 60, 60) },
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };

                var txtOtgSerial = new Guna2TextBox()
                {
                    Left = 160,
                    Top = 112,
                    Width = 260,
                    Height = 34,
                    Text = _otgSerial,
                    PlaceholderText = "Dejar vacío si hay un solo dispositivo",
                    Font = new Font("Segoe UI", 9f),
                    FillColor = Color.FromArgb(42, 42, 45),
                    ForeColor = textPrimary,
                    BorderColor = Color.FromArgb(60, 60, 60),
                    BorderRadius = 4
                };
                txtOtgSerial.TextChanged += (s, e) => { _otgSerial = txtOtgSerial.Text; if (!_cargandoPagina) MarcarCambiosSinGuardar(); };

                var txtOtgConsola = new Label()
                {
                    Left = 24,
                    Top = 158,
                    Width = cardOtg.Width - 48,
                    Height = 50,
                    BackColor = Color.FromArgb(42, 42, 45),
                    ForeColor = Color.FromArgb(0, 200, 0),
                    Font = new Font("Consolas", 8f),
                    Text = "Presiona 'Detectar' para ver dispositivos conectados",
                    AutoSize = false
                };

                var btnDetectarOtg = new Guna2Button()
                {
                    Text = "🔄 Detectar Dispositivos",
                    Width = 200,
                    Height = 36,
                    Left = 24,
                    Top = 216,
                    Font = new Font("Segoe UI", 9f),
                    FillColor = Color.FromArgb(55, 40, 75),
                    ForeColor = textSecondary,
                    BorderColor = Color.FromArgb(60, 60, 60),
                    BorderThickness = 1,
                    BorderRadius = 4
                };

                togOtg.CheckedChanged += (s, e) =>
                {
                    _modoOtg = togOtg.Checked;
                    if (_modoOtg && _usarWifi)
                    {
                        _usarWifi = false;
                        MessageBox.Show("OTG es incompatible con WiFi. WiFi desactivado.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    if (_modoOtg)
                        MessageBox.Show("Modo OTG activado.\n\n• Video y audio se desactivarán automáticamente\n• Solo funciona por USB — no compatible con WiFi\n• No requiere depuración USB habilitada\n\nEl teclado y mouse de tu PC controlarán el dispositivo directamente.", "ℹ Modo OTG", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    if (!_cargandoPagina) MarcarCambiosSinGuardar();
                };

                btnDetectarOtg.Click += async (s, e) =>
                {
                    btnDetectarOtg.Enabled = false;
                    txtOtgConsola.Text = "Detectando...";
                    var (exito, seriales, output) = await Task.Run(() => adbManager.ListarDispositivos());
                    if (exito && seriales.Count > 0)
                    {
                        txtOtgConsola.Text = output;
                        if (seriales.Count == 1) { _otgSerial = seriales[0]; txtOtgSerial.Text = seriales[0]; }
                    }
                    else
                        txtOtgConsola.Text = "❌ No se detectaron dispositivos\n• Verifica USB\n• Depuración USB habilitada";
                    btnDetectarOtg.Enabled = true;
                };

                cardOtg.Controls.AddRange(new Control[]
                {
                new Label() { Text = "Activar Modo OTG", Font = new Font("Segoe UI", 10f), ForeColor = textPrimary, Left = 24, Top = 50, AutoSize = true },
                togOtg,
                new Label() { Text = "Control total del teléfono via teclado/mouse físico.\nNo incluye transmisión de video/audio.", Font = new Font("Segoe UI", 8.5f), ForeColor = textSecondary, Left = 24, Top = 72, AutoSize = true },
                new Label() { Text = "Serial (opcional):", Font = new Font("Segoe UI", 9.5f), ForeColor = textPrimary, Left = 24, Top = 116, AutoSize = true },
                txtOtgSerial, txtOtgConsola, btnDetectarOtg
                });

                // ── CARD: WiFi ────────────────────────────────────────────
                var cardWifi = CreateCard("Conexión WiFi (Avanzada)", 30, 460, 374);

                // El toggle refleja si hay una sesión WiFi activa o en progreso.
                // _usarWifi se persiste en config, pero puede quedar true sin sesión real
                // (ej: app cerrada a mitad del setup). Se recalcula desde el estado real.
                bool wifiActivoAlCargar = _wifiConectado || _puertotcpActivo;
                _usarWifi = wifiActivoAlCargar;

                var togWifi = new Guna2ToggleSwitch()
                {
                    Left = cardWifi.Width - 70,
                    Top = 48,
                    Checked = wifiActivoAlCargar,
                    CheckedState = { FillColor = accentColor },
                    UncheckedState = { FillColor = Color.FromArgb(60, 60, 60) },
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };

                var numPuerto = new Guna2TextBox()
                {
                    Left = 120,
                    Top = 102,
                    Width = 110,
                    Height = 32,
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
                    Left = 180,
                    Top = 146,
                    Width = 160,
                    Height = 34,
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
                    Width = 36,
                    Height = 34,
                    Left = 350,
                    Top = 146,
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
                    Left = 24,
                    Top = 194,
                    AutoSize = true
                };

                var btnHabilitarPuerto = new Guna2Button()
                {
                    Text = _puertotcpActivo ? "✓ Puerto Habilitado" : "③ Habilitar Puerto",
                    Width = 170,
                    Height = 36,
                    Left = 24,
                    Top = 224,
                    Font = new Font("Segoe UI", 9f),
                    FillColor = accentColor,
                    ForeColor = Color.White,
                    BorderRadius = 4,
                    Enabled = wifiActivoAlCargar && !_puertotcpActivo
                };

                var btnConectarWifi = new Guna2Button()
                {
                    Text = _wifiConectado ? "✓ Conectado" : "⑤ Conectar WiFi",
                    Width = 150,
                    Height = 36,
                    Left = 204,
                    Top = 224,
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
                    Width = 160,
                    Height = 36,
                    Left = 24,
                    Top = 270,
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
                        // Toggle activado → habilitar el primer paso si el puerto aún no está activo
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

                cardWifi.Controls.AddRange(new Control[]
                {
                // Paso 1 — Conectar cable (informativo)
                new Label() { Text = "① Conecta el cable USB primero", Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(107, 47, 196), Left = 24, Top = 36, AutoSize = true },
                // Paso 2 — Activar WiFi
                new Label() { Text = "② Activar WiFi", Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = textPrimary, Left = 24, Top = 54, AutoSize = true },
                togWifi,
                new Label() { Text = "⚠ Solo usar en redes privadas — No en redes públicas", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(255, 167, 38), Left = 24, Top = 76, AutoSize = true },
                new Label() { Text = "Puerto:", Font = new Font("Segoe UI", 9.5f), ForeColor = textPrimary, Left = 24, Top = 106, AutoSize = true },
                numPuerto,
                // IP — se rellena automáticamente al habilitar el puerto o se introduce a mano
                new Label() { Text = "IP del dispositivo:", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), ForeColor = Color.FromArgb(107, 47, 196), Left = 24, Top = 150, AutoSize = true },
                txtIp, btnDetectarIp,
                lblWifiStatus, btnHabilitarPuerto, btnConectarWifi, btnCerrarPuerto,
                new Label() {
                    Text = "📋 Pasos: ① Cable USB  →  ② Activar toggle  →  ③ Habilitar Puerto\n" +
                           "  → La IP se detecta sola. Si no, escríbela arriba  →  ④ Conectar WiFi\n" +
                           "  → ¡Listo! Ya puedes quitar el cable  ·  Para volver: Cerrar Puerto",
                    Font = new Font("Segoe UI", 8f),
                    ForeColor = Color.FromArgb(160, 140, 190),
                    Left = 24, Top = 310, Width = cardWifi.Width - 48, Height = 52,
                    AutoSize = false,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                }
                });

                contentPanel.Controls.AddRange(new Control[] { cardAdb, cardOtg, cardWifi });

                _ = ActualizarEstadoAdbAsync(lblAdbEstado);

            }
            finally { _cargandoPagina = false; }
        }
    }
}