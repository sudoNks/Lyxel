using Guna.UI2.WinForms;
using System.Drawing;
using System.Windows.Forms;

namespace MobiladorStex
{
    /// <summary>
    /// Diálogo de advertencia para funciones avanzadas (Opción 2 y 3).
    /// Requiere que el usuario marque los checkboxes antes de continuar.
    /// </summary>
    public class DialogoAvanzado : Form
    {
        public bool NoVolverMostrar { get; private set; } = false;

        private static readonly Color BG        = Color.FromArgb(33, 32, 35);
        private static readonly Color BG_CARD   = Color.FromArgb(42, 42, 45);
        private static readonly Color ACCENT    = Color.FromArgb(107, 47, 196);
        private static readonly Color TEXTO     = Color.FromArgb(238, 238, 238);
        private static readonly Color SECUNDARIO = Color.FromArgb(120, 120, 120);
        private static readonly Color NARANJA   = Color.FromArgb(255, 167, 38);

        private Guna2Button _btnContinuar;
        private CheckBox[]  _chkObligatorios;

        public DialogoAvanzado(string titulo, string descripcion, string[] checks)
        {
            this.Text            = "";
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.BackColor       = BG;
            this.Size            = new Size(500, 0); // altura se calcula después
            this.ShowInTaskbar   = false;

            BuildUI(titulo, descripcion, checks);
        }

        private void BuildUI(string titulo, string descripcion, string[] checks)
        {
            int y = 0;

            // ── Barra de título ───────────────────────────────────────
            var pnlTitulo = new Panel()
            {
                Left = 0, Top = 0, Width = this.Width, Height = 48,
                BackColor = Color.FromArgb(78, 28, 141)
            };
            var lblTitulo = new Label()
            {
                Text = "⚠  " + titulo,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = TEXTO, Left = 16, Top = 12, AutoSize = true
            };
            pnlTitulo.Controls.Add(lblTitulo);
            this.Controls.Add(pnlTitulo);
            y = 48;

            // ── Descripción ───────────────────────────────────────────
            var lblDesc = new Label()
            {
                Text = descripcion,
                Font = new Font("Segoe UI", 9f),
                ForeColor = SECUNDARIO,
                Left = 20, Top = y + 16,
                Width = this.Width - 40,
                AutoSize = false
            };
            // Calcular altura necesaria
            var g = this.CreateGraphics();
            var sz = g.MeasureString(descripcion, lblDesc.Font, lblDesc.Width);
            lblDesc.Height = (int)sz.Height + 4;
            g.Dispose();
            this.Controls.Add(lblDesc);
            y = lblDesc.Top + lblDesc.Height + 16;

            // ── Línea divisoria ───────────────────────────────────────
            var linea = new Panel()
            {
                Left = 20, Top = y, Width = this.Width - 40, Height = 1,
                BackColor = Color.FromArgb(78, 28, 141)
            };
            this.Controls.Add(linea);
            y += 12;

            // ── Checkboxes obligatorios ───────────────────────────────
            var lblPara = new Label()
            {
                Text = "Para continuar, confirma que entiendes lo siguiente:",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = TEXTO, Left = 20, Top = y, AutoSize = true
            };
            this.Controls.Add(lblPara);
            y += 24;

            _chkObligatorios = new CheckBox[checks.Length];
            for (int i = 0; i < checks.Length; i++)
            {
                var chk = new CheckBox()
                {
                    Text      = checks[i],
                    Font      = new Font("Segoe UI", 9f),
                    ForeColor = TEXTO,
                    BackColor = Color.Transparent,
                    Left      = 20, Top = y,
                    Width     = this.Width - 40,
                    AutoSize  = false, Height = 36
                };
                chk.CheckedChanged += (s, e) => ActualizarBoton();
                this.Controls.Add(chk);
                _chkObligatorios[i] = chk;
                y += 38;
            }

            y += 8;

            // ── Checkbox opcional ─────────────────────────────────────
            var chkNoMostrar = new CheckBox()
            {
                Text      = "No volver a mostrar este mensaje",
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = SECUNDARIO,
                BackColor = Color.Transparent,
                Left = 20, Top = y, AutoSize = true
            };
            chkNoMostrar.CheckedChanged += (s, e) =>
                NoVolverMostrar = chkNoMostrar.Checked;
            this.Controls.Add(chkNoMostrar);
            y += 32;

            // ── Botones ───────────────────────────────────────────────
            y += 8;
            var btnCancelar = new Guna2Button()
            {
                Text = "Cancelar", Width = 110, Height = 36,
                Left = this.Width - 244, Top = y,
                Font = new Font("Segoe UI", 9f),
                FillColor = Color.FromArgb(55, 40, 75),
                ForeColor = SECUNDARIO,
                BorderColor = Color.FromArgb(80, 60, 100),
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
                Text = "Continuar →", Width = 120, Height = 36,
                Left = this.Width - 130, Top = y,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                FillColor = Color.FromArgb(60, 60, 65), // deshabilitado inicialmente
                ForeColor = Color.FromArgb(100, 100, 100),
                BorderRadius = 6, Enabled = false
            };
            _btnContinuar.Click += (s, e) =>
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            };
            this.Controls.Add(_btnContinuar);
            y += 36 + 20;

            // ── Ajustar altura del form ───────────────────────────────
            this.Height = y;

            // ── Borde sutil ───────────────────────────────────────────
            this.Paint += (s, e) =>
            {
                using var pen = new System.Drawing.Pen(Color.FromArgb(78, 28, 141), 1);
                e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
            };

            // Arrastrar por la barra de título
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
            _btnContinuar.FillColor = todos ? Color.FromArgb(107, 47, 196) : Color.FromArgb(60, 60, 65);
            _btnContinuar.ForeColor = todos ? Color.FromArgb(238, 238, 238) : Color.FromArgb(100, 100, 100);
        }
    }
}
