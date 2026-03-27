using Guna.UI2.WinForms;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System;

namespace MobiladorStex
{
    public partial class Form1
    {
        private void LoadPantallaPage()
        {
            _cargandoPagina = true;
            try
            {
                // Forward-declarations necesarias por ActualizarEstados() — deben estar
                // asignadas antes de que se registre cualquier delegate que la llame.
                Guna2Button btnAplicarRes = null!;
                Guna2Button btnResetearRes = null!;
                Guna2Button btnAplicarWm = null!;
                Guna2Button btnRevertirWm = null!;
                Guna2Button btnRestablecerCrop = null!;
                Label lblAdbConflicto = null!;
                Guna2Button? btnCalcularCropRef = null;

                // ── CARD: Fullscreen ──────────────────────────────────────
                var cardFullscreen = CreateCard("Pantalla Completa", S(30), S(20), S(100));

                var togFullscreen = new Guna2ToggleSwitch()
                {
                    Left = cardFullscreen.Width - S(70),
                    Top = S(48),
                    Checked = _fullscreen,
                    CheckedState = { FillColor = accentColor },
                    UncheckedState = { FillColor = Color.FromArgb(60, 60, 60) },
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                togFullscreen.CheckedChanged += (s, e) => { _fullscreen = togFullscreen.Checked; if (!_cargandoPagina) MarcarCambiosSinGuardar(); };

                cardFullscreen.Controls.AddRange(new Control[]
                {
                new Label() { Text = "Activar Pantalla Completa", Font = new Font("Segoe UI", 10f), ForeColor = textPrimary, Left = S(24), Top = S(50), AutoSize = true },
                new Label() { Text = "Usa MOD+F para salir", Font = new Font("Segoe UI", 8f), ForeColor = textSecondary, Left = S(24), Top = S(72), AutoSize = true },
                togFullscreen
                });

                // ── CARD: Resolución del Dispositivo ─────────────────────
                // ── CARD: Window Size ────────────────────────────────────
                var cardWindowSize = CreateCard("Tamaño de Ventana", S(30), S(140), S(120));

                var numWindowW = CreateNumeric(S(160), S(64), S(100), 0, 3840, _windowWidth, 10);
                var numWindowH = CreateNumeric(S(290), S(64), S(100), 0, 2160, _windowHeight, 10);
                numWindowW.ValueChanged += (s, e) => { _windowWidth = (int)numWindowW.Value; if (!_cargandoPagina) MarcarCambiosSinGuardar(); };
                numWindowW.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; this.ActiveControl = null; } };
                numWindowW.Leave += (s, e) => { _windowWidth = (int)numWindowW.Value; if (!_cargandoPagina) MarcarCambiosSinGuardar(); };
                numWindowH.ValueChanged += (s, e) => { _windowHeight = (int)numWindowH.Value; if (!_cargandoPagina) MarcarCambiosSinGuardar(); };
                numWindowH.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; this.ActiveControl = null; } };
                numWindowH.Leave += (s, e) => { _windowHeight = (int)numWindowH.Value; if (!_cargandoPagina) MarcarCambiosSinGuardar(); };

                cardWindowSize.Controls.AddRange(new Control[]
                {
                new Label() { Text = "0 = automático", Font = new Font("Segoe UI", 8f), ForeColor = textSecondary, Left = S(24), Top = S(42), AutoSize = true },
                numWindowW,
                new Label() { Text = "×", Font = new Font("Segoe UI", 10f), ForeColor = textSecondary, Left = S(270), Top = S(68), AutoSize = true },
                numWindowH
                });

                var cardResolucion = CreateCard("Resolución Nativa del Dispositivo", S(30), S(280), S(140));

                // Resolución como label solo lectura — se detecta automáticamente
                var lblResAncho = new Label()
                {
                    Text = _resolucionAncho.ToString(),
                    Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(107, 47, 196),
                    Left = S(160),
                    Top = S(60),
                    AutoSize = true
                };
                var lblResAlto = new Label()
                {
                    Text = _resolucionAlto.ToString(),
                    Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(107, 47, 196),
                    Left = S(290),
                    Top = S(60),
                    AutoSize = true
                };

                var lblResStatus = new Label()
                {
                    Text = "Presiona 🔄 para detectar automáticamente",
                    Font = new Font("Segoe UI", 8f),
                    ForeColor = textSecondary,
                    Left = S(24),
                    Top = S(100),
                    AutoSize = true
                };

