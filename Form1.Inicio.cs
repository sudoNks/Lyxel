using Guna.UI2.WinForms;
using LyXel.Helpers;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LyXel
{
    public partial class Form1
    {
        // Página de inicio: estado del dispositivo y acceso rápido a scrcpy

        private void CargarUltimoPerfilSiExiste()
        {
            if (string.IsNullOrEmpty(_perfilSeleccionado)) return;
            var cfg = perfilManager.ObtenerPerfil(_perfilSeleccionado);
            if (cfg != null) CargarPerfilEnApp(cfg);

            if (!string.IsNullOrEmpty(PerfilManager.UltimoError))
                ToastNotification.Mostrar(this,
                    "El archivo de perfiles tuvo un problema al cargarse. Se usarán los valores por defecto.",
                    ToastNotification.ToastTipo.Advertencia);
        }

        // Limpio conexiones WiFi residuales antes de la detección inicial para evitar
        // falsos positivos cuando solo hay USB. Pauso el monitor ADB durante la limpieza
        // y lo reactivo cuando el estado ya es estable.
        private async Task IniciarDeteccionDispositivoAsync()
        {
            adbManager.DetenerTrackDevices(); // silencio eventos mientras limpio

            await adbManager.DesconectarTodoAsync();
            await Task.Delay(1500);

            // Verifico que no queden WiFi residuales tras el disconnect.
            // Si hay seriales con ':' (formato ip:puerto) reintento una vez más.
            var (_, seriales, _) = await Task.Run(() => adbManager.ListarDispositivos());
            if (seriales.Any(s => s.Contains(':')))
            {
                System.Diagnostics.Debug.WriteLine("[Init] WiFi residual detectado tras disconnect — reintentando");
                await adbManager.DesconectarTodoAsync();
                await Task.Delay(500);
            }

            // Esta llamada pone _inicializacionCompleta = true al terminar
            await ActualizarEstadoDispositivoAsync(mostrarToast: true);

            if (!ArquitecturaHelper.ScrcpyDisponible())
                InvokeSeguro(() => ToastNotification.Mostrar(this,
                    $"No se encontró scrcpy ({(ArquitecturaHelper.ModoCompatibilidad ? "32 bits" : "64 bits")}). Reinstala la aplicación.",
                    ToastNotification.ToastTipo.Error));

            // Reactivo el monitor solo cuando el estado ya es definitivo
            if (!IsDisposed) adbManager.IniciarTrackDevices();
        }

        private void LoadInicioPage()
        {
            var cardEstado = CreateCard("Estado del Dispositivo", S(30), S(56), S(160));

            lblEstadoIndicador = new Label()
            {
                Text = "●",
                Font = new Font("Segoe UI", 12f),
                ForeColor = AppTheme.TextSecondary, // gris mientras verifica
                Left = S(24),
                Top = S(62),
                AutoSize = true
            };

            lblEstadoTexto = new Label()
            {
                Text = "Verificando...",
                Font = new Font("Segoe UI", 10f),
                ForeColor = textSecondary,
                Left = S(44),
                Top = S(64),
                AutoSize = true
            };

            var btnReconectar = new Guna2Button()
            {
                Text = "RECONECTAR ADB",
                Width = S(180),
                Height = S(36),
                Left = S(24),
                Top = S(100),
                Font = new Font("Segoe UI", 9f),
                FillColor = AppTheme.BtnSecondary,
                ForeColor = textSecondary,
                BorderColor = AppTheme.BorderSecondary,
                BorderThickness = 1,
                BorderRadius = 4,
                ImageSize = new Size(S(18), S(18)),
                ImageAlign = HorizontalAlignment.Left
            };
            btnReconectar.Image = IconMap.Refresh;
            btnReconectar.Click += async (s, e) =>
            {
                btnReconectar.Text = "Reconectando...";
                btnReconectar.Enabled = false;
                await adbManager.ReiniciarServidorAsync();
                await ActualizarEstadoDispositivoAsync();
                btnReconectar.Text = "RECONECTAR ADB";
                btnReconectar.Enabled = true;
            };

            cardEstado.Controls.AddRange(new Control[] { lblEstadoIndicador, lblEstadoTexto, btnReconectar });

            // Toggle Modo Compatibilidad — lógica sin cambios
            var togCompatInicio = new Guna2ToggleSwitch()
            {
                Checked = ArquitecturaHelper.ModoCompatibilidad,
                CheckedState = { FillColor = accentColor },
                UncheckedState = { FillColor = AppTheme.BorderNeutral },
                Anchor = AnchorStyles.None,
                Margin = new Padding(0)
            };
            var ttCompatInicio = new ToolTip();
            ttCompatInicio.SetToolTip(togCompatInicio, "Usa scrcpy de 32 bits. Activa esto si tienes problemas de compatibilidad.");
            togCompatInicio.CheckedChanged += (s, e) =>
            {
                ArquitecturaHelper.ModoCompatibilidad = togCompatInicio.Checked;
                GuardarConfigTema();
                ActualizarPreviewComando();
                string msg = togCompatInicio.Checked
                    ? "Modo Compatibilidad activado. Aplica al próximo inicio de scrcpy."
                    : "Modo Compatibilidad desactivado. Aplica al próximo inicio de scrcpy.";
                ToastNotification.Mostrar(this, msg, ToastNotification.ToastTipo.Info, 3500);
            };

            // TLP interno de cardCompat: [label Fill | toggle AutoSize]
            var tblCompat = new TableLayoutPanel()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Color.Transparent,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            tblCompat.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); // label: Fill
            tblCompat.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));     // toggle: AutoSize
            tblCompat.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            tblCompat.Controls.Add(new Label()
            {
                Text = "Modo Compatibilidad (32 bits)",
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = AppTheme.TextSecondary,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0)
            }, 0, 0);
            tblCompat.Controls.Add(togCompatInicio, 1, 0);

            // Card contenedora: fondo de card, padding interno, alineada a la derecha del header
            var cardCompat = new Panel()
            {
                BackColor = AppTheme.BgCard,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(10, 6, 10, 6),
                Height = S(36),
                AutoSize = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Margin = new Padding(0, 2, 0, 2)
            };
            cardCompat.Controls.Add(tblCompat);

            // TableLayoutPanel de encabezado: [título AutoSize | espacio Fill | cardCompat AutoSize]
            var tblTituloFila = new TableLayoutPanel()
            {
                Dock = DockStyle.Top,
                Height = 40,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = Color.Transparent,
                Padding = new Padding(S(30), 0, S(16), 0)
            };
            tblTituloFila.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));      // "Inicio"
            tblTituloFila.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));  // espacio
            tblTituloFila.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));      // cardCompat
            tblTituloFila.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            tblTituloFila.Controls.Add(new Label()
            {
                Text = "Inicio",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Margin = new Padding(0, S(8), 0, 0)
            }, 0, 0);
            tblTituloFila.Controls.Add(cardCompat, 2, 0);

            var cardRapido = CreateCard("Acceso Rápido", S(30), S(236), S(248));

            btnIniciarScrcpy = new Guna2Button()
            {
                Text = "Detectando dispositivo...",
                Width = cardRapido.Width - S(48),
                Height = S(48),
                Left = S(24),
                Top = S(56),
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                FillColor = AppTheme.BtnInactive,
                ForeColor = AppTheme.TextDimmer,
                BorderRadius = 6,
                ImageSize = new Size(S(22), S(22)),
                ImageAlign = HorizontalAlignment.Center,
                Padding = new Padding(0),
                Enabled = false, // deshabilitado hasta confirmar dispositivo
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            btnIniciarScrcpy.Image = IconMap.PowerDark;
            btnIniciarScrcpy.Click += (s, e) => LanzarScrcpy();

            btnDetenerScrcpy = new Guna2Button()
            {
                Text = "DETENER SCRCPY",
                Width = cardRapido.Width - S(48),
                Height = S(36),
                Left = S(24),
                Top = S(114),
                Font = new Font("Segoe UI", 9.5f),
                FillColor = AppTheme.BtnSecondary,
                ForeColor = textSecondary,
                BorderColor = AppTheme.BorderSecondary,
                BorderThickness = 1,
                BorderRadius = 4,
                ImageSize = new Size(S(22), S(22)),
                ImageAlign = HorizontalAlignment.Center,
                Padding = new Padding(0),
                Enabled = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            btnDetenerScrcpy.Image = IconMap.PowerDark;
            btnDetenerScrcpy.Click += (s, e) => DetenerScrcpy();

            lblUltimoPerfil = new FlowLayoutPanel()
            {
                Left = S(24),
                Top = S(161),
                Width = cardRapido.Width - S(48),
                Height = S(26),
                BackColor = bgCard,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            ActualizarChipsPerfil();

            var lblCmd = new Label()
            {
                Text = "CMD:",
                Font = new Font("Segoe UI", 8f),
                ForeColor = AppTheme.TextSecondary,
                AutoSize = true,
                Margin = new Padding(0, 4, 4, 0)
            };

            txtPreviewComando = new TextBox()
            {
                Height = S(24),
                Width = cardRapido.Width - S(24) - S(24) - S(34), // panel - left pad - right pad - label approx
                Multiline = false,
                ReadOnly = true,
                ScrollBars = ScrollBars.None,
                BackColor = AppTheme.BgDarkMid,
                ForeColor = AppTheme.TextSecondary,
                Font = new Font("Consolas", 8f),
                BorderStyle = BorderStyle.None,
                Anchor = AnchorStyles.Left | AnchorStyles.Right,
                TabStop = false,
                Margin = new Padding(0, 4, 0, 0)
            };
            txtPreviewComando.Enter += (s, e) => this.BeginInvoke(() => this.ActiveControl = null);

            var btnCopiarComando = new Button()
            {
                Width = S(24),
                Height = S(24),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand,
                TabStop = false,
                Margin = new Padding(4, 4, 0, 0),
                ImageAlign = ContentAlignment.MiddleCenter,
                Image = IconMap.ContentCopy
            };
            btnCopiarComando.FlatAppearance.BorderSize = 0;
            btnCopiarComando.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 255, 255, 255);
            btnCopiarComando.FlatAppearance.MouseDownBackColor = Color.FromArgb(60, 255, 255, 255);
            var ttCopiar = new ToolTip();
            ttCopiar.SetToolTip(btnCopiarComando, "Copiar comando");
            btnCopiarComando.Click += (s, e) =>
            {
                bool tieneDispositivo = _hayDispositivo || _modoOtg || _wifiConectado;
                string prefijo = ArquitecturaHelper.ModoCompatibilidad ? "scrcpy.exe (x86)" : "scrcpy.exe (x86_64)";
                string comandoCompleto = tieneDispositivo
                    ? prefijo + " " + ScrcpyManager.ConstruirArgumentos(ObtenerConfigActual())
                    : prefijo + " (conecta un dispositivo para ver el comando completo)";
                Clipboard.SetText(comandoCompleto);
                ToastNotification.Mostrar(this, "Comando copiado al portapapeles.", ToastNotification.ToastTipo.Info, 2500);
            };

            var panelCmd = new FlowLayoutPanel()
            {
                Left = S(24),
                Top = S(197),
                Width = cardRapido.Width - S(48),
                Height = S(32),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = AppTheme.BgDarkMid,
                Padding = new Padding(0),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            panelCmd.Click += (s, e) => this.ActiveControl = null;

            // El TextBox ocupa el espacio restante entre label y botón copiar
            panelCmd.Controls.AddRange(new Control[] { lblCmd, txtPreviewComando, btnCopiarComando });
            panelCmd.Layout += (s, e) =>
            {
                int reservado = lblCmd.Width + lblCmd.Margin.Horizontal
                              + S(24) + btnCopiarComando.Margin.Horizontal;
                txtPreviewComando.Width = Math.Max(0,
                    panelCmd.ClientSize.Width - reservado - txtPreviewComando.Margin.Horizontal);
            };

            cardRapido.Controls.AddRange(new Control[] {
                btnIniciarScrcpy, btnDetenerScrcpy, lblUltimoPerfil,
                panelCmd
            });
            contentPanel.Controls.AddRange(new Control[] { tblTituloFila, cardEstado, cardRapido });

            // Solo refresco el estado si la init ya terminó — durante el arranque se encarga IniciarDeteccionDispositivoAsync
            if (_inicializacionCompleta) _ = ActualizarEstadoDispositivoAsync();
            IniciarLoopEstadoScrcpy();
            ActualizarPreviewComando();
        }

        private async void LanzarScrcpy()
        {
            try
            {
                if (!_modoOtg)
                {
                    // Modo normal: necesito ADB para verificar que hay dispositivo
                    var (hayDispositivo, serialesAdb, _) = await Task.Run(() => adbManager.ListarDispositivos());

                    if (!hayDispositivo || serialesAdb.Count == 0)
                    {
                        MessageBox.Show(
                            "No se puede iniciar scrcpy.\n\n" +
                            "Verifica que:\n" +
                            "• El teléfono esté conectado por USB o WiFi\n" +
                            "• La depuración USB esté habilitada\n" +
                            "• ADB reconozca el dispositivo (botón Reconectar)",
                            "Sin dispositivo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                else
                {
                    // Modo OTG: si no hay serial guardado, intento obtenerlo via ADB
                    // para pasar -s [serial] y evitar ambigüedad si hay varios dispositivos
                    if (string.IsNullOrWhiteSpace(_otgSerial))
                    {
                        var (_, serialesAdb, _) = await Task.Run(() => adbManager.ListarDispositivos());
                        if (serialesAdb.Count == 1)
                        {
                            _otgSerial = serialesAdb[0];
                            GuardarConfigTema();
                        }
                        else if (serialesAdb.Count > 1)
                        {
                            MessageBox.Show(
                                "Hay varios dispositivos conectados.\n\n" +
                                "Ve a Conexión → Modo OTG → Detectar Dispositivos\n" +
                                "y selecciona el serial del dispositivo a usar.",
                                "OTG — Selecciona dispositivo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        // Si ADB no detecta nada (sin depuración USB), scrcpy --otg se arregla solo por USB físico
                    }
                }

                var config = ObtenerConfigActual();

                bool exito = scrcpyManager.Lanzar(config);

                if (!exito)
                {
                    MessageBox.Show(
                        "No se pudo lanzar scrcpy.\n\nIntenta reconectar ADB e inténtalo de nuevo.",
                        "Error al iniciar scrcpy", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _scrcpyEstabaActivo = true;
                _ultimaSesionWifi = _wifiConectado;
                _ultimaSesionOtg = _modoOtg;
                GuardarConfigTema();
                ActualizarBotonesScrcpy();
                this.WindowState = FormWindowState.Minimized;

                // Si OTG cerró en menos de 3 segundos, probablemente fue un error de compatibilidad
                if (_modoOtg)
                {
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(3000);
                        if (!scrcpyManager.EstaCorriendo)
                            InvokeSeguro(() =>
                            {
                                this.Show();
                                this.WindowState = FormWindowState.Normal;
                                this.BringToFront();
                                this.Activate();
                                MessageBox.Show(this,
                                    "OTG no pudo iniciarse. Verifica que tu cable soporte modo OTG y que el dispositivo sea compatible con esta función.\n\n• Si usas cable normal con depuración USB habilitada, intenta desactivar y reactivar la depuración USB en tu teléfono antes de lanzar.",
                                    "OTG — Error de inicio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            });
                    });
                }

                string modo = _modoOtg ? "OTG" : _usarWifi ? "WiFi" : (!_video && !_audio) ? "Control Only" : "USB";
                string info = $"{_perfilSeleccionado}  |  {_fps} FPS  |  {_bitrate} Mb  |  {modo}";

                if (_mostrarFlotante)
                {
                    Action onDetenerFlotante = () =>
                    {
                        scrcpyManager.Detener();
                        _scrcpyEstabaActivo = false;
                        if (adbManager.HayDispositivoConectado())
                        {
                            if (_wmSizeActivo) adbManager.ResetearResolucion();
                            if (_resAdbActiva) adbManager.ResetearResolucion();
                        }
                        InvokeSeguro(() =>
                        {
                            _flotante = null;
                            this.WindowState = FormWindowState.Normal;
                            this.BringToFront();
                            ActualizarBotonesScrcpy();
                        });
                    };
                    Action onMostrarFlotante = () =>
                    {
                        InvokeSeguro(() =>
                        {
                            this.WindowState = FormWindowState.Normal;
                            this.BringToFront();
                        });
                    };

                    if (_modoOtg)
                    {
                        _flotante = new FloatingWindowOtg(
                            scrcpyManager,
                            serial: _otgSerial,
                            shortcutMod: _shortcutMod,
                            onDetener: onDetenerFlotante,
                            onMostrarApp: onMostrarFlotante);
                    }
                    else
                    {
                        _flotante = new FloatingWindow(
                            scrcpyManager, info,
                            onDetener: onDetenerFlotante,
                            onMostrarApp: onMostrarFlotante,
                            printFps: _printFps);
                    }

                    this.Resize += MostrarFlotanteAlMinimizar;

                    _flotante.FormClosed += (s, e) =>
                    {
                        _flotante = null;
                        this.Resize -= MostrarFlotanteAlMinimizar;
                    };

                    _flotante.Show();
                }
                else
                {
                    Task.Run(async () =>
                    {
                        while (scrcpyManager.EstaCorriendo)
                            await Task.Delay(500);

                        _scrcpyEstabaActivo = false;
                        InvokeSeguro(() =>
                        {
                            this.WindowState = FormWindowState.Normal;
                            this.BringToFront();
                            ActualizarBotonesScrcpy();
                        });
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado al iniciar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ActualizarBotonesScrcpy();
            }
        }

        private void MostrarFlotanteAlMinimizar(object? sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized
                && _flotante != null && !_flotante.IsDisposed
                && scrcpyManager.EstaCorriendo)
            {
                _flotante.Show();
            }
        }

        private void DetenerScrcpy()
        {
            scrcpyManager.Detener();
            _scrcpyEstabaActivo = false;
            // Si hay dispositivo conectado revierto wm size y resolución ADB
            if (adbManager.HayDispositivoConectado())
            {
                if (_wmSizeActivo) _ = adbManager.ResetearResolucionAsync();
                if (_resAdbActiva) _ = adbManager.ResetearResolucionAsync();
            }
            ActualizarBotonesScrcpy();
        }

        private void ActualizarBotonesScrcpy()
        {
            if (btnIniciarScrcpy == null || btnDetenerScrcpy == null) return;
            bool corriendo = scrcpyManager.EstaCorriendo;
            bool hayDispositivo = _hayDispositivo || _modoOtg;

            bool puedeIniciar = _inicializacionCompleta && !corriendo && (hayDispositivo || _usarWifi);
            btnIniciarScrcpy.Enabled = puedeIniciar;
            btnIniciarScrcpy.FillColor = puedeIniciar ? accentColor : AppTheme.BtnInactive;
            btnIniciarScrcpy.ForeColor = puedeIniciar ? Color.White : AppTheme.TextDimmer;
            if (!_inicializacionCompleta)
                btnIniciarScrcpy.Text = "Detectando dispositivo...";
            else if (corriendo)
                btnIniciarScrcpy.Text = "INICIAR SCRCPY";
            else
                btnIniciarScrcpy.Text = puedeIniciar ? "INICIAR SCRCPY" : "Sin dispositivo";
            btnDetenerScrcpy.Enabled = corriendo;

            // Ajustar intervalo del timer: más frecuente mientras corre, más lento en espera
            if (_timerScrcpy != null)
            {
                if (corriendo  && _timerScrcpy.Interval != 500)  _timerScrcpy.Interval = 500;
                else if (!corriendo && _timerScrcpy.Interval != 2000) _timerScrcpy.Interval = 2000;
            }

            ActualizarPreviewComando();
        }

        private void IniciarLoopEstadoScrcpy()
        {
            _timerScrcpy?.Stop();
            _timerScrcpy?.Dispose();
            _timerScrcpy = new System.Windows.Forms.Timer { Interval = 500 };
            _timerScrcpy.Tick += (s, e) => ActualizarBotonesScrcpy();
            _timerScrcpy.Start();
        }

        // Actualización rápida del indicador desde el evento de TrackDevices, sin consulta ADB
        private void ActualizarIndicadorDispositivo(bool hayDispositivo)
        {
            if (lblEstadoIndicador == null || lblEstadoTexto == null) return;
            if (hayDispositivo)
            {
                lblEstadoIndicador.ForeColor = AppTheme.Success;
                lblEstadoTexto.Text = "Dispositivo conectado";
            }
            else
            {
                lblEstadoIndicador.ForeColor = AppTheme.Error;
                lblEstadoTexto.Text = "Sin dispositivo detectado";
            }
        }

        // Monitoreo USB mientras WiFi está activo porque track-devices no distingue USB de WiFi
        // de forma fiable cuando ambos están presentes. Hago polling de `adb devices` cada 2s.
        private async Task MonitorearUsbConWifiAsync()
        {
            System.Diagnostics.Debug.WriteLine("[WiFiMonitor] Iniciado");

            // Capturo el estado inicial para no tostar por USB que ya estaba conectado
            var (_, serialesInicio, _) = await Task.Run(() => adbManager.ListarDispositivos());
            if (IsDisposed || !_wifiConectado) return;
            _hayUsbDispositivo = serialesInicio.Any(s => !s.Contains(':'));
            System.Diagnostics.Debug.WriteLine($"[WiFiMonitor] Estado inicial: _hayUsbDispositivo={_hayUsbDispositivo}, seriales=[{string.Join(", ", serialesInicio)}]");

            while (_wifiConectado && !IsDisposed)
            {
                await Task.Delay(2000);
                if (!_wifiConectado || IsDisposed) break;

                var (_, seriales, _) = await Task.Run(() => adbManager.ListarDispositivos());
                if (IsDisposed || !_wifiConectado) break;

                bool hayUsb = seriales.Any(s => !s.Contains(':'));
                System.Diagnostics.Debug.WriteLine($"[WiFiMonitor] Poll: hayUsb={hayUsb} (prev={_hayUsbDispositivo}), seriales=[{string.Join(", ", seriales)}]");

                if (hayUsb == _hayUsbDispositivo) continue;
                _hayUsbDispositivo = hayUsb;

                InvokeSeguro(() =>
                {
                    if (!_wifiConectado || IsDisposed) return;
                    if (hayUsb)
                    {
                        System.Diagnostics.Debug.WriteLine("[WiFiMonitor] → Toast: Cable USB detectado");
                        ToastNotification.Mostrar(this,
                            "Cable USB detectado. Para volver a modo USB, ve a la sección Conexión y cierra el puerto.",
                            ToastNotification.ToastTipo.Info, 5000);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("[WiFiMonitor] → Toast: Conectado por WiFi");
                        ToastNotification.Mostrar(this,
                            "Conectado por WiFi. Ya puedes usar la app sin cable.",
                            ToastNotification.ToastTipo.Exito, 4000);
                    }
                });
            }

            _hayUsbDispositivo = false;
            System.Diagnostics.Debug.WriteLine("[WiFiMonitor] Detenido");
        }

        private async Task ActualizarEstadoDispositivoAsync(bool mostrarToast = false)
        {
            var (exito, seriales, _) = await Task.Run(() => adbManager.ListarDispositivos());
            if (lblEstadoIndicador == null || lblEstadoTexto == null) return;

            if (exito && seriales.Count > 0)
            {
                _hayDispositivo = true;
                lblEstadoIndicador.ForeColor = AppTheme.Success;
                lblEstadoTexto.Text = seriales.Count == 1
                    ? $"Conectado: {seriales[0]}"
                    : $"{seriales.Count} dispositivos conectados";

                // Si la sesión anterior terminó con resolución o DPI modificados (crash o Task Manager),
                // revierto silenciosamente antes de que el usuario empiece a usar la app
                if (_resolucionPendienteReset)
                {
                    await Task.Run(() => adbManager.ResetearResolucion());
                    _wmSizeActivo = false;
                    _resAdbActiva = false;
                    _resolucionPendienteReset = false;
                    GuardarConfigTema();
                }
                if (_dpiPendienteReset != 0)
                {
                    var (dpiOk, _, __) = await Task.Run(() => adbManager.ResetearDPI());
                    if (dpiOk) _dpiPendienteReset = 0;
                    GuardarConfigTema();
                }
            }
            else
            {
                _hayDispositivo = false;
                lblEstadoIndicador.ForeColor = AppTheme.Error;
                lblEstadoTexto.Text = "Sin dispositivo detectado";
            }
            _inicializacionCompleta = true;
            InvokeSeguro(() =>
            {
                ActualizarBotonesScrcpy();
                ActualizarLabelOptAdvertencia();
                if (!mostrarToast) return;
                if (_hayDispositivo)
                    ToastNotification.Mostrar(this, "Dispositivo conectado", ToastNotification.ToastTipo.Exito, 2500);
                else
                    ToastNotification.Mostrar(this, "Sin dispositivo detectado", ToastNotification.ToastTipo.Info, 2500);
            });
        }
    }
}
