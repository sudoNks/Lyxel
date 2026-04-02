using Guna.UI2.WinForms;
using LyXel.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace LyXel
{
    /// <summary>
    /// Control numérico personalizado con paleta LyXel.
    /// Reemplaza Guna2NumericUpDown con control total sobre colores.
    /// Uso: igual que NumericUpDown — Value, Minimum, Maximum, Increment
    /// </summary>
    public class StexNumericUpDown : UserControl
    {
        // Propiedades públicas que expongo al exterior
        private decimal _value = 0;
        private decimal _minimum = 0;
        private decimal _maximum = 100;
        private decimal _increment = 1;

        public event EventHandler? ValueChanged;

        public decimal Value
        {
            get => _value;
            set
            {
                decimal clamped = Math.Max(_minimum, Math.Min(_maximum, value));
                if (clamped == _value) return;
                _value = clamped;
                _txtValue.Text = _value.ToString();
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public decimal Minimum
        {
            get => _minimum;
            set { _minimum = value; if (_value < _minimum) Value = _minimum; }
        }

        public decimal Maximum
        {
            get => _maximum;
            set { _maximum = value; if (_value > _maximum) Value = _maximum; }
        }

        public decimal Increment
        {
            get => _increment;
            set { _increment = Math.Max(1, value); }
        }

        // Controles internos del composite
        private Guna2TextBox _txtValue;
        private Guna2Button _btnUp;
        private Guna2Button _btnDown;

        // Colores hardcodeados de la paleta — si cambio el tema los actualizo aquí
        private static readonly Color COLOR_FONDO = AppTheme.BgCard;
        private static readonly Color COLOR_TEXTO = AppTheme.TextPrimary;
        private static readonly Color COLOR_BOTON = AppTheme.AccentDark;
        private static readonly Color COLOR_HOVER = AppTheme.Accent;
        private static readonly Color COLOR_BORDE = AppTheme.Accent;

        public StexNumericUpDown()
        {
            this.Size = new Size(100, 32);
            this.BackColor = COLOR_FONDO;
            this.BorderStyle = BorderStyle.None;

            BuildUI();
        }

        private void BuildUI()
        {
            int btnW = 26;

            // TextBox central donde va el número
            _txtValue = new Guna2TextBox()
            {
                Left = 0,
                Top = 0,
                Width = this.Width - (btnW * 2) - 2,
                Height = this.Height,
                Text = _value.ToString(),
                Font = new Font("Segoe UI", 9.5f),
                FillColor = COLOR_FONDO,
                ForeColor = COLOR_TEXTO,
                BorderColor = COLOR_BORDE,
                BorderRadius = 0,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom
            };

            // Botón de decremento, a la izquierda del botón de incremento
            _btnDown = new Guna2Button()
            {
                Left = _txtValue.Width + 1,
                Top = 0,
                Width = btnW,
                Height = this.Height,
                Text = "",
                Image = IconMap.ArrowBack,
                ImageSize = new Size(14, 14),
                FillColor = COLOR_BOTON,
                ForeColor = COLOR_TEXTO,
                BorderRadius = 0,
                BorderColor = COLOR_BORDE,
                BorderThickness = 1,
                Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
            };

            // Botón de incremento, a la derecha
            _btnUp = new Guna2Button()
            {
                Left = _txtValue.Width + btnW + 2,
                Top = 0,
                Width = btnW,
                Height = this.Height,
                Text = "",
                Image = IconMap.ArrowNext,
                ImageSize = new Size(14, 14),
                FillColor = COLOR_BOTON,
                ForeColor = COLOR_TEXTO,
                BorderRadius = 0,
                BorderColor = COLOR_BORDE,
                BorderThickness = 1,
                Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom
            };

            // Eventos de los botones y del textbox
            _btnUp.Click += (s, e) => Value += _increment;
            _btnDown.Click += (s, e) => Value -= _increment;

            _btnUp.MouseEnter += (s, e) => _btnUp.FillColor = COLOR_HOVER;
            _btnUp.MouseLeave += (s, e) => _btnUp.FillColor = COLOR_BOTON;
            _btnDown.MouseEnter += (s, e) => _btnDown.FillColor = COLOR_HOVER;
            _btnDown.MouseLeave += (s, e) => _btnDown.FillColor = COLOR_BOTON;

            // Confirmo el valor cuando el usuario sale del textbox o presiona Enter
            _txtValue.Leave += (s, e) => ConfirmarTexto();
            _txtValue.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    ConfirmarTexto();
                    this.Parent?.Focus();
                }
                // Solo dejo pasar números y teclas de navegación, el resto lo bloqueo
                bool esNumero = (e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9)
                                || (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9);
                bool esControl = e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete
                                || e.KeyCode == Keys.Left || e.KeyCode == Keys.Right
                                || e.KeyCode == Keys.Tab;
                bool esMenos = e.KeyCode == Keys.OemMinus || e.KeyCode == Keys.Subtract;
                if (!esNumero && !esControl && !esMenos)
                    e.SuppressKeyPress = true;
            };

            // El scroll del mouse funciona solo si el control tiene el foco, para no interferir con el scroll de la página
            this.MouseWheel += (s, e) =>
            {
                if (!this.ContainsFocus) return;
                if (e.Delta > 0) Value += _increment;
                else Value -= _increment;
            };

            this.Controls.AddRange(new Control[] { _txtValue, _btnUp, _btnDown });
        }

        private void ConfirmarTexto()
        {
            if (decimal.TryParse(_txtValue.Text, out decimal parsed))
                Value = parsed;
            else
                _txtValue.Text = _value.ToString(); // revertir si inválido
        }

        // Recalculo el layout de los controles internos al redimensionar
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (_txtValue == null) return;
            int btnW = 26;
            _txtValue.Width = this.Width - (btnW * 2) - 2;
            _txtValue.Height = this.Height;
            _btnDown.Left = _txtValue.Width + 1;
            _btnDown.Height = this.Height;
            _btnUp.Left = _txtValue.Width + btnW + 2;
            _btnUp.Height = this.Height;
        }
    }
}