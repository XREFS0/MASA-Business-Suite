using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;

namespace MASA_Business_Suite
{
    public class FrmSplash : Form
    {
        private Guna2ProgressBar progressBar;
        private Label lblPercentage;
        private Timer timer;
        private int progressValue = 0;

        public FrmSplash()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(900, 600);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ShowInTaskbar = false;
            this.DoubleBuffered = true;

            // Load Background Image from Icons directory
            Image bg = FrmMain.LoadIcon("back.png");
            if (bg != null)
            {
                this.BackgroundImage = bg;
                this.BackgroundImageLayout = ImageLayout.Stretch;
            }
            else
            {
                // Fallback background color if image not found
                this.BackColor = Color.FromArgb(15, 23, 42); // deep navy slate
            }

            // Guna2ProgressBar Setup
            progressBar = new Guna2ProgressBar();
            progressBar.Size = new Size(400, 8);
            progressBar.Location = new Point((this.Width - progressBar.Width) / 2, 480);
            progressBar.BorderRadius = 4;
            
            // Sleek blue/cyan gradient progress bar matching the design
            progressBar.FillColor = Color.FromArgb(15, 32, 67); // Dark track
            progressBar.ProgressColor = Color.FromArgb(37, 99, 235); // Royal Blue
            progressBar.ProgressColor2 = Color.FromArgb(0, 191, 255); // Bright Cyan
            progressBar.Value = 0;
            progressBar.BackColor = Color.Transparent;
            this.Controls.Add(progressBar);

            // Percentage Label
            lblPercentage = new Label();
            lblPercentage.Text = "0%";
            lblPercentage.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            lblPercentage.ForeColor = Color.FromArgb(37, 99, 235);
            lblPercentage.BackColor = Color.Transparent;
            lblPercentage.AutoSize = true;
            lblPercentage.TextAlign = ContentAlignment.MiddleLeft;
            lblPercentage.Location = new Point(progressBar.Right + 12, progressBar.Top - 4);
            this.Controls.Add(lblPercentage);

            // Timer Setup for smooth progress loading
            timer = new Timer();
            timer.Interval = 25; // 25ms for smooth progression
            timer.Tick += Timer_Tick;

            this.Load += FrmSplash_Load;
        }

        private void FrmSplash_Load(object sender, Array e)
        {
            // Center the loading label based on actual dimensions after creation
        }

        // Standard event signature for Load
        private void FrmSplash_Load(object sender, EventArgs e)
        {
            lblPercentage.Location = new Point(progressBar.Right + 12, progressBar.Top - 5);
            
            // Start progress animation
            timer.Start();

            // Run DB initialization in the background
            Task.Run(() =>
            {
                try
                {
                    DatabaseHelper.InitializeDatabase();
                }
                catch (Exception ex)
                {
                    this.BeginInvoke(new Action(() => {
                        MessageBox.Show("خطأ أثناء تهيئة قاعدة البيانات: " + ex.Message, "خطأ في النظام", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                }
            });
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (progressValue < 100)
            {
                progressValue += 1;
                progressBar.Value = progressValue;
                lblPercentage.Text = progressValue + "%";
            }
            else
            {
                timer.Stop();
                this.Hide();

                // Open the main dashboard form
                FrmMain mainForm = new FrmMain();
                mainForm.FormClosed += (s, args) => this.Close(); // Safely exit the application loop when main is closed
                mainForm.Show();
            }
        }
    }
}
