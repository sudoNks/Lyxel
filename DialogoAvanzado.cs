using Guna.UI2.WinForms;
using System.Drawing;
using System.Windows.Forms;

namespace LyXel
{
    /// <summary>
    /// Diálogo de advertencia para funciones avanzadas (Opción 2 y 3).
    /// Requiere que el usuario marque los checkboxes antes de continuar.
    /// </summary>
    public class DialogoAvanzado : Form
    {
        public bool NoVolverMostrar { get; private set; } = false;

        private static readonly Color BG        = AppTheme.BgPrimary;
        private static readonly Color BG_CARD   = AppTheme.BgCard;
        private static readonly Color ACCENT    = AppTheme.Accent;
        private static readonly Color TEXTO     = AppTheme.TextPrimary;
        private static readonly Color SECUNDARIO = AppTheme.TextSecondary;
        private static readonly Color NARANJA   = AppTheme.Warning;

        private Guna2Button _btnContinuar;
        private CheckBox[]  _chkObligatorios;

        private int S(int px) => (int)Math.Round(px * this.DeviceDpi / 96.0);

        public DialogoAvanzado(string titulo, string descripcion, string[] checks)
        {
            this.Text            = "";
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.BackColor       = BG;
            this.Size            = new Size(S(500), 0); // altura se calcula después
            this.ShowInTaskbar   = false;

            BuildUI(titulo, descripcion, checks);
        }

        private void BuildUI(string titulo, string descripcion, string[] checks)
        {
            int y = 0;

            // Barra de título con ícono de advertencia
            var pnlTitulo = new Panel()
            {
                Left = 0, Top = 0, Width = this.Width, Height = S(48),
                BackColor = AppTheme.AccentDark
            };
            var lblTitulo = new Label()
            {
                Text = "⚠  " + titulo,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = TEXTO, Left = S(16), Top = S(12), AutoSize = true
            };
            pnlTitulo.Controls.Add(lblTitulo);
            this.Controls.Add(pnlTitulo);
            y = S(48);

            // Descripción del diálogo
            var lblDesc = new Label()
            {
                Text = descripcion,
                Font = new Font("Segoe UI", 9f),
                ForeColor = SECUNDARIO,
                Left = S(20), Top = y + S(16),
                Width = this.Width - S(40),
                AutoSize = false
            };
            // MeasureString devuelve píxeles DPI-aware, así que no aplico escala extra
            var g = this.CreateGraphics();
            var sz = g.MeasureString(descripcion, lblDesc.Font, lblDesc.Width);
            lblDesc.Height = (int)sz.Height + S(4);
            g.Dispose();
            this.Controls.Add(lblDesc);
            y = lblDesc.Top + lblDesc.Height + S(16);

            // Línea divisoria
            var linea = new Panel()
            {
                Left = S(20), Top = y, Width = this.Width - S(40), Height = 1,
                BackColor = AppTheme.AccentDark
            };
            this.Controls.Add(linea);
            y += S(12);

            // Checkboxes que el usuario debe marcar todos para poder continuar
            var lblPara = new Label()
            {
                Text = "Para continuar, confirma que entiendes lo siguiente:",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = TEXTO, Left = S(20), Top = y, AutoSize = true
            };
            this.Controls.Add(lblPara);
            y += S(24);

            _chkObligatorios = new CheckBox[checks.Length];
            for (int i = 0; i < checks.Length; i++)
            {
                var chk = new CheckBox()
                {
                    Text      = checks[i],
                    Font      = new Font("Segoe UI", 9f),
                    ForeColor = TEXTO,
                    BackColor = Color.Transparent,
                    Left      = S(20), Top = y,
                    Width     = this.Width - S(40),
                    AutoSize  = false, Height = S(36)
                };
                chk.CheckedChanged += (s, e) => ActualizarBoton();
                this.Controls.Add(chk);
                _chkObligatorios[i] = chk;
                y += S(38);
            }

            y += S(8);

            // Checkbox opcional para no volver a mostrar
            var chkNoMostrar = new CheckBox()
            {
                Text      = "No volver a mostrar este mensaje",
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = SECUNDARIO,
                BackColor = Color.Transparent,
                Left = S(20), Top = y, AutoSize = true
            };
            chkNoMostrar.CheckedChanged += (s, e) =>
                NoVolverMostrar = chkNoMostrar.Checked;
            this.Controls.Add(chkNoMostrar);
            y += S(32);

            // Botones de acción
            y += S(8);
            var btnCancelar = new Guna2Button()
            {
                Text = "Cancelar", Width = S(110), Height = S(36),
                Left = this.Width - S(244), Top = y,
                Font = new Font("Segoe UI", 9f),
                FillColor = AppTheme.BtnSecondary,
                ForeColor = SECUNDARIO,
                BorderColor = AppTheme.BorderSecondary,
                BorderThickness = 1, BorderRadius = 6
            };
            btnCancelar.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            };
            this.Controls.Add(btnCancelar);

            _btnContinuar = new Guna2Button()
            {
                Text = "Continuar →", Width = S(120), Height = S(36),
                Left = this.Width - S(130), Top = y,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                FillColor = AppTheme.BtnDisabled, // deshabilitado inicialmente
                ForeColor = AppTheme.TextDisabled,
                BorderRadius = 6, Enabled = false
            };
            _btnContinuar.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            this.Controls.Add(_btnContinuar);
            y += S(36) + S(20);

            // Ajusto la altura del form al contenido real
            this.Height = y;

            // Borde sutil pintado a mano en OnPaint
            this.Paint += (s, e) =>
            {
                using var pen = new System.Drawing.Pen(AppTheme.AccentDark, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
            };

            // Arrastre nativo de Windows para mover el diálogo por la barra de título
            pnlTitulo.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    pnlTitulo.Capture = false;
                    var msg = Message.Create(this.Handle, 0xA1, (nint)2, nint.Zero);
                    this.WndProc(ref msg);
                }
            };
        }

        private void ActualizarBoton()
        {
            bool todos = true;
            foreach (var chk in _chkObligatorios)
                if (!chk.Checked) { todos = false; break; }

            _btnContinuar.Enabled   = todos;
            _btnContinuar.FillColor = todos ? AppTheme.Accent : AppTheme.BtnDisabled;
            _btnContinuar.ForeColor = todos ? AppTheme.TextPrimary : AppTheme.TextDisabled;
        }
    }
}
