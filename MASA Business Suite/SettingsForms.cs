using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace MASA_Business_Suite
{
    // ==========================================
    // 1. Company Settings Form (FrmSettings)
    // ==========================================
    public class FrmSettings : Form
    {
        private Guna2TextBox txtCompanyName, txtPhone, txtEmail, txtAddress, txtTaxRate, txtLoyaltyRate;
        private Guna2ComboBox cmbTheme;
        private Guna2Button btnSaveSettings;

        public FrmSettings()
        {
            InitializeComponent();
            LoadActiveSettings();
            cmbTheme.SelectedIndexChanged += CmbTheme_SelectedIndexChanged;
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.DoubleBuffered = true;

            TableLayoutPanel tlp = new TableLayoutPanel();
            tlp.Dock = DockStyle.Fill;
            tlp.ColumnCount = 1;
            tlp.Padding = new Padding(20);
            this.Controls.Add(tlp);

            // Settings Card Panel
            Guna2Panel pnlCard = new Guna2Panel();
            pnlCard.Dock = DockStyle.Fill;
            pnlCard.BorderRadius = 12;
            pnlCard.FillColor = Color.White;
            pnlCard.CustomBorderColor = Color.FromArgb(226, 232, 240);
            pnlCard.CustomBorderThickness = new Padding(1);
            pnlCard.Padding = new Padding(30);
            tlp.Controls.Add(pnlCard, 0, 0);

            Label lblTitle = new Label();
            lblTitle.Text = "إعدادات الشركة والنظام | Company Settings";
            lblTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 40;
            pnlCard.Controls.Add(lblTitle);

            // Inputs Layout Grid
            TableLayoutPanel tlpFields = new TableLayoutPanel();
            tlpFields.Dock = DockStyle.Top;
            tlpFields.Height = 350;
            tlpFields.ColumnCount = 2;
            tlpFields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpFields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            pnlCard.Controls.Add(tlpFields);
            tlpFields.BringToFront();

            // Col 1 Fields
            int y1 = 10;
            Guna2Panel pnlCol1 = new Guna2Panel();
            pnlCol1.Dock = DockStyle.Fill;
            tlpFields.Controls.Add(pnlCol1, 0, 0);

            AddLabel("اسم الشركة | Company Name:", pnlCol1, ref y1);
            txtCompanyName = CreateTextBox("MASA Business Suite", pnlCol1, ref y1);

            AddLabel("رقم الهاتف | Phone Number:", pnlCol1, ref y1);
            txtPhone = CreateTextBox("+2010xxxxxxxx", pnlCol1, ref y1);

            AddLabel("البريد الإلكتروني | Email Address:", pnlCol1, ref y1);
            txtEmail = CreateTextBox("info@masa.com", pnlCol1, ref y1);

            AddLabel("المظهر الافتراضي | Theme Mode:", pnlCol1, ref y1);
            cmbTheme = new Guna2ComboBox();
            cmbTheme.Location = new Point(20, y1);
            cmbTheme.Size = new Size(350, 36);
            cmbTheme.BorderRadius = 8;
            cmbTheme.Items.AddRange(new string[] { "Dark Slate (Navy)", "Light Style" });
            cmbTheme.SelectedIndex = 0;
            pnlCol1.Controls.Add(cmbTheme);

            // Col 2 Fields
            int y2 = 10;
            Guna2Panel pnlCol2 = new Guna2Panel();
            pnlCol2.Dock = DockStyle.Fill;
            tlpFields.Controls.Add(pnlCol2, 1, 0);

            AddLabel("العنوان المقر الرئيسي | Company Address:", pnlCol2, ref y2);
            txtAddress = CreateTextBox("Cairo, Egypt", pnlCol2, ref y2);

            AddLabel("ضريبة القيمة المضافة | Tax Rate (%):", pnlCol2, ref y2);
            txtTaxRate = CreateTextBox("14.0", pnlCol2, ref y2);

            AddLabel("معدل نقاط الولاء | Loyalty Rate (EGP per Point):", pnlCol2, ref y2);
            txtLoyaltyRate = CreateTextBox("50.0", pnlCol2, ref y2);

            // Action Save Button below Fields
            Panel pnlActions = new Panel();
            pnlActions.Height = 60;
            pnlActions.Dock = DockStyle.Bottom;
            pnlCard.Controls.Add(pnlActions);

            btnSaveSettings = new Guna2Button();
            btnSaveSettings.Text = "حفظ التغييرات | Save Settings";
            btnSaveSettings.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnSaveSettings.FillColor = Color.FromArgb(16, 185, 129); // Green
            btnSaveSettings.ForeColor = Color.White;
            btnSaveSettings.BorderRadius = 8;
            btnSaveSettings.Size = new Size(240, 45);
            btnSaveSettings.Location = new Point(20, 10);
            btnSaveSettings.Image = FrmMain.LoadIcon("save.png");
            btnSaveSettings.ImageSize = new Size(18, 18);
            btnSaveSettings.ImageAlign = HorizontalAlignment.Left;
            btnSaveSettings.ImageOffset = new Point(10, 0);
            btnSaveSettings.TextOffset = new Point(8, 0);
            btnSaveSettings.Click += BtnSaveSettings_Click;
            pnlActions.Controls.Add(btnSaveSettings);
        }

        private void AddLabel(string text, Panel parent, ref int y)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            lbl.ForeColor = Color.FromArgb(71, 85, 105);
            lbl.Location = new Point(20, y);
            lbl.Size = new Size(350, 20);
            parent.Controls.Add(lbl);
            y += 22;
        }

        private Guna2TextBox CreateTextBox(string placeholder, Panel parent, ref int y)
        {
            Guna2TextBox txt = new Guna2TextBox();
            txt.PlaceholderText = placeholder;
            txt.Location = new Point(20, y);
            txt.Size = new Size(350, 36);
            txt.BorderRadius = 8;
            txt.BorderColor = Color.FromArgb(203, 213, 225);
            parent.Controls.Add(txt);
            y += 48;
            return txt;
        }

        private void CmbTheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool newDark = (cmbTheme.SelectedIndex == 0);
            if (ThemeManager.IsDarkMode != newDark)
            {
                ThemeManager.IsDarkMode = newDark;

                // Save to database immediately
                string themeValue = newDark ? "Dark" : "Light";
                try
                {
                    DatabaseHelper.ExecuteNonQuery("UPDATE Settings SET ThemeMode = @theme WHERE SettingID = 1;",
                        new SQLiteParameter[] { new SQLiteParameter("@theme", themeValue) });
                }
                catch { }

                // Propagate theme update instantly across all forms
                FrmMain mainForm = null;
                foreach (Form f in Application.OpenForms)
                {
                    if (f is FrmMain)
                    {
                        mainForm = (FrmMain)f;
                        break;
                    }
                }
                if (mainForm != null)
                {
                    mainForm.ApplyShellTheme();
                }
            }
        }

        private void LoadActiveSettings()
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery("SELECT * FROM Settings ORDER BY SettingID DESC LIMIT 1;");
                if (dt.Rows.Count > 0)
                {
                    DataRow row = dt.Rows[0];
                    txtCompanyName.Text = row["CompanyName"].ToString();
                    txtPhone.Text = row["Phone"].ToString();
                    txtEmail.Text = row["Email"].ToString();
                    txtAddress.Text = row["Address"].ToString();
                    txtTaxRate.Text = row["TaxRate"].ToString();
                    txtLoyaltyRate.Text = row["LoyaltyRate"].ToString();

                    // Select correct theme in combobox without triggering event yet
                    string savedTheme = row["ThemeMode"]?.ToString() ?? "Dark";
                    if (savedTheme.Equals("Light", StringComparison.OrdinalIgnoreCase))
                    {
                        cmbTheme.SelectedIndex = 1;
                    }
                    else
                    {
                        cmbTheme.SelectedIndex = 0;
                    }
                }
            }
            catch { }
        }

        private void BtnSaveSettings_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCompanyName.Text)) return;

            try
            {
                double tax = 14.0;
                double.TryParse(txtTaxRate.Text, out tax);

                double loyalty = 50.0;
                double.TryParse(txtLoyaltyRate.Text, out loyalty);

                string sql = @"
                    UPDATE Settings 
                    SET CompanyName=@name, Phone=@phone, Email=@email, Address=@address, TaxRate=@tax, LoyaltyRate=@loyalty
                    WHERE SettingID = 1;";
                
                var parameters = new SQLiteParameter[]
                {
                    new SQLiteParameter("@name", txtCompanyName.Text),
                    new SQLiteParameter("@phone", txtPhone.Text),
                    new SQLiteParameter("@email", txtEmail.Text),
                    new SQLiteParameter("@address", txtAddress.Text),
                    new SQLiteParameter("@tax", tax),
                    new SQLiteParameter("@loyalty", loyalty)
                };

                DatabaseHelper.ExecuteNonQuery(sql, parameters);
                DatabaseHelper.LogActivity("INFO", "تم تعديل وحفظ إعدادات النظام بنجاح.");
                MessageBox.Show("تم حفظ وتحديث إعدادات النظام بنجاح!", "حفظ الإعدادات", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }

    // ==========================================
    // 2. Backup & Restore (FrmBackup)
    // ==========================================
    public class FrmBackup : Form
    {
        private Guna2Button btnBackup, btnRestore;
        private Label lblStatus;

        public FrmBackup()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.DoubleBuffered = true;

            TableLayoutPanel tlp = new TableLayoutPanel();
            tlp.Dock = DockStyle.Fill;
            tlp.ColumnCount = 1;
            tlp.Padding = new Padding(30);
            this.Controls.Add(tlp);

            // Card Panel
            Guna2Panel pnlCard = new Guna2Panel();
            pnlCard.Dock = DockStyle.Fill;
            pnlCard.BorderRadius = 12;
            pnlCard.FillColor = Color.White;
            pnlCard.CustomBorderColor = Color.FromArgb(226, 232, 240);
            pnlCard.CustomBorderThickness = new Padding(1);
            pnlCard.Padding = new Padding(40);
            tlp.Controls.Add(pnlCard, 0, 0);

            Label lblTitle = new Label();
            lblTitle.Text = "صيانة النظام - النسخ الاحتياطي والاسترجاع\nDatabase Backup & Restore Operations";
            lblTitle.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 60;
            lblTitle.TextAlign = ContentAlignment.TopCenter;
            pnlCard.Controls.Add(lblTitle);

            // Centered Controls Flow Panel
            FlowLayoutPanel flp = new FlowLayoutPanel();
            flp.Dock = DockStyle.Fill;
            flp.FlowDirection = FlowDirection.TopDown;
            flp.Padding = new Padding(20, 50, 20, 20);
            pnlCard.Controls.Add(flp);
            flp.BringToFront();

            btnBackup = new Guna2Button();
            btnBackup.Text = "إنشاء نسخة احتياطية الآن | Backup SQLite DB";
            btnBackup.FillColor = Color.FromArgb(14, 165, 233); // Sky Blue
            btnBackup.ForeColor = Color.White;
            btnBackup.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            btnBackup.BorderRadius = 8;
            btnBackup.Size = new Size(420, 45);
            btnBackup.Margin = new Padding(10);
            btnBackup.Image = FrmMain.LoadIcon("data-recovery.png");
            btnBackup.ImageSize = new Size(18, 18);
            btnBackup.ImageAlign = HorizontalAlignment.Left;
            btnBackup.ImageOffset = new Point(12, 0);
            btnBackup.TextOffset = new Point(10, 0);
            btnBackup.Click += BtnBackup_Click;
            flp.Controls.Add(btnBackup);

            btnRestore = new Guna2Button();
            btnRestore.Text = "استرجاع قاعدة البيانات | Restore Database";
            btnRestore.FillColor = Color.FromArgb(245, 158, 11); // Amber
            btnRestore.ForeColor = Color.White;
            btnRestore.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            btnRestore.BorderRadius = 8;
            btnRestore.Size = new Size(420, 45);
            btnRestore.Margin = new Padding(10);
            btnRestore.Image = FrmMain.LoadIcon("data-recovery2.png");
            btnRestore.ImageSize = new Size(18, 18);
            btnRestore.ImageAlign = HorizontalAlignment.Left;
            btnRestore.ImageOffset = new Point(12, 0);
            btnRestore.TextOffset = new Point(10, 0);
            btnRestore.Click += BtnRestore_Click;
            flp.Controls.Add(btnRestore);

            lblStatus = new Label();
            lblStatus.Text = "تنبيه: استرجاع قاعدة البيانات سيقوم باستبدال كافة البيانات الحالية بالبيانات المحفوظة. يرجى أخذ نسخة احتياطية أولاً.";
            lblStatus.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            lblStatus.ForeColor = Color.FromArgb(239, 68, 68); // Warning Red
            lblStatus.Size = new Size(420, 60);
            lblStatus.Margin = new Padding(10, 20, 10, 10);
            lblStatus.TextAlign = ContentAlignment.MiddleCenter;
            flp.Controls.Add(lblStatus);
        }

        private void BtnBackup_Click(object sender, EventArgs e)
        {
            string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "masa_erp.db");
            if (!File.Exists(dbPath))
            {
                MessageBox.Show("خطأ: قاعدة البيانات الحالية غير موجودة!");
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "SQLite Database Files (*.db)|*.db";
                sfd.FileName = $"masa_erp_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db";
                sfd.Title = "اختر موقع حفظ النسخة الاحتياطية";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Copy database file
                        File.Copy(dbPath, sfd.FileName, true);
                        DatabaseHelper.LogActivity("INFO", $"تم إنشاء نسخة احتياطية بنجاح إلى: {sfd.FileName}");
                        MessageBox.Show("تم إنشاء وحفظ النسخة الاحتياطية من قاعدة البيانات بنجاح!", "النسخ الاحتياطي", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("فشل نسخ الملف: " + ex.Message);
                    }
                }
            }
        }

        private void BtnRestore_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("تحذير! استرجاع نسخة احتياطية سيقوم باستبدال قاعدة البيانات الحالية بالكامل. هل تريد المتابعة؟", "تأكيد الاسترجاع", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "SQLite Database Files (*.db)|*.db";
                    ofd.Title = "اختر ملف النسخة الاحتياطية للاسترجاع";

                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "masa_erp.db");

                        try
                        {
                            // Clear all active connections in database helper pool before file operations
                            GC.Collect();
                            GC.WaitForPendingFinalizers();

                            // Overwrite active database file
                            File.Copy(ofd.FileName, dbPath, true);

                            DatabaseHelper.LogActivity("INFO", $"تم استرجاع قاعدة البيانات بنجاح من الملف: {ofd.FileName}");
                            MessageBox.Show("تم استرجاع قاعدة البيانات بنجاح! سيتم إغلاق التطبيق الآن لتطبيق التغييرات. يرجى إعادة تشغيل البرنامج.", "استرجاع ناجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            
                            Application.Exit();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("حدث خطأ أثناء استبدال الملف. تأكد من إغلاق كافة الاتصالات أو النوافذ المفتوحة أولاً!\nتفاصيل الخطأ: " + ex.Message);
                        }
                    }
                }
            }
        }
    }
}
