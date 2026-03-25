using Guna.UI2.WinForms;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System;

namespace MobiladorStex
{
    public partial class Form1
    {
        private void LoadExtrasPage()
        {
            _cargandoPagina = true;
            try
            {

                // ── CARD: Comportamiento del dispositivo ──────────────────
                var cardComport = CreateCard("Comportamiento del Dispositivo", 30, 20, 220);

                var togScreensaver = new Guna2ToggleSwitch()
                {
                    Left = cardComport.Width - 70,
                    Top = 58,
                    Checked = _disableScreensaver,
                    CheckedState = { FillColor = accentColor },
                    UncheckedState = { FillColor = Color.FromArgb(60, 60, 60) },
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                togScreensaver.CheckedChanged += (s, e) => { _disableScreensaver = togScreensaver.Checked; if (!_cargandoPagina) MarcarCambiosSinGuardar(); };

                var togStayAwake = new Guna2ToggleSwitch()
                {
                    Left = cardComport.Width - 70,
                    Top = 108,
                    Checked = _stayAwake,
                    CheckedState = { FillColor = accentColor },
                    UncheckedState = { FillColor = Color.FromArgb(60, 60, 60) },
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                togStayAwake.CheckedChanged += (s, e) => { _stayAwake = togStayAwake.Checked; if (!_cargandoPagina) MarcarCambiosSinGuardar(); };

                var togScreenOff = new Guna2ToggleSwitch()
                {
                    Left = cardComport.Width - 70,
                    Top = 158,
                    Checked = _turnScreenOff,
                    CheckedState = { FillColor = accentColor },
                    UncheckedState = { FillColor = Color.FromArgb(60, 60, 60) },
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                togScreenOff.CheckedChanged += (s, e) => { _turnScreenOff = togScreenOff.Checked; if (!_cargandoPagina) MarcarCambiosSinGuardar(); };

                cardComport.Controls.AddRange(new Control[]
                {
                new Label() { Text = "Disable Screensaver", Font = new Font("Segoe UI", 10f), ForeColor = textPrimary, Left = 24, Top = 60, AutoSize = true },
                togScreensaver,
                new Label() { Text = "Stay Awake",          Font = new Font("Segoe UI", 10f), ForeColor = textPrimary, Left = 24, Top = 110, AutoSize = true },
                togStayAwake,
                new Label() { Text = "Turn Screen Off",     Font = new Font("Segoe UI", 10f), ForeColor = textPrimary, Left = 24, Top = 160, AutoSize = true },
                togScreenOff
                });

                // ── CARD: Depuración ──────────────────────────────────────
                var cardDebug = CreateCard("Depuración", 30, 260, 230);

                var togFlotante = new Guna2ToggleSwitch()
                {
                    Left = cardDebug.Width - 70,
                    Top = 58,
                    Checked = _mostrarFlotante,
                    CheckedState = { FillColor = accentColor },
                    UncheckedState = { FillColor = Color.FromArgb(60, 60, 60) },
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                togFlotante.CheckedChanged += (s, e) => { _mostrarFlotante = togFlotante.Checked; if (!_cargandoPagina) MarcarCambiosSinGuardar(); };

                var togPrintFps = new Guna2ToggleSwitch()
                {
                    Left = cardDebug.Width - 70,
                    Top = 128,
                    Checked = _printFps,
                    CheckedState = { FillColor = accentColor },
                    UncheckedState = { FillColor = Color.FromArgb(60, 60, 60) },
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                togPrintFps.CheckedChanged += (s, e) => { _printFps = togPrintFps.Checked; if (!_cargandoPagina) MarcarCambiosSinGuardar(); };

                cardDebug.Controls.AddRange(new Control[]
                {
                new Label() { Text = "Ventana flotante", Font = new Font("Segoe UI", 10f), ForeColor = textPrimary, Left = 24, Top = 60, AutoSize = true },
                new Label() { Text = "Muestra el panel flotante con info de la sesión al iniciar scrcpy", Font = new Font("Segoe UI", 8f), ForeColor = textSecondary, Left = 24, Top = 80, AutoSize = true },
                togFlotante,
                new Label() { Text = "Mostrar FPS", Font = new Font("Segoe UI", 10f), ForeColor = textPrimary, Left = 24, Top = 130, AutoSize = true },
                new Label() { Text = "Muestra los FPS reales en la ventana flotante durante la sesión", Font = new Font("Segoe UI", 8f), ForeColor = textSecondary, Left = 24, Top = 150, AutoSize = true },
                togPrintFps,
                new Label() { Text = "ℹ El contador aparecerá al iniciar scrcpy, no al activar esta opción.", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = Color.FromArgb(255, 167, 38), Left = 24, Top = 180, Width = cardDebug.Width - 48, AutoSize = false, Height = 20, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right }
                });

                // ── CARD: Mouse y Teclado ─────────────────────────────────
                var cardInput = CreateCard("Mouse y Teclado", 30, 510, 240);

                var togForwardClicks = new Guna2ToggleSwitch()
                {
                    Left = cardInput.Width - 70,
                    Top = 58,
                    Checked = _forwardAllClicks,
                    CheckedState = { FillColor = accentColor },
                    UncheckedState = { FillColor = Color.FromArgb(60, 60, 60) },
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                togForwardClicks.CheckedChanged += (s, e) => { _forwardAllClicks = togForwardClicks.Checked; if (!_cargandoPagina) MarcarCambiosSinGuardar(); };

                var cmbInputMode = new Guna2ComboBox()
                {
                    Left = 160,
                    Top = 128,
                    Width = 160,
                    Height = 32,
                    Font = new Font("Segoe UI", 9f),
                    FillColor = Color.FromArgb(42, 42, 45),
                    ForeColor = textPrimary,
                    BorderColor = Color.FromArgb(80, 60, 100),
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
                    ForeColor = Color.FromArgb(210, 210, 210),
                    Left = 24,
                    Top = 172,
                    Width = 280,
                    AutoSize = false,
                    Height = 36
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
                new Label() { Text = "Pasar todos los clics al dispositivo", Font = new Font("Segoe UI", 10f), ForeColor = textPrimary, Left = 24, Top = 60, AutoSize = true },
                new Label() { Text = "Fix para Shift+clic derecho en juegos (Free Fire, etc.)", Font = new Font("Segoe UI", 8f), ForeColor = textSecondary, Left = 24, Top = 80, AutoSize = true },
                togForwardClicks,
                new Label() { Text = "Modo de Entrada", Font = new Font("Segoe UI", 10f), ForeColor = textPrimary, Left = 24, Top = 110, AutoSize = true },
                cmbInputMode,
                lblInputDesc
                });

                // ── CARD: Tecla de Atajos (MOD) ───────────────────────────
                var cardMod = CreateCard("Tecla de Atajos (MOD)", 30, 770, 140);

                var cmbMod = new Guna2ComboBox()
                {
                    Left = 24,
                    Top = 56,
                    Width = 200,
                    Height = 32,
                    Font = new Font("Segoe UI", 9f),
                    FillColor = Color.FromArgb(42, 42, 45),
                    ForeColor = textPrimary,
                    BorderColor = Color.FromArgb(80, 60, 100),
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
                new Label() { Text = "Tecla para atajos: MOD+F fullscreen, MOD+M menú, MOD+P power", Font = new Font("Segoe UI", 8f), ForeColor = Color.FromArgb(210, 210, 210), Left = 24, Top = 104, Width = cardMod.Width - 48, AutoSize = false, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right }
                });

                // ── CARD: Velocidad del Cursor ────────────────────────────
                var cardCursor = CreateCard("Velocidad del Cursor (Mouse)  —  Experimental", 30, 930, 170);

                var trackCursor = new Guna2TrackBar()
                {
                    Left = 24,
                    Top = 66,
                    Width = 300,
                    Height = 30,
                    Minimum = -7,
                    Maximum = 7,
                    Value = _pointerSpeed,
                    ThumbColor = Color.FromArgb(107, 47, 196),
                    FillColor = Color.FromArgb(55, 40, 75),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };

                var lblCursorValor = new Label()
                {
                    Text = _pointerSpeed == 0 ? "0 (default)" : _pointerSpeed.ToString("+0;-0"),
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(107, 47, 196),
                    Left = 310,
                    Top = 62,
                    AutoSize = true
                };

                var btnAplicarCursor = new Guna2Button()
                {
                    Text = "✓ Aplicar ahora",
                    Width = 140,
                    Height = 32,
                    Left = 24,
                    Top = 118,
                    Font = new Font("Segoe UI", 8.5f),
                    FillColor = accentColor,
                    ForeColor = Color.White,
                    BorderRadius = 4
                };

                var btnResetCursor = new Guna2Button()
                {
                    Text = "↺ Restablecer (0)",
                    Width = 150,
                    Height = 32,
                    Left = 174,
                    Top = 118,
                    Font = new Font("Segoe UI", 8.5f),
                    FillColor = Color.FromArgb(55, 40, 75),
                    ForeColor = textSecondary,
                    BorderColor = Color.FromArgb(80, 60, 100),
                    BorderThickness = 1,
                    BorderRadius = 4
                };

                var lblCursorStatus = new Label()
                {
                    Text = "",
                    Font = new Font("Segoe UI", 8f, FontStyle.Italic),
                    ForeColor = Color.FromArgb(16, 124, 16),
                    Left = 334,
                    Top = 126,
                    AutoSize = true
                };

                trackCursor.ValueChanged += (s, e) =>
                {
                    _pointerSpeed = trackCursor.Value;
                    lblCursorValor.Text = _pointerSpeed == 0 ? "0 (default)" : _pointerSpeed.ToString("+0;-0");
                    if (!_cargandoPagina) MarcarCambiosSinGuardar();
                };

                btnAplicarCursor.Click += async (s, e) =>
                {
                    btnAplicarCursor.Enabled = false; btnAplicarCursor.Text = "Aplicando...";
                    var (exito, error) = await adbManager.AplicarPointerSpeedAsync(_pointerSpeed);
                    lblCursorStatus.Text = exito ? "✓ Aplicado" : "✗ Sin dispositivo conectado";
                    lblCursorStatus.ForeColor = exito ? Color.FromArgb(16, 124, 16) : Color.FromArgb(220, 50, 50);
                    btnAplicarCursor.Text = "✓ Aplicar ahora"; btnAplicarCursor.Enabled = true;
                    await Task.Delay(2500);
                    if (!lblCursorStatus.IsDisposed) lblCursorStatus.Text = "";
                };

                btnResetCursor.Click += async (s, e) =>
                {
                    _cargandoPagina = true; trackCursor.Value = 0; _pointerSpeed = 0; _cargandoPagina = false;
                    lblCursorValor.Text = "0 (default)"; MarcarCambiosSinGuardar();
                    var (exito, _) = await adbManager.AplicarPointerSpeedAsync(0);
                    lblCursorStatus.Text = exito ? "✓ Restablecido a 0" : "Guardado — se aplicará al conectar";
                    lblCursorStatus.ForeColor = exito ? Color.FromArgb(16, 124, 16) : textSecondary;
                    await Task.Delay(2500);
                    if (!lblCursorStatus.IsDisposed) lblCursorStatus.Text = "";
                };

                cardCursor.Controls.AddRange(new Control[]
                {
                new Label() { Text = "Ajusta la velocidad del cursor del mouse en Android.", Font = new Font("Segoe UI", 8.5f), ForeColor = Color.FromArgb(210, 210, 210), Left = 24, Top = 40, AutoSize = true },
                trackCursor, lblCursorValor,
                new Label() { Text = "-7 = más lento   |   0 = default   |   +7 = más rápido", Font = new Font("Segoe UI", 7.5f), ForeColor = textSecondary, Left = 24, Top = 94, AutoSize = true },
                btnAplicarCursor, btnResetCursor, lblCursorStatus
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