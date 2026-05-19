using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace MASA_Business_Suite
{
    // ==========================================
    // 1. Point of Sale (POS) Form (FrmPOS)
    // ==========================================
    public class FrmPOS : Form
    {
        private Form mainShell;
        private Guna2TextBox txtBarcodeSearch;
        private Guna2ComboBox cmbCategoryFilter, cmbCustomerSelect;
        private FlowLayoutPanel flpProducts;
        private Guna2DataGridView dgvCart;
        private Label lblSubtotal, lblTax, lblTotal;
        private Guna2TextBox txtDiscountPercent;
        private Guna2Button btnPayPrint, btnClearCart;
        private DataTable dtCart;

        public FrmPOS(Form mainForm)
        {
            this.mainShell = mainForm;
            InitializeComponent();
            SetupCartTable();
            LoadCategories();
            LoadCustomers();
            LoadProductCards();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.DoubleBuffered = true;

            TableLayoutPanel tlp = new TableLayoutPanel();
            tlp.Dock = DockStyle.Fill;
            tlp.ColumnCount = 2;
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));  // Left: Catalog
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));  // Right: Cart
            this.Controls.Add(tlp);

            // Left Catalog Panel
            Guna2Panel pnlCatalog = new Guna2Panel();
            pnlCatalog.Dock = DockStyle.Fill;
            pnlCatalog.Padding = new Padding(15);
            pnlCatalog.Margin = new Padding(10);
            tlp.Controls.Add(pnlCatalog, 0, 0);

            // Flow Catalog Header (Fluid!)
            FlowLayoutPanel flpCatalogHeader = new FlowLayoutPanel();
            flpCatalogHeader.Dock = DockStyle.Top;
            flpCatalogHeader.Height = 55;
            flpCatalogHeader.FlowDirection = FlowDirection.LeftToRight;
            flpCatalogHeader.BackColor = Color.Transparent;
            pnlCatalog.Controls.Add(flpCatalogHeader);

            txtBarcodeSearch = new Guna2TextBox();
            txtBarcodeSearch.PlaceholderText = "مسح باركود المنتج أو البحث...";
            txtBarcodeSearch.Width = 260;
            txtBarcodeSearch.Height = 36;
            txtBarcodeSearch.BorderRadius = 8;
            txtBarcodeSearch.BorderColor = Color.FromArgb(203, 213, 225);
            txtBarcodeSearch.KeyDown += TxtBarcodeSearch_KeyDown;
            txtBarcodeSearch.Margin = new Padding(0, 5, 10, 0);
            flpCatalogHeader.Controls.Add(txtBarcodeSearch);

            cmbCategoryFilter = new Guna2ComboBox();
            cmbCategoryFilter.Width = 180;
            cmbCategoryFilter.Height = 36;
            cmbCategoryFilter.BorderRadius = 8;
            cmbCategoryFilter.BorderColor = Color.FromArgb(203, 213, 225);
            cmbCategoryFilter.SelectedIndexChanged += CmbCategoryFilter_SelectedIndexChanged;
            cmbCategoryFilter.Margin = new Padding(0, 5, 0, 0);
            cmbCategoryFilter.Font = new Font("Segoe UI", 9.5F);
            flpCatalogHeader.Controls.Add(cmbCategoryFilter);

            // Products Flow Panel
            flpProducts = new FlowLayoutPanel();
            flpProducts.Dock = DockStyle.Fill;
            flpProducts.AutoScroll = true;
            flpProducts.BackColor = Color.FromArgb(248, 250, 252);
            pnlCatalog.Controls.Add(flpProducts);
            flpProducts.BringToFront();

            // Right Cart Panel
            Guna2Panel pnlCart = new Guna2Panel();
            pnlCart.Dock = DockStyle.Fill;
            pnlCart.BorderRadius = 12;
            pnlCart.FillColor = Color.White;
            pnlCart.CustomBorderColor = Color.FromArgb(226, 232, 240);
            pnlCart.CustomBorderThickness = new Padding(1);
            pnlCart.Padding = new Padding(15);
            pnlCart.Margin = new Padding(10);
            tlp.Controls.Add(pnlCart, 1, 0);

            // Cart Header Flow Panel
            FlowLayoutPanel flpCartHeader = new FlowLayoutPanel();
            flpCartHeader.Dock = DockStyle.Top;
            flpCartHeader.Height = 50;
            flpCartHeader.FlowDirection = FlowDirection.LeftToRight;
            flpCartHeader.BackColor = Color.White;
            pnlCart.Controls.Add(flpCartHeader);

            Label lblCust = new Label();
            lblCust.Text = "العميل";
            lblCust.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblCust.ForeColor = Color.FromArgb(71, 85, 105);
            lblCust.AutoSize = true;
            lblCust.Margin = new Padding(5, 12, 10, 0);
            flpCartHeader.Controls.Add(lblCust);

            cmbCustomerSelect = new Guna2ComboBox();
            cmbCustomerSelect.Width = 220;
            cmbCustomerSelect.Height = 36;
            cmbCustomerSelect.BorderRadius = 8;
            cmbCustomerSelect.BorderColor = Color.FromArgb(203, 213, 225);
            cmbCustomerSelect.Font = new Font("Segoe UI", 9.5F);
            cmbCustomerSelect.Margin = new Padding(0, 5, 0, 0);
            flpCartHeader.Controls.Add(cmbCustomerSelect);

            // Cart Grid
            dgvCart = new Guna2DataGridView();
            dgvCart.Dock = DockStyle.Fill;
            dgvCart.AllowUserToAddRows = false;
            dgvCart.AllowUserToDeleteRows = false;
            dgvCart.BorderStyle = BorderStyle.None;
            dgvCart.Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Default;
            dgvCart.ThemeStyle.HeaderStyle.BackColor = Color.FromArgb(241, 245, 249);
            dgvCart.ThemeStyle.HeaderStyle.ForeColor = Color.FromArgb(71, 85, 105);
            dgvCart.RowHeadersVisible = false;
            pnlCart.Controls.Add(dgvCart);
            dgvCart.BringToFront();

            // Cart Summary Panel
            Guna2Panel pnlSummary = new Guna2Panel();
            pnlSummary.Dock = DockStyle.Bottom;
            pnlSummary.Height = 220;
            pnlSummary.CustomBorderColor = Color.FromArgb(226, 232, 240);
            pnlSummary.CustomBorderThickness = new Padding(0, 1, 0, 0);
            pnlSummary.Padding = new Padding(10, 15, 10, 10);
            pnlCart.Controls.Add(pnlSummary);

            // Summary vertical flow panel
            FlowLayoutPanel flpSummary = new FlowLayoutPanel();
            flpSummary.Dock = DockStyle.Fill;
            flpSummary.FlowDirection = FlowDirection.TopDown;
            flpSummary.WrapContents = false;
            flpSummary.AutoScroll = true;
            flpSummary.BackColor = Color.White;
            flpSummary.Padding = new Padding(5);
            pnlSummary.Controls.Add(flpSummary);

            // Subtotal Row
            FlowLayoutPanel flpSub = new FlowLayoutPanel();
            flpSub.Width = 310;
            flpSub.Height = 28;
            flpSub.FlowDirection = FlowDirection.LeftToRight;
            
            Label lblSubTitle = new Label();
            lblSubTitle.Text = "الإجمالي الفرعي:";
            lblSubTitle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblSubTitle.ForeColor = Color.FromArgb(71, 85, 105);
            lblSubTitle.Width = 140;
            flpSub.Controls.Add(lblSubTitle);

            lblSubtotal = new Label();
            lblSubtotal.Text = "0.00 EGP";
            lblSubtotal.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblSubtotal.ForeColor = Color.FromArgb(15, 23, 42);
            lblSubtotal.Width = 140;
            lblSubtotal.TextAlign = ContentAlignment.TopRight;
            flpSub.Controls.Add(lblSubtotal);

            flpSummary.Controls.Add(flpSub);

            // Discount Row
            FlowLayoutPanel flpDisc = new FlowLayoutPanel();
            flpDisc.Width = 310;
            flpDisc.Height = 36;
            flpDisc.FlowDirection = FlowDirection.LeftToRight;

            Label lblDiscTitle = new Label();
            lblDiscTitle.Text = "الخصم %:";
            lblDiscTitle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblDiscTitle.ForeColor = Color.FromArgb(71, 85, 105);
            lblDiscTitle.Width = 140;
            lblDiscTitle.Margin = new Padding(0, 8, 0, 0);
            flpDisc.Controls.Add(lblDiscTitle);

            txtDiscountPercent = new Guna2TextBox();
            txtDiscountPercent.Text = "0";
            txtDiscountPercent.Width = 80;
            txtDiscountPercent.Height = 28;
            txtDiscountPercent.BorderRadius = 4;
            txtDiscountPercent.TextAlign = HorizontalAlignment.Center;
            txtDiscountPercent.TextChanged += TxtDiscountPercent_TextChanged;
            flpDisc.Controls.Add(txtDiscountPercent);

            flpSummary.Controls.Add(flpDisc);

            // Tax Row
            FlowLayoutPanel flpTax = new FlowLayoutPanel();
            flpTax.Width = 310;
            flpTax.Height = 28;
            flpTax.FlowDirection = FlowDirection.LeftToRight;

            Label lblTaxTitle = new Label();
            lblTaxTitle.Text = "الضريبة (14%):";
            lblTaxTitle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblTaxTitle.ForeColor = Color.FromArgb(71, 85, 105);
            lblTaxTitle.Width = 140;
            flpTax.Controls.Add(lblTaxTitle);

            lblTax = new Label();
            lblTax.Text = "0.00 EGP";
            lblTax.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblTax.ForeColor = Color.FromArgb(15, 23, 42);
            lblTax.Width = 140;
            lblTax.TextAlign = ContentAlignment.TopRight;
            flpTax.Controls.Add(lblTax);

            flpSummary.Controls.Add(flpTax);

            // Total Label
            lblTotal = new Label();
            lblTotal.Text = "المجموع النهائي:  0.00 EGP";
            lblTotal.Font = new Font("Outfit", 12.5F, FontStyle.Bold);
            lblTotal.ForeColor = Color.FromArgb(16, 185, 129); // emerald green
            lblTotal.Width = 310;
            lblTotal.Height = 30;
            lblTotal.Margin = new Padding(0, 5, 0, 8);
            flpSummary.Controls.Add(lblTotal);

            // Action Buttons Row
            FlowLayoutPanel flpAct = new FlowLayoutPanel();
            flpAct.Width = 310;
            flpAct.Height = 45;
            flpAct.FlowDirection = FlowDirection.LeftToRight;

            btnPayPrint = new Guna2Button();
            btnPayPrint.Text = "دفع وطباعة";
            btnPayPrint.FillColor = Color.FromArgb(16, 185, 129);
            btnPayPrint.ForeColor = Color.White;
            btnPayPrint.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnPayPrint.BorderRadius = 8;
            btnPayPrint.Width = 160;
            btnPayPrint.Height = 36;
            btnPayPrint.Click += BtnPayPrint_Click;
            flpAct.Controls.Add(btnPayPrint);

            btnClearCart = new Guna2Button();
            btnClearCart.Text = "تفريغ";
            btnClearCart.FillColor = Color.FromArgb(239, 68, 68);
            btnClearCart.ForeColor = Color.White;
            btnClearCart.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnClearCart.BorderRadius = 8;
            btnClearCart.Width = 100;
            btnClearCart.Height = 36;
            btnClearCart.Click += BtnClearCart_Click;
            flpAct.Controls.Add(btnClearCart);

            flpSummary.Controls.Add(flpAct);
        }

        private void SetupCartTable()
        {
            dtCart = new DataTable();
            dtCart.Columns.Add("ProductID", typeof(int));
            dtCart.Columns.Add("المنتج", typeof(string));
            dtCart.Columns.Add("الباركود", typeof(string));
            dtCart.Columns.Add("السعر", typeof(double));
            dtCart.Columns.Add("الكمية", typeof(int));
            dtCart.Columns.Add("الإجمالي", typeof(double));

            dgvCart.BindingContext = new BindingContext();
            dgvCart.DataSource = dtCart;

            if (dgvCart.Columns.Contains("ProductID")) dgvCart.Columns["ProductID"].Visible = false;
            if (dgvCart.Columns.Contains("المنتج")) dgvCart.Columns["المنتج"].Width = 140;
            if (dgvCart.Columns.Contains("الكمية")) dgvCart.Columns["الكمية"].Width = 60;
        }

        private void LoadCategories()
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery("SELECT CategoryID, Name FROM Categories;");
                
                DataRow r = dt.NewRow();
                r["CategoryID"] = 0;
                r["Name"] = "كل التصنيفات";
                dt.Rows.InsertAt(r, 0);

                cmbCategoryFilter.DataSource = dt;
                cmbCategoryFilter.DisplayMember = "Name";
                cmbCategoryFilter.ValueMember = "CategoryID";
            }
            catch { }
        }

        private void LoadCustomers()
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery("SELECT CustomerID, Name FROM Customers;");
                cmbCustomerSelect.DataSource = dt;
                cmbCustomerSelect.DisplayMember = "Name";
                cmbCustomerSelect.ValueMember = "CustomerID";
            }
            catch { }
        }

        private void LoadProductCards(string search = "", int categoryId = 0)
        {
            flpProducts.Controls.Clear();
            try
            {
                string sql = "SELECT ProductID, Name, Barcode, SellPrice, Qty FROM Products WHERE 1=1";
                if (categoryId > 0) sql += $" AND CategoryID = {categoryId}";
                if (!string.IsNullOrEmpty(search)) sql += " AND (Name LIKE @search OR Barcode = @searchBare)";

                SQLiteCommand cmd = new SQLiteCommand(sql, DatabaseHelper.GetConnection());
                if (!string.IsNullOrEmpty(search))
                {
                    cmd.Parameters.AddWithValue("@search", $"%{search}%");
                    cmd.Parameters.AddWithValue("@searchBare", search);
                }

                SQLiteDataAdapter da = new SQLiteDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    int pId = Convert.ToInt32(row["ProductID"]);
                    string name = row["Name"].ToString();
                    string barcode = row["Barcode"].ToString();
                    double price = Convert.ToDouble(row["SellPrice"]);
                    int qty = Convert.ToInt32(row["Qty"]);

                    Guna2Panel card = new Guna2Panel();
                    card.Size = new Size(160, 140);
                    card.BorderRadius = 10;
                    card.FillColor = Color.White;
                    card.CustomBorderColor = Color.FromArgb(226, 232, 240);
                    card.CustomBorderThickness = new Padding(1);
                    card.Margin = new Padding(8);

                    Label lblName = new Label();
                    lblName.Text = name;
                    lblName.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                    lblName.ForeColor = Color.FromArgb(15, 23, 42);
                    lblName.Location = new Point(10, 10);
                    lblName.Size = new Size(140, 40);
                    lblName.TextAlign = ContentAlignment.TopCenter;
                    card.Controls.Add(lblName);

                    Label lblPrice = new Label();
                    lblPrice.Text = $"{price:N2} EGP";
                    lblPrice.Font = new Font("Outfit", 10, FontStyle.Bold);
                    lblPrice.ForeColor = Color.FromArgb(14, 165, 233);
                    lblPrice.Location = new Point(10, 55);
                    lblPrice.Size = new Size(140, 20);
                    lblPrice.TextAlign = ContentAlignment.MiddleCenter;
                    card.Controls.Add(lblPrice);

                    Label lblQty = new Label();
                    lblQty.Text = $"المتاح: {qty}";
                    lblQty.Font = new Font("Segoe UI", 8, FontStyle.Regular);
                    lblQty.ForeColor = qty <= 5 ? Color.Red : Color.Gray;
                    lblQty.Location = new Point(10, 75);
                    lblQty.Size = new Size(140, 18);
                    lblQty.TextAlign = ContentAlignment.MiddleCenter;
                    card.Controls.Add(lblQty);

                    Guna2Button btnAdd = new Guna2Button();
                    btnAdd.Text = "إضافة للسلة";
                    btnAdd.FillColor = Color.FromArgb(59, 130, 246);
                    btnAdd.ForeColor = Color.White;
                    btnAdd.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
                    btnAdd.Size = new Size(140, 28);
                    btnAdd.Location = new Point(10, 100);
                    btnAdd.BorderRadius = 6;
                    btnAdd.Click += (s, e) => {
                        AddProductToCart(pId, name, barcode, price, qty);
                    };
                    card.Controls.Add(btnAdd);

                    flpProducts.Controls.Add(card);
                }
            }
            catch { }
        }

        private void AddProductToCart(int pId, string name, string barcode, double price, int maxQty)
        {
            DataRow[] found = dtCart.Select($"ProductID = {pId}");
            if (found.Length > 0)
            {
                int current = Convert.ToInt32(found[0]["الكمية"]);
                if (current >= maxQty)
                {
                    MessageBox.Show("لا توجد كمية كافية متوفرة في المستودع حالياً!", "تنبيه المخزون", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                found[0]["الكمية"] = current + 1;
                found[0]["الإجمالي"] = (current + 1) * price;
            }
            else
            {
                if (maxQty <= 0)
                {
                    MessageBox.Show("المنتج نافذ تماماً من المستودع!", "تنبيه المخزون", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                DataRow r = dtCart.NewRow();
                r["ProductID"] = pId;
                r["المنتج"] = name;
                r["الباركود"] = barcode;
                r["السعر"] = price;
                r["الكمية"] = 1;
                r["الإجمالي"] = price;
                dtCart.Rows.Add(r);
            }
            UpdateTotals();
        }

        private void UpdateTotals()
        {
            double subtotal = 0;
            foreach (DataRow row in dtCart.Rows)
            {
                subtotal += Convert.ToDouble(row["الإجمالي"]);
            }

            double discPercent = 0;
            double.TryParse(txtDiscountPercent.Text, out discPercent);

            double discount = subtotal * (discPercent / 100.0);
            double tax = (subtotal - discount) * 0.14;
            double total = (subtotal - discount) + tax;

            lblSubtotal.Text = $"{subtotal:N2} EGP";
            lblTax.Text = $"{tax:N2} EGP";
            lblTotal.Text = $"المجموع النهائي:  {total:N2} EGP";
        }

        private void TxtDiscountPercent_TextChanged(object sender, EventArgs e)
        {
            UpdateTotals();
        }

        private void CmbCategoryFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCategoryFilter.SelectedValue is int catId)
            {
                LoadProductCards(txtBarcodeSearch.Text, catId);
            }
        }

        private void TxtBarcodeSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string code = txtBarcodeSearch.Text;
                if (!string.IsNullOrEmpty(code))
                {
                    DataTable dt = DatabaseHelper.ExecuteQuery($"SELECT ProductID, Name, Barcode, SellPrice, Qty FROM Products WHERE Barcode = '{code}' OR Name LIKE '%{code}%';");
                    if (dt.Rows.Count > 0)
                    {
                        DataRow row = dt.Rows[0];
                        AddProductToCart(
                            Convert.ToInt32(row["ProductID"]),
                            row["Name"].ToString(),
                            row["Barcode"].ToString(),
                            Convert.ToDouble(row["SellPrice"]),
                            Convert.ToInt32(row["Qty"])
                        );
                        txtBarcodeSearch.Clear();
                    }
                    else
                    {
                        LoadProductCards(code, (int)cmbCategoryFilter.SelectedValue);
                    }
                }
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void BtnClearCart_Click(object sender, EventArgs e)
        {
            dtCart.Rows.Clear();
            UpdateTotals();
        }

        private void BtnPayPrint_Click(object sender, EventArgs e)
        {
            if (dtCart.Rows.Count == 0)
            {
                MessageBox.Show("سلة المشتريات فارغة!");
                return;
            }

            try
            {
                int customerID = Convert.ToInt32(cmbCustomerSelect.SelectedValue ?? 1);
                string invoiceNo = "INV-" + DateTime.Now.ToString("yyyyMMddhhmmss");

                double subtotal = 0;
                foreach (DataRow row in dtCart.Rows)
                {
                    subtotal += Convert.ToDouble(row["الإجمالي"]);
                }

                double discPercent = 0;
                double.TryParse(txtDiscountPercent.Text, out discPercent);
                double discount = subtotal * (discPercent / 100.0);
                double tax = (subtotal - discount) * 0.14;
                double total = (subtotal - discount) + tax;

                string sqlSale = @"
                    INSERT INTO Sales (InvoiceNo, CustomerID, SaleDate, Discount, Tax, Total)
                    VALUES (@inv, @cust, @date, @disc, @tax, @total);
                    SELECT last_insert_rowid();";

                SQLiteParameter[] paramSale = new SQLiteParameter[]
                {
                    new SQLiteParameter("@inv", invoiceNo),
                    new SQLiteParameter("@cust", customerID),
                    new SQLiteParameter("@date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                    new SQLiteParameter("@disc", discount),
                    new SQLiteParameter("@tax", tax),
                    new SQLiteParameter("@total", total)
                };

                object lastId = DatabaseHelper.ExecuteScalar(sqlSale, paramSale);
                int saleID = Convert.ToInt32(lastId);

                // Insert Sale Items & decrease stock
                foreach (DataRow row in dtCart.Rows)
                {
                    int pId = Convert.ToInt32(row["ProductID"]);
                    int qty = Convert.ToInt32(row["الكمية"]);
                    double price = Convert.ToDouble(row["السعر"]);
                    double rowTotal = Convert.ToDouble(row["الإجمالي"]);

                    string sqlItem = "INSERT INTO SaleItems (SaleID, ProductID, Qty, Price, Total) VALUES (@sId, @pId, @qty, @prc, @tot);";
                    DatabaseHelper.ExecuteNonQuery(sqlItem, new SQLiteParameter[]
                    {
                        new SQLiteParameter("@sId", saleID),
                        new SQLiteParameter("@pId", pId),
                        new SQLiteParameter("@qty", qty),
                        new SQLiteParameter("@prc", price),
                        new SQLiteParameter("@tot", rowTotal)
                    });

                    // Decrement Stock
                    DatabaseHelper.ExecuteNonQuery($"UPDATE Products SET Qty = Qty - {qty} WHERE ProductID = {pId};");
                }

                // Award customer loyalty points (1 point per 50 EGP spent)
                int pointsEarned = (int)(total / 50);
                if (pointsEarned > 0)
                {
                    DatabaseHelper.ExecuteNonQuery($"UPDATE Customers SET LoyaltyPoints = LoyaltyPoints + {pointsEarned} WHERE CustomerID = {customerID};");
                }

                DatabaseHelper.LogActivity("INFO", $"فاتورة بيع مباشر صادرة برقم: {invoiceNo}. القيمة: {total:N2} EGP");

                ShowReceiptDialog(invoiceNo, cmbCustomerSelect.Text, subtotal, discount, tax, total, pointsEarned);

                dtCart.Rows.Clear();
                UpdateTotals();
                LoadProductCards();
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ أثناء إتمام عملية البيع: " + ex.Message);
            }
        }

        private void ShowReceiptDialog(string invoiceNo, string customer, double subtotal, double discount, double tax, double total, int points)
        {
            Form f = new Form();
            f.Size = new Size(420, 680);
            f.Text = "طباعة الفاتورة | Print Thermal Receipt";
            f.StartPosition = FormStartPosition.CenterParent;
            f.FormBorderStyle = FormBorderStyle.FixedDialog;
            f.MaximizeBox = false;
            f.MinimizeBox = false;
            f.BackColor = Color.FromArgb(241, 245, 249);
            if (ThemeManager.AppIcon != null) f.Icon = ThemeManager.AppIcon;

            // Wrapper panel to provide border padding
            Panel pnlWrapper = new Panel();
            pnlWrapper.Dock = DockStyle.Fill;
            pnlWrapper.Padding = new Padding(15);
            pnlWrapper.BackColor = Color.FromArgb(241, 245, 249);
            f.Controls.Add(pnlWrapper);

            // White receipt card
            Guna2Panel pnlReceiptCard = new Guna2Panel();
            pnlReceiptCard.Dock = DockStyle.Fill;
            pnlReceiptCard.FillColor = Color.White;
            pnlReceiptCard.BorderRadius = 16;
            pnlReceiptCard.Padding = new Padding(15);
            pnlReceiptCard.ShadowDecoration.Enabled = true;
            pnlReceiptCard.ShadowDecoration.Color = Color.FromArgb(15, 23, 42);
            pnlReceiptCard.ShadowDecoration.Depth = 10;
            pnlReceiptCard.ShadowDecoration.BorderRadius = 16;
            pnlWrapper.Controls.Add(pnlReceiptCard);

            // FlowLayout for receipt contents
            FlowLayoutPanel flpReceipt = new FlowLayoutPanel();
            flpReceipt.Dock = DockStyle.Fill;
            flpReceipt.FlowDirection = FlowDirection.TopDown;
            flpReceipt.WrapContents = false;
            flpReceipt.AutoScroll = true;
            flpReceipt.BackColor = Color.White;
            flpReceipt.Padding = new Padding(5, 5, 15, 5); // Right padding to clear scrollbar
            pnlReceiptCard.Controls.Add(flpReceipt);

            int contentWidth = 330; // 360 card width - 30 padding

            // Header Section
            Label lblCompany = new Label();
            lblCompany.Text = "MASA ERP PRO";
            lblCompany.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblCompany.ForeColor = Color.FromArgb(15, 23, 42);
            lblCompany.Size = new Size(contentWidth, 30);
            lblCompany.TextAlign = ContentAlignment.MiddleCenter;
            lblCompany.Margin = new Padding(0, 5, 0, 2);
            flpReceipt.Controls.Add(lblCompany);

            Label lblSubtitle = new Label();
            lblSubtitle.Text = "إيصال بيع مباشر - تذكرة كاشير";
            lblSubtitle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
            lblSubtitle.Size = new Size(contentWidth, 20);
            lblSubtitle.TextAlign = ContentAlignment.MiddleCenter;
            lblSubtitle.Margin = new Padding(0, 0, 0, 10);
            flpReceipt.Controls.Add(lblSubtitle);

            // Helper for separators
            Func<Guna2Separator> createSeparator = () =>
            {
                Guna2Separator sep = new Guna2Separator();
                sep.Width = contentWidth;
                sep.Height = 15;
                sep.FillColor = Color.FromArgb(226, 232, 240);
                sep.Margin = new Padding(0, 5, 0, 5);
                return sep;
            };

            flpReceipt.Controls.Add(createSeparator());

            // Local helper function to add meta rows
            void AddMetaRow(TableLayoutPanel tlp, int row, string key, string value)
            {
                Label lblKey = new Label
                {
                    Text = key,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(100, 116, 139), // slate-500
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleRight
                };

                Label lblVal = new Label
                {
                    Text = value,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(15, 23, 42), // slate-900
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                tlp.Controls.Add(lblKey, 0, row);
                tlp.Controls.Add(lblVal, 1, row);
            }

            // Local helper function to add summary rows
            void AddSummaryRow(TableLayoutPanel tlp, int row, string key, string value, bool isGrandTotal, bool isDiscount)
            {
                Label lblKey = new Label
                {
                    Text = key,
                    Font = new Font("Segoe UI", isGrandTotal ? 11F : 9.5F, isGrandTotal ? FontStyle.Bold : FontStyle.Regular),
                    ForeColor = isGrandTotal ? Color.FromArgb(15, 23, 42) : Color.FromArgb(71, 85, 105),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleRight
                };

                Color valColor = Color.FromArgb(15, 23, 42);
                if (isGrandTotal) valColor = Color.FromArgb(16, 185, 129); // Emerald Green
                else if (isDiscount) valColor = Color.FromArgb(239, 68, 68); // Red for discount

                Label lblVal = new Label
                {
                    Text = value,
                    Font = new Font("Segoe UI", isGrandTotal ? 11F : 9.5F, FontStyle.Bold),
                    ForeColor = valColor,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                tlp.Controls.Add(lblKey, 0, row);
                tlp.Controls.Add(lblVal, 1, row);
            }

            // Meta Info Section (TableLayoutPanel RTL)
            TableLayoutPanel tlpMeta = new TableLayoutPanel();
            tlpMeta.Width = contentWidth;
            tlpMeta.Height = 85;
            tlpMeta.ColumnCount = 2;
            tlpMeta.RowCount = 3;
            tlpMeta.RightToLeft = RightToLeft.Yes;
            tlpMeta.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tlpMeta.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tlpMeta.Margin = new Padding(0, 5, 0, 5);

            AddMetaRow(tlpMeta, 0, "رقم الفاتورة:", invoiceNo);
            AddMetaRow(tlpMeta, 1, "التاريخ:", DateTime.Now.ToString("yyyy-MM-dd hh:mm tt"));
            AddMetaRow(tlpMeta, 2, "العميل:", customer);
            flpReceipt.Controls.Add(tlpMeta);

            flpReceipt.Controls.Add(createSeparator());

            // Items List Section Headers
            TableLayoutPanel tlpItemsHeader = new TableLayoutPanel();
            tlpItemsHeader.Width = contentWidth;
            tlpItemsHeader.Height = 25;
            tlpItemsHeader.ColumnCount = 3;
            tlpItemsHeader.RightToLeft = RightToLeft.Yes;
            tlpItemsHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F)); // Product Name
            tlpItemsHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F)); // Qty
            tlpItemsHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F)); // Total Price
            tlpItemsHeader.Margin = new Padding(0, 2, 0, 2);

            Label lblHName = new Label { Text = "المنتج", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight };
            Label lblHQty = new Label { Text = "الكمية", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
            Label lblHTotal = new Label { Text = "الإجمالي", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.FromArgb(71, 85, 105), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };

            tlpItemsHeader.Controls.Add(lblHName, 0, 0);
            tlpItemsHeader.Controls.Add(lblHQty, 1, 0);
            tlpItemsHeader.Controls.Add(lblHTotal, 2, 0);
            flpReceipt.Controls.Add(tlpItemsHeader);

            // Add each item row
            foreach (DataRow row in dtCart.Rows)
            {
                TableLayoutPanel tlpItem = new TableLayoutPanel();
                tlpItem.Width = contentWidth;
                tlpItem.Height = 32;
                tlpItem.ColumnCount = 3;
                tlpItem.RightToLeft = RightToLeft.Yes;
                tlpItem.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
                tlpItem.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15F));
                tlpItem.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
                tlpItem.Margin = new Padding(0, 1, 0, 1);

                string prodName = row["المنتج"].ToString();
                double priceVal = Convert.ToDouble(row["السعر"]);
                int qtyVal = Convert.ToInt32(row["الكمية"]);
                if (qtyVal > 1)
                {
                    prodName += $" ({priceVal:N2})";
                }

                Label lblName = new Label
                {
                    Text = prodName,
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                    ForeColor = Color.FromArgb(15, 23, 42),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleRight
                };

                Label lblQty = new Label
                {
                    Text = $"x{qtyVal}",
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(100, 116, 139),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };

                Label lblTotalVal = new Label
                {
                    Text = $"{Convert.ToDouble(row["الإجمالي"]):N2} EGP",
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(15, 23, 42),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                tlpItem.Controls.Add(lblName, 0, 0);
                tlpItem.Controls.Add(lblQty, 1, 0);
                tlpItem.Controls.Add(lblTotalVal, 2, 0);
                flpReceipt.Controls.Add(tlpItem);
            }

            flpReceipt.Controls.Add(createSeparator());

            // Summary Section
            TableLayoutPanel tlpSummary = new TableLayoutPanel();
            tlpSummary.Width = contentWidth;
            tlpSummary.Height = 110;
            tlpSummary.ColumnCount = 2;
            tlpSummary.RowCount = 4;
            tlpSummary.RightToLeft = RightToLeft.Yes;
            tlpSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpSummary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpSummary.Margin = new Padding(0, 5, 0, 5);

            AddSummaryRow(tlpSummary, 0, "الإجمالي الفرعي:", $"{subtotal:N2} EGP", false, false);
            AddSummaryRow(tlpSummary, 1, "الخصم:", $"-{discount:N2} EGP", false, discount > 0);
            AddSummaryRow(tlpSummary, 2, "ضريبة المضافة (14%):", $"{tax:N2} EGP", false, false);
            AddSummaryRow(tlpSummary, 3, "المجموع النهائي:", $"{total:N2} EGP", true, false);
            flpReceipt.Controls.Add(tlpSummary);

            flpReceipt.Controls.Add(createSeparator());


            // Footer Text
            Label lblFooter = new Label();
            lblFooter.Text = "شكراً لتسوقكم معنا في MASA POS Pro!\nيسعدنا دائماً خدمتكم.";
            lblFooter.Font = new Font("Segoe UI", 8.5F, FontStyle.Italic);
            lblFooter.ForeColor = Color.FromArgb(148, 163, 184); // slate-400
            lblFooter.Size = new Size(contentWidth, 36);
            lblFooter.TextAlign = ContentAlignment.MiddleCenter;
            lblFooter.Margin = new Padding(0, 5, 0, 15);
            flpReceipt.Controls.Add(lblFooter);

            // Bottom action panel (fixed outside wrapper to stay docked)
            Panel pnlActions = new Panel();
            pnlActions.Height = 65;
            pnlActions.Dock = DockStyle.Bottom;
            pnlActions.Padding = new Padding(15, 10, 15, 15);
            pnlActions.BackColor = Color.FromArgb(241, 245, 249);
            f.Controls.Add(pnlActions);

            Guna2Button btnPrint = new Guna2Button();
            btnPrint.Text = "طباعة التذكرة الآن | Print Receipt";
            btnPrint.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            btnPrint.FillColor = Color.FromArgb(16, 185, 129); // Emerald-500
            btnPrint.ForeColor = Color.White;
            btnPrint.BorderRadius = 8;
            btnPrint.Dock = DockStyle.Fill;
            
            // Try to load printer icon
            Image printerImg = FrmMain.LoadIcon("printer.png");
            if (printerImg != null)
            {
                btnPrint.Image = printerImg;
                btnPrint.ImageSize = new Size(16, 16);
                btnPrint.ImageAlign = HorizontalAlignment.Left;
                btnPrint.ImageOffset = new Point(10, 0);
            }

            btnPrint.Click += (s, e) =>
            {
                MessageBox.Show("تم إرسال الفاتورة للطابعة الحرارية بنجاح!", "نجاح الطباعة", MessageBoxButtons.OK, MessageBoxIcon.Information);
                f.Close();
            };
            pnlActions.Controls.Add(btnPrint);

            f.ShowDialog();
        }
    }

    // ==========================================
    // 2. Sales Journal (FrmSales)
    // ==========================================
    public class FrmSales : Form, ISearchable
    {
        private Guna2DataGridView dgvSales;
        private Guna2Button btnViewInvoice;
        private Guna2TextBox txtSearch;

        public FrmSales()
        {
            InitializeComponent();
            LoadSalesJournal();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.DoubleBuffered = true;

            TableLayoutPanel tlp = new TableLayoutPanel();
            tlp.Dock = DockStyle.Fill;
            tlp.ColumnCount = 1;
            tlp.Padding = new Padding(15);
            this.Controls.Add(tlp);

            // Invoice Grid Card
            Guna2Panel pnlGridCard = new Guna2Panel();
            pnlGridCard.Dock = DockStyle.Fill;
            pnlGridCard.BorderRadius = 12;
            pnlGridCard.FillColor = Color.White;
            pnlGridCard.CustomBorderColor = Color.FromArgb(226, 232, 240);
            pnlGridCard.CustomBorderThickness = new Padding(1);
            pnlGridCard.Padding = new Padding(15);
            tlp.Controls.Add(pnlGridCard, 0, 0);

            // Flow Grid Actions Header
            FlowLayoutPanel flpHeader = new FlowLayoutPanel();
            flpHeader.Dock = DockStyle.Top;
            flpHeader.Height = 55;
            flpHeader.FlowDirection = FlowDirection.LeftToRight;
            flpHeader.BackColor = Color.White;
            pnlGridCard.Controls.Add(flpHeader);

            Label lblTitle = new Label();
            lblTitle.Text = "سجل فواتير المبيعات";
            lblTitle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.AutoSize = true;
            lblTitle.Margin = new Padding(5, 12, 20, 0);
            flpHeader.Controls.Add(lblTitle);

            btnViewInvoice = new Guna2Button();
            btnViewInvoice.Text = "تفاصيل الفاتورة";
            btnViewInvoice.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnViewInvoice.FillColor = Color.FromArgb(59, 130, 246);
            btnViewInvoice.ForeColor = Color.White;
            btnViewInvoice.BorderRadius = 6;
            btnViewInvoice.Size = new Size(180, 36);
            btnViewInvoice.Margin = new Padding(0, 5, 0, 0);
            btnViewInvoice.Click += BtnViewInvoice_Click;
            flpHeader.Controls.Add(btnViewInvoice);

            txtSearch = new Guna2TextBox();
            txtSearch.PlaceholderText = "ابحث عن فاتورة بالرقم أو اسم العميل...";
            txtSearch.Size = new Size(280, 36);
            txtSearch.Margin = new Padding(20, 5, 0, 0);
            txtSearch.BorderRadius = 8;
            txtSearch.BorderColor = Color.FromArgb(203, 213, 225);
            txtSearch.IconRight = FrmMain.LoadIcon("search.png");
            txtSearch.IconRightSize = new Size(16, 16);
            txtSearch.IconRightOffset = new Point(6, 0);
            txtSearch.TextChanged += TxtSearch_TextChanged;
            flpHeader.Controls.Add(txtSearch);

            dgvSales = new Guna2DataGridView();
            dgvSales.Dock = DockStyle.Fill;
            dgvSales.ReadOnly = true;
            dgvSales.AllowUserToAddRows = false;
            dgvSales.AllowUserToDeleteRows = false;
            dgvSales.BorderStyle = BorderStyle.None;
            dgvSales.Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Default;
            dgvSales.ThemeStyle.HeaderStyle.BackColor = Color.FromArgb(241, 245, 249);
            dgvSales.ThemeStyle.HeaderStyle.ForeColor = Color.FromArgb(71, 85, 105);
            dgvSales.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            pnlGridCard.Controls.Add(dgvSales);
            dgvSales.BringToFront();
        }

        private void LoadSalesJournal(string searchQuery = "")
        {
            try
            {
                string sql = @"
                    SELECT s.SaleID as 'ID', s.InvoiceNo as 'رقم الفاتورة', c.Name as 'العميل', 
                           s.SaleDate as 'تاريخ البيع', s.Discount as 'الخصم', s.Tax as 'الضريبة', s.Total as 'المجموع النهائي'
                    FROM Sales s
                    LEFT JOIN Customers c ON s.CustomerID = c.CustomerID";
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    sql += " WHERE s.InvoiceNo LIKE @search OR c.Name LIKE @search";
                    var param = new SQLiteParameter("@search", $"%{searchQuery}%");
                    dgvSales.DataSource = DatabaseHelper.ExecuteQuery(sql + " ORDER BY s.SaleID DESC;", new SQLiteParameter[] { param });
                }
                else
                {
                    dgvSales.DataSource = DatabaseHelper.ExecuteQuery(sql + " ORDER BY s.SaleID DESC;");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadSalesJournal(txtSearch.Text);
        }

        public void PerformSearch(string query)
        {
            txtSearch.Text = query;
        }

        private void BtnViewInvoice_Click(object sender, EventArgs e)
        {
            if (dgvSales.SelectedRows.Count == 0)
            {
                MessageBox.Show("يرجى اختيار فاتورة من الجدول أولاً!");
                return;
            }

            try
            {
                int saleId = Convert.ToInt32(dgvSales.SelectedRows[0].Cells["ID"].Value);
                string invoiceNo = dgvSales.SelectedRows[0].Cells["رقم الفاتورة"].Value?.ToString() ?? "";
                string customer = dgvSales.SelectedRows[0].Cells["العميل"].Value?.ToString() ?? "";
                string date = dgvSales.SelectedRows[0].Cells["تاريخ البيع"].Value?.ToString() ?? "";
                string total = dgvSales.SelectedRows[0].Cells["المجموع النهائي"].Value?.ToString() ?? "";

                // Fetch sale items
                string sqlItems = @"
                    SELECT p.Name as 'المنتج', p.Barcode as 'الباركود', i.Qty as 'الكمية', i.Price as 'سعر القطعة', i.Total as 'الإجمالي'
                    FROM SaleItems i
                    LEFT JOIN Products p ON i.ProductID = p.ProductID
                    WHERE i.SaleID = @sId;";
                DataTable dtItems = DatabaseHelper.ExecuteQuery(sqlItems, new SQLiteParameter[] { new SQLiteParameter("@sId", saleId) });

                // Dialog detail view
                Form f = new Form();
                f.Size = new Size(500, 480);
                f.Text = $"تفاصيل فاتورة مبيعات: {invoiceNo}";
                f.StartPosition = FormStartPosition.CenterParent;
                if (ThemeManager.AppIcon != null) f.Icon = ThemeManager.AppIcon;

                Guna2DataGridView dgvDetails = new Guna2DataGridView();
                dgvDetails.Dock = DockStyle.Fill;
                dgvDetails.ReadOnly = true;
                dgvDetails.AllowUserToAddRows = false;
                dgvDetails.AllowUserToDeleteRows = false;
                dgvDetails.DataSource = dtItems;

                Panel pnlInfo = new Panel();
                pnlInfo.Height = 85;
                pnlInfo.Dock = DockStyle.Top;
                pnlInfo.BackColor = Color.FromArgb(241, 245, 249);
                f.Controls.Add(pnlInfo);

                Label lblHeader = new Label();
                lblHeader.Text = $"رقم الفاتورة: {invoiceNo} | العميل: {customer}\nالتاريخ: {date} | المجموع: {total} EGP";
                lblHeader.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
                lblHeader.ForeColor = Color.FromArgb(15, 23, 42);
                lblHeader.Dock = DockStyle.Fill;
                lblHeader.TextAlign = ContentAlignment.MiddleCenter;
                pnlInfo.Controls.Add(lblHeader);

                f.Controls.Add(dgvDetails);
                dgvDetails.BringToFront();

                f.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }

    // ==========================================
    // 3. Customers Form (FrmCustomers)
    // ==========================================
    public class FrmCustomers : Form, ISearchable
    {
        private Guna2TextBox txtName, txtPhone, txtEmail, txtAddress, txtLoyalty, txtSearch;
        private Guna2DataGridView dgvCust;
        private Guna2Button btnAdd, btnEdit, btnDelete, btnClear;
        private int selectedCustomerID = -1;

        public FrmCustomers()
        {
            InitializeComponent();
            LoadCustomers();
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

            // Inputs
            AddLabel("اسم العميل", flp);
            txtName = CreateTextBox("الاسم الكامل للعميل", flp);

            AddLabel("رقم الهاتف", flp);
            txtPhone = CreateTextBox("01xxxxxxxxx", flp);

            AddLabel("البريد الإلكتروني", flp);
            txtEmail = CreateTextBox("customer@email.com", flp);

            AddLabel("العنوان السكني", flp);
            txtAddress = CreateTextBox("عنوان العميل", flp);

            AddLabel("نقاط الولاء", flp);
            txtLoyalty = CreateTextBox("0", flp);

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
            txtSearch.PlaceholderText = "ابحث عن عميل بالاسم أو رقم الهاتف...";
            txtSearch.Size = new Size(350, 36);
            txtSearch.Location = new Point(0, 5);
            txtSearch.BorderRadius = 8;
            txtSearch.BorderColor = Color.FromArgb(203, 213, 225);
            txtSearch.IconRight = FrmMain.LoadIcon("search.png");
            txtSearch.IconRightSize = new Size(16, 16);
            txtSearch.IconRightOffset = new Point(6, 0);
            txtSearch.TextChanged += TxtSearch_TextChanged;
            pnlSearch.Controls.Add(txtSearch);

            dgvCust = new Guna2DataGridView();
            dgvCust.Dock = DockStyle.Fill;
            dgvCust.ReadOnly = true;
            dgvCust.AllowUserToAddRows = false;
            dgvCust.AllowUserToDeleteRows = false;
            dgvCust.BorderStyle = BorderStyle.None;
            dgvCust.Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Default;
            dgvCust.ThemeStyle.HeaderStyle.BackColor = Color.FromArgb(241, 245, 249);
            dgvCust.ThemeStyle.HeaderStyle.ForeColor = Color.FromArgb(71, 85, 105);
            dgvCust.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvCust.SelectionChanged += DgvCust_SelectionChanged;
            pnlRight.Controls.Add(dgvCust);
            dgvCust.BringToFront();
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

        private void LoadCustomers(string searchQuery = "")
        {
            try
            {
                string sql = "SELECT CustomerID as 'ID', Name as 'الاسم', Phone as 'الهاتف', Email as 'البريد', Address as 'العنوان', LoyaltyPoints as 'نقاط الولاء' FROM Customers";
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    sql += " WHERE Name LIKE @search OR Phone LIKE @search;";
                    var param = new SQLiteParameter("@search", $"%{searchQuery}%");
                    dgvCust.DataSource = DatabaseHelper.ExecuteQuery(sql, new SQLiteParameter[] { param });
                }
                else
                {
                    sql += ";";
                    dgvCust.DataSource = DatabaseHelper.ExecuteQuery(sql);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DgvCust_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCust.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvCust.SelectedRows[0];
                selectedCustomerID = Convert.ToInt32(row.Cells["ID"].Value);
                txtName.Text = row.Cells["الاسم"].Value?.ToString() ?? "";
                txtPhone.Text = row.Cells["الهاتف"].Value?.ToString() ?? "";
                txtEmail.Text = row.Cells["البريد"].Value?.ToString() ?? "";
                txtAddress.Text = row.Cells["العنوان"].Value?.ToString() ?? "";
                txtLoyalty.Text = row.Cells["نقاط الولاء"].Value?.ToString() ?? "";
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text)) return;

            try
            {
                string sql = "INSERT INTO Customers (Name, Phone, Email, Address, LoyaltyPoints) VALUES (@name, @phone, @email, @address, @points);";
                var parameters = new SQLiteParameter[]
                {
                    new SQLiteParameter("@name", txtName.Text),
                    new SQLiteParameter("@phone", txtPhone.Text),
                    new SQLiteParameter("@email", txtEmail.Text),
                    new SQLiteParameter("@address", txtAddress.Text),
                    new SQLiteParameter("@points", Convert.ToInt32(string.IsNullOrEmpty(txtLoyalty.Text) ? "0" : txtLoyalty.Text))
                };
                DatabaseHelper.ExecuteNonQuery(sql, parameters);
                DatabaseHelper.LogActivity("INFO", $"تمت إضافة عميل جديد: {txtName.Text}");
                LoadCustomers();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ أثناء الإضافة: " + ex.Message);
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (selectedCustomerID == -1) return;

            try
            {
                string sql = "UPDATE Customers SET Name=@name, Phone=@phone, Email=@email, Address=@address, LoyaltyPoints=@points WHERE CustomerID=@id;";
                var parameters = new SQLiteParameter[]
                {
                    new SQLiteParameter("@name", txtName.Text),
                    new SQLiteParameter("@phone", txtPhone.Text),
                    new SQLiteParameter("@email", txtEmail.Text),
                    new SQLiteParameter("@address", txtAddress.Text),
                    new SQLiteParameter("@points", Convert.ToInt32(string.IsNullOrEmpty(txtLoyalty.Text) ? "0" : txtLoyalty.Text)),
                    new SQLiteParameter("@id", selectedCustomerID)
                };
                DatabaseHelper.ExecuteNonQuery(sql, parameters);
                DatabaseHelper.LogActivity("INFO", $"تم تعديل العميل: {txtName.Text}");
                LoadCustomers();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ أثناء التعديل: " + ex.Message);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedCustomerID == -1) return;

            if (MessageBox.Show("هل أنت متأكد من حذف هذا العميل؟", "تأكيد الحذف", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    string sql = "DELETE FROM Customers WHERE CustomerID=@id;";
                    DatabaseHelper.ExecuteNonQuery(sql, new SQLiteParameter[] { new SQLiteParameter("@id", selectedCustomerID) });
                    DatabaseHelper.LogActivity("WARNING", $"تم حذف العميل بالمعرف: {selectedCustomerID}");
                    LoadCustomers();
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
            selectedCustomerID = -1;
            txtName.Clear();
            txtPhone.Clear();
            txtEmail.Clear();
            txtAddress.Clear();
            txtLoyalty.Clear();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadCustomers(txtSearch.Text);
        }

        public void PerformSearch(string query)
        {
            txtSearch.Text = query;
        }
    }

    // ==========================================
    // 4. Suppliers Form (FrmSuppliers)
    // ==========================================
    public class FrmSuppliers : Form, ISearchable
    {
        private Guna2TextBox txtName, txtPhone, txtCompany, txtAddress, txtNotes, txtSearch;
        private Guna2DataGridView dgvSupp;
        private Guna2Button btnAdd, btnEdit, btnDelete, btnClear;
        private int selectedSupplierID = -1;

        public FrmSuppliers()
        {
            InitializeComponent();
            LoadSuppliers();
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

            // Inputs
            AddLabel("اسم المورد", flp);
            txtName = CreateTextBox("الاسم الكامل للمورد", flp);

            AddLabel("رقم الهاتف", flp);
            txtPhone = CreateTextBox("01xxxxxxxxx", flp);

            AddLabel("الشركة", flp);
            txtCompany = CreateTextBox("الشركة أو المؤسسة التابع لها", flp);

            AddLabel("العنوان", flp);
            txtAddress = CreateTextBox("عنوان المورد", flp);

            AddLabel("ملاحظات", flp);
            txtNotes = CreateTextBox("ملاحظات وشروط التوريد", flp);

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
            txtSearch.PlaceholderText = "ابحث عن مورد بالاسم، الشركة أو الهاتف...";
            txtSearch.Size = new Size(350, 36);
            txtSearch.Location = new Point(0, 5);
            txtSearch.BorderRadius = 8;
            txtSearch.BorderColor = Color.FromArgb(203, 213, 225);
            txtSearch.IconRight = FrmMain.LoadIcon("search.png");
            txtSearch.IconRightSize = new Size(16, 16);
            txtSearch.IconRightOffset = new Point(6, 0);
            txtSearch.TextChanged += TxtSearch_TextChanged;
            pnlSearch.Controls.Add(txtSearch);

            dgvSupp = new Guna2DataGridView();
            dgvSupp.Dock = DockStyle.Fill;
            dgvSupp.ReadOnly = true;
            dgvSupp.AllowUserToAddRows = false;
            dgvSupp.AllowUserToDeleteRows = false;
            dgvSupp.BorderStyle = BorderStyle.None;
            dgvSupp.Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Default;
            dgvSupp.ThemeStyle.HeaderStyle.BackColor = Color.FromArgb(241, 245, 249);
            dgvSupp.ThemeStyle.HeaderStyle.ForeColor = Color.FromArgb(71, 85, 105);
            dgvSupp.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvSupp.SelectionChanged += DgvSupp_SelectionChanged;
            pnlRight.Controls.Add(dgvSupp);
            dgvSupp.BringToFront();
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

        private void LoadSuppliers(string searchQuery = "")
        {
            try
            {
                string sql = "SELECT SupplierID as 'ID', Name as 'الاسم', Phone as 'الهاتف', Company as 'الشركة', Address as 'العنوان', Notes as 'ملاحظات' FROM Suppliers";
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    sql += " WHERE Name LIKE @search OR Phone LIKE @search OR Company LIKE @search;";
                    var param = new SQLiteParameter("@search", $"%{searchQuery}%");
                    dgvSupp.DataSource = DatabaseHelper.ExecuteQuery(sql, new SQLiteParameter[] { param });
                }
                else
                {
                    sql += ";";
                    dgvSupp.DataSource = DatabaseHelper.ExecuteQuery(sql);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DgvSupp_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvSupp.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvSupp.SelectedRows[0];
                selectedSupplierID = Convert.ToInt32(row.Cells["ID"].Value);
                txtName.Text = row.Cells["الاسم"].Value?.ToString() ?? "";
                txtPhone.Text = row.Cells["الهاتف"].Value?.ToString() ?? "";
                txtCompany.Text = row.Cells["الشركة"].Value?.ToString() ?? "";
                txtAddress.Text = row.Cells["العنوان"].Value?.ToString() ?? "";
                txtNotes.Text = row.Cells["ملاحظات"].Value?.ToString() ?? "";
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text)) return;

            try
            {
                string sql = "INSERT INTO Suppliers (Name, Phone, Company, Address, Notes) VALUES (@name, @phone, @company, @address, @notes);";
                var parameters = new SQLiteParameter[]
                {
                    new SQLiteParameter("@name", txtName.Text),
                    new SQLiteParameter("@phone", txtPhone.Text),
                    new SQLiteParameter("@company", txtCompany.Text),
                    new SQLiteParameter("@address", txtAddress.Text),
                    new SQLiteParameter("@notes", txtNotes.Text)
                };
                DatabaseHelper.ExecuteNonQuery(sql, parameters);
                DatabaseHelper.LogActivity("INFO", $"تمت إضافة مورد جديد: {txtName.Text}");
                LoadSuppliers();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ أثناء الإضافة: " + ex.Message);
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (selectedSupplierID == -1) return;

            try
            {
                string sql = "UPDATE Suppliers SET Name=@name, Phone=@phone, Company=@company, Address=@address, Notes=@notes WHERE SupplierID=@id;";
                var parameters = new SQLiteParameter[]
                {
                    new SQLiteParameter("@name", txtName.Text),
                    new SQLiteParameter("@phone", txtPhone.Text),
                    new SQLiteParameter("@company", txtCompany.Text),
                    new SQLiteParameter("@address", txtAddress.Text),
                    new SQLiteParameter("@notes", txtNotes.Text),
                    new SQLiteParameter("@id", selectedSupplierID)
                };
                DatabaseHelper.ExecuteNonQuery(sql, parameters);
                DatabaseHelper.LogActivity("INFO", $"تم تعديل بيانات المورد: {txtName.Text}");
                LoadSuppliers();
                ClearInputs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("حدث خطأ أثناء التعديل: " + ex.Message);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedSupplierID == -1) return;

            if (MessageBox.Show("هل أنت متأكد من حذف هذا المورد؟", "تأكيد الحذف", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    string sql = "DELETE FROM Suppliers WHERE SupplierID=@id;";
                    DatabaseHelper.ExecuteNonQuery(sql, new SQLiteParameter[] { new SQLiteParameter("@id", selectedSupplierID) });
                    DatabaseHelper.LogActivity("WARNING", $"تم حذف المورد بالمعرف: {selectedSupplierID}");
                    LoadSuppliers();
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
            selectedSupplierID = -1;
            txtName.Clear();
            txtPhone.Clear();
            txtCompany.Clear();
            txtAddress.Clear();
            txtNotes.Clear();
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadSuppliers(txtSearch.Text);
        }

        public void PerformSearch(string query)
        {
            txtSearch.Text = query;
        }
    }

    // ==========================================
    // 5. Purchases Management (FrmPurchases)
    // ==========================================
    public class FrmPurchases : Form, ISearchable
    {
        private Guna2ComboBox cmbSupplier, cmbProduct;
        private Guna2TextBox txtQty, txtPrice, txtSearch;
        private Guna2DateTimePicker dtDate;
        private Guna2DataGridView dgvPurchases;
        private Guna2Button btnAdd, btnDelete;
        private int selectedPurchaseID = -1;

        public FrmPurchases()
        {
            InitializeComponent();
            LoadSuppliers();
            LoadProducts();
            LoadPurchases();
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

            // Left Inputs Card
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

            // Inputs
            AddLabel("المورد", flp);
            cmbSupplier = new Guna2ComboBox();
            cmbSupplier.Width = 290;
            cmbSupplier.Height = 36;
            cmbSupplier.BorderRadius = 8;
            cmbSupplier.BorderColor = Color.FromArgb(203, 213, 225);
            cmbSupplier.Font = new Font("Segoe UI", 9.5F);
            cmbSupplier.Margin = new Padding(0, 0, 0, 8);
            flp.Controls.Add(cmbSupplier);

            AddLabel("المنتج", flp);
            cmbProduct = new Guna2ComboBox();
            cmbProduct.Width = 290;
            cmbProduct.Height = 36;
            cmbProduct.BorderRadius = 8;
            cmbProduct.BorderColor = Color.FromArgb(203, 213, 225);
            cmbProduct.Font = new Font("Segoe UI", 9.5F);
            cmbProduct.SelectedIndexChanged += CmbProduct_SelectedIndexChanged;
            cmbProduct.Margin = new Padding(0, 0, 0, 8);
            flp.Controls.Add(cmbProduct);

            AddLabel("الكمية المشتراة", flp);
            txtQty = CreateTextBox("الكمية المضافة للمخزن", flp);

            AddLabel("سعر الشراء الفردي", flp);
            txtPrice = CreateTextBox("0.00 EGP", flp);

            AddLabel("تاريخ الشراء", flp);
            dtDate = new Guna2DateTimePicker();
            dtDate.Width = 290;
            dtDate.Height = 36;
            dtDate.BorderRadius = 8;
            dtDate.FillColor = Color.FromArgb(59, 130, 246);
            dtDate.ForeColor = Color.White;
            dtDate.Font = new Font("Segoe UI", 9.5F);
            dtDate.Format = DateTimePickerFormat.Short;
            dtDate.Margin = new Padding(0, 0, 0, 15);
            flp.Controls.Add(dtDate);

            // Buttons
            FlowLayoutPanel flpButtons = new FlowLayoutPanel();
            flpButtons.Width = 290;
            flpButtons.Height = 45;
            flpButtons.FlowDirection = FlowDirection.LeftToRight;
            flpButtons.BackColor = Color.Transparent;

            btnAdd = CreateActionButton("تسجيل شراء", Color.FromArgb(16, 185, 129), 130, 36, flpButtons);
            btnDelete = CreateActionButton("حذف", Color.FromArgb(239, 68, 68), 130, 36, flpButtons);

            flp.Controls.Add(flpButtons);

            btnAdd.Click += BtnAdd_Click;
            btnDelete.Click += BtnDelete_Click;

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
            txtSearch.PlaceholderText = "ابحث عن مشتريات باسم المورد أو المنتج...";
            txtSearch.Size = new Size(350, 36);
            txtSearch.Location = new Point(0, 5);
            txtSearch.BorderRadius = 8;
            txtSearch.BorderColor = Color.FromArgb(203, 213, 225);
            txtSearch.IconRight = FrmMain.LoadIcon("search.png");
            txtSearch.IconRightSize = new Size(16, 16);
            txtSearch.IconRightOffset = new Point(6, 0);
            txtSearch.TextChanged += TxtSearch_TextChanged;
            pnlSearch.Controls.Add(txtSearch);

            dgvPurchases = new Guna2DataGridView();
            dgvPurchases.Dock = DockStyle.Fill;
            dgvPurchases.ReadOnly = true;
            dgvPurchases.AllowUserToAddRows = false;
            dgvPurchases.AllowUserToDeleteRows = false;
            dgvPurchases.BorderStyle = BorderStyle.None;
            dgvPurchases.Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Default;
            dgvPurchases.ThemeStyle.HeaderStyle.BackColor = Color.FromArgb(241, 245, 249);
            dgvPurchases.ThemeStyle.HeaderStyle.ForeColor = Color.FromArgb(71, 85, 105);
            dgvPurchases.ThemeStyle.HeaderStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            dgvPurchases.SelectionChanged += DgvPurchases_SelectionChanged;
            pnlRight.Controls.Add(dgvPurchases);
            dgvPurchases.BringToFront();
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
            if (text == "إضافة" || text == "إضافة جديد" || text == "تسجيل شراء") iconFile = "plus.png";
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

        private void LoadSuppliers()
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery("SELECT SupplierID, Name FROM Suppliers;");
                cmbSupplier.DataSource = dt;
                cmbSupplier.DisplayMember = "Name";
                cmbSupplier.ValueMember = "SupplierID";
            }
            catch { }
        }

        private void LoadProducts()
        {
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery("SELECT ProductID, Name FROM Products;");
                cmbProduct.DataSource = dt;
                cmbProduct.DisplayMember = "Name";
                cmbProduct.ValueMember = "ProductID";
            }
            catch { }
        }

        private void CmbProduct_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbProduct.SelectedValue != null && cmbProduct.SelectedValue is int pId)
            {
                object buy = DatabaseHelper.ExecuteScalar($"SELECT BuyPrice FROM Products WHERE ProductID={pId};");
                if (buy != null && buy != DBNull.Value)
                {
                    txtPrice.Text = Convert.ToDouble(buy).ToString("N2");
                }
            }
        }

        private void LoadPurchases(string searchQuery = "")
        {
            try
            {
                string sql = @"
                    SELECT p.PurchaseID as 'ID', s.Name as 'المورد', pr.Name as 'المنتج', 
                           p.Qty as 'الكمية', p.Price as 'سعر الشراء', (p.Qty * p.Price) as 'الإجمالي', p.PurchaseDate as 'التاريخ'
                    FROM Purchases p
                    LEFT JOIN Suppliers s ON p.SupplierID = s.SupplierID
                    LEFT JOIN Products pr ON p.ProductID = pr.ProductID";
                if (!string.IsNullOrEmpty(searchQuery))
                {
                    sql += " WHERE s.Name LIKE @search OR pr.Name LIKE @search";
                    var param = new SQLiteParameter("@search", $"%{searchQuery}%");
                    dgvPurchases.DataSource = DatabaseHelper.ExecuteQuery(sql + " ORDER BY p.PurchaseDate DESC;", new SQLiteParameter[] { param });
                }
                else
                {
                    dgvPurchases.DataSource = DatabaseHelper.ExecuteQuery(sql + " ORDER BY p.PurchaseDate DESC;");
                }
            }
            catch { }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadPurchases(txtSearch.Text);
        }

        public void PerformSearch(string query)
        {
            txtSearch.Text = query;
        }

        private void DgvPurchases_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPurchases.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dgvPurchases.SelectedRows[0];
                selectedPurchaseID = Convert.ToInt32(row.Cells["ID"].Value);
                cmbSupplier.Text = row.Cells["المورد"].Value?.ToString() ?? "";
                cmbProduct.Text = row.Cells["المنتج"].Value?.ToString() ?? "";
                txtQty.Text = row.Cells["الكمية"].Value?.ToString() ?? "";
                txtPrice.Text = row.Cells["سعر الشراء"].Value?.ToString() ?? "";
                
                DateTime pDate;
                if (row.Cells["التاريخ"].Value != null && DateTime.TryParse(row.Cells["التاريخ"].Value.ToString(), out pDate))
                {
                    dtDate.Value = pDate;
                }
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (cmbSupplier.SelectedValue == null || cmbProduct.SelectedValue == null) return;

            int qty;
            double prc;
            if (!int.TryParse(txtQty.Text, out qty) || qty <= 0 || !double.TryParse(txtPrice.Text, out prc) || prc <= 0)
            {
                MessageBox.Show("يرجى إدخال كمية وسعر شراء صحيحين!");
                return;
            }

            try
            {
                int pId = Convert.ToInt32(cmbProduct.SelectedValue);
                
                string sql = "INSERT INTO Purchases (SupplierID, ProductID, Qty, Price, PurchaseDate) VALUES (@supp, @prod, @qty, @prc, @date);";
                var parameters = new SQLiteParameter[]
                {
                    new SQLiteParameter("@supp", cmbSupplier.SelectedValue),
                    new SQLiteParameter("@prod", pId),
                    new SQLiteParameter("@qty", qty),
                    new SQLiteParameter("@prc", prc),
                    new SQLiteParameter("@date", dtDate.Value.ToString("yyyy-MM-dd"))
                };

                DatabaseHelper.ExecuteNonQuery(sql, parameters);
                
                DatabaseHelper.ExecuteNonQuery($"UPDATE Products SET Qty = Qty + {qty} WHERE ProductID = {pId};");

                DatabaseHelper.LogActivity("INFO", $"فاتورة شراء جديدة: تم توريد {qty} قطع من ({cmbProduct.Text})");
                LoadPurchases();
                txtQty.Clear();
                selectedPurchaseID = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedPurchaseID == -1) return;

            try
            {
                string sql = "DELETE FROM Purchases WHERE PurchaseID = @id;";
                DatabaseHelper.ExecuteNonQuery(sql, new SQLiteParameter[] { new SQLiteParameter("@id", selectedPurchaseID) });
                DatabaseHelper.LogActivity("WARNING", $"تم حذف فاتورة شراء بالرقم: {selectedPurchaseID}");
                LoadPurchases();
                selectedPurchaseID = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
