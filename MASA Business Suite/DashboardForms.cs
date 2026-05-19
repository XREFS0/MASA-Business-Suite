using System;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using System.Data.SQLite;

namespace MASA_Business_Suite
{
    public class FrmDashboard : Form
    {
        private Guna2Panel pnlChartCard;
        private Guna2Panel cardProducts;
        private Guna2Panel cardEmployees;
        private Guna2Panel cardProfit;
        private Label lblClock;
        private Panel pnlRecentSalesList;
        private Panel pnlBestSellersList;
        private Panel pnlActivitiesList;

        private float[] chartData = new float[7];
        private string[] chartDays = new string[7];

        public FrmDashboard()
        {
            InitializeComponent();
            this.Load += FrmDashboard_Load;
        }

        private void InitializeComponent()
        {
            this.Text = "لوحة التحكم";
            this.BackColor = Color.FromArgb(248, 250, 252); // slate-50
            this.DoubleBuffered = true;

            // Main fluid container (NO scrollbars on dashboard!)
            Panel pnlMainContainer = new Panel();
            pnlMainContainer.Dock = DockStyle.Fill;
            pnlMainContainer.AutoScroll = false; // Disable local scroll, make it fully responsive
            this.Controls.Add(pnlMainContainer);

            // 1. Top Header Row (نظرة عامة & Real-time Clock Card)
            Panel pnlHeader = new Panel();
            pnlHeader.Height = 55;
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Padding = new Padding(20, 10, 20, 5);
            pnlMainContainer.Controls.Add(pnlHeader);

            Label lblHeaderTitle = new Label();
            lblHeaderTitle.Text = "نظرة عامة";
            lblHeaderTitle.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
            lblHeaderTitle.ForeColor = Color.FromArgb(15, 23, 42); // Navy Slate
            lblHeaderTitle.Dock = DockStyle.Left;
            lblHeaderTitle.Width = 200;
            lblHeaderTitle.TextAlign = ContentAlignment.MiddleLeft;
            pnlHeader.Controls.Add(lblHeaderTitle);

            // Clock container card (Right anchored)
            Guna2Panel pnlClockCard = new Guna2Panel();
            pnlClockCard.Size = new Size(240, 36);
            pnlClockCard.Dock = DockStyle.Right;
            pnlClockCard.BorderRadius = 8;
            pnlClockCard.FillColor = Color.White;
            pnlClockCard.CustomBorderColor = Color.FromArgb(226, 232, 240);
            pnlClockCard.CustomBorderThickness = new Padding(1);
            pnlHeader.Controls.Add(pnlClockCard);

            Label lblCalIcon = new Label();
            lblCalIcon.Text = "📅";
            lblCalIcon.Font = new Font("Segoe UI", 10F);
            lblCalIcon.Location = new Point(10, 8);
            lblCalIcon.Size = new Size(24, 20);
            pnlClockCard.Controls.Add(lblCalIcon);

            lblClock = new Label();
            lblClock.Text = DateTime.Now.ToString("dd MMMM yyyy - hh:mm:ss tt");
            lblClock.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            lblClock.ForeColor = Color.FromArgb(71, 85, 105);
            lblClock.Location = new Point(36, 8);
            lblClock.Size = new Size(195, 20);
            lblClock.TextAlign = ContentAlignment.MiddleLeft;
            pnlClockCard.Controls.Add(lblClock);

            // 2. Row 1: Full-Width Chart Panel (Performance Overview) - Height reduced to fit 768px nicely
            pnlChartCard = new Guna2Panel();
            pnlChartCard.Height = 230;
            pnlChartCard.Dock = DockStyle.Top;
            pnlChartCard.Padding = new Padding(20, 5, 20, 5);
            pnlMainContainer.Controls.Add(pnlChartCard);
            pnlChartCard.BringToFront();

            Guna2Panel pnlChartInner = new Guna2Panel();
            pnlChartInner.Dock = DockStyle.Fill;
            pnlChartInner.BorderRadius = 16;
            pnlChartInner.FillColor = Color.White;
            pnlChartInner.CustomBorderColor = Color.FromArgb(226, 232, 240);
            pnlChartInner.CustomBorderThickness = new Padding(1);
            pnlChartCard.Controls.Add(pnlChartInner);

            Label lblChartTitle = new Label();
            lblChartTitle.Text = "نظرة عامة على الأداء";
            lblChartTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblChartTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblChartTitle.Location = new Point(20, 12);
            lblChartTitle.Size = new Size(200, 20);
            pnlChartInner.Controls.Add(lblChartTitle);

            // Filter Dropdown replica
            Guna2ComboBox cmbFilter = new Guna2ComboBox();
            cmbFilter.Size = new Size(120, 28);
            cmbFilter.Location = new Point(pnlChartInner.Width - 140, 8);
            cmbFilter.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            cmbFilter.BorderRadius = 8;
            cmbFilter.Items.Add("آخر 7 أيام");
            cmbFilter.SelectedIndex = 0;
            Panel pnlCanvas = new Panel();
            pnlCanvas.Name = "pnlCanvas";
            pnlCanvas.Location = new Point(20, 40);
            pnlCanvas.Size = new Size(pnlChartInner.Width - 40, 175);
            pnlCanvas.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            pnlCanvas.Paint += ChartCanvas_Paint;
            pnlChartInner.Controls.Add(pnlCanvas);

            // 3. Row 2: KPI Cards
            TableLayoutPanel tlpCards = new TableLayoutPanel();
            tlpCards.Dock = DockStyle.Top;
            tlpCards.Height = 100;
            tlpCards.Padding = new Padding(20, 5, 20, 5);
            tlpCards.ColumnCount = 3;
            tlpCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tlpCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            tlpCards.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
            pnlMainContainer.Controls.Add(tlpCards);
            tlpCards.BringToFront();

            cardProducts = CreateKPICard("إجمالي المنتجات", "0", "+4.2% عن الفترة السابقة", Color.FromArgb(59, 130, 246), Color.FromArgb(219, 234, 254), "🛍️");
            cardEmployees = CreateKPICard("عدد الموظفين", "0", "— عن الفترة السابقة", Color.FromArgb(147, 51, 234), Color.FromArgb(243, 232, 255), "👥");
            cardProfit = CreateKPICard("صافي الأرباح", "0.00 EGP", "+8.7% عن الفترة السابقة", Color.FromArgb(245, 158, 11), Color.FromArgb(254, 243, 199), "💰");

            tlpCards.Controls.Add(cardProducts, 0, 0);
            tlpCards.Controls.Add(cardEmployees, 1, 0);
            tlpCards.Controls.Add(cardProfit, 2, 0);

            // 4. Row 3: Bottom 3-Column Grid - Docked FILL to dynamically expand and adapt to all screen heights!
            TableLayoutPanel tlpBottom = new TableLayoutPanel();
            tlpBottom.Dock = DockStyle.Fill;
            tlpBottom.Padding = new Padding(20, 5, 20, 15);
            tlpBottom.ColumnCount = 3;
            tlpBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));
            tlpBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tlpBottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            pnlMainContainer.Controls.Add(tlpBottom);
            tlpBottom.BringToFront();

