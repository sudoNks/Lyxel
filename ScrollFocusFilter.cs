using System.Windows.Forms;

namespace LyXel
{
    /// <summary>
    /// Bloquea WM_MOUSEWHEEL en controles de tipo ComboBox, TrackBar y NumericUpDown
    /// cuando el control bajo el cursor no tiene el foco. Así el scroll solo actúa
    /// sobre el control con el que el usuario interactuó explícitamente.
    /// Registrar una sola vez con Application.AddMessageFilter(new ScrollFocusFilter()).
    /// </summary>
    internal sealed class ScrollFocusFilter : IMessageFilter
    {
        private const int WM_MOUSEWHEEL = 0x020A;

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg != WM_MOUSEWHEEL) return false;

            Control? control = Control.FromHandle(m.HWnd);
            while (control != null)
            {
                if (control is ComboBox or TrackBar or NumericUpDown or StexNumericUpDown)
                    return !control.ContainsFocus;
                control = control.Parent;
            }
            return false;
        }
    }
}
