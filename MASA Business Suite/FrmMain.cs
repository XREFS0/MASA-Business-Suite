using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace MASA_Business_Suite
{
    public class FrmMain : Form
    {
        private Guna2Panel pnlTitleBar; // Unified top Title Bar for modern borderless experience
        private Guna2Panel pnlSidebar;
        private Guna2Panel pnlNavbar;
        private Guna2Panel pnlContent;
        private Label lblTitle;
        private Label lblDateTime;
        private Timer timerClock;
        private Form activeForm = null;
        private Guna2Button currentBtn = null;
        private Point dragStartPoint = Point.Empty; // Class-level variable for dragging window

        // Class-level fields for interactive Search, Notifications, and Themes
        private Guna2TextBox txtSearch;
        private FlowLayoutPanel flpNavbarRight;
        private Guna2Panel pnlNotificationsDropdown = null;
        private Guna2Panel pnlMessagesDropdown = null;

        public FrmMain()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Normal;
            this.Size = new Size(1280, 720);
            this.StartPosition = FormStartPosition.CenterScreen;
            // Prevent borderless form from covering the Windows Taskbar when maximized
            this.MaximizedBounds = Screen.FromHandle(this.Handle).WorkingArea;
            
            // Set Form Icon
            if (ThemeManager.AppIcon != null)
            {
                this.Icon = ThemeManager.AppIcon;
            }
            
            LoadDefaultPage();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1280, 720);
            this.MinimumSize = new Size(1100, 700);
            this.Text = "MASA ERP Pro - نظام إدارة الأعمال المتكامل";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(248, 250, 252); // slate-50
            this.FormBorderStyle = FormBorderStyle.None; // Make form borderless for a premium experience

            // Modern, smooth border resizing for borderless form
            Guna2ResizeForm resizeForm = new Guna2ResizeForm();
            resizeForm.TargetForm = this;

            // =========================================================================
            // 0. Title Bar Panel (Unified White Titlebar spanning full width)
            // =========================================================================
            pnlTitleBar = new Guna2Panel();
            pnlTitleBar.Height = 32;
            pnlTitleBar.Dock = DockStyle.Top;
            pnlTitleBar.FillColor = Color.White;
            pnlTitleBar.CustomBorderColor = Color.FromArgb(241, 245, 249); // slate-100
            pnlTitleBar.CustomBorderThickness = new Padding(0, 0, 0, 1); // Bottom border only

            // Window Title on the left
            Label lblWindowTitle = new Label();
            lblWindowTitle.Text = "  MASA ERP Pro - نظام إدارة الأعمال المتكامل";
            lblWindowTitle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblWindowTitle.ForeColor = Color.FromArgb(15, 23, 42); // Navy Slate
            lblWindowTitle.Location = new Point(12, 0);
            lblWindowTitle.Size = new Size(350, 32);
            lblWindowTitle.TextAlign = ContentAlignment.MiddleLeft;
            pnlTitleBar.Controls.Add(lblWindowTitle);

            // Modern Flat rectangular Window Control buttons FlowLayout on the right
            FlowLayoutPanel flpTitleControls = new FlowLayoutPanel();
            flpTitleControls.Size = new Size(138, 32);
            flpTitleControls.Dock = DockStyle.Right;
            flpTitleControls.FlowDirection = FlowDirection.LeftToRight;
            flpTitleControls.Margin = new Padding(0);
            flpTitleControls.Padding = new Padding(0);
            flpTitleControls.BackColor = Color.Transparent;
            pnlTitleBar.Controls.Add(flpTitleControls);

            Guna2ControlBox btnMin = new Guna2ControlBox();
            btnMin.ControlBoxType = Guna.UI2.WinForms.Enums.ControlBoxType.MinimizeBox;
            btnMin.Size = new Size(46, 32);
            btnMin.Margin = new Padding(0);
            btnMin.FillColor = Color.Transparent;
            btnMin.IconColor = Color.FromArgb(100, 116, 139); // Slate-500
            btnMin.HoverState.FillColor = Color.FromArgb(241, 245, 249);
            btnMin.HoverState.IconColor = Color.FromArgb(15, 23, 42);
            flpTitleControls.Controls.Add(btnMin);

            Guna2ControlBox btnMax = new Guna2ControlBox();
            btnMax.ControlBoxType = Guna.UI2.WinForms.Enums.ControlBoxType.MaximizeBox;
            btnMax.Size = new Size(46, 32);
            btnMax.Margin = new Padding(0);
            btnMax.FillColor = Color.Transparent;
            btnMax.IconColor = Color.FromArgb(100, 116, 139);
            btnMax.HoverState.FillColor = Color.FromArgb(241, 245, 249);
            btnMax.HoverState.IconColor = Color.FromArgb(15, 23, 42);
            flpTitleControls.Controls.Add(btnMax);

            Guna2ControlBox btnClose = new Guna2ControlBox();
            btnClose.ControlBoxType = Guna.UI2.WinForms.Enums.ControlBoxType.CloseBox;
            btnClose.Size = new Size(46, 32);
            btnClose.Margin = new Padding(0);
            btnClose.FillColor = Color.Transparent;
            btnClose.IconColor = Color.FromArgb(100, 116, 139); // Slate-500
            btnClose.HoverState.FillColor = Color.FromArgb(239, 68, 68); // Vibrant Red
            btnClose.HoverState.IconColor = Color.White;
            flpTitleControls.Controls.Add(btnClose);

            // Wire up robust C# dragging event handlers (DPI & component independent)
            pnlTitleBar.MouseDown += TitleBar_MouseDown;
            pnlTitleBar.MouseMove += TitleBar_MouseMove;
            pnlTitleBar.MouseUp += TitleBar_MouseUp;

            lblWindowTitle.MouseDown += TitleBar_MouseDown;
            lblWindowTitle.MouseMove += TitleBar_MouseMove;
            lblWindowTitle.MouseUp += TitleBar_MouseUp;

            // =========================================================================
            // 1. Sidebar Panel (NARROW WIDTH = 220px, NO scrollbars!)
            // =========================================================================
            pnlSidebar = new Guna2Panel();
            pnlSidebar.Width = 220;
            pnlSidebar.Dock = DockStyle.Left;
            pnlSidebar.FillColor = Color.White;
            pnlSidebar.CustomBorderColor = Color.FromArgb(241, 245, 249); // slate-100
            pnlSidebar.CustomBorderThickness = new Padding(0, 0, 1, 0); // Right border only
            pnlSidebar.AutoScroll = false; // Disable scroll completely

            // Logo Header inside Sidebar (Compact Height = 70px)
            Guna2Panel pnlLogo = new Guna2Panel();
            pnlLogo.Height = 70;
            pnlLogo.Dock = DockStyle.Top;
            pnlLogo.FillColor = Color.White;
            pnlLogo.Padding = new Padding(12, 10, 12, 10);
            pnlSidebar.Controls.Add(pnlLogo);

            // Sleek blue logo icon (Centered & Modern Minimalist)
            PictureBox pnlLogoIcon = new PictureBox();
            pnlLogoIcon.Size = new Size(40, 40);
            pnlLogoIcon.Location = new Point(90, 15); // Centered: (220 - 40) / 2 = 90
            pnlLogoIcon.SizeMode = PictureBoxSizeMode.Zoom;
            pnlLogoIcon.Image = LoadIcon("logo.png");
            pnlLogo.Controls.Add(pnlLogoIcon);

            // Bottom space cleaned up for a minimalist and highly professional presentation

            // Inner Menu Holder panel (Fixed space - NO scrollbars will ever appear!)
            Panel pnlMenuHolder = new Panel();
            pnlMenuHolder.Dock = DockStyle.Fill;
            pnlMenuHolder.AutoScroll = false; // Completely disable scroll here too
            pnlMenuHolder.BackColor = Color.White;
            pnlSidebar.Controls.Add(pnlMenuHolder);
            pnlMenuHolder.BringToFront();

            int currentY = 5;
            
            // Sidebar buttons with narrow width (190) and small height (32)
            AddMenuHeader("الرئيسية", ref currentY, pnlMenuHolder);
            var btnDashboard = AddMenuButton("لوحة التحكم", "Dashboard", "dashboard.png", ref currentY, pnlMenuHolder);
            
            AddMenuHeader("الموارد البشرية", ref currentY, pnlMenuHolder);
            AddMenuButton("الموظفين", "Employees", "manager.png", ref currentY, pnlMenuHolder);
            AddMenuButton("الأقسام", "Departments", "department.png", ref currentY, pnlMenuHolder);
            AddMenuButton("الحضور والانصراف", "Attendance", "immigration.png", ref currentY, pnlMenuHolder);
            AddMenuButton("المرتبات", "Payroll", "employment.png", ref currentY, pnlMenuHolder);

            AddMenuHeader("المخزون والمستودعات", ref currentY, pnlMenuHolder);
            AddMenuButton("المنتجات", "Products", "box.png", ref currentY, pnlMenuHolder);
            AddMenuButton("التصنيفات", "Categories", "classification.png", ref currentY, pnlMenuHolder);
            AddMenuButton("المخزون", "Inventory", "warehouse.png", ref currentY, pnlMenuHolder);

            AddMenuHeader("المبيعات", ref currentY, pnlMenuHolder);
            AddMenuButton("نقاط البيع (POS)", "POS", "money.png", ref currentY, pnlMenuHolder);
            AddMenuButton("المبيعات", "Sales", "invoice.png", ref currentY, pnlMenuHolder);
            AddMenuButton("العملاء", "Customers", "rating.png", ref currentY, pnlMenuHolder);

            AddMenuHeader("التقارير والإعدادات", ref currentY, pnlMenuHolder);
            AddMenuButton("التقارير", "Reports", "analysis.png", ref currentY, pnlMenuHolder);
            AddMenuButton("الإعدادات", "Settings", "cogwheel.png", ref currentY, pnlMenuHolder);

            // =========================================================================
            // 2. Navbar Panel (Clean White with bottom border)
            // =========================================================================
            pnlNavbar = new Guna2Panel();
            pnlNavbar.Height = 65;
            pnlNavbar.Dock = DockStyle.Top;
            pnlNavbar.FillColor = Color.White;
            pnlNavbar.CustomBorderColor = Color.FromArgb(241, 245, 249); // slate-100
            pnlNavbar.CustomBorderThickness = new Padding(0, 0, 0, 1);

            // Hamburger icon or menu button
            Guna2Button btnHamburger = new Guna2Button();
            btnHamburger.Size = new Size(36, 36);
            btnHamburger.Location = new Point(12, 14);
            btnHamburger.FillColor = Color.Transparent;
            btnHamburger.Text = "≡";
            btnHamburger.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            btnHamburger.ForeColor = Color.FromArgb(71, 85, 105);
            btnHamburger.Cursor = Cursors.Hand;
            btnHamburger.Click += (s, e) => {
                pnlSidebar.Visible = !pnlSidebar.Visible;
            };
            pnlNavbar.Controls.Add(btnHamburger);

            lblTitle = new Label();
            lblTitle.Text = "لوحة التحكم";
            lblTitle.Font = new Font("Segoe UI", 11.5F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42); // Navy Slate
            lblTitle.Location = new Point(55, 12);
            lblTitle.Size = new Size(200, 36);
            lblTitle.TextAlign = ContentAlignment.MiddleLeft;
            pnlNavbar.Controls.Add(lblTitle);

            // Center Search Box (matching exact design search UI)
            this.txtSearch = new Guna2TextBox();
            this.txtSearch.PlaceholderText = "بحث...";
            this.txtSearch.Size = new Size(240, 32);
            this.txtSearch.Location = new Point(270, 16);
            this.txtSearch.BorderRadius = 16;
            this.txtSearch.FillColor = Color.FromArgb(241, 245, 249); // slate-100
            this.txtSearch.BorderColor = Color.Transparent;
            this.txtSearch.TextOffset = new Point(8, 0);
            this.txtSearch.IconRight = LoadIcon("search.png");
            this.txtSearch.IconRightSize = new Size(14, 14);
            this.txtSearch.IconRightOffset = new Point(8, 0);
            this.txtSearch.TextChanged += TxtSearch_TextChanged;
            pnlNavbar.Controls.Add(this.txtSearch);

            // Right Utility Panel (Notifications, Messages, Mode, and Admin Profile Card)
            this.flpNavbarRight = new FlowLayoutPanel();
            this.flpNavbarRight.Dock = DockStyle.Right;
            this.flpNavbarRight.Width = 380;
            this.flpNavbarRight.FlowDirection = FlowDirection.LeftToRight;
            this.flpNavbarRight.Padding = new Padding(10, 14, 10, 10);
            this.flpNavbarRight.BackColor = Color.Transparent;
            pnlNavbar.Controls.Add(this.flpNavbarRight);

            // Populate navbar icons dynamically (Supports theme toggling & real data)
            UpdateNavbarIcons();

            // Real-Time date/clock element
            lblDateTime = new Label();
            lblDateTime.Text = DateTime.Now.ToString("dd MMMM yyyy - hh:mm tt");
            lblDateTime.Font = new Font("Segoe UI", 8F, FontStyle.Regular);
            lblDateTime.ForeColor = Color.FromArgb(100, 116, 139);
            lblDateTime.Location = new Point(flpNavbarRight.Left - 210, 12);
            lblDateTime.Size = new Size(190, 36);
            lblDateTime.TextAlign = ContentAlignment.MiddleRight;
            pnlNavbar.Controls.Add(lblDateTime);

            // Bottom footer removed as requested

            // =========================================================================
            // 4. Content Panel (Docked FILL - the main working page area)
            // =========================================================================
            pnlContent = new Guna2Panel();
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.FillColor = Color.FromArgb(248, 250, 252);

            // =========================================================================
            // 5. CRITICAL: Add Controls in Correct Layout Z-Order (PREVENTS OVERLAPPING)
            // =========================================================================
            // Adding DockStyle.Fill first, then surrounding DockStyle panels.
            // This guarantees they will align perfectly as adjacent grids without ever overlapping!
            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlNavbar);
            this.Controls.Add(pnlSidebar);
            this.Controls.Add(pnlTitleBar);

            // Clock Timer
            timerClock = new Timer();
            timerClock.Interval = 1000;
            timerClock.Tick += TimerClock_Tick;
            timerClock.Start();
        }

        private Guna2Panel CreateNavbarIcon(string emoji, string badgeVal)
        {
            Guna2Panel pnlIcon = new Guna2Panel();
            pnlIcon.Size = new Size(32, 32);
            pnlIcon.BorderRadius = 16;
            pnlIcon.FillColor = ThemeManager.IsDarkMode ? Color.FromArgb(51, 65, 85) : Color.FromArgb(241, 245, 249);
            pnlIcon.Margin = new Padding(4, 0, 4, 0);
            pnlIcon.Cursor = Cursors.Hand;
            pnlIcon.BackColor = Color.Transparent;

            Label lbl = new Label();
            lbl.Text = emoji;
            lbl.Font = new Font("Segoe UI", 11);
            lbl.TextAlign = ContentAlignment.MiddleCenter;
            lbl.Dock = DockStyle.Fill;
            lbl.Cursor = Cursors.Hand;
            lbl.ForeColor = ThemeManager.TextColor;
            lbl.BackColor = Color.Transparent;
            pnlIcon.Controls.Add(lbl);

            EventHandler clickHandler = (s, ev) => {
                NavbarIcon_Click(emoji);
            };

            pnlIcon.Click += clickHandler;
            lbl.Click += clickHandler;

            if (!string.IsNullOrEmpty(badgeVal))
            {
                Guna2Panel pnlBadge = new Guna2Panel();
                pnlBadge.Size = new Size(14, 14);
                pnlBadge.Location = new Point(20, 0);
                pnlBadge.BorderRadius = 7;
                pnlBadge.FillColor = Color.FromArgb(59, 130, 246); // Bright Blue badge
                pnlBadge.Cursor = Cursors.Hand;
                pnlBadge.BackColor = Color.Transparent;

                Label lblBadge = new Label();
                lblBadge.Text = badgeVal;
                lblBadge.Font = new Font("Segoe UI", 6.5F, FontStyle.Bold);
                lblBadge.ForeColor = Color.White;
                lblBadge.TextAlign = ContentAlignment.MiddleCenter;
                lblBadge.Dock = DockStyle.Fill;
                lblBadge.Cursor = Cursors.Hand;
                lblBadge.BackColor = Color.Transparent;

                pnlBadge.Controls.Add(lblBadge);
                pnlIcon.Controls.Add(pnlBadge);
                pnlBadge.BringToFront();

                pnlBadge.Click += clickHandler;
                lblBadge.Click += clickHandler;
            }

            // Wire up premium hover effects
            lbl.MouseEnter += (s, ev) => pnlIcon.FillColor = ThemeManager.IsDarkMode ? Color.FromArgb(71, 85, 105) : Color.FromArgb(226, 232, 240);
            lbl.MouseLeave += (s, ev) => pnlIcon.FillColor = ThemeManager.IsDarkMode ? Color.FromArgb(51, 65, 85) : Color.FromArgb(241, 245, 249);
            pnlIcon.MouseEnter += (s, ev) => pnlIcon.FillColor = ThemeManager.IsDarkMode ? Color.FromArgb(71, 85, 105) : Color.FromArgb(226, 232, 240);
            pnlIcon.MouseLeave += (s, ev) => pnlIcon.FillColor = ThemeManager.IsDarkMode ? Color.FromArgb(51, 65, 85) : Color.FromArgb(241, 245, 249);

            return pnlIcon;
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            if (activeForm is ISearchable searchable)
            {
                searchable.PerformSearch(txtSearch.Text);
            }
        }

        private void NavbarIcon_Click(string emoji)
        {
            if (emoji == "🌙" || emoji == "☀️")
            {
                ToggleTheme();
            }
            else if (emoji == "🔔")
            {
                ShowNotifications();
            }
            else if (emoji == "✉️")
            {
                ShowMessages();
            }
        }

        public void ToggleTheme()
        {
            ThemeManager.IsDarkMode = !ThemeManager.IsDarkMode;
            ApplyShellTheme();
        }

        public void ApplyShellTheme()
        {
            this.BackColor = ThemeManager.BackgroundColor;
            
            pnlTitleBar.FillColor = ThemeManager.CardColor;
            pnlTitleBar.BackColor = ThemeManager.CardColor;
            pnlTitleBar.CustomBorderColor = ThemeManager.BorderColor;
            
            foreach (Control ctrl in pnlTitleBar.Controls)
            {
                if (ctrl is Label lbl)
                {
                    lbl.BackColor = Color.Transparent;
                    lbl.ForeColor = ThemeManager.TextColor;
                }
            }

            pnlSidebar.FillColor = ThemeManager.CardColor;
            pnlSidebar.BackColor = ThemeManager.CardColor;
            pnlSidebar.CustomBorderColor = ThemeManager.BorderColor;
            
            foreach (Control ctrl in pnlSidebar.Controls)
            {
                if (ctrl is Guna2Panel gPnl)
                {
                    gPnl.FillColor = ThemeManager.CardColor;
                    gPnl.BackColor = ThemeManager.CardColor;
                }

                if (ctrl is Panel pnl)
                {
                    pnl.BackColor = ThemeManager.CardColor;
                    foreach (Control c in pnl.Controls)
                    {
                        if (c is Guna2Button btn)
                        {
                            if (btn.Checked)
                            {
                                btn.CheckedState.FillColor = Color.FromArgb(59, 130, 246);
                                btn.CheckedState.ForeColor = Color.White;
                            }
                            else
                            {
                                btn.ForeColor = ThemeManager.TextColor;
                                btn.HoverState.FillColor = ThemeManager.IsDarkMode ? Color.FromArgb(51, 65, 85) : Color.FromArgb(241, 245, 249);
                                btn.HoverState.ForeColor = ThemeManager.TextColor;
                            }
                        }
                        else if (c is Label lblHeader)
                        {
                            lblHeader.BackColor = Color.Transparent;
                            lblHeader.ForeColor = ThemeManager.IsDarkMode ? Color.FromArgb(148, 163, 184) : Color.FromArgb(100, 116, 139);
                        }
                        else if (c is PictureBox pboxLogo)
                        {
                            pboxLogo.BackColor = Color.Transparent;
                        }
                    }
                }
            }

            pnlNavbar.FillColor = ThemeManager.CardColor;
            pnlNavbar.BackColor = ThemeManager.CardColor;
            pnlNavbar.CustomBorderColor = ThemeManager.BorderColor;

            foreach (Control c in pnlNavbar.Controls)
            {
                if (c is Label lbl)
                {
                    lbl.BackColor = Color.Transparent;
                    if (lbl == lblTitle)
                    {
                        lbl.ForeColor = ThemeManager.TextColor;
                    }
                    else if (lbl == lblDateTime)
                    {
                        lbl.ForeColor = ThemeManager.SubtextColor;
                    }
                }
                else if (c is Guna2Button btn)
                {
                    btn.BackColor = Color.Transparent;
                    btn.FillColor = Color.Transparent;
                    btn.ForeColor = ThemeManager.TextColor;
                }
                else if (c is Guna2TextBox txt)
                {
                    txt.BackColor = Color.Transparent;
                    txt.FillColor = ThemeManager.IsDarkMode ? Color.FromArgb(51, 65, 85) : Color.FromArgb(241, 245, 249);
                    txt.ForeColor = ThemeManager.TextColor;
                    txt.BorderColor = ThemeManager.BorderColor;
                }
            }

            UpdateNavbarIcons();

            pnlContent.FillColor = ThemeManager.BackgroundColor;
            pnlContent.BackColor = ThemeManager.BackgroundColor;

            // Apply theme to currently loaded child form
            if (activeForm != null)
            {
                ThemeManager.ApplyTheme(activeForm);
            }

            // Close active dropdowns to prevent style mismatches
            if (pnlNotificationsDropdown != null) pnlNotificationsDropdown.Visible = false;
            if (pnlMessagesDropdown != null) pnlMessagesDropdown.Visible = false;
        }

        private void UpdateNavbarIcons()
        {
            flpNavbarRight.Controls.Clear();
            
            flpNavbarRight.Controls.Add(CreateNavbarIcon("🔔", "3")); // Bell
            
            string themeEmoji = ThemeManager.IsDarkMode ? "☀️" : "🌙";
            flpNavbarRight.Controls.Add(CreateNavbarIcon(themeEmoji, "")); // Theme switch

            // Profile circle avatar
            Guna2Panel pnlAvatar = new Guna2Panel();
            pnlAvatar.Size = new Size(32, 32);
            pnlAvatar.BorderRadius = 16;
            pnlAvatar.FillColor = Color.FromArgb(37, 99, 235); // solid blue
            pnlAvatar.Margin = new Padding(12, 0, 5, 0);
            pnlAvatar.Cursor = Cursors.Hand;
            pnlAvatar.BackColor = Color.Transparent;

            Label lblAvatarChar = new Label();
            lblAvatarChar.Text = "A";
            lblAvatarChar.ForeColor = Color.White;
            lblAvatarChar.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblAvatarChar.TextAlign = ContentAlignment.MiddleCenter;
            lblAvatarChar.Dock = DockStyle.Fill;
            lblAvatarChar.Cursor = Cursors.Hand;
            lblAvatarChar.BackColor = Color.Transparent;
            pnlAvatar.Controls.Add(lblAvatarChar);
            flpNavbarRight.Controls.Add(pnlAvatar);

            // Profile text
            Label lblAdminName = new Label();
            lblAdminName.Text = "Admin\nمدير النظام";
            lblAdminName.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            lblAdminName.ForeColor = ThemeManager.TextColor;
            lblAdminName.Size = new Size(80, 32);
            lblAdminName.TextAlign = ContentAlignment.MiddleLeft;
            lblAdminName.Cursor = Cursors.Hand;
            lblAdminName.BackColor = Color.Transparent;
            flpNavbarRight.Controls.Add(lblAdminName);

            // Wire up profile click to open Settings page!
            EventHandler profileClickHandler = (s, ev) => {
                NavigateToPage("Settings");
            };
            pnlAvatar.Click += profileClickHandler;
            lblAvatarChar.Click += profileClickHandler;
            lblAdminName.Click += profileClickHandler;
        }

        private void ShowNotifications()
        {
            if (pnlMessagesDropdown != null && pnlMessagesDropdown.Visible)
            {
                pnlMessagesDropdown.Visible = false;
            }

            if (pnlNotificationsDropdown == null)
            {
                pnlNotificationsDropdown = new Guna2Panel();
                pnlNotificationsDropdown.Size = new Size(320, 360);
                pnlNotificationsDropdown.BorderRadius = 12;
                pnlNotificationsDropdown.CustomBorderThickness = new Padding(1);
                pnlNotificationsDropdown.ShadowDecoration.Enabled = true;
                pnlNotificationsDropdown.ShadowDecoration.BorderRadius = 12;
                pnlNotificationsDropdown.ShadowDecoration.Depth = 15;
                pnlNotificationsDropdown.ShadowDecoration.Color = Color.FromArgb(15, 23, 42);
                
                this.Controls.Add(pnlNotificationsDropdown);
            }

            // Sync colors with current theme
            pnlNotificationsDropdown.FillColor = ThemeManager.CardColor;
            pnlNotificationsDropdown.CustomBorderColor = ThemeManager.BorderColor;

            pnlNotificationsDropdown.Visible = !pnlNotificationsDropdown.Visible;

            if (pnlNotificationsDropdown.Visible)
            {
                pnlNotificationsDropdown.Location = new Point(this.Width - 340, pnlNavbar.Bottom + pnlTitleBar.Height);
                pnlNotificationsDropdown.BringToFront();
                PopulateNotifications();
            }
        }

        private void PopulateNotifications()
        {
            pnlNotificationsDropdown.Controls.Clear();

            // Header Label
            Label lblHeader = new Label();
            lblHeader.Text = "آخر الإشعارات والتنبيهات";
            lblHeader.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblHeader.ForeColor = ThemeManager.TextColor;
            lblHeader.Location = new Point(15, 12);
            lblHeader.Size = new Size(290, 24);
            lblHeader.TextAlign = ContentAlignment.MiddleLeft;
            pnlNotificationsDropdown.Controls.Add(lblHeader);

            // Separator Line
            Guna2Panel pnlSep = new Guna2Panel();
            pnlSep.FillColor = ThemeManager.BorderColor;
            pnlSep.Location = new Point(15, 40);
            pnlSep.Size = new Size(290, 1);
            pnlNotificationsDropdown.Controls.Add(pnlSep);

            // Flow Container for Logs list
            FlowLayoutPanel flpLogs = new FlowLayoutPanel();
            flpLogs.Location = new Point(15, 48);
            flpLogs.Size = new Size(290, 260);
            flpLogs.FlowDirection = FlowDirection.TopDown;
            flpLogs.WrapContents = false;
            flpLogs.AutoScroll = true;
            flpLogs.BackColor = Color.Transparent;
            pnlNotificationsDropdown.Controls.Add(flpLogs);

            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery("SELECT Timestamp, LogType, Description FROM Logs ORDER BY LogID DESC LIMIT 5;");
                if (dt != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow r in dt.Rows)
                    {
                        string desc = r["Description"]?.ToString() ?? "";
                        string type = r["LogType"]?.ToString() ?? "INFO";
                        string ts = r["Timestamp"]?.ToString() ?? "";

                        // Parse short time
                        try {
                            DateTime parsed = DateTime.Parse(ts);
                            ts = parsed.ToString("hh:mm tt");
                        } catch {}

                        Guna2Panel pnlLogItem = new Guna2Panel();
                        pnlLogItem.Size = new Size(265, 48);
                        pnlLogItem.BorderRadius = 6;
                        pnlLogItem.FillColor = ThemeManager.IsDarkMode ? Color.FromArgb(51, 65, 85) : Color.FromArgb(248, 250, 252);
                        pnlLogItem.Margin = new Padding(0, 0, 0, 6);

                        // Color Indicator Circle
                        Guna2Panel pnlDot = new Guna2Panel();
                        pnlDot.Size = new Size(8, 8);
                        pnlDot.BorderRadius = 4;
                        pnlDot.Location = new Point(8, 20);
                        if (type == "WARNING" || type == "ERROR")
                        {
                            pnlDot.FillColor = Color.FromArgb(239, 68, 68); // Red
                        }
                        else
                        {
                            pnlDot.FillColor = Color.FromArgb(16, 185, 129); // Green
                        }
                        pnlLogItem.Controls.Add(pnlDot);

                        // Arabic description
                        Label lblDesc = new Label();
                        lblDesc.Text = desc;
                        lblDesc.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
                        lblDesc.ForeColor = ThemeManager.TextColor;
                        lblDesc.Location = new Point(22, 4);
                        lblDesc.Size = new Size(185, 40);
                        lblDesc.TextAlign = ContentAlignment.MiddleLeft;
                        pnlLogItem.Controls.Add(lblDesc);

                        // Timestamp
                        Label lblTs = new Label();
                        lblTs.Text = ts;
                        lblTs.Font = new Font("Segoe UI", 7F);
                        lblTs.ForeColor = ThemeManager.SubtextColor;
                        lblTs.Location = new Point(210, 4);
                        lblTs.Size = new Size(50, 40);
                        lblTs.TextAlign = ContentAlignment.MiddleRight;
                        pnlLogItem.Controls.Add(lblTs);

                        flpLogs.Controls.Add(pnlLogItem);
                    }
                }
                else
                {
                    Label lblEmpty = new Label();
                    lblEmpty.Text = "لا توجد تنبيهات نشطة حالياً";
                    lblEmpty.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
                    lblEmpty.ForeColor = ThemeManager.SubtextColor;
                    lblEmpty.Size = new Size(265, 60);
                    lblEmpty.TextAlign = ContentAlignment.MiddleCenter;
                    flpLogs.Controls.Add(lblEmpty);
                }
            }
            catch
            {
                Label lblEmpty = new Label();
                lblEmpty.Text = "لا توجد تنبيهات نشطة حالياً";
                lblEmpty.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
                lblEmpty.ForeColor = ThemeManager.SubtextColor;
                lblEmpty.Size = new Size(265, 60);
                lblEmpty.TextAlign = ContentAlignment.MiddleCenter;
                flpLogs.Controls.Add(lblEmpty);
            }

            // View all reports link button
            Guna2Button btnViewAll = new Guna2Button();
            btnViewAll.Text = "عرض جميع التقارير والسجلات";
            btnViewAll.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            btnViewAll.FillColor = Color.FromArgb(59, 130, 246);
            btnViewAll.ForeColor = Color.White;
            btnViewAll.Size = new Size(290, 28);
            btnViewAll.Location = new Point(15, 318);
            btnViewAll.BorderRadius = 6;
            btnViewAll.Click += (s, ev) => {
                pnlNotificationsDropdown.Visible = false;
                // Navigate to Reports screen
                foreach (Control ctrl in pnlSidebar.Controls)
                {
                    if (ctrl is Panel pnl)
                    {
                        foreach (Control c in pnl.Controls)
                        {
                            if (c is Guna2Button btn && btn.Tag.ToString() == "Reports")
                            {
                                btn.Checked = true;
                                lblTitle.Text = btn.Text;
                                LoadPage("Reports");
                                return;
                            }
                        }
                    }
                }
            };
            pnlNotificationsDropdown.Controls.Add(btnViewAll);
        }

        private void ShowMessages()
        {
            if (pnlNotificationsDropdown != null && pnlNotificationsDropdown.Visible)
            {
                pnlNotificationsDropdown.Visible = false;
            }

            if (pnlMessagesDropdown == null)
            {
                pnlMessagesDropdown = new Guna2Panel();
                pnlMessagesDropdown.Size = new Size(320, 260);
                pnlMessagesDropdown.BorderRadius = 12;
                pnlMessagesDropdown.CustomBorderThickness = new Padding(1);
                pnlMessagesDropdown.ShadowDecoration.Enabled = true;
                pnlMessagesDropdown.ShadowDecoration.BorderRadius = 12;
                pnlMessagesDropdown.ShadowDecoration.Depth = 15;
                pnlMessagesDropdown.ShadowDecoration.Color = Color.FromArgb(15, 23, 42);
                
                this.Controls.Add(pnlMessagesDropdown);
            }

            // Sync colors with current theme
            pnlMessagesDropdown.FillColor = ThemeManager.CardColor;
            pnlMessagesDropdown.CustomBorderColor = ThemeManager.BorderColor;

            pnlMessagesDropdown.Visible = !pnlMessagesDropdown.Visible;

            if (pnlMessagesDropdown.Visible)
            {
                pnlMessagesDropdown.Location = new Point(this.Width - 340, pnlNavbar.Bottom + pnlTitleBar.Height);
                pnlMessagesDropdown.BringToFront();
                PopulateMessages();
            }
        }

        private void PopulateMessages()
        {
            pnlMessagesDropdown.Controls.Clear();

            // Header Label
            Label lblHeader = new Label();
            lblHeader.Text = "صندوق الرسائل الواردة";
            lblHeader.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblHeader.ForeColor = ThemeManager.TextColor;
            lblHeader.Location = new Point(15, 12);
            lblHeader.Size = new Size(290, 24);
            lblHeader.TextAlign = ContentAlignment.MiddleLeft;
            pnlMessagesDropdown.Controls.Add(lblHeader);

            // Separator Line
            Guna2Panel pnlSep = new Guna2Panel();
            pnlSep.FillColor = ThemeManager.BorderColor;
            pnlSep.Location = new Point(15, 40);
            pnlSep.Size = new Size(290, 1);
            pnlMessagesDropdown.Controls.Add(pnlSep);

            // Flow Container for Messages
            FlowLayoutPanel flpMsg = new FlowLayoutPanel();
            flpMsg.Location = new Point(15, 48);
            flpMsg.Size = new Size(290, 195);
            flpMsg.FlowDirection = FlowDirection.TopDown;
            flpMsg.WrapContents = false;
            flpMsg.AutoScroll = true;
            flpMsg.BackColor = Color.Transparent;
            pnlMessagesDropdown.Controls.Add(flpMsg);

            // Seed mock premium messages in Arabic
            string[,] msgs = new string[,] {
                { "رسالة من المدير (أحمد المصري)", "يرجى مراجعة تقارير المبيعات لشهر مايو.", "10:15 AM" },
                { "تنبيه المخازن (مستودع 1)", "المنتجات التالية أوشكت على النفاد.", "09:30 AM" },
                { "الدعم الفني (مؤسسة MASA)", "تم تحديث نظام قاعدة البيانات بنجاح.", "أمس" }
            };

            for (int i = 0; i < 3; i++)
            {
                Guna2Panel pnlMsgItem = new Guna2Panel();
                pnlMsgItem.Size = new Size(265, 52);
                pnlMsgItem.BorderRadius = 6;
                pnlMsgItem.FillColor = ThemeManager.IsDarkMode ? Color.FromArgb(51, 65, 85) : Color.FromArgb(248, 250, 252);
                pnlMsgItem.Margin = new Padding(0, 0, 0, 6);

                // Avatar bubble icon
                Guna2Panel pnlAv = new Guna2Panel();
                pnlAv.Size = new Size(24, 24);
                pnlAv.BorderRadius = 12;
                pnlAv.FillColor = i == 0 ? Color.FromArgb(59, 130, 246) : (i == 1 ? Color.FromArgb(239, 68, 68) : Color.FromArgb(16, 185, 129));
                pnlAv.Location = new Point(8, 14);
                
                Label lblAv = new Label();
                lblAv.Text = i == 0 ? "أ" : (i == 1 ? "ت" : "م");
                lblAv.Font = new Font("Segoe UI", 7F, FontStyle.Bold);
                lblAv.ForeColor = Color.White;
                lblAv.TextAlign = ContentAlignment.MiddleCenter;
                lblAv.Dock = DockStyle.Fill;
                pnlAv.Controls.Add(lblAv);
                pnlMsgItem.Controls.Add(pnlAv);

                // Title and Body text
                Label lblTitleText = new Label();
                lblTitleText.Text = msgs[i, 0] + "\n" + msgs[i, 1];
                lblTitleText.Font = new Font("Segoe UI", 7.2F, FontStyle.Bold);
                lblTitleText.ForeColor = ThemeManager.TextColor;
                lblTitleText.Location = new Point(36, 4);
                lblTitleText.Size = new Size(175, 44);
                lblTitleText.TextAlign = ContentAlignment.MiddleLeft;
                pnlMsgItem.Controls.Add(lblTitleText);

                // Time label
                Label lblTime = new Label();
                lblTime.Text = msgs[i, 2];
                lblTime.Font = new Font("Segoe UI", 6.5F);
                lblTime.ForeColor = ThemeManager.SubtextColor;
                lblTime.Location = new Point(212, 4);
                lblTime.Size = new Size(48, 44);
                lblTime.TextAlign = ContentAlignment.MiddleRight;
                pnlMsgItem.Controls.Add(lblTime);

                flpMsg.Controls.Add(pnlMsgItem);
            }
        }

        private void TimerClock_Tick(object sender, EventArgs e)
        {
            lblDateTime.Text = DateTime.Now.ToString("dd MMMM yyyy - hh:mm:ss tt");
        }

        private void AddMenuHeader(string text, ref int yPos, Panel parent)
        {
            Label lblHeader = new Label();
            lblHeader.Text = text;
            lblHeader.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            lblHeader.ForeColor = Color.FromArgb(148, 163, 184); // slate-400
            lblHeader.Location = new Point(15, yPos);
            lblHeader.Size = new Size(190, 16);
            lblHeader.TextAlign = ContentAlignment.MiddleLeft;
            parent.Controls.Add(lblHeader);
            yPos += 18;
        }

        public static Image LoadIcon(string fileName)
        {
            try
            {
                string[] paths = new string[]
                {
                    Path.Combine(Application.StartupPath, "Icons", fileName),
                    Path.Combine(Application.StartupPath, "..", "..", "Icons", fileName),
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Icons", fileName)
                };

                foreach (string p in paths)
                {
                    if (File.Exists(p))
                    {
                        return Image.FromFile(p);
                    }
                }
            }
            catch { }
            return null;
        }

        private Guna2Button AddMenuButton(string text, string pageTag, string iconFile, ref int yPos, Panel parent)
        {
            Guna2Button btn = new Guna2Button();
            btn.Text = text;
            btn.Tag = pageTag;
            btn.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            btn.ForeColor = Color.FromArgb(71, 85, 105); // slate-600
            btn.FillColor = Color.Transparent;
            btn.TextAlign = HorizontalAlignment.Left;
            btn.TextOffset = new Point(14, 0); // Spaced nicely next to the icon!
            btn.Size = new Size(190, 32); // Compact sizing fits all menu buttons beautifully!
            btn.Location = new Point(15, yPos); // Perfectly centered with 15px margin left and right
            btn.BorderRadius = 8;
            btn.ButtonMode = Guna.UI2.WinForms.Enums.ButtonMode.RadioButton;

            // Load icon
            if (!string.IsNullOrEmpty(iconFile))
            {
                btn.Image = LoadIcon(iconFile);
                btn.ImageSize = new Size(16, 16);
                btn.ImageAlign = HorizontalAlignment.Left;
                btn.ImageOffset = new Point(8, 0);
            }

            // Hover and checked styling (Exactly matching your premium blue-pill design)
            btn.HoverState.FillColor = Color.FromArgb(241, 245, 249); // light grey slate hover
            btn.HoverState.ForeColor = Color.FromArgb(15, 23, 42);
            btn.CheckedState.FillColor = Color.FromArgb(59, 130, 246); // Royal Blue Active Card
            btn.CheckedState.ForeColor = Color.White;

            btn.Click += MenuButton_Click;

            parent.Controls.Add(btn);
            yPos += 34; // 32 height + 2 spacing = fits perfectly!
            return btn;
        }

        private void MenuButton_Click(object sender, EventArgs e)
        {
            Guna2Button btn = sender as Guna2Button;
            if (btn == null) return;

            currentBtn = btn;
            string page = btn.Tag.ToString();
            
            lblTitle.Text = btn.Text;
            
            LoadPage(page);
        }

        private void LoadDefaultPage()
        {
            // Activate Dashboard Button
            foreach (Control ctrl in pnlSidebar.Controls)
            {
                if (ctrl is Panel pnl)
                {
                    foreach (Control c in pnl.Controls)
                    {
                        if (c is Guna2Button btn && btn.Tag.ToString() == "Dashboard")
                        {
                            btn.Checked = true;
                            lblTitle.Text = btn.Text;
                            LoadPage("Dashboard");
                            return;
                        }
                    }
                }
            }
        }

        private void LoadPage(string pageTag)
        {
            Form formToLoad = null;

            switch (pageTag)
            {
                case "Dashboard":
                    formToLoad = new FrmDashboard();
                    break;
                case "Employees":
                    formToLoad = new FrmEmployees();
                    break;
                case "Departments":
                    formToLoad = new FrmDepartments();
                    break;
                case "Attendance":
                    formToLoad = new FrmAttendance();
                    break;
                case "Payroll":
                    formToLoad = new FrmPayroll();
                    break;
                case "Products":
                    formToLoad = new FrmProducts();
                    break;
                case "Categories":
                    formToLoad = new FrmCategories();
                    break;
                case "Inventory":
                    formToLoad = new FrmInventory();
                    break;
                case "POS":
                    formToLoad = new FrmPOS(this);
                    break;
                case "Sales":
                    formToLoad = new FrmSales();
                    break;
                case "Customers":
                    formToLoad = new FrmCustomers();
                    break;
                case "Suppliers":
                    formToLoad = new FrmSuppliers();
                    break;
                case "Purchases":
                    formToLoad = new FrmPurchases();
                    break;
                case "Expenses":
                    formToLoad = new FrmExpenses();
                    break;
                case "Reports":
                    formToLoad = new FrmReports();
                    break;
                case "Settings":
                    formToLoad = new FrmSettings();
                    break;
                case "Backup":
                    formToLoad = new FrmBackup();
                    break;
            }

            if (formToLoad != null)
            {
                if (activeForm != null)
                {
                    activeForm.Close();
                    activeForm.Dispose();
                }

                activeForm = formToLoad;
                formToLoad.TopLevel = false;
                formToLoad.FormBorderStyle = FormBorderStyle.None;
                formToLoad.Dock = DockStyle.Fill;
                
                // Apply theme before rendering
                ThemeManager.ApplyTheme(formToLoad);

                pnlContent.Controls.Add(formToLoad);
                pnlContent.Tag = formToLoad;
                formToLoad.BringToFront();
                formToLoad.Show();

                // Apply theme again after showing to capture dynamic elements generated in Form_Load
                ThemeManager.ApplyTheme(formToLoad);
            }
        }

        public void NavigateToPage(string pageTag)
        {
            foreach (Control ctrl in pnlSidebar.Controls)
            {
                if (ctrl is Panel pnl)
                {
                    foreach (Control c in pnl.Controls)
                    {
                        if (c is Guna2Button btn && btn.Tag != null && btn.Tag.ToString() == pageTag)
                        {
                            btn.Checked = true;
                            lblTitle.Text = btn.Text;
                            LoadPage(pageTag);
                            return;
                        }
                    }
                }
            }
            LoadPage(pageTag);
        }

        // =========================================================================
        // 6. Native Dragging Event Handlers (DPI immune, high performance)
        // =========================================================================
        private void TitleBar_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                dragStartPoint = new Point(e.X, e.Y);
            }
        }

        private void TitleBar_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && dragStartPoint != Point.Empty)
            {
                Point currentPagePoint = this.PointToScreen(new Point(e.X, e.Y));
                this.Location = new Point(currentPagePoint.X - dragStartPoint.X, currentPagePoint.Y - dragStartPoint.Y);
            }
        }

        private void TitleBar_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                dragStartPoint = Point.Empty;
            }
        }
    }
}