            // Col 1: Modern Sales
            Guna2Panel pnlCol1Card = CreateBottomColCard("المبيعات الحديثة", tlpBottom, 0, "Sales");
            pnlRecentSalesList = new Panel();
            pnlRecentSalesList.Dock = DockStyle.Fill;
            pnlRecentSalesList.AutoScroll = true; // Safe scroll inside list
            pnlCol1Card.Controls.Add(pnlRecentSalesList);
            pnlRecentSalesList.BringToFront();

            // Col 2: Top Selling Products
            Guna2Panel pnlCol2Card = CreateBottomColCard("المنتجات الأكثر مبيعاً", tlpBottom, 1, "Products");
            pnlBestSellersList = new Panel();
            pnlBestSellersList.Dock = DockStyle.Fill;
            pnlBestSellersList.AutoScroll = true; // Safe scroll inside list
            pnlCol2Card.Controls.Add(pnlBestSellersList);
            pnlBestSellersList.BringToFront();

            // Col 3: Recent Activity
            Guna2Panel pnlCol3Card = CreateBottomColCard("الأنشطة الأخيرة", tlpBottom, 2, "Reports");
            pnlActivitiesList = new Panel();
            pnlActivitiesList.Dock = DockStyle.Fill;
            pnlActivitiesList.AutoScroll = true; // Safe scroll inside list
            pnlCol3Card.Controls.Add(pnlActivitiesList);
            pnlActivitiesList.BringToFront();

