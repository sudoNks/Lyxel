using Guna.UI2.WinForms;
using IniParser;
using IniParser.Model;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System;
using System.Linq;
using Panel = System.Windows.Forms.Panel;

namespace MobiladorStex
{
    public partial class Form1 : Form
    {
        // ══════════════════════════════════════════════════════════════
        // SISTEMA DE TEMAS — Solo Azul/Verde + Oscuro/Claro
        // ══════════════════════════════════════════════════════════════

        private Color accentColor;
        private Color bgPrimary;
        private Color bgSecondary;
        private Color bgCard;
        private Color textPrimary;
        private Color textSecondary;

        // Referencias globales
        private Panel sidePanel;
        private Panel mainPanel;
        private Panel contentPanel;
        private Label lblTituloPagina;
        private Label lblLogo;
        private Label lblVersion;
        private Guna2Button btnToggle;
        private Guna2Button btnGuardadoRapido;
        private Guna2Button[] navButtons;
        private ADBManager adbManager;
        private string scrcpyPath;
        private ScrcpyManager scrcpyManager;
        private PerfilManager perfilManager;
        private Label lblEstadoIndicador;
        private Label lblEstadoTexto;
        private Label lblUltimoPerfil;
        private Guna2Button btnIniciarScrcpy;
        private Guna2Button btnDetenerScrcpy;
        private System.Windows.Forms.Timer _timerScrcpy;
        private Form? _flotante;

        // ── VIDEO Y AUDIO — Estado persistente ───────────────────────
        private bool _video = true;
        private bool _audio = true;
        private bool _audioDoble = false;
        private string _audioCodec = "opus";
        private int _audioBitrate = 128;
        private int _fps = 90;
        private int _bitrate = 32;
        private int _maxSize = 1600;
        private int _windowWidth = 0;
        private int _windowHeight = 0;
        private string _videoCodec = "h264";
        private int _videoBuffer = 0;
        private int _audioBuffer = 50;
        private bool _disableScreensaver = false;
        private bool _stayAwake = false;
        private bool _turnScreenOff = false;
        private string _shortcutMod = "lalt";
        private string _inputMode = "uhid";
        private int _pointerSpeed = 0;
        private bool _printFps = false;
        private bool _forwardAllClicks = false;
        private bool _mostrarFlotante = true;
        private bool _wmSizeActivo = false;
        private bool _resAdbActiva = false; // true cuando hay resolución ADB aplicada
        private bool _resolucionPendienteReset = false; // true si la app se cerró con resolución modificada sin revertir
        private string _wmSizeValor = "";
        private bool _useAdvancedEncoder = false;
        private bool _scrcpyEstabaActivo = false; // bandera para detección de desconexión inesperada
        private bool _hayDispositivo = false; // estado actual del dispositivo
        private bool _operacionWifiEnCurso = false; // suprime actualizaciones visuales de track-devices durante setup WiFi
        private bool _hayUsbDispositivo = false;    // estado USB conocido mientras WiFi está activo (monitoreado por polling)
        private bool _puertotcpActivo = false; // puerto tcpip habilitado
        private bool _wifiConectado = false; // conexión WiFi establecida
        private bool _inicializacionCompleta = false; // detección inicial completada
        private string _videoEncoder = "";
        private List<string> _encodersDetectados = new();
        private List<string> _encodersDisplayLabels = new();

        // PANTALLA
        private bool _fullscreen = false;
        private string _fullscreenCrop = "";
        private bool _cropActivo = false;
        private bool _avisoAdbVisto = false;
        private bool _avisoWmSizeVisto = false; // true cuando crop está calculado y activo
        private int _resolucionAncho = 1080;
        private int _resolucionAlto = 2400;
        private string _aspectRatio = "16:9";
        private int _customRatioW = 16;
        private int _customRatioH = 9;
        private int _dpi = 420;

        // CONEXIÓN
        private bool _usarWifi = false;
        private string _wifiIp = "";
        private int _wifiPuerto = 5555;
        private bool _modoOtg = false;
        private string _otgSerial = "";

