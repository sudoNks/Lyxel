using Guna.UI2.WinForms;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LyXel
{
    public class FloatingWindow : Form
    {
        // WinAPI para arrastre nativo y detección de fullscreen
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

        // El arrastre nativo de Windows funciona mejor que el manual, lo propago a todos los hijos
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

        // Campos de la ventana flotante
        private ScrcpyManager _scrcpyManager;
        private Action _onDetener;
        private Action _onMostrarApp;
        private System.Windows.Forms.Timer _timerEstado;
        private Label _lblFps;          // null si PrintFps está desactivado
        private bool _printFps;
        private Action<string>? _fpsHandler; // referencia para poder desuscribir

        private int S(int px) => (int)Math.Round(px * this.DeviceDpi / 96.0);

        public FloatingWindow(ScrcpyManager scrcpyManager, string infoText,
                              Action onDetener, Action onMostrarApp, bool printFps = false)
        {
            _scrcpyManager = scrcpyManager;
            _onDetener = onDetener;
            _onMostrarApp = onMostrarApp;
            _printFps = printFps;

            BuildUI(infoText);
            IniciarTimer();

            // Me suscribo al evento de FPS solo si el usuario activó PrintFps
            if (_printFps)
            {
                _fpsHandler = fps =>
                {
                    // El evento viene de un hilo de fondo — necesito Invoke para tocar la UI
                    if (_lblFps == null || _lblFps.IsDisposed) return;
                    try
                    {
                        if (_lblFps.IsHandleCreated)
                            _lblFps.Invoke(() => _lblFps.Text = $"● {fps}");
                    }
                    catch { /* la ventana ya se cerró, ignoro */ }
                };
                _scrcpyManager.OnFpsUpdate += _fpsHandler;
            }
        }

        private void BuildUI(string infoText)
        {
            this.Text = "";
            this.FormBorderStyle = FormBorderStyle.None;
            this.TopMost = true;
            this.StartPosition = FormStartPosition.Manual;
            this.BackColor = AppTheme.BgPrimary;
            this.Opacity = 0.92;
            this.ShowInTaskbar = false;

            // Si PrintFps está activo agrego espacio arriba para el contador
            int alturaExtra = _printFps ? S(28) : 0;
            this.Size = new Size(S(220), S(130) + alturaExtra);

            var screen = Screen.PrimaryScreen.WorkingArea;
            this.Location = new Point(
                screen.Right - this.Width - S(24),
                screen.Bottom / 2 - this.Height / 2);

            // Arrastre nativo vía WinAPI en el fondo de la ventana
            this.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) IniciarArrastre(); };

            // Label de FPS en la parte superior, solo si PrintFps está activo
            if (_printFps)
            {
                _lblFps = new Label()
                {
                    Text = "● esperando fps...",
                    Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                    ForeColor = AppTheme.AccentLight,
                    Left = S(12),
                    Top = S(10),
                    Width = S(196),
                    AutoSize = false
                };
                this.Controls.Add(_lblFps);
                _lblFps.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) IniciarArrastre(); };
            }

            // Si hay FPS arriba, bajo el resto del contenido 28px
            int oy = _printFps ? S(28) : 0;

            var lblTitulo = new Label()
            {
                Text = "▶  scrcpy corriendo",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = AppTheme.AccentLight,
                Left = S(12),
                Top = S(10) + oy,
                AutoSize = true
            };

            var lblInfo = new Label()
            {
                Text = infoText,
                Font = new Font("Segoe UI", 7.5f),
                ForeColor = AppTheme.TextLight,
                Left = S(12),
                Top = S(30) + oy,
                Width = S(196),
                AutoSize = false
            };

            var linea = new Panel()
            {
                Left = 0,
                Top = S(72) + oy,
                Width = S(220),
                Height = 1,
                BackColor = AppTheme.AccentDark
            };

            var btnDetener = new Guna2Button()
            {
                Text = "⏹ Detener",
                Width = S(94),
                Height = S(32),
                Left = S(12),
                Top = S(82) + oy,
                Font = new Font("Segoe UI", 8.5f),
                FillColor = AppTheme.BtnDangerDark,
                ForeColor = Color.White,
                BorderRadius = 4
            };

            var btnMostrar = new Guna2Button()
            {
                Text = "↑ Mostrar",
                Width = S(94),
                Height = S(32),
                Left = S(114),
                Top = S(82) + oy,
                Font = new Font("Segoe UI", 8.5f),
                FillColor = AppTheme.AccentDeep,
                ForeColor = AppTheme.AccentPale,
                BorderColor = AppTheme.Accent,
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

            // Propago el arrastre a los controles estáticos para que toda la ventana sea arrastrable
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
                // Si el usuario cerró scrcpy por su cuenta, cierro la flotante también
                if (!_scrcpyManager.EstaCorriendo)
                {
                    _timerEstado.Stop();
                    _onDetener?.Invoke();
                    this.Close();
                    return;
                }

                // Me oculto cuando scrcpy está en fullscreen y vuelvo a aparecer al salir
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
            if (_fpsHandler != null)
            {
                _scrcpyManager.OnFpsUpdate -= _fpsHandler;
                _fpsHandler = null;
            }
            base.OnFormClosed(e);
        }

        // Borde sutil para que la ventana no flote sin contorno
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using var pen = new System.Drawing.Pen(AppTheme.AccentDark, 1);
            e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
        }
    }
}