using System;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace MASA_Business_Suite
{
    public static class ThemeManager
    {
        public static bool IsDarkMode = false;

        // Unified harmony-curated color palettes
        // Light Mode: Slate-50 background, Slate-100 borders, Slate-800 text, White cards
        // Dark Mode: Deep navy slate (#0f172a) background, Slate-800 (#1e293b) cards, Slate-700 (#334155) borders, Slate-100 (#f1f5f9) text
        public static Color BackgroundColor => IsDarkMode ? Color.FromArgb(15, 23, 42) : Color.FromArgb(248, 250, 252);
        public static Color CardColor => IsDarkMode ? Color.FromArgb(30, 41, 59) : Color.White;
        public static Color TextColor => IsDarkMode ? Color.FromArgb(241, 245, 249) : Color.FromArgb(15, 23, 42);
        public static Color BorderColor => IsDarkMode ? Color.FromArgb(51, 65, 85) : Color.FromArgb(226, 232, 240);
        public static Color SubtextColor => IsDarkMode ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139);

        private static Icon _appIcon = null;
        public static Icon AppIcon
        {
            get
            {
                if (_appIcon == null)
                {
                    try
                    {
                        string[] paths = new string[]
                        {
                            System.IO.Path.Combine(Application.StartupPath, "Icons", "logo.ico"),
                            System.IO.Path.Combine(Application.StartupPath, "..", "..", "Icons", "logo.ico"),
                            System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Icons", "logo.ico")
                        };

                        foreach (string p in paths)
                        {
                            if (System.IO.File.Exists(p))
                            {
                                _appIcon = new Icon(p);
                                break;
                            }
                        }
                    }
                    catch { }
                }
                return _appIcon;
            }
        }

        public static void ApplyTheme(Form form)
        {
            form.BackColor = BackgroundColor;
            
            try
            {
                if (AppIcon != null)
                {
                    form.Icon = AppIcon;
                }
            }
            catch { }
            
            foreach (Control ctrl in form.Controls)
            {
                ApplyThemeToControl(ctrl);
            }
            
            form.Invalidate(true);
        }

        private static bool IsInsideCard(Control ctrl, out Color containerColor)
        {
            containerColor = Color.Transparent;
            Control parent = ctrl.Parent;
            while (parent != null)
            {
                if (parent is Guna2Panel gPnl && gPnl.Name != "pnlSidebar" && gPnl.Name != "pnlNavbar" && gPnl.Name != "pnlTitleBar" && gPnl.Name != "pnlLogo")
                {
                    containerColor = CardColor;
                    return true;
                }
                parent = parent.Parent;
            }
            return false;
        }

        private static void ApplyThemeToControl(Control ctrl)
        {
            Color parentCardColor;
            bool insideCard = IsInsideCard(ctrl, out parentCardColor);

            if (ctrl is Guna2Panel pnl)
            {
                pnl.FillColor = CardColor;
                pnl.CustomBorderColor = BorderColor;
                if (pnl.Name == "pnlSidebar" || pnl.Name == "pnlNavbar" || pnl.Name == "pnlTitleBar" || pnl.Name == "pnlLogo")
                {
                    pnl.BackColor = CardColor;
                }
                else
                {
                    pnl.BackColor = Color.Transparent;
                }
            }
            else if (ctrl is FlowLayoutPanel flp)
            {
                flp.BackColor = insideCard ? parentCardColor : (flp.BackColor == Color.Transparent ? Color.Transparent : CardColor);
            }
            else if (ctrl is RTLFlowLayoutPanel rflp)
            {
                rflp.BackColor = insideCard ? parentCardColor : (rflp.BackColor == Color.Transparent ? Color.Transparent : CardColor);
            }
            else if (ctrl is TableLayoutPanel tlp)
            {
                tlp.BackColor = insideCard ? parentCardColor : (tlp.BackColor == Color.Transparent ? Color.Transparent : BackgroundColor);
            }
            else if (ctrl is Panel p)
            {
                if (p.Name != "pnlContent")
                {
                    p.BackColor = insideCard ? parentCardColor : (p.BackColor == Color.Transparent ? Color.Transparent : CardColor);
                }
            }
            else if (ctrl is Label lbl)
            {
                lbl.BackColor = insideCard ? parentCardColor : Color.Transparent;

                // Keep vibrant accent colors intact (e.g. green status, red danger labels, blue totals)
                if (lbl.ForeColor != Color.White && 
                    lbl.ForeColor != Color.FromArgb(16, 185, 129) && 
                    lbl.ForeColor != Color.FromArgb(59, 130, 246) && 
                    lbl.ForeColor != Color.FromArgb(37, 99, 235) && 
                    lbl.ForeColor != Color.FromArgb(239, 68, 68))
                {
                    lbl.ForeColor = TextColor;
                }
            }
            else if (ctrl is Guna2DataGridView dgv)
            {
                dgv.BackgroundColor = IsDarkMode ? BackgroundColor : Color.White;
                dgv.GridColor = BorderColor;

                dgv.ColumnHeadersDefaultCellStyle.BackColor = IsDarkMode ? Color.FromArgb(30, 41, 59) : Color.FromArgb(241, 245, 249);
                dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextColor;
                dgv.ColumnHeadersDefaultCellStyle.SelectionBackColor = IsDarkMode ? Color.FromArgb(30, 41, 59) : Color.FromArgb(241, 245, 249);
                dgv.ColumnHeadersDefaultCellStyle.SelectionForeColor = TextColor;

                dgv.RowsDefaultCellStyle.BackColor = CardColor;
                dgv.RowsDefaultCellStyle.ForeColor = TextColor;
                dgv.RowsDefaultCellStyle.SelectionBackColor = IsDarkMode ? Color.FromArgb(51, 65, 85) : Color.FromArgb(241, 245, 249);
                dgv.RowsDefaultCellStyle.SelectionForeColor = TextColor;

                dgv.AlternatingRowsDefaultCellStyle.BackColor = IsDarkMode ? Color.FromArgb(15, 23, 42) : Color.FromArgb(248, 250, 252);
                dgv.AlternatingRowsDefaultCellStyle.ForeColor = TextColor;
                dgv.AlternatingRowsDefaultCellStyle.SelectionBackColor = IsDarkMode ? Color.FromArgb(51, 65, 85) : Color.FromArgb(241, 245, 249);
                dgv.AlternatingRowsDefaultCellStyle.SelectionForeColor = TextColor;

                dgv.ThemeStyle.RowsStyle.BackColor = CardColor;
                dgv.ThemeStyle.RowsStyle.ForeColor = TextColor;
                dgv.ThemeStyle.RowsStyle.SelectionBackColor = IsDarkMode ? Color.FromArgb(51, 65, 85) : Color.FromArgb(241, 245, 249);
                dgv.ThemeStyle.RowsStyle.SelectionForeColor = TextColor;

                dgv.ThemeStyle.AlternatingRowsStyle.BackColor = IsDarkMode ? Color.FromArgb(15, 23, 42) : Color.FromArgb(248, 250, 252);
                dgv.ThemeStyle.AlternatingRowsStyle.ForeColor = TextColor;

                dgv.ThemeStyle.HeaderStyle.BackColor = IsDarkMode ? Color.FromArgb(30, 41, 59) : Color.FromArgb(241, 245, 249);
                dgv.ThemeStyle.HeaderStyle.ForeColor = TextColor;
            }
            else if (ctrl is Guna2TextBox txt)
            {
                txt.FillColor = IsDarkMode ? Color.FromArgb(15, 23, 42) : Color.White;
                txt.ForeColor = TextColor;
                txt.BorderColor = BorderColor;
                txt.PlaceholderForeColor = SubtextColor;
                txt.FocusedState.BorderColor = Color.FromArgb(59, 130, 246);
            }
            else if (ctrl is Guna2ComboBox cmb)
            {
                cmb.FillColor = IsDarkMode ? Color.FromArgb(15, 23, 42) : Color.White;
                cmb.ForeColor = TextColor;
                cmb.BorderColor = BorderColor;
                cmb.FocusedState.BorderColor = Color.FromArgb(59, 130, 246);
                cmb.ItemsAppearance.BackColor = CardColor;
                cmb.ItemsAppearance.ForeColor = TextColor;
                cmb.ItemsAppearance.SelectedBackColor = IsDarkMode ? Color.FromArgb(51, 65, 85) : Color.FromArgb(241, 245, 249);
                cmb.ItemsAppearance.SelectedForeColor = TextColor;
            }
            else if (ctrl is Guna2DateTimePicker dtp)
            {
                dtp.FillColor = IsDarkMode ? Color.FromArgb(15, 23, 42) : Color.White;
                dtp.ForeColor = TextColor;
                dtp.BorderColor = BorderColor;
            }
            else if (ctrl is Guna2CheckBox chk)
            {
                chk.BackColor = insideCard ? parentCardColor : Color.Transparent;
                chk.ForeColor = TextColor;
                chk.UncheckedState.BorderColor = BorderColor;
            }
            else if (ctrl is Guna2RadioButton rad)
            {
                rad.BackColor = insideCard ? parentCardColor : Color.Transparent;
                rad.ForeColor = TextColor;
                rad.UncheckedState.BorderColor = BorderColor;
            }
            else if (ctrl is CheckBox stdChk)
            {
                stdChk.BackColor = insideCard ? parentCardColor : Color.Transparent;
                stdChk.ForeColor = TextColor;
            }
            else if (ctrl is RadioButton stdRad)
            {
                stdRad.BackColor = insideCard ? parentCardColor : Color.Transparent;
                stdRad.ForeColor = TextColor;
            }

            // Apply recursively to all child controls nested inside this container
            foreach (Control child in ctrl.Controls)
            {
                ApplyThemeToControl(child);
            }
        }
    }
}
