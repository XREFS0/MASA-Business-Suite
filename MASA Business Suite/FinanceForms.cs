using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace MASA_Business_Suite
{
    // ==========================================
    // 1. Expenses Management (FrmExpenses)
    // ==========================================
    public class FrmExpenses : Form, ISearchable
    {
        private Guna2TextBox txtAmount, txtDescription, txtSearch;
        private Guna2ComboBox cmbType;
        private Guna2DateTimePicker dtDate;
        private Guna2DataGridView dgvExpenses;
        private Guna2Button btnAdd, btnDelete;
        private Panel pnlPieChartCanvas;
        private int selectedExpenseID = -1;

        public FrmExpenses()
        {
            InitializeComponent();
            LoadExpenses();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.DoubleBuffered = true;

            TableLayoutPanel tlp = new TableLayoutPanel();
            tlp.Dock = DockStyle.Fill;
            tlp.ColumnCount = 3;
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 340F)); // Inputs
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));  // Grid
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));  // Pie Chart
            this.Controls.Add(tlp);

            // Left Inputs Card
            Guna2Panel pnlInputs = new Guna2Panel();
            pnlInputs.Dock = DockStyle.Fill;
            pnlInputs.BorderRadius = 12;
            pnlInputs.FillColor = Color.White;
            pnlInputs.CustomBorderColor = Color.FromArgb(226, 232, 240);
            pnlInputs.CustomBorderThickness = new Padding(1);
            pnlInputs.Padding = new Padding(20);
            pnlInputs.Margin = new Padding(10);
            tlp.Controls.Add(pnlInputs, 0, 0);

            RTLFlowLayoutPanel flp = new RTLFlowLayoutPanel();
            flp.Dock = DockStyle.Fill;
            flp.FlowDirection = FlowDirection.TopDown;
            flp.WrapContents = false;
            flp.AutoScroll = true;
            flp.Padding = new Padding(5);
            flp.BackColor = Color.White;
            pnlInputs.Controls.Add(flp);

            AddLabel("نوع المصروف", flp);
            cmbType = new Guna2ComboBox();
            cmbType.Width = 270;
            cmbType.Height = 36;
            cmbType.BorderRadius = 8;
            cmbType.BorderColor = Color.FromArgb(203, 213, 225);
            cmbType.Items.AddRange(new string[] { "إيجار المقر", "كهرباء ومياه", "رواتب الموظفين", "مستلزمات مكتبية", "دعاية وتسويق", "بوفيه وضيافة", "صيانة وإصلاحات" });
            cmbType.SelectedIndex = 0;
            cmbType.Font = new Font("Segoe UI", 9.5F);
            cmbType.Margin = new Padding(0, 0, 0, 8);
            flp.Controls.Add(cmbType);

            AddLabel("المبلغ", flp);
            txtAmount = CreateTextBox("0.00 EGP", flp);

            AddLabel("البيان والوصف", flp);
            txtDescription = CreateTextBox("تفاصيل عملية الصرف", flp);

            AddLabel("التاريخ", flp);
            dtDate = new Guna2DateTimePicker();
            dtDate.Width = 270;
            dtDate.Height = 36;
            dtDate.BorderRadius = 8;
            dtDate.FillColor = Color.FromArgb(59, 130, 246);
            dtDate.ForeColor = Color.White;
            dtDate.Format = DateTimePickerFormat.Short;
            dtDate.Font = new Font("Segoe UI", 9.5F);
            dtDate.Margin = new Padding(0, 0, 0, 15);
            flp.Controls.Add(dtDate);

            // Buttons
            FlowLayoutPanel flpButtons = new FlowLayoutPanel();
            flpButtons.Width = 270;
            flpButtons.Height = 45;
            flpButtons.FlowDirection = FlowDirection.LeftToRight;
            flpButtons.BackColor = Color.Transparent;

            btnAdd = CreateActionButton("إضافة", Color.FromArgb(16, 185, 129), 120, 36, flpButtons);
            btnDelete = CreateActionButton("حذف", Color.FromArgb(239, 68, 68), 120, 36, flpButtons);

            flp.Controls.Add(flpButtons);

            btnAdd.Click += BtnAdd_Click;
            btnDelete.Click += BtnDelete_Click;

            // Middle Grid Card
            Guna2Panel pnlGridCard = new Guna2Panel();
            pnlGridCard.Dock = DockStyle.Fill;
            pnlGridCard.BorderRadius = 12;
            pnlGridCard.FillColor = Color.White;
            pnlGridCard.CustomBorderColor = Color.FromArgb(226, 232, 240);
            pnlGridCard.CustomBorderThickness = new Padding(1);
            pnlGridCard.Padding = new Padding(15);
            pnlGridCard.Margin = new Padding(10);
            tlp.Controls.Add(pnlGridCard, 1, 0);

            Label lblGridTitle = new Label();
            lblGridTitle.Text = "قائمة المصروفات الحالية";
            lblGridTitle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblGridTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblGridTitle.Dock = DockStyle.Top;
            lblGridTitle.Height = 30;
            pnlGridCard.Controls.Add(lblGridTitle);

            // Search bar
            Guna2Panel pnlSearch = new Guna2Panel();
            pnlSearch.Height = 50;
            pnlSearch.Dock = DockStyle.Top;
            pnlGridCard.Controls.Add(pnlSearch);

            txtSearch = new Guna2TextBox();
            txtSearch.PlaceholderText = "ابحث عن مصروف بالنوع أو البيان...";
            txtSearch.Size = new Size(300, 36);
            txtSearch.Location = new Point(0, 5);
            txtSearch.BorderRadius = 8;
            txtSearch.BorderColor = Color.FromArgb(203, 213, 225);
            txtSearch.IconRight = FrmMain.LoadIcon("search.png");
            txtSearch.IconRightSize = new Size(16, 16);
            txtSearch.IconRightOffset = new Point(6, 0);
            txtSearch.TextChanged += TxtSearch_TextChanged;
            pnlSearch.Controls.Add(txtSearch);

            dgvExpenses = new Guna2DataGridView();
            dgvExpenses.Dock = DockStyle.Fill;
            dgvExpenses.ReadOnly = true;
            dgvExpenses.AllowUserToAddRows = false;
            dgvExpenses.AllowUserToDeleteRows = false;
            dgvExpenses.BorderStyle = BorderStyle.None;
            dgvExpenses.Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Default;
            dgvExpenses.ThemeStyle.HeaderStyle.BackColor = Color.FromArgb(241, 245, 249);
            dgvExpenses.ThemeStyle.HeaderStyle.ForeColor = Color.FromArgb(71, 85, 105);
            dgvExpenses.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvExpenses.SelectionChanged += DgvExpenses_SelectionChanged;
            pnlGridCard.Controls.Add(dgvExpenses);
            dgvExpenses.BringToFront();

            // Right Pie Chart Card
            Guna2Panel pnlChartCard = new Guna2Panel();
            pnlChartCard.Dock = DockStyle.Fill;
            pnlChartCard.BorderRadius = 12;
            pnlChartCard.FillColor = Color.White;
            pnlChartCard.CustomBorderColor = Color.FromArgb(226, 232, 240);
            pnlChartCard.CustomBorderThickness = new Padding(1);
            pnlChartCard.Padding = new Padding(15);
            pnlChartCard.Margin = new Padding(10);
            tlp.Controls.Add(pnlChartCard, 2, 0);

            Label lblChartTitle = new Label();
            lblChartTitle.Text = "توزيع المصاريف";
            lblChartTitle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblChartTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblChartTitle.Dock = DockStyle.Top;
            lblChartTitle.Height = 30;
            pnlChartCard.Controls.Add(lblChartTitle);

            pnlPieChartCanvas = new Panel();
            pnlPieChartCanvas.Dock = DockStyle.Fill;
            pnlPieChartCanvas.BackColor = Color.White;
            pnlPieChartCanvas.Paint += PieChartCanvas_Paint;
            pnlChartCard.Controls.Add(pnlPieChartCanvas);
            pnlPieChartCanvas.BringToFront();
        }

        private void AddLabel(string text, FlowLayoutPanel parent)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lbl.ForeColor = Color.FromArgb(71, 85, 105);
            lbl.AutoSize = true;
            lbl.Margin = new Padding(0, 6, 0, 4);
            parent.Controls.Add(lbl);
        }

        private Guna2TextBox CreateTextBox(string placeholder, FlowLayoutPanel parent)
        {
            Guna2TextBox txt = new Guna2TextBox();
            txt.PlaceholderText = placeholder;
            txt.Width = 270;
            txt.Height = 36;
            txt.BorderRadius = 8;
            txt.BorderColor = Color.FromArgb(203, 213, 225);
            txt.Font = new Font("Segoe UI", 9.5F);
            txt.Margin = new Padding(0, 0, 0, 8);
            parent.Controls.Add(txt);
            return txt;
        }

        private Guna2Button CreateActionButton(string text, Color color, int w, int h, FlowLayoutPanel parent)
        {
            Guna2Button btn = new Guna2Button();
            btn.Text = text;
            btn.FillColor = color;
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btn.BorderRadius = 8;
            btn.Size = new Size(w, h);
            btn.Margin = new Padding(0, 0, 5, 0);

            // Automatically load professional PNG icons
            string iconFile = null;
            if (text == "إضافة" || text == "إضافة جديد") iconFile = "plus.png";
            else if (text == "تعديل") iconFile = "pencil.png";
            else if (text == "حذف") iconFile = "delete.png";
            else if (text == "حفظ") iconFile = "save.png";
            else if (text == "بحث") iconFile = "search.png";

            if (!string.IsNullOrEmpty(iconFile))
            {
                btn.Image = FrmMain.LoadIcon(iconFile);
                btn.ImageSize = new Size(16, 16);
                btn.ImageAlign = HorizontalAlignment.Left;
                btn.ImageOffset = new Point(6, 0);
                btn.TextOffset = new Point(4, 0);
            }

            parent.Controls.Add(btn);
            return btn;
        }

        private void LoadExpenses(string searchQuery = "")
        {
            try
            {
                string sql = "SELECT ExpenseID as 'ID', ExpenseType as 'النوع', Amount as 'القيمة', Description as 'البيان', Date as 'التاريخ' FROM Expenses";
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    sql += " WHERE ExpenseType LIKE @search OR Description LIKE @search";
                    var param = new SQLiteParameter("@search", $"%{searchQuery}%");
                    dgvExpenses.DataSource = DatabaseHelper.ExecuteQuery(sql + " ORDER BY Date DESC;", new SQLiteParameter[] { param });
                }
                else
                {
                    dgvExpenses.DataSource = DatabaseHelper.ExecuteQuery(sql + " ORDER BY Date DESC;");
                }
                pnlPieChartCanvas.Invalidate(); // Redraw pie chart
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadExpenses(txtSearch.Text);
        }

        public void PerformSearch(string query)
        {
            txtSearch.Text = query;
        }

        private void DgvExpenses_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvExpenses.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvExpenses.SelectedRows[0];
                selectedExpenseID = Convert.ToInt32(row.Cells["ID"].Value);
                cmbType.Text = row.Cells["النوع"].Value?.ToString() ?? "";
                txtAmount.Text = row.Cells["القيمة"].Value?.ToString() ?? "";
                txtDescription.Text = row.Cells["البيان"].Value?.ToString() ?? "";
                
                DateTime eDate;
                if (row.Cells["التاريخ"].Value != null && DateTime.TryParse(row.Cells["التاريخ"].Value.ToString(), out eDate))
                {
                    dtDate.Value = eDate;
                }
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            double amt;
            if (string.IsNullOrEmpty(txtAmount.Text) || !double.TryParse(txtAmount.Text, out amt) || amt <= 0)
            {
                MessageBox.Show("يرجى إدخال مبلغ صحيح!");
                return;
            }

            try
            {
                string sql = "INSERT INTO Expenses (ExpenseType, Amount, Description, Date) VALUES (@type, @amt, @desc, @date);";
                var parameters = new SQLiteParameter[]
                {
                    new SQLiteParameter("@type", cmbType.Text),
                    new SQLiteParameter("@amt", amt),
                    new SQLiteParameter("@desc", txtDescription.Text),
                    new SQLiteParameter("@date", dtDate.Value.ToString("yyyy-MM-dd"))
                };
                DatabaseHelper.ExecuteNonQuery(sql, parameters);
                DatabaseHelper.LogActivity("WARNING", $"تسجيل مصروف جديد ({cmbType.Text}) بقيمة: {amt} EGP");
                LoadExpenses();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedExpenseID == -1) return;

            try
            {
                string sql = "DELETE FROM Expenses WHERE ExpenseID = @id;";
                DatabaseHelper.ExecuteNonQuery(sql, new SQLiteParameter[] { new SQLiteParameter("@id", selectedExpenseID) });
                DatabaseHelper.LogActivity("WARNING", $"تم حذف قيد مصروف بالرقم: {selectedExpenseID}");
                LoadExpenses();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ClearInputs()
        {
            selectedExpenseID = -1;
            txtAmount.Clear();
            txtDescription.Clear();
        }

        private void PieChartCanvas_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int w = pnlPieChartCanvas.Width;
            int h = pnlPieChartCanvas.Height;

            int size = Math.Min(w, h) - 100;
            if (size <= 50) return;

            int x = (w - size) / 2;
            int y = (h - size) / 2 - 20;

            // Query total expenses per type
            DataTable dt = DatabaseHelper.ExecuteQuery("SELECT ExpenseType, SUM(Amount) as Total FROM Expenses GROUP BY ExpenseType;");
            if (dt.Rows.Count == 0)
            {
                g.DrawString("لا توجد مصروفات مسجلة لعرض الرسم البياني", new Font("Segoe UI", 9, FontStyle.Italic), Brushes.Gray, new PointF(10, 50));
                return;
            }

            double grandTotal = 0.0;
            foreach (DataRow row in dt.Rows)
            {
                grandTotal += Convert.ToDouble(row["Total"]);
            }

            Color[] sliceColors = {
                Color.FromArgb(14, 165, 233), // Sky
                Color.FromArgb(239, 68, 68),  // Red
                Color.FromArgb(16, 185, 129), // Emerald
                Color.FromArgb(245, 158, 11), // Amber
                Color.FromArgb(99, 102, 241), // Indigo
                Color.FromArgb(236, 72, 153), // Pink
                Color.FromArgb(107, 114, 128) // Gray
            };

            float startAngle = 0f;
            int colorIndex = 0;

            int legendY = y + size + 20;

            foreach (DataRow row in dt.Rows)
            {
                double total = Convert.ToDouble(row["Total"]);
                float sweepAngle = (float)((total / grandTotal) * 360.0);

                Color c = sliceColors[colorIndex % sliceColors.Length];

                using (SolidBrush brush = new SolidBrush(c))
                {
                    g.FillPie(brush, x, y, size, size, startAngle, sweepAngle);
                }

                // Draw Legend item
                using (SolidBrush brush = new SolidBrush(c))
                {
                    g.FillRectangle(brush, 20, legendY, 12, 12);
                }
                using (SolidBrush textBrush = new SolidBrush(Color.FromArgb(15, 23, 42)))
                {
                    g.DrawString($"{row["ExpenseType"]} ({total:N0} EGP)", new Font("Segoe UI", 8, FontStyle.Bold), textBrush, 40, legendY - 2);
                }

                startAngle += sweepAngle;
                colorIndex++;
                legendY += 18;
            }
        }
    }

    // ==========================================
    // 2. Reports Trigger Form (FrmReports)
    // ==========================================
    public class FrmReports : Form, ISearchable
    {
        private Guna2ComboBox cmbReportType;
        private Guna2Button btnPDF, btnExcel, btnPrint;
        private Guna2DataGridView dgvReportPreview;
        private Guna2TextBox txtSearch;

        public FrmReports()
        {
            InitializeComponent();
            cmbReportType.SelectedIndex = 0;
            LoadReportPreview();
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

            // Container Panel Card
            Guna2Panel pnlCard = new Guna2Panel();
            pnlCard.Dock = DockStyle.Fill;
            pnlCard.BorderRadius = 12;
            pnlCard.FillColor = Color.White;
            pnlCard.CustomBorderColor = Color.FromArgb(226, 232, 240);
            pnlCard.CustomBorderThickness = new Padding(1);
            pnlCard.Padding = new Padding(20);
            tlp.Controls.Add(pnlCard, 0, 0);

            // Filter Top FlowPanel (Fluid for all screen scales!)
            FlowLayoutPanel flpFilter = new FlowLayoutPanel();
            flpFilter.Dock = DockStyle.Top;
            flpFilter.AutoSize = true;
            flpFilter.MinimumSize = new Size(0, 55);
            flpFilter.FlowDirection = FlowDirection.LeftToRight;
            flpFilter.BackColor = Color.White;
            pnlCard.Controls.Add(flpFilter);

            Label lblType = new Label();
            lblType.Text = "نوع التقرير";
            lblType.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblType.ForeColor = Color.FromArgb(71, 85, 105);
            lblType.AutoSize = true;
            lblType.Margin = new Padding(5, 12, 10, 0);
            flpFilter.Controls.Add(lblType);

            cmbReportType = new Guna2ComboBox();
            cmbReportType.Width = 180;
            cmbReportType.Height = 36;
            cmbReportType.BorderRadius = 8;
            cmbReportType.Items.AddRange(new string[] { "تقرير المبيعات", "تقرير الموظفين", "تقرير المخزون", "الأرباح والمصروفات" });
            cmbReportType.SelectedIndexChanged += CmbReportType_SelectedIndexChanged;
            cmbReportType.Font = new Font("Segoe UI", 9.5F);
            cmbReportType.Margin = new Padding(0, 5, 20, 0);
            flpFilter.Controls.Add(cmbReportType);

            txtSearch = new Guna2TextBox();
            txtSearch.PlaceholderText = "ابحث في التقرير المعروض...";
            txtSearch.Size = new Size(200, 36);
            txtSearch.Margin = new Padding(0, 5, 20, 0);
            txtSearch.BorderRadius = 8;
            txtSearch.BorderColor = Color.FromArgb(203, 213, 225);
            txtSearch.IconRight = FrmMain.LoadIcon("search.png");
            txtSearch.IconRightSize = new Size(16, 16);
            txtSearch.IconRightOffset = new Point(6, 0);
            txtSearch.TextChanged += TxtSearch_TextChanged;
            flpFilter.Controls.Add(txtSearch);

            // Export Buttons
            btnPDF = CreateActionButton("تصدير PDF", Color.FromArgb(239, 68, 68), 130, 36, flpFilter);
            btnExcel = CreateActionButton("تصدير Excel", Color.FromArgb(16, 185, 129), 130, 36, flpFilter);
            btnPrint = CreateActionButton("طباعة مباشر", Color.FromArgb(59, 130, 246), 130, 36, flpFilter);

            btnPDF.Click += BtnPDF_Click;
            btnExcel.Click += BtnExcel_Click;
            btnPrint.Click += BtnPrint_Click;

            // Grid Preview
            dgvReportPreview = new Guna2DataGridView();
            dgvReportPreview.Dock = DockStyle.Fill;
            dgvReportPreview.ReadOnly = true;
            dgvReportPreview.AllowUserToAddRows = false;
            dgvReportPreview.AllowUserToDeleteRows = false;
            dgvReportPreview.BorderStyle = BorderStyle.None;
            dgvReportPreview.Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Default;
            dgvReportPreview.ThemeStyle.HeaderStyle.BackColor = Color.FromArgb(241, 245, 249);
            dgvReportPreview.ThemeStyle.HeaderStyle.ForeColor = Color.FromArgb(71, 85, 105);
            pnlCard.Controls.Add(dgvReportPreview);
            dgvReportPreview.BringToFront();
        }

        private Guna2Button CreateActionButton(string text, Color color, int w, int h, FlowLayoutPanel parent)
        {
            Guna2Button btn = new Guna2Button();
            btn.Text = text;
            btn.FillColor = color;
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btn.BorderRadius = 8;
            btn.Size = new Size(w, h);
            btn.Margin = new Padding(0, 5, 8, 0);

            // Automatically load professional PNG icons
            string iconFile = null;
            if (text == "تصدير PDF" || text == "تصدير Excel" || text == "تصدير") iconFile = "export.png";
            else if (text == "طباعة مباشر" || text == "طباعة") iconFile = "printer.png";

            if (!string.IsNullOrEmpty(iconFile))
            {
                btn.Image = FrmMain.LoadIcon(iconFile);
                btn.ImageSize = new Size(16, 16);
                btn.ImageAlign = HorizontalAlignment.Left;
                btn.ImageOffset = new Point(6, 0);
                btn.TextOffset = new Point(4, 0);
            }

            parent.Controls.Add(btn);
            return btn;
        }

        private void CmbReportType_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadReportPreview();
        }

        private void LoadReportPreview(string searchQuery = "")
        {
            try
            {
                DataTable dt = QueryReportData(searchQuery);
                dgvReportPreview.DataSource = dt;
            }
            catch { }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadReportPreview(txtSearch.Text);
        }

        public void PerformSearch(string query)
        {
            txtSearch.Text = query;
        }

        private DataTable QueryReportData(string searchQuery = "")
        {
            int index = cmbReportType.SelectedIndex;
            string likeClause = string.IsNullOrEmpty(searchQuery) ? "" : $"%{searchQuery}%";

            if (index == 0) // Sales Report
            {
                if (!string.IsNullOrEmpty(likeClause))
                {
                    return DatabaseHelper.ExecuteQuery("SELECT InvoiceNo as 'الفاتورة', SaleDate as 'التاريخ', Discount as 'خصم', Tax as 'ضريبة', Total as 'الإجمالي' FROM Sales WHERE InvoiceNo LIKE @search;", new SQLiteParameter[] { new SQLiteParameter("@search", likeClause) });
                }
                return DatabaseHelper.ExecuteQuery("SELECT InvoiceNo as 'الفاتورة', SaleDate as 'التاريخ', Discount as 'خصم', Tax as 'ضريبة', Total as 'الإجمالي' FROM Sales;");
            }
            else if (index == 1) // Employees List
            {
                if (!string.IsNullOrEmpty(likeClause))
                {
                    return DatabaseHelper.ExecuteQuery("SELECT e.Name as 'الموظف', e.Phone as 'الهاتف', d.Name as 'القسم', e.Salary as 'الراتب الاساس', e.JoinDate as 'تاريخ التعيين' FROM Employees e LEFT JOIN Departments d ON e.DepartmentID=d.DepartmentID WHERE e.Name LIKE @search OR e.Phone LIKE @search OR d.Name LIKE @search;", new SQLiteParameter[] { new SQLiteParameter("@search", likeClause) });
                }
                return DatabaseHelper.ExecuteQuery("SELECT e.Name as 'الموظف', e.Phone as 'الهاتف', d.Name as 'القسم', e.Salary as 'الراتب الاساس', e.JoinDate as 'تاريخ التعيين' FROM Employees e LEFT JOIN Departments d ON e.DepartmentID=d.DepartmentID;");
            }
            else if (index == 2) // Inventory
            {
                if (!string.IsNullOrEmpty(likeClause))
                {
                    return DatabaseHelper.ExecuteQuery("SELECT Name as 'المنتج', Barcode as 'الباركود', BuyPrice as 'سعر الشراء', SellPrice as 'سعر البيع', Qty as 'الكمية المخزنة' FROM Products WHERE Name LIKE @search OR Barcode LIKE @search;", new SQLiteParameter[] { new SQLiteParameter("@search", likeClause) });
                }
                return DatabaseHelper.ExecuteQuery("SELECT Name as 'المنتج', Barcode as 'الباركود', BuyPrice as 'سعر الشراء', SellPrice as 'سعر البيع', Qty as 'الكمية المخزنة' FROM Products;");
            }
            else // Profit & Loss
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("بند الحساب", typeof(string));
                dt.Columns.Add("القيمة (EGP)", typeof(double));

                object salesObj = DatabaseHelper.ExecuteScalar("SELECT SUM(Total) FROM Sales;");
                double sales = salesObj != DBNull.Value ? Convert.ToDouble(salesObj) : 0.0;

                object expObj = DatabaseHelper.ExecuteScalar("SELECT SUM(Amount) FROM Expenses;");
                double expenses = expObj != DBNull.Value ? Convert.ToDouble(expObj) : 0.0;

                dt.Rows.Add("إجمالي إيرادات المبيعات (Sales)", sales);
                dt.Rows.Add("إجمالي المصاريف التشغيلية (Expenses)", expenses);
                dt.Rows.Add("صافي الأرباح التشغيلية (Net Profit)", sales - expenses);

                return dt;
            }
        }

        private void BtnPDF_Click(object sender, EventArgs e)
        {
            ExportReportHTML("PDF");
        }

        private void BtnExcel_Click(object sender, EventArgs e)
        {
            ExportReportCSV();
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            ExportReportHTML("Print");
        }

        private void ExportReportHTML(string mode)
        {
            try
            {
                DataTable dt = QueryReportData();
                string title = cmbReportType.Text;

                // Build modern printable HTML
                string html = $@"
<html>
<head>
    <title>{title}</title>
    <style>
        body {{ font-family: 'Segoe UI', Arial, sans-serif; direction: rtl; padding: 30px; color: #1e293b; background: #f8fafc; }}
        h1 {{ color: #0f172a; text-align: center; margin-bottom: 5px; }}
        h3 {{ text-align: center; color: #64748b; margin-top: 0; font-weight: normal; }}
        table {{ width: 100%; border-collapse: collapse; margin-top: 30px; background: white; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 6px -1px rgb(0 0 0 / 0.1); }}
        th, td {{ padding: 12px 15px; text-align: right; border-bottom: 1px solid #e2e8f0; }}
        th {{ background: #0ea5e9; color: white; font-weight: bold; }}
        tr:hover {{ background: #f1f5f9; }}
        .footer {{ text-align: center; margin-top: 50px; font-size: 12px; color: #94a3b8; }}
        @media print {{
            body {{ background: white; padding: 0; }}
            table {{ box-shadow: none; }}
        }}
    </style>
</head>
<body>
    <h1>MASA BUSINESS SUITE</h1>
    <h3>{title} - تقرير مستخرج من النظام</h3>
    <hr/>
    <table>
        <thead>
            <tr>";
                foreach (DataColumn col in dt.Columns)
                {
                    html += $"<th>{col.ColumnName}</th>";
                }
                html += @"
            </tr>
        </thead>
        <tbody>";

                foreach (DataRow row in dt.Rows)
                {
                    html += "<tr>";
                    foreach (var item in row.ItemArray)
                    {
                        html += $"<td>{item}</td>";
                    }
                    html += "</tr>";
                }

                html += $@"
        </tbody>
    </table>
    <div class='footer'>
        تم استخراج هذا التقرير تلقائياً بواسطة نظام MASA ERP Pro في تاريخ {DateTime.Now.ToString("yyyy-MM-dd hh:mm tt")}
    </div>
</body>
</html>";

                string fileName = "Report_" + Guid.NewGuid().ToString().Substring(0, 8) + ".html";
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
                File.WriteAllText(fullPath, html);

                // Open in default browser which satisfies PDF rendering/printing natively!
                System.Diagnostics.Process.Start(fullPath);

                if (mode == "Print")
                {
                    MessageBox.Show("تم تحضير التقرير للطباعة بنجاح! يرجى استخدام الاختصار (Ctrl + P) في المتصفح الذي فتح لطباعة التقرير مباشرة.", "جاهز للطباعة", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("تم تصدير التقرير كملف PDF بنجاح! يمكنك حفظ الصفحة كملف PDF من خلال نافذة المتصفح المفتوحة.", "تصدير ناجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating report: " + ex.Message);
            }
        }

        private void ExportReportCSV()
        {
            try
            {
                DataTable dt = QueryReportData();
                string title = cmbReportType.Text;

                string fileName = $"CSV_{Guid.NewGuid().ToString().Substring(0, 8)}.csv";
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

                using (StreamWriter sw = new StreamWriter(fullPath, false, System.Text.Encoding.UTF8))
                {
                    // Write header
                    for (int i = 0; i < dt.Columns.Count; i++)
                    {
                        sw.Write(dt.Columns[i].ColumnName);
                        if (i < dt.Columns.Count - 1) sw.Write(",");
                    }
                    sw.WriteLine();

                    // Write rows
                    foreach (DataRow row in dt.Rows)
                    {
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            sw.Write(row[i].ToString().Replace(",", ""));
                            if (i < dt.Columns.Count - 1) sw.Write(",");
                        }
                        sw.WriteLine();
                    }
                }

                System.Diagnostics.Process.Start(fullPath);
                MessageBox.Show($"تم تصدير التقرير بنجاح كملف Excel CSV في المسار:\n{fullPath}", "تصدير ناجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exporting CSV: " + ex.Message);
            }
        }
    }
}