                var btnDetectarRes = new Guna2Button()
                {
                    Text = "🔄",
                    Width = S(36),
                    Height = S(34),
                    Left = S(400),
                    Top = S(58),
                    Font = new Font("Segoe UI", 11f),
                    FillColor = Color.FromArgb(107, 47, 196),
                    ForeColor = Color.White,
                    BorderRadius = 4
                };
                btnDetectarRes.Click += async (s, e) =>
                {
                    btnDetectarRes.Enabled = false;
                    lblResStatus.Text = "Detectando...";
                    lblResStatus.ForeColor = Color.FromArgb(255, 167, 38);
                    try
                    {
                        var (exito, ancho, alto, mensaje) = await adbManager.DetectarResolucionAsync();
                        if (IsDisposed) return;
                        if (exito)
                        {
                            _resolucionAncho = ancho; _resolucionAlto = alto;
                            lblResAncho.Text = ancho.ToString();
                            lblResAlto.Text = alto.ToString();
                            lblResStatus.Text = $"✓ {mensaje}";
                            lblResStatus.ForeColor = Color.FromArgb(16, 124, 16);
                            ActualizarEstados();
                            MarcarCambiosSinGuardar();
                        }
                        else
                        {
                            lblResStatus.Text = $"⚠ {mensaje}";
                            lblResStatus.ForeColor = Color.FromArgb(255, 167, 38);
                        }
                    }
                    finally
                    {
                        if (!IsDisposed) btnDetectarRes.Enabled = true;
                    }
                };

                cardResolucion.Controls.AddRange(new Control[]
                {
                new Label() { Text = "Ancho:", Font = new Font("Segoe UI", 9.5f), ForeColor = textPrimary, Left = S(24), Top = S(64), AutoSize = true },
                lblResAncho,
                new Label() { Text = "×", Font = new Font("Segoe UI", 10f), ForeColor = textSecondary, Left = S(270), Top = S(66), AutoSize = true },
                lblResAlto, btnDetectarRes, lblResStatus
                });

                // ── CARD: Crop ────────────────────────────────────────────
                var cardCrop = CreateCard("Recorte de Imagen — Opción 1 (Recomendado)", S(30), S(440), S(254));

                var lblCropDesc = new Label()
                {
                    Text = "Elimina bordes recortando parte del contenido del teléfono. Es normal que se pierda\n" +
                           "algo de imagen — no hay forma de eliminar bordes sin algún compromiso visual.\n" +
                           "Si el resultado no es satisfactorio, prueba la Opción 3.",
                    Font = new Font("Segoe UI", 8f),
                    ForeColor = textSecondary,
                    Left = S(24),
                    Top = S(48),
                    Width = cardCrop.Width - S(48),
                    AutoSize = false,
                    Height = S(46),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };

                var lblCropConflicto = new Label()
                {
                    Text = "⚠ Resolución ADB activa. Resetea la resolución para usar crop.",
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(255, 167, 38),
                    Left = S(24),
                    Top = S(100),
                    Width = cardCrop.Width - S(48),
                    AutoSize = false,
                    Visible = false,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };

                var cmbAspect = new Guna2ComboBox()
                {
                    Left = S(160),
                    Top = S(118),
                    Width = S(130),
                    Height = S(32),
                    FillColor = Color.FromArgb(42, 42, 45),
                    ForeColor = textPrimary,
                    BorderColor = Color.FromArgb(107, 47, 196),
                    BorderRadius = 4,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = new Font("Segoe UI", 9f)
                };
                foreach (var op in new[] { "16:9", "16:10", "21:9", "18:9", "4:3", "Personalizado" })
                    cmbAspect.Items.Add(op);
                cmbAspect.SelectedItem = _aspectRatio;
                if (cmbAspect.SelectedIndex < 0) cmbAspect.SelectedIndex = 0;

                var numCustomW = CreateNumeric(S(300), S(118), S(60), 1, 99, _customRatioW, 1);
                var lblCustomX = new Label() { Text = ":", Font = new Font("Segoe UI", 10f), ForeColor = textSecondary, Left = S(364), Top = S(124), AutoSize = true };
                var numCustomH = CreateNumeric(S(374), S(118), S(60), 1, 99, _customRatioH, 1);