        // INDICADORES DE SESIÓN ANTERIOR
        private bool _ultimaSesionWifi = false;
        private bool _ultimaSesionOtg = false;
        private int _ultimoDpiAplicado = 0;            // 0 = nunca aplicado
        private int _ultimaVelocidadCursor = int.MinValue; // MinValue = nunca aplicado

        // PERFILES
        private string _perfilSeleccionado = "";
        private ListBox _lstPerfiles;
        private Panel _panelDetalle;

        private string _configPath;
        private bool _sidebarAnimating = false;
        private bool _haysCambiosSinGuardar = false;
        private bool _cargandoPagina = false;
        private bool sidebarExpanded = true;
        private const int SIDEBAR_WIDTH = 200;
        private const int SIDEBAR_COLLAPSED = 56;
        private int paginaActiva = 0;
        private string tituloPaginaActiva = "Inicio";

        private int S(int px) => (int)Math.Round(px * this.DeviceDpi / 96.0);

        public Form1()
        {
            InitializeComponent();
            Application.AddMessageFilter(new ScrollFocusFilter());

            string adbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "adb", "adb.exe");
            scrcpyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin", "scrcpy", "scrcpy.exe");

            adbManager = new ADBManager(adbPath);
            scrcpyManager = new ScrcpyManager(scrcpyPath, adbPath);

            string perfilesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "perfiles.ini");
            if (!File.Exists(perfilesPath))
                File.WriteAllText(perfilesPath, "", System.Text.Encoding.UTF8);

            perfilManager = new PerfilManager(perfilesPath);
            _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");

            CargarConfigTema();
            ApplyTheme();
            BuildUI();
            CargarUltimoPerfilSiExiste();

            _ = IniciarDeteccionDispositivoAsync();

            // Track devices event-driven — detecta conexión/desconexión sin polling
            adbManager.OnDispositivoCambio += hayDispositivo =>
            {
                InvokeSeguro(() =>
                {
                    _hayDispositivo = hayDispositivo;
                    // Durante setup WiFi (habilitar puerto / detectar IP), adb tcpip reinicia el
                    // daemon del dispositivo causando una desconexión USB transitoria. Suprimimos
                    // las actualizaciones visuales para evitar que el indicador parpadee en rojo y
                    // que aparezcan toasts de "conectado/desconectado" fuera de contexto.
                    if (_operacionWifiEnCurso) return;
                    ActualizarIndicadorDispositivo(hayDispositivo);
                    ActualizarBotonesScrcpy();
                    if (!_inicializacionCompleta)
                    {
                        // Evento transitorio antes de que la detección inicial termine — ignorar.
                    }
                    // Los toasts de conexión/desconexión USB se gestionan en OnDispositivoUsbCambio.
                    // OnDispositivoCambio solo maneja el estado general del indicador.
                });
            };

            // Evento USB-específico: cuando NO hay WiFi activo, maneja toasts normales.
            // Cuando WiFi está activo, los cambios USB se detectan por MonitorearUsbConWifiAsync.
            adbManager.OnDispositivoUsbCambio += hayUsb =>
            {
                System.Diagnostics.Debug.WriteLine($"[USB] OnDispositivoUsbCambio: hayUsb={hayUsb} | _wifiConectado={_wifiConectado} | _operacionWifiEnCurso={_operacionWifiEnCurso} | _inicializacionCompleta={_inicializacionCompleta}");
                InvokeSeguro(() =>
                {
                    if (_operacionWifiEnCurso || !_inicializacionCompleta) return;
                    if (_wifiConectado) return; // WiFi activo: gestionado por MonitorearUsbConWifiAsync

                    if (hayUsb)
                    {
                        if (!_puertotcpActivo)
                            ToastNotification.Mostrar(this, "Dispositivo conectado", ToastNotification.ToastTipo.Exito, 2500);
                        // Si _puertotcpActivo: reconexión transitoria post-tcpip — silencioso
                    }
                    else
                    {
                        if (_scrcpyEstabaActivo)
                        {
                            _scrcpyEstabaActivo = false;
                            if (this.WindowState == FormWindowState.Minimized)
                                this.WindowState = FormWindowState.Normal;
                            FlashVentana(this);
                            ToastNotification.Mostrar(this,
                                "Dispositivo desconectado — si fue inesperado, repórtalo en Discord (Acerca de)",
                                ToastNotification.ToastTipo.Advertencia, 5000, forzar: true);
                        }
                        else
                        {
                            ToastNotification.Mostrar(this, "Dispositivo desconectado", ToastNotification.ToastTipo.Info, 2500);
                        }
                    }
                });
            };

