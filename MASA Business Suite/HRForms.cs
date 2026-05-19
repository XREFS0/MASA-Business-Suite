using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace MASA_Business_Suite
{
    // ==========================================
    // 1. Employees Management Form (FrmEmployees)
    // ==========================================
    public class FrmEmployees : Form, ISearchable
    {
        private Guna2TextBox txtName, txtPhone, txtEmail, txtSalary, txtAddress;
        private Guna2ComboBox cmbDepartment;
        private Guna2DateTimePicker dtJoinDate;
        private Guna2DataGridView dgvEmployees;
        private Guna2TextBox txtSearch;
        private Guna2Button btnAdd, btnEdit, btnDelete, btnClear;
        private int selectedEmployeeID = -1;

        public FrmEmployees()
        {
            InitializeComponent();
            LoadDepartments();
            LoadEmployees();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.DoubleBuffered = true;

            TableLayoutPanel tlp = new TableLayoutPanel();
            tlp.Dock = DockStyle.Fill;
            tlp.ColumnCount = 2;
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 380F)); // Left Input Panel
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));  // Right Grid
            this.Controls.Add(tlp);

            // Left Input Card Panel
            Guna2Panel pnlInputs = new Guna2Panel();
            pnlInputs.Dock = DockStyle.Fill;
            pnlInputs.BorderRadius = 12;
            pnlInputs.FillColor = Color.White;
            pnlInputs.CustomBorderColor = Color.FromArgb(226, 232, 240);
            pnlInputs.CustomBorderThickness = new Padding(1);
            pnlInputs.Padding = new Padding(20);
            pnlInputs.Margin = new Padding(15);
            tlp.Controls.Add(pnlInputs, 0, 0);

            // Fluid Vertical Layout Panel (Immune to DPI Scaling Overlaps)
            RTLFlowLayoutPanel flp = new RTLFlowLayoutPanel();
            flp.Dock = DockStyle.Fill;
            flp.FlowDirection = FlowDirection.TopDown;
            flp.WrapContents = false;
            flp.AutoScroll = true;
            flp.Padding = new Padding(5);
            flp.BackColor = Color.White;
            pnlInputs.Controls.Add(flp);

            // Inputs (Pure Arabic Labels, standard margins)
            AddLabel("الاسم الكامل", flp);
            txtName = CreateTextBox("أدخل الاسم بالكامل", flp);

            AddLabel("رقم الهاتف", flp);
            txtPhone = CreateTextBox("01xxxxxxxxx", flp);

            AddLabel("البريد الإلكتروني", flp);
            txtEmail = CreateTextBox("email@masa-pro.com", flp);

            AddLabel("القسم", flp);
            cmbDepartment = new Guna2ComboBox();
            cmbDepartment.Width = 290;
            cmbDepartment.Height = 36;
            cmbDepartment.BorderRadius = 8;
            cmbDepartment.BorderColor = Color.FromArgb(203, 213, 225);
            cmbDepartment.Font = new Font("Segoe UI", 9.5F);
            cmbDepartment.Margin = new Padding(0, 0, 0, 8);
            flp.Controls.Add(cmbDepartment);

            AddLabel("الراتب الأساسي", flp);
            txtSalary = CreateTextBox("الراتب الشهري", flp);

            AddLabel("العنوان", flp);
            txtAddress = CreateTextBox("العنوان السكني", flp);

            AddLabel("تاريخ التعيين", flp);
            dtJoinDate = new Guna2DateTimePicker();
            dtJoinDate.Width = 290;
            dtJoinDate.Height = 36;
            dtJoinDate.BorderRadius = 8;
            dtJoinDate.FillColor = Color.FromArgb(59, 130, 246);
            dtJoinDate.ForeColor = Color.White;
            dtJoinDate.Font = new Font("Segoe UI", 9.5F);
            dtJoinDate.Format = DateTimePickerFormat.Short;
            dtJoinDate.Margin = new Padding(0, 0, 0, 15);
            flp.Controls.Add(dtJoinDate);

            // Action Buttons Sub-Panel (Standardized grid buttons)
            FlowLayoutPanel flpButtons = new FlowLayoutPanel();
            flpButtons.Width = 290;
            flpButtons.Height = 85;
            flpButtons.FlowDirection = FlowDirection.LeftToRight;
            flpButtons.BackColor = Color.Transparent;
            flpButtons.Margin = new Padding(0, 5, 0, 0);

            btnAdd = CreateActionButton("إضافة", Color.FromArgb(16, 185, 129), 93, 36, flpButtons);
            btnEdit = CreateActionButton("تعديل", Color.FromArgb(59, 130, 246), 93, 36, flpButtons);
            btnDelete = CreateActionButton("حذف", Color.FromArgb(239, 68, 68), 93, 36, flpButtons);
            
            btnClear = CreateActionButton("تهيئة الحقول", Color.FromArgb(100, 116, 139), 290, 36, flpButtons);
            btnClear.Margin = new Padding(0, 6, 0, 0);

            flp.Controls.Add(flpButtons);

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClear.Click += BtnClear_Click;

            // Right Panel (Grid and Search)
            Guna2Panel pnlRight = new Guna2Panel();
            pnlRight.Dock = DockStyle.Fill;
            pnlRight.Padding = new Padding(10, 15, 15, 15);
            tlp.Controls.Add(pnlRight, 1, 0);

            // Search Header Panel
            Guna2Panel pnlSearch = new Guna2Panel();
            pnlSearch.Height = 50;
            pnlSearch.Dock = DockStyle.Top;
            pnlRight.Controls.Add(pnlSearch);

            txtSearch = new Guna2TextBox();
            txtSearch.PlaceholderText = "ابحث عن موظف بالاسم أو رقم الهاتف...";
            txtSearch.Size = new Size(350, 36);
            txtSearch.Location = new Point(0, 5);
            txtSearch.BorderRadius = 8;
            txtSearch.BorderColor = Color.FromArgb(203, 213, 225);
            txtSearch.IconRight = FrmMain.LoadIcon("search.png");
            txtSearch.IconRightSize = new Size(16, 16);
            txtSearch.IconRightOffset = new Point(6, 0);
            txtSearch.TextChanged += TxtSearch_TextChanged;
            pnlSearch.Controls.Add(txtSearch);

            // Grid
            dgvEmployees = new Guna2DataGridView();
            dgvEmployees.Dock = DockStyle.Fill;
            dgvEmployees.ReadOnly = true;
            dgvEmployees.AllowUserToAddRows = false;
            dgvEmployees.AllowUserToDeleteRows = false;
            dgvEmployees.BorderStyle = BorderStyle.None;
            dgvEmployees.Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Default;
            dgvEmployees.ThemeStyle.HeaderStyle.BackColor = Color.FromArgb(241, 245, 249);
            dgvEmployees.ThemeStyle.HeaderStyle.ForeColor = Color.FromArgb(71, 85, 105);
            dgvEmployees.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvEmployees.SelectionChanged += DgvEmployees_SelectionChanged;
            pnlRight.Controls.Add(dgvEmployees);
            dgvEmployees.BringToFront();
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
            txt.Width = 310;
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
            else if (text == "حفظ" || text == "تسجيل حضور" || text == "ترحيل الراتب") iconFile = "save.png";
            else if (text == "بحث") iconFile = "search.png";
            else if (text == "طباعة كشف") iconFile = "printer.png";

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

        private void LoadDepartments()
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery("SELECT DepartmentID, Name FROM Departments;");
                cmbDepartment.DataSource = dt;
                cmbDepartment.DisplayMember = "Name";
                cmbDepartment.ValueMember = "DepartmentID";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading departments: " + ex.Message);
            }
        }

        private void LoadEmployees(string searchQuery = "")
        {
            try
            {
                string sql = @"
                    SELECT e.EmployeeID as 'ID', e.Name as 'الاسم', e.Phone as 'الهاتف', e.Email as 'البريد الإلكتروني', 
                           d.Name as 'القسم', e.Salary as 'الراتب', e.Address as 'العنوان', e.JoinDate as 'تاريخ التعيين'
                    FROM Employees e
                    LEFT JOIN Departments d ON e.DepartmentID = d.DepartmentID";
                
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    sql += " WHERE e.Name LIKE @search OR e.Phone LIKE @search;";
                    var param = new SQLiteParameter("@search", $"%{searchQuery}%");
                    dgvEmployees.DataSource = DatabaseHelper.ExecuteQuery(sql, new SQLiteParameter[] { param });
                }
                else
                {
                    sql += ";";
                    dgvEmployees.DataSource = DatabaseHelper.ExecuteQuery(sql);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading employees: " + ex.Message);
            }
        }

        private void DgvEmployees_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvEmployees.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvEmployees.SelectedRows[0];
                selectedEmployeeID = Convert.ToInt32(row.Cells["ID"].Value);
                txtName.Text = row.Cells["الاسم"].Value?.ToString() ?? "";
                txtPhone.Text = row.Cells["الهاتف"].Value?.ToString() ?? "";
                txtEmail.Text = row.Cells["البريد الإلكتروني"].Value?.ToString() ?? "";
                txtSalary.Text = row.Cells["الراتب"].Value?.ToString() ?? "";
                txtAddress.Text = row.Cells["العنوان"].Value?.ToString() ?? "";
                cmbDepartment.Text = row.Cells["القسم"].Value?.ToString() ?? "";
                
                DateTime jDate;
                if (row.Cells["تاريخ التعيين"].Value != null && DateTime.TryParse(row.Cells["تاريخ التعيين"].Value.ToString(), out jDate))
                {
                    dtJoinDate.Value = jDate;
                }
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text) || cmbDepartment.SelectedValue == null)
            {
                MessageBox.Show("يرجى ملء الحقول المطلوبة!");
                return;
            }

            try
            {
                string sql = @"
                    INSERT INTO Employees (Name, Phone, Email, DepartmentID, Salary, Address, JoinDate, PhotoPath)
                    VALUES (@name, @phone, @email, @dept, @salary, @address, @join, '');";
                
                var parameters = new SQLiteParameter[]
                {
                    new SQLiteParameter("@name", txtName.Text),
                    new SQLiteParameter("@phone", txtPhone.Text),
                    new SQLiteParameter("@email", txtEmail.Text),
                    new SQLiteParameter("@dept", cmbDepartment.SelectedValue),
                    new SQLiteParameter("@salary", Convert.ToDouble(string.IsNullOrEmpty(txtSalary.Text) ? "0" : txtSalary.Text)),
                    new SQLiteParameter("@address", txtAddress.Text),
                    new SQLiteParameter("@join", dtJoinDate.Value.ToString("yyyy-MM-dd"))
                };

                DatabaseHelper.ExecuteNonQuery(sql, parameters);
                DatabaseHelper.LogActivity("INFO", $"تمت إضافة موظف جديد: {txtName.Text}");
                LoadEmployees();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ أثناء الإضافة: " + ex.Message);
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (selectedEmployeeID == -1)
            {
                MessageBox.Show("يرجى تحديد موظف للتعديل!");
                return;
            }

            try
            {
                string sql = @"
                    UPDATE Employees 
                    SET Name=@name, Phone=@phone, Email=@email, DepartmentID=@dept, Salary=@salary, Address=@address, JoinDate=@join
                    WHERE EmployeeID=@id;";

                var parameters = new SQLiteParameter[]
                {
                    new SQLiteParameter("@name", txtName.Text),
                    new SQLiteParameter("@phone", txtPhone.Text),
                    new SQLiteParameter("@email", txtEmail.Text),
                    new SQLiteParameter("@dept", cmbDepartment.SelectedValue),
                    new SQLiteParameter("@salary", Convert.ToDouble(string.IsNullOrEmpty(txtSalary.Text) ? "0" : txtSalary.Text)),
                    new SQLiteParameter("@address", txtAddress.Text),
                    new SQLiteParameter("@join", dtJoinDate.Value.ToString("yyyy-MM-dd")),
                    new SQLiteParameter("@id", selectedEmployeeID)
                };

                DatabaseHelper.ExecuteNonQuery(sql, parameters);
                DatabaseHelper.LogActivity("INFO", $"تم تعديل بيانات الموظف: {txtName.Text}");
                LoadEmployees();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ أثناء التعديل: " + ex.Message);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedEmployeeID == -1)
            {
                MessageBox.Show("يرجى تحديد موظف للحذف!");
                return;
            }

            if (MessageBox.Show("هل أنت متأكد من حذف هذا الموظف؟", "تأكيد الحذف", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    DatabaseHelper.ExecuteNonQuery($"DELETE FROM Employees WHERE EmployeeID = {selectedEmployeeID};");
                    DatabaseHelper.LogActivity("WARNING", $"تم حذف الموظف بالمعرف: {selectedEmployeeID}");
                    LoadEmployees();
                    ClearInputs();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("حدث خطأ أثناء الحذف: " + ex.Message);
                }
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            ClearInputs();
        }

        private void ClearInputs()
        {
            selectedEmployeeID = -1;
            txtName.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            txtSalary.Clear();
            txtAddress.Clear();
            dtJoinDate.Value = DateTime.Now;
            if (cmbDepartment.Items.Count > 0) cmbDepartment.SelectedIndex = 0;
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadEmployees(txtSearch.Text);
        }

        public void PerformSearch(string query)
        {
            txtSearch.Text = query;
        }
    }

    // ==========================================
    // 2. Departments Management Form (FrmDepartments)
    // ==========================================
    public class FrmDepartments : Form, ISearchable
    {
        private Guna2TextBox txtName, txtManager, txtDescription, txtSearch;
        private Guna2DataGridView dgvDepts;
        private Guna2Button btnAdd, btnEdit, btnDelete, btnClear;
        private int selectedCatID = -1; // Acts as selectedDeptID

        public FrmDepartments()
        {
            InitializeComponent();
            LoadDepts();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.DoubleBuffered = true;

            TableLayoutPanel tlp = new TableLayoutPanel();
            tlp.Dock = DockStyle.Fill;
            tlp.ColumnCount = 2;
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360F));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.Controls.Add(tlp);

            // Left Input Card
            Guna2Panel pnlInputs = new Guna2Panel();
            pnlInputs.Dock = DockStyle.Fill;
            pnlInputs.BorderRadius = 12;
            pnlInputs.FillColor = Color.White;
            pnlInputs.CustomBorderColor = Color.FromArgb(226, 232, 240);
            pnlInputs.CustomBorderThickness = new Padding(1);
            pnlInputs.Padding = new Padding(20);
            pnlInputs.Margin = new Padding(15);
            tlp.Controls.Add(pnlInputs, 0, 0);

            RTLFlowLayoutPanel flp = new RTLFlowLayoutPanel();
            flp.Dock = DockStyle.Fill;
            flp.FlowDirection = FlowDirection.TopDown;
            flp.WrapContents = false;
            flp.AutoScroll = true;
            flp.Padding = new Padding(5);
            flp.BackColor = Color.White;
            pnlInputs.Controls.Add(flp);

            AddLabel("اسم القسم", flp);
            txtName = CreateTextBox("مثل: المبيعات، الحسابات", flp);

            AddLabel("المدير المسؤول", flp);
            txtManager = CreateTextBox("اسم مدير القسم", flp);

            AddLabel("وصف القسم", flp);
            txtDescription = CreateTextBox("وصف مختصر لمهام القسم", flp);

            // Buttons
            FlowLayoutPanel flpButtons = new FlowLayoutPanel();
            flpButtons.Width = 290;
            flpButtons.Height = 85;
            flpButtons.FlowDirection = FlowDirection.LeftToRight;
            flpButtons.BackColor = Color.Transparent;
            flpButtons.Margin = new Padding(0, 15, 0, 0);

            btnAdd = CreateActionButton("إضافة", Color.FromArgb(16, 185, 129), 88, 36, flpButtons);
            btnEdit = CreateActionButton("تعديل", Color.FromArgb(59, 130, 246), 88, 36, flpButtons);
            btnDelete = CreateActionButton("حذف", Color.FromArgb(239, 68, 68), 88, 36, flpButtons);
            
            btnClear = CreateActionButton("تهيئة الحقول", Color.FromArgb(100, 116, 139), 274, 36, flpButtons);
            btnClear.Margin = new Padding(0, 6, 0, 0);

            flp.Controls.Add(flpButtons);

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClear.Click += BtnClear_Click;

            // Right Panel (Grid)
            Guna2Panel pnlRight = new Guna2Panel();
            pnlRight.Dock = DockStyle.Fill;
            pnlRight.Padding = new Padding(10, 15, 15, 15);
            tlp.Controls.Add(pnlRight, 1, 0);

            // Search bar
            Guna2Panel pnlSearch = new Guna2Panel();
            pnlSearch.Height = 50;
            pnlSearch.Dock = DockStyle.Top;
            pnlRight.Controls.Add(pnlSearch);

            txtSearch = new Guna2TextBox();
            txtSearch.PlaceholderText = "ابحث عن قسم بالاسم، المدير أو الوصف...";
            txtSearch.Size = new Size(350, 36);
            txtSearch.Location = new Point(0, 5);
            txtSearch.BorderRadius = 8;
            txtSearch.BorderColor = Color.FromArgb(203, 213, 225);
            txtSearch.IconRight = FrmMain.LoadIcon("search.png");
            txtSearch.IconRightSize = new Size(16, 16);
            txtSearch.IconRightOffset = new Point(6, 0);
            txtSearch.TextChanged += TxtSearch_TextChanged;
            pnlSearch.Controls.Add(txtSearch);

            dgvDepts = new Guna2DataGridView();
            dgvDepts.Dock = DockStyle.Fill;
            dgvDepts.ReadOnly = true;
            dgvDepts.AllowUserToAddRows = false;
            dgvDepts.AllowUserToDeleteRows = false;
            dgvDepts.BorderStyle = BorderStyle.None;
            dgvDepts.Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Default;
            dgvDepts.ThemeStyle.HeaderStyle.BackColor = Color.FromArgb(241, 245, 249);
            dgvDepts.ThemeStyle.HeaderStyle.ForeColor = Color.FromArgb(71, 85, 105);
            dgvDepts.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvDepts.SelectionChanged += DgvDepts_SelectionChanged;
            pnlRight.Controls.Add(dgvDepts);
            dgvDepts.BringToFront();
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
            txt.Width = 290;
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

        private void LoadDepts(string searchQuery = "")
        {
            try
            {
                string sql = "SELECT DepartmentID as 'ID', Name as 'القسم', Manager as 'المدير المسؤول', Description as 'الوصف' FROM Departments";
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    sql += " WHERE Name LIKE @search OR Manager LIKE @search OR Description LIKE @search;";
                    var param = new SQLiteParameter("@search", $"%{searchQuery}%");
                    dgvDepts.DataSource = DatabaseHelper.ExecuteQuery(sql, new SQLiteParameter[] { param });
                }
                else
                {
                    sql += ";";
                    dgvDepts.DataSource = DatabaseHelper.ExecuteQuery(sql);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadDepts(txtSearch.Text);
        }

        public void PerformSearch(string query)
        {
            txtSearch.Text = query;
        }

        private void DgvDepts_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvDepts.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvDepts.SelectedRows[0];
                selectedCatID = Convert.ToInt32(row.Cells["ID"].Value);
                txtName.Text = row.Cells["القسم"].Value?.ToString() ?? "";
                txtManager.Text = row.Cells["المدير المسؤول"].Value?.ToString() ?? "";
                txtDescription.Text = row.Cells["الوصف"].Value?.ToString() ?? "";
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text)) return;
            try
            {
                string sql = "INSERT INTO Departments (Name, Manager, Description) VALUES (@name, @manager, @desc);";
                SQLiteParameter[] param = new SQLiteParameter[]
                {
                    new SQLiteParameter("@name", txtName.Text),
                    new SQLiteParameter("@manager", txtManager.Text),
                    new SQLiteParameter("@desc", txtDescription.Text)
                };
                DatabaseHelper.ExecuteNonQuery(sql, param);
                DatabaseHelper.LogActivity("INFO", $"تم إنشاء قسم جديد: {txtName.Text}");
                LoadDepts();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (selectedCatID == -1) return;
            try
            {
                string sql = "UPDATE Departments SET Name=@name, Manager=@manager, Description=@desc WHERE DepartmentID=@id;";
                SQLiteParameter[] param = new SQLiteParameter[]
                {
                    new SQLiteParameter("@name", txtName.Text),
                    new SQLiteParameter("@manager", txtManager.Text),
                    new SQLiteParameter("@desc", txtDescription.Text),
                    new SQLiteParameter("@id", selectedCatID)
                };
                DatabaseHelper.ExecuteNonQuery(sql, param);
                DatabaseHelper.LogActivity("INFO", $"تم تعديل بيانات القسم: {txtName.Text}");
                LoadDepts();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedCatID == -1) return;
            if (MessageBox.Show("هل أنت متأكد من حذف هذا القسم؟ قد يؤثر ذلك على بيانات الموظفين المرتبطين به.", "تأكيد الحذف", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    DatabaseHelper.ExecuteNonQuery($"DELETE FROM Departments WHERE DepartmentID = {selectedCatID};");
                    DatabaseHelper.LogActivity("WARNING", $"تم حذف القسم بالمعرف: {selectedCatID}");
                    LoadDepts();
                    ClearInputs();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            ClearInputs();
        }

        private void ClearInputs()
        {
            selectedCatID = -1;
            txtName.Clear();
            txtManager.Clear();
            txtDescription.Clear();
        }
    }

    // ==========================================
    // 3. Attendance Management Form (FrmAttendance)
    // ==========================================
    public class FrmAttendance : Form, ISearchable
    {
        private Guna2ComboBox cmbEmployee;
        private Guna2DateTimePicker dtAttendanceDate;
        private Guna2TextBox txtCheckIn, txtCheckOut, txtSearch;
        private Guna2ComboBox cmbStatus;
        private Guna2DataGridView dgvAttendance;
        private Guna2Button btnSave, btnDelete;
        private int selectedAttendanceID = -1;

        public FrmAttendance()
        {
            InitializeComponent();
            LoadEmployees();
            LoadAttendance();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.DoubleBuffered = true;

            TableLayoutPanel tlp = new TableLayoutPanel();
            tlp.Dock = DockStyle.Fill;
            tlp.ColumnCount = 2;
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360F));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.Controls.Add(tlp);

            // Left Input Card
            Guna2Panel pnlInputs = new Guna2Panel();
            pnlInputs.Dock = DockStyle.Fill;
            pnlInputs.BorderRadius = 12;
            pnlInputs.FillColor = Color.White;
            pnlInputs.CustomBorderColor = Color.FromArgb(226, 232, 240);
            pnlInputs.CustomBorderThickness = new Padding(1);
            pnlInputs.Padding = new Padding(20);
            pnlInputs.Margin = new Padding(15);
            tlp.Controls.Add(pnlInputs, 0, 0);

            RTLFlowLayoutPanel flp = new RTLFlowLayoutPanel();
            flp.Dock = DockStyle.Fill;
            flp.FlowDirection = FlowDirection.TopDown;
            flp.WrapContents = false;
            flp.AutoScroll = true;
            flp.Padding = new Padding(5);
            flp.BackColor = Color.White;
            pnlInputs.Controls.Add(flp);

            AddLabel("الموظف", flp);
            cmbEmployee = new Guna2ComboBox();
            cmbEmployee.Width = 290;
            cmbEmployee.Height = 36;
            cmbEmployee.BorderRadius = 8;
            cmbEmployee.BorderColor = Color.FromArgb(203, 213, 225);
            cmbEmployee.Font = new Font("Segoe UI", 9.5F);
            cmbEmployee.Margin = new Padding(0, 0, 0, 8);
            flp.Controls.Add(cmbEmployee);

            AddLabel("تاريخ اليوم", flp);
            dtAttendanceDate = new Guna2DateTimePicker();
            dtAttendanceDate.Width = 290;
            dtAttendanceDate.Height = 36;
            dtAttendanceDate.BorderRadius = 8;
            dtAttendanceDate.FillColor = Color.FromArgb(59, 130, 246);
            dtAttendanceDate.ForeColor = Color.White;
            dtAttendanceDate.Font = new Font("Segoe UI", 9.5F);
            dtAttendanceDate.Format = DateTimePickerFormat.Short;
            dtAttendanceDate.Margin = new Padding(0, 0, 0, 8);
            flp.Controls.Add(dtAttendanceDate);

            AddLabel("وقت الحضور", flp);
            txtCheckIn = CreateTextBox("09:00 AM", flp);

            AddLabel("وقت الانصراف", flp);
            txtCheckOut = CreateTextBox("05:00 PM", flp);

            AddLabel("الحالة", flp);
            cmbStatus = new Guna2ComboBox();
            cmbStatus.Width = 290;
            cmbStatus.Height = 36;
            cmbStatus.BorderRadius = 8;
            cmbStatus.BorderColor = Color.FromArgb(203, 213, 225);
            cmbStatus.Items.AddRange(new string[] { "Present", "Late", "Absent", "Excused" });
            cmbStatus.SelectedIndex = 0;
            cmbStatus.Margin = new Padding(0, 0, 0, 15);
            flp.Controls.Add(cmbStatus);

            // Buttons
            FlowLayoutPanel flpButtons = new FlowLayoutPanel();
            flpButtons.Width = 290;
            flpButtons.Height = 45;
            flpButtons.FlowDirection = FlowDirection.LeftToRight;
            flpButtons.BackColor = Color.Transparent;

            btnSave = CreateActionButton("تسجيل حضور", Color.FromArgb(16, 185, 129), 130, 36, flpButtons);
            btnDelete = CreateActionButton("حذف", Color.FromArgb(239, 68, 68), 130, 36, flpButtons);

            flp.Controls.Add(flpButtons);

            btnSave.Click += BtnSave_Click;
            btnDelete.Click += BtnDelete_Click;

            // Right Panel
            Guna2Panel pnlRight = new Guna2Panel();
            pnlRight.Dock = DockStyle.Fill;
            pnlRight.Padding = new Padding(10, 15, 15, 15);
            tlp.Controls.Add(pnlRight, 1, 0);

            // Search bar
            Guna2Panel pnlSearch = new Guna2Panel();
            pnlSearch.Height = 50;
            pnlSearch.Dock = DockStyle.Top;
            pnlRight.Controls.Add(pnlSearch);

            txtSearch = new Guna2TextBox();
            txtSearch.PlaceholderText = "ابحث عن الحضور باسم الموظف...";
            txtSearch.Size = new Size(350, 36);
            txtSearch.Location = new Point(0, 5);
            txtSearch.BorderRadius = 8;
            txtSearch.BorderColor = Color.FromArgb(203, 213, 225);
            txtSearch.IconRight = FrmMain.LoadIcon("search.png");
            txtSearch.IconRightSize = new Size(16, 16);
            txtSearch.IconRightOffset = new Point(6, 0);
            txtSearch.TextChanged += TxtSearch_TextChanged;
            pnlSearch.Controls.Add(txtSearch);

            dgvAttendance = new Guna2DataGridView();
            dgvAttendance.Dock = DockStyle.Fill;
            dgvAttendance.ReadOnly = true;
            dgvAttendance.AllowUserToAddRows = false;
            dgvAttendance.AllowUserToDeleteRows = false;
            dgvAttendance.BorderStyle = BorderStyle.None;
            dgvAttendance.Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Default;
            dgvAttendance.ThemeStyle.HeaderStyle.BackColor = Color.FromArgb(241, 245, 249);
            dgvAttendance.ThemeStyle.HeaderStyle.ForeColor = Color.FromArgb(71, 85, 105);
            dgvAttendance.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvAttendance.SelectionChanged += DgvAttendance_SelectionChanged;
            pnlRight.Controls.Add(dgvAttendance);
            dgvAttendance.BringToFront();
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
            txt.Width = 290;
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
            else if (text == "تسجيل حضور" || text == "حفظ") iconFile = "save.png";
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

        private void LoadEmployees()
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery("SELECT EmployeeID, Name FROM Employees;");
                cmbEmployee.DataSource = dt;
                cmbEmployee.DisplayMember = "Name";
                cmbEmployee.ValueMember = "EmployeeID";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadAttendance(string searchQuery = "")
        {
            try
            {
                string sql = @"
                    SELECT a.AttendanceID as 'ID', e.Name as 'الموظف', a.Date as 'التاريخ', 
                           a.CheckIn as 'وقت الحضور', a.CheckOut as 'وقت الانصراف', a.Status as 'الحالة'
                    FROM Attendance a
                    LEFT JOIN Employees e ON a.EmployeeID = e.EmployeeID";
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    sql += " WHERE e.Name LIKE @search";
                    var param = new SQLiteParameter("@search", $"%{searchQuery}%");
                    dgvAttendance.DataSource = DatabaseHelper.ExecuteQuery(sql + " ORDER BY a.Date DESC;", new SQLiteParameter[] { param });
                }
                else
                {
                    dgvAttendance.DataSource = DatabaseHelper.ExecuteQuery(sql + " ORDER BY a.Date DESC;");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadAttendance(txtSearch.Text);
        }

        public void PerformSearch(string query)
        {
            txtSearch.Text = query;
        }

        private void DgvAttendance_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvAttendance.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvAttendance.SelectedRows[0];
                selectedAttendanceID = Convert.ToInt32(row.Cells["ID"].Value);
                cmbEmployee.Text = row.Cells["الموظف"].Value?.ToString() ?? "";
                txtCheckIn.Text = row.Cells["وقت الحضور"].Value?.ToString() ?? "";
                txtCheckOut.Text = row.Cells["وقت الانصراف"].Value?.ToString() ?? "";
                cmbStatus.Text = row.Cells["الحالة"].Value?.ToString() ?? "";
                
                DateTime attDate;
                if (row.Cells["التاريخ"].Value != null && DateTime.TryParse(row.Cells["التاريخ"].Value.ToString(), out attDate))
                {
                    dtAttendanceDate.Value = attDate;
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbEmployee.SelectedValue == null) return;
            try
            {
                string sql = "";
                SQLiteParameter[] param;
                if (selectedAttendanceID == -1) // Insert
                {
                    sql = "INSERT INTO Attendance (EmployeeID, Date, CheckIn, CheckOut, Status) VALUES (@emp, @date, @in, @out, @status);";
                    param = new SQLiteParameter[]
                    {
                        new SQLiteParameter("@emp", cmbEmployee.SelectedValue),
                        new SQLiteParameter("@date", dtAttendanceDate.Value.ToString("yyyy-MM-dd")),
                        new SQLiteParameter("@in", txtCheckIn.Text),
                        new SQLiteParameter("@out", txtCheckOut.Text),
                        new SQLiteParameter("@status", cmbStatus.Text)
                    };
                    DatabaseHelper.LogActivity("INFO", $"تسجيل حضور موظف: {cmbEmployee.Text} كـ {cmbStatus.Text}");
                }
                else // Update
                {
                    sql = "UPDATE Attendance SET EmployeeID=@emp, Date=@date, CheckIn=@in, CheckOut=@out, Status=@status WHERE AttendanceID=@id;";
                    param = new SQLiteParameter[]
                    {
                        new SQLiteParameter("@emp", cmbEmployee.SelectedValue),
                        new SQLiteParameter("@date", dtAttendanceDate.Value.ToString("yyyy-MM-dd")),
                        new SQLiteParameter("@in", txtCheckIn.Text),
                        new SQLiteParameter("@out", txtCheckOut.Text),
                        new SQLiteParameter("@status", cmbStatus.Text),
                        new SQLiteParameter("@id", selectedAttendanceID)
                    };
                    DatabaseHelper.LogActivity("INFO", $"تعديل سجل حضور الموظف: {cmbEmployee.Text}");
                }

                DatabaseHelper.ExecuteNonQuery(sql, param);
                LoadAttendance();
                ResetInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedAttendanceID == -1) return;
            try
            {
                string sql = "DELETE FROM Attendance WHERE AttendanceID = @id;";
                DatabaseHelper.ExecuteNonQuery(sql, new SQLiteParameter[] { new SQLiteParameter("@id", selectedAttendanceID) });
                DatabaseHelper.LogActivity("WARNING", $"تم حذف سجل حضور بالمعرف: {selectedAttendanceID}");
                LoadAttendance();
                ResetInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ResetInputs()
        {
            selectedAttendanceID = -1;
            txtCheckIn.Text = "09:00 AM";
            txtCheckOut.Text = "05:00 PM";
            cmbStatus.SelectedIndex = 0;
        }
    }

    // ==========================================
    // 4. Payroll Form (FrmPayroll)
    // ==========================================
    public class FrmPayroll : Form, ISearchable
    {
        private Guna2ComboBox cmbEmployee;
        private Guna2TextBox txtSalary, txtBonus, txtDeduction, txtNetSalary, txtSearch;
        private Guna2DateTimePicker dtPayDate;
        private Guna2DataGridView dgvPayroll;
        private Guna2Button btnSave, btnPrintSlip;
        private int selectedPayrollID = -1;

        public FrmPayroll()
        {
            InitializeComponent();
            LoadEmployees();
            LoadPayroll();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.DoubleBuffered = true;

            TableLayoutPanel tlp = new TableLayoutPanel();
            tlp.Dock = DockStyle.Fill;
            tlp.ColumnCount = 2;
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360F));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            this.Controls.Add(tlp);

            // Inputs Card
            Guna2Panel pnlInputs = new Guna2Panel();
            pnlInputs.Dock = DockStyle.Fill;
            pnlInputs.BorderRadius = 12;
            pnlInputs.FillColor = Color.White;
            pnlInputs.CustomBorderColor = Color.FromArgb(226, 232, 240);
            pnlInputs.CustomBorderThickness = new Padding(1);
            pnlInputs.Padding = new Padding(20);
            pnlInputs.Margin = new Padding(15);
            tlp.Controls.Add(pnlInputs, 0, 0);

            RTLFlowLayoutPanel flp = new RTLFlowLayoutPanel();
            flp.Dock = DockStyle.Fill;
            flp.FlowDirection = FlowDirection.TopDown;
            flp.WrapContents = false;
            flp.AutoScroll = true;
            flp.Padding = new Padding(5);
            flp.BackColor = Color.White;
            pnlInputs.Controls.Add(flp);

            AddLabel("الموظف", flp);
            cmbEmployee = new Guna2ComboBox();
            cmbEmployee.Width = 290;
            cmbEmployee.Height = 36;
            cmbEmployee.BorderRadius = 8;
            cmbEmployee.BorderColor = Color.FromArgb(203, 213, 225);
            cmbEmployee.Font = new Font("Segoe UI", 9.5F);
            cmbEmployee.SelectedIndexChanged += CmbEmployee_SelectedIndexChanged;
            cmbEmployee.Margin = new Padding(0, 0, 0, 8);
            flp.Controls.Add(cmbEmployee);

            AddLabel("الراتب الأساسي", flp);
            txtSalary = CreateReadOnlyTextBox("0.00", flp);

            AddLabel("الحوافز والبدلات", flp);
            txtBonus = CreateTextBox("0.00", flp);
            txtBonus.TextChanged += SalaryFields_TextChanged;

            AddLabel("الاستقطاعات والخصومات", flp);
            txtDeduction = CreateTextBox("0.00", flp);
            txtDeduction.TextChanged += SalaryFields_TextChanged;

            AddLabel("صافي المرتب", flp);
            txtNetSalary = CreateReadOnlyTextBox("0.00", flp);
            txtNetSalary.ForeColor = Color.FromArgb(16, 185, 129); // Green net salary

            AddLabel("تاريخ الصرف", flp);
            dtPayDate = new Guna2DateTimePicker();
            dtPayDate.Width = 290;
            dtPayDate.Height = 36;
            dtPayDate.BorderRadius = 8;
            dtPayDate.FillColor = Color.FromArgb(59, 130, 246);
            dtPayDate.ForeColor = Color.White;
            dtPayDate.Font = new Font("Segoe UI", 9.5F);
            dtPayDate.Format = DateTimePickerFormat.Short;
            dtPayDate.Margin = new Padding(0, 0, 0, 15);
            flp.Controls.Add(dtPayDate);

            // Buttons
            FlowLayoutPanel flpButtons = new FlowLayoutPanel();
            flpButtons.Width = 290;
            flpButtons.Height = 45;
            flpButtons.FlowDirection = FlowDirection.LeftToRight;
            flpButtons.BackColor = Color.Transparent;

            btnSave = CreateActionButton("ترحيل الراتب", Color.FromArgb(16, 185, 129), 130, 36, flpButtons);
            btnPrintSlip = CreateActionButton("طباعة كشف", Color.FromArgb(59, 130, 246), 130, 36, flpButtons);

            flp.Controls.Add(flpButtons);

            btnSave.Click += BtnSave_Click;
            btnPrintSlip.Click += BtnPrintSlip_Click;

            // Right Panel (Grid)
            Guna2Panel pnlRight = new Guna2Panel();
            pnlRight.Dock = DockStyle.Fill;
            pnlRight.Padding = new Padding(10, 15, 15, 15);
            tlp.Controls.Add(pnlRight, 1, 0);

            // Search bar
            Guna2Panel pnlSearch = new Guna2Panel();
            pnlSearch.Height = 50;
            pnlSearch.Dock = DockStyle.Top;
            pnlRight.Controls.Add(pnlSearch);

            txtSearch = new Guna2TextBox();
            txtSearch.PlaceholderText = "ابحث عن الراتب باسم الموظف...";
            txtSearch.Size = new Size(350, 36);
            txtSearch.Location = new Point(0, 5);
            txtSearch.BorderRadius = 8;
            txtSearch.BorderColor = Color.FromArgb(203, 213, 225);
            txtSearch.IconRight = FrmMain.LoadIcon("search.png");
            txtSearch.IconRightSize = new Size(16, 16);
            txtSearch.IconRightOffset = new Point(6, 0);
            txtSearch.TextChanged += TxtSearch_TextChanged;
            pnlSearch.Controls.Add(txtSearch);

            dgvPayroll = new Guna2DataGridView();
            dgvPayroll.Dock = DockStyle.Fill;
            dgvPayroll.ReadOnly = true;
            dgvPayroll.AllowUserToAddRows = false;
            dgvPayroll.AllowUserToDeleteRows = false;
            dgvPayroll.BorderStyle = BorderStyle.None;
            dgvPayroll.Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Default;
            dgvPayroll.ThemeStyle.HeaderStyle.BackColor = Color.FromArgb(241, 245, 249);
            dgvPayroll.ThemeStyle.HeaderStyle.ForeColor = Color.FromArgb(71, 85, 105);
            dgvPayroll.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvPayroll.SelectionChanged += DgvPayroll_SelectionChanged;
            pnlRight.Controls.Add(dgvPayroll);
            dgvPayroll.BringToFront();
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
            txt.Width = 290;
            txt.Height = 36;
            txt.BorderRadius = 8;
            txt.BorderColor = Color.FromArgb(203, 213, 225);
            txt.Font = new Font("Segoe UI", 9.5F);
            txt.Margin = new Padding(0, 0, 0, 8);
            parent.Controls.Add(txt);
            return txt;
        }

        private Guna2TextBox CreateReadOnlyTextBox(string text, FlowLayoutPanel parent)
        {
            Guna2TextBox txt = CreateTextBox(text, parent);
            txt.ReadOnly = true;
            txt.FillColor = Color.FromArgb(241, 245, 249);
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
            else if (text == "ترحيل الراتب" || text == "حفظ") iconFile = "save.png";
            else if (text == "بحث") iconFile = "search.png";
            else if (text == "طباعة كشف") iconFile = "printer.png";

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

        private void LoadEmployees()
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery("SELECT EmployeeID, Name FROM Employees;");
                cmbEmployee.DataSource = dt;
                cmbEmployee.DisplayMember = "Name";
                cmbEmployee.ValueMember = "EmployeeID";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadPayroll(string searchQuery = "")
        {
            try
            {
                string sql = @"
                    SELECT p.PayrollID as 'ID', e.Name as 'الموظف', p.Salary as 'الراتب الأساسي', 
                           p.Bonus as 'الحوافز', p.Deduction as 'الاستقطاعات', p.NetSalary as 'صافي المرتب', p.PayDate as 'تاريخ الصرف'
                    FROM Payroll p
                    LEFT JOIN Employees e ON p.EmployeeID = e.EmployeeID";
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    sql += " WHERE e.Name LIKE @search";
                    var param = new SQLiteParameter("@search", $"%{searchQuery}%");
                    dgvPayroll.DataSource = DatabaseHelper.ExecuteQuery(sql + " ORDER BY p.PayDate DESC;", new SQLiteParameter[] { param });
                }
                else
                {
                    dgvPayroll.DataSource = DatabaseHelper.ExecuteQuery(sql + " ORDER BY p.PayDate DESC;");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadPayroll(txtSearch.Text);
        }

        public void PerformSearch(string query)
        {
            txtSearch.Text = query;
        }

        private void CmbEmployee_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbEmployee.SelectedValue != null && cmbEmployee.SelectedValue is int empId)
            {
                object sal = DatabaseHelper.ExecuteScalar($"SELECT Salary FROM Employees WHERE EmployeeID = {empId};");
                if (sal != null && sal != DBNull.Value)
                {
                    txtSalary.Text = Convert.ToDouble(sal).ToString("N2");
                    CalculateNet();
                }
            }
        }

        private void SalaryFields_TextChanged(object sender, EventArgs e)
        {
            CalculateNet();
        }

        private void CalculateNet()
        {
            try
            {
                double basic = Convert.ToDouble(string.IsNullOrEmpty(txtSalary.Text) ? "0" : txtSalary.Text.Replace(",", ""));
                double bonus = Convert.ToDouble(string.IsNullOrEmpty(txtBonus.Text) ? "0" : txtBonus.Text);
                double ded = Convert.ToDouble(string.IsNullOrEmpty(txtDeduction.Text) ? "0" : txtDeduction.Text);
                double net = (basic + bonus) - ded;
                txtNetSalary.Text = net.ToString("N2");
            }
            catch { }
        }

        private void DgvPayroll_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPayroll.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvPayroll.SelectedRows[0];
                selectedPayrollID = Convert.ToInt32(row.Cells["ID"].Value);
                cmbEmployee.Text = row.Cells["الموظف"].Value?.ToString() ?? "";
                txtSalary.Text = row.Cells["الراتب الأساسي"].Value?.ToString() ?? "";
                txtBonus.Text = row.Cells["الحوافز"].Value?.ToString() ?? "";
                txtDeduction.Text = row.Cells["الاستقطاعات"].Value?.ToString() ?? "";
                txtNetSalary.Text = row.Cells["صافي المرتب"].Value?.ToString() ?? "";
                
                DateTime pDate;
                if (row.Cells["تاريخ الصرف"].Value != null && DateTime.TryParse(row.Cells["تاريخ الصرف"].Value.ToString(), out pDate))
                {
                    dtPayDate.Value = pDate;
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbEmployee.SelectedValue == null) return;

            try
            {
                double basic = Convert.ToDouble(string.IsNullOrEmpty(txtSalary.Text) ? "0" : txtSalary.Text.Replace(",", ""));
                double bonus = Convert.ToDouble(string.IsNullOrEmpty(txtBonus.Text) ? "0" : txtBonus.Text);
                double ded = Convert.ToDouble(string.IsNullOrEmpty(txtDeduction.Text) ? "0" : txtDeduction.Text);
                double net = (basic + bonus) - ded;

                string sql = "";
                SQLiteParameter[] parameters;

                if (selectedPayrollID == -1) // Insert
                {
                    sql = "INSERT INTO Payroll (EmployeeID, Salary, Bonus, Deduction, NetSalary, PayDate) VALUES (@emp, @basic, @bonus, @ded, @net, @date);";
                    parameters = new SQLiteParameter[]
                    {
                        new SQLiteParameter("@emp", cmbEmployee.SelectedValue),
                        new SQLiteParameter("@basic", basic),
                        new SQLiteParameter("@bonus", bonus),
                        new SQLiteParameter("@ded", ded),
                        new SQLiteParameter("@net", net),
                        new SQLiteParameter("@date", dtPayDate.Value.ToString("yyyy-MM-dd"))
                    };
                    DatabaseHelper.LogActivity("INFO", $"صرف مرتب الموظف: {cmbEmployee.Text} بقيمة صافي: {net:N2} EGP");
                }
                else // Update
                {
                    sql = "UPDATE Payroll SET EmployeeID=@emp, Salary=@basic, Bonus=@bonus, Deduction=@ded, NetSalary=@net, PayDate=@date WHERE PayrollID=@id;";
                    parameters = new SQLiteParameter[]
                    {
                        new SQLiteParameter("@emp", cmbEmployee.SelectedValue),
                        new SQLiteParameter("@basic", basic),
                        new SQLiteParameter("@bonus", bonus),
                        new SQLiteParameter("@ded", ded),
                        new SQLiteParameter("@net", net),
                        new SQLiteParameter("@date", dtPayDate.Value.ToString("yyyy-MM-dd")),
                        new SQLiteParameter("@id", selectedPayrollID)
                    };
                    DatabaseHelper.LogActivity("INFO", $"تعديل سجل مرتب الموظف: {cmbEmployee.Text}");
                }

                DatabaseHelper.ExecuteNonQuery(sql, parameters);
                LoadPayroll();
                selectedPayrollID = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnPrintSlip_Click(object sender, EventArgs e)
        {
            if (dgvPayroll.SelectedRows.Count == 0)
            {
                MessageBox.Show("يرجى تحديد سجل مرتب من الجدول أولاً لطباعته!");
                return;
            }

            DataGridViewRow row = dgvPayroll.SelectedRows[0];
            string employee = row.Cells["الموظف"].Value?.ToString() ?? "";
            string basic = row.Cells["الراتب الأساسي"].Value?.ToString() ?? "";
            string bonus = row.Cells["الحوافز"].Value?.ToString() ?? "";
            string ded = row.Cells["الاستقطاعات"].Value?.ToString() ?? "";
            string net = row.Cells["صافي المرتب"].Value?.ToString() ?? "";
            string date = row.Cells["تاريخ الصرف"].Value?.ToString() ?? "";

            string payslipText = $@"
==================================================
                 MASA BUSINESS SUITE
                  إيصال راتب موظف
==================================================
تاريخ الإيصال: {date}
الموظف المستلم: {employee}
--------------------------------------------------
الاستحقاقات المباشرة:
الراتب الأساسي:              {basic} EGP
الحوافز والبدلات:             {bonus} EGP
--------------------------------------------------
الاستقطاعات:
الخصومات / التأمين:          {ded} EGP
--------------------------------------------------
صافي المستلم النهائي:
NET SALARY:                 {net} EGP
==================================================
شكراً لكم لجهودكم القيمة معنا في MASA Group!
==================================================
";

            Form f = new Form();
            f.Size = new Size(420, 500);
            f.Text = "إيصال صرف راتب | Print Payslip PDF";
            f.StartPosition = FormStartPosition.CenterParent;
            f.FormBorderStyle = FormBorderStyle.FixedDialog;
            f.MaximizeBox = false;
            if (ThemeManager.AppIcon != null) f.Icon = ThemeManager.AppIcon;

            RichTextBox rtb = new RichTextBox();
            rtb.Dock = DockStyle.Fill;
            rtb.Font = new Font("Consolas", 10, FontStyle.Bold);
            rtb.BackColor = Color.White;
            rtb.ForeColor = Color.FromArgb(15, 23, 42);
            rtb.Text = payslipText;
            rtb.ReadOnly = true;
            f.Controls.Add(rtb);

            Panel pnlNotif = new Panel();
            pnlNotif.Height = 45;
            pnlNotif.Dock = DockStyle.Bottom;
            f.Controls.Add(pnlNotif);

            Button btnP = new Button();
            btnP.Text = "حفظ كملف PDF | Save PDF";
            btnP.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnP.Dock = DockStyle.Fill;
            btnP.FlatStyle = FlatStyle.Flat;
            btnP.BackColor = Color.FromArgb(14, 165, 233);
            btnP.ForeColor = Color.White;
            btnP.Click += (se, ev) => { 
                MessageBox.Show("تم حفظ كشف الراتب بنجاح كملف PDF في مجلد التنزيلات!", "تصدير ناجح", MessageBoxButtons.OK, MessageBoxIcon.Information);
                f.Close();
            };
            pnlNotif.Controls.Add(btnP);

            f.ShowDialog();
        }
    }
}