                numCustomW.ValueChanged += (s, e) => { _customRatioW = (int)numCustomW.Value; if (!_cargandoPagina) MarcarCambiosSinGuardar(); };
                numCustomW.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        e.SuppressKeyPress = true;
                        this.ActiveControl = null;
                    }
                };
                numCustomW.Leave += (s, e) => { _customRatioW = (int)numCustomW.Value; if (!_cargandoPagina) MarcarCambiosSinGuardar(); };
                numCustomH.ValueChanged += (s, e) => { _customRatioH = (int)numCustomH.Value; if (!_cargandoPagina) MarcarCambiosSinGuardar(); };
                numCustomH.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        e.SuppressKeyPress = true;
                        this.ActiveControl = null;
                    }
                };
                numCustomH.Leave += (s, e) => { _customRatioH = (int)numCustomH.Value; if (!_cargandoPagina) MarcarCambiosSinGuardar(); };

                numCustomW.Visible = lblCustomX.Visible = numCustomH.Visible = (_aspectRatio == "Personalizado");

                cmbAspect.SelectedIndexChanged += (s, e) =>
                {
                    _aspectRatio = cmbAspect.SelectedItem?.ToString() ?? "16:9";
                    bool custom = _aspectRatio == "Personalizado";
                    numCustomW.Visible = lblCustomX.Visible = numCustomH.Visible = custom;
                    if (!_cargandoPagina) MarcarCambiosSinGuardar();
                };

                var lblCropAplicado = new Label()
                {
                    Text = _cropActivo ? $"✓ Crop activo: {_fullscreenCrop}"
                         : !string.IsNullOrEmpty(_fullscreenCrop) ? $"Crop en perfil: {_fullscreenCrop} (no activo)"
                         : "Sin crop aplicado",
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                    ForeColor = _cropActivo ? Color.FromArgb(16, 124, 16) : textSecondary,
                    Left = S(24),
                    Top = S(166),
                    AutoSize = true
                };

                // ── Función central de estados ────────────────────────────
                // Lee los flags y configura TODOS los botones de una vez
                void ActualizarEstados()
                {
                    bool cropActivo = _cropActivo;
                    bool adbActivo = _resAdbActiva;
                    bool wmActivo = _wmSizeActivo;
                    bool hayDev = _hayDispositivo;
                    bool resValida = _resolucionAncho > 0 && _resolucionAlto > 0;

                    // Solo una opción activa a la vez.
                    // Además, las acciones destructivas requieren dispositivo conectado
                    // y resolución nativa detectada (> 0) para no enviar comandos inválidos.

                    // Botones Crop — requieren resolución válida para calcular el offset
                    if (btnCalcularCropRef != null) btnCalcularCropRef.Enabled = hayDev && resValida && !adbActivo && !wmActivo && !cropActivo;
                    if (btnRestablecerCrop != null) btnRestablecerCrop.Enabled = cropActivo;

                    // Botones ADB — requieren resolución válida (usan _resolucionAncho/_Alto)
                    if (btnAplicarRes != null) btnAplicarRes.Enabled = hayDev && resValida && !cropActivo && !wmActivo && !adbActivo;
                    if (btnResetearRes != null) btnResetearRes.Enabled = hayDev && adbActivo;

                    // Botones WmSize — el campo es texto libre, no depende de resolución
                    if (btnAplicarWm != null) btnAplicarWm.Enabled = hayDev && !cropActivo && !adbActivo && !wmActivo;
                    if (btnRevertirWm != null) btnRevertirWm.Enabled = wmActivo;

                    // Aviso visual
                    if (lblAdbConflicto != null) lblAdbConflicto.Visible = cropActivo;
                }

                var btnCalcularCrop = new Guna2Button()
                {
                    Text = "Calcular Crop",
                    Width = S(140),
                    Height = S(36),
                    Left = S(24),
                    Top = S(190),
                    Font = new Font("Segoe UI", 9f),
                    FillColor = accentColor,
                    ForeColor = Color.White,
                    BorderRadius = 4
                };
                btnCalcularCropRef = btnCalcularCrop;

                btnRestablecerCrop = new Guna2Button()
                {
                    Text = "Restablecer",
                    Width = S(110),
                    Height = S(36),
                    Left = S(174),
                    Top = S(190),
                    Font = new Font("Segoe UI", 9f),
                    FillColor = accentColor,
                    ForeColor = Color.White,
                    BorderRadius = 4
                };

                btnCalcularCrop.Click += (s, e) =>
                {
                    if (_resolucionAncho == 0 || _resolucionAlto == 0)
                    {
                        MessageBox.Show(this, "Detecta la resolución nativa primero (presiona 🔄).", "Resolución no detectada", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    double ratio = ObtenerAspectRatioValor();
                    if (ratio <= 0) return;
                    int altoIdeal = (int)(_resolucionAncho * ratio);
                    if (_resolucionAlto > altoIdeal)
                    {
                        int offset = (_resolucionAlto - altoIdeal) / 2;
                        _fullscreenCrop = $"{_resolucionAncho}:{altoIdeal}:0:{offset}";
                        _cropActivo = true;
                        lblCropAplicado.Text = $"✓ Crop activo: {_fullscreenCrop}";
                        lblCropAplicado.ForeColor = Color.FromArgb(16, 124, 16);
                        ActualizarEstados();
                        MarcarCambiosSinGuardar();
                        MessageBox.Show($"Crop calculado para {_aspectRatio}:\n{_fullscreenCrop}\n\nAlto ideal: {altoIdeal}px  |  Offset: {offset}px", "Crop Calculado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        _fullscreenCrop = "";
                        lblCropAplicado.Text = $"Tu resolución ya es {_aspectRatio}, no se necesita crop";
                        lblCropAplicado.ForeColor = textSecondary;
                        MarcarCambiosSinGuardar();
                    }
                };

                btnRestablecerCrop.Click += (s, e) =>
                {
                    _fullscreenCrop = "";
                    _cropActivo = false;
                    lblCropAplicado.Text = "Sin crop aplicado";
                    lblCropAplicado.ForeColor = textSecondary;
                    ActualizarEstados();
                    if (btnAplicarRes != null) btnAplicarRes.FillColor = accentColor;
                    MarcarCambiosSinGuardar();
                };

                cardCrop.Controls.AddRange(new Control[]
                {
                lblCropDesc, lblCropConflicto,
                new Label() { Text = "Aspect Ratio:", Font = new Font("Segoe UI", 9.5f), ForeColor = textPrimary, Left = S(24), Top = S(124), AutoSize = true },
                cmbAspect, numCustomW, lblCustomX, numCustomH,
                lblCropAplicado, btnCalcularCrop, btnRestablecerCrop
                });

                // ── CARD: Resolución ADB ──────────────────────────────────
                var cardAdb = CreateCard("Modificar Resolución ADB — Opción 2 (Avanzado)", S(30), S(700), S(202));

                var lblAdbAdvertencia = new Label()
                {
                    Text = "Modifica cómo el teléfono renderiza la imagen. Puede mejorar la experiencia en algunos " +
                           "dispositivos, pero en Qualcomm/Snapdragon puede causar caídas de FPS y latencia. " +
                           "En ciertos modelos el fabricante puede bloquear este comando vía ADB. " +
                           "Si experimentas problemas, desactívalo — los bordes son preferibles a un mal rendimiento.",
                    Font = new Font("Segoe UI", 8f),
                    ForeColor = Color.FromArgb(120, 120, 120),
                    Left = S(24),
                    Top = S(40),
                    Width = cardAdb.Width - S(48),
                    AutoSize = false,
                    Height = S(54),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };

                lblAdbConflicto = new Label()
                {
                    Text = "⚠ Tienes un crop activo. Restablece el crop para usar esta opción.",
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(255, 167, 38),
                    Left = S(24),
                    Top = S(100),
                    Width = cardAdb.Width - S(48),
                    AutoSize = false,
                    Visible = !string.IsNullOrEmpty(_fullscreenCrop),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };

                var lblAdbStatus = new Label()
                {
                    Text = "",
                    Font = new Font("Segoe UI", 8f, FontStyle.Italic),
                    ForeColor = textSecondary,
                    Left = S(24),
                    Top = S(116),
                    AutoSize = true
                };

                btnAplicarRes = new Guna2Button()
                {
                    Text = "Aplicar Resolución",
                    Width = S(180),
                    Height = S(36),
                    Left = S(24),
                    Top = S(146),
                    Font = new Font("Segoe UI", 9f),
                    FillColor = accentColor,
                    ForeColor = Color.White,
                    BorderRadius = 4
                };

                btnResetearRes = new Guna2Button()
                {
                    Text = "Resetear",
                    Width = S(100),
                    Height = S(36),
                    Left = S(214),
                    Top = S(146),
                    Font = new Font("Segoe UI", 9f),
                    FillColor = accentColor,
                    ForeColor = Color.White,
                    BorderRadius = 4
                };

                btnAplicarRes.Click += async (s, e) =>
                {
                    if (!_avisoAdbVisto)
                    {
                        using var dlg = new DialogoAvanzado(
                            "Resolución ADB — Función Avanzada",
                            "Esta opción modifica la resolución física del dispositivo vía ADB.\n\n" +
                            "El comportamiento puede variar según el dispositivo y la capa del sistema.\n" +
                            "Algunos fabricantes (Xiaomi, Sony, Oppo) pueden bloquear este comando o\n" +
                            "requerir permisos adicionales. En chips Qualcomm/Snapdragon puede causar\n" +
                            "caída de FPS y aumento de latencia. Si algo queda distorsionado,\n" +
                            "usa el botón Resetear para restaurar.",
                            new[] {
                            "Entiendo que puede afectar el rendimiento de mi dispositivo",
                            "Sé cómo usar el botón Resetear si algo sale mal",
                            "Usaré conexión USB al probar por primera vez"
                            });
                        if (dlg.ShowDialog(this) != DialogResult.OK) return;
                        if (dlg.NoVolverMostrar)
                        {
                            _avisoAdbVisto = true;
                            GuardarConfigTema();
                        }
                    }
                    double ratio = ObtenerAspectRatioValor();
                    if (ratio <= 0) return;
                    if (_resolucionAncho == 0 || _resolucionAlto == 0)
                    {
                        lblAdbStatus.Text = "⚠ Resolución sin detectar — presiona 🔄 primero";
                        lblAdbStatus.ForeColor = Color.FromArgb(220, 50, 50);
                        return;
                    }
                    btnAplicarRes.Enabled = false;
                    lblAdbStatus.Text = "Aplicando...";
                    bool completadoAplicar = false;
                    try
                    {
                        var (exito, wmSize, error) = await adbManager.AplicarResolucionAsync(_resolucionAncho, _resolucionAlto, ratio);
                        completadoAplicar = true;
                        if (IsDisposed) return;
                        if (exito)
                        {
                            lblAdbStatus.Text = $"✓ Resolución aplicada: {wmSize}";
                            _resAdbActiva = true;
                            lblAdbStatus.ForeColor = Color.FromArgb(16, 124, 16);
                            ActualizarEstados();
                            lblCropConflicto.Visible = true;
                            MarcarCambiosSinGuardar();
                        }
                        else
                        {
                            // Detectar error de permisos — dispositivo no compatible
                            string mensajeError = error.Contains("WRITE_SECURE_SETTINGS") || error.Contains("SecurityException")
                                ? "⚠ Tu dispositivo no permite cambiar la resolución vía ADB.\nEsta función no es compatible con todos los modelos."
                                : $"✗ Error: {error}";
                            lblAdbStatus.Text = mensajeError;
                            lblAdbStatus.ForeColor = Color.FromArgb(220, 50, 50);
                            btnAplicarRes.Enabled = true;
                        }
                    }
                    finally
                    {
                        if (!completadoAplicar && !IsDisposed) btnAplicarRes.Enabled = true;
                    }
                };

                btnResetearRes.Click += async (s, e) =>
                {
                    btnResetearRes.Enabled = false;
                    bool completadoReset = false;
                    try
                    {
                        var (exito, error) = await adbManager.ResetearResolucionAsync();
                        completadoReset = true;
                        if (IsDisposed) return;
                        lblAdbStatus.Text = exito ? "✓ Resolución restaurada" : $"✗ {error}";
                        lblAdbStatus.ForeColor = exito ? Color.FromArgb(16, 124, 16) : Color.FromArgb(220, 50, 50);
                        if (exito)
                        {
                            _resAdbActiva = false;
                            ActualizarEstados();
                            if (btnAplicarRes != null) btnAplicarRes.FillColor = accentColor;
                            MarcarCambiosSinGuardar();
                        }
                        else
                        {
                            btnResetearRes.Enabled = true;
                        }
                    }
                    finally
                    {
                        if (!completadoReset && !IsDisposed) btnResetearRes.Enabled = true;
                    }
                };

                cardAdb.Controls.AddRange(new Control[]
                {
                lblAdbAdvertencia,
                lblAdbConflicto,
                lblAdbStatus, btnAplicarRes, btnResetearRes
                });


                // ── CARD: Resolución Personalizada wm size ────────────────
                var cardWmSize = CreateCard("Resolución Personalizada — Opción 3 (Recomendado)", S(30), S(960), S(218));

                var lblWmSizeInfo = new Label()
                {
                    Text = "Fuerza una resolución personalizada directamente en el dispositivo via ADB. " +
                           "En algunos modelos puede causar latencia, caída de FPS o desalineación del cursor. " +
                           "El fabricante puede bloquear este comando en ciertos dispositivos. " +
                           "Si experimentas problemas, usa Revertir. Se restaura automáticamente al detener la app.",
                    Font = new Font("Segoe UI", 8f),
                    ForeColor = textSecondary,
                    Left = S(24),
                    Top = S(40),
                    Width = cardWmSize.Width - S(48),
                    AutoSize = false,
                    Height = S(54),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };

                var txtWmSize = new Guna2TextBox()
                {
                    Left = S(24),
                    Top = S(108),
                    Width = S(180),
                    Height = S(34),
                    Text = _wmSizeValor,
                    PlaceholderText = "ej. 1280x720",
                    Font = new Font("Segoe UI", 10f),
                    FillColor = Color.FromArgb(42, 42, 45),
                    ForeColor = Color.FromArgb(238, 238, 238),
                    BorderColor = Color.FromArgb(107, 47, 196),
                    BorderRadius = 4
                };
                txtWmSize.TextChanged += (s, e) =>
                {
                    _wmSizeValor = txtWmSize.Text;
                    if (!_cargandoPagina) MarcarCambiosSinGuardar();
                };

                var lblWmSizeStatus = new Label()
                {
                    Text = _wmSizeActivo ? "✓ Activo" : "",
                    Font = new Font("Segoe UI", 8f, FontStyle.Italic),
                    ForeColor = _wmSizeActivo ? Color.FromArgb(16, 124, 16) : textSecondary,
                    Left = S(214),
                    Top = S(98),
                    AutoSize = true
                };

                btnAplicarWm = new Guna2Button()
                {
                    Text = "Aplicar",
                    Width = S(100),
                    Height = S(34),
                    Left = S(24),
                    Top = S(156),
                    Font = new Font("Segoe UI", 9f),
                    FillColor = accentColor,
                    ForeColor = Color.White,
                    BorderRadius = 4
                };

                btnRevertirWm = new Guna2Button()
                {
                    Text = "Revertir",
                    Width = S(100),
                    Height = S(34),
                    Left = S(134),
                    Top = S(156),
                    Font = new Font("Segoe UI", 9f),
                    FillColor = accentColor,
                    ForeColor = Color.White,
                    BorderRadius = 4
                };

                btnAplicarWm.Click += async (s, e) =>
                {
                    string valor = txtWmSize.Text.Trim();
                    if (string.IsNullOrEmpty(valor) || !System.Text.RegularExpressions.Regex.IsMatch(valor, @"^\d+x\d+$"))
                    {
                        lblWmSizeStatus.Text = "⚠ Formato inválido. Usa: 1280x720";
                        lblWmSizeStatus.ForeColor = Color.FromArgb(220, 50, 50);
                        return;
                    }
                    if (!_avisoWmSizeVisto)
                    {
                        using var dlg = new DialogoAvanzado(
                            "Resolución Personalizada — Función Avanzada",
                            "Esta opción fuerza una resolución personalizada en el dispositivo vía ADB.\n\n" +
                            "El comportamiento puede variar según el dispositivo y la capa del sistema.\n" +
                            "Puede causar desalineación del toque/clic respecto a los elementos en pantalla.\n" +
                            "Algunos fabricantes pueden bloquear este comando. Los cambios se revierten\n" +
                            "automáticamente al detener o cerrar la app correctamente.",
                            new[] {
                            "Entiendo que el toque puede desalinearse en algunos dispositivos",
                            "Sé cómo usar el botón Revertir si algo sale mal",
                            "Usaré conexión USB al probar por primera vez"
                            });
                        if (dlg.ShowDialog(this) != DialogResult.OK) return;
                        if (dlg.NoVolverMostrar)
                        {
                            _avisoWmSizeVisto = true;
                            GuardarConfigTema();
                        }
                    }
                    btnAplicarWm.Enabled = false;
                    lblWmSizeStatus.Text = "Aplicando...";
                    lblWmSizeStatus.ForeColor = textSecondary;
                    bool completadoWm = false;
                    try
                    {
                        var (exito, error) = await adbManager.AplicarWmSizePersonalizadaAsync(valor);
                        completadoWm = true;
                        if (IsDisposed) return;
                        if (exito)
                        {
                            _wmSizeActivo = true;
                            _wmSizeValor = valor;
                            _fullscreenCrop = "";
                            _resAdbActiva = false;
                            ActualizarEstados();
                            lblWmSizeStatus.Text = "✓ Aplicado";
                            lblWmSizeStatus.ForeColor = Color.FromArgb(16, 124, 16);
                            MarcarCambiosSinGuardar();
                        }
                        else
                        {
                            lblWmSizeStatus.Text = $"✗ {(string.IsNullOrEmpty(error) ? "Sin dispositivo conectado" : error)}";
                            lblWmSizeStatus.ForeColor = Color.FromArgb(220, 50, 50);
                            btnAplicarWm.Enabled = true;
                        }
                    }
                    finally
                    {
                        if (!completadoWm && !IsDisposed) btnAplicarWm.Enabled = true;
                    }
                };

                btnRevertirWm.Click += async (s, e) =>
                {
                    btnRevertirWm.Enabled = false;
                    bool completadoRevertir = false;
                    try
                    {
                        var (exito, error) = await adbManager.ResetearResolucionAsync();
                        completadoRevertir = true;
                        if (IsDisposed) return;
                        _wmSizeActivo = false;
                        lblWmSizeStatus.Text = exito ? "✓ Revertido" : "Guardado — sin dispositivo";
                        lblWmSizeStatus.ForeColor = exito ? textSecondary : Color.FromArgb(255, 167, 38);
                        ActualizarEstados();
                        if (btnAplicarRes != null) btnAplicarRes.FillColor = accentColor;
                        MarcarCambiosSinGuardar();
                    }
                    finally
                    {
                        if (!completadoRevertir && !IsDisposed) ActualizarEstados();
                    }
                };

                cardWmSize.Controls.AddRange(new Control[]
                {
                lblWmSizeInfo, txtWmSize, lblWmSizeStatus, btnAplicarWm, btnRevertirWm
                });

                // ── CARD: DPI ─────────────────────────────────────────────
                var cardDpi = CreateCard("Control de DPI — Opción 4 (Avanzado)", S(30), S(1198), S(234));

                var lblDpiActual = new Label()
                {
                    Text = "DPI actual: Detectando...",
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = textSecondary,
                    Left = S(24),
                    Top = S(56),
                    AutoSize = true
                };

                var btnDetectarDpi = new Guna2Button()
                {
                    Text = "🔄",
                    Width = S(36),
                    Height = S(32),
                    Left = S(220),
                    Top = S(54),
                    Font = new Font("Segoe UI", 11f),
                    FillColor = Color.FromArgb(107, 47, 196),
                    ForeColor = Color.White,
                    BorderRadius = 4
                };

                var numDpi = CreateNumeric(S(160), S(100), S(100), 120, 800, _dpi, 10);
                numDpi.ValueChanged += (s, e) => { _dpi = (int)numDpi.Value; };
                numDpi.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        e.SuppressKeyPress = true;
                        this.ActiveControl = null;
                    }
                };
                numDpi.Leave += (s, e) => { _dpi = (int)numDpi.Value; };

                var lblDpiStatus = new Label()
                {
                    Text = "",
                    Font = new Font("Segoe UI", 8f, FontStyle.Italic),
                    ForeColor = textSecondary,
                    Left = S(24),
                    Top = S(148),
                    AutoSize = true
                };

                btnDetectarDpi.Click += async (s, e) =>
                {
                    btnDetectarDpi.Enabled = false;
                    try
                    {
                        var (exito, dpi, _) = await adbManager.DetectarDPIAsync();
                        if (IsDisposed) return;
                        if (exito) { _dpi = dpi; numDpi.Value = dpi; lblDpiActual.Text = $"DPI actual: {dpi}"; lblDpiActual.ForeColor = Color.FromArgb(16, 124, 16); }
                        else { lblDpiActual.Text = "DPI actual: No detectado"; lblDpiActual.ForeColor = Color.FromArgb(255, 167, 38); }
                    }
                    finally
                    {
                        if (!IsDisposed) btnDetectarDpi.Enabled = true;
                    }
                };

                var btnAplicarDpi = new Guna2Button()
                {
                    Text = "Aplicar DPI",
                    Width = S(140),
                    Height = S(36),
                    Left = S(24),
                    Top = S(172),
                    Font = new Font("Segoe UI", 9f),
                    FillColor = accentColor,
                    ForeColor = Color.White,
                    BorderRadius = 4
                };
                btnAplicarDpi.Click += async (s, e) =>
                {
                    if (MessageBox.Show($"¿Aplicar DPI {_dpi}?\n\nUsa 'Resetear' si algo sale mal.", "⚠ Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                    btnAplicarDpi.Enabled = false;
                    try
                    {
                        var (exito, mensaje, _) = await adbManager.AplicarDPIAsync(_dpi);
                        if (IsDisposed) return;
                        lblDpiStatus.Text = exito ? $"✓ {mensaje}" : $"✗ {mensaje}";
                        lblDpiStatus.ForeColor = exito ? Color.FromArgb(16, 124, 16) : Color.FromArgb(220, 50, 50);
                        if (exito) { _ultimoDpiAplicado = _dpi; GuardarConfigTema(); }
                    }
                    finally
                    {
                        if (!IsDisposed) btnAplicarDpi.Enabled = true;
                    }
                };

                var btnResetearDpi = new Guna2Button()
                {
                    Text = "Resetear DPI",
                    Width = S(120),
                    Height = S(36),
                    Left = S(174),
                    Top = S(172),
                    Font = new Font("Segoe UI", 9f),
                    FillColor = accentColor,
                    ForeColor = Color.White,
                    BorderRadius = 4
                };
                btnResetearDpi.Click += async (s, e) =>
                {
                    btnResetearDpi.Enabled = false;
                    try
                    {
                        var (exito, mensaje, _) = await adbManager.ResetearDPIAsync();
                        if (IsDisposed) return;
                        lblDpiStatus.Text = exito ? $"✓ {mensaje}" : $"✗ {mensaje}";
                        lblDpiStatus.ForeColor = exito ? Color.FromArgb(16, 124, 16) : Color.FromArgb(220, 50, 50);
                        if (exito) { var (eD, dpi, _2) = await adbManager.DetectarDPIAsync(); if (!IsDisposed && eD) { _dpi = dpi; numDpi.Value = dpi; lblDpiActual.Text = $"DPI actual: {dpi}"; } }
                    }
                    finally
                    {
                        if (!IsDisposed) btnResetearDpi.Enabled = true;
                    }
                };

                cardDpi.Controls.AddRange(new Control[]
                {
                lblDpiActual, btnDetectarDpi,
                new Label() { Text = "Nuevo DPI (120-800):", Font = new Font("Segoe UI", 9.5f), ForeColor = textPrimary, Left = S(24), Top = S(106), AutoSize = true },
                numDpi, lblDpiStatus, btnAplicarDpi, btnResetearDpi
                });

                if (_ultimoDpiAplicado > 0)
                    cardDpi.Controls.Add(new Label()
                    {
                        Text = $"Último DPI aplicado: {_ultimoDpiAplicado}",
                        Font = new Font("Segoe UI", 7.5f, FontStyle.Italic),
                        ForeColor = Color.FromArgb(107, 47, 196),
                        Left = S(24),
                        Top = S(212),
                        AutoSize = true
                    });

                contentPanel.Controls.AddRange(new Control[]
                {
                cardFullscreen, cardWindowSize, cardResolucion, cardCrop, cardAdb, cardWmSize, cardDpi
                });

                _ = DetectarDpiAlCargarAsync(lblDpiActual, numDpi);

                // Auto-detectar resolución nativa si hay dispositivo y aún no se conoce.
                // Se hace inline (no en método separado) para tener acceso a ActualizarEstados().
                if (_hayDispositivo && _resolucionAncho == 0)
                {
                    lblResStatus.Text = "⏳ Detectando...";
                    lblResStatus.ForeColor = Color.FromArgb(255, 167, 38);
                    btnDetectarRes.Enabled = false;

                    async Task DetectarResolucionAlCargar()
                    {
                        var (exito, ancho, alto, mensaje) = await adbManager.DetectarResolucionAsync();
                        if (lblResAncho.IsDisposed) return;
                        if (exito)
                        {
                            _resolucionAncho = ancho; _resolucionAlto = alto;
                            lblResAncho.Text = ancho.ToString();
                            lblResAlto.Text = alto.ToString();
                            lblResStatus.Text = $"✓ {mensaje}";
                            lblResStatus.ForeColor = Color.FromArgb(16, 124, 16);
                            ActualizarEstados();
                        }
                        else
                        {
                            lblResStatus.Text = "⚠ No detectado — conecta el cable y presiona 🔄";
                            lblResStatus.ForeColor = Color.FromArgb(255, 167, 38);
                        }
                        if (!btnDetectarRes.IsDisposed) btnDetectarRes.Enabled = true;
                    }
                    _ = DetectarResolucionAlCargar();
                }

                // Aplicar estado inicial de todos los botones según flags actuales
                ActualizarEstados();
                if (btnAplicarRes != null) btnAplicarRes.FillColor = Color.FromArgb(180, 80, 0);

            }
            finally { _cargandoPagina = false; }
        }
    }
}