            // IniciarTrackDevices() se llama al final de IniciarDeteccionDispositivoAsync,
            // una vez que _inicializacionCompleta = true y el estado ADB está estable.

            this.FormClosing += (s, e) =>
            {
                // 1. Detener scrcpy primero — bloqueante:
                //    CloseMainWindow → WaitForExit(2s) → Kill() si sigue vivo
                //    Garantiza que el proceso esté muerto antes de continuar
                scrcpyManager.Detener();

                // 2. Cerrar ventana flotante DESPUÉS de que scrcpy terminó,
                //    para que su timer no dispare _onDetener sobre recursos liberados
                try { _flotante?.Close(); } catch { }
                _flotante = null;

                adbManager.DetenerTrackDevices();

                // 3. OTG: desconectar serial ADB si tenía USB debug activo
                if (_modoOtg && !string.IsNullOrEmpty(_otgSerial))
                    adbManager.DesconectarTodo();

                bool hayDispositivo = _hayDispositivo;

                // ── WiFi / TCP cleanup (sync — no fire-and-forget) ────────
                if (_puertotcpActivo || _usarWifi)
                {
                    adbManager.DesconectarTodo();                    // adb disconnect — rápido
                    if (hayDispositivo && _puertotcpActivo)
                        adbManager.AplicarUsb();                     // revierte tcpip → USB, sin kill-server
                }

                // ── Resolución — revertir si fue modificada ───────────────
                if (_wmSizeActivo || _resAdbActiva)
                {
                    if (hayDispositivo)
                    {
                        var (resetOk, _) = adbManager.ResetearResolucion();
                        if (resetOk)
                        {
                            _wmSizeActivo = false;
                            _resAdbActiva = false;
                        }
                    }
                    // Si no hay dispositivo o falló, el flag queda activo →
                    // se persiste en config.ini y se limpia en el próximo arranque
                }

                // Persistir estado (incluye resolucion_pendiente_reset actualizado)
                GuardarConfigTema();

                adbManager.CerrarDaemonLocal();
            };

            try
            {
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                string? resName = assembly.GetManifestResourceNames()
                                          .FirstOrDefault(n => n.EndsWith("LogoApp.ico"));
                if (!string.IsNullOrEmpty(resName))
                {
                    using Stream? stream = assembly.GetManifestResourceStream(resName);
                    if (stream != null)
                        this.Icon = new Icon(stream);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error cargando icono: {ex.Message}");
            }
        }

        // ══════════════════════════════════════════════════════════════
        // CONFIG — CARGAR Y GUARDAR
        // ══════════════════════════════════════════════════════════════

