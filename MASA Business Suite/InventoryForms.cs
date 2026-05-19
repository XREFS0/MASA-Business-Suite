using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace MASA_Business_Suite
{
    // ==========================================
    // 1. Products Management (FrmProducts)
    // ==========================================
    public class FrmProducts : Form, ISearchable
    {
        private Guna2TextBox txtName, txtBarcode, txtBuyPrice, txtSellPrice, txtQty, txtMinStock;
        private Guna2ComboBox cmbCategory;
        private Guna2DataGridView dgvProducts;
        private Guna2TextBox txtSearch;
        private Guna2Button btnAdd, btnEdit, btnDelete, btnClear;
        private int selectedProductID = -1;

        public FrmProducts()
        {
            InitializeComponent();
            LoadCategories();
            LoadProducts();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.DoubleBuffered = true;

            TableLayoutPanel tlp = new TableLayoutPanel();
            tlp.Dock = DockStyle.Fill;
            tlp.ColumnCount = 2;
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 380F)); // Input card
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));  // Grid
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

            // Inputs (Pure Arabic Labels)
            AddLabel("اسم المنتج", flp);
            txtName = CreateTextBox("شاشة DELL 24, كيبورد, إلخ", flp);

            AddLabel("الباركود / الرمز", flp);
            txtBarcode = CreateTextBox("أدخل أو امسح الباركود", flp);

            AddLabel("التصنيف", flp);
            cmbCategory = new Guna2ComboBox();
            cmbCategory.Width = 290;
            cmbCategory.Height = 36;
            cmbCategory.BorderRadius = 8;
            cmbCategory.BorderColor = Color.FromArgb(203, 213, 225);
            cmbCategory.Font = new Font("Segoe UI", 9.5F);
            cmbCategory.Margin = new Padding(0, 0, 0, 8);
            flp.Controls.Add(cmbCategory);

            AddLabel("سعر الشراء", flp);
            txtBuyPrice = CreateTextBox("0.00 EGP", flp);

            AddLabel("سعر البيع", flp);
            txtSellPrice = CreateTextBox("0.00 EGP", flp);

            AddLabel("الكمية المتوفرة", flp);
            txtQty = CreateTextBox("الكمية الحالية بالمستودع", flp);

            AddLabel("الحد الأدنى للتنبيه", flp);
            txtMinStock = CreateTextBox("5", flp);

            // Buttons
            FlowLayoutPanel flpButtons = new FlowLayoutPanel();
            flpButtons.Width = 290;
            flpButtons.Height = 85;
            flpButtons.FlowDirection = FlowDirection.LeftToRight;
            flpButtons.BackColor = Color.Transparent;
            flpButtons.Margin = new Padding(0, 10, 0, 0);

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
            txtSearch.PlaceholderText = "ابحث بالاسم أو الباركود...";
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
            dgvProducts = new Guna2DataGridView();
            dgvProducts.Dock = DockStyle.Fill;
            dgvProducts.ReadOnly = true;
            dgvProducts.AllowUserToAddRows = false;
            dgvProducts.AllowUserToDeleteRows = false;
            dgvProducts.BorderStyle = BorderStyle.None;
            dgvProducts.Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Default;
            dgvProducts.ThemeStyle.HeaderStyle.BackColor = Color.FromArgb(241, 245, 249);
            dgvProducts.ThemeStyle.HeaderStyle.ForeColor = Color.FromArgb(71, 85, 105);
            dgvProducts.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvProducts.SelectionChanged += DgvProducts_SelectionChanged;
            pnlRight.Controls.Add(dgvProducts);
            dgvProducts.BringToFront();
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

        private void LoadCategories()
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery("SELECT CategoryID, Name FROM Categories;");
                cmbCategory.DataSource = dt;
                cmbCategory.DisplayMember = "Name";
                cmbCategory.ValueMember = "CategoryID";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadProducts(string search = "")
        {
            try
            {
                string sql = @"
                    SELECT p.ProductID as 'ID', p.Name as 'المنتج', p.Barcode as 'الباركود', 
                           c.Name as 'التصنيف', p.BuyPrice as 'سعر الشراء', p.SellPrice as 'سعر البيع', 
                           p.Qty as 'الكمية', p.MinStock as 'الحد الأدنى'
                    FROM Products p
                    LEFT JOIN Categories c ON p.CategoryID = c.CategoryID";
                
                if (!string.IsNullOrEmpty(search))
                {
                    sql += " WHERE p.Name LIKE @search OR p.Barcode LIKE @search;";
                    var param = new SQLiteParameter("@search", $"%{search}%");
                    dgvProducts.DataSource = DatabaseHelper.ExecuteQuery(sql, new SQLiteParameter[] { param });
                }
                else
                {
                    sql += " ORDER BY p.ProductID DESC;";
                    dgvProducts.DataSource = DatabaseHelper.ExecuteQuery(sql);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DgvProducts_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvProducts.SelectedRows[0];
                selectedProductID = Convert.ToInt32(row.Cells["ID"].Value);
                txtName.Text = row.Cells["المنتج"].Value?.ToString() ?? "";
                txtBarcode.Text = row.Cells["الباركود"].Value?.ToString() ?? "";
                txtBuyPrice.Text = row.Cells["سعر الشراء"].Value?.ToString() ?? "";
                txtSellPrice.Text = row.Cells["سعر البيع"].Value?.ToString() ?? "";
                txtQty.Text = row.Cells["الكمية"].Value?.ToString() ?? "";
                txtMinStock.Text = row.Cells["الحد الأدنى"].Value?.ToString() ?? "";
                cmbCategory.Text = row.Cells["التصنيف"].Value?.ToString() ?? "";
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text) || cmbCategory.SelectedValue == null)
            {
                MessageBox.Show("يرجى ملء الاسم والتصنيف!");
                return;
            }

            try
            {
                string sql = @"
                    INSERT INTO Products (Name, Barcode, CategoryID, BuyPrice, SellPrice, Qty, MinStock)
                    VALUES (@name, @barcode, @cat, @buy, @sell, @qty, @min);";
                
                var param = new SQLiteParameter[]
                {
                    new SQLiteParameter("@name", txtName.Text),
                    new SQLiteParameter("@barcode", txtBarcode.Text),
                    new SQLiteParameter("@cat", cmbCategory.SelectedValue),
                    new SQLiteParameter("@buy", Convert.ToDouble(string.IsNullOrEmpty(txtBuyPrice.Text) ? "0" : txtBuyPrice.Text)),
                    new SQLiteParameter("@sell", Convert.ToDouble(string.IsNullOrEmpty(txtSellPrice.Text) ? "0" : txtSellPrice.Text)),
                    new SQLiteParameter("@qty", Convert.ToInt32(string.IsNullOrEmpty(txtQty.Text) ? "0" : txtQty.Text)),
                    new SQLiteParameter("@min", Convert.ToInt32(string.IsNullOrEmpty(txtMinStock.Text) ? "0" : txtMinStock.Text))
                };

                DatabaseHelper.ExecuteNonQuery(sql, param);
                DatabaseHelper.LogActivity("INFO", $"تمت إضافة المنتج الجديد: {txtName.Text}");
                LoadProducts();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (selectedProductID == -1) return;

            try
            {
                string sql = @"
                    UPDATE Products 
                    SET Name=@name, Barcode=@barcode, CategoryID=@cat, BuyPrice=@buy, SellPrice=@sell, Qty=@qty, MinStock=@min
                    WHERE ProductID=@id;";
                
                var param = new SQLiteParameter[]
                {
                    new SQLiteParameter("@name", txtName.Text),
                    new SQLiteParameter("@barcode", txtBarcode.Text),
                    new SQLiteParameter("@cat", cmbCategory.SelectedValue),
                    new SQLiteParameter("@buy", Convert.ToDouble(string.IsNullOrEmpty(txtBuyPrice.Text) ? "0" : txtBuyPrice.Text)),
                    new SQLiteParameter("@sell", Convert.ToDouble(string.IsNullOrEmpty(txtSellPrice.Text) ? "0" : txtSellPrice.Text)),
                    new SQLiteParameter("@qty", Convert.ToInt32(string.IsNullOrEmpty(txtQty.Text) ? "0" : txtQty.Text)),
                    new SQLiteParameter("@min", Convert.ToInt32(string.IsNullOrEmpty(txtMinStock.Text) ? "0" : txtMinStock.Text)),
                    new SQLiteParameter("@id", selectedProductID)
                };

                DatabaseHelper.ExecuteNonQuery(sql, param);
                DatabaseHelper.LogActivity("INFO", $"تم تعديل المنتج: {txtName.Text}");
                LoadProducts();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedProductID == -1) return;

            if (MessageBox.Show("هل أنت متأكد من حذف هذا المنتج؟", "تأكيد الحذف", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    DatabaseHelper.ExecuteNonQuery($"DELETE FROM Products WHERE ProductID={selectedProductID};");
                    DatabaseHelper.LogActivity("WARNING", $"تم حذف المنتج بالمعرف: {selectedProductID}");
                    LoadProducts();
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
            selectedProductID = -1;
            txtName.Clear();
            txtBarcode.Clear();
            txtBuyPrice.Clear();
            txtSellPrice.Clear();
            txtQty.Clear();
            txtMinStock.Clear();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadProducts(txtSearch.Text);
        }

        public void PerformSearch(string query)
        {
            txtSearch.Text = query;
        }
    }

    // ==========================================
    // 2. Categories Management (FrmCategories)
    // ==========================================
    public class FrmCategories : Form, ISearchable
    {
        private Guna2TextBox txtName, txtDescription, txtSearch;
        private Guna2DataGridView dgvCats;
        private Guna2Button btnAdd, btnEdit, btnDelete, btnClear;
        private int selectedCatID = -1;

        public FrmCategories()
        {
            InitializeComponent();
            LoadCategories();
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

            AddLabel("اسم التصنيف", flp);
            txtName = CreateTextBox("مثل: إلكترونيات، مكتبة", flp);

            AddLabel("الوصف", flp);
            txtDescription = CreateTextBox("وصف محتويات التصنيف", flp);

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

            // Right Panel Grid
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
            txtSearch.PlaceholderText = "ابحث عن تصنيف بالاسم أو الوصف...";
            txtSearch.Size = new Size(350, 36);
            txtSearch.Location = new Point(0, 5);
            txtSearch.BorderRadius = 8;
            txtSearch.BorderColor = Color.FromArgb(203, 213, 225);
            txtSearch.IconRight = FrmMain.LoadIcon("search.png");
            txtSearch.IconRightSize = new Size(16, 16);
            txtSearch.IconRightOffset = new Point(6, 0);
            txtSearch.TextChanged += TxtSearch_TextChanged;
            pnlSearch.Controls.Add(txtSearch);

            dgvCats = new Guna2DataGridView();
            dgvCats.Dock = DockStyle.Fill;
            dgvCats.ReadOnly = true;
            dgvCats.AllowUserToAddRows = false;
            dgvCats.AllowUserToDeleteRows = false;
            dgvCats.BorderStyle = BorderStyle.None;
            dgvCats.Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Default;
            dgvCats.ThemeStyle.HeaderStyle.BackColor = Color.FromArgb(241, 245, 249);
            dgvCats.ThemeStyle.HeaderStyle.ForeColor = Color.FromArgb(71, 85, 105);
            dgvCats.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvCats.SelectionChanged += DgvCats_SelectionChanged;
            pnlRight.Controls.Add(dgvCats);
            dgvCats.BringToFront();
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

        private void LoadCategories(string searchQuery = "")
        {
            try
            {
                string sql = "SELECT CategoryID as 'ID', Name as 'التصنيف', Description as 'الوصف' FROM Categories";
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    sql += " WHERE Name LIKE @search OR Description LIKE @search;";
                    SQLiteParameter[] param = new SQLiteParameter[]
                    {
                        new SQLiteParameter("@search", $"%{searchQuery}%")
                    };
                    dgvCats.DataSource = DatabaseHelper.ExecuteQuery(sql, param);
                }
                else
                {
                    sql += ";";
                    dgvCats.DataSource = DatabaseHelper.ExecuteQuery(sql);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadCategories(txtSearch.Text);
        }

        public void PerformSearch(string query)
        {
            txtSearch.Text = query;
        }

        private void DgvCats_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCats.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvCats.SelectedRows[0];
                selectedCatID = Convert.ToInt32(row.Cells["ID"].Value);
                txtName.Text = row.Cells["التصنيف"].Value?.ToString() ?? "";
                txtDescription.Text = row.Cells["الوصف"].Value?.ToString() ?? "";
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text)) return;
            try
            {
                string sql = "INSERT INTO Categories (Name, Description) VALUES (@name, @desc);";
                SQLiteParameter[] param = new SQLiteParameter[]
                {
                    new SQLiteParameter("@name", txtName.Text),
                    new SQLiteParameter("@desc", txtDescription.Text)
                };
                DatabaseHelper.ExecuteNonQuery(sql, param);
                DatabaseHelper.LogActivity("INFO", $"تم إنشاء التصنيف الجديد: {txtName.Text}");
                LoadCategories();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء الإضافة: " + ex.Message);
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (selectedCatID == -1) return;
            try
            {
                string sql = "UPDATE Categories SET Name=@name, Description=@desc WHERE CategoryID=@id;";
                SQLiteParameter[] param = new SQLiteParameter[]
                {
                    new SQLiteParameter("@name", txtName.Text),
                    new SQLiteParameter("@desc", txtDescription.Text),
                    new SQLiteParameter("@id", selectedCatID)
                };
                DatabaseHelper.ExecuteNonQuery(sql, param);
                DatabaseHelper.LogActivity("INFO", $"تم تعديل التصنيف: {txtName.Text}");
                LoadCategories();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء التعديل: " + ex.Message);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedCatID == -1) return;

            if (MessageBox.Show("هل أنت متأكد من حذف التصنيف؟ قد يؤثر ذلك على المنتجات التابعة له.", "تحذير الحذف", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    string sql = "DELETE FROM Categories WHERE CategoryID=@id;";
                    DatabaseHelper.ExecuteNonQuery(sql, new SQLiteParameter[] { new SQLiteParameter("@id", selectedCatID) });
                    DatabaseHelper.LogActivity("WARNING", $"تم حذف التصنيف بالمعرف: {selectedCatID}");
                    LoadCategories();
                    ClearInputs();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("خطأ أثناء الحذف: " + ex.Message);
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
            txtDescription.Clear();
        }
    }

    // ==========================================
    // 3. Inventory / Stock Control (FrmInventory)
    // ==========================================
    public class FrmInventory : Form, ISearchable
    {
        private Guna2DataGridView dgvStock;
        private Guna2TextBox txtAdjustmentQty, txtSearch;
        private Guna2Button btnStockIn, btnStockOut;
        private int selectedProductID = -1;

        public FrmInventory()
        {
            InitializeComponent();
            LoadStock();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.DoubleBuffered = true;

            TableLayoutPanel tlp = new TableLayoutPanel();
            tlp.Dock = DockStyle.Fill;
            tlp.ColumnCount = 2;
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F)); // Left Stock Table
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));  // Right Adjustments Panel
            this.Controls.Add(tlp);

            // Left Stock Table Card
            Guna2Panel pnlLeft = new Guna2Panel();
            pnlLeft.Dock = DockStyle.Fill;
            pnlLeft.BorderRadius = 12;
            pnlLeft.FillColor = Color.White;
            pnlLeft.CustomBorderColor = Color.FromArgb(226, 232, 240);
            pnlLeft.CustomBorderThickness = new Padding(1);
            pnlLeft.Padding = new Padding(20);
            pnlLeft.Margin = new Padding(15);
            tlp.Controls.Add(pnlLeft, 0, 0);

            Label lblTableTitle = new Label();
            lblTableTitle.Text = "حالة المخزون والكميات الحالية";
            lblTableTitle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblTableTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTableTitle.Dock = DockStyle.Top;
            lblTableTitle.Height = 30;
            pnlLeft.Controls.Add(lblTableTitle);

            // Search bar
            Guna2Panel pnlSearch = new Guna2Panel();
            pnlSearch.Height = 50;
            pnlSearch.Dock = DockStyle.Top;
            pnlLeft.Controls.Add(pnlSearch);

            txtSearch = new Guna2TextBox();
            txtSearch.PlaceholderText = "ابحث عن منتج بالمخزون بالاسم أو الباركود...";
            txtSearch.Size = new Size(350, 36);
            txtSearch.Location = new Point(0, 5);
            txtSearch.BorderRadius = 8;
            txtSearch.BorderColor = Color.FromArgb(203, 213, 225);
            txtSearch.IconRight = FrmMain.LoadIcon("search.png");
            txtSearch.IconRightSize = new Size(16, 16);
            txtSearch.IconRightOffset = new Point(6, 0);
            txtSearch.TextChanged += TxtSearch_TextChanged;
            pnlSearch.Controls.Add(txtSearch);

            dgvStock = new Guna2DataGridView();
            dgvStock.Dock = DockStyle.Fill;
            dgvStock.ReadOnly = true;
            dgvStock.AllowUserToAddRows = false;
            dgvStock.AllowUserToDeleteRows = false;
            dgvStock.BorderStyle = BorderStyle.None;
            dgvStock.Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Default;
            dgvStock.ThemeStyle.HeaderStyle.BackColor = Color.FromArgb(241, 245, 249);
            dgvStock.ThemeStyle.HeaderStyle.ForeColor = Color.FromArgb(71, 85, 105);
            dgvStock.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvStock.SelectionChanged += DgvStock_SelectionChanged;
            pnlLeft.Controls.Add(dgvStock);
            dgvStock.BringToFront();

            // Right Adjustments Card
            Guna2Panel pnlRight = new Guna2Panel();
            pnlRight.Dock = DockStyle.Fill;
            pnlRight.BorderRadius = 12;
            pnlRight.FillColor = Color.White;
            pnlRight.CustomBorderColor = Color.FromArgb(226, 232, 240);
            pnlRight.CustomBorderThickness = new Padding(1);
            pnlRight.Padding = new Padding(20);
            pnlRight.Margin = new Padding(15);
            tlp.Controls.Add(pnlRight, 1, 0);

            RTLFlowLayoutPanel flp = new RTLFlowLayoutPanel();
            flp.Dock = DockStyle.Fill;
            flp.FlowDirection = FlowDirection.TopDown;
            flp.WrapContents = false;
            flp.AutoScroll = true;
            flp.Padding = new Padding(5);
            flp.BackColor = Color.White;
            pnlRight.Controls.Add(flp);

            Label lblRightTitle = new Label();
            lblRightTitle.Text = "تعديل وتسوية المخزون";
            lblRightTitle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblRightTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblRightTitle.AutoSize = true;
            lblRightTitle.Margin = new Padding(0, 0, 0, 15);
            flp.Controls.Add(lblRightTitle);

            Label lblLabelQty = new Label();
            lblLabelQty.Text = "كمية التسوية";
            lblLabelQty.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblLabelQty.ForeColor = Color.FromArgb(71, 85, 105);
            lblLabelQty.AutoSize = true;
            lblLabelQty.Margin = new Padding(0, 0, 0, 4);
            flp.Controls.Add(lblLabelQty);

            txtAdjustmentQty = new Guna2TextBox();
            txtAdjustmentQty.PlaceholderText = "مثال: 10, 5, 20";
            txtAdjustmentQty.Width = 250;
            txtAdjustmentQty.Height = 36;
            txtAdjustmentQty.BorderRadius = 8;
            txtAdjustmentQty.BorderColor = Color.FromArgb(203, 213, 225);
            txtAdjustmentQty.Font = new Font("Segoe UI", 9.5F);
            txtAdjustmentQty.Margin = new Padding(0, 0, 0, 15);
            flp.Controls.Add(txtAdjustmentQty);

            btnStockIn = new Guna2Button();
            btnStockIn.Text = "توريد مخزن (+)";
            btnStockIn.FillColor = Color.FromArgb(16, 185, 129); // Green
            btnStockIn.ForeColor = Color.White;
            btnStockIn.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnStockIn.BorderRadius = 8;
            btnStockIn.Width = 250;
            btnStockIn.Height = 40;
            btnStockIn.Margin = new Padding(0, 0, 0, 10);
            btnStockIn.Click += BtnStockIn_Click;
            flp.Controls.Add(btnStockIn);

            btnStockOut = new Guna2Button();
            btnStockOut.Text = "صرف مخزن (-)";
            btnStockOut.FillColor = Color.FromArgb(239, 68, 68); // Red
            btnStockOut.ForeColor = Color.White;
            btnStockOut.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            btnStockOut.BorderRadius = 8;
            btnStockOut.Width = 250;
            btnStockOut.Height = 40;
            btnStockOut.Click += BtnStockOut_Click;
            flp.Controls.Add(btnStockOut);
        }

        private void LoadStock(string searchQuery = "")
        {
            try
            {
                string sql = @"
                    SELECT p.ProductID as 'ID', p.Name as 'المنتج', p.Barcode as 'الباركود', 
                           c.Name as 'التصنيف', p.Qty as 'الكمية الحالية', p.MinStock as 'الحد الأدنى للتنبيه'
                    FROM Products p
                    LEFT JOIN Categories c ON p.CategoryID = c.CategoryID";
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    sql += " WHERE p.Name LIKE @search OR p.Barcode LIKE @search";
                    var param = new SQLiteParameter("@search", $"%{searchQuery}%");
                    dgvStock.DataSource = DatabaseHelper.ExecuteQuery(sql + " ORDER BY p.Qty ASC;", new SQLiteParameter[] { param });
                }
                else
                {
                    dgvStock.DataSource = DatabaseHelper.ExecuteQuery(sql + " ORDER BY p.Qty ASC;");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadStock(txtSearch.Text);
        }

        public void PerformSearch(string query)
        {
            txtSearch.Text = query;
        }

        private void DgvStock_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvStock.SelectedRows.Count > 0)
            {
                selectedProductID = Convert.ToInt32(dgvStock.SelectedRows[0].Cells["ID"].Value);
            }
        }

        private void BtnStockIn_Click(object sender, EventArgs e)
        {
            AdjustStock(true);
        }

        private void BtnStockOut_Click(object sender, EventArgs e)
        {
            AdjustStock(false);
        }

        private void AdjustStock(bool isStockIn)
        {
            if (selectedProductID == -1)
            {
                MessageBox.Show("يرجى تحديد منتج من الجدول أولاً لتعديل كميته!");
                return;
            }

            int qtyChange;
            if (!int.TryParse(txtAdjustmentQty.Text, out qtyChange) || qtyChange <= 0)
            {
                MessageBox.Show("يرجى إدخال كمية تسوية صحيحة (رقم أكبر من الصفر)!");
                return;
            }

            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery($"SELECT Name, Qty FROM Products WHERE ProductID={selectedProductID};");
                if (dt.Rows.Count == 0) return;
                string pName = dt.Rows[0]["Name"].ToString();
                int currentQty = Convert.ToInt32(dt.Rows[0]["Qty"]);

                int newQty = isStockIn ? (currentQty + qtyChange) : (currentQty - qtyChange);
                if (newQty < 0)
                {
                    MessageBox.Show("خطأ: لا يمكن صرف كمية أكبر من المتوفرة بالمخزون حالياً!");
                    return;
                }

                DatabaseHelper.ExecuteNonQuery($"UPDATE Products SET Qty={newQty} WHERE ProductID={selectedProductID};");
                
                string action = isStockIn ? "توريد للمخزن" : "صرف من المخزن";
                DatabaseHelper.LogActivity(isStockIn ? "INFO" : "WARNING", 
                    $"تسوية مخزون لمنتج ({pName}): {action} بمقدار {qtyChange}. الكمية الجديدة: {newQty}");

                LoadStock();
                txtAdjustmentQty.Clear();
                MessageBox.Show("تمت تسوية وتحديث كمية المنتج بنجاح!", "نجاح العملية", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
