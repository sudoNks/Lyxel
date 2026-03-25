using System;
using System.Collections.Generic;
using System.IO;
using IniParser;
using IniParser.Model;

namespace MobiladorStex
{
    public class PerfilManager
    {
        private readonly string _perfilesPath;
        private IniData _data;
        private readonly FileIniDataParser _parser;

        public PerfilManager(string perfilesPath)
        {
            _perfilesPath = perfilesPath;
            _parser = new FileIniDataParser();
            _data = CargarIni();
        }

        // ══════════════════════════════════════════════════════════════
        // CARGA Y GUARDADO
        // ══════════════════════════════════════════════════════════════

        private IniData CargarIni()
        {
            if (File.Exists(_perfilesPath))
            {
                try
                {
                    return _parser.ReadFile(_perfilesPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error cargando perfiles: {ex.Message}");
                }
            }
            return new IniData();
        }

        private void GuardarIni()
        {
            try
            {
                string dir = Path.GetDirectoryName(_perfilesPath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                _parser.WriteFile(_perfilesPath, _data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error guardando perfiles: {ex.Message}");
            }
        }

        // ══════════════════════════════════════════════════════════════
        // OPERACIONES DE PERFIL
        // ══════════════════════════════════════════════════════════════

        public List<string> ListarPerfiles()
        {
            var nombres = new List<string>();
            foreach (var seccion in _data.Sections)
                nombres.Add(seccion.SectionName);
            return nombres;
        }

        public (bool exito, string error) AgregarPerfil(string nombre, ScrcpyConfig config)
        {
            try
            {
                if (_data.Sections.ContainsSection(nombre))
                    _data.Sections.RemoveSection(nombre);
                _data.Sections.AddSection(nombre);
                var s = _data[nombre];
                s["fps"] = config.Fps.ToString();
                s["bitrate"] = config.Bitrate.ToString();
                s["audio_buffer"] = config.AudioBuffer.ToString();
                s["audio_codec"] = config.AudioCodec ?? "opus";
                s["audio_bitrate"] = config.AudioBitrate.ToString();
                s["video"] = config.Video.ToString().ToLower();
                s["audio"] = config.Audio.ToString().ToLower();
                s["audio_doble"] = config.AudioDoble.ToString().ToLower();
                s["max_size"] = config.MaxSize.ToString();
                s["window_width"] = config.WindowWidth.ToString();
                s["window_height"] = config.WindowHeight.ToString();
                s["video_codec"] = config.VideoCodec ?? "h264";
                s["video_buffer"] = config.VideoBuffer.ToString();
                s["print_fps"] = config.PrintFps.ToString().ToLower();
                s["forward_all_clicks"] = config.ForwardAllClicks.ToString().ToLower();
                s["mostrar_flotante"] = config.MostrarFlotante.ToString().ToLower();
                s["wm_size_activo"] = config.WmSizeActivo.ToString().ToLower();
                s["wm_size_valor"] = config.WmSizeValor ?? "";
                s["use_advanced_encoder"] = config.UseAdvancedEncoder.ToString().ToLower();
                s["video_encoder"] = config.VideoEncoder ?? "";
                s["disable_screensaver"] = config.DisableScreensaver.ToString().ToLower();
                s["stay_awake"] = config.StayAwake.ToString().ToLower();
                s["turn_screen_off"] = config.TurnScreenOff.ToString().ToLower();
                s["fullscreen"] = config.Fullscreen.ToString().ToLower();
                s["fullscreen_crop"] = config.FullscreenCrop ?? "";
                s["shortcut_mod"] = config.ShortcutMod ?? "lalt";
                s["modo_otg"] = config.ModoOtg.ToString().ToLower();
                s["otg_serial"] = config.OtgSerial ?? "";
                s["resolucion_ancho"] = config.ResolucionAncho.ToString();
                s["resolucion_alto"] = config.ResolucionAlto.ToString();
                s["aspect_ratio"] = config.AspectRatio ?? "16:9";
                s["custom_ratio_w"] = config.CustomRatioW.ToString();
                s["custom_ratio_h"] = config.CustomRatioH.ToString();
                s["dpi"] = config.Dpi.ToString();
                s["input_mode"] = config.InputMode ?? "uhid";
                s["pointer_speed"] = config.PointerSpeed.ToString();
                GuardarIni();
                return (true, "");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool exito, string error) EliminarPerfil(string nombre)
        {
            try
            {
                if (!_data.Sections.ContainsSection(nombre))
                    return (false, $"El perfil '{nombre}' no existe");
                _data.Sections.RemoveSection(nombre);
                GuardarIni();
                return (true, "");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public ScrcpyConfig ObtenerPerfil(string nombre)
        {
            if (!_data.Sections.ContainsSection(nombre))
                return null;

            var s = _data[nombre];

            return new ScrcpyConfig
            {
                Fps = ParseInt(s["fps"], 60),
                Bitrate = ParseInt(s["bitrate"], 16),
                AudioBuffer = ParseInt(s["audio_buffer"], 50),
                AudioCodec = s["audio_codec"] ?? "opus",
                AudioBitrate = ParseInt(s["audio_bitrate"], 128),
                Video = ParseBool(s["video"], true),
                Audio = ParseBool(s["audio"], true),
                AudioDoble = ParseBool(s["audio_doble"], false),
                MaxSize = ParseInt(s["max_size"], 0),
                WindowWidth = ParseInt(s["window_width"], 0),
                WindowHeight = ParseInt(s["window_height"], 0),
                VideoCodec = s["video_codec"] ?? "h264",
                VideoBuffer = ParseInt(s["video_buffer"], 0),
                PrintFps = ParseBool(s["print_fps"], false),
                ForwardAllClicks = ParseBool(s["forward_all_clicks"], false),
                MostrarFlotante = ParseBool(s["mostrar_flotante"], true),
                WmSizeActivo = ParseBool(s["wm_size_activo"], false),
                WmSizeValor = s.ContainsKey("wm_size_valor") ? s["wm_size_valor"] : "",
                UseAdvancedEncoder = ParseBool(s["use_advanced_encoder"], false),
                VideoEncoder = s["video_encoder"] ?? "",
                DisableScreensaver = ParseBool(s["disable_screensaver"], false),
                StayAwake = ParseBool(s["stay_awake"], false),
                TurnScreenOff = ParseBool(s["turn_screen_off"], false),
                Fullscreen = ParseBool(s["fullscreen"], false),
                FullscreenCrop = s["fullscreen_crop"] ?? "",
                ShortcutMod = s["shortcut_mod"] ?? "lalt",
                ModoOtg = ParseBool(s["modo_otg"], false),
                OtgSerial = s["otg_serial"] ?? "",
                ResolucionAncho = ParseInt(s["resolucion_ancho"], 1080),
                ResolucionAlto = ParseInt(s["resolucion_alto"], 2400),
                AspectRatio = s["aspect_ratio"] ?? "16:9",
                CustomRatioW = ParseInt(s["custom_ratio_w"], 16),
                CustomRatioH = ParseInt(s["custom_ratio_h"], 9),
                Dpi = ParseInt(s["dpi"], 420),
                InputMode = s["input_mode"] ?? "uhid",
                PointerSpeed = ParseInt(s["pointer_speed"], 0)
            };
        }

        public bool ExistePerfil(string nombre) =>
            _data.Sections.ContainsSection(nombre);

        // ══════════════════════════════════════════════════════════════
        // IMPORTAR / EXPORTAR
        // ══════════════════════════════════════════════════════════════

        public (bool exito, string nombre, string error) ImportarDesdeArchivo(string rutaArchivo)
        {
            try
            {
                var dataImportada = _parser.ReadFile(rutaArchivo);
                string primerNombre = "";
                foreach (var seccion in dataImportada.Sections)
                {
                    if (string.IsNullOrEmpty(primerNombre)) primerNombre = seccion.SectionName;
                    if (_data.Sections.ContainsSection(seccion.SectionName))
                        _data.Sections.RemoveSection(seccion.SectionName);
                    _data.Sections.AddSection(seccion.SectionName);
                    foreach (var key in seccion.Keys)
                        _data[seccion.SectionName][key.KeyName] = key.Value;
                }
                GuardarIni();
                return (true, primerNombre, "");
            }
            catch (Exception ex) { return (false, "", ex.Message); }
        }

        public (bool exito, string error) ExportarPerfil(string nombre, string rutaArchivo)
        {
            try
            {
                if (!_data.Sections.ContainsSection(nombre))
                    return (false, $"El perfil '{nombre}' no existe");
                var dataExport = new IniData();
                dataExport.Sections.AddSection(nombre);
                foreach (var key in _data[nombre])
                    dataExport[nombre][key.KeyName] = key.Value;
                _parser.WriteFile(rutaArchivo, dataExport);
                return (true, "");
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public (bool exito, string error) RenombrarPerfil(string nombreActual, string nuevoNombre)
        {
            try
            {
                if (!_data.Sections.ContainsSection(nombreActual))
                    return (false, $"El perfil '{nombreActual}' no existe");

                if (_data.Sections.ContainsSection(nuevoNombre))
                    return (false, $"Ya existe un perfil con el nombre '{nuevoNombre}'");

                var seccionVieja = _data.Sections[nombreActual];
                _data.Sections.AddSection(nuevoNombre);
                var seccionNueva = _data.Sections[nuevoNombre];

                foreach (var key in seccionVieja)
                    seccionNueva.AddKey(key.KeyName, key.Value);

                _data.Sections.RemoveSection(nombreActual);
                GuardarIni();
                return (true, "");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public (bool exito, string error) GuardarConfigEnPerfil(string nombre, ScrcpyConfig config)
        {
            try
            {
                if (!_data.Sections.ContainsSection(nombre))
                    _data.Sections.AddSection(nombre);

                var sec = _data.Sections[nombre];
                sec.RemoveAllKeys();

                sec.AddKey("video", config.Video.ToString());
                sec.AddKey("audio", config.Audio.ToString());
                sec.AddKey("audio_doble", config.AudioDoble.ToString());
                sec.AddKey("fps", config.Fps.ToString());
                sec.AddKey("bitrate", config.Bitrate.ToString());
                sec.AddKey("max_size", config.MaxSize.ToString());
                sec.AddKey("window_width", config.WindowWidth.ToString());
                sec.AddKey("window_height", config.WindowHeight.ToString());
                sec.AddKey("video_codec", config.VideoCodec ?? "h264");
                sec.AddKey("video_buffer", config.VideoBuffer.ToString());
                sec.AddKey("audio_buffer", config.AudioBuffer.ToString());
                sec.AddKey("audio_codec", config.AudioCodec ?? "opus");
                sec.AddKey("audio_bitrate", config.AudioBitrate.ToString());
                sec.AddKey("print_fps", config.PrintFps.ToString());
                sec.AddKey("forward_all_clicks", config.ForwardAllClicks.ToString());
                sec.AddKey("mostrar_flotante", config.MostrarFlotante.ToString());
                sec.AddKey("wm_size_activo", config.WmSizeActivo.ToString());
                sec.AddKey("wm_size_valor", config.WmSizeValor ?? "");
                sec.AddKey("use_advanced_encoder", config.UseAdvancedEncoder.ToString());
                sec.AddKey("video_encoder", config.VideoEncoder ?? "");
                sec.AddKey("disable_screensaver", config.DisableScreensaver.ToString());
                sec.AddKey("stay_awake", config.StayAwake.ToString());
                sec.AddKey("turn_screen_off", config.TurnScreenOff.ToString());
                sec.AddKey("shortcut_mod", config.ShortcutMod ?? "lalt");
                sec.AddKey("fullscreen", config.Fullscreen.ToString());
                sec.AddKey("fullscreen_crop", config.FullscreenCrop ?? "");
                sec.AddKey("modo_otg", config.ModoOtg.ToString());
                sec.AddKey("otg_serial", config.OtgSerial ?? "");
                sec.AddKey("resolucion_ancho", config.ResolucionAncho.ToString());
                sec.AddKey("resolucion_alto", config.ResolucionAlto.ToString());
                sec.AddKey("aspect_ratio", config.AspectRatio ?? "16:9");
                sec.AddKey("custom_ratio_w", config.CustomRatioW.ToString());
                sec.AddKey("custom_ratio_h", config.CustomRatioH.ToString());
                sec.AddKey("dpi", config.Dpi.ToString());
                sec.AddKey("input_mode", config.InputMode ?? "uhid");
                sec.AddKey("pointer_speed", config.PointerSpeed.ToString());

                GuardarIni();
                return (true, "");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // ══════════════════════════════════════════════════════════════
        // HELPERS
        // ══════════════════════════════════════════════════════════════

        private static int ParseInt(string valor, int defecto)
        {
            if (string.IsNullOrEmpty(valor)) return defecto;
            return int.TryParse(valor, out int resultado) ? resultado : defecto;
        }

        private static bool ParseBool(string valor, bool defecto)
        {
            if (string.IsNullOrEmpty(valor)) return defecto;
            return bool.TryParse(valor, out bool resultado) ? resultado : defecto;
        }
    }
}