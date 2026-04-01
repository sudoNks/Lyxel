using Guna.UI2.WinForms;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MobiladorStex
{
    public partial class Form1
    {
        // ══════════════════════════════════════════════════════════════
        // INICIO
        // ══════════════════════════════════════════════════════════════

        private void CargarUltimoPerfilSiExiste()
        {
            if (string.IsNullOrEmpty(_perfilSeleccionado)) return;
            var cfg = perfilManager.ObtenerPerfil(_perfilSeleccionado);
            if (cfg != null) CargarPerfilEnApp(cfg);
        }

        // Limpia conexiones WiFi residuales de sesiones anteriores antes de la
        // detección inicial, evitando falsos positivos cuando solo hay USB conectado.
        // El monitor ADB se pausa durante toda la limpieza y se reactiva al final,
        // cuando _inicializacionCompleta = true y el estado es estable.
        private async Task IniciarDeteccionDispositivoAsync()
        {
            adbManager.DetenerTrackDevices(); // silenciar eventos durante la limpieza

            await adbManager.DesconectarTodoAsync();
            await Task.Delay(1500);

            // Verificar que no queden dispositivos WiFi residuales tras el primer disconnect.
            // Si aún hay seriales con ':' (formato ip:puerto), reintentar una vez más.
            var (_, seriales, _) = await Task.Run(() => adbManager.ListarDispositivos());
            if (seriales.Any(s => s.Contains(':')))
            {
                System.Diagnostics.Debug.WriteLine("[Init] WiFi residual detectado tras disconnect — reintentando");
                await adbManager.DesconectarTodoAsync();
                await Task.Delay(500);
            }

            // ActualizarEstadoDispositivoAsync pone _inicializacionCompleta = true al final.
            await ActualizarEstadoDispositivoAsync(mostrarToast: true);

            // Reanudar el monitor solo cuando el estado ya es definitivo.
            if (!IsDisposed) adbManager.IniciarTrackDevices();
        }

        private void LoadInicioPage()
        {
            var cardEstado = CreateCard("Estado del Dispositivo", S(30), S(20), S(160));

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
                BorderRadius = 4
            };
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

            var cardRapido = CreateCard("Acceso Rápido", S(30), S(200), S(180));

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
                Enabled = false, // deshabilitado hasta confirmar dispositivo
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            btnIniciarScrcpy.Click += (s, e) => LanzarScrcpy();

            btnDetenerScrcpy = new Guna2Button()
            {
                Text = "⏹  DETENER SCRCPY",
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
                Enabled = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            btnDetenerScrcpy.Click += (s, e) => DetenerScrcpy();

            lblUltimoPerfil = new Label()
            {
                Text = ObtenerTextoUltimoPerfil(),
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = textSecondary,
                Left = S(24),
                Top = S(158),
                AutoSize = true
            };

            cardRapido.Controls.AddRange(new Control[] { btnIniciarScrcpy, btnDetenerScrcpy, lblUltimoPerfil });
            contentPanel.Controls.AddRange(new Control[] { cardEstado, cardRapido });

            // Solo refrescar estado al navegar aquí después de la init — durante el arranque
            // IniciarDeteccionDispositivoAsync es el responsable de la primera detección.
            if (_inicializacionCompleta) _ = ActualizarEstadoDispositivoAsync();
            IniciarLoopEstadoScrcpy();
        }

        private async void LanzarScrcpy()
        {
            try
            {
                if (!_modoOtg)
                {
                    // Modo normal: ADB requerido para verificar el dispositivo
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
                    // Modo OTG: si no hay serial seleccionado, intentar obtenerlo via ADB
                    // para pasar -s [serial] y evitar ambigüedad con múltiples conexiones activas
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
                        // Si ADB no detecta nada (adaptador OTG sin USB debug),
                        // scrcpy --otg identifica el dispositivo por USB físico — continuar sin serial
                    }
                }

                var config = ObtenerConfigActual();

                bool exito = scrcpyManager.Lanzar(config);

                if (!exito)
                {
                    MessageBox.Show(
                        "No se pudo lanzar scrcpy.\n\nIntenta reconectar ADB e inténtalo de nuevo.",
                        "Error al iniciar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _scrcpyEstabaActivo = true;
                _ultimaSesionWifi = _wifiConectado;
                _ultimaSesionOtg = _modoOtg;
                GuardarConfigTema();
                ActualizarBotonesScrcpy();
                this.WindowState = FormWindowState.Minimized;

                // OTG: detectar cierre rápido por fallo de compatibilidad
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
            // Revertir wm size y resolución ADB si hay dispositivo conectado
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
                btnIniciarScrcpy.Text = "▶  INICIAR SCRCPY";
            else
                btnIniciarScrcpy.Text = puedeIniciar ? "▶  INICIAR SCRCPY" : "Sin dispositivo";
            btnDetenerScrcpy.Enabled = corriendo;
        }

        private void IniciarLoopEstadoScrcpy()
        {
            _timerScrcpy?.Stop();
            _timerScrcpy?.Dispose();
            _timerScrcpy = new System.Windows.Forms.Timer { Interval = 500 };
            _timerScrcpy.Tick += (s, e) => ActualizarBotonesScrcpy();
            _timerScrcpy.Start();
        }

        // Actualización inmediata desde evento TrackDevices — sin consulta ADB
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

        // ──────────────────────────────────────────────────────────────────────────
        // Monitorea la presencia de USB mientras WiFi está activo.
        // track-devices no distingue USB de WiFi de forma fiable cuando ambos
        // están presentes. Esta tarea hace polling de `adb devices` para detectar
        // cambios reales en el cable USB.
        private async Task MonitorearUsbConWifiAsync()
        {
            System.Diagnostics.Debug.WriteLine("[WiFiMonitor] Iniciado");

            // Capturar estado inicial para no tostar por USB ya conectado
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

                // Recuperación post-kill: si la sesión anterior terminó con resolución
                // modificada (cierre forzado por Task Manager u otro crash), revertirla
                // ahora silenciosamente antes de que el usuario empiece a usar la app.
                if (_resolucionPendienteReset)
                {
                    await Task.Run(() => adbManager.ResetearResolucion());
                    _wmSizeActivo = false;
                    _resAdbActiva = false;
                    _resolucionPendienteReset = false;
                    GuardarConfigTema(); // limpiar el flag persistido
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
                if (!mostrarToast) return;
                if (_hayDispositivo)
                    ToastNotification.Mostrar(this, "Dispositivo conectado", ToastNotification.ToastTipo.Exito, 2500);
                else
                    ToastNotification.Mostrar(this, "Sin dispositivo detectado", ToastNotification.ToastTipo.Info, 2500);
            });
        }
    }
}
