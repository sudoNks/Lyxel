using Guna.UI2.WinForms;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LyXel
{
    /// <summary>
    /// Bloquea WM_MOUSEWHEEL en controles de tipo ComboBox, TrackBar, NumericUpDown y
    /// StexNumericUpDown cuando el control bajo el cursor no tiene el foco. Así el scroll
    /// solo actúa sobre el control con el que el usuario interactuó explícitamente.
    /// Cuando bloquea, redirige el mensaje al panel AutoScroll más cercano para que la
    /// página haga scroll normalmente.
    /// Registrar una sola vez con Application.AddMessageFilter(new ScrollFocusFilter()).
    /// </summary>
    internal sealed class ScrollFocusFilter : IMessageFilter
    {
        private const int WM_MOUSEWHEEL = 0x020A;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg != WM_MOUSEWHEEL) return false;

            Control? control = Control.FromHandle(m.HWnd);
            Control? candidate = null;
            Control? walker = control;
            while (walker != null)
            {
                if (walker is ComboBox or TrackBar or Guna2TrackBar or NumericUpDown or StexNumericUpDown)
                {
                    candidate = walker;
                    break;
                }
                walker = walker.Parent;
            }

            if (candidate == null) return false; // control neutro → dejar pasar al panel

            if (candidate.ContainsFocus) return false; // tiene foco → dejar pasar al control

            // Sin foco: bloquear al control y redirigir al panel ancestor con AutoScroll
            // SendMessage es síncrono y va directo al WndProc, sin pasar por esta filter
            ScrollableControl? panel = FindScrollableAncestor(candidate);
            if (panel != null)
                SendMessage(panel.Handle, WM_MOUSEWHEEL, m.WParam, m.LParam);

            return true; // bloquear el mensaje original
        }

        private static ScrollableControl? FindScrollableAncestor(Control control)
        {
            Control? parent = control.Parent;
            while (parent != null)
            {
                if (parent is ScrollableControl sc && sc.AutoScroll)
                    return sc;
                parent = parent.Parent;
            }
            return null;
        }
    }
}
