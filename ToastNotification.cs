using System;
using System.Drawing;
using System.Windows.Forms;

namespace MobiladorStex
{
    /// <summary>
    /// Notificación flotante reutilizable que aparece en la esquina inferior derecha
    /// de la ventana padre y se desvanece automáticamente.
    /// Uso: ToastNotification.Mostrar(owner, "Mensaje", ToastTipo.Exito);
    /// </summary>
    public class ToastNotification : Form
    {
        public enum ToastTipo { Exito, Advertencia, Error, Info }

        private System.Windows.Forms.Timer _timerFade;
        private int _duracionMs;

        // ── Constructor privado — usar el método estático Mostrar ─────
        private ToastNotification(string mensaje, ToastTipo tipo, int duracionMs)
        {
            _duracionMs = duracionMs;
            BuildUI(mensaje, tipo);
            IniciarFade();
        }

        // ══════════════════════════════════════════════════════════════
        // API PÚBLICA
        // ══════════════════════════════════════════════════════════════

        // Toast activo actual — solo uno a la vez
        private static ToastNotification? _toastActivo;

        /// <summary>
        /// Muestra una notificación toast sobre la ventana owner.
        /// Solo se muestra si la ventana no está minimizada.
        /// Cierra cualquier toast previo antes de mostrar el nuevo.
        /// </summary>
        public static void Mostrar(Form owner, string mensaje,
            ToastTipo tipo = ToastTipo.Info, int duracionMs = 3000, bool forzar = false)
        {
            if (owner == null || owner.IsDisposed) return;

            // Crear y mostrar en el hilo de UI
            if (owner.InvokeRequired)
            {
                owner.Invoke(() => Mostrar(owner, mensaje, tipo, duracionMs, forzar));
                return;
            }

            // No mostrar si la app está minimizada, salvo forzar=true
            if (!forzar && owner.WindowState == FormWindowState.Minimized) return;

            // Cerrar toast anterior si existe
            if (_toastActivo != null && !_toastActivo.IsDisposed)
            {
                _toastActivo.Close();
                _toastActivo = null;
            }

            var toast = new ToastNotification(mensaje, tipo, duracionMs);
            _toastActivo = toast;
            toast.FormClosed += (s, e) =>
            {
                if (_toastActivo == toast) _toastActivo = null;
            };
            PosicionarSobreOwner(toast, owner);
            toast.Show(owner);
        }

        // ══════════════════════════════════════════════════════════════
        // UI
        // ══════════════════════════════════════════════════════════════

        private void BuildUI(string mensaje, ToastTipo tipo)
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Opacity = 0.95;
            this.BackColor = ObtenerColorFondo(tipo);
            this.Size = new Size(400, 0); // altura dinámica

            // ── Icono ─────────────────────────────────────────────────
            var lblIcono = new Label()
            {
                Text = ObtenerIcono(tipo),
                Font = new Font("Segoe UI", 14f),
                ForeColor = Color.White,
                Left = 12,
                Top = 14,
                AutoSize = true
            };

            // ── Mensaje ───────────────────────────────────────────────
            var lblMensaje = new Label()
            {
                Text = mensaje,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.White,
                Left = 42,
                Top = 10,
                Width = 346,
                AutoSize = false
            };

            // Calcular altura necesaria para el texto
            using var g = Graphics.FromHwnd(IntPtr.Zero);
            var size = g.MeasureString(mensaje, lblMensaje.Font,
                new SizeF(lblMensaje.Width, 200));
            int alturaTexto = (int)Math.Ceiling(size.Height);
            lblMensaje.Height = alturaTexto + 4;

            // Ajustar altura total del form
            int alturaTotal = Math.Max(48, alturaTexto + 28);
            this.Height = alturaTotal;
            lblIcono.Top = (alturaTotal - 22) / 2;
            lblMensaje.Top = (alturaTotal - lblMensaje.Height) / 2;

            // ── Barra de color izquierda ──────────────────────────────
            var barraIzq = new Panel()
            {
                Left = 0,
                Top = 0,
                Width = 4,
                Height = alturaTotal,
                BackColor = ObtenerColorBarra(tipo)
            };

            this.Controls.AddRange(new Control[] { barraIzq, lblIcono, lblMensaje });

            // Borde sutil
            this.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(60, 255, 255, 255), 1);
                e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
            };
        }

        private void IniciarFade()
        {
            // Esperar duracionMs y luego hacer fade out
            int pasoFade = 50;   // ms entre cada paso
            int pasoOpac = 5;    // reducción de opacidad por paso (0-100)
            int ticksEspera = _duracionMs / pasoFade;
            int tickActual = 0;
            double opacInicial = this.Opacity;

            _timerFade = new System.Windows.Forms.Timer { Interval = pasoFade };
            _timerFade.Tick += (s, e) =>
            {
                tickActual++;

                if (tickActual < ticksEspera)
                    return; // esperar antes de empezar fade

                // Fade out
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

        private static void PosicionarSobreOwner(ToastNotification toast, Form owner)
        {
            // Esquina inferior derecha del owner, con margen
            int margen = 16;
            int x = owner.Left + owner.Width - toast.Width - margen;
            int y = owner.Top + owner.Height - toast.Height - margen - 40;
            toast.Location = new Point(x, y);
        }

        // ══════════════════════════════════════════════════════════════
        // HELPERS DE COLOR E ICONO
        // ══════════════════════════════════════════════════════════════

        private static Color ObtenerColorFondo(ToastTipo tipo) => tipo switch
        {
            ToastTipo.Exito => Color.FromArgb(20, 80, 20),
            ToastTipo.Advertencia => Color.FromArgb(90, 60, 10),
            ToastTipo.Error => Color.FromArgb(90, 20, 20),
            _ => Color.FromArgb(25, 50, 80)   // Info
        };

        private static Color ObtenerColorBarra(ToastTipo tipo) => tipo switch
        {
            ToastTipo.Exito => Color.FromArgb(16, 185, 16),
            ToastTipo.Advertencia => Color.FromArgb(255, 167, 38),
            ToastTipo.Error => Color.FromArgb(220, 50, 50),
            _ => Color.FromArgb(0, 120, 212)   // Info
        };

        private static string ObtenerIcono(ToastTipo tipo) => tipo switch
        {
            ToastTipo.Exito => "✓",
            ToastTipo.Advertencia => "⚠",
            ToastTipo.Error => "✗",
            _ => "ℹ"
        };

        // ══════════════════════════════════════════════════════════════
        // LIMPIEZA
        // ══════════════════════════════════════════════════════════════

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _timerFade?.Stop();
            _timerFade?.Dispose();
            base.OnFormClosed(e);
        }
    }
}