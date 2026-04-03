using Guna.UI2.WinForms;
using LyXel.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LyXel
{
    public partial class Form1
    {
        // Estado persistente de optimizaciones — se guarda en config.ini [optimizacion_estado]
        private Dictionary<string, bool> _optimizacionEstado = new();
        // Referencia al label de advertencia; se actualiza cuando cambia _hayDispositivo
        private Label? _lblOptAdvertencia;

        /// <summary>Muestra u oculta el label de advertencia según el estado de dispositivo y toggles.</summary>
        private void ActualizarLabelOptAdvertencia()
        {
            if (_lblOptAdvertencia == null || _lblOptAdvertencia.IsDisposed) return;
            bool hayActivos = _optimizacionEstado.Values.Any(v => v);
            _lblOptAdvertencia.Visible = hayActivos && !_hayDispositivo;
        }

        private void LoadOptimizacionPage()
        {
            // ── Guardia de aceptación ────────────────────────────────────────────
            if (!_optimizacionAceptada)
            {
                using var aviso = new DialogoAvanzado(
                    "Zona de Optimización",
                    "Estos comandos modifican configuraciones internas de Android. Se han probado en varios dispositivos pero pueden no funcionar en todos. Si no sabes qué hace cada opción, mejor no la actives. Al continuar aceptas que es bajo tu propia responsabilidad.",
                    new[] { "Entiendo que estas opciones modifican configuraciones internas de Android y las activo bajo mi propia responsabilidad." });

                if (aviso.ShowDialog(this) != System.Windows.Forms.DialogResult.OK)
                {
                    LoadPage(0, "Inicio", LoadInicioPage);
                    return;
                }

                _optimizacionAceptada = true;
                if (aviso.NoVolverMostrar)
                    GuardarConfigTema();
            }

            int cardLeft  = S(30);
            int cardY     = S(20);
            int headerH   = S(54);
            int rowH      = S(70);
            int comboRowH = S(95);
            int btnRowH   = S(70); // aumentado de S(56) para alojar el label de comando
            int cardPad   = S(20);

            // Registro de toggles para "Revertir todo": (toggle, clave, comando OFF)
            var toggleRegistry = new List<(Guna2ToggleSwitch tog, string key, Func<Task> offCmd)>();

            bool revirtiendoTodo = false;
            int  fallosRevert    = 0;

            // ── Ejecuta un comando shell con verificación de dispositivo ─────────
            async Task EjecutarOpt(string cmd, Guna2ToggleSwitch? togOrigen = null)
            {
                if (!_hayDispositivo)
                {
                    if (!IsDisposed)
                    {
                        ToastNotification.Mostrar(this,
                            "Conecta tu teléfono antes de aplicar optimizaciones.",
                            ToastNotification.ToastTipo.Advertencia);
                        if (togOrigen != null)
                            togOrigen.Checked = false;
                    }
                    return;
                }

                var (exito, _, stderr) = await Task.Run(() => adbManager.EjecutarShell(cmd));

                if (exito)
                {
                    if (!revirtiendoTodo && togOrigen != null && !IsDisposed)
                    {
                        string msg = togOrigen.Checked
                            ? "Optimización aplicada correctamente."
                            : "Optimización restablecida.";
                        ToastNotification.Mostrar(this, msg, ToastNotification.ToastTipo.Exito);
                    }
                    return;
                }

                if (IsDisposed) return;

                if (togOrigen != null)
                {
                    string msg;
                    if (stderr.Contains("SecurityException") || stderr.Contains("Permission denial"))
                        msg = "Este comando requiere permisos que tu versión de Android no permite. Prueba en Android 12 o anterior.";
                    else if (stderr.Contains("not found") || stderr.Contains("Unknown command"))
                        msg = "Este comando no es compatible con tu dispositivo o versión de Android.";
                    else
                        msg = "No se pudo aplicar esta optimización en tu dispositivo.";

                    ToastNotification.Mostrar(this, msg, ToastNotification.ToastTipo.Error);
                    togOrigen.Checked = false;
                }
                else if (revirtiendoTodo)
                {
                    fallosRevert++;
                }
            }

            // ── Fila de toggle ───────────────────────────────────────────────────
            void AddToggleRow(Panel card, ref int y, string key, string titulo, string desc, string cmdON,
                Func<Guna2ToggleSwitch, Task> onEnable, Func<Guna2ToggleSwitch, Task> onDisable)
            {
                int rowWidth = card.Width - S(48);

                card.Controls.Add(new Label()
                {
                    Text      = titulo,
                    Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    ForeColor = textPrimary,
                    Left = S(24), Top = y,
                    Width = rowWidth - S(64), Height = S(22),
                    AutoSize = false
                });
                card.Controls.Add(new Label()
                {
                    Text      = desc,
                    Font      = new Font("Segoe UI", 8.5f),
                    ForeColor = textSecondary,
                    Left = S(24), Top = y + S(22),
                    Width = rowWidth - S(64), Height = S(30),
                    AutoSize = false
                });
                card.Controls.Add(new Label()
                {
                    Text      = $"cmd: {cmdON}",
                    Font      = new Font("Consolas", 7.5f),
                    ForeColor = Color.FromArgb(160, 160, 160),
                    Left = S(24), Top = y + S(52),
                    Width = rowWidth - S(64), Height = S(16),
                    AutoSize = false
                });

                bool initialChecked = _optimizacionEstado.TryGetValue(key, out bool sv) && sv;
                var tog = new Guna2ToggleSwitch()
                {
                    Left    = card.Width - S(64),
                    Top     = y + S(16),
                    Checked = initialChecked,
                    Anchor  = AnchorStyles.Top | AnchorStyles.Right
                };
                tog.CheckedState.FillColor   = AppTheme.Accent;
                tog.UncheckedState.FillColor = AppTheme.BtnDisabled;

                bool enProgreso = false;
                tog.CheckedChanged += async (s, e) =>
                {
                    if (enProgreso || revirtiendoTodo) return;
                    enProgreso  = true;
                    tog.Enabled = false;
                    try
                    {
                        if (tog.Checked) await onEnable(tog);
                        else             await onDisable(tog);
                    }
                    finally
                    {
                        enProgreso = false;
                        if (!IsDisposed)
                        {
                            tog.Enabled = true;
                            _optimizacionEstado[key] = tog.Checked;
                            GuardarConfigTema();
                            ActualizarLabelOptAdvertencia();
                        }
                    }
                };
                card.Controls.Add(tog);

                toggleRegistry.Add((tog, key, () => onDisable(null)));
                y += rowH;
            }

            // ── Fila de ComboBox + Aplicar + Resetear ────────────────────────────
            void AddComboRow(Panel card, ref int y, string titulo, string desc, string cmdLabel,
                string[] opciones, int defaultIndex,
                Func<string, Task> onAplicar, Func<Task> onResetear)
            {
                int rowWidth = card.Width - S(48);

                card.Controls.Add(new Label()
                {
                    Text      = titulo,
                    Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    ForeColor = textPrimary,
                    Left = S(24), Top = y,
                    Width = rowWidth, Height = S(18),
                    AutoSize = false
                });
                card.Controls.Add(new Label()
                {
                    Text      = desc,
                    Font      = new Font("Segoe UI", 8.5f),
                    ForeColor = textSecondary,
                    Left = S(24), Top = y + S(18),
                    Width = rowWidth, Height = S(16),
                    AutoSize = false
                });

                var cmb = new Guna2ComboBox()
                {
                    Left            = S(30),
                    Top             = y + S(46),
                    Width           = S(200),
                    Height          = S(32),
                    Font            = new Font("Segoe UI", 9f),
                    FillColor       = AppTheme.BgCard,
                    ForeColor       = AppTheme.TextPrimary,
                    BorderColor     = AppTheme.BorderSecondary,
                    BorderThickness = 1,
                    BorderRadius    = 4,
                    DropDownWidth   = S(180),
                    TabStop         = false,
                    Anchor          = AnchorStyles.Top | AnchorStyles.Left
                };
                cmb.Items.AddRange(opciones);
                cmb.SelectedIndex = defaultIndex;

                var btnAplicar = new Guna2Button()
                {
                    Text            = "Aplicar",
                    Left            = S(240),
                    Top             = y + S(48),
                    Width           = S(96),
                    Height          = S(30),
                    Font            = new Font("Segoe UI", 8.5f),
                    FillColor       = AppTheme.Accent,
                    ForeColor       = AppTheme.TextPrimary,
                    BorderRadius    = 4,
                    Anchor          = AnchorStyles.Top | AnchorStyles.Left
                };
                btnAplicar.Click += async (s, e) =>
                {
                    if (!_hayDispositivo)
                    {
                        ToastNotification.Mostrar(this,
                            "Conecta tu teléfono antes de aplicar optimizaciones.",
                            ToastNotification.ToastTipo.Advertencia);
                        return;
                    }
                    btnAplicar.Enabled = false;
                    string textoOrig   = btnAplicar.Text;
                    btnAplicar.Text    = "...";
                    string valor = (cmb.SelectedItem?.ToString() ?? opciones[defaultIndex]).Split(' ')[0];
                    try   { await onAplicar(valor); }
                    finally
                    {
                        if (!IsDisposed)
                        {
                            btnAplicar.Text    = textoOrig;
                            btnAplicar.Enabled = true;
                        }
                    }
                };

                var btnResetear = new Guna2Button()
                {
                    Text            = "Resetear",
                    Left            = S(340),
                    Top             = y + S(48),
                    Width           = S(96),
                    Height          = S(30),
                    Font            = new Font("Segoe UI", 8.5f),
                    FillColor       = AppTheme.BtnSecondary,
                    ForeColor       = AppTheme.TextSecondary,
                    BorderColor     = AppTheme.BorderSecondary,
                    BorderThickness = 1,
                    BorderRadius    = 4,
                    Anchor          = AnchorStyles.Top | AnchorStyles.Left
                };
                btnResetear.Click += async (s, e) =>
                {
                    if (!_hayDispositivo)
                    {
                        ToastNotification.Mostrar(this,
                            "Conecta tu teléfono antes de aplicar optimizaciones.",
                            ToastNotification.ToastTipo.Advertencia);
                        return;
                    }
                    btnResetear.Enabled = false;
                    string textoOrig    = btnResetear.Text;
                    btnResetear.Text    = "...";
                    try   { await onResetear(); }
                    finally
                    {
                        if (!IsDisposed)
                        {
                            btnResetear.Text    = textoOrig;
                            btnResetear.Enabled = true;
                            cmb.SelectedIndex   = defaultIndex;
                        }
                    }
                };

                card.Controls.Add(new Label()
                {
                    Text      = $"cmd: {cmdLabel}",
                    Font      = new Font("Consolas", 7.5f),
                    ForeColor = Color.FromArgb(160, 160, 160),
                    Left = S(30), Top = y + S(80),
                    Width = rowWidth, Height = S(14),
                    AutoSize = false
                });

                card.Controls.AddRange(new Control[] { cmb, btnAplicar, btnResetear });
                y += comboRowH;
            }

            // ── Fila de botón simple ─────────────────────────────────────────────
            void AddButtonRow(Panel card, ref int y, string titulo, string desc,
                string btnText, string cmdLabel, Func<Task> accion)
            {
                int rowWidth = card.Width - S(48);
                card.Controls.Add(new Label()
                {
                    Text      = titulo,
                    Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    ForeColor = textPrimary,
                    Left = S(24), Top = y,
                    Width = rowWidth - S(160), Height = S(22),
                    AutoSize = false
                });
                card.Controls.Add(new Label()
                {
                    Text      = desc,
                    Font      = new Font("Segoe UI", 8.5f),
                    ForeColor = textSecondary,
                    Left = S(24), Top = y + S(22),
                    Width = rowWidth - S(160), Height = S(30),
                    AutoSize = false
                });
                card.Controls.Add(new Label()
                {
                    Text      = $"cmd: {cmdLabel}",
                    Font      = new Font("Consolas", 7.5f),
                    ForeColor = Color.FromArgb(160, 160, 160),
                    Left = S(24), Top = y + S(52),
                    Width = rowWidth - S(160), Height = S(14),
                    AutoSize = false
                });

                var btn = new Guna2Button()
                {
                    Text         = btnText,
                    Left         = card.Width - S(164),
                    Top          = y + S(10),
                    Width        = S(140),
                    Height       = S(34),
                    Font         = new Font("Segoe UI", 8.5f),
                    FillColor    = AppTheme.Accent,
                    ForeColor    = AppTheme.TextPrimary,
                    BorderRadius = 4,
                    Anchor       = AnchorStyles.Top | AnchorStyles.Right
                };
                btn.Click += async (s, e) =>
                {
                    if (!_hayDispositivo)
                    {
                        ToastNotification.Mostrar(this,
                            "Conecta tu teléfono antes de aplicar optimizaciones.",
                            ToastNotification.ToastTipo.Advertencia);
                        return;
                    }
                    btn.Enabled = false;
                    string textoOriginal = btn.Text;
                    btn.Text = "Ejecutando...";
                    try   { await accion(); }
                    finally
                    {
                        if (!IsDisposed)
                        {
                            btn.Text    = textoOriginal;
                            btn.Enabled = true;
                        }
                    }
                };
                card.Controls.Add(btn);
                y += btnRowH;
            }

            // ── Botón "Revertir todo" + label de advertencia ─────────────────────
            var btnRevertirTodo = new Guna2Button()
            {
                Text            = "  Revertir todo",
                Image           = IconMap.History,
                ImageSize       = new Size(S(18), S(18)),
                ImageAlign      = HorizontalAlignment.Left,
                Left            = cardLeft,
                Top             = cardY,
                Width           = S(170),
                Height          = S(36),
                Font            = new Font("Segoe UI", 9f),
                FillColor       = AppTheme.BtnSecondary,
                ForeColor       = textSecondary,
                BorderColor     = AppTheme.BorderNeutral,
                BorderThickness = 1,
                BorderRadius    = 4
            };

            bool showWarning = !_hayDispositivo && _optimizacionEstado.Values.Any(v => v);
            var lblAdvertencia = new Label()
            {
                Text      = "⚠ Tienes optimizaciones activas. Conecta tu teléfono para revertirlas.",
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = AppTheme.WarningText,
                Left      = cardLeft + S(185),
                Top       = cardY,
                Width     = contentPanel.Width - cardLeft - S(215),
                Height    = S(36),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize  = false,
                Visible   = showWarning
            };
            _lblOptAdvertencia = lblAdvertencia;

            btnRevertirTodo.Click += async (s, e) =>
            {
                if (!_hayDispositivo)
                {
                    ToastNotification.Mostrar(this,
                        "Conecta tu teléfono antes de revertir.",
                        ToastNotification.ToastTipo.Advertencia);
                    return;
                }

                var activos = toggleRegistry.Where(t => t.tog.Checked).ToList();
                if (activos.Count == 0)
                {
                    ToastNotification.Mostrar(this,
                        "No hay optimizaciones activas para revertir.",
                        ToastNotification.ToastTipo.Info);
                    return;
                }

                btnRevertirTodo.Enabled = false;
                btnRevertirTodo.Text    = "Revirtiendo...";
                revirtiendoTodo         = true;
                fallosRevert            = 0;
                int togglesFallidos     = 0;
                int total               = activos.Count;
                try
                {
                    foreach (var (_, _, offCmd) in activos)
                    {
                        int fallosAntes = fallosRevert;
                        await offCmd();
                        if (fallosRevert > fallosAntes) togglesFallidos++;
                    }

                    foreach (var (tog, key, _) in activos)
                    {
                        if (!IsDisposed)
                        {
                            tog.Checked = false;
                            _optimizacionEstado[key] = false;
                        }
                    }
                    if (!IsDisposed) GuardarConfigTema();
                }
                finally
                {
                    revirtiendoTodo = false;
                    if (!IsDisposed)
                    {
                        btnRevertirTodo.Text    = "  Revertir todo";
                        btnRevertirTodo.Enabled = true;
                        ActualizarLabelOptAdvertencia();
                    }
                }
                if (!IsDisposed)
                {
                    if (togglesFallidos == 0)
                        ToastNotification.Mostrar(this,
                            "Todas las optimizaciones han sido revertidas.",
                            ToastNotification.ToastTipo.Exito);
                    else
                        ToastNotification.Mostrar(this,
                            $"Se revirtieron {total - togglesFallidos} de {total} optimizaciones. Algunas pueden requerir reiniciar el dispositivo.",
                            ToastNotification.ToastTipo.Advertencia);
                }
            };
            cardY += S(36) + S(14);

            // ════════════════════════════════════════════════════════════════════
            // CARD 1 — Rendimiento General
            // toggles: Fixed, Animaciones, GameDriver, Blur, VSync, OpenGL = 6×rowH
            // combos: Tasa refresco, FPS mínimo = 2×comboRowH
            // buttons: LimpiarRAM, LimpiarCache, ART, DEXOPT = 4×btnRowH
            // ════════════════════════════════════════════════════════════════════
            int c1y         = headerH;
            int card1Height = headerH + rowH * 6 + comboRowH * 2 + btnRowH * 4 + cardPad;
            var card1 = CreateCard("Rendimiento General", cardLeft, cardY, card1Height);

            AddToggleRow(card1, ref c1y,
                key: "fixed_perf",
                titulo: "Fixed Performance Mode",
                desc: "Fija CPU/GPU al máximo, elimina throttling térmico. Úsalo en sesiones cortas.",
                cmdON: "cmd power set-fixed-performance-mode-enabled true",
                onEnable:  (t) => EjecutarOpt("cmd power set-fixed-performance-mode-enabled true",  t),
                onDisable: (t) => EjecutarOpt("cmd power set-fixed-performance-mode-enabled false", t));

            AddToggleRow(card1, ref c1y,
                key: "animaciones",
                titulo: "Animaciones reducidas",
                desc: "Hace el sistema más ágil y responsivo.",
                cmdON: "settings put global window_animation_scale 0",
                onEnable: async (t) =>
                {
                    await EjecutarOpt("settings put global window_animation_scale 0",    t);
                    await EjecutarOpt("settings put global transition_animation_scale 0", t);
                    await EjecutarOpt("settings put global animator_duration_scale 0",    t);
                },
                onDisable: async (t) =>
                {
                    await EjecutarOpt("settings put global window_animation_scale 1",    t);
                    await EjecutarOpt("settings put global transition_animation_scale 1", t);
                    await EjecutarOpt("settings put global animator_duration_scale 1",    t);
                });

            AddComboRow(card1, ref c1y,
                titulo:       "Tasa de refresco máxima",
                desc:         "Evita que el sistema baje la tasa de refresco para ahorrar batería.",
                cmdLabel:     "settings put system peak_refresh_rate {valor}",
                opciones:     new[] { "30", "60", "90", "120", "144", "240" },
                defaultIndex: 1, // 60
                onAplicar: async (v) =>
                {
                    await EjecutarOpt($"settings put system peak_refresh_rate {v}");
                    await EjecutarOpt($"settings put system min_refresh_rate {v}");
                },
                onResetear: async () =>
                {
                    await EjecutarOpt("settings put system peak_refresh_rate 60");
                    await EjecutarOpt("settings put system min_refresh_rate 60");
                });

            AddComboRow(card1, ref c1y,
                titulo:       "FPS mínimo del sistema",
                desc:         "Evita que el sistema baje de este valor. Elige el máximo que soporte tu pantalla.",
                cmdLabel:     "setprop debug.refresh_rate.min_fps {valor}",
                opciones:     new[] { "0 (sin límite)", "30", "60", "90", "120", "144", "240" },
                defaultIndex: 0, // 0 (sin límite)
                onAplicar: async (v) =>
                {
                    await EjecutarOpt($"setprop debug.refresh_rate.min_fps {v}");
                },
                onResetear: async () =>
                {
                    await EjecutarOpt("setprop debug.refresh_rate.min_fps 0");
                });

            AddToggleRow(card1, ref c1y,
                key: "game_driver",
                titulo: "Game Driver universal",
                desc: "Usa drivers gráficos optimizados en todas las apps.",
                cmdON: "settings put global game_driver_all_apps 1",
                onEnable:  (t) => EjecutarOpt("settings put global game_driver_all_apps 1", t),
                onDisable: (t) => EjecutarOpt("settings put global game_driver_all_apps 0", t));

            AddToggleRow(card1, ref c1y,
                key: "blur",
                titulo: "Desactivar blur de ventanas",
                desc: "Elimina efectos de desenfoque que consumen GPU (Android 15+).",
                cmdON: "settings put global disable_window_blurs 1",
                onEnable:  (t) => EjecutarOpt("settings put global disable_window_blurs 1", t),
                onDisable: (t) => EjecutarOpt("settings put global disable_window_blurs 0", t));

            AddToggleRow(card1, ref c1y,
                key: "vsync",
                titulo: "Forzar VSync por GPU",
                desc: "Elimina el tearing visual forzando sincronización vertical por GPU.",
                cmdON: "setprop debug.hwc.force_gpu_vsync 1",
                onEnable:  (t) => EjecutarOpt("setprop debug.hwc.force_gpu_vsync 1", t),
                onDisable: (t) => EjecutarOpt("setprop debug.hwc.force_gpu_vsync 0", t));

            AddToggleRow(card1, ref c1y,
                key: "opengl",
                titulo: "Forzar renderizado OpenGL",
                desc: "Útil en dispositivos con Android 12 o anterior que tienen problemas con Vulkan.",
                cmdON: "setprop debug.force-opengl 1",
                onEnable:  (t) => EjecutarOpt("setprop debug.force-opengl 1", t),
                onDisable: (t) => EjecutarOpt("setprop debug.force-opengl 0", t));

            AddButtonRow(card1, ref c1y,
                titulo:   "Limpieza de procesos",
                desc:     "Libera RAM cerrando procesos en segundo plano.",
                btnText:  "Limpiar RAM",
                cmdLabel: "am kill-all",
                accion:   () => EjecutarOpt("am kill-all"));

            AddButtonRow(card1, ref c1y,
                titulo:   "Limpiar caché de apps",
                desc:     "Libera caché acumulada de todas las apps.",
                btnText:  "Limpiar caché",
                cmdLabel: "pm trim-caches 999G",
                accion:   () => EjecutarOpt("pm trim-caches 999G"));

            AddButtonRow(card1, ref c1y,
                titulo:   "Optimizar compilación ART",
                desc:     "Recompila las apps según tu uso para abrirlas más rápido. Tarda unos segundos.",
                btnText:  "Optimizar",
                cmdLabel: "cmd package compile -m speed-profile -a",
                accion:   () => EjecutarOpt("cmd package compile -m speed-profile -a"));

            AddButtonRow(card1, ref c1y,
                titulo:   "Optimización DEXOPT",
                desc:     "Optimización en segundo plano que Android hace normalmente en reposo.",
                btnText:  "Ejecutar",
                cmdLabel: "pm bg-dexopt-job",
                accion:   () => EjecutarOpt("pm bg-dexopt-job"));

            cardY += card1Height + S(20);

            // ════════════════════════════════════════════════════════════════════
            // CARD 2 — Gaming — Free Fire
            // ════════════════════════════════════════════════════════════════════
            int c2y         = headerH;
            int card2Height = headerH + rowH * 3 + btnRowH * 1 + cardPad;
            var card2 = CreateCard("Gaming — Free Fire", cardLeft, cardY, card2Height);

            AddToggleRow(card2, ref c2y,
                key: "ff_game_mode",
                titulo: "Game Mode Performance",
                desc: "Prioriza rendimiento sobre batería para Free Fire.",
                cmdON: "cmd game mode performance com.dts.freefireth",
                onEnable:  (t) => EjecutarOpt("cmd game mode performance com.dts.freefireth", t),
                onDisable: (t) => EjecutarOpt("cmd game mode standard com.dts.freefireth",    t));

            AddToggleRow(card2, ref c2y,
                key: "ff_bg",
                titulo: "Bloquear background en partida",
                desc: "Impide que otras apps consuman CPU mientras juegas.",
                cmdON: "cmd appops set com.dts.freefireth RUN_ANY_IN_BACKGROUND deny",
                onEnable:  (t) => EjecutarOpt("cmd appops set com.dts.freefireth RUN_ANY_IN_BACKGROUND deny",  t),
                onDisable: (t) => EjecutarOpt("cmd appops set com.dts.freefireth RUN_ANY_IN_BACKGROUND allow", t));

            AddToggleRow(card2, ref c2y,
                key: "ff_mem",
                titulo: "Bloquear Free Fire en memoria",
                desc: "Evita que el sistema cierre el juego al cambiar de app.",
                cmdON: "am set-standby-bucket com.dts.freefireth active",
                onEnable: async (t) =>
                {
                    await EjecutarOpt("am set-standby-bucket com.dts.freefireth active",  t);
                    await EjecutarOpt("am set-standby-bucket com.dts.freefiremax active", t);
                },
                onDisable: async (t) =>
                {
                    await EjecutarOpt("am set-standby-bucket com.dts.freefireth working_set",  t);
                    await EjecutarOpt("am set-standby-bucket com.dts.freefiremax working_set", t);
                });

            AddButtonRow(card2, ref c2y,
                titulo:   "Compilar Free Fire (velocidad)",
                desc:     "Compila el juego para tu procesador, mejora tiempos de carga.",
                btnText:  "Compilar",
                cmdLabel: "pm compile -m speed -f com.dts.freefireth",
                accion:   async () =>
                {
                    await EjecutarOpt("pm compile -m speed -f com.dts.freefireth");
                    await EjecutarOpt("pm compile -m speed -f com.dts.freefiremax");
                });

            cardY += card2Height + S(20);

            // ════════════════════════════════════════════════════════════════════
            // CARD 3 — Samsung
            // ════════════════════════════════════════════════════════════════════
            int c3y         = headerH;
            int card3Height = headerH + rowH * 4 + cardPad;
            var card3 = CreateCard("Samsung", cardLeft, cardY, card3Height);

            AddToggleRow(card3, ref c3y,
                key: "samsung_hd",
                titulo: "Audio HD (K2HD)",
                desc: "Mejora calidad de audio en dispositivos Samsung compatibles.",
                cmdON: "settings put global k2hd_effect 1",
                onEnable:  (t) => EjecutarOpt("settings put global k2hd_effect 1", t),
                onDisable: (t) => EjecutarOpt("settings put global k2hd_effect 0", t));

            AddToggleRow(card3, ref c3y,
                key: "samsung_amp",
                titulo: "Tube Amp Effect",
                desc: "Efecto de amplificador de audio Samsung.",
                cmdON: "settings put global tube_amp_effect 1",
                onEnable:  (t) => EjecutarOpt("settings put global tube_amp_effect 1", t),
                onDisable: (t) => EjecutarOpt("settings put global tube_amp_effect 0", t));

            AddToggleRow(card3, ref c3y,
                key: "samsung_gos",
                titulo: "Desactivar GOS (Game Optimizing Service)",
                desc: "Evita que Samsung limite CPU/GPU en juegos.",
                cmdON: "pm disable-user --user 0 com.samsung.android.game.gos",
                onEnable:  (t) => EjecutarOpt("pm disable-user --user 0 com.samsung.android.game.gos", t),
                onDisable: (t) => EjecutarOpt("pm enable com.samsung.android.game.gos",                t));

            AddToggleRow(card3, ref c3y,
                key: "samsung_cpu",
                titulo: "Enhanced CPU",
                desc: "Inyecta más voltaje a los núcleos principales en One UI.",
                cmdON: "settings put global sem_enhanced_cpu_responsiveness 1",
                onEnable:  (t) => EjecutarOpt("settings put global sem_enhanced_cpu_responsiveness 1", t),
                onDisable: (t) => EjecutarOpt("settings put global sem_enhanced_cpu_responsiveness 0", t));

            cardY += card3Height + S(20);

            // ════════════════════════════════════════════════════════════════════
            // CARD 4 — Xiaomi
            // ════════════════════════════════════════════════════════════════════
            int c4y         = headerH;
            int card4Height = headerH + rowH * 2 + cardPad;
            var card4 = CreateCard("Xiaomi", cardLeft, cardY, card4Height);

            AddToggleRow(card4, ref c4y,
                key: "miui_opt",
                titulo: "Desactivar optimización MIUI",
                desc: "Evita gestión agresiva de apps en segundo plano.",
                cmdON: "settings put secure miui_optimization false",
                onEnable:  (t) => EjecutarOpt("settings put secure miui_optimization false", t),
                onDisable: (t) => EjecutarOpt("settings put secure miui_optimization true",  t));

            AddToggleRow(card4, ref c4y,
                key: "miui_analytics",
                titulo: "Desactivar analytics",
                desc: "Corta rastreo interno de Xiaomi.",
                cmdON: "pm uninstall -k --user 0 com.miui.analytics",
                onEnable:  (t) => EjecutarOpt("pm uninstall -k --user 0 com.miui.analytics", t),
                onDisable: (t) => EjecutarOpt("pm install-existing com.miui.analytics",       t));

            cardY += card4Height + S(20);

            // ════════════════════════════════════════════════════════════════════
            // CARD 5 — Google Pixel
            // ════════════════════════════════════════════════════════════════════
            int c5y         = headerH;
            int card5Height = headerH + rowH * 4 + cardPad;
            var card5 = CreateCard("Google Pixel", cardLeft, cardY, card5Height);

            AddToggleRow(card5, ref c5y,
                key: "pixel_freeze",
                titulo: "Congelar apps inactivas",
                desc: "Congela apps en memoria cuando no las usas, liberando CPU.",
                cmdON: "settings put global cached_apps_freezer enabled",
                onEnable:  (t) => EjecutarOpt("settings put global cached_apps_freezer enabled",  t),
                onDisable: (t) => EjecutarOpt("settings put global cached_apps_freezer disabled", t));

            AddToggleRow(card5, ref c5y,
                key: "pixel_battery",
                titulo: "Desactivar batería adaptativa",
                desc: "Desactiva la gestión agresiva de batería de Pixel.",
                cmdON: "settings put global adaptive_battery_management_enabled 0",
                onEnable:  (t) => EjecutarOpt("settings put global adaptive_battery_management_enabled 0", t),
                onDisable: (t) => EjecutarOpt("settings put global adaptive_battery_management_enabled 1", t));

            AddToggleRow(card5, ref c5y,
                key: "pixel_smartspace",
                titulo: "Desactivar Smartspace",
                desc: "Apaga el widget inteligente que consume CPU en segundo plano.",
                cmdON: "settings put secure smartspace 0",
                onEnable:  (t) => EjecutarOpt("settings put secure smartspace 0", t),
                onDisable: (t) => EjecutarOpt("settings put secure smartspace 1", t));

            AddToggleRow(card5, ref c5y,
                key: "pixel_hotword",
                titulo: "Desactivar detección Hey Google",
                desc: "Elimina el proceso de escucha constante en background.",
                cmdON: "settings put global hotword_detection_enabled 0",
                onEnable:  (t) => EjecutarOpt("settings put global hotword_detection_enabled 0", t),
                onDisable: (t) => EjecutarOpt("settings put global hotword_detection_enabled 1", t));

            cardY += card5Height + S(20);

            // ════════════════════════════════════════════════════════════════════
            // CARD 6 — Huawei
            // ════════════════════════════════════════════════════════════════════
            int c6y         = headerH;
            int card6Height = headerH + rowH * 1 + cardPad;
            var card6 = CreateCard("Huawei", cardLeft, cardY, card6Height);

            AddToggleRow(card6, ref c6y,
                key: "huawei_powergenie",
                titulo: "Optimizar PowerGenie",
                desc: "Prioriza rendimiento sobre ahorro extremo de batería.",
                cmdON: "cmd package compile -m speed -f com.huawei.powergenie",
                onEnable:  (t) => EjecutarOpt("cmd package compile -m speed -f com.huawei.powergenie", t),
                onDisable: (t) =>
                {
                    if (!IsDisposed)
                        ToastNotification.Mostrar(this,
                            "Aplicado permanentemente hasta reinicio.",
                            ToastNotification.ToastTipo.Info);
                    return Task.CompletedTask;
                });

            contentPanel.Controls.AddRange(
                new Control[] { btnRevertirTodo, lblAdvertencia, card1, card2, card3, card4, card5, card6 });
        }
    }
}
