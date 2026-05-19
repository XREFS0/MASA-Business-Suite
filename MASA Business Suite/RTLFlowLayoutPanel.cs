using System;
using System.Drawing;
using System.Windows.Forms;

namespace MASA_Business_Suite
{
    /// <summary>
    /// A custom FlowLayoutPanel designed to prevent the standard WinForms horizontal scrolling bug 
    /// that occurs when controls receive focus or are typed in inside Right-To-Left (RTL) layout containers.
    /// </summary>
    public class RTLFlowLayoutPanel : FlowLayoutPanel
    {
        public RTLFlowLayoutPanel()
        {
            this.DoubleBuffered = true;
        }

        protected override Point ScrollToControl(Control activeControl)
        {
            // Return the current AutoScrollPosition instead of standard scrolling calculations.
            // This prevents WinForms from jumping horizontally and breaking the layout when textboxes are typed in.
            return this.AutoScrollPosition;
        }
    }
}
