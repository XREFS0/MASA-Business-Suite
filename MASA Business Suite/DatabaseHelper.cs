using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace MASA_Business_Suite
{
    public static class DatabaseHelper
    {
        private static string DbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "masa_erp.db");
        private static string ConnectionString = $"Data Source={DbPath};Version=3;";

        public static SQLiteConnection GetConnection()
        {
            return new SQLiteConnection(ConnectionString);
        }

        public static void InitializeDatabase()
        {
            bool dbExists = File.Exists(DbPath);
            if (!dbExists)
            {
                SQLiteConnection.CreateFile(DbPath);
            }

            using (var conn = GetConnection())
            {
                conn.Open();

                // Create Tables
                using (var cmd = conn.CreateCommand())
                {
                    // 1. Settings Table
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Settings (
                            SettingID INTEGER PRIMARY KEY AUTOINCREMENT,
                            CompanyName TEXT,
                            Phone TEXT,
                            Email TEXT,
                            LogoPath TEXT,
                            ThemeMode TEXT,
                            Address TEXT,
                            TaxRate REAL,
                            LoyaltyRate REAL
                        );";
                    cmd.ExecuteNonQuery();

                    // 2. Departments Table
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Departments (
                            DepartmentID INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT UNIQUE,
                            Manager TEXT,
                            Description TEXT
                        );";
                    cmd.ExecuteNonQuery();

                    // 3. Employees Table
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Employees (
                            EmployeeID INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT,
                            Phone TEXT,
                            Email TEXT,
                            DepartmentID INTEGER,
                            Salary REAL,
                            Address TEXT,
                            JoinDate TEXT,
                            PhotoPath TEXT,
                            FOREIGN KEY(DepartmentID) REFERENCES Departments(DepartmentID)
                        );";
                    cmd.ExecuteNonQuery();

                    // 4. Attendance Table
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Attendance (
                            AttendanceID INTEGER PRIMARY KEY AUTOINCREMENT,
                            EmployeeID INTEGER,
                            CheckIn TEXT,
                            CheckOut TEXT,
                            Date TEXT,
                            Status TEXT,
                            FOREIGN KEY(EmployeeID) REFERENCES Employees(EmployeeID)
                        );";
                    cmd.ExecuteNonQuery();

                    // 5. Payroll Table
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Payroll (
                            PayrollID INTEGER PRIMARY KEY AUTOINCREMENT,
                            EmployeeID INTEGER,
                            Salary REAL,
                            Bonus REAL,
                            Deduction REAL,
                            NetSalary REAL,
                            PayDate TEXT,
                            FOREIGN KEY(EmployeeID) REFERENCES Employees(EmployeeID)
                        );";
                    cmd.ExecuteNonQuery();

                    // 6. Categories Table
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Categories (
                            CategoryID INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT UNIQUE,
                            Description TEXT
                        );";
                    cmd.ExecuteNonQuery();

                    // 7. Products Table
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Products (
                            ProductID INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT,
                            Barcode TEXT UNIQUE,
                            CategoryID INTEGER,
                            BuyPrice REAL,
                            SellPrice REAL,
                            Qty INTEGER,
                            ImagePath TEXT,
                            MinStock INTEGER DEFAULT 5,
                            FOREIGN KEY(CategoryID) REFERENCES Categories(CategoryID)
                        );";
                    cmd.ExecuteNonQuery();

                    // 8. Suppliers Table
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Suppliers (
                            SupplierID INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT,
                            Phone TEXT,
                            Address TEXT,
                            Company TEXT,
                            Notes TEXT
                        );";
                    cmd.ExecuteNonQuery();

                    // 9. Customers Table
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Customers (
                            CustomerID INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT,
                            Phone TEXT,
                            Email TEXT,
                            Address TEXT,
                            LoyaltyPoints INTEGER DEFAULT 0
                        );";
                    cmd.ExecuteNonQuery();

                    // 10. Sales Table
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Sales (
                            SaleID INTEGER PRIMARY KEY AUTOINCREMENT,
                            InvoiceNo TEXT UNIQUE,
                            CustomerID INTEGER,
                            SaleDate TEXT,
                            Discount REAL,
                            Tax REAL,
                            Total REAL,
                            FOREIGN KEY(CustomerID) REFERENCES Customers(CustomerID)
                        );";
                    cmd.ExecuteNonQuery();

                    // 11. SaleItems Table
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS SaleItems (
                            SaleItemID INTEGER PRIMARY KEY AUTOINCREMENT,
                            SaleID INTEGER,
                            ProductID INTEGER,
                            Qty INTEGER,
                            Price REAL,
                            Total REAL,
                            FOREIGN KEY(SaleID) REFERENCES Sales(SaleID),
                            FOREIGN KEY(ProductID) REFERENCES Products(ProductID)
                        );";
                    cmd.ExecuteNonQuery();

                    // 12. Purchases Table
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Purchases (
                            PurchaseID INTEGER PRIMARY KEY AUTOINCREMENT,
                            SupplierID INTEGER,
                            Product TEXT,
                            Qty INTEGER,
                            Cost REAL,
                            Date TEXT,
                            FOREIGN KEY(SupplierID) REFERENCES Suppliers(SupplierID)
                        );";
                    cmd.ExecuteNonQuery();

                    // 13. Expenses Table
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Expenses (
                            ExpenseID INTEGER PRIMARY KEY AUTOINCREMENT,
                            ExpenseType TEXT,
                            Amount REAL,
                            Description TEXT,
                            Date TEXT
                        );";
                    cmd.ExecuteNonQuery();

                    // 14. Logs Table
                    cmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS Logs (
                            LogID INTEGER PRIMARY KEY AUTOINCREMENT,
                            Timestamp TEXT,
                            LogType TEXT,
                            Description TEXT
                        );";
                    cmd.ExecuteNonQuery();
                }

                // Seed Mock Data if empty
                SeedData(conn);
            }
        }

        private static void SeedData(SQLiteConnection conn)
        {
            // Seed Settings if not present
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM Settings;";
                long count = (long)cmd.ExecuteScalar();
                if (count == 0)
                {
                    cmd.CommandText = @"
                        INSERT INTO Settings (CompanyName, Phone, Email, LogoPath, ThemeMode, Address, TaxRate, LoyaltyRate)
                        VALUES ('MASA Business Suite', '+201023456789', 'info@masa-pro.com', '', 'Dark', 'Cairo, Egypt', 14.0, 1.0);";
                    cmd.ExecuteNonQuery();
                }
            }

            // Check if we need to seed rich statistics (if sales count is low)
            bool needsRichSeeding = false;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM Sales;";
                long count = (long)cmd.ExecuteScalar();
                if (count < 5)
                {
                    needsRichSeeding = true;
                }
            }

            if (!needsRichSeeding)
            {
                return;
            }

            // Clear existing tables to prevent duplicate key or constraint conflicts
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "DELETE FROM SaleItems; DELETE FROM Sales; DELETE FROM Products; DELETE FROM Categories; DELETE FROM Employees; DELETE FROM Departments; DELETE FROM Customers; DELETE FROM Expenses; DELETE FROM Logs; DELETE FROM Attendance; DELETE FROM Payroll; DELETE FROM Purchases; DELETE FROM Suppliers;";
                cmd.ExecuteNonQuery();
            }

            // Seed Departments
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO Departments (DepartmentID, Name, Manager, Description) VALUES
                    (1, 'HR & Operations', 'أحمد المصري', 'إدارة الموارد البشرية والرواتب والحضور والإنصراف'),
                    (2, 'Sales & POS', 'سارة علي', 'المبيعات المباشرة وإدارة الكاشير ونقاط البيع'),
                    (3, 'Inventory & Stock', 'محمود حسن', 'إدارة المنتجات والمستودعات والباركود'),
                    (4, 'Finance', 'نهى كمال', 'المصروفات والأرباح والتقارير المالية');";
                cmd.ExecuteNonQuery();
            }

            // Seed Employees
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO Employees (EmployeeID, Name, Phone, Email, DepartmentID, Salary, Address, JoinDate, PhotoPath) VALUES
                    (1, 'محمد أحمد', '01001234567', 'mohamed@masa.com', 1, 9500.00, 'القاهرة', '2025-01-10', ''),
                    (2, 'سارة علي', '01229876543', 'sara@masa.com', 2, 8000.00, 'الجيزة', '2025-02-15', ''),
                    (3, 'خالد محمود', '01115554433', 'khaled@masa.com', 3, 7500.00, 'الإسكندرية', '2025-03-01', ''),
                    (4, 'نهى كمال', '01552229988', 'noha@masa.com', 4, 11000.00, 'المعادي', '2024-11-20', ''),
                    (5, 'عبد الرحمن حسن', '01012345678', 'abdo@masa.com', 2, 6500.00, 'شبرا', '2025-04-10', ''),
                    (6, 'ياسمين ممدوح', '01145678901', 'yasmin@masa.com', 1, 7200.00, 'مصر الجديدة', '2025-04-12', ''),
                    (7, 'مصطفى رجب', '01234567890', 'mostafa@masa.com', 3, 8300.00, 'حلوان', '2025-02-01', '');";
                cmd.ExecuteNonQuery();
            }

            // Seed Categories
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO Categories (CategoryID, Name, Description) VALUES
                    (1, 'أجهزة إلكترونية', 'شاشات، كيبورد، هواتف، أجهزة كمبيوتر وطابعات'),
                    (2, 'مستلزمات مكتبية', 'أوراق، أقلام، دفاتر ومعدات مكتبية'),
                    (3, 'أثاث مكتبي', 'مكاتب، كراسي، وخزائن مكتبية'),
                    (4, 'برمجيات ورخص', 'تراخيص تشغيل، حزم أوفيس، برامج محاسبية');";
                cmd.ExecuteNonQuery();
            }

            // Seed Products
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO Products (ProductID, Name, Barcode, CategoryID, BuyPrice, SellPrice, Qty, ImagePath, MinStock) VALUES
                    (1, 'شاشة Dell 24 بوصة', 'DELL24', 1, 3500.00, 4500.00, 15, '', 5),
                    (2, 'كيبورد ميكانيكي Guna', 'GUNAKB', 1, 450.00, 650.00, 8, '', 5),
                    (3, 'ماوس لاسلكي Logitech', 'LOGIMS', 1, 200.00, 320.00, 30, '', 10),
                    (4, 'حزمة أوراق A4', 'PAPA4', 2, 90.00, 130.00, 65, '', 15),
                    (5, 'لاب توب HP ProBook', 'HPPROB', 1, 18000.00, 22000.00, 5, '', 5),
                    (6, 'كرسي مكتب هيدروليك', 'CHAIRH', 3, 1200.00, 1850.00, 12, '', 5),
                    (7, 'مكتب خشبي مودرن', 'DESKM', 3, 2500.00, 3500.00, 6, '', 5),
                    (8, 'طابعة Canon Laser', 'CANONPR', 1, 4200.00, 5500.00, 4, '', 5);";
                cmd.ExecuteNonQuery();
            }

            // Seed Customers
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO Customers (CustomerID, Name, Phone, Email, Address, LoyaltyPoints) VALUES
                    (1, 'أحمد محمود', '01004561234', 'ahmed@gmail.com', 'مصر الجديدة', 120),
                    (2, 'عميل نقدي', '00000000000', 'cash@masa.com', 'نقدي', 0),
                    (3, 'منى سعيد', '01123456789', 'mona@hotmail.com', 'التجمع الخامس', 85),
                    (4, 'ياسر جلال', '01278945612', 'yasser@yahoo.com', 'المهندسين', 210),
                    (5, 'رنا شريف', '01512345678', 'rana@gmail.com', 'الدقي', 50);";
                cmd.ExecuteNonQuery();
            }

            // Seed Suppliers
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
                    INSERT INTO Suppliers (SupplierID, Name, Phone, Address, Company, Notes) VALUES
                    (1, 'الشركة المصرية للتوريدات', '022345678', 'القاهرة الجديدة', 'EgyptSupplies', 'المورد الرئيسي للإلكترونيات'),
                    (2, 'مكتبة الفجالة الكبرى', '023456789', 'الفجالة، القاهرة', 'FaggalaBook', 'تأمين كافة الأوراق والمستلزمات المكتبية'),
                    (3, 'العربي جروب', '024567890', 'بنها', 'ElAraby', 'توريد الأجهزة المكتبية والتكييفات');";
                cmd.ExecuteNonQuery();
            }

            // Seed Attendance
            using (var cmd = conn.CreateCommand())
            {
                string today = DateTime.Now.ToString("yyyy-MM-dd");
                string yesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                cmd.CommandText = $@"
                    INSERT INTO Attendance (EmployeeID, CheckIn, CheckOut, Date, Status) VALUES
                    (1, '09:00 AM', '05:00 PM', '{yesterday}', 'Present'),
                    (2, '09:15 AM', '05:00 PM', '{yesterday}', 'Late'),
                    (3, '08:55 AM', '05:00 PM', '{yesterday}', 'Present'),
                    (4, '09:00 AM', '05:00 PM', '{yesterday}', 'Present'),
                    (1, '09:00 AM', '05:00 PM', '{today}', 'Present'),
                    (2, '09:15 AM', '05:00 PM', '{today}', 'Late'),
                    (3, '08:55 AM', '05:00 PM', '{today}', 'Present'),
                    (4, '09:00 AM', '05:00 PM', '{today}', 'Present');";
                cmd.ExecuteNonQuery();
            }

            // Seed Payroll
            using (var cmd = conn.CreateCommand())
            {
                string prevMonth = DateTime.Now.AddMonths(-1).ToString("yyyy-MM");
                cmd.CommandText = $@"
                    INSERT INTO Payroll (EmployeeID, Salary, Bonus, Deduction, NetSalary, PayDate) VALUES
                    (1, 9500.00, 500.00, 0.00, 10000.00, '{prevMonth}-28'),
                    (2, 8000.00, 200.00, 100.00, 8100.00, '{prevMonth}-28'),
                    (3, 7500.00, 0.00, 150.00, 7350.00, '{prevMonth}-28'),
                    (4, 11000.00, 1000.00, 0.00, 12000.00, '{prevMonth}-28');";
                cmd.ExecuteNonQuery();
            }

            // Seed Sales (12 sales over the last 7 days for realistic statistics)
            using (var cmd = conn.CreateCommand())
            {
                string day6 = DateTime.Now.AddDays(-6).ToString("yyyy-MM-dd");
                string day5 = DateTime.Now.AddDays(-5).ToString("yyyy-MM-dd");
                string day4 = DateTime.Now.AddDays(-4).ToString("yyyy-MM-dd");
                string day3 = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd");
                string day2 = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd");
                string day1 = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                string today = DateTime.Now.ToString("yyyy-MM-dd");

                cmd.CommandText = $@"
                    INSERT INTO Sales (SaleID, InvoiceNo, CustomerID, SaleDate, Discount, Tax, Total) VALUES
                    (1, 'INV-2026-0001', 1, '{day6}', 50.00, 14.0, 4450.00),
                    (2, 'INV-2026-0002', 2, '{day5}', 0.00, 14.0, 970.00),
                    (3, 'INV-2026-0003', 3, '{day5}', 0.00, 14.0, 650.00),
                    (4, 'INV-2026-0004', 4, '{day4}', 0.00, 14.0, 3500.00),
                    (5, 'INV-2026-0005', 1, '{day3}', 0.00, 14.0, 5500.00),
                    (6, 'INV-2026-0006', 2, '{day2}', 0.00, 14.0, 320.00),
                    (7, 'INV-2026-0007', 5, '{day2}', 0.00, 14.0, 1850.00),
                    (8, 'INV-2026-0008', 3, '{day1}', 0.00, 14.0, 22000.00),
                    (9, 'INV-2026-0009', 1, '{today}', 50.00, 14.0, 4450.00),
                    (10, 'INV-2026-0010', 2, '{today}', 0.00, 14.0, 970.00),
                    (11, 'INV-2026-0011', 4, '{today}', 0.00, 14.0, 320.00),
                    (12, 'INV-2026-0012', 5, '{today}', 0.00, 14.0, 1850.00);";
                cmd.ExecuteNonQuery();

                cmd.CommandText = @"
                    INSERT INTO SaleItems (SaleID, ProductID, Qty, Price, Total) VALUES
                    (1, 1, 1, 4500.00, 4500.00),
                    (2, 2, 1, 650.00, 650.00),
                    (2, 3, 1, 320.00, 320.00),
                    (3, 2, 1, 650.00, 650.00),
                    (4, 7, 1, 3500.00, 3500.00),
                    (5, 8, 1, 5500.00, 5500.00),
                    (6, 3, 1, 320.00, 320.00),
                    (7, 6, 1, 1850.00, 1850.00),
                    (8, 5, 1, 22000.00, 22000.00),
                    (9, 1, 1, 4500.00, 4500.00),
                    (10, 2, 1, 650.00, 650.00),
                    (10, 3, 1, 320.00, 320.00),
                    (11, 3, 1, 320.00, 320.00),
                    (12, 6, 1, 1850.00, 1850.00);";
                cmd.ExecuteNonQuery();
            }

            // Seed Purchases
            using (var cmd = conn.CreateCommand())
            {
                string today = DateTime.Now.ToString("yyyy-MM-dd");
                cmd.CommandText = $@"
                    INSERT INTO Purchases (SupplierID, Product, Qty, Cost, Date) VALUES
                    (1, 'شاشة Dell 24 بوصة', 10, 3500.00, '{today}'),
                    (2, 'حزمة أوراق A4', 50, 90.00, '{today}'),
                    (3, 'كرسي مكتب هيدروليك', 5, 1200.00, '{today}');";
                cmd.ExecuteNonQuery();
            }

            // Seed Expenses
            using (var cmd = conn.CreateCommand())
            {
                string day6 = DateTime.Now.AddDays(-6).ToString("yyyy-MM-dd");
                string day5 = DateTime.Now.AddDays(-5).ToString("yyyy-MM-dd");
                string day3 = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd");
                string day1 = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
                string today = DateTime.Now.ToString("yyyy-MM-dd");

                cmd.CommandText = $@"
                    INSERT INTO Expenses (ExpenseType, Amount, Description, Date) VALUES
                    ('صيانة أجهزة', 1500.00, 'صيانة أجهزة الكمبيوتر بقسم المبيعات', '{day6}'),
                    ('ضيافة وبوفيه', 300.00, 'مستلزمات الشاي والقهوة والمياه', '{day5}'),
                    ('فاتورة الإنترنت', 800.00, 'اشتراك الإنترنت عالي السرعة للشركة', '{day3}'),
                    ('قرطاسية وأدوات مكتبية', 400.00, 'شراء دفاتر وأقلام ومستلزمات مكتبية', '{day1}'),
                    ('إيجار المقر', 5000.00, 'إيجار المكتب الرئيسي لشهر مايو', '{today}'),
                    ('فاتورة كهرباء ومياه', 1200.00, 'مصاريف المرافق والخدمات لشهر مايو', '{today}');";
                cmd.ExecuteNonQuery();
            }

            // Seed Logs
            using (var cmd = conn.CreateCommand())
            {
                string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                cmd.CommandText = $@"
                    INSERT INTO Logs (Timestamp, LogType, Description) VALUES
                    ('{now}', 'INFO', 'تم إنشاء وإعداد قاعدة البيانات بنجاح.'),
                    ('{now}', 'INFO', 'تم تعبئة البيانات الافتراضية بنجاح لمختلف الأنظمة الفرعية.'),
                    ('{now}', 'INFO', 'تهيئة قاعدة البيانات بسجلات حقيقية لإحصائيات متكاملة.');";
                cmd.ExecuteNonQuery();
            }
        }

        public static DataTable ExecuteQuery(string sql, SQLiteParameter[] parameters = null)
        {
            var dt = new DataTable();
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    using (var da = new SQLiteDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            return dt;
        }

        public static int ExecuteNonQuery(string sql, SQLiteParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public static object ExecuteScalar(string sql, SQLiteParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    return cmd.ExecuteScalar();
                }
            }
        }

        public static void LogActivity(string logType, string description)
        {
            try
            {
                string sql = "INSERT INTO Logs (Timestamp, LogType, Description) VALUES (@ts, @type, @desc);";
                var parameters = new SQLiteParameter[]
                {
                    new SQLiteParameter("@ts", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                    new SQLiteParameter("@type", logType),
                    new SQLiteParameter("@desc", description)
                };
                ExecuteNonQuery(sql, parameters);
            }
            catch { }
        }
    }
}
