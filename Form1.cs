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
        private FloatingWindow? _flotante;

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
                adbManager.DetenerTrackDevices();
                scrcpyManager.Detener();

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
                string resName = assembly.GetManifestResourceNames()
                                          .FirstOrDefault(n => n.EndsWith("LogoApp.ico"));
                if (!string.IsNullOrEmpty(resName))
                {
                    using Stream stream = assembly.GetManifestResourceStream(resName);
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
            accentColor = Color.FromArgb(107, 47, 196);   // #6b2fc4 morado brillante
            bgPrimary = Color.FromArgb(33, 32, 35);     // #212023 fondo general
            bgSecondary = Color.FromArgb(26, 26, 28);     // #1a1a1c sidebar
            bgCard = Color.FromArgb(42, 42, 45);     // #2a2a2d cards
            textPrimary = Color.FromArgb(238, 238, 238);  // #eeeeee texto principal
            textSecondary = Color.FromArgb(120, 120, 120);  // #787878 texto secundario
        }



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
        // BUILD UI
        // ══════════════════════════════════════════════════════════════

        private void BuildUI()
        {
            this.Text = "MobiladorSteX";
            this.Size = new Size(1100, 720);
            this.BackColor = bgPrimary;
            this.ForeColor = textPrimary;
            this.Font = new Font("Segoe UI", 9f);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(900, 600);

            // ── SIDEBAR ──────────────────────────────────────────────
            sidePanel = new Panel()
            {
                Left = 0,
                Top = 0,
                Width = SIDEBAR_WIDTH,
                Height = this.ClientSize.Height,
                BackColor = bgSecondary,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
            };

            lblLogo = new Label()
            {
                Text = "MobiladorSteX",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = textPrimary,
                Left = 16,
                Top = 20,
                AutoSize = true
            };

            lblVersion = new Label()
            {
                Text = ObtenerVersionApp(),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(147, 90, 220),
                Left = 18,
                Top = 48,
                AutoSize = true
            };


            navButtons = new Guna2Button[7];
            navButtons[0] = CreateNavButton("🏠  Inicio", 0);
            navButtons[1] = CreateNavButton("🎬  Video y Audio", 1);
            navButtons[2] = CreateNavButton("🖥️  Pantalla", 2);
            navButtons[3] = CreateNavButton("📶  Conexión", 3);
            navButtons[4] = CreateNavButton("⚙️  Opciones Extras", 4);
            navButtons[5] = CreateNavButton("💾  Perfiles", 5);
            navButtons[6] = CreateNavButton("ℹ️  Acerca de", 6);

            navButtons[paginaActiva].Checked = true;
            navButtons[paginaActiva].FillColor = Color.FromArgb(60, 40, 80);

            btnToggle = new Guna2Button()
            {
                Text = "☰",
                Width = 40,
                Height = 40,
                Left = 8,
                Top = this.ClientSize.Height - 170,
                Font = new Font("Segoe UI", 13f),
                FillColor = Color.Transparent,
                ForeColor = Color.FromArgb(150, 150, 150),
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
                Width = 200,
                Height = 32,
                Left = 10,
                Top = this.ClientSize.Height - 210,
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
                Left = SIDEBAR_WIDTH,
                Top = 0,
                Width = this.ClientSize.Width - SIDEBAR_WIDTH,
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
                Height = 70,
                BackColor = bgPrimary,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            lblTituloPagina = new Label()
            {
                Text = tituloPaginaActiva,
                Font = new Font("Segoe UI", 24f, FontStyle.Bold),
                ForeColor = textPrimary,
                Left = 30,
                Top = 6,
                AutoSize = true
            };

            var lblAviso = new Label()
            {
                Text = "",
                Name = "lblAvisoHeader",
                Font = new Font("Segoe UI", 9f, FontStyle.Italic),
                ForeColor = Color.FromArgb(255, 167, 38),
                Left = 30,
                Top = 52,
                AutoSize = true
            };
            headerPanel.Controls.AddRange(new Control[] { lblTituloPagina, lblAviso });

            contentPanel = new Panel()
            {
                Left = 0,
                Top = 70,
                Width = mainPanel.Width - 20,
                Height = mainPanel.Height - 70,
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
            }
            navButtons[index].Checked = true;
            navButtons[index].FillColor = Color.FromArgb(60, 40, 80);

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
                int targetWidth = sidebarExpanded ? SIDEBAR_WIDTH : SIDEBAR_COLLAPSED;

                sidePanel.Width = targetWidth;
                mainPanel.Left = targetWidth;
                mainPanel.Width = this.ClientSize.Width - targetWidth;

                lblLogo.Visible = sidebarExpanded;
                // lblVersion siempre visible pero texto cambia
                if (sidebarExpanded)
                {
                    lblVersion.Text = ObtenerVersionApp();
                    lblVersion.Left = 18;
                }
                else
                {
                    lblVersion.Text = "v" + (System.Reflection.Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.2.0");
                    lblVersion.Left = 4;
                }

                // Botón guardado rápido: ajustar tamaño igual que navButtons
                if (btnGuardadoRapido != null && btnGuardadoRapido.Visible)
                {
                    btnGuardadoRapido.Width = sidebarExpanded ? 200 : 40;
                    btnGuardadoRapido.Left = sidebarExpanded ? 10 : 8;
                    btnGuardadoRapido.Text = sidebarExpanded ? "💾 Guardar" : "💾";
                }

                foreach (var btn in navButtons)
                {
                    if (sidebarExpanded)
                        btn.Text = btn.Tag?.ToString() ?? btn.Text;
                    else
                    {
                        string full = btn.Tag?.ToString() ?? "";
                        btn.Text = full.Length >= 2 ? full.Substring(0, 2) : "●";
                    }
                    btn.Width = sidebarExpanded ? 200 : 40;
                    btn.Left = sidebarExpanded ? 10 : 8;
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
                navButtons[5].Text = sidebarExpanded ? "💾  Perfiles ●" : "💾";
                navButtons[5].Tag = "💾  Perfiles ●";
            }
            // Diferir la visibilidad del botón para no interrumpir renders en curso
            if (btnGuardadoRapido != null && !string.IsNullOrEmpty(_perfilSeleccionado))
            {
                this.BeginInvoke(() =>
                {
                    if (btnGuardadoRapido == null) return;
                    btnGuardadoRapido.Visible = true;
                    btnGuardadoRapido.Width = sidebarExpanded ? 200 : 40;
                    btnGuardadoRapido.Left = sidebarExpanded ? 10 : 8;
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
                navButtons[5].Text = sidebarExpanded ? "💾  Perfiles" : "💾";
                navButtons[5].Tag = "💾  Perfiles";
            }
            if (btnGuardadoRapido != null)
                btnGuardadoRapido.Visible = false;
        }

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
            var cardEstado = CreateCard("Estado del Dispositivo", 30, 20, 160);

            lblEstadoIndicador = new Label()
            {
                Text = "●",
                Font = new Font("Segoe UI", 12f),
                ForeColor = Color.FromArgb(120, 120, 120), // gris mientras verifica
                Left = 24,
                Top = 62,
                AutoSize = true
            };

            lblEstadoTexto = new Label()
            {
                Text = "Verificando...",
                Font = new Font("Segoe UI", 10f),
                ForeColor = textSecondary,
                Left = 44,
                Top = 64,
                AutoSize = true
            };

            var btnReconectar = new Guna2Button()
            {
                Text = "RECONECTAR ADB",
                Width = 180,
                Height = 36,
                Left = 24,
                Top = 100,
                Font = new Font("Segoe UI", 9f),
                FillColor = Color.FromArgb(55, 40, 75),
                ForeColor = textSecondary,
                BorderColor = Color.FromArgb(80, 60, 100),
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

            var cardRapido = CreateCard("Acceso Rápido", 30, 200, 180);

            btnIniciarScrcpy = new Guna2Button()
            {
                Text = "Detectando dispositivo...",
                Width = cardRapido.Width - 48,
                Height = 48,
                Left = 24,
                Top = 56,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                FillColor = Color.FromArgb(60, 45, 80),
                ForeColor = Color.FromArgb(150, 150, 150),
                BorderRadius = 6,
                Enabled = false, // deshabilitado hasta confirmar dispositivo
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            btnIniciarScrcpy.Click += (s, e) => LanzarScrcpy();

            btnDetenerScrcpy = new Guna2Button()
            {
                Text = "⏹  DETENER SCRCPY",
                Width = cardRapido.Width - 48,
                Height = 36,
                Left = 24,
                Top = 114,
                Font = new Font("Segoe UI", 9.5f),
                FillColor = Color.FromArgb(55, 40, 75),
                ForeColor = textSecondary,
                BorderColor = Color.FromArgb(80, 60, 100),
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
                Left = 24,
                Top = 158,
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
                // OTG no requiere ADB — scrcpy detecta el dispositivo por USB físico
                var seriales = new List<string>();
                if (!_modoOtg)
                {
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
                    seriales = serialesAdb;
                }

                if (_modoOtg && string.IsNullOrWhiteSpace(_otgSerial))
                {
                    if (seriales.Count == 1)
                        _otgSerial = seriales[0];
                    else if (seriales.Count > 1)
                    {
                        MessageBox.Show(
                            "Hay varios dispositivos conectados.\n\n" +
                            "Ve a Conexión → Modo OTG → Detectar Dispositivos\n" +
                            "y selecciona el serial del dispositivo a usar.",
                            "OTG — Selecciona dispositivo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    // Si ADB no ve el dispositivo (sin USB debug), scrcpy --otg lo identifica por USB físico
                }

                var config = ObtenerConfigActual();

                if (_pointerSpeed != 0)
                    _ = adbManager.AplicarPointerSpeedAsync(_pointerSpeed);

                bool exito = scrcpyManager.Lanzar(config);

                if (!exito)
                {
                    MessageBox.Show(
                        "No se pudo lanzar scrcpy.\n\nIntenta reconectar ADB e inténtalo de nuevo.",
                        "Error al iniciar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _scrcpyEstabaActivo = true;
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
                                    "OTG no pudo iniciarse. Verifica que tu cable soporte modo OTG y que el dispositivo sea compatible con esta función.",
                                    "OTG — Error de inicio", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            });
                    });
                }

                string modo = _modoOtg ? "OTG" : _usarWifi ? "WiFi" : (!_video && !_audio) ? "Control Only" : "USB";
                string info = $"{_perfilSeleccionado}  |  {_fps} FPS  |  {_bitrate} Mb  |  {modo}";

                if (_mostrarFlotante)
                {
                    _flotante = new FloatingWindow(
                        scrcpyManager, info,
                        onDetener: () =>
                        {
                            scrcpyManager.Detener();
                            // Reset de resolución al detener — igual que al cerrar el launcher
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
                        },
                        onMostrarApp: () =>
                        {
                            InvokeSeguro(() =>
                            {
                                this.WindowState = FormWindowState.Normal;
                                this.BringToFront();
                            });
                        },
                        printFps: _printFps
                    );

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
            btnIniciarScrcpy.FillColor = puedeIniciar ? accentColor : Color.FromArgb(60, 45, 80);
            btnIniciarScrcpy.ForeColor = puedeIniciar ? Color.White : Color.FromArgb(150, 150, 150);
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
                lblEstadoIndicador.ForeColor = Color.FromArgb(16, 124, 16);
                lblEstadoTexto.Text = "Dispositivo conectado";
            }
            else
            {
                lblEstadoIndicador.ForeColor = Color.FromArgb(220, 50, 50);
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
                lblEstadoIndicador.ForeColor = Color.FromArgb(16, 124, 16);
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
                lblEstadoIndicador.ForeColor = Color.FromArgb(220, 50, 50);
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

        // ══════════════════════════════════════════════════════════════
        // HELPERS
        // ══════════════════════════════════════════════════════════════

        private Guna2Button CreateNavButton(string text, int index)
        {
            var btn = new Guna2Button()
            {
                Text = text,
                Tag = text,
                Width = 200,
                Height = 44,
                Left = 10,
                Top = 90 + (index * 48),
                Font = new Font("Segoe UI", 9.5f),
                FillColor = Color.Transparent,
                ForeColor = textSecondary,
                BorderColor = Color.Transparent,
                BorderThickness = 0,
                BorderRadius = 6,
                TextAlign = HorizontalAlignment.Left,
                ButtonMode = Guna.UI2.WinForms.Enums.ButtonMode.RadioButton
            };
            btn.MouseEnter += (s, e) =>
            {
                if (!btn.Checked)
                    btn.FillColor = Color.FromArgb(50, 35, 70);
            };
            btn.MouseLeave += (s, e) =>
            {
                if (!btn.Checked) btn.FillColor = Color.Transparent;
            };
            return btn;
        }

        private Panel CreateCard(string title, int left, int top, int height = 150)
        {
            var card = new Panel()
            {
                Left = left,
                Top = top,
                Width = contentPanel.Width - 60,
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
                Left = 24,
                Top = 20,
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
                Height = 34,
                Minimum = min,
                Maximum = max,
                Value = value,
                Increment = increment,
                Font = new Font("Segoe UI", 10f)
            };
        }

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
            _fullscreenCrop = cfg.FullscreenCrop;
            _cropActivo = false; // al cargar perfil, crop no está activo hasta que se calcula
            _modoOtg = cfg.ModoOtg;
            _otgSerial = cfg.OtgSerial;
            // WiFi no se carga del perfil — el modo de conexión lo determina
            // exclusivamente el estado actual de _wifiConectado y la IP disponible.
            _resolucionAncho = cfg.ResolucionAncho;
            _resolucionAlto = cfg.ResolucionAlto;
            _aspectRatio = cfg.AspectRatio;
            _customRatioW = cfg.CustomRatioW;
            _customRatioH = cfg.CustomRatioH;
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

        private async Task ActualizarEstadoAdbAsync(Label lblEstado)
        {
            var (exito, seriales, _) = await Task.Run(() => adbManager.ListarDispositivos());
            if (lblEstado == null) return;
            if (exito && seriales.Count > 0)
            {
                lblEstado.Text = $"●  {seriales.Count} dispositivo(s): {string.Join(", ", seriales)}";
                lblEstado.ForeColor = Color.FromArgb(16, 124, 16);
            }
            else
            {
                lblEstado.Text = "●  Sin dispositivo detectado";
                lblEstado.ForeColor = Color.FromArgb(220, 50, 50);
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
            PrintFps = _printFps,
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
                lblDpiActual.ForeColor = Color.FromArgb(16, 124, 16);
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

        // ══════════════════════════════════════════════════════════════
        // PERFILES
        // ══════════════════════════════════════════════════════════════

        private void LoadPerfilesPage()
        {
            var panelIzq = new Panel()
            {
                Left = 30,
                Top = 20,
                Width = 240,
                Height = contentPanel.Height - 40,
                BackColor = bgCard,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
            };

            var panelDer = new Panel()
            {
                Left = 286,
                Top = 20,
                Width = contentPanel.Width - 316,
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
                Left = 16,
                Top = 16,
                AutoSize = true
            });

            _lstPerfiles = new ListBox()
            {
                Left = 12,
                Top = 48,
                Width = panelIzq.Width - 24,
                Height = panelIzq.Height - 140,
                BackColor = Color.FromArgb(33, 32, 35),
                ForeColor = textPrimary,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9.5f),
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 28,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            _lstPerfiles.DrawItem += (s, e) =>
            {
                if (e.Index < 0) return;
                bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                e.Graphics.FillRectangle(
                    new System.Drawing.SolidBrush(selected
                        ? Color.FromArgb(78, 28, 141)
                        : Color.FromArgb(33, 32, 35)),
                    e.Bounds);
                e.Graphics.DrawString(
                    _lstPerfiles.Items[e.Index].ToString(),
                    e.Font ?? new Font("Segoe UI", 9.5f),
                    new System.Drawing.SolidBrush(Color.FromArgb(238, 238, 238)),
                    e.Bounds.X + 8, e.Bounds.Y + 4);
            };

            var btnNuevoPerfil = new Guna2Button()
            {
                Text = "＋ Nuevo Perfil",
                Left = 12,
                Top = panelIzq.Height - 84,
                Width = panelIzq.Width - 24,
                Height = 36,
                Font = new Font("Segoe UI", 9f),
                FillColor = accentColor,
                ForeColor = Color.White,
                BorderRadius = 4,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            var btnImportar = new Guna2Button()
            {
                Text = "📂 Importar",
                Left = 12,
                Top = panelIzq.Height - 42,
                Width = (panelIzq.Width - 30) / 2,
                Height = 32,
                Font = new Font("Segoe UI", 8.5f),
                FillColor = Color.FromArgb(55, 40, 75),
                ForeColor = textSecondary,
                BorderColor = Color.FromArgb(60, 60, 60),
                BorderThickness = 1,
                BorderRadius = 4,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };

            var btnExportarLista = new Guna2Button()
            {
                Text = "💾 Exportar",
                Left = 18 + (panelIzq.Width - 30) / 2,
                Top = panelIzq.Height - 42,
                Width = (panelIzq.Width - 30) / 2,
                Height = 32,
                Font = new Font("Segoe UI", 8.5f),
                FillColor = Color.FromArgb(55, 40, 75),
                ForeColor = textSecondary,
                BorderColor = Color.FromArgb(60, 60, 60),
                BorderThickness = 1,
                BorderRadius = 4,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

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
                Left = 40,
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
                Left = 24,
                Top = 20,
                AutoSize = true
            });

            var txtNombre = new Guna2TextBox()
            {
                Left = 24,
                Top = 48,
                Width = 300,
                Height = 36,
                Text = nombre,
                Font = new Font("Segoe UI", 10f),
                FillColor = Color.FromArgb(42, 42, 45),
                ForeColor = textPrimary,
                BorderColor = Color.FromArgb(60, 60, 60),
                BorderRadius = 4
            };

            var lblNombreError = new Label()
            {
                Text = "",
                Font = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(220, 50, 50),
                Left = 24,
                Top = 88,
                AutoSize = true
            };

            _panelDetalle.Controls.Add(new Label()
            {
                Text = "Valores del Perfil",
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = textPrimary,
                Left = 24,
                Top = 106,
                AutoSize = true
            });

            var lblValores = new Label()
            {
                Text = FormatearValoresPerfil(perfil),
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = textSecondary,
                Left = 24,
                Top = 128,
                Width = _panelDetalle.Width - 48,
                Height = 160,
                AutoSize = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var btnCargar = new Guna2Button()
            {
                Text = "▶ Cargar en App",
                Left = 24,
                Top = 300,
                Width = 160,
                Height = 38,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                FillColor = accentColor,
                ForeColor = Color.White,
                BorderRadius = 4
            };

            var btnGuardar = new Guna2Button()
            {
                Text = "💾 Guardar Cambios",
                Left = 194,
                Top = 300,
                Width = 170,
                Height = 38,
                Font = new Font("Segoe UI", 9.5f),
                FillColor = Color.FromArgb(55, 40, 75),
                ForeColor = textSecondary,
                BorderColor = Color.FromArgb(60, 60, 60),
                BorderThickness = 1,
                BorderRadius = 4
            };

            var btnEliminar = new Guna2Button()
            {
                Text = "🗑 Eliminar",
                Left = 24,
                Top = 348,
                Width = 130,
                Height = 34,
                Font = new Font("Segoe UI", 9f),
                FillColor = Color.FromArgb(180, 40, 40),
                ForeColor = Color.White,
                BorderRadius = 4
            };

            var btnExportar = new Guna2Button()
            {
                Text = "📤 Exportar este Perfil",
                Left = 164,
                Top = 348,
                Width = 190,
                Height = 34,
                Font = new Font("Segoe UI", 9f),
                FillColor = Color.FromArgb(55, 40, 75),
                ForeColor = textSecondary,
                BorderColor = Color.FromArgb(60, 60, 60),
                BorderThickness = 1,
                BorderRadius = 4
            };

            var lblAccionStatus = new Label()
            {
                Text = "",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                ForeColor = Color.FromArgb(16, 124, 16),
                Left = 24,
                Top = 392,
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
                lblAccionStatus.ForeColor = Color.FromArgb(16, 124, 16);
            };

            btnGuardar.Click += (s, e) =>
            {
                string nuevoNombre = txtNombre.Text.Trim();
                string errorNombre = ValidarNombrePerfil(nuevoNombre, nombre);
                if (!string.IsNullOrEmpty(errorNombre))
                {
                    lblNombreError.Text = errorNombre;
                    txtNombre.BorderColor = Color.FromArgb(220, 50, 50);
                    return;
                }
                lblNombreError.Text = "";
                txtNombre.BorderColor = Color.FromArgb(60, 60, 60);

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
                    lblValores.Text = FormatearValoresPerfil(perfilManager.ObtenerPerfil(nombre));
                    lblAccionStatus.Text = $"✓ Perfil '{nombre}' guardado";
                    lblAccionStatus.ForeColor = Color.FromArgb(16, 124, 16);
                    LimpiarIndicadorCambios();
                }
                else
                {
                    lblAccionStatus.Text = $"✗ Error: {error}";
                    lblAccionStatus.ForeColor = Color.FromArgb(220, 50, 50);
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
                txtNombre.BorderColor = Color.FromArgb(60, 60, 60);
            };

            _panelDetalle.Controls.AddRange(new Control[]
            {
                txtNombre, lblNombreError, lblValores,
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
                FillColor = Color.FromArgb(42, 42, 45),
                ForeColor = textPrimary,
                BorderColor = Color.FromArgb(60, 60, 60),
                BorderRadius = 4,
                MaxLength = 30
            };

            var lblError = new Label()
            {
                Text = "",
                Font = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(220, 50, 50),
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
                FillColor = Color.FromArgb(55, 40, 75),
                ForeColor = textSecondary,
                BorderColor = Color.FromArgb(60, 60, 60),
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
                    txtNuevo.BorderColor = Color.FromArgb(220, 50, 50);
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

        // ══════════════════════════════════════════════════════════════
        // ACERCA DE
        // ══════════════════════════════════════════════════════════════

        private void LoadAcercaPage()
        {
            var cardMain = CreateCard("MobiladorSteX × Morrigan", 30, 20, 240);

            var picBox = new PictureBox()
            {
                Left = cardMain.Width - 130,
                Top = 48,
                Width = 100,
                Height = 100,
                BackColor = Color.FromArgb(60, 45, 80),
                BorderStyle = BorderStyle.None,
                SizeMode = PictureBoxSizeMode.Zoom,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };

            string logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logo.png");
            if (File.Exists(logoPath))
                picBox.Image = Image.FromFile(logoPath);
            else
            {
                picBox.Paint += (s, e) =>
                    e.Graphics.DrawString("logo.png", new Font("Segoe UI", 7f),
                        new SolidBrush(textSecondary),
                        new RectangleF(0, 0, picBox.Width, picBox.Height),
                        new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }

            cardMain.Controls.AddRange(new Control[]
            {
                picBox,
                new Label() { Text = $"MobiladorSteX — MORRIGAN God's Apocalypse\n{ObtenerVersionApp()}", Font = new Font("Segoe UI", 10f, FontStyle.Bold), ForeColor = accentColor, Left = 24, Top = 52, AutoSize = true },
                new Label() { Text = "Desarrollado por Dario (@nks_array)", Font = new Font("Segoe UI", 9f), ForeColor = textSecondary, Left = 24, Top = 92, AutoSize = true },
                new Label() { Text = "\"No controlas el teléfono. Controlas la distancia.\"", Font = new Font("Segoe UI", 9f, FontStyle.Italic), ForeColor = Color.FromArgb(180, 140, 220), Left = 24, Top = 116, Width = cardMain.Width - 160, AutoSize = false, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right },
                new Label() { Text = "Versión insignia de la comunidad Free Fire PC.", Font = new Font("Segoe UI", 8.5f), ForeColor = textSecondary, Left = 24, Top = 148, AutoSize = true },
                CreateBtnSocial("🎵  TikTok — @nks_array", 24, 180, 210, Color.FromArgb(55, 40, 75), "https://www.tiktok.com/@nks_array"),
                CreateBtnSocial("💬  Discord — Unirse", 244, 180, 185, Color.FromArgb(78, 28, 141), "https://discord.gg/CU5quVNyun")
            });

            var cardCreditos = CreateCard("Créditos", 30, 280, 140);
            cardCreditos.Controls.Add(new Label()
            {
                Text = "scrcpy — Genymobile  |  Licencia Apache 2.0\n" +
                       "Guna UI2 — Guna Systems  |  Librería de controles WinForms\n" +
                       "ini-parser — Ricardo Amores  |  MIT License",
                Font = new Font("Segoe UI", 9f),
                ForeColor = textSecondary,
                Left = 24,
                Top = 52,
                AutoSize = true
            });

            var cardProyecto = CreateCard("Acerca del Proyecto", 30, 440, 200);
            cardProyecto.Controls.Add(new Label()
            {
                Text = "MobiladorSteX está diseñado para llevar la experiencia móvil a PC con la mayor fluidez\n" +
                       "y calidad posible, enfocado en usuarios que buscan control, precisión y estabilidad.\n\n" +
                       "Esta edición especial introduce una identidad visual inspirada en Morrigan, integrando\n" +
                       "una estética distintiva con una experiencia optimizada, sin perder el enfoque en\n" +
                       "rendimiento y claridad.\n\n" +
                       "Más que una herramienta, es una forma de transformar cómo ves y controlas tu dispositivo.",
                Font = new Font("Segoe UI", 9f),
                ForeColor = textSecondary,
                Left = 24,
                Top = 50,
                AutoSize = true
            });

            contentPanel.Controls.AddRange(new Control[] { cardMain, cardCreditos, cardProyecto });
        }

        private Guna2Button CreateBtnSocial(string text, int left, int top, int width, Color fill, string url)
        {
            var btn = new Guna2Button()
            {
                Text = text,
                Width = width,
                Height = 38,
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