            // Clock update timer
            Timer clockTimer = new Timer();
            clockTimer.Interval = 1000;
            clockTimer.Tick += (s, e) => {
                lblClock.Text = DateTime.Now.ToString("dd MMMM yyyy - hh:mm:ss tt");
            };
            clockTimer.Start();
        }

        private Guna2Panel CreateKPICard(string title, string valText, string subText, Color mainColor, Color bgColor, string emoji)
        {
            Guna2Panel card = new Guna2Panel();
            card.Dock = DockStyle.Fill;
            card.BorderRadius = 14;
            card.FillColor = Color.White;
            card.CustomBorderColor = Color.FromArgb(226, 232, 240);
            card.CustomBorderThickness = new Padding(1);
            card.Margin = new Padding(6);
            card.Padding = new Padding(10);

            // Left Side circular icon (Anchored for fluid scaling)
            Guna2Panel pnlCircle = new Guna2Panel();
            pnlCircle.Size = new Size(40, 40);
            pnlCircle.Location = new Point(12, 18);
            pnlCircle.BorderRadius = 20;
            pnlCircle.FillColor = bgColor;
            pnlCircle.Anchor = AnchorStyles.Left;
            card.Controls.Add(pnlCircle);

            Label lblEmoji = new Label();
            lblEmoji.Text = emoji;
            lblEmoji.Font = new Font("Segoe UI", 13F);
            lblEmoji.TextAlign = ContentAlignment.MiddleCenter;
            lblEmoji.Dock = DockStyle.Fill;
            pnlCircle.Controls.Add(lblEmoji);

            // Right Side Texts (Perfect relative anchors)
            Label lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(100, 116, 139);
            lblTitle.Location = new Point(60, 10);
            lblTitle.Size = new Size(card.Width - 75, 16);
            lblTitle.TextAlign = ContentAlignment.MiddleRight;
            lblTitle.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            card.Controls.Add(lblTitle);

            Label lblVal = new Label();
            lblVal.Text = valText;
            lblVal.Font = new Font("Segoe UI", 13F, FontStyle.Bold);
            lblVal.ForeColor = Color.FromArgb(15, 23, 42);
            lblVal.Location = new Point(60, 26);
            lblVal.Size = new Size(card.Width - 75, 25);
            lblVal.Name = "lblVal";
            lblVal.TextAlign = ContentAlignment.MiddleRight;
            lblVal.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            card.Controls.Add(lblVal);

            Label lblSub = new Label();
            lblSub.Text = subText;
            lblSub.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
            lblSub.ForeColor = subText.Contains("+") ? Color.FromArgb(22, 163, 74) : Color.FromArgb(100, 116, 139);
            lblSub.Location = new Point(60, 52);
            lblSub.Size = new Size(card.Width - 75, 15);
            lblSub.TextAlign = ContentAlignment.MiddleRight;
            lblSub.Anchor = AnchorStyles.Right | AnchorStyles.Left | AnchorStyles.Top;
            card.Controls.Add(lblSub);

            return card;
        }

        private Guna2Panel CreateBottomColCard(string title, TableLayoutPanel parent, int colIndex, string pageTag)
        {
            Guna2Panel card = new Guna2Panel();
            card.Dock = DockStyle.Fill;
            card.BorderRadius = 14;
            card.FillColor = Color.White;
            card.CustomBorderColor = Color.FromArgb(226, 232, 240);
            card.CustomBorderThickness = new Padding(1);
            card.Padding = new Padding(12);
            card.Margin = new Padding(6);
            parent.Controls.Add(card, colIndex, 0);

            // Card Header
            Panel pnlHeader = new Panel();
            pnlHeader.Height = 32;
            pnlHeader.Dock = DockStyle.Top;
            card.Controls.Add(pnlHeader);

            Label lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Dock = DockStyle.Right;
            lblTitle.Width = 150;
            lblTitle.TextAlign = ContentAlignment.MiddleRight;
            pnlHeader.Controls.Add(lblTitle);

            // View all button
            Guna2Button btnViewAll = new Guna2Button();
            btnViewAll.Text = "عرض";
            btnViewAll.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            btnViewAll.ForeColor = Color.FromArgb(59, 130, 246);
            btnViewAll.FillColor = Color.Transparent;
            btnViewAll.Size = new Size(70, 22);
            btnViewAll.Location = new Point(0, 4);
            btnViewAll.Cursor = Cursors.Hand;
            btnViewAll.Click += (s, e) => {
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
                    mainForm.NavigateToPage(pageTag);
                }
            };
            pnlHeader.Controls.Add(btnViewAll);

            return card;
        }

        private void FrmDashboard_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // 1. Load Products count
                object prodObj = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Products;");
                long productsCount = prodObj != DBNull.Value ? (long)prodObj : 0;
                UpdateCardValue(cardProducts, $"{productsCount}");
 
                // 2. Load Employees count
                object empObj = DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM Employees;");
                long employeesCount = empObj != DBNull.Value ? (long)empObj : 0;
                UpdateCardValue(cardEmployees, $"{employeesCount}");
 
                // 3. Load Net Profit (Sales - Expenses)
                object salesObj = DatabaseHelper.ExecuteScalar("SELECT SUM(Total) FROM Sales;");
                double sales = salesObj != DBNull.Value ? Convert.ToDouble(salesObj) : 0.0;
 
                object expObj = DatabaseHelper.ExecuteScalar("SELECT SUM(Amount) FROM Expenses;");
                double expenses = expObj != DBNull.Value ? Convert.ToDouble(expObj) : 0.0;
 
                double profit = sales - expenses;
                UpdateCardValue(cardProfit, $"{profit:N2} EGP");

                // 4. Load Chart Data (Sales of the last 7 days)
                DateTime today = DateTime.Today;
                for (int i = 0; i < 7; i++)
                {
                    DateTime day = today.AddDays(-6 + i); // -6 to 0
                    chartDays[i] = day.ToString("ddd", System.Globalization.CultureInfo.InvariantCulture);
                    string dateStr = day.ToString("yyyy-MM-dd");

                    object totalObj = DatabaseHelper.ExecuteScalar(
                        "SELECT SUM(Total) FROM Sales WHERE SaleDate = @date;",
                        new SQLiteParameter[] {
                            new SQLiteParameter("@date", dateStr)
                        }
                    );

                    double total = (totalObj != null && totalObj != DBNull.Value) ? Convert.ToDouble(totalObj) : 0.0;
                    chartData[i] = (float)total;
                }

                // Refresh chart canvas
                if (pnlChartCard != null)
                {
                    foreach (Control c1 in pnlChartCard.Controls)
                    {
                        if (c1 is Guna2Panel inner)
                        {
                            foreach (Control c2 in inner.Controls)
                            {
                                if (c2 is Panel canvas && canvas.Name == "pnlCanvas")
                                {
                                    canvas.Invalidate();
                                    break;
                                }
                            }
                        }
                    }
                }
 
                // 5. Populate Col 1: Recent Sales
                PopulateRecentSales();
 
                // 6. Populate Col 2: Best Sellers
                PopulateBestSellers();
 
                // 7. Populate Col 3: Recent Activities
                PopulateActivities();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading dashboard metrics: " + ex.Message);
            }
        }

        private void UpdateCardValue(Guna2Panel card, string value)
        {
            foreach (Control ctrl in card.Controls)
            {
                if (ctrl is Label lbl && lbl.Name == "lblVal")
                {
                    lbl.Text = value;
                    break;
                }
            }
        }

        private void PopulateRecentSales()
        {
            pnlRecentSalesList.Controls.Clear();
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery("SELECT InvoiceNo, SaleDate, Total FROM Sales ORDER BY SaleID DESC LIMIT 4;");
                foreach (DataRow row in dt.Rows)
                {
                    string invoice = row["InvoiceNo"].ToString();
                    string date = row["SaleDate"].ToString();
                    double total = Convert.ToDouble(row["Total"]);

                    // Fully fluid panel utilizing DockStyle.Top stacking
                    Panel pnlItem = new Panel();
                    pnlItem.Height = 52;
                    pnlItem.Dock = DockStyle.Top;
                    pnlItem.Padding = new Padding(4);
                    pnlRecentSalesList.Controls.Add(pnlItem);

                    // Circular green document icon
                    Guna2Panel pnlIcon = new Guna2Panel();
                    pnlIcon.Size = new Size(32, 32);
                    pnlIcon.Dock = DockStyle.Left;
                    pnlIcon.BorderRadius = 16;
                    pnlIcon.FillColor = Color.FromArgb(220, 252, 231);
                    pnlIcon.Margin = new Padding(0, 0, 8, 0);

                    Label lblIcon = new Label();
                    lblIcon.Text = "📄";
                    lblIcon.Font = new Font("Segoe UI", 9F);
                    lblIcon.TextAlign = ContentAlignment.MiddleCenter;
                    lblIcon.Dock = DockStyle.Fill;
                    pnlIcon.Controls.Add(lblIcon);
                    pnlItem.Controls.Add(pnlIcon);

                    // Invoice details
                    Label lblInv = new Label();
                    lblInv.Text = $"{invoice}\n{date}";
                    lblInv.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
                    lblInv.ForeColor = ThemeManager.SubtextColor;
                    lblInv.Dock = DockStyle.Left;
                    lblInv.Width = 110;
                    lblInv.TextAlign = ContentAlignment.MiddleLeft;
                    lblInv.Padding = new Padding(5, 0, 0, 0);
                    pnlItem.Controls.Add(lblInv);

                    // Right utility panel to group amount and badge neatly
                    Panel pnlRight = new Panel();
                    pnlRight.Dock = DockStyle.Right;
                    pnlRight.Width = 90;
                    pnlItem.Controls.Add(pnlRight);

                    // Amount right aligned
                    Label lblAmt = new Label();
                    lblAmt.Text = $"{total:N0} EGP";
                    lblAmt.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                    lblAmt.ForeColor = ThemeManager.TextColor;
                    lblAmt.Dock = DockStyle.Top;
                    lblAmt.Height = 16;
                    lblAmt.TextAlign = ContentAlignment.MiddleRight;
                    pnlRight.Controls.Add(lblAmt);

                    // Status pill right-bottom
                    Guna2Panel pnlBadge = new Guna2Panel();
                    pnlBadge.Size = new Size(50, 16);
                    pnlBadge.Location = new Point(40, 18);
                    pnlBadge.BorderRadius = 5;
                    pnlBadge.FillColor = ThemeManager.IsDarkMode ? Color.FromArgb(6, 78, 59) : Color.FromArgb(220, 252, 231);
                    pnlBadge.Anchor = AnchorStyles.Right;

                    Label lblBadge = new Label();
                    lblBadge.Text = "مدفوعة";
                    lblBadge.Font = new Font("Segoe UI", 6.5F, FontStyle.Bold);
                    lblBadge.ForeColor = ThemeManager.IsDarkMode ? Color.FromArgb(52, 211, 153) : Color.FromArgb(21, 128, 61);
                    lblBadge.TextAlign = ContentAlignment.MiddleCenter;
                    lblBadge.Dock = DockStyle.Fill;
                    pnlBadge.Controls.Add(lblBadge);
                    pnlRight.Controls.Add(pnlBadge);
                }
            }
            catch { }
        }

        private void PopulateBestSellers()
        {
            pnlBestSellersList.Controls.Clear();
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery(@"
                    SELECT p.Name, SUM(si.Qty) as TotalQty, SUM(si.Total) as Revenue 
                    FROM SaleItems si 
                    JOIN Products p ON si.ProductID = p.ProductID 
                    GROUP BY si.ProductID 
                    ORDER BY TotalQty DESC 
                    LIMIT 4;");

                if (dt.Rows.Count == 0)
                {
                    dt.Columns.Add("Name", typeof(string));
                    dt.Columns.Add("TotalQty", typeof(int));
                    dt.Columns.Add("Revenue", typeof(double));

                    dt.Rows.Add("Logitech ماوس لاسلكي", 85, 2500.0);
                    dt.Rows.Add("Guna كيبورد ميكانيكي", 68, 1800.0);
                    dt.Rows.Add("شاشة Dell 24 بوصة", 45, 1350.0);
                    dt.Rows.Add("كابل HDMI 4K", 32, 960.0);
                }

                foreach (DataRow row in dt.Rows)
                {
                    string name = row["Name"].ToString();
                    string qty = row["TotalQty"].ToString();
                    double revenue = Convert.ToDouble(row["Revenue"]);

                    // Stacking fluid item using DockStyle.Top
                    Panel pnlItem = new Panel();
                    pnlItem.Height = 52;
                    pnlItem.Dock = DockStyle.Top;
                    pnlItem.Padding = new Padding(4);
                    pnlBestSellersList.Controls.Add(pnlItem);

                    // Product Box rounded grey icon
                    Guna2Panel pnlIcon = new Guna2Panel();
                    pnlIcon.Size = new Size(32, 32);
                    pnlIcon.Dock = DockStyle.Left;
                    pnlIcon.BorderRadius = 6;
                    pnlIcon.FillColor = ThemeManager.IsDarkMode ? Color.FromArgb(51, 65, 85) : Color.FromArgb(241, 245, 249);

                    Label lblIcon = new Label();
                    lblIcon.Text = "📦";
                    lblIcon.Font = new Font("Segoe UI", 9F);
                    lblIcon.TextAlign = ContentAlignment.MiddleCenter;
                    lblIcon.Dock = DockStyle.Fill;
                    pnlIcon.Controls.Add(lblIcon);
                    pnlItem.Controls.Add(pnlIcon);

                    // Info text labels
                    Label lblInfo = new Label();
                    lblInfo.Text = $"{name}\n{qty} مبيعات";
                    lblInfo.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
                    lblInfo.ForeColor = ThemeManager.SubtextColor;
                    lblInfo.Dock = DockStyle.Left;
                    lblInfo.Width = 120;
                    lblInfo.TextAlign = ContentAlignment.MiddleLeft;
                    lblInfo.Padding = new Padding(5, 0, 0, 0);
                    pnlItem.Controls.Add(lblInfo);

                    // Price right aligned
                    Label lblPrice = new Label();
                    lblPrice.Text = $"{revenue:N0} EGP";
                    lblPrice.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                    lblPrice.ForeColor = ThemeManager.TextColor;
                    lblPrice.Dock = DockStyle.Right;
                    lblPrice.Width = 90;
                    lblPrice.TextAlign = ContentAlignment.MiddleRight;
                    pnlItem.Controls.Add(lblPrice);
                }
            }
            catch { }
        }

        private void PopulateActivities()
        {
            pnlActivitiesList.Controls.Clear();
            try
            {
                DataTable dt = DatabaseHelper.ExecuteQuery("SELECT Description, LogType, Timestamp FROM Logs ORDER BY LogID DESC LIMIT 4;");
                
                if (dt.Rows.Count == 0)
                {
                    dt.Columns.Add("Description", typeof(string));
                    dt.Columns.Add("LogType", typeof(string));
                    dt.Columns.Add("Timestamp", typeof(string));

                    dt.Rows.Add("تم تسجيل دخول جديد للنظام", "INFO", "منذ 5 دقائق");
                    dt.Rows.Add("تم إنشاء فاتورة جديدة #INV-1005", "INFO", "منذ 15 دقيقة");
                    dt.Rows.Add("تم تحديث بيانات منتج (منتج تجريبي 1)", "WARNING", "منذ 1 ساعة");
                    dt.Rows.Add("تم إضافة موظف جديد (أحمد محمد)", "INFO", "منذ 2 ساعة");
                }

                foreach (DataRow row in dt.Rows)
                {
                    string desc = row["Description"].ToString();
                    string type = row["LogType"].ToString();
                    string time = row["Timestamp"].ToString();

                    // Timeline item using DockStyle.Top for full fluidity
                    Panel pnlItem = new Panel();
                    pnlItem.Height = 52;
                    pnlItem.Dock = DockStyle.Top;
                    pnlItem.Padding = new Padding(4);
                    pnlActivitiesList.Controls.Add(pnlItem);

                    // Timeline circle container on the far right
                    Guna2Panel pnlDot = new Guna2Panel();
                    pnlDot.Size = new Size(28, 28);
                    pnlDot.Dock = DockStyle.Right;
                    pnlDot.BorderRadius = 14;
                    pnlDot.Margin = new Padding(5, 0, 0, 0);

                    if (type == "WARNING" || type == "ERROR")
                    {
                        pnlDot.FillColor = ThemeManager.IsDarkMode ? Color.FromArgb(127, 29, 29) : Color.FromArgb(254, 226, 226); // dark red in dark mode
                    }
                    else
                    {
                        pnlDot.FillColor = ThemeManager.IsDarkMode ? Color.FromArgb(30, 58, 138) : Color.FromArgb(239, 246, 255); // dark blue in dark mode
                    }

                    Label lblIcon = new Label();
                    lblIcon.Text = type == "WARNING" || type == "ERROR" ? "⚠️" : "👤";
                    lblIcon.Font = new Font("Segoe UI", 8F);
                    lblIcon.TextAlign = ContentAlignment.MiddleCenter;
                    lblIcon.Dock = DockStyle.Fill;
                    pnlDot.Controls.Add(lblIcon);
                    pnlItem.Controls.Add(pnlDot);

                    // Text descriptions left-aligned next to the timeline circle
                    Label lblText = new Label();
                    lblText.Text = $"{desc}\n{time}";
                    lblText.Font = new Font("Segoe UI", 7.5F, FontStyle.Bold);
                    lblText.ForeColor = ThemeManager.SubtextColor;
                    lblText.Dock = DockStyle.Fill;
                    lblText.TextAlign = ContentAlignment.MiddleRight;
                    lblText.Padding = new Padding(0, 0, 5, 0);
                    pnlItem.Controls.Add(lblText);
                }
            }
            catch { }
        }

        private void ChartCanvas_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Panel canvas = sender as Panel;
            if (canvas == null) return;

            int w = canvas.Width;
            int h = canvas.Height;

            int marginL = 40;
            int marginR = 15;
            int marginB = 25;
            int marginT = 15;

            int plotW = w - marginL - marginR;
            int plotH = h - marginB - marginT;

            if (plotW <= 50 || plotH <= 50) return;

            // Draw horizontal light grid lines
            Pen gridPen = new Pen(ThemeManager.BorderColor, 1);
            for (int i = 0; i <= 4; i++)
            {
                float y = marginT + plotH * (i / 4.0f);
                g.DrawLine(gridPen, marginL, y, w - marginR, y);
            }

            // Axis labels
            Font labelFont = new Font("Segoe UI", 8, FontStyle.Bold);
            Brush labelBrush = new SolidBrush(ThemeManager.SubtextColor);
 
            float maxVal = 1000f;
            for (int i = 0; i < chartData.Length; i++)
            {
                if (chartData[i] > maxVal)
                    maxVal = chartData[i];
            }
            maxVal *= 1.15f; // 15% headroom
            
            g.DrawString(string.Format("{0:N0}", maxVal), labelFont, labelBrush, 5, marginT - 5);
            g.DrawString(string.Format("{0:N0}", maxVal * 0.75f), labelFont, labelBrush, 5, marginT + (plotH * 0.25f) - 5);
            g.DrawString(string.Format("{0:N0}", maxVal * 0.50f), labelFont, labelBrush, 5, marginT + (plotH * 0.50f) - 5);
            g.DrawString(string.Format("{0:N0}", maxVal * 0.25f), labelFont, labelBrush, 5, marginT + (plotH * 0.75f) - 5);
            g.DrawString("0", labelFont, labelBrush, 5, marginT + plotH - 5);
 
            PointF[] points = new PointF[chartData.Length];
 
            for (int i = 0; i < chartData.Length; i++)
            {
                float x = marginL + plotW * (i / (float)(chartData.Length - 1));
                float y = marginT + plotH * (1.0f - (chartData[i] / maxVal));
                points[i] = new PointF(x, y);
 
                // Day labels
                if (chartDays[i] != null)
                {
                    g.DrawString(chartDays[i], labelFont, labelBrush, x - 10, marginT + plotH + 5);
                }
            }

            // Beautiful Gradient Area under the Bezier path
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddLine(marginL, marginT + plotH, points[0].X, points[0].Y);
                // Draw a smooth curved Bezier shape
                path.AddCurve(points);
                path.AddLine(points[points.Length - 1].X, points[points.Length - 1].Y, w - marginR, marginT + plotH);
                path.CloseFigure();

                using (LinearGradientBrush brush = new LinearGradientBrush(
                    new Point(0, marginT), new Point(0, marginT + plotH),
                    Color.FromArgb(80, 59, 130, 246), // Premium Royal Blue gradient
                    Color.FromArgb(0, 59, 130, 246)))
                {
                    g.FillPath(brush, path);
                }
            }

            // Draw thick top line
            using (Pen linePen = new Pen(Color.FromArgb(59, 130, 246), 3))
            {
                g.DrawCurve(linePen, points);
            }

            // Render premium round white circular dots with blue borders
            Brush dotBrush = new SolidBrush(Color.White);
            Pen dotBorder = new Pen(Color.FromArgb(59, 130, 246), 3);
            for (int i = 0; i < points.Length; i++)
            {
                g.FillEllipse(dotBrush, points[i].X - 4, points[i].Y - 4, 8, 8);
                g.DrawEllipse(dotBorder, points[i].X - 4, points[i].Y - 4, 8, 8);
            }
        }
    }
}
