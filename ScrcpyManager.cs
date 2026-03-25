using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace MobiladorStex
{
    public class ScrcpyConfig
    {
        // ── Video ────────────────────────────────────────────────────
        public bool Video { get; set; } = true;
        public int Fps { get; set; } = 90;
        public int Bitrate { get; set; } = 32;
        public int MaxSize { get; set; } = 1600;
        public int WindowWidth { get; set; } = 0;
        public int WindowHeight { get; set; } = 0;
        public string VideoCodec { get; set; } = "h264";
        public int VideoBuffer { get; set; } = 0;
        public bool PrintFps { get; set; } = false;
        public bool ForwardAllClicks { get; set; } = false; // --mouse-bind=++++:++++
        public bool MostrarFlotante { get; set; } = true;
        public bool WmSizeActivo { get; set; } = false;
        public string WmSizeValor { get; set; } = "";

        // ── Encoder Avanzado ─────────────────────────────────────────
        public bool UseAdvancedEncoder { get; set; } = false;
        public string VideoEncoder { get; set; } = "";

        // ── Audio ────────────────────────────────────────────────────
        public bool Audio { get; set; } = true;
        public bool AudioDoble { get; set; } = false;
        public int AudioBuffer { get; set; } = 50;
        public string AudioCodec { get; set; } = "opus";   // opus | aac | flac
        public int AudioBitrate { get; set; } = 128;       // Kbps (no aplica a raw)

        // ── Comportamiento ───────────────────────────────────────────
        public bool DisableScreensaver { get; set; } = false;
        public bool StayAwake { get; set; } = false;
        public bool TurnScreenOff { get; set; } = false;
        public string ShortcutMod { get; set; } = "lalt";

        // ── Pantalla ─────────────────────────────────────────────────
        public bool Fullscreen { get; set; } = false;
        public string FullscreenCrop { get; set; } = "";
        public int ResolucionAncho { get; set; } = 1080;
        public int ResolucionAlto { get; set; } = 2400;
        public string AspectRatio { get; set; } = "16:9";
        public int CustomRatioW { get; set; } = 16;
        public int CustomRatioH { get; set; } = 9;
        public int Dpi { get; set; } = 420;

        // ── Entrada (teclado + mouse juntos) ─────────────────────────
        // sdk  = inyección API Android (default, más compatible)
        // uhid = teclado/mouse HID físico vía kernel (recomendado)
        // aoa  = HID físico vía protocolo AOA (solo USB)
        public string InputMode { get; set; } = "uhid";

        // Velocidad del cursor Android (-7 a 7, 0 = default)
        public int PointerSpeed { get; set; } = 0;

        // ── Conexión ─────────────────────────────────────────────────
        public bool ModoOtg { get; set; } = false;
        public string OtgSerial { get; set; } = "";
        public bool UsarWifi { get; set; } = false;
        public string WifiIp { get; set; } = "";
        public int WifiPuerto { get; set; } = 5555;
    }

    public class ScrcpyManager
    {
        private readonly string _scrcpyPath;
        private readonly string _adbPath;
        private Process? _proceso;

        public ScrcpyManager(string scrcpyPath, string adbPath)
        {
            _scrcpyPath = scrcpyPath;
            _adbPath = adbPath;
        }

        // ══════════════════════════════════════════════════════════════
        // ESTADO
        // ══════════════════════════════════════════════════════════════

        public bool EstaCorriendo
        {
            get { var p = _proceso; return p != null && !p.HasExited; }
        }

        // ══════════════════════════════════════════════════════════════
        // LANZAR
        // ══════════════════════════════════════════════════════════════

        public bool Lanzar(ScrcpyConfig config)
        {
            var cmd = new List<string>();

            // Tecla MOD
            if (!string.IsNullOrEmpty(config.ShortcutMod))
                cmd.Add($"--shortcut-mod={config.ShortcutMod}");

            // ── MODO OTG ─────────────────────────────────────────────
            if (config.ModoOtg)
            {
                cmd.Add("--otg");
                if (!string.IsNullOrEmpty(config.OtgSerial))
                    cmd.AddRange(new[] { "-s", config.OtgSerial });
                cmd.AddRange(new[] { "--window-title", "Mobilador_SteX_OTG" });
                return LanzarProceso(cmd, config);
            }

            // ── WIFI ─────────────────────────────────────────────────
            if (config.UsarWifi)
            {
                if (string.IsNullOrEmpty(config.WifiIp))
                    return false;
                cmd.Add($"--tcpip={config.WifiIp}:{config.WifiPuerto}");
            }

            // ── TÍTULO DE VENTANA ────────────────────────────────────
            cmd.AddRange(new[] { "--window-title",
                config.UsarWifi ? "Mobilador_SteX_WiFi" : "Mobilador_SteX" });

            // ── VIDEO ─────────────────────────────────────────────────
            if (!config.Video)
            {
                cmd.Add("--no-video");
            }
            else
            {
                cmd.Add($"--max-fps={config.Fps}");
                cmd.AddRange(new[] { "-b", $"{config.Bitrate}M" });

                if (config.MaxSize > 0)
                    cmd.AddRange(new[] { "-m", config.MaxSize.ToString() });

                if (!string.IsNullOrEmpty(config.FullscreenCrop))
                {
                    cmd.Add($"--crop={config.FullscreenCrop}");
                }
                else
                {
                    if (config.WindowWidth > 0)
                        cmd.Add($"--window-width={config.WindowWidth}");
                    if (config.WindowHeight > 0)
                        cmd.Add($"--window-height={config.WindowHeight}");
                }

                // Encoder avanzado tiene prioridad sobre codec genérico
                if (config.UseAdvancedEncoder && !string.IsNullOrWhiteSpace(config.VideoEncoder))
                {
                    string codec = InferirCodecDeEncoder(config.VideoEncoder);
                    cmd.Add($"--video-codec={codec}");
                    cmd.Add($"--video-encoder={config.VideoEncoder}");
                }
                else if (!string.IsNullOrEmpty(config.VideoCodec))
                {
                    cmd.Add($"--video-codec={config.VideoCodec}");
                }

                if (config.VideoBuffer > 0)
                    cmd.Add($"--video-buffer={config.VideoBuffer}");

                if (config.Fullscreen)
                    cmd.Add("-f");

                if (config.PrintFps)
                    cmd.Add("--print-fps");
            }

            // ── AUDIO ─────────────────────────────────────────────────
            if (!config.Audio)
            {
                cmd.Add("--no-audio");
            }
            else
            {
                cmd.Add($"--audio-buffer={config.AudioBuffer}");
                if (config.AudioDoble)
                    cmd.Add("--audio-dup");

                // Codec — solo agregar si no es el default para no saturar el comando
                string codec = string.IsNullOrEmpty(config.AudioCodec) ? "opus" : config.AudioCodec;
                if (codec != "opus")
                    cmd.Add($"--audio-codec={codec}");

                // Bitrate — no aplica a flac/raw, scrcpy lo ignora si no es relevante
                if (config.AudioBitrate > 0 && config.AudioBitrate != 128)
                    cmd.Add($"--audio-bit-rate={config.AudioBitrate}K");
            }

            // ── INPUT ─────────────────────────────────────────────────
            // OTG ya fuerza aoa implícitamente — no agregar flags manuales
            if (!config.ModoOtg)
            {
                string mode = config.InputMode switch
                {
                    "sdk" => "sdk",
                    "aoa" => "aoa",
                    _ => "uhid"   // default seguro
                };
                cmd.Add($"--keyboard={mode}");
                cmd.Add($"--mouse={mode}");
                // Pasar todos los clics al dispositivo — fix para Shift+clic derecho en juegos
                if (config.ForwardAllClicks)
                    cmd.Add("--mouse-bind=++++:++++");
            }

            // ── EXTRAS ────────────────────────────────────────────────
            if (config.DisableScreensaver) cmd.Add("--disable-screensaver");
            if (config.StayAwake) cmd.Add("-w");
            if (config.TurnScreenOff) cmd.Add("--turn-screen-off");

            return LanzarProceso(cmd, config);
        }

        // Evento FPS — FloatingWindow se suscribe si PrintFps está activo
        // IMPORTANTE: se dispara en hilo de fondo, usar Invoke en la UI
        public event Action<string>? OnFpsUpdate;

        private bool LanzarProceso(List<string> cmd, ScrcpyConfig config)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = _scrcpyPath,
                    Arguments = string.Join(" ", cmd),
                    UseShellExecute = false,   // siempre false — sin consola externa
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,   // scrcpy escribe FPS en stdout
                    StandardOutputEncoding = Encoding.UTF8
                };
                startInfo.Environment["ADB"] = _adbPath;

                _proceso = new Process { StartInfo = startInfo };
                _proceso.Start();

                // Leer stderr en hilo separado siempre (evita bloqueo del proceso)
                // Solo dispara el evento si PrintFps está activo
                bool capturarFps = config.PrintFps;
                Task.Run(() =>
                {
                    try
                    {
                        while (!_proceso.StandardOutput.EndOfStream)
                        {
                            string? linea = _proceso.StandardOutput.ReadLine();
                            if (linea == null) break;

                            if (!capturarFps) continue;

                            // scrcpy 3.x formato: "[server] INFO: 60 fps"
                            var match = System.Text.RegularExpressions.Regex.Match(
                                linea, @"(\d+)\s*fps",
                                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                            if (match.Success)
                                OnFpsUpdate?.Invoke($"{match.Groups[1].Value} fps");
                        }
                    }
                    catch { /* proceso terminado, salir limpiamente */ }
                });

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error lanzando scrcpy: {ex.Message}");
                return false;
            }
        }

        // ══════════════════════════════════════════════════════════════
        // DETENER
        // ══════════════════════════════════════════════════════════════

        public void Detener()
        {
            if (_proceso == null) return;
            try
            {
                if (!_proceso.HasExited)
                {
                    _proceso.CloseMainWindow();
                    if (!_proceso.WaitForExit(2000))
                        _proceso.Kill();
                }
            }
            catch { }
            finally
            {
                _proceso?.Dispose();
                _proceso = null;
            }
        }

        // ══════════════════════════════════════════════════════════════
        // DETECTAR ENCODERS
        // ══════════════════════════════════════════════════════════════

        // Devuelve: (exito, lista de nombres de encoder, lista de etiquetas display, output raw)
        // - encoders:      ["c2.exynos.h264.encoder", "c2.android.avc.encoder", ...]
        // - displayLabels: ["c2.exynos.h264.encoder  (hw) h264", "c2.android.avc.encoder  (sw) h264", ...]
        public async Task<(bool exito, List<string> encoders, List<string> displayLabels, string output)> DetectarEncodersAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    var encoderStartInfo = new ProcessStartInfo
                    {
                        FileName = _scrcpyPath,
                        Arguments = "--list-encoders",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8
                    };
                    encoderStartInfo.Environment["ADB"] = _adbPath;
                    var proc = new Process { StartInfo = encoderStartInfo };

                    proc.Start();

                    var stdoutTask = proc.StandardOutput.ReadToEndAsync();
                    var stderrTask = proc.StandardError.ReadToEndAsync();
                    Task.WaitAll(stdoutTask, stderrTask);
                    proc.WaitForExit(10000);

                    string fullOutput = stdoutTask.Result + "\n" + stderrTask.Result;
                    var (encoders, displayLabels) = ParsearEncoders(fullOutput);

                    return (encoders.Count > 0, encoders, displayLabels, fullOutput.Trim());
                }
                catch (Exception ex)
                {
                    return (false, new List<string>(), new List<string>(), $"Error: {ex.Message}");
                }
            });
        }

        // Formato confirmado scrcpy 3.3.4:
        // [server] INFO: List of video encoders:
        //     --video-codec=h264 --video-encoder=c2.exynos.h264.encoder   (hw) [vendor]
        //     --video-codec=h264 --video-encoder=c2.android.avc.encoder   (sw)
        //     --video-codec=h265 --video-encoder=c2.exynos.hevc.encoder   (hw) [vendor]
        // [server] INFO: List of audio encoders:
        //     ...
        private (List<string> encoders, List<string> displayLabels) ParsearEncoders(string output)
        {
            var encoders = new List<string>();
            var displayLabels = new List<string>();
            bool enVideo = false;

            foreach (var lineaRaw in output.Split('\n'))
            {
                string linea = lineaRaw.Trim();
                if (string.IsNullOrWhiteSpace(linea)) continue;

                if (linea.Contains("List of video encoders") || linea.Contains("Video encoders"))
                { enVideo = true; continue; }

                if (linea.Contains("List of audio encoders") || linea.Contains("Audio encoders"))
                { enVideo = false; continue; }

                if (!enVideo) continue;

                // Extraer codec: "--video-codec=h264 ..."
                string codec = "h264";
                int idxCodec = linea.IndexOf("--video-codec=", StringComparison.Ordinal);
                if (idxCodec >= 0)
                {
                    string restoCodec = linea.Substring(idxCodec + "--video-codec=".Length).Trim();
                    codec = restoCodec.Split(' ')[0].Trim();
                }

                // Extraer encoder: "... --video-encoder=c2.exynos.h264.encoder ..."
                int idxEnc = linea.IndexOf("--video-encoder=", StringComparison.Ordinal);
                if (idxEnc < 0) continue;
                string restoEnc = linea.Substring(idxEnc + "--video-encoder=".Length).Trim();
                string encoder = restoEnc.Split(' ')[0].Trim();
                if (string.IsNullOrWhiteSpace(encoder) || !encoder.Contains(".")) continue;
                if (encoders.Contains(encoder)) continue;

                // Detectar (hw) o (sw) en el resto de la línea
                string tipo = restoEnc.Contains("(hw)") ? "hw" : "sw";

                encoders.Add(encoder);
                displayLabels.Add($"{encoder}  [{tipo}] [{codec}]");
            }

            return (encoders, displayLabels);
        }

        private void AgregarEncoder(List<string> lista, string encoder)
        {
            if (!string.IsNullOrWhiteSpace(encoder)
                && encoder.Contains(".")
                && !lista.Contains(encoder))
                lista.Add(encoder);
        }

        // ══════════════════════════════════════════════════════════════
        // ASYNC HELPERS
        // ══════════════════════════════════════════════════════════════

        public Task<bool> LanzarAsync(ScrcpyConfig config) =>
            Task.Run(() => Lanzar(config));

        public Task DetenerAsync() =>
            Task.Run(() => Detener());

        // ══════════════════════════════════════════════════════════════
        // PRIVADOS
        // ══════════════════════════════════════════════════════════════

        private string InferirCodecDeEncoder(string encoderName)
        {
            string lower = encoderName.ToLower();
            if (lower.Contains("hevc") || lower.Contains("h265") || lower.Contains("h.265"))
                return "h265";
            if (lower.Contains("av01") || lower.Contains("av1"))
                return "av1";
            return "h264";
        }
    }
}