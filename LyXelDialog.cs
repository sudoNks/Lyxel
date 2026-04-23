using Guna.UI2.WinForms;
using System.Drawing;
using System.Windows.Forms;

namespace LyXel
{
    public static class LyXelDialog
    {
        private static readonly Color _colorExito       = Color.FromArgb(45, 122, 45);
        private static readonly Color _colorAdvertencia = Color.FromArgb(184, 134, 11);
        private static readonly Color _colorError       = Color.FromArgb(139, 0, 0);

        public static void Info(Form? owner, string titulo, string mensaje)
            => MostrarSimple(owner, titulo, mensaje, AppTheme.Accent);

        public static void Exito(Form? owner, string titulo, string mensaje)
            => MostrarSimple(owner, titulo, mensaje, _colorExito);

        public static void Advertencia(Form? owner, string titulo, string mensaje)
            => MostrarSimple(owner, titulo, mensaje, _colorAdvertencia);

        public static void Error(Form? owner, string titulo, string mensaje)
            => MostrarSimple(owner, titulo, mensaje, _colorError);

        public static bool Confirmar(Form? owner, string titulo, string mensaje,
            string submensaje = "",
            string textoConfirmar = "Sí",
            string textoCancelar = "No")
        {
            bool tieneSub = !string.IsNullOrEmpty(submensaje);
            int h = tieneSub ? 200 : 180;
            bool resultado = false;

            using var dlg = CrearBase(owner, h);

            dlg.Controls.Add(CrearFranja(AppTheme.Accent, h));
            dlg.Controls.Add(CrearTitulo(titulo));
            dlg.Controls.Add(CrearMensaje(mensaje, 45));

            if (tieneSub)
                dlg.Controls.Add(CrearSubMensaje(submensaje, 80));

            int btnW = 90, btnH = 28, btnTop = h - 44, rightEdge = 352;

            var btnCancelar = new Guna2Button()
            {
                Text            = textoCancelar,
                Left            = rightEdge - btnW * 2 - 8,
                Top             = btnTop,
                Width           = btnW,
                Height          = btnH,
                Font            = new Font("Segoe UI", 9f),
                FillColor       = AppTheme.BgCard,
                ForeColor       = AppTheme.TextSecondary,
                BorderColor     = AppTheme.BorderNeutral,
                BorderThickness = 1,
                BorderRadius    = 4
            };
            var btnOk = new Guna2Button()
            {
                Text         = textoConfirmar,
                Left         = rightEdge - btnW,
                Top          = btnTop,
                Width        = btnW,
                Height       = btnH,
                Font         = new Font("Segoe UI", 9f, FontStyle.Bold),
                FillColor    = AppTheme.Accent,
                ForeColor    = AppTheme.TextPrimary,
                BorderRadius = 4
            };
            btnCancelar.Click += (s, e) => dlg.Close();
            btnOk.Click       += (s, e) => { resultado = true; dlg.Close(); };

            dlg.Controls.AddRange(new Control[] { btnCancelar, btnOk });
            AplicarComunes(dlg);

            dlg.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape) dlg.Close();
                else if (e.KeyCode == Keys.Enter) { resultado = true; dlg.Close(); }
            };

            if (owner != null) dlg.ShowDialog(owner);
            else dlg.ShowDialog();

            return resultado;
        }

        private static void MostrarSimple(Form? owner, string titulo, string mensaje, Color franjaColor)
        {
            using var dlg = CrearBase(owner, 180);

            dlg.Controls.Add(CrearFranja(franjaColor, 180));
            dlg.Controls.Add(CrearTitulo(titulo));
            dlg.Controls.Add(CrearMensaje(mensaje, 45));

            var btnAceptar = new Guna2Button()
            {
                Text         = "Aceptar",
                Left         = 352 - 90,
                Top          = 180 - 44,
                Width        = 90,
                Height       = 28,
                Font         = new Font("Segoe UI", 9f, FontStyle.Bold),
                FillColor    = AppTheme.Accent,
                ForeColor    = AppTheme.TextPrimary,
                BorderRadius = 4
            };
            btnAceptar.Click += (s, e) => dlg.Close();
            dlg.Controls.Add(btnAceptar);

            AplicarComunes(dlg);

            dlg.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Enter) dlg.Close();
            };

            if (owner != null) dlg.ShowDialog(owner);
            else dlg.ShowDialog();
        }

        private static Form CrearBase(Form? owner, int height) => new Form()
        {
            Text            = "",
            FormBorderStyle = FormBorderStyle.None,
            StartPosition   = owner != null ? FormStartPosition.CenterParent : FormStartPosition.CenterScreen,
            BackColor       = AppTheme.BgPrimary,
            Size            = new Size(360, height),
            ShowInTaskbar   = false,
            TopMost         = true
        };

        private static Panel CrearFranja(Color color, int height) => new Panel()
        {
            Left      = 0,
            Top       = 0,
            Width     = 8,
            Height    = height,
            BackColor = color
        };

        private static Label CrearTitulo(string texto) => new Label()
        {
            Text      = texto,
            Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
            ForeColor = AppTheme.TextPrimary,
            Left      = 24,
            Top       = 20,
            AutoSize  = true
        };

        private static Label CrearMensaje(string texto, int top) => new Label()
        {
            Text        = texto,
            Font        = new Font("Segoe UI", 9f),
            ForeColor   = AppTheme.TextPrimary,
            Left        = 24,
            Top         = top,
            MaximumSize = new Size(316, 0),
            AutoSize    = true
        };

        private static Label CrearSubMensaje(string texto, int top) => new Label()
        {
            Text        = texto,
            Font        = new Font("Segoe UI", 8.5f),
            ForeColor   = AppTheme.TextSecondary,
            Left        = 24,
            Top         = top,
            MaximumSize = new Size(316, 0),
            AutoSize    = true
        };

        private static void AplicarComunes(Form dlg)
        {
            dlg.KeyPreview = true;
            dlg.Paint += (s, e) =>
            {
                using var pen = new Pen(AppTheme.BorderSecondary, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, dlg.Width - 1, dlg.Height - 1);
            };
        }
    }
}
