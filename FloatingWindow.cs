using Guna.UI2.WinForms;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MobiladorStex
{
    public class FloatingWindow : Form
    {
        // ── WinAPI ────────────────────────────────────────────────────
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        // Arrastre nativo — funciona sobre cualquier control hijo
        private void IniciarArrastre()
        {
            ReleaseCapture();
            SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
        }

        private bool EstaEnFullscreen()
        {
            var hwnd = GetForegroundWindow();
            if (hwnd == IntPtr.Zero) return false;
            GetWindowRect(hwnd, out RECT rect);
            var screen = Screen.PrimaryScreen.Bounds;
            return rect.Left <= 0 && rect.Top <= 0
                && rect.Right >= screen.Width
                && rect.Bottom >= screen.Height;
        }

        // ── Campos ────────────────────────────────────────────────────
        private ScrcpyManager _scrcpyManager;
        private Action _onDetener;
        private Action _onMostrarApp;
        private System.Windows.Forms.Timer _timerEstado;
        private Label _lblFps;          // null si PrintFps está desactivado
        private bool _printFps;

        public FloatingWindow(ScrcpyManager scrcpyManager, string infoText,
                              Action onDetener, Action onMostrarApp, bool printFps = false)
        {
            _scrcpyManager = scrcpyManager;
            _onDetener = onDetener;
            _onMostrarApp = onMostrarApp;
            _printFps = printFps;

            BuildUI(infoText);
            IniciarTimer();

            // Suscribirse al evento solo si PrintFps está activo
            if (_printFps)
            {
                _scrcpyManager.OnFpsUpdate += fps =>
                {
                    // El evento viene de un hilo de fondo — Invoke para tocar la UI
                    if (_lblFps == null || _lblFps.IsDisposed) return;
                    try
                    {
                        if (_lblFps.IsHandleCreated)
                            _lblFps.Invoke(() => _lblFps.Text = $"● {fps}");
                    }
                    catch { /* ventana cerrada */ }
                };
            }
        }

        private void BuildUI(string infoText)
        {
            this.Text = "";
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.StartPosition = FormStartPosition.Manual;
            this.BackColor = Color.FromArgb(33, 32, 35);
            this.Opacity = 0.92;
            this.ShowInTaskbar = false;

            // Altura: 130 base + 28 extra si hay FPS
            int alturaExtra = _printFps ? 28 : 0;
            this.Size = new Size(220, 130 + alturaExtra);

            var screen = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(
                screen.Right - this.Width - 24,
                screen.Bottom / 2 - this.Height / 2);

            // ── Arrastre nativo vía WinAPI ────────────────────────────
            this.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) IniciarArrastre(); };

            // ── Label FPS (visible solo si PrintFps activo) ───────────
            // Se muestra en la parte superior con color verde brillante
            if (_printFps)
            {
                _lblFps = new Label()
                {
                    Text = "● esperando fps...",
                    Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(147, 90, 220),
                    Left = 12,
                    Top = 10,
                    Width = 196,
                    AutoSize = false
                };
                this.Controls.Add(_lblFps);
                _lblFps.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) IniciarArrastre(); };
            }

            // Offset vertical: si hay FPS el resto baja 28px
            int oy = _printFps ? 28 : 0;

            var lblTitulo = new Label()
            {
                Text = "▶  scrcpy corriendo",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(147, 90, 220),
                Left = 12,
                Top = 10 + oy,
                AutoSize = true
            };

            var lblInfo = new Label()
            {
                Text = infoText,
                Font = new Font("Segoe UI", 7.5f),
                ForeColor = Color.FromArgb(200, 200, 200),
                Left = 12,
                Top = 30 + oy,
                Width = 196,
                AutoSize = false
            };

            var linea = new Panel()
            {
                Left = 0,
                Top = 72 + oy,
                Width = 220,
                Height = 1,
                BackColor = Color.FromArgb(78, 28, 141)
            };

            var btnDetener = new Guna2Button()
            {
                Text = "⏹ Detener",
                Width = 94,
                Height = 32,
                Left = 12,
                Top = 82 + oy,
                Font = new Font("Segoe UI", 8.5f),
                FillColor = Color.FromArgb(160, 30, 30),
                ForeColor = Color.White,
                BorderRadius = 4
            };

            var btnMostrar = new Guna2Button()
            {
                Text = "↑ Mostrar",
                Width = 94,
                Height = 32,
                Left = 114,
                Top = 82 + oy,
                Font = new Font("Segoe UI", 8.5f),
                FillColor = Color.FromArgb(55, 28, 100),
                ForeColor = Color.FromArgb(220, 200, 255),
                BorderColor = Color.FromArgb(107, 47, 196),
                BorderThickness = 1,
                BorderRadius = 4
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
            foreach (Control c in new Control[] { lblTitulo, lblInfo, linea })
                c.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) IniciarArrastre(); };

            this.Controls.AddRange(new Control[]
            {
                lblTitulo, lblInfo, linea, btnDetener, btnMostrar
            });
        }

        private void IniciarTimer()
        {
            _timerEstado = new System.Windows.Forms.Timer { Interval = 500 };
            _timerEstado.Tick += (s, e) =>
            {
                // Si scrcpy se cerró externamente
                if (!_scrcpyManager.EstaCorriendo)
                {
                    _timerEstado.Stop();
                    _onDetener?.Invoke();
                    this.Close();
                    return;
                }

                // Ocultar en fullscreen, mostrar al salir
                if (EstaEnFullscreen())
                {
                    if (this.Visible) this.Hide();
                }
                else
                {
                    if (!this.Visible) this.Show();
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

        // Borde sutil
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using var pen = new System.Drawing.Pen(Color.FromArgb(78, 28, 141), 1);
            e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
        }
    }
}