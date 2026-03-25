using Guna.UI2.WinForms;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System;

namespace MobiladorStex
{
    public partial class Form1
    {
        // ── CAMPO DE CLASE ────────────────────────────────────────────────────────
        // Persiste entre navegaciones — se llena al detectar y se reutiliza al volver
        // IMPORTANTE: declarar también en Form1.cs junto al resto de campos:
        //   private List<string> _encodersDetectados      = new();
        //   private List<string> _encodersDisplayLabels   = new();

        private void LoadVideoPage()
        {
            _cargandoPagina = true;
            try
            {

                // ── CARD: Video ───────────────────────────────────────────
                var cardVideo = CreateCard("Configuración de Video", 30, 20, 470);

                var togVideo = new Guna2ToggleSwitch()
                {
                    Left = cardVideo.Width - 70,
                    Top = 58,
                    Checked = _video,
                    CheckedState = { FillColor = accentColor },
                    UncheckedState = { FillColor = Color.FromArgb(60, 60, 60) },
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                togVideo.CheckedChanged += (s, e) => { _video = togVideo.Checked; if (!_cargandoPagina) MarcarCambiosSinGuardar(); };

                var trackFps = new Guna2TrackBar()
                {
                    Minimum = 30,
                    Maximum = 240,
                    Value = _fps,
                    Left = 160,
                    Top = 106,
                    Width = cardVideo.Width - 300,
                    Height = 28,
                    ThumbColor = accentColor,
                    FillColor = Color.FromArgb(40, 40, 40),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                var lblFpsVal = new Label()
                {
                    Text = _fps.ToString(),
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    ForeColor = textPrimary,
                    Left = cardVideo.Width - 70,
                    Top = 108,
                    Width = 50,
                    TextAlign = ContentAlignment.MiddleRight,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                trackFps.Scroll += (s, e) =>
                {
                    _fps = trackFps.Value;
                    lblFpsVal.Text = _fps.ToString();
                    if (!_cargandoPagina) MarcarCambiosSinGuardar();
                };

                var trackBitrate = new Guna2TrackBar()
                {
                    Minimum = 1,
                    Maximum = 200,
                    Value = _bitrate,
                    Left = 160,
                    Top = 156,
                    Width = cardVideo.Width - 300,
                    Height = 28,
                    ThumbColor = accentColor,
                    FillColor = Color.FromArgb(40, 40, 40),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };
                var lblBitrateVal = new Label()
                {
                    Text = _bitrate.ToString(),
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    ForeColor = textPrimary,
                    Left = cardVideo.Width - 70,
                    Top = 158,
                    Width = 50,
                    TextAlign = ContentAlignment.MiddleRight,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                trackBitrate.Scroll += (s, e) =>
                {
                    _bitrate = trackBitrate.Value;
                    lblBitrateVal.Text = _bitrate.ToString();
                    if (!_cargandoPagina) MarcarCambiosSinGuardar();
                };

                var numMaxSize = CreateNumeric(160, 265, 120, 0, 4000, _maxSize, 100);
                numMaxSize.ValueChanged += (s, e) => { _maxSize = (int)numMaxSize.Value; if (!_cargandoPagina) MarcarCambiosSinGuardar(); };
                numMaxSize.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        e.SuppressKeyPress = true; // suprime el sonido de Windows
                        this.ActiveControl = null; // quita el foco → dispara Leave
                    }
                };
                numMaxSize.Leave += (s, e) => { _maxSize = (int)numMaxSize.Value; if (!_cargandoPagina) MarcarCambiosSinGuardar(); };


                // Video Codec — en fila 435, label arriba, combo + aviso debajo
                var cmbCodec = new Guna2ComboBox()
                {
                    Left = 160,
                    Top = 318,
                    Width = 120,
                    Height = 32,
                    FillColor = Color.FromArgb(42, 42, 45),
                    ForeColor = textPrimary,
                    BorderColor = Color.FromArgb(60, 60, 60),
                    BorderRadius = 4,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = new Font("Segoe UI", 9f),
                    Enabled = !_useAdvancedEncoder
                };
                cmbCodec.Items.AddRange(new object[] { "h264", "h265", "av1" });
                cmbCodec.SelectedItem = _videoCodec;
                if (cmbCodec.SelectedIndex < 0) cmbCodec.SelectedIndex = 0;
                cmbCodec.SelectedIndexChanged += (s, e) =>
                {
                    _videoCodec = cmbCodec.SelectedItem?.ToString() ?? "h264";
                    if (!_cargandoPagina) MarcarCambiosSinGuardar();
                };
                if (_useAdvancedEncoder)
                    cmbCodec.FillColor = Color.FromArgb(20, 20, 20);

                // Aviso de codec desactivado — debajo del combo, separado, nunca encima
                var lblCodecDesactivado = new Label()
                {
                    Text = "⚠ Desactivado — usando encoder avanzado",
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(255, 140, 0),
                    Left = 292,
                    Top = 322,   // a la derecha del combo (160+120+12)
                    Width = 340,
                    AutoSize = false,
                    Visible = _useAdvancedEncoder
                };

                // ── Video Buffer ──────────────────────────────────────────
                var numVideoBuffer = new StexNumericUpDown()
                {
                    Left = 160,
                    Top = 418,
                    Width = 100,
                    Height = 32,
                    Minimum = 0,
                    Maximum = 500,
                    Value = _videoBuffer,
                    Increment = 10
                };
                numVideoBuffer.ValueChanged += (s, e) =>
                {
                    _videoBuffer = (int)numVideoBuffer.Value;
                    if (!_cargandoPagina) MarcarCambiosSinGuardar();
                };
                numVideoBuffer.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        e.SuppressKeyPress = true; // suprime el sonido de Windows
                        this.ActiveControl = null; // quita el foco → dispara Leave
                    }
                };
                numVideoBuffer.Leave += (s, e) =>
                {
                    _videoBuffer = (int)numVideoBuffer.Value;
                    if (!_cargandoPagina) MarcarCambiosSinGuardar();
                };

                cardVideo.Controls.AddRange(new Control[]
                {
                new Label() { Text = "Habilitar Video",   Font = new Font("Segoe UI", 10f), ForeColor = textPrimary, Left = 24, Top = 60, AutoSize = true },
                togVideo,
                new Label() { Text = "FPS",               Font = new Font("Segoe UI", 9.5f), ForeColor = textPrimary, Left = 24, Top = 110, AutoSize = true },
                trackFps, lblFpsVal,
                new Label() { Text = "Bitrate (Mb)",      Font = new Font("Segoe UI", 9.5f), ForeColor = textPrimary, Left = 24, Top = 160, AutoSize = true },
                trackBitrate, lblBitrateVal,
                new Label() { Text = "⚠ Si la pantalla no abre, reduce FPS o Bitrate. Depende del dispositivo y cable.", Font = new Font("Segoe UI", 7.5f), ForeColor = Color.FromArgb(200, 140, 40), Left = 24, Top = 186, Width = cardVideo.Width - 48, AutoSize = false, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right },
                new Label() { Text = "Max Size",          Font = new Font("Segoe UI", 9.5f), ForeColor = textPrimary, Left = 24, Top = 215, AutoSize = true },
                new Label() { Text = "Limita la resolución transmitida (menor = más rendimiento)", Font = new Font("Segoe UI", 8f), ForeColor = textSecondary, Left = 24, Top = 235, Width = cardVideo.Width - 48, AutoSize = false, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right },
                numMaxSize,
                new Label() { Text = "Video Codec",       Font = new Font("Segoe UI", 9.5f), ForeColor = textPrimary, Left = 24, Top = 290, AutoSize = true },
                cmbCodec,
                lblCodecDesactivado,
                new Label() { Text = "Video Buffer (ms)", Font = new Font("Segoe UI", 9.5f), ForeColor = textPrimary, Left = 24, Top = 370, AutoSize = true },
                new Label() { Text = "0 = sin buffer (menor latencia). Aumentar si hay saltos de imagen.", Font = new Font("Segoe UI", 8f), ForeColor = textSecondary, Left = 24, Top = 390, Width = cardVideo.Width - 48, AutoSize = false, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right },
                numVideoBuffer
                });

                // ── CARD: Audio ───────────────────────────────────────────
                var cardAudio = CreateCard("Configuración de Audio", 30, 510, 400);

                var togAudio = new Guna2ToggleSwitch()
                {
                    Left = cardAudio.Width - 70,
                    Top = 58,
                    Checked = _audio,
                    CheckedState = { FillColor = accentColor },
                    UncheckedState = { FillColor = Color.FromArgb(60, 60, 60) },
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                togAudio.CheckedChanged += (s, e) => { _audio = togAudio.Checked; if (!_cargandoPagina) MarcarCambiosSinGuardar(); };

                var togAudioDoble = new Guna2ToggleSwitch()
                {
                    Left = cardAudio.Width - 70,
                    Top = 108,
                    Checked = _audioDoble,
                    CheckedState = { FillColor = accentColor },
                    UncheckedState = { FillColor = Color.FromArgb(60, 60, 60) },
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                togAudioDoble.CheckedChanged += (s, e) => { _audioDoble = togAudioDoble.Checked; if (!_cargandoPagina) MarcarCambiosSinGuardar(); };

                // ── Audio Codec ───────────────────────────────────────────
                var cmbAudioCodec = new Guna2ComboBox()
                {
                    Left = 160,
                    Top = 192,
                    Width = 120,
                    Height = 32,
                    Font = new Font("Segoe UI", 9f),
                    FillColor = Color.FromArgb(42, 42, 45),
                    ForeColor = textPrimary,
                    BorderColor = Color.FromArgb(60, 60, 60),
                    BorderRadius = 4,
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                cmbAudioCodec.Items.AddRange(new object[] { "opus", "aac", "flac" });
                cmbAudioCodec.SelectedItem = _audioCodec ?? "opus";
                if (cmbAudioCodec.SelectedIndex < 0) cmbAudioCodec.SelectedIndex = 0;
                cmbAudioCodec.SelectedIndexChanged += (s, e) =>
                {
                    _audioCodec = cmbAudioCodec.SelectedItem?.ToString() ?? "opus";
                    if (!_cargandoPagina) MarcarCambiosSinGuardar();
                };

                // ── Audio Bitrate ─────────────────────────────────────────
                var numAudioBitrate = new StexNumericUpDown()
                {
                    Left = 160,
                    Top = 252,
                    Width = 100,
                    Height = 32,
                    Minimum = 32,
                    Maximum = 320,
                    Value = _audioBitrate,
                    Increment = 8
                };
                numAudioBitrate.ValueChanged += (s, e) =>
                {
                    _audioBitrate = (int)numAudioBitrate.Value;
                    if (!_cargandoPagina) MarcarCambiosSinGuardar();
                };
                numAudioBitrate.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        e.SuppressKeyPress = true; // suprime el sonido de Windows
                        this.ActiveControl = null; // quita el foco → dispara Leave
                    }
                };
                numAudioBitrate.Leave += (s, e) =>
                {
                    _audioBitrate = (int)numAudioBitrate.Value;
                    if (!_cargandoPagina) MarcarCambiosSinGuardar();
                };

                // ── Audio Buffer ──────────────────────────────────────────
                var numAudioBuffer = new StexNumericUpDown()
                {
                    Left = 160,
                    Top = 342,
                    Width = 100,
                    Height = 32,
                    Minimum = 0,
                    Maximum = 500,
                    Value = _audioBuffer,
                    Increment = 10
                };
                numAudioBuffer.ValueChanged += (s, e) =>
                {
                    _audioBuffer = (int)numAudioBuffer.Value;
                    if (!_cargandoPagina) MarcarCambiosSinGuardar();
                };
                numAudioBuffer.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                    {
                        e.SuppressKeyPress = true; // suprime el sonido de Windows
                        this.ActiveControl = null; // quita el foco → dispara Leave
                    }
                };
                numAudioBuffer.Leave += (s, e) =>
                {
                    _audioBuffer = (int)numAudioBuffer.Value;
                    if (!_cargandoPagina) MarcarCambiosSinGuardar();
                };

                cardAudio.Controls.AddRange(new Control[]
                {
                new Label() { Text = "Habilitar Audio",   Font = new Font("Segoe UI", 10f), ForeColor = textPrimary, Left = 24, Top = 60, AutoSize = true },
                togAudio,
                new Label() { Text = "Audio Doble",       Font = new Font("Segoe UI", 10f), ForeColor = textPrimary, Left = 24, Top = 110, AutoSize = true },
                new Label() { Text = "Android 13+ | Usa audífonos para evitar eco", Font = new Font("Segoe UI", 8f), ForeColor = textSecondary, Left = 24, Top = 132, AutoSize = true },
                togAudioDoble,
                new Label() { Text = "Audio Codec",       Font = new Font("Segoe UI", 10f), ForeColor = textPrimary, Left = 24, Top = 158, AutoSize = true },
                new Label() { Text = "Usa aac si opus falla en tu dispositivo", Font = new Font("Segoe UI", 8f), ForeColor = textSecondary, Left = 24, Top = 176, AutoSize = true },
                cmbAudioCodec,
                new Label() { Text = "Bitrate (Kbps)",    Font = new Font("Segoe UI", 10f), ForeColor = textPrimary, Left = 24, Top = 236, AutoSize = true },
                new Label() { Text = "Default: 128 Kbps", Font = new Font("Segoe UI", 8f), ForeColor = textSecondary, Left = 24, Top = 256, AutoSize = true },
                numAudioBitrate,
                new Label() { Text = "Audio Buffer (ms)", Font = new Font("Segoe UI", 10f), ForeColor = textPrimary, Left = 24, Top = 296, AutoSize = true },
                new Label() { Text = "Default: 50 ms. Aumentar si hay cortes de audio.", Font = new Font("Segoe UI", 8f), ForeColor = textSecondary, Left = 24, Top = 318, AutoSize = true },
                numAudioBuffer
                });

                // ── CARD: Encoder Avanzado ────────────────────────────────
                var cardEncoder = CreateCard("Encoder Avanzado", 30, 930, 210);

                var togEncoder = new Guna2ToggleSwitch()
                {
                    Left = cardEncoder.Width - 70,
                    Top = 48,
                    Checked = _useAdvancedEncoder,
                    CheckedState = { FillColor = accentColor },
                    UncheckedState = { FillColor = Color.FromArgb(60, 60, 60) },
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };

                var cmbEncoders = new Guna2ComboBox()
                {
                    Left = 90,
                    Top = 110,
                    Width = cardEncoder.Width - 90 - 136,
                    Height = 34,
                    FillColor = Color.FromArgb(42, 42, 45),
                    ForeColor = textPrimary,
                    BorderColor = Color.FromArgb(60, 60, 60),
                    BorderRadius = 4,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = new Font("Segoe UI", 8.5f),
                    Enabled = _useAdvancedEncoder,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };

                var btnDetectarEncoders = new Guna2Button()
                {
                    Text = "🔄 Detectar",
                    Width = 120,
                    Height = 34,
                    Left = cardEncoder.Width - 144,
                    Top = 110,
                    Font = new Font("Segoe UI", 9f),
                    FillColor = accentColor,
                    ForeColor = Color.White,
                    BorderRadius = 4,
                    Enabled = _useAdvancedEncoder,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };

                var lblEncoderStatus = new Label()
                {
                    Text = "",
                    Font = new Font("Segoe UI", 8f),
                    ForeColor = textSecondary,
                    Left = 24,
                    Top = 156,
                    Width = cardEncoder.Width - 48,
                    Height = 18,
                    AutoSize = false,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };

                var lblEncoderActivo = new Label()
                {
                    Text = _useAdvancedEncoder && !string.IsNullOrEmpty(_videoEncoder)
                        ? $"✓ Encoder: {_videoEncoder}  |  Codec: {InferirCodecDeEncoderUI(_videoEncoder)}"
                        : "Encoder avanzado desactivado — usando codec genérico",
                    Font = new Font("Segoe UI", 8f, FontStyle.Italic),
                    ForeColor = _useAdvancedEncoder && !string.IsNullOrEmpty(_videoEncoder)
                        ? Color.FromArgb(16, 124, 16) : textSecondary,
                    Left = 24,
                    Top = 178,
                    Width = cardEncoder.Width - 48,
                    Height = 18,
                    AutoSize = false,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };

                // ── Llenar combo al cargar usando la lista persistida ─────
                // _encodersDetectados persiste entre navegaciones (campo de Form1)
                // Si ya había detectado antes, recarga la lista completa sin volver a detectar
                if (_encodersDetectados.Count > 0)
                {
                    cmbEncoders.Items.Clear();
                    foreach (var lbl in _encodersDisplayLabels) cmbEncoders.Items.Add(lbl);
                    int idx = _encodersDetectados.IndexOf(_videoEncoder ?? "");
                    cmbEncoders.SelectedIndex = idx >= 0 ? idx : 0;
                    lblEncoderStatus.Text = $"✓ {_encodersDetectados.Count} encoder(s) disponibles";
                    lblEncoderStatus.ForeColor = Color.FromArgb(16, 124, 16);
                }
                else if (!string.IsNullOrEmpty(_videoEncoder))
                {
                    // Primera carga con encoder guardado: mostrar solo el activo con etiqueta inferida
                    string tipo = InferirTipoEncoder(_videoEncoder);
                    string codec = InferirCodecDeEncoderUI(_videoEncoder);
                    cmbEncoders.Items.Add($"{_videoEncoder}  [{tipo}] [{codec}]");
                    cmbEncoders.SelectedIndex = 0;
                    lblEncoderStatus.Text = "Presiona Detectar para ver todos los encoders disponibles";
                    lblEncoderStatus.ForeColor = textSecondary;
                }
                else
                {
                    cmbEncoders.Items.Add("— Presiona Detectar —");
                    cmbEncoders.SelectedIndex = 0;
                }

                togEncoder.CheckedChanged += (s, e) =>
                {
                    _useAdvancedEncoder = togEncoder.Checked;
                    cmbEncoders.Enabled = _useAdvancedEncoder;
                    btnDetectarEncoders.Enabled = _useAdvancedEncoder;
                    cmbCodec.Enabled = !_useAdvancedEncoder;
                    lblCodecDesactivado.Visible = _useAdvancedEncoder;
                    cmbCodec.FillColor = !_useAdvancedEncoder
                        ? (Color.FromArgb(42, 42, 45))
                        : (Color.FromArgb(20, 20, 20));

                    if (!_useAdvancedEncoder)
                    {
                        _videoEncoder = "";
                        lblEncoderStatus.Text = "";
                        lblEncoderActivo.Text = "Encoder avanzado desactivado — usando codec genérico";
                        lblEncoderActivo.ForeColor = textSecondary;
                    }
                    else
                    {
                        lblEncoderStatus.Text = _encodersDetectados.Count > 0
                            ? $"✓ {_encodersDetectados.Count} encoder(s) disponibles"
                            : "Presiona Detectar para cargar los encoders del dispositivo";
                        lblEncoderStatus.ForeColor = _encodersDetectados.Count > 0
                            ? Color.FromArgb(16, 124, 16) : textSecondary;
                        lblEncoderActivo.Text = string.IsNullOrEmpty(_videoEncoder)
                            ? "⚠ Selecciona un encoder después de detectar"
                            : $"✓ Encoder: {_videoEncoder}  |  Codec: {InferirCodecDeEncoderUI(_videoEncoder)}";
                        lblEncoderActivo.ForeColor = string.IsNullOrEmpty(_videoEncoder)
                            ? Color.FromArgb(255, 167, 38) : Color.FromArgb(16, 124, 16);
                    }

                    if (!_cargandoPagina) MarcarCambiosSinGuardar();
                };

                btnDetectarEncoders.Click += async (s, e) =>
                {
                    btnDetectarEncoders.Enabled = false;
                    btnDetectarEncoders.Text = "Detectando...";
                    lblEncoderStatus.Text = "⏳ Consultando encoders...";
                    lblEncoderStatus.ForeColor = Color.FromArgb(255, 167, 38);
                    try {
                    var (exito, nombres, displayLabels, rawOutput) = await scrcpyManager.DetectarEncodersAsync();
                    if (IsDisposed) return;

                    if (exito && nombres.Count > 0)
                    {
                        // Persistir en campos de clase para que sobrevivan la navegación
                        _encodersDetectados = nombres;
                        _encodersDisplayLabels = displayLabels;

                        cmbEncoders.Items.Clear();
                        foreach (var lbl in displayLabels) cmbEncoders.Items.Add(lbl);

                        int prevIdx = nombres.IndexOf(_videoEncoder ?? "");
                        cmbEncoders.SelectedIndex = prevIdx >= 0 ? prevIdx : 0;

                        lblEncoderStatus.Text = $"✓ {nombres.Count} encoder(s) detectado(s)";
                        lblEncoderStatus.ForeColor = Color.FromArgb(16, 124, 16);
                    }
                    else
                    {
                        lblEncoderStatus.Text = "⚠ Sin encoders — verifica que el dispositivo esté conectado";
                        lblEncoderStatus.ForeColor = Color.FromArgb(255, 167, 38);

                        var diagResult = MessageBox.Show(
                            "No se pudieron detectar encoders.\n\n¿Ver el output de scrcpy para depurar?",
                            "Diagnóstico Encoders", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (diagResult == DialogResult.Yes)
                        {
                            using var diagForm = new Form()
                            {
                                Text = "Output: scrcpy --list-encoders",
                                Width = 640,
                                Height = 400,
                                StartPosition = FormStartPosition.CenterParent,
                                BackColor = Color.FromArgb(20, 20, 20)
                            };
                            var txt = new TextBox()
                            {
                                Multiline = true,
                                ReadOnly = true,
                                ScrollBars = ScrollBars.Both,
                                Dock = DockStyle.Fill,
                                BackColor = Color.FromArgb(30, 30, 30),
                                ForeColor = Color.FromArgb(0, 220, 0),
                                Font = new Font("Consolas", 9f),
                                Text = string.IsNullOrWhiteSpace(rawOutput) ? "(Output vacío)" : rawOutput
                            };
                            diagForm.Controls.Add(txt);
                            diagForm.ShowDialog(this);
                        }
                    }

                    } finally {
                        if (!IsDisposed) { btnDetectarEncoders.Text = "🔄 Detectar"; btnDetectarEncoders.Enabled = true; }
                    }
                };

                cmbEncoders.SelectedIndexChanged += (s, e) =>
                {
                    int idx = cmbEncoders.SelectedIndex;
                    // Usar lista persistida si está disponible, sino usar SelectedItem
                    string sel = (_encodersDetectados.Count > idx && idx >= 0)
                        ? _encodersDetectados[idx]
                        : cmbEncoders.SelectedItem?.ToString() ?? "";

                    // Ignorar placeholders
                    if (string.IsNullOrWhiteSpace(sel) || sel.StartsWith("—") || sel.StartsWith("⏳")) return;

                    // Si sel es un display label (contiene "["), extraer solo el nombre
                    if (sel.Contains("["))
                        sel = sel.Split(new[] { "  [" }, StringSplitOptions.None)[0].Trim();

                    if (string.IsNullOrWhiteSpace(sel)) return;

                    _videoEncoder = sel;
                    lblEncoderActivo.Text = $"✓ Encoder: {sel}  |  Codec: {InferirCodecDeEncoderUI(sel)}";
                    lblEncoderActivo.ForeColor = Color.FromArgb(16, 124, 16);
                    if (!_cargandoPagina) MarcarCambiosSinGuardar();
                };

                cardEncoder.Controls.AddRange(new Control[]
                {
                new Label() { Text = "Activar Encoder Avanzado", Font = new Font("Segoe UI", 10f), ForeColor = textPrimary, Left = 24, Top = 50, AutoSize = true },
                togEncoder,
                new Label() { Text = "Usa un encoder específico del dispositivo cuando el codec genérico tiene problemas.", Font = new Font("Segoe UI", 8.5f), ForeColor = textSecondary, Left = 24, Top = 68, Width = cardEncoder.Width - 48, AutoSize = false, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right },
                new Label() { Text = "Encoder:", Font = new Font("Segoe UI", 9.5f), ForeColor = textPrimary, Left = 24, Top = 118, AutoSize = true },
                cmbEncoders, btnDetectarEncoders,
                lblEncoderStatus, lblEncoderActivo
                });

                contentPanel.Controls.AddRange(new Control[]
                {
                cardVideo, cardAudio, cardEncoder
                });

            }
            finally { _cargandoPagina = false; }
        }

        // ── HELPERS PRIVADOS ─────────────────────────────────────────

        // Inferir hw/sw del nombre del encoder
        // hw: chips de fabricante (exynos, qcom, mtk, mediatek, mali, vendor)
        // sw: implementaciones de software (android, google, omx.google)
        private string InferirTipoEncoder(string encoderName)
        {
            if (string.IsNullOrEmpty(encoderName)) return "sw";
            string lower = encoderName.ToLower();
            if (lower.Contains("exynos") || lower.Contains("qcom") ||
                lower.Contains("mtk") || lower.Contains("mediatek") ||
                lower.Contains("mali") || lower.Contains("vendor"))
                return "hw";
            return "sw";
        }

        // Inferir codec del nombre del encoder
        private string InferirCodecDeEncoderUI(string encoderName)
        {
            if (string.IsNullOrEmpty(encoderName)) return "h264";
            string lower = encoderName.ToLower();
            if (lower.Contains("hevc") || lower.Contains("h265") || lower.Contains("h.265")) return "h265";
            if (lower.Contains("av01") || lower.Contains("av1")) return "av1";
            return "h264";
        }
    }
}