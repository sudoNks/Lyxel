using Guna.UI2.WinForms;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MobiladorStex
{
    public class FloatingWindowOtg : Form
    {
        // ── WinAPI ────────────────────────────────────────────────────
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION       = 0x2;

        private void IniciarArrastre()
        {
            ReleaseCapture();
            SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }

        // ── Campos ────────────────────────────────────────────────────
        private readonly ScrcpyManager _scrcpyManager;
        private readonly Action _onDetener;
        private readonly Action _onMostrarApp;
        private System.Windows.Forms.Timer _timerEstado;

        private int S(int px) => (int)Math.Round(px * this.DeviceDpi / 96.0);

        public FloatingWindowOtg(ScrcpyManager scrcpyManager, string serial,
                                  string shortcutMod, Action onDetener, Action onMostrarApp)
        {
            _scrcpyManager = scrcpyManager;
            _onDetener     = onDetener;
            _onMostrarApp  = onMostrarApp;

            BuildUI(serial, shortcutMod);
            IniciarTimer();
        }

        private void BuildUI(string serial, string shortcutMod)
        {
            this.Text            = "";
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost         = true;
            this.StartPosition   = FormStartPosition.Manual;
            this.BackColor       = Color.FromArgb(33, 32, 35);
            this.Opacity         = 0.92;
            this.ShowInTaskbar   = false;
            this.Size            = new Size(S(264), S(156));

            var screen = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(
                screen.Right  - this.Width  - S(24),
                screen.Bottom / 2 - this.Height / 2);

            this.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) IniciarArrastre(); };

            var lblTitulo = new Label()
            {
                Text      = "MobiladorSteX — Modo OTG",
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(147, 90, 220),
                Left = S(12), Top = S(10), Width = S(240), Height = S(20),
                AutoSize = false
            };

            var lblEstado = new Label()
            {
                Text      = "⚡ OTG Activo",
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 200, 80),
                Left = S(12), Top = S(34), Width = S(240), Height = S(20),
                AutoSize = false
            };

            string dispositivoTexto = string.IsNullOrWhiteSpace(serial)
                ? "Dispositivo: detección automática"
                : $"Dispositivo: {serial}";

            var lblDispositivo = new Label()
            {
                Text      = dispositivoTexto,
                Font      = new Font("Segoe UI", 7.5f),
                ForeColor = Color.FromArgb(200, 200, 200),
                Left = S(12), Top = S(60), Width = S(240), Height = S(16),
                AutoSize = false
            };

            string modTexto = string.IsNullOrEmpty(shortcutMod) ? "lalt" : shortcutMod;
            var lblMod = new Label()
            {
                Text      = $"Tecla MOD: {modTexto}",
                Font      = new Font("Segoe UI", 7.5f),
                ForeColor = Color.FromArgb(160, 150, 180),
                Left = S(12), Top = S(80), Width = S(240), Height = S(16),
                AutoSize = false
            };

            var linea = new Panel()
            {
                Left      = 0,
                Top       = S(100),
                Width     = S(264),
                Height    = 1,
                BackColor = Color.FromArgb(78, 28, 141)
            };

            var btnDetener = new Guna2Button()
            {
                Text         = "⏹ Detener",
                Width        = S(116), Height = S(32),
                Left         = S(12), Top = S(110),
                Font         = new Font("Segoe UI", 8.5f),
                FillColor    = Color.FromArgb(160, 30, 30),
                ForeColor    = Color.White,
                BorderRadius = 4
            };

            var btnMostrar = new Guna2Button()
            {
                Text            = "↑ Mostrar launcher",
                Width           = S(116), Height = S(32),
                Left            = S(136), Top = S(110),
                Font            = new Font("Segoe UI", 8.5f),
                FillColor       = Color.FromArgb(55, 28, 100),
                ForeColor       = Color.FromArgb(220, 200, 255),
                BorderColor     = Color.FromArgb(107, 47, 196),
                BorderThickness = 1,
                BorderRadius    = 4
            };

            btnDetener.Click += (s, e) =>
            {
                _timerEstado?.Stop();
                _onDetener?.Invoke();
                this.Close();
            };

            btnMostrar.Click += (s, e) =>
            {
                _onMostrarApp?.Invoke();
                this.Hide();
            };

            // Propagar arrastre a controles estáticos
            foreach (Control c in new Control[] { lblTitulo, lblEstado, lblDispositivo, lblMod, linea })
                c.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) IniciarArrastre(); };

            this.Controls.AddRange(new Control[]
            {
                lblTitulo, lblEstado, lblDispositivo, lblMod, linea, btnDetener, btnMostrar
            });
        }

        private void IniciarTimer()
        {
            _timerEstado = new System.Windows.Forms.Timer { Interval = 500 };
            _timerEstado.Tick += (s, e) =>
            {
                if (!_scrcpyManager.EstaCorriendo)
                {
                    _timerEstado.Stop();
                    _onDetener?.Invoke();
                    this.Close();
                }
            };
            _timerEstado.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _timerEstado?.Stop();
            _timerEstado?.Dispose();
            base.OnFormClosed(e);
        }

        // Borde sutil morado
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using var pen = new System.Drawing.Pen(Color.FromArgb(78, 28, 141), 1);
            e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
        }
    }
}