        private void CargarConfigTema()
        {
            if (!File.Exists(_configPath)) return;
            try
            {
                var parser = new FileIniDataParser();
                var data = parser.ReadFile(_configPath);

                // Avisos avanzados vistos
                if (data["Tema"].ContainsKey("aviso_adb_visto"))
                    bool.TryParse(data["Tema"]["aviso_adb_visto"], out _avisoAdbVisto);
                if (data["Tema"].ContainsKey("aviso_wmsize_visto"))
                    bool.TryParse(data["Tema"]["aviso_wmsize_visto"], out _avisoWmSizeVisto);

                // Recuperación post-kill: resolución modificada sin revertir en sesión anterior
                if (data.Sections.ContainsSection("Dispositivo") &&
                    data["Dispositivo"].ContainsKey("resolucion_pendiente_reset"))
                    bool.TryParse(data["Dispositivo"]["resolucion_pendiente_reset"], out _resolucionPendienteReset);

                // Último perfil activo
                string ultimoPerfil = data["Tema"]["ultimo_perfil"];
                if (!string.IsNullOrEmpty(ultimoPerfil))
                {
                    _perfilSeleccionado = ultimoPerfil;
                    var cfg = perfilManager?.ObtenerPerfil(ultimoPerfil);
                    if (cfg != null) CargarPerfilEnApp(cfg);
                }

                // Encoders detectados — persisten entre sesiones
                // Formato: nombres separados por '|', paralelos a los display labels
                string encDet = data["Video"]["encoders_detectados"] ?? "";
                string encLbl = data["Video"]["encoders_display_labels"] ?? "";

                _encodersDetectados = string.IsNullOrWhiteSpace(encDet)
                    ? new List<string>()
                    : new List<string>(encDet.Split('|').Where(x => !string.IsNullOrWhiteSpace(x)));

                _encodersDisplayLabels = string.IsNullOrWhiteSpace(encLbl)
                    ? new List<string>()
                    : new List<string>(encLbl.Split('|').Where(x => !string.IsNullOrWhiteSpace(x)));

                // Sesión anterior — WiFi, OTG, DPI aplicado, velocidad cursor
                if (data.Sections.ContainsSection("Sesion"))
                {
                    var s = data["Sesion"];
                    if (s.ContainsKey("ultima_sesion_wifi"))
                        bool.TryParse(s["ultima_sesion_wifi"], out _ultimaSesionWifi);
                    if (s.ContainsKey("ultima_sesion_otg"))
                        bool.TryParse(s["ultima_sesion_otg"], out _ultimaSesionOtg);
                    if (s.ContainsKey("wifi_ip") && !string.IsNullOrEmpty(s["wifi_ip"]))
                        _wifiIp = s["wifi_ip"];
                    if (s.ContainsKey("wifi_puerto") && int.TryParse(s["wifi_puerto"], out int wp) && wp >= 1024 && wp <= 65535)
                        _wifiPuerto = wp;
                    if (s.ContainsKey("otg_serial") && !string.IsNullOrEmpty(s["otg_serial"]))
                        _otgSerial = s["otg_serial"];
                    if (s.ContainsKey("ultimo_dpi_aplicado") && int.TryParse(s["ultimo_dpi_aplicado"], out int ud) && ud > 0)
                        _ultimoDpiAplicado = ud;
                    if (s.ContainsKey("ultima_velocidad_cursor") && int.TryParse(s["ultima_velocidad_cursor"], out int uvc))
                        _ultimaVelocidadCursor = uvc;
                }
            }
            catch { }
        }

        private void GuardarConfigTema()
        {
            try
            {
                var parser = new FileIniDataParser();
                var data = new IniData();

                // Tema
                data.Sections.AddSection("Tema");
                data["Tema"]["ultimo_perfil"] = _perfilSeleccionado ?? "";
                data["Tema"]["aviso_adb_visto"] = _avisoAdbVisto.ToString().ToLower();
                data["Tema"]["aviso_wmsize_visto"] = _avisoWmSizeVisto.ToString().ToLower();

                // Encoders — guardamos la lista detectada para no tener que volver a detectar
                data.Sections.AddSection("Video");
                data["Video"]["encoders_detectados"] = string.Join("|", _encodersDetectados);
                data["Video"]["encoders_display_labels"] = string.Join("|", _encodersDisplayLabels);

                // Estado de recuperación: si la resolución quedó modificada sin revertir
                // (cierre normal fallido o kill forzado), se limpia en el próximo arranque
                data.Sections.AddSection("Dispositivo");
                data["Dispositivo"]["resolucion_pendiente_reset"] = (_wmSizeActivo || _resAdbActiva).ToString().ToLower();

                // Sesión anterior — para indicadores visuales y restauración parcial de estado
                data.Sections.AddSection("Sesion");
                data["Sesion"]["ultima_sesion_wifi"] = _ultimaSesionWifi.ToString().ToLower();
                data["Sesion"]["ultima_sesion_otg"] = _ultimaSesionOtg.ToString().ToLower();
                data["Sesion"]["wifi_ip"] = _wifiIp ?? "";
                data["Sesion"]["wifi_puerto"] = _wifiPuerto.ToString();
                data["Sesion"]["otg_serial"] = _otgSerial ?? "";
                data["Sesion"]["ultimo_dpi_aplicado"] = _ultimoDpiAplicado > 0 ? _ultimoDpiAplicado.ToString() : "";
                data["Sesion"]["ultima_velocidad_cursor"] = _ultimaVelocidadCursor != int.MinValue ? _ultimaVelocidadCursor.ToString() : "";

                parser.WriteFile(_configPath, data);
            }
            catch { }
        }

