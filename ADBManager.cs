using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LyXel
{
    public class ADBManager
    {
        private readonly string _adbPath;

        public ADBManager(string adbPath)
        {
            _adbPath = adbPath;
        }

        private (bool exito, string stdout, string stderr) EjecutarComando(
            List<string> args, int timeoutMs = 10000)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = _adbPath,
                    Arguments = string.Join(" ", args),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var proceso = new Process { StartInfo = startInfo };
                proceso.Start();

                string stdout = proceso.StandardOutput.ReadToEnd();
                string stderr = proceso.StandardError.ReadToEnd();

                bool termino = proceso.WaitForExit(timeoutMs);

                if (!termino)
                {
                    try { proceso.Kill(); } catch { }
                    return (false, "", $"Timeout después de {timeoutMs / 1000}s");
                }

                return (proceso.ExitCode == 0, stdout, stderr);
            }
            catch (Exception ex)
            {
                string stderr = (ex is UnauthorizedAccessException || ex is System.ComponentModel.Win32Exception)
                    ? $"[PERMISOS] {ex.Message}"
                    : ex.Message;
                return (false, "", stderr);
            }
        }

        public (bool exito, int ancho, int alto, string mensaje) DetectarResolucion()
        {
            var (exito, stdout, stderr) = EjecutarComando(new List<string> { "shell", "wm", "size" });

            if (!exito)
                return (false, 1080, 2400, "No se pudo conectar con el dispositivo");

            try
            {
                foreach (var linea in stdout.Split('\n'))
                {
                    if (linea.Contains("Physical size:") || linea.Contains("Override size:"))
                    {
                        var resolucion = linea.Split(':')[^1].Trim();
                        var partes = resolucion.Split('x');
                        int ancho = int.Parse(partes[0]);
                        int alto = int.Parse(partes[1]);
                        return (true, ancho, alto, $"Detectado: {ancho}x{alto}");
                    }
                }
                return (false, 1080, 2400, "No se pudo parsear la resolución");
            }
            catch (Exception ex)
            {
                return (false, 1080, 2400, $"Error procesando respuesta: {ex.Message}");
            }
        }

        public (bool exito, string wmSize, string error) AplicarResolucion(
            int ancho, int alto, double aspectRatio)
        {
            int altoIdeal = (int)(ancho * aspectRatio);
            string wmSize = $"{ancho}x{altoIdeal}";

            var (exito, stdout, stderr) = EjecutarComando(
                new List<string> { "shell", "wm", "size", wmSize });

            return (exito, wmSize, stderr);
        }

        public (bool exito, string error) ResetearResolucion()
        {
            var (exito, stdout, stderr) = EjecutarComando(
                new List<string> { "shell", "wm", "size", "reset" });
            return (exito, stderr);
        }

        /// <summary>Aplica una resolución personalizada — ej. "1280x720"</summary>
        public (bool exito, string error) AplicarWmSizePersonalizada(string resolucion)
        {
            if (string.IsNullOrWhiteSpace(resolucion))
                return (false, "Resolución vacía");
            var (exito, _, stderr) = EjecutarComando(
                new List<string> { "shell", "wm", "size", resolucion.Trim() });
            return (exito, stderr);
        }

        public Task<(bool, string)> AplicarWmSizePersonalizadaAsync(string resolucion) =>
            Task.Run(() => AplicarWmSizePersonalizada(resolucion));

        /// <summary>Verifica si hay al menos un dispositivo conectado y autorizado.</summary>
        public bool HayDispositivoConectado()
        {
            try
            {
                var (_, stdout, _) = EjecutarComando(new List<string> { "devices" });
                return stdout.Split('\n')
                    .Skip(1)
                    .Any(l => l.Contains("\tdevice"));
            }
            catch { return false; }
        }

        public (bool exito, int dpi, string mensaje) DetectarDPI()
        {
            var (exito, stdout, stderr) = EjecutarComando(
                new List<string> { "shell", "wm", "density" });

            if (!exito)
                return (false, 420, "No se pudo conectar con el dispositivo");

            try
            {
                foreach (var linea in stdout.Split('\n'))
                {
                    if (linea.ToLower().Contains("density:"))
                    {
                        int dpi = int.Parse(linea.Split(':')[^1].Trim());
                        return (true, dpi, $"Detectado: {dpi} DPI");
                    }
                }
                return (false, 420, "No se pudo parsear el DPI");
            }
            catch (Exception ex)
            {
                return (false, 420, $"Error procesando respuesta: {ex.Message}");
            }
        }

        public (bool exito, string mensaje, string error) AplicarDPI(int nuevoDpi)
        {
            if (nuevoDpi < 120)
                return (false, "DPI muy bajo (mínimo recomendado: 120)", "Validación fallida");
            if (nuevoDpi > 800)
                return (false, "DPI muy alto (máximo recomendado: 800)", "Validación fallida");

            var (exito, stdout, stderr) = EjecutarComando(
                new List<string> { "shell", "wm", "density", nuevoDpi.ToString() });

            return exito
                ? (true, $"DPI cambiado a {nuevoDpi}", "")
                : (false, "Error aplicando DPI", stderr);
        }

        public (bool exito, string mensaje, string error) ResetearDPI()
        {
            var (exito, stdout, stderr) = EjecutarComando(
                new List<string> { "shell", "wm", "density", "reset" });

            return exito
                ? (true, "DPI restaurado a valor de fábrica", "")
                : (false, "Error reseteando DPI", stderr);
        }

        public (bool exito, List<string> seriales, string output) ListarDispositivos()
        {
            var (exito, stdout, stderr) = EjecutarComando(new List<string> { "devices" });

            if (!exito)
                return (false, new List<string>(), stderr);

            var seriales = new List<string>();
            var lineas = stdout.Split('\n');

            foreach (var linea in lineas[1..])
            {
                if (linea.Contains('\t'))
                {
                    var serial = linea.Split('\t')[0].Trim();
                    if (!string.IsNullOrEmpty(serial))
                        seriales.Add(serial);
                }
            }

            return (true, seriales, stdout);
        }


        public (bool exito, string mensaje) ReiniciarServidor()
        {
            try
            {
                // En Windows tengo que matar el proceso manualmente, de lo contrario el puerto queda ocupado
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "taskkill",
                        Arguments = "/F /IM adb.exe /T",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    })?.WaitForExit(3000);
                }

                // Kill-server
                EjecutarComando(new List<string> { "kill-server" }, 5000);
                Thread.Sleep(1500);

                // Start-server
                var (exito, stdout, stderr) = EjecutarComando(
                    new List<string> { "start-server" }, 15000);

                return exito
                    ? (true, "Servidor ADB reiniciado correctamente")
                    : (false, $"Servidor no inició: {stderr}");
            }
            catch (Exception ex)
            {
                return (false, $"Error crítico al reiniciar ADB: {ex.Message}");
            }
        }

        public (bool exito, string mensaje, string error) HabilitarTcpip(int puerto = 5555)
        {
            var (exito, stdout, stderr) = EjecutarComando(
                new List<string> { "tcpip", puerto.ToString() });

            return exito
                ? (true, $"Puerto {puerto} habilitado correctamente", "")
                : (false, "Error habilitando puerto TCP/IP", stderr);
        }

        public (bool exito, string ip, string mensaje) DetectarIPDispositivo()
        {
            var (exito, stdout, stderr) = EjecutarComando(
                new List<string> { "shell", "ip", "addr", "show", "wlan0" });

            if (!exito)
                return (false, "", "No se pudo ejecutar comando");

            try
            {
                foreach (var linea in stdout.Split('\n'))
                {
                    if (linea.Contains("inet ") && !linea.Contains("127.0.0.1"))
                    {
                        var partes = linea.Trim().Split(' ');
                        foreach (var parte in partes)
                        {
                            if (parte.Contains('.') && parte.Contains('/'))
                            {
                                string ip = parte.Split('/')[0];
                                return (true, ip, $"IP detectada: {ip}");
                            }
                        }
                    }
                }
                return (false, "", "No se encontró IP WiFi");
            }
            catch (Exception ex)
            {
                return (false, "", $"Error parseando IP: {ex.Message}");
            }
        }

        public (bool exito, string mensaje, string error) ConectarWifi(
            string ip, int puerto = 5555)
        {
            string direccion = $"{ip}:{puerto}";
            var (exito, stdout, stderr) = EjecutarComando(
                new List<string> { "connect", direccion });

            return (exito || stdout.ToLower().Contains("connected"))
                ? (true, $"Conectado a {direccion}", "")
                : (false, $"No se pudo conectar a {direccion}", stderr);
        }

        public (bool exito, string mensaje, string error) DesconectarWifi(
            string ip, int puerto = 5555)
        {
            string direccion = $"{ip}:{puerto}";
            var (exito, stdout, stderr) = EjecutarComando(
                new List<string> { "disconnect", direccion });

            return (exito || stdout.ToLower().Contains("disconnected"))
                ? (true, $"Desconectado de {direccion}", "")
                : (false, $"Error desconectando de {direccion}", stderr);
        }

        public (bool exito, string mensaje) DesconectarTodo()
        {
            var (exito, stdout, stderr) = EjecutarComando(new List<string> { "disconnect" });
            return (exito || stdout.ToLower().Contains("disconnected"))
                ? (true, "Todas las conexiones WiFi desconectadas")
                : (false, $"Error: {stderr}");
        }

        public bool PingDispositivoTcp(string ip, int puerto = 5555, int timeoutMs = 2000)
        {
            try
            {
                using var socket = new Socket(
                    AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var result = socket.BeginConnect(ip, puerto, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(timeoutMs, true);
                if (success) socket.EndConnect(result);
                return success && socket.Connected;
            }
            catch
            {
                return false;
            }
        }

        public (bool exito, int cantidad, string mensaje) LimpiarConexionesWifi(
            bool excluirActivas = true)
        {
            try
            {
                var (exito, stdout, stderr) = EjecutarComando(
                    new List<string> { "devices", "-l" });

                if (!exito)
                    return (false, 0, "No se pudo listar dispositivos");

                var conexionesAEliminar = new List<(string direccion, string estado)>();

                foreach (var linea in stdout.Split('\n'))
                {
                    if (!linea.Contains(':')) continue;

                    var match = Regex.Match(linea,
                        @"^([\d\.]+:\d+)\s+(device|offline|unauthorized)");

                    if (match.Success)
                    {
                        string direccion = match.Groups[1].Value;
                        string estado = match.Groups[2].Value;

                        if (estado is "offline" or "unauthorized")
                            conexionesAEliminar.Add((direccion, estado));
                        else if (estado == "device" && !excluirActivas)
                            conexionesAEliminar.Add((direccion, estado));
                    }
                }

                int eliminadas = 0;
                foreach (var (direccion, estado) in conexionesAEliminar)
                {
                    var (exitoDisc, _, _) = EjecutarComando(
                        new List<string> { "disconnect", direccion });
                    if (exitoDisc) eliminadas++;
                }

                return eliminadas > 0
                    ? (true, eliminadas, $"Se eliminaron {eliminadas} conexión(es) WiFi huérfanas")
                    : (true, 0, "No hay conexiones WiFi que limpiar");
            }
            catch (Exception ex)
            {
                return (false, 0, $"Error durante limpieza: {ex.Message}");
            }
        }

        public (bool exito, string mensaje, string error) CerrarTcpip()
        {
            try
            {
                EjecutarComando(new List<string> { "kill-server" }, 10000);
                Thread.Sleep(1000);

                var (exito, stdout, stderr) = EjecutarComando(
                    new List<string> { "start-server" }, 15000);

                Thread.Sleep(1500);

                return (true, "Puerto WiFi cerrado. Servidor ADB reiniciado.", "");
            }
            catch (Exception ex)
            {
                return (false, $"Error cerrando puerto WiFi: {ex.Message}", ex.Message);
            }
        }

        public Task<(bool, int, int, string)> DetectarResolucionAsync() =>
            Task.Run(() => DetectarResolucion());

        public Task<(bool, int, string)> DetectarDPIAsync() =>
            Task.Run(() => DetectarDPI());

        public Task<(bool, string)> ReiniciarServidorAsync() =>
            Task.Run(() => ReiniciarServidor());

        public Task<(bool, int, string)> LimpiarConexionesWifiAsync(bool excluirActivas = true) =>
            Task.Run(() => LimpiarConexionesWifi(excluirActivas));

        public Task<(bool, string, string)> HabilitarTcpipAsync(int puerto = 5555) =>
            Task.Run(() => HabilitarTcpip(puerto));

        public Task<(bool, string, string)> ConectarWifiAsync(string ip, int puerto = 5555) =>
            Task.Run(() => ConectarWifi(ip, puerto));

        public Task<(bool, string)> DesconectarTodoAsync() =>
            Task.Run(() => DesconectarTodo());

        public Task<(bool, string, string)> CerrarTcpipAsync() =>
            Task.Run(() => CerrarTcpip());

        public Task<(bool, string, string)> AplicarDPIAsync(int nuevoDpi) =>
            Task.Run(() => AplicarDPI(nuevoDpi));

        public Task<(bool, string, string)> ResetearDPIAsync() =>
            Task.Run(() => ResetearDPI());

        public Task<(bool, string, string)> AplicarResolucionAsync(
            int ancho, int alto, double aspectRatio) =>
            Task.Run(() => AplicarResolucion(ancho, alto, aspectRatio));

        public Task<(bool, string)> ResetearResolucionAsync() =>
            Task.Run(() => ResetearResolucion());

        public Task<bool> PingDispositivoTcpAsync(string ip, int puerto = 5555) =>
            Task.Run(() => PingDispositivoTcp(ip, puerto));

        public Task<(bool, string, string)> DetectarIPDispositivoAsync() =>
            Task.Run(() =>
            {
                var (e, ip, m) = DetectarIPDispositivo();
                return (e, ip, m);
            });

        // Este evento viene de un hilo background, acordarme de usar Invoke en la UI
        public event Action<bool>? OnDispositivoCambio;

        // Solo USB — el serial WiFi tiene ':' así que es fácil distinguirlos
        public event Action<bool>? OnDispositivoUsbCambio;

        private System.Diagnostics.Process? _trackProcess;
        private volatile bool _ultimoEstadoDispositivo = false;
        private volatile bool _ultimoEstadoDispositivoUsb = false;
        private volatile bool _trackActivo = false;

        public (bool exito, string error) AplicarPointerSpeed(int speed)
        {
            speed = Math.Max(-7, Math.Min(7, speed));
            var (exito, _, stderr) = EjecutarComando(
                new List<string> { "shell", "settings", "put", "system",
                    "pointer_speed", speed.ToString() });
            return (exito, stderr);
        }

        public Task<(bool, string)> AplicarPointerSpeedAsync(int speed) =>
            Task.Run(() => AplicarPointerSpeed(speed));

        /// <summary>
        /// Ejecuta un comando arbitrario en la shell del dispositivo.
        /// Devuelve (exito, stdout, stderr). Usar para comandos de optimización y diagnóstico.
        /// </summary>
        public (bool exito, string stdout, string stderr) EjecutarShell(string comando)
            => EjecutarComando(new List<string> { "shell", comando }, 15000);

        public async Task<(bool, string, string)> EjecutarShellAsync(string comando)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName               = _adbPath,
                    Arguments              = $"shell {comando}",
                    RedirectStandardOutput = true,
                    RedirectStandardError  = true,
                    UseShellExecute        = false,
                    CreateNoWindow         = true
                };

                using var proceso = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
                proceso.Start();

                var stdoutTask = proceso.StandardOutput.ReadToEndAsync();
                var stderrTask = proceso.StandardError.ReadToEndAsync();

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                try
                {
                    await proceso.WaitForExitAsync(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    try { proceso.Kill(); } catch { }
                    return (false, "", "timeout");
                }

                string stdout = await stdoutTask;
                string stderr = await stderrTask;
                return (proceso.ExitCode == 0, stdout, stderr);
            }
            catch (Exception ex)
            {
                string stderr = (ex is UnauthorizedAccessException || ex is System.ComponentModel.Win32Exception)
                    ? $"[PERMISOS] {ex.Message}"
                    : ex.Message;
                return (false, "", stderr);
            }
        }

        /// <summary>
        /// Ordena al dispositivo salir del modo tcpip y volver a escuchar por USB.
        /// Mucho más rápido que kill-server + start-server para el cierre de la app.
        /// </summary>
        public (bool exito, string mensaje) AplicarUsb()
        {
            var (exito, _, stderr) = EjecutarComando(new List<string> { "usb" }, 5000);
            return exito
                ? (true, "Dispositivo vuelto a modo USB")
                : (false, $"No se pudo cambiar a USB: {stderr}");
        }

        public void CerrarDaemonLocal()
        {
            try
            {
                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = _adbPath,
                    Arguments = "kill-server",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using var p = new System.Diagnostics.Process { StartInfo = startInfo };
                p.Start();
                p.WaitForExit(3000);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"ADB kill-server: {ex.Message}"); }

            try
            {
                // kill-server no siempre cierra todos los procesos, mato los que son de nuestra carpeta
                string carpetaAdb = Path.GetDirectoryName(_adbPath) ?? "";
                foreach (var proceso in System.Diagnostics.Process.GetProcessesByName("adb"))
                {
                    try
                    {
                        string rutaProceso = proceso.MainModule?.FileName ?? "";
                        if (rutaProceso.StartsWith(carpetaAdb, StringComparison.OrdinalIgnoreCase))
                        {
                            if (!proceso.HasExited)
                            {
                                proceso.Kill();
                                proceso.WaitForExit(1000);
                            }
                        }
                    }
                    catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"ADB kill proceso: {ex.Message}"); }
                    finally { proceso.Dispose(); }
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"ADB cierre local: {ex.Message}"); }
        }

        public void IniciarTrackDevices()
        {
            if (_trackActivo) return;
            _trackActivo = true;

            Task.Run(() =>
            {
                while (_trackActivo)
                {
                    try
                    {
                        var startInfo = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = _adbPath,
                            Arguments = "track-devices",
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardOutput = true,
                            StandardOutputEncoding = System.Text.Encoding.UTF8
                        };

                        _trackProcess = new System.Diagnostics.Process { StartInfo = startInfo };
                        _trackProcess.Start();

                        var proceso = _trackProcess; // referencia local para no tener race condition si _trackProcess se nulea

                        while (_trackActivo)
                        {
                            string? linea;
                            try
                            {
                                if (proceso.StandardOutput.EndOfStream) break;
                                linea = proceso.StandardOutput.ReadLine();
                            }
                            catch { break; }

                            if (linea == null) break;

                            bool hayDispositivo = linea.Contains("device") &&
                                                  !linea.Contains("offline") &&
                                                  !linea.Contains("unauthorized");
                            bool esUsb = hayDispositivo && !linea.Contains(':');

                            if (hayDispositivo != _ultimoEstadoDispositivo)
                            {
                                _ultimoEstadoDispositivo = hayDispositivo;
                                OnDispositivoCambio?.Invoke(hayDispositivo);
                            }

                            if (esUsb != _ultimoEstadoDispositivoUsb)
                            {
                                _ultimoEstadoDispositivoUsb = esUsb;
                                OnDispositivoUsbCambio?.Invoke(esUsb);
                            }
                        }
                    }
                    catch { }
                    finally
                    {
                        _trackProcess?.Dispose();
                        _trackProcess = null;
                    }

                    if (_trackActivo)
                        System.Threading.Thread.Sleep(2000);
                }
            });
        }

        public void DetenerTrackDevices()
        {
            _trackActivo = false;
            try { _trackProcess?.Kill(); } catch { }
            _trackProcess?.Dispose();
            _trackProcess = null;
        }
    }
}