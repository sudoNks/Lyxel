using Guna.UI2.WinForms;
using LyXel.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LyXel
{
    public partial class Form1
    {
        private void LoadOptimizacionPage()
        {
            // ── Guardia de aceptación ────────────────────────────────────────────
            // Mostrar advertencia si el usuario no ha aceptado aún (o no marcó "No volver
            // a mostrar"). La persistencia en disco ocurre SOLO si marcó ese checkbox.
            if (!_optimizacionAceptada)
            {
                using var aviso = new DialogoAvanzado(
                    "Zona de Optimización",
                    "Estos comandos modifican configuraciones internas de Android. Se han probado en varios dispositivos pero pueden no funcionar en todos. Si no sabes qué hace cada opción, mejor no la actives. Al continuar aceptas que es bajo tu propia responsabilidad.",
                    new[] { "Entiendo que estas opciones modifican configuraciones internas de Android y las activo bajo mi propia responsabilidad." });

                if (aviso.ShowDialog(this) != System.Windows.Forms.DialogResult.OK)
                {
                    // El usuario canceló: mantener false y volver a Inicio
                    LoadPage(0, "Inicio", LoadInicioPage);
                    return;
                }

                _optimizacionAceptada = true;
                if (aviso.NoVolverMostrar)
                    GuardarConfigTema(); // Solo persiste en disco si lo pidió explícitamente
                // Si no marcó "No volver a mostrar", true queda en memoria solo esta sesión
            }

            int cardLeft = S(30);
            int cardY    = S(20);
            int headerH  = S(54);
            int rowH      = S(70);
            int comboRowH = S(95); // altura dedicada a filas con ComboBox; ajustar independientemente de rowH
            int btnRowH   = S(56);
            int cardPad  = S(20);

            // Registro de toggles para "Revertir todo": (toggle, comando OFF)
            var toggleRegistry = new List<(Guna2ToggleSwitch tog, Func<Task> offCmd)>();

            // Flag compartido para suprimir handlers durante el revertido masivo
            bool revirtiendoTodo = false;

            // ── Ejecuta un comando shell con verificación de dispositivo y manejo
            //    de errores clasificado. Revierte el toggle a OFF si falla.
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
                if (exito || IsDisposed) return;

                // Suprimir Toasts individuales durante revertido masivo (togOrigen == null)
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
                }

                if (togOrigen != null)
                    togOrigen.Checked = false;
            }

            // ── Fila de toggle ───────────────────────────────────────────────────
            void AddToggleRow(Panel card, ref int y, string titulo, string desc,
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
                    Width = rowWidth - S(64), Height = S(34),
                    AutoSize = false
                });

                var tog = new Guna2ToggleSwitch()
                {
                    Left    = card.Width - S(64),
                    Top     = y + S(16),
                    Checked = false,
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
                        if (!IsDisposed) tog.Enabled = true;
                    }
                };
                card.Controls.Add(tog);

                toggleRegistry.Add((tog, () => onDisable(null)));
                y += rowH;
            }

            // ── Fila de ComboBox + Aplicar + Resetear ────────────────────────────
            void AddComboRow(Panel card, ref int y, string titulo, string desc,
                string[] opciones, int defaultIndex,
                Func<string, Task> onAplicar, Func<Task> onResetear)
            {
                int rowWidth = card.Width - S(48);
                // Título y descripción en la mitad superior de la fila
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

                // ComboBox + botones en la mitad inferior (Top = y+S(36), cabe en comboRowH=S(70))
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
                    btnAplicar.Enabled = false;
                    string textoOrig   = btnAplicar.Text;
                    btnAplicar.Text    = "...";
                    // Extraer valor numérico (ej: "0 (sin límite)" → "0")
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

                card.Controls.AddRange(new Control[] { cmb, btnAplicar, btnResetear });
                y += comboRowH;
            }

            // ── Fila de botón simple ─────────────────────────────────────────────
            void AddButtonRow(Panel card, ref int y, string titulo, string desc,
                string btnText, Func<Task> accion)
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

            // ── Botón "Revertir todo" — antes de las cards ───────────────────────
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
            btnRevertirTodo.Click += async (s, e) =>
            {
                if (!_hayDispositivo)
                {
                    ToastNotification.Mostrar(this,
                        "Conecta tu teléfono antes de revertir.",
                        ToastNotification.ToastTipo.Advertencia);
                    return;
                }
                btnRevertirTodo.Enabled = false;
                btnRevertirTodo.Text    = "Revirtiendo...";
                revirtiendoTodo         = true;
                try
                {
                    foreach (var (_, offCmd) in toggleRegistry)
                        await offCmd();

                    foreach (var (tog, _) in toggleRegistry)
                        if (!IsDisposed) tog.Checked = false;
                }
                finally
                {
                    revirtiendoTodo = false;
                    if (!IsDisposed)
                    {
                        btnRevertirTodo.Text    = "  Revertir todo";
                        btnRevertirTodo.Enabled = true;
                    }
                }
                if (!IsDisposed)
                    ToastNotification.Mostrar(this,
                        "Todas las optimizaciones han sido revertidas.",
                        ToastNotification.ToastTipo.Exito);
            };
            cardY += S(36) + S(14);

            // ════════════════════════════════════════════════════════════════════
            // CARD 1 — Rendimiento General
            // toggles: Fixed, Animaciones, Tasa(combo), FPS(combo), GameDriver, Blur, VSync, OpenGL = 8×rowH
            // buttons: LimpiarRAM, LimpiarCache, ART, DEXOPT = 4×btnRowH
            // ════════════════════════════════════════════════════════════════════
            int c1y         = headerH;
            int card1Height = headerH + rowH * 6 + comboRowH * 2 + btnRowH * 4 + cardPad;
            var card1 = CreateCard("Rendimiento General", cardLeft, cardY, card1Height);

            AddToggleRow(card1, ref c1y,
                "Fixed Performance Mode",
                "Fija CPU/GPU al máximo, elimina throttling térmico. Úsalo en sesiones cortas.",
                onEnable:  (t) => EjecutarOpt("cmd power set-fixed-performance-mode-enabled true",  t),
                onDisable: (t) => EjecutarOpt("cmd power set-fixed-performance-mode-enabled false", t));

            AddToggleRow(card1, ref c1y,
                "Animaciones reducidas",
                "Hace el sistema más ágil y responsivo.",
                onEnable: async (t) =>
                {
                    await EjecutarOpt("settings put global window_animation_scale 0.5",    t);
                    await EjecutarOpt("settings put global transition_animation_scale 0.5", t);
                    await EjecutarOpt("settings put global animator_duration_scale 0.5",    t);
                },
                onDisable: async (t) =>
                {
                    await EjecutarOpt("settings put global window_animation_scale 1.0",    t);
                    await EjecutarOpt("settings put global transition_animation_scale 1.0", t);
                    await EjecutarOpt("settings put global animator_duration_scale 1.0",    t);
                });

            AddComboRow(card1, ref c1y,
                "Tasa de refresco máxima",
                "Evita que el sistema baje la tasa de refresco para ahorrar batería.",
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
                "FPS mínimo del sistema",
                "Evita que el sistema baje de este valor. Elige el máximo que soporte tu pantalla.",
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
                "Game Driver universal",
                "Usa drivers gráficos optimizados en todas las apps.",
                onEnable:  (t) => EjecutarOpt("settings put global game_driver_all_apps 1", t),
                onDisable: (t) => EjecutarOpt("settings put global game_driver_all_apps 0", t));

            AddToggleRow(card1, ref c1y,
                "Desactivar blur de ventanas",
                "Elimina efectos de desenfoque que consumen GPU (Android 15+).",
                onEnable:  (t) => EjecutarOpt("settings put global disable_window_blurs 1", t),
                onDisable: (t) => EjecutarOpt("settings put global disable_window_blurs 0", t));

            AddToggleRow(card1, ref c1y,
                "Forzar VSync por GPU",
                "Elimina el tearing visual forzando sincronización vertical por GPU.",
                onEnable:  (t) => EjecutarOpt("setprop debug.hwc.force_gpu_vsync 1", t),
                onDisable: (t) => EjecutarOpt("setprop debug.hwc.force_gpu_vsync 0", t));

            AddToggleRow(card1, ref c1y,
                "Forzar renderizado OpenGL",
                "Útil en dispositivos con Android 12 o anterior que tienen problemas con Vulkan.",
                onEnable:  (t) => EjecutarOpt("setprop debug.force-opengl 1", t),
                onDisable: (t) => EjecutarOpt("setprop debug.force-opengl 0", t));

            AddButtonRow(card1, ref c1y,
                "Limpieza de procesos",
                "Libera RAM cerrando procesos en segundo plano.",
                "Limpiar RAM",
                () => EjecutarOpt("am kill-all"));

            AddButtonRow(card1, ref c1y,
                "Limpiar caché de apps",
                "Libera caché acumulada de todas las apps.",
                "Limpiar caché",
                () => EjecutarOpt("pm trim-caches 999G"));

            AddButtonRow(card1, ref c1y,
                "Optimizar compilación ART",
                "Recompila las apps según tu uso para abrirlas más rápido. Tarda unos segundos.",
                "Optimizar",
                () => EjecutarOpt("cmd package compile -m speed-profile -a"));

            AddButtonRow(card1, ref c1y,
                "Optimización DEXOPT",
                "Optimización en segundo plano que Android hace normalmente en reposo.",
                "Ejecutar",
                () => EjecutarOpt("pm bg-dexopt-job"));

            cardY += card1Height + S(20);

            // ════════════════════════════════════════════════════════════════════
            // CARD 2 — Gaming — Free Fire
            // ════════════════════════════════════════════════════════════════════
            int c2y         = headerH;
            int card2Height = headerH + rowH * 3 + btnRowH * 1 + cardPad;
            var card2 = CreateCard("Gaming — Free Fire", cardLeft, cardY, card2Height);

            AddToggleRow(card2, ref c2y,
                "Game Mode Performance",
                "Prioriza rendimiento sobre batería para Free Fire.",
                onEnable:  (t) => EjecutarOpt("cmd game mode performance com.dts.freefireth", t),
                onDisable: (t) => EjecutarOpt("cmd game mode standard com.dts.freefireth",    t));

            AddToggleRow(card2, ref c2y,
                "Bloquear background en partida",
                "Impide que otras apps consuman CPU mientras juegas.",
                onEnable:  (t) => EjecutarOpt("cmd appops set com.dts.freefireth RUN_ANY_IN_BACKGROUND deny",  t),
                onDisable: (t) => EjecutarOpt("cmd appops set com.dts.freefireth RUN_ANY_IN_BACKGROUND allow", t));

            AddToggleRow(card2, ref c2y,
                "Bloquear Free Fire en memoria",
                "Evita que el sistema cierre el juego al cambiar de app.",
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
                "Compilar Free Fire (velocidad)",
                "Compila el juego para tu procesador, mejora tiempos de carga.",
                "Compilar",
                async () =>
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
                "Audio HD (K2HD)",
                "Mejora calidad de audio en dispositivos Samsung compatibles.",
                onEnable:  (t) => EjecutarOpt("settings put global k2hd_effect 1", t),
                onDisable: (t) => EjecutarOpt("settings put global k2hd_effect 0", t));

            AddToggleRow(card3, ref c3y,
                "Tube Amp Effect",
                "Efecto de amplificador de audio Samsung.",
                onEnable:  (t) => EjecutarOpt("settings put global tube_amp_effect 1", t),
                onDisable: (t) => EjecutarOpt("settings put global tube_amp_effect 0", t));

            AddToggleRow(card3, ref c3y,
                "Desactivar GOS (Game Optimizing Service)",
                "Evita que Samsung limite CPU/GPU en juegos.",
                onEnable:  (t) => EjecutarOpt("pm disable-user --user 0 com.samsung.android.game.gos", t),
                onDisable: (t) => EjecutarOpt("pm enable com.samsung.android.game.gos",                t));

            AddToggleRow(card3, ref c3y,
                "Enhanced CPU",
                "Inyecta más voltaje a los núcleos principales en One UI.",
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
                "Desactivar optimización MIUI",
                "Evita gestión agresiva de apps en segundo plano.",
                onEnable:  (t) => EjecutarOpt("settings put secure miui_optimization false", t),
                onDisable: (t) => EjecutarOpt("settings put secure miui_optimization true",  t));

            AddToggleRow(card4, ref c4y,
                "Desactivar analytics",
                "Corta rastreo interno de Xiaomi.",
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
                "Congelar apps inactivas",
                "Congela apps en memoria cuando no las usas, liberando CPU.",
                onEnable:  (t) => EjecutarOpt("settings put global cached_apps_freezer enabled",  t),
                onDisable: (t) => EjecutarOpt("settings put global cached_apps_freezer disabled", t));

            AddToggleRow(card5, ref c5y,
                "Desactivar batería adaptativa",
                "Desactiva la gestión agresiva de batería de Pixel.",
                onEnable:  (t) => EjecutarOpt("settings put global adaptive_battery_management_enabled 0", t),
                onDisable: (t) => EjecutarOpt("settings put global adaptive_battery_management_enabled 1", t));

            AddToggleRow(card5, ref c5y,
                "Desactivar Smartspace",
                "Apaga el widget inteligente que consume CPU en segundo plano.",
                onEnable:  (t) => EjecutarOpt("settings put secure smartspace 0", t),
                onDisable: (t) => EjecutarOpt("settings put secure smartspace 1", t));

            AddToggleRow(card5, ref c5y,
                "Desactivar detección Hey Google",
                "Elimina el proceso de escucha constante en background.",
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
                "Optimizar PowerGenie",
                "Prioriza rendimiento sobre ahorro extremo de batería.",
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
                new Control[] { btnRevertirTodo, card1, card2, card3, card4, card5, card6 });
        }
    }
}
