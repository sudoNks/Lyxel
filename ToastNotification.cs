using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace LyXel
{
    /// <summary>
    /// Notificación flotante reutilizable que aparece en la esquina inferior derecha
    /// de la ventana padre y se desvanece automáticamente.
    /// Soporta hasta 3 toasts apilados verticalmente (el más reciente encima).
    /// Uso: ToastNotification.Mostrar(owner, "Mensaje", ToastTipo.Exito);
    /// </summary>
    public class ToastNotification : Form
    {
        public enum ToastTipo { Exito, Advertencia, Error, Info }

        private System.Windows.Forms.Timer _timerFade;
        private int _duracionMs;

        private int S(int px) => (int)Math.Round(px * this.DeviceDpi / 96.0);

        // Constructor privado — usar el método estático Mostrar
        private ToastNotification(string mensaje, ToastTipo tipo, int duracionMs)
        {
            _duracionMs = duracionMs;
            BuildUI(mensaje, tipo);
            IniciarFade();
        }

        // Lista de toasts activos — índice 0 = más antiguo (abajo), último = más reciente (arriba)
        private static readonly List<ToastNotification> _toastsActivos = new();
        private static readonly object _lock = new();

        // Gap vertical entre toasts apilados (px lógicos)
        private const int GapToasts = 8;
        // Máximo de toasts simultáneos
        private const int MaxToasts = 3;

        /// <summary>
        /// Muestra una notificación toast sobre la ventana owner.
        /// Puede coexistir con hasta 2 toasts previos (máximo 3 simultáneos).
        /// Si ya hay 3 activos, el nuevo se descarta silenciosamente.
        /// </summary>
        public static void Mostrar(Form owner, string mensaje,
            ToastTipo tipo = ToastTipo.Info, int duracionMs = 3000, bool forzar = false)
        {
            if (owner == null || owner.IsDisposed) return;

            // Asegurar ejecución en el hilo UI
            if (owner.InvokeRequired)
            {
                owner.Invoke(() => Mostrar(owner, mensaje, tipo, duracionMs, forzar));
                return;
            }

            // No mostrar si la app está minimizada, salvo forzar=true
            if (!forzar && owner.WindowState == FormWindowState.Minimized) return;

            // Descartar silenciosamente si ya hay el máximo de toasts
            lock (_lock)
            {
                if (_toastsActivos.Count >= MaxToasts) return;
            }

            var toast = new ToastNotification(mensaje, tipo, duracionMs);

            lock (_lock)
            {
                _toastsActivos.Add(toast);
            }

            toast.FormClosed += (s, e) =>
            {
                lock (_lock) { _toastsActivos.Remove(toast); }
                // Reposicionar los toasts restantes en el siguiente ciclo del message loop
                if (!owner.IsDisposed && owner.IsHandleCreated)
                    owner.BeginInvoke(() => RecalcularPosiciones(owner));
            };

            // Posicionar antes de mostrar para evitar salto visual
            RecalcularPosicionesTodasMas(owner, toast);
            toast.Show(owner);
        }

        /// <summary>
        /// Recalcula y aplica la posición de todos los toasts activos.
        /// Índice 0 = más antiguo, ocupa la posición inferior; cada siguiente queda encima.
        /// </summary>
        private static void RecalcularPosiciones(Form owner)
        {
            if (owner == null || owner.IsDisposed) return;

            List<ToastNotification> snapshot;
            lock (_lock)
            {
                snapshot = new List<ToastNotification>(_toastsActivos);
            }

            float scale  = owner.DeviceDpi / 96f;
            int margen   = (int)(16 * scale);
            int margenBottom = (int)(40 * scale);
            int gap      = (int)(GapToasts * scale);

            // Posición base: esquina inferior derecha del owner
            int baseX = owner.Left + owner.Width  - margen;
            int baseY = owner.Top  + owner.Height - margenBottom - margen;

            for (int i = 0; i < snapshot.Count; i++)
            {
                var t = snapshot[i];
                if (t == null || t.IsDisposed) continue;

                int x = baseX - t.Width;
                int y;
                if (i == 0)
                {
                    y = baseY - t.Height;
                }
                else
                {
                    var prev = snapshot[i - 1];
                    y = (prev == null || prev.IsDisposed)
                        ? baseY - t.Height
                        : prev.Top - t.Height - gap;
                }
                t.Location = new Point(x, y);
            }
        }

        /// <summary>
        /// Variante usada al agregar un nuevo toast: recalcula los existentes y
        /// posiciona el recién creado (aún no en la lista) en la posición correcta.
        /// Evita el salto visual que ocurriría si se añadiera a la lista antes de posicionar.
        /// </summary>
        private static void RecalcularPosicionesTodasMas(Form owner, ToastNotification nuevo)
        {
            // Primero reposiciona los ya existentes
            RecalcularPosiciones(owner);

            // Luego posiciona el nuevo encima del último activo
            float scale      = owner.DeviceDpi / 96f;
            int margen       = (int)(16 * scale);
            int margenBottom = (int)(40 * scale);
            int gap          = (int)(GapToasts * scale);

            int baseX = owner.Left + owner.Width  - margen;
            int baseY = owner.Top  + owner.Height - margenBottom - margen;

            List<ToastNotification> snapshot;
            lock (_lock)
            {
                snapshot = new List<ToastNotification>(_toastsActivos);
            }

            int x = baseX - nuevo.Width;
            int y;
            if (snapshot.Count == 0)
            {
                y = baseY - nuevo.Height;
            }
            else
            {
                var ultimo = snapshot[^1];
                y = (ultimo == null || ultimo.IsDisposed)
                    ? baseY - nuevo.Height
                    : ultimo.Top - nuevo.Height - gap;
            }
            nuevo.Location = new Point(x, y);
        }

        private void BuildUI(string mensaje, ToastTipo tipo)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Opacity = 0.95;
            this.BackColor = ObtenerColorFondo(tipo);
            this.Size = new Size(S(400), 0); // altura dinámica

            // Ancho del ícono fijo para que quede bien a cualquier DPI
            int iconLeft  = S(12);
            int iconWidth = S(24);
            int msgLeft   = iconLeft + iconWidth + S(4);
            int msgWidth  = this.Width - msgLeft - S(12);

            var lblIcono = new Label()
            {
                Text      = ObtenerIcono(tipo),
                Font      = new Font("Segoe UI", 14f),
                ForeColor = AppTheme.TextPrimary,
                Left      = iconLeft,
                Top       = 0,
                Width     = iconWidth,
                AutoSize  = false,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblMensaje = new Label()
            {
                Text      = mensaje,
                Font      = new Font("Segoe UI", 9f),
                ForeColor = AppTheme.TextPrimary,
                Left      = msgLeft,
                Top       = S(10),
                Width     = msgWidth,
                AutoSize  = false
            };

            using var g = Graphics.FromHwnd(IntPtr.Zero);
            var size = g.MeasureString(mensaje, lblMensaje.Font,
                new SizeF(msgWidth, 200));
            int alturaTexto = (int)Math.Ceiling(size.Height);
            lblMensaje.Height = alturaTexto + S(4);

            int alturaTotal  = Math.Max(S(48), alturaTexto + S(28));
            this.Height      = alturaTotal;
            lblIcono.Height  = alturaTotal;
            lblMensaje.Top   = (alturaTotal - lblMensaje.Height) / 2;

            var barraIzq = new Panel()
            {
                Left      = 0,
                Top       = 0,
                Width     = S(4),
                Height    = alturaTotal,
                BackColor = ObtenerColorBarra(tipo)
            };

            this.Controls.AddRange(new Control[] { barraIzq, lblIcono, lblMensaje });

            this.Paint += (s, e) =>
            {
                using var pen = new Pen(AppTheme.WhiteBorderPen, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
            };
        }

        private void IniciarFade()
        {
            int pasoFade = 50;
            int ticksEspera = _duracionMs / pasoFade;
            int tickActual = 0;
            double opacInicial = this.Opacity;

            _timerFade = new System.Windows.Forms.Timer { Interval = pasoFade };
            _timerFade.Tick += (s, e) =>
            {
                tickActual++;

                if (tickActual < ticksEspera)
                    return;

                double nuevaOpac = this.Opacity - (opacInicial / (1000.0 / pasoFade));
                if (nuevaOpac <= 0 || this.IsDisposed)
                {
                    _timerFade.Stop();
                    if (!this.IsDisposed) this.Close();
                    return;
                }
                this.Opacity = nuevaOpac;
            };
            _timerFade.Start();
        }

        // Helpers de color e ícono

        private static Color ObtenerColorFondo(ToastTipo tipo) => tipo switch
        {
            ToastTipo.Exito       => AppTheme.ToastBgSuccess,
            ToastTipo.Advertencia => AppTheme.ToastBgWarning,
            ToastTipo.Error       => AppTheme.ToastBgError,
            _                     => AppTheme.ToastBgInfo
        };

        private static Color ObtenerColorBarra(ToastTipo tipo) => tipo switch
        {
            ToastTipo.Exito       => AppTheme.ToastBarSuccess,
            ToastTipo.Advertencia => AppTheme.Warning,
            ToastTipo.Error       => AppTheme.Error,
            _                     => AppTheme.ToastBarInfo
        };

        private static string ObtenerIcono(ToastTipo tipo) => tipo switch
        {
            ToastTipo.Exito       => "✓",
            ToastTipo.Advertencia => "⚠",
            ToastTipo.Error       => "✗",
            _                     => "ℹ"
        };

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _timerFade?.Stop();
            _timerFade?.Dispose();
            base.OnFormClosed(e);
        }
    }
}