        // ══════════════════════════════════════════════════════════════
        // TEMAS
        // ══════════════════════════════════════════════════════════════

        private void ApplyTheme()
        {
            // ── Paleta fija MobiladorSteX ─────────────────────────────
            accentColor = AppTheme.Accent;   // #6b2fc4 morado brillante
            bgPrimary = AppTheme.BgPrimary;     // #212023 fondo general
            bgSecondary = AppTheme.BgSecondary;     // #1a1a1c sidebar
            bgCard = AppTheme.BgCard;     // #2a2a2d cards
            textPrimary = AppTheme.TextPrimary;  // #eeeeee texto principal
            textSecondary = AppTheme.TextSecondary;  // #787878 texto secundario
        }




        /// <summary>Invoke seguro — no lanza si el Form ya fue cerrado.</summary>
        private void InvokeSeguro(Action accion)
        {
            try
            {
                if (!this.IsDisposed && this.IsHandleCreated)
                    this.Invoke(accion);
            }
            catch (ObjectDisposedException) { }
            catch (InvalidOperationException) { }
        }



        // ══════════════════════════════════════════════════════════════
        // HELPERS
        // ══════════════════════════════════════════════════════════════


        private Panel CreateCard(string title, int left, int top, int height = 150)
        {
            var card = new Panel()
            {
                Left = left,
                Top = top,
                Width = contentPanel.Width - S(60),
                Height = height,
                BackColor = bgCard,
                BorderStyle = BorderStyle.None,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            card.Controls.Add(new Label()
            {
                Text = title,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = textPrimary,
                Left = S(24),
                Top = S(20),
                AutoSize = true
            });
            return card;
        }

        private StexNumericUpDown CreateNumeric(int left, int top, int width,
            decimal min, decimal max, decimal value, decimal increment = 1)
        {
            return new StexNumericUpDown()
            {
                Left = left,
                Top = top,
                Width = width,
                Height = S(34),
                Minimum = min,
                Maximum = max,
                Value = value,
                Increment = increment,
                Font = new Font("Segoe UI", 10f)
            };
        }


        private async Task ActualizarEstadoAdbAsync(Label lblEstado)
        {
            var (exito, seriales, _) = await Task.Run(() => adbManager.ListarDispositivos());
            if (lblEstado == null) return;
            if (exito && seriales.Count > 0)
            {
                lblEstado.Text = $"●  {seriales.Count} dispositivo(s): {string.Join(", ", seriales)}";
                lblEstado.ForeColor = AppTheme.Success;
            }
            else
            {
                lblEstado.Text = "●  Sin dispositivo detectado";
                lblEstado.ForeColor = AppTheme.Error;
            }
        }

        private string ObtenerVersionApp()
        {
            var v = System.Reflection.Assembly
                .GetExecutingAssembly()
                .GetName()
                .Version;
            return v != null ? $"v{v.Major}.{v.Minor}.{v.Build}" : "v1.1.0";
        }

        // Parpadeo en taskbar — llama la atención sin forzar el foco
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool FlashWindow(IntPtr hwnd, bool bInvert);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool FlashWindowEx(ref FLASHWINFO pfwi);

        [System.Runtime.InteropServices.StructLayout(
            System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        private const uint FLASHW_ALL = 3; // taskbar + título
        private const uint FLASHW_TIMERNOFG = 12; // parpadea hasta que el usuario lo enfoca

        private static void FlashVentana(Form form)
        {
            if (form == null || form.IsDisposed) return;
            var info = new FLASHWINFO
            {
                cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(FLASHWINFO)),
                hwnd = form.Handle,
                dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG,
                uCount = 5,
                dwTimeout = 0
            };
            FlashWindowEx(ref info);
        }

        private void GuardadoRapido()
        {
            if (string.IsNullOrEmpty(_perfilSeleccionado)) return;
            var config = ObtenerConfigActual();
            var (exito, error) = perfilManager.GuardarConfigEnPerfil(_perfilSeleccionado, config);
            if (exito)
            {
                LimpiarIndicadorCambios();
            }
            else
            {
                MessageBox.Show($"No se pudo guardar:\n{error}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ObtenerTextoUltimoPerfil()
        {
            string perfil = !string.IsNullOrEmpty(_perfilSeleccionado) ? _perfilSeleccionado : "Sin perfil";
            string modo = _modoOtg ? "OTG" : _usarWifi ? "WiFi"
                          : (!_video && !_audio) ? "Control Only" : "USB";
            return $"Perfil: {perfil}  |  {_fps} FPS  |  {_bitrate} Mb  |  {_videoCodec}  |  {modo}";
        }

        private ScrcpyConfig ObtenerConfigActual() => new ScrcpyConfig
        {
            Video = _video,
            Audio = _audio,
            AudioDoble = _audioDoble,
            AudioCodec = _audioCodec,
            AudioBitrate = _audioBitrate,
            Fps = _fps,
            Bitrate = _bitrate,
            MaxSize = _maxSize,
            WindowWidth = _windowWidth,
            WindowHeight = _windowHeight,
            VideoCodec = _videoCodec,
            VideoBuffer = _videoBuffer,
            AudioBuffer = _audioBuffer,
            DisableScreensaver = _disableScreensaver,
            StayAwake = _stayAwake,
            TurnScreenOff = _turnScreenOff,
            ShortcutMod = _shortcutMod,
            Fullscreen = _fullscreen,
            FullscreenCrop = _fullscreenCrop,
            ModoOtg = _modoOtg,
            OtgSerial = _otgSerial,
            UsarWifi = _wifiConectado,
            WifiIp = _wifiIp,
            WifiPuerto = _wifiPuerto,
            ResolucionAncho = _resolucionAncho,
            ResolucionAlto = _resolucionAlto,
            AspectRatio = _aspectRatio,
            CustomRatioW = _customRatioW,
            CustomRatioH = _customRatioH,
            Dpi = _dpi,
            PrintFps = _printFps && _mostrarFlotante,
            ForwardAllClicks = _forwardAllClicks,
            MostrarFlotante = _mostrarFlotante,
            WmSizeActivo = _wmSizeActivo,
            WmSizeValor = _wmSizeValor,
            UseAdvancedEncoder = _useAdvancedEncoder,
            VideoEncoder = _videoEncoder,
            InputMode = _inputMode,
            PointerSpeed = _pointerSpeed
        };

        private async Task DetectarDpiAlCargarAsync(Label lblDpiActual, StexNumericUpDown numDpi)
        {
            var (exito, dpi, _) = await adbManager.DetectarDPIAsync();
            if (exito && lblDpiActual != null)
            {
                _dpi = dpi;
                numDpi.Value = dpi;
                lblDpiActual.Text = $"DPI actual: {dpi}";
                lblDpiActual.ForeColor = AppTheme.Success;
            }
        }

        private double ObtenerAspectRatioValor() => _aspectRatio switch
        {
            "16:9" => 16.0 / 9.0,
            "16:10" => 16.0 / 10.0,
            "21:9" => 21.0 / 9.0,
            "18:9" => 18.0 / 9.0,
            "4:3" => 4.0 / 3.0,
            "Personalizado" => _customRatioH > 0 ? (double)_customRatioW / _customRatioH : 16.0 / 9.0,
            _ => 16.0 / 9.0
        };


    }
}
