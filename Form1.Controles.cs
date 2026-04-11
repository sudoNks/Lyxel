using Guna.UI2.WinForms;
using LyXel.Helpers;
using System;
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
                // ── Card: Modo de Entrada ─────────────────────────────────────────
                var cardModoEntrada = CreateCard("Modo de Entrada", S(30), S(20), S(150));

                var cmbModoEntrada = new Guna2ComboBox()
                {
                    Left = S(24),
                    Top = S(82),
                    Width = S(180),
                    Height = S(34),
                    Font = new Font("Segoe UI", 9.5f),
                    ForeColor = textPrimary,
                    FillColor = AppTheme.BgCard,
                    BorderColor = AppTheme.BorderNeutral,
                    BorderRadius = 6,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left
                };
                cmbModoEntrada.Items.AddRange(new object[] { "uhid", "sdk" });
                cmbModoEntrada.SelectedIndex = _inputMode == "sdk" ? 1 : 0;

                cmbModoEntrada.SelectedIndexChanged += (s, e) =>
                {
                    if (_cargandoPagina) return;
                    _inputMode = cmbModoEntrada.SelectedItem?.ToString() ?? "uhid";
                    // Propagar a teclado y mouse si están activos
                    if (_tecladoModo != "disabled") _tecladoModo = _inputMode;
                    if (_mouseModo != "disabled") _mouseModo = _inputMode;
                    MarcarCambiosSinGuardar();
                };

                cardModoEntrada.Controls.AddRange(new Control[]
                {
                    new Label() { Text = "Modo de entrada", Font = new Font("Segoe UI", 10f), ForeColor = textPrimary, Left = S(24), Top = S(60), AutoSize = true },
                    cmbModoEntrada,
                    new Label()
                    {
                        Text = "UHID simula un dispositivo físico HID, recomendado para juegos. SDK es el modo estándar de Android, útil para ver contenido.",
                        Font = new Font("Segoe UI", 8f), ForeColor = textSecondary,
                        Left = S(24), Top = S(118), Width = cardModoEntrada.Width - S(48), Height = S(28),
                        AutoSize = false, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                    }
                });

                // ── Card: Teclado y Mouse ─────────────────────────────────────────
                var cardTecladoMouse = CreateCard("Teclado y Mouse", S(30), S(190), S(310));

                var togTeclado = new Guna2ToggleSwitch()
                {
                    Left = cardTecladoMouse.Width - S(70),
                    Top = S(58),
                    Checked = _tecladoModo != "disabled",
                    CheckedState = { FillColor = accentColor },
                    UncheckedState = { FillColor = AppTheme.BorderNeutral },
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };

                var togMouse = new Guna2ToggleSwitch()
                {
                    Left = cardTecladoMouse.Width - S(70),
                    Top = S(128),
                    Checked = _mouseModo != "disabled",
                    CheckedState = { FillColor = accentColor },
                    UncheckedState = { FillColor = AppTheme.BorderNeutral },
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };

                var togForwardClicks = new Guna2ToggleSwitch()
                {
                    Left = cardTecladoMouse.Width - S(70),
                    Top = S(198),
                    Checked = _forwardAllClicks,
                    CheckedState = { FillColor = accentColor },
                    UncheckedState = { FillColor = AppTheme.BorderNeutral },
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };

                string gamepadInfoText = "⚠ Teclado y Mouse desactivados por Gamepad UHID";

                var lblGamepadInfo = new Label()
                {
                    Text = gamepadInfoText,
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                    ForeColor = AppTheme.Warning,
                    Left = S(24),
                    Top = S(254),
                    Width = cardTecladoMouse.Width - S(48),
                    Height = S(22),
                    AutoSize = false,
                    Visible = _gamepadModo != "disabled",
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };

                togTeclado.CheckedChanged += (s, e) =>
                {
                    if (_cargandoPagina) return;
                    _tecladoModo = togTeclado.Checked ? _inputMode : "disabled";
                    MarcarCambiosSinGuardar();
                };
                togMouse.CheckedChanged += (s, e) =>
                {
                    if (_cargandoPagina) return;
                    _mouseModo = togMouse.Checked ? _inputMode : "disabled";
                    MarcarCambiosSinGuardar();
                };
                togForwardClicks.CheckedChanged += (s, e) =>
                {
                    if (_cargandoPagina) return;
                    _forwardAllClicks = togForwardClicks.Checked;
                    MarcarCambiosSinGuardar();
                };

                // Deshabilitar visualmente teclado/mouse si el gamepad ya estaba activo al cargar
                if (_gamepadModo != "disabled")
                {
                    togTeclado.Enabled = false;
                    togMouse.Enabled = false;
                }

                cardTecladoMouse.Controls.AddRange(new Control[]
                {
                    new Label() { Text = "Activar Teclado", Font = new Font("Segoe UI", 10f), ForeColor = textPrimary, Left = S(24), Top = S(60), AutoSize = true },
                    new Label()
                    {
                        Text = "Simula teclado físico HID. Funciona por USB y WiFi. Usa MOD+K para configurar el layout en el dispositivo.",
                        Font = new Font("Segoe UI", 8f), ForeColor = textSecondary,
                        Left = S(24), Top = S(80), Width = cardTecladoMouse.Width - S(100), Height = S(28),
                        AutoSize = false, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                    },
                    togTeclado,
                    new Label() { Text = "Activar Mouse", Font = new Font("Segoe UI", 10f), ForeColor = textPrimary, Left = S(24), Top = S(130), AutoSize = true },
                    new Label()
                    {
                        Text = "Simula mouse físico HID. El cursor desaparece del PC y aparece en el dispositivo. Usa MOD para liberar el cursor.",
                        Font = new Font("Segoe UI", 8f), ForeColor = textSecondary,
                        Left = S(24), Top = S(150), Width = cardTecladoMouse.Width - S(100), Height = S(28),
                        AutoSize = false, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                    },
                    togMouse,
                    new Label() { Text = "Pasar todos los clics", Font = new Font("Segoe UI", 10f), ForeColor = textPrimary, Left = S(24), Top = S(200), AutoSize = true },
                    new Label()
                    {
                        Text = "Fix para Shift+clic derecho en juegos (Free Fire, etc.)",
                        Font = new Font("Segoe UI", 8f), ForeColor = textSecondary, Left = S(24), Top = S(220), AutoSize = true
                    },
                    togForwardClicks,
                    lblGamepadInfo
                });

                // ── Card: Gamepad ─────────────────────────────────────────────────
                var cardGamepad = CreateCard("Gamepad", S(30), S(520), S(150));

                var togGamepadUhid = new Guna2ToggleSwitch()
                {
                    Left = cardGamepad.Width - S(70),
                    Top = S(58),
                    Checked = _gamepadModo == "uhid",
                    CheckedState = { FillColor = accentColor },
                    UncheckedState = { FillColor = AppTheme.BorderNeutral },
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };

                var ttGamepad = new ToolTip();
                ttGamepad.SetToolTip(togGamepadUhid, "Conecta tu mando vía USB. Compatible con la mayoría de controladores.");

                togGamepadUhid.CheckedChanged += (s, e) =>
                {
                    if (_cargandoPagina) return;

                    if (togGamepadUhid.Checked)
                    {
                        _gamepadPrevTeclado = _tecladoModo;
                        _gamepadPrevMouse = _mouseModo;

                        _tecladoModo = "disabled";
                        _mouseModo = "disabled";
                        _gamepadModo = "uhid";

                        _cargandoPagina = true;
                        togTeclado.Checked = false;
                        togMouse.Checked = false;
                        _cargandoPagina = false;

                        togTeclado.Enabled = false;
                        togMouse.Enabled = false;

                        lblGamepadInfo.Visible = true;
                    }
                    else
                    {
                        _tecladoModo = _gamepadPrevTeclado;
                        _mouseModo = _gamepadPrevMouse;
                        _gamepadModo = "disabled";

                        _cargandoPagina = true;
                        togTeclado.Checked = _tecladoModo != "disabled";
                        togMouse.Checked = _mouseModo != "disabled";
                        _cargandoPagina = false;

                        togTeclado.Enabled = true;
                        togMouse.Enabled = true;

                        lblGamepadInfo.Visible = false;
                    }

                    MarcarCambiosSinGuardar();
                };

                cardGamepad.Controls.AddRange(new Control[]
                {
                    new Label() { Text = "Gamepad (UHID)", Font = new Font("Segoe UI", 10f), ForeColor = textPrimary, Left = S(24), Top = S(60), AutoSize = true },
                    new Label()
                    {
                        Text = "Simula gamepad físico HID. Funciona por USB y WiFi. Al activarlo, teclado y mouse se desactivan automáticamente.",
                        Font = new Font("Segoe UI", 8f), ForeColor = textSecondary,
                        Left = S(24), Top = S(80), Width = cardGamepad.Width - S(100), Height = S(40),
                        AutoSize = false, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                    },
                    togGamepadUhid
                });

                contentPanel.Controls.AddRange(new Control[] { cardModoEntrada, cardTecladoMouse, cardGamepad });
            }
            finally { _cargandoPagina = false; }
        }
    }
}
