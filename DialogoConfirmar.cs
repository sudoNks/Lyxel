using Guna.UI2.WinForms;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace LyXel
{
    /// <summary>
    /// Diálogo de confirmación simple, con tema LyXel. Alternativa a MessageBox.
    /// Uso:
    ///   using var dlg = new DialogoConfirmar("¿Aplicar DPI 420?", "Usa 'Resetear' si algo sale mal.");
    ///   if (dlg.ShowDialog(this) != DialogResult.OK) return;
    /// </summary>
    public class DialogoConfirmar : Form
    {
        private int S(int px) => (int)Math.Round(px * this.DeviceDpi / 96.0);

        public DialogoConfirmar(string mensaje, string subMensaje,
            string textoBtnOk = "Aplicar", string textoBtnCancel = "Cancelar",
            string titulo = "Confirmar")
        {
            this.Text            = "";
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition   = FormStartPosition.CenterParent;
            this.BackColor       = AppTheme.BgPrimary;
            this.Size            = new Size(S(320), S(160));
            this.ShowInTaskbar   = false;

            BuildUI(titulo, mensaje, subMensaje, textoBtnOk, textoBtnCancel);
        }

        private void BuildUI(string titulo, string mensaje, string subMensaje,
            string textoBtnOk, string textoBtnCancel)
        {
            // Barra de título
            var pnlHeader = new Panel()
            {
                Left = 0, Top = 0,
                Width = this.Width, Height = S(40),
                BackColor = AppTheme.BgCard
            };
            pnlHeader.Controls.Add(new Label()
            {
                Text = titulo,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = AppTheme.TextPrimary,
                Left = S(16), Top = S(10),
                AutoSize = true
            });
            this.Controls.Add(pnlHeader);

            // Mensaje principal
            this.Controls.Add(new Label()
            {
                Text = mensaje,
                Font = new Font("Segoe UI", 9.5f),
                ForeColor = AppTheme.TextPrimary,
                Left = S(20), Top = S(52),
                Width = this.Width - S(40), Height = S(22),
                AutoSize = false
            });

            // Sub-mensaje
            this.Controls.Add(new Label()
            {
                Text = subMensaje,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = AppTheme.TextSecondary,
                Left = S(20), Top = S(76),
                Width = this.Width - S(40), Height = S(20),
                AutoSize = false
            });

            // Botones (alineados a la derecha)
            int btnW     = S(110);
            int btnH     = S(34);
            int btnTop   = S(112);
            int rightEdge = this.Width - S(16);

            var btnCancelar = new Guna2Button()
            {
                Text            = textoBtnCancel,
                Left            = rightEdge - btnW * 2 - S(8),
                Top             = btnTop,
                Width           = btnW,
                Height          = btnH,
                Font            = new Font("Segoe UI", 9f),
                FillColor       = AppTheme.BgCard,
                ForeColor       = AppTheme.TextSecondary,
                BorderColor     = AppTheme.BorderNeutral,
                BorderThickness = 1,
                BorderRadius    = 5
            };
            btnCancelar.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            var btnOk = new Guna2Button()
            {
                Text         = textoBtnOk,
                Left         = rightEdge - btnW,
                Top          = btnTop,
                Width        = btnW,
                Height       = btnH,
                Font         = new Font("Segoe UI", 9f, FontStyle.Bold),
                FillColor    = AppTheme.Accent,
                ForeColor    = AppTheme.TextPrimary,
                BorderRadius = 5
            };
            btnOk.Click += (s, e) => { this.DialogResult = DialogResult.OK; this.Close(); };

            this.Controls.AddRange(new Control[] { btnCancelar, btnOk });

            // Borde exterior sutil
            this.Paint += (s, e) =>
            {
                using var pen = new Pen(AppTheme.BorderSecondary, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, this.Width - 1, this.Height - 1);
            };

            // Arrastreable desde la barra de título
            pnlHeader.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    pnlHeader.Capture = false;
                    var msg = Message.Create(this.Handle, 0xA1, (nint)2, nint.Zero);
                    this.WndProc(ref msg);
                }
            };
        }
    }
}
