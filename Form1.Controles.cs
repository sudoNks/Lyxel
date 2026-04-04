using Guna.UI2.WinForms;
using LyXel.Helpers;
using System.Drawing;
using System.Windows.Forms;

namespace LyXel
{
    public partial class Form1
    {
        private void LoadControlesPage()
        {
            _cargandoPagina = true;
            try
            {
                // ── Card: Teclado y Mouse ────────────────────────────────────────
                var cardTecladoMouse = CreateCard("Teclado y Mouse", S(30), S(20), S(284));

                var togTeclado = new Guna2ToggleSwitch()
                {
                    Left = cardTecladoMouse.Width - S(70),
                    Top = S(58),
                    Checked = _tecladoModo == "uhid",
                    CheckedState = { FillColor = accentColor },
                    UncheckedState = { FillColor = AppTheme.BorderNeutral },
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                togTeclado.CheckedChanged += (s, e) =>
                {
                    _tecladoModo = togTeclado.Checked ? "uhid" : "disabled";
                    if (!_cargandoPagina) MarcarCambiosSinGuardar();
                };

                var togMouse = new Guna2ToggleSwitch()
                {
                    Left = cardTecladoMouse.Width - S(70),
                    Top = S(118),
                    Checked = _mouseModo == "uhid",
                    CheckedState = { FillColor = accentColor },
                    UncheckedState = { FillColor = AppTheme.BorderNeutral },
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                togMouse.CheckedChanged += (s, e) =>
                {
                    _mouseModo = togMouse.Checked ? "uhid" : "disabled";
                    if (!_cargandoPagina) MarcarCambiosSinGuardar();
                };

                var togForwardClicks = new Guna2ToggleSwitch()
                {
                    Left = cardTecladoMouse.Width - S(70),
                    Top = S(188),
                    Checked = _forwardAllClicks,
                    CheckedState = { FillColor = accentColor },
                    UncheckedState = { FillColor = AppTheme.BorderNeutral },
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                togForwardClicks.CheckedChanged += (s, e) =>
                {
                    _forwardAllClicks = togForwardClicks.Checked;
                    if (!_cargandoPagina) MarcarCambiosSinGuardar();
                };

                cardTecladoMouse.Controls.AddRange(new Control[]
                {
                    new Label() { Text = "Activar Teclado",   Font = new Font("Segoe UI", 10f), ForeColor = textPrimary,    Left = S(24), Top = S(60),  AutoSize = true },
                    new Label() { Text = "Simula un teclado físico UHID en el dispositivo. Recomendado.",
                                  Font = new Font("Segoe UI", 8f), ForeColor = textSecondary, Left = S(24), Top = S(80), AutoSize = true },
                    togTeclado,
                    new Label() { Text = "Activar Mouse",     Font = new Font("Segoe UI", 10f), ForeColor = textPrimary,    Left = S(24), Top = S(120), AutoSize = true },
                    new Label() { Text = "Simula un mouse físico UHID en el dispositivo. Recomendado.",
                                  Font = new Font("Segoe UI", 8f), ForeColor = textSecondary, Left = S(24), Top = S(140), AutoSize = true },
                    togMouse,
                    new Label() { Text = "Pasar todos los clics",  Font = new Font("Segoe UI", 10f), ForeColor = textPrimary, Left = S(24), Top = S(190), AutoSize = true },
                    new Label() { Text = "Fix para Shift+clic derecho en juegos (Free Fire, etc.)",
                                  Font = new Font("Segoe UI", 8f), ForeColor = textSecondary, Left = S(24), Top = S(210), AutoSize = true },
                    togForwardClicks
                });

                // ── Card: Gamepad ────────────────────────────────────────────────
                var cardGamepad = CreateCard("Gamepad", S(30), S(324), S(144));

                var togGamepad = new Guna2ToggleSwitch()
                {
                    Left = cardGamepad.Width - S(70),
                    Top = S(58),
                    Checked = _gamepadModo == "uhid",
                    CheckedState = { FillColor = accentColor },
                    UncheckedState = { FillColor = AppTheme.BorderNeutral },
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                togGamepad.CheckedChanged += (s, e) =>
                {
                    _gamepadModo = togGamepad.Checked ? "uhid" : "disabled";
                    if (!_cargandoPagina) MarcarCambiosSinGuardar();
                };

                cardGamepad.Controls.AddRange(new Control[]
                {
                    new Label() { Text = "Activar Gamepad",  Font = new Font("Segoe UI", 10f), ForeColor = textPrimary, Left = S(24), Top = S(60), AutoSize = true },
                    new Label() { Text = "Emula un gamepad UHID. Requiere Android 13+ y USB o WiFi estable.",
                                  Font = new Font("Segoe UI", 8f), ForeColor = textSecondary, Left = S(24), Top = S(80), Width = cardGamepad.Width - S(110), AutoSize = false, Height = S(30),
                                  Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right },
                    togGamepad
                });

                contentPanel.Controls.AddRange(new Control[] { cardTecladoMouse, cardGamepad });
            }
            finally { _cargandoPagina = false; }
        }
    }
}
