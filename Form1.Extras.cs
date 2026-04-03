using Guna.UI2.WinForms;
using LyXel.Helpers;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System;

namespace LyXel
{
    public partial class Form1
    {
        private void LoadExtrasPage()
        {
            _cargandoPagina = true;
            try
            {

                // Card de comportamiento del dispositivo (screensaver, stay awake, screen off)
                var cardComport = CreateCard("Comportamiento del Dispositivo", S(30), S(20), S(220));

                var togScreensaver = new Guna2ToggleSwitch()
                {
                    Left = cardComport.Width - S(70),
                    Top = S(58),
                    Checked = _disableScreensaver,
                    CheckedState = { FillColor = accentColor },
                    UncheckedState = { FillColor = AppTheme.BorderNeutral },
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                togScreensaver.CheckedChanged += (s, e) => { _disableScreensaver = togScreensaver.Checked; if (!_cargandoPagina) MarcarCambiosSinGuardar(); };

                var togStayAwake = new Guna2ToggleSwitch()
                {
                    Left = cardComport.Width - S(70),
                    Top = S(108),
                    Checked = _stayAwake,
                    CheckedState = { FillColor = accentColor },
                    UncheckedState = { FillColor = AppTheme.BorderNeutral },
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                togStayAwake.CheckedChanged += (s, e) => { _stayAwake = togStayAwake.Checked; if (!_cargandoPagina) MarcarCambiosSinGuardar(); };

                var togScreenOff = new Guna2ToggleSwitch()
                {
                    Left = cardComport.Width - S(70),
                    Top = S(158),
                    Checked = _turnScreenOff,
                    CheckedState = { FillColor = accentColor },
                    UncheckedState = { FillColor = AppTheme.BorderNeutral },
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                togScreenOff.CheckedChanged += (s, e) => { _turnScreenOff = togScreenOff.Checked; if (!_cargandoPagina) MarcarCambiosSinGuardar(); };

                cardComport.Controls.AddRange(new Control[]
                {
                new Label() { Text = "Disable Screensaver", Font = new Font("Segoe UI", 10f), ForeColor = textPrimary, Left = S(24), Top = S(60), AutoSize = true },
                togScreensaver,
                new Label() { Text = "Stay Awake",          Font = new Font("Segoe UI", 10f), ForeColor = textPrimary, Left = S(24), Top = S(110), AutoSize = true },
                togStayAwake,
                new Label() { Text = "Turn Screen Off",     Font = new Font("Segoe UI", 10f), ForeColor = textPrimary, Left = S(24), Top = S(160), AutoSize = true },
                togScreenOff
                });

                // Card de depuración: ventana flotante y contador de FPS
                var cardDebug = CreateCard("Depuración", S(30), S(260), S(230));

                var togFlotante = new Guna2ToggleSwitch()
                {
                    Left = cardDebug.Width - S(70),
                    Top = S(58),
                    Checked = _mostrarFlotante,
                    CheckedState = { FillColor = accentColor },
                    UncheckedState = { FillColor = AppTheme.BorderNeutral },
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };

                var togPrintFps = new Guna2ToggleSwitch()
                {
                    Left = cardDebug.Width - S(70),
                    Top = S(128),
                    Checked = _printFps,
                    Enabled = _mostrarFlotante,
                    CheckedState = { FillColor = accentColor },
                    UncheckedState = { FillColor = AppTheme.BorderNeutral },
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };

                var lblFpsDesc = new Label()
                {
                    Text = _mostrarFlotante
                        ? "Muestra los FPS reales en la ventana flotante durante la sesión"
                        : "Requiere ventana flotante activa",
                    Font = new Font("Segoe UI", 8f),
                    ForeColor = _mostrarFlotante ? textSecondary : AppTheme.TextError,
                    Left = S(24),
                    Top = S(150),
                    AutoSize = true
                };

                togFlotante.CheckedChanged += (s, e) =>
                {
                    _mostrarFlotante = togFlotante.Checked;
                    togPrintFps.Enabled = _mostrarFlotante;
                    lblFpsDesc.Text = _mostrarFlotante
                        ? "Muestra los FPS reales en la ventana flotante durante la sesión"
                        : "Requiere ventana flotante activa";
                    lblFpsDesc.ForeColor = _mostrarFlotante ? textSecondary : AppTheme.TextError;
                    if (!_cargandoPagina) MarcarCambiosSinGuardar();
                };

                togPrintFps.CheckedChanged += (s, e) => { _printFps = togPrintFps.Checked; if (!_cargandoPagina) MarcarCambiosSinGuardar(); };

                cardDebug.Controls.AddRange(new Control[]
                {
                new Label() { Text = "Ventana flotante", Font = new Font("Segoe UI", 10f), ForeColor = textPrimary, Left = S(24), Top = S(60), AutoSize = true },
                new Label() { Text = "Muestra el panel flotante con info de la sesión al iniciar scrcpy", Font = new Font("Segoe UI", 8f), ForeColor = textSecondary, Left = S(24), Top = S(80), AutoSize = true },
                togFlotante,
                new Label() { Text = "Mostrar FPS", Font = new Font("Segoe UI", 10f), ForeColor = textPrimary, Left = S(24), Top = S(130), AutoSize = true },
                lblFpsDesc,
                togPrintFps,
                new Label() { Text = "ℹ El contador aparecerá al iniciar scrcpy, no al activar esta opción.", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = AppTheme.Warning, Left = S(24), Top = S(180), Width = cardDebug.Width - S(48), AutoSize = false, Height = S(20), Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right }
                });

                // Card de configuración de mouse y teclado
                var cardInput = CreateCard("Mouse y Teclado", S(30), S(510), S(240));

                var togForwardClicks = new Guna2ToggleSwitch()
                {
                    Left = cardInput.Width - S(70),
                    Top = S(58),
                    Checked = _forwardAllClicks,
                    CheckedState = { FillColor = accentColor },
                    UncheckedState = { FillColor = AppTheme.BorderNeutral },
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                togForwardClicks.CheckedChanged += (s, e) => { _forwardAllClicks = togForwardClicks.Checked; if (!_cargandoPagina) MarcarCambiosSinGuardar(); };

                var cmbInputMode = new Guna2ComboBox()
                {
                    Left = S(160),
                    Top = S(128),
                    Width = S(160),
                    Height = S(32),
                    Font = new Font("Segoe UI", 9f),
                    FillColor = AppTheme.BgCard,
                    ForeColor = textPrimary,
                    BorderColor = AppTheme.Accent,
                    BorderRadius = 4,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                cmbInputMode.Items.AddRange(new object[] { "uhid", "sdk" });
                cmbInputMode.SelectedItem = _inputMode ?? "uhid";
                if (cmbInputMode.SelectedIndex < 0) cmbInputMode.SelectedIndex = 0;

                var lblInputDesc = new Label()
                {
                    Text = _inputMode == "uhid"
                        ? "UHID — Simula teclado/mouse físico. Recomendado."
                        : "SDK — Inyección vía API Android. Más compatible.",
                    Font = new Font("Segoe UI", 8.5f),
                    ForeColor = AppTheme.TextLighter,
                    Left = S(24),
                    Top = S(172),
                    Width = S(280),
                    AutoSize = false,
                    Height = S(36)
                };

                cmbInputMode.SelectedIndexChanged += (s, e) =>
                {
                    _inputMode = cmbInputMode.SelectedItem?.ToString() ?? "uhid";
                    lblInputDesc.Text = _inputMode == "uhid"
                        ? "UHID — Simula teclado/mouse físico. Recomendado."
                        : "SDK — Inyección vía API Android. Más compatible.";
                    if (!_cargandoPagina) MarcarCambiosSinGuardar();
                };

                cardInput.Controls.AddRange(new Control[]
                {
                new Label() { Text = "Pasar todos los clics al dispositivo", Font = new Font("Segoe UI", 10f), ForeColor = textPrimary, Left = S(24), Top = S(60), AutoSize = true },
                new Label() { Text = "Fix para Shift+clic derecho en juegos (Free Fire, etc.)", Font = new Font("Segoe UI", 8f), ForeColor = textSecondary, Left = S(24), Top = S(80), AutoSize = true },
                togForwardClicks,
                new Label() { Text = "Modo de Entrada", Font = new Font("Segoe UI", 10f), ForeColor = textPrimary, Left = S(24), Top = S(110), AutoSize = true },
                cmbInputMode,
                lblInputDesc
                });

                // Card para configurar la tecla MOD de scrcpy
                var cardMod = CreateCard("Tecla de Atajos (MOD)", S(30), S(770), S(140));

                var cmbMod = new Guna2ComboBox()
                {
                    Left = S(24),
                    Top = S(56),
                    Width = S(200),
                    Height = S(32),
                    Font = new Font("Segoe UI", 9f),
                    FillColor = AppTheme.BgCard,
                    ForeColor = textPrimary,
                    BorderColor = AppTheme.Accent,
                    BorderRadius = 4,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                var modItems = new (string valor, string nombre)[]
                {
                    ("lalt",   "Alt izquierdo"),
                    ("ralt",   "Alt derecho"),
                    ("lctrl",  "Ctrl izquierdo"),
                    ("rctrl",  "Ctrl derecho"),
                    ("lsuper", "Tecla Windows izquierda"),
                    ("rsuper", "Tecla Windows derecha"),
                };
                foreach (var m in modItems) cmbMod.Items.Add(m.nombre);
                int modIdx = Array.FindIndex(modItems, m => m.valor == _shortcutMod);
                cmbMod.SelectedIndex = modIdx >= 0 ? modIdx : 0;
                cmbMod.SelectedIndexChanged += (s, e) =>
                {
                    int idx = cmbMod.SelectedIndex;
                    _shortcutMod = idx >= 0 && idx < modItems.Length ? modItems[idx].valor : "lalt";
                    if (!_cargandoPagina) MarcarCambiosSinGuardar();
                };

                cardMod.Controls.AddRange(new Control[]
                {
                cmbMod,
                new Label() { Text = "Tecla para atajos: MOD+F fullscreen, MOD+M menú, MOD+P power", Font = new Font("Segoe UI", 8f), ForeColor = AppTheme.TextLighter, Left = S(24), Top = S(104), Width = cardMod.Width - S(48), AutoSize = false, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right }
                });

                // Card de velocidad del cursor — experimental, aplica via ADB
                var cardCursor = CreateCard("Velocidad del Cursor (Mouse)  —  Experimental", S(30), S(930), S(185));

                var trackCursor = new Guna2TrackBar()
                {
                    Left = S(24),
                    Top = S(66),
                    Width = S(300),
                    Height = S(30),
                    Minimum = -7,
                    Maximum = 7,
                    Value = _pointerSpeed,
                    ThumbColor = AppTheme.Accent,
                    FillColor = AppTheme.BtnSecondary,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };

                var lblCursorValor = new Label()
                {
                    Text = _pointerSpeed == 0 ? "0 (default)" : _pointerSpeed.ToString("+0;-0"),
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    ForeColor = AppTheme.Accent,
                    Left = S(310),
                    Top = S(62),
                    AutoSize = true
                };

                var btnAplicarCursor = new Guna2Button()
                {
                    Text = "Aplicar ahora",
                    Width = S(140),
                    Height = S(32),
                    Left = S(24),
                    Top = S(118),
                    Font = new Font("Segoe UI", 8.5f),
                    FillColor = accentColor,
                    ForeColor = Color.White,
                    BorderRadius = 4,
                    ImageSize = new Size(S(18), S(18)),
                    ImageAlign = HorizontalAlignment.Left
                };
                btnAplicarCursor.Image = IconMap.Apply;

                var btnResetCursor = new Guna2Button()
                {
                    Text = "Restablecer (0)",
                    Width = S(150),
                    Height = S(32),
                    Left = S(174),
                    Top = S(118),
                    Font = new Font("Segoe UI", 8.5f),
                    FillColor = AppTheme.BtnSecondary,
                    ForeColor = textSecondary,
                    BorderColor = AppTheme.BorderSecondary,
                    BorderThickness = 1,
                    BorderRadius = 4,
                    ImageSize = new Size(S(18), S(18)),
                    ImageAlign = HorizontalAlignment.Left
                };
                btnResetCursor.Image = IconMap.Reset;

                var lblCursorStatus = new Label()
                {
                    Text = "",
                    Font = new Font("Segoe UI", 8f, FontStyle.Italic),
                    ForeColor = AppTheme.Success,
                    Left = S(334),
                    Top = S(126),
                    AutoSize = true
                };

                trackCursor.ValueChanged += (s, e) =>
                {
                    _pointerSpeed = trackCursor.Value;
                    lblCursorValor.Text = _pointerSpeed == 0 ? "0 (default)" : _pointerSpeed.ToString("+0;-0");
                };
                trackCursor.MouseWheel += (s, e) =>
                {
                    if (trackCursor.ContainsFocus)
                    {
                        if (e is HandledMouseEventArgs he) he.Handled = true;
                    }
                    else
                    {
                        PropagateWheelToContent(e);
                    }
                };

                btnAplicarCursor.Click += async (s, e) =>
                {
                    btnAplicarCursor.Enabled = false; btnAplicarCursor.Text = "Aplicando...";
                    var (exito, error) = await adbManager.AplicarPointerSpeedAsync(_pointerSpeed);
                    lblCursorStatus.Text = exito ? "✓ Aplicado" : "✗ Sin dispositivo conectado";
                    lblCursorStatus.ForeColor = exito ? AppTheme.Success : AppTheme.Error;
                    if (exito) { _ultimaVelocidadCursor = _pointerSpeed; GuardarConfigTema(); }
                    btnAplicarCursor.Text = "Aplicar ahora"; btnAplicarCursor.Enabled = true;
                    await Task.Delay(2500);
                    if (!lblCursorStatus.IsDisposed) lblCursorStatus.Text = "";
                };

                btnResetCursor.Click += async (s, e) =>
                {
                    _cargandoPagina = true; trackCursor.Value = 0; _pointerSpeed = 0; _cargandoPagina = false;
                    lblCursorValor.Text = "0 (default)";
                    var (exito, _) = await adbManager.AplicarPointerSpeedAsync(0);
                    lblCursorStatus.Text = exito ? "✓ Restablecido a 0" : "Guardado — se aplicará al conectar";
                    lblCursorStatus.ForeColor = exito ? AppTheme.Success : textSecondary;
                    await Task.Delay(2500);
                    if (!lblCursorStatus.IsDisposed) lblCursorStatus.Text = "";
                };

                cardCursor.Controls.AddRange(new Control[]
                {
                new Label() { Text = "Ajusta la velocidad del cursor del mouse en Android.", Font = new Font("Segoe UI", 8.5f), ForeColor = AppTheme.TextLighter, Left = S(24), Top = S(40), AutoSize = true },
                trackCursor, lblCursorValor,
                new Label() { Text = "-7 = más lento   |   0 = default   |   +7 = más rápido", Font = new Font("Segoe UI", 7.5f), ForeColor = textSecondary, Left = S(24), Top = S(94), AutoSize = true },
                btnAplicarCursor, btnResetCursor, lblCursorStatus
                });

                if (_ultimaVelocidadCursor != int.MinValue)
                    cardCursor.Controls.Add(new Label()
                    {
                        Text = $"Última velocidad aplicada: {(_ultimaVelocidadCursor == 0 ? "0 (default)" : _ultimaVelocidadCursor.ToString("+0;-0"))}",
                        Font = new Font("Segoe UI", 7.5f, FontStyle.Italic),
                        ForeColor = AppTheme.Accent,
                        Left = S(24),
                        Top = S(158),
                        AutoSize = true
                    });

                contentPanel.Controls.AddRange(new Control[]
                {
                cardComport, cardDebug, cardInput, cardMod, cardCursor
                });

            }
            finally { _cargandoPagina = false; }
        }
    }
}