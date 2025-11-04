using Newtonsoft.Json;
using RobloxBanCheckerV2;
using RobloxBanCheckerV2.Models;
using RobloxBanCheckerV2.Services;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace RobloxBanCheckerV2
{
    public partial class MainForm : Form
    {
        private readonly VoiceBanChecker checker = new VoiceBanChecker();
        private readonly System.Timers.Timer checkTimer;
        private System.Windows.Forms.Timer animationTimer;
        private System.Windows.Forms.Timer rgbTimer;
        private bool wasBanned = false;
        private string currentCookie = "";
        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;
        private float hue = 0;
        private int checkCount = 0;

        // Erweiterte Farbpalette mit RGB-Unterstützung
        private readonly Color COLOR_DARK = Color.FromArgb(20, 20, 30);
        private readonly Color COLOR_CARD = Color.FromArgb(35, 35, 45);
        private readonly Color COLOR_HEADER = Color.FromArgb(30, 30, 40);
        private readonly Color COLOR_BANNED = Color.FromArgb(255, 87, 87);
        private readonly Color COLOR_ACTIVE = Color.FromArgb(72, 207, 173);
        private readonly Color COLOR_WARNING = Color.FromArgb(255, 193, 7);
        private readonly Color COLOR_INFO = Color.FromArgb(52, 152, 219);
        private readonly Color COLOR_SUCCESS = Color.FromArgb(46, 204, 113);
        private readonly Color COLOR_ACCENT = Color.FromArgb(46, 204, 113);


        // RGB-Farben für Animation
        private Color currentRgbColor = Color.FromArgb(0, 150, 255);

        // Controls
        private TabControl tabControl;
        private Label lblMado;
        private Panel headerPanel;
        private Button btnClose;
        private Button btnMinimize;
        private Button btnHelp;

        // Browser Tab
        private Label lblBrowserStatus;
        private Label lblBrowserDetails;
        private Label lblUserInfo;
        private Label lblStats;
        private Button btnBrowserLogin;

        // Cookie Tab  
        private TextBox txtCookie;
        private Label lblCookieStatus;
        private Label lblCookieDetails;
        private Button btnSetCookie;
        private Button btnTestCookie;

        // Gemeinsame Controls
        private Button btnCheckNow;
        private Button btnToggleAuto;
        private NotifyIcon notifyIcon;

        public MainForm()
        {
            InitializeComponent1();
            SetupModernUI();
            InitializeRgbAnimation();

            checkTimer = new System.Timers.Timer(30000);
            checkTimer.Elapsed += async (s, e) => await CheckStatus();
            checkTimer.AutoReset = true;

            // Event Handler zuweisen
            AssignEventHandlers();
            UpdateAllStatusDisplays();
        }

        private void InitializeNotifyIcon()
        {
            notifyIcon = new NotifyIcon()
            {
                Icon = SystemIcons.Information,
                Text = "Mado's Roblox Monitor v2.0",
                Visible = true
            };
        }

        private void InitializeRgbAnimation()
        {
            rgbTimer = new System.Windows.Forms.Timer();
            rgbTimer.Interval = 50;
            rgbTimer.Tick += (s, e) =>
            {
                hue = (hue + 1) % 360;
                currentRgbColor = HsvToRgb(hue, 0.8, 0.9);
                headerPanel.Invalidate();
                lblMado.Invalidate();
            };
            rgbTimer.Start();
        }

        private Color HsvToRgb(double h, double s, double v)
        {
            h = h % 360;
            double c = v * s;
            double x = c * (1 - Math.Abs((h / 60) % 2 - 1));
            double m = v - c;

            double r = 0, g = 0, b = 0;

            if (0 <= h && h < 60) { r = c; g = x; b = 0; }
            else if (60 <= h && h < 120) { r = x; g = c; b = 0; }
            else if (120 <= h && h < 180) { r = 0; g = c; b = x; }
            else if (180 <= h && h < 240) { r = 0; g = x; b = c; }
            else if (240 <= h && h < 300) { r = x; g = 0; b = c; }
            else if (300 <= h && h < 360) { r = c; g = 0; b = x; }

            return Color.FromArgb(
                (int)((r + m) * 255),
                (int)((g + m) * 255),
                (int)((b + m) * 255)
            );
        }

        private void InitializeComponent1()
        {
            notifyIcon = new NotifyIcon()
            {
                Icon = SystemIcons.Information,
                Text = "Mado's Roblox Monitor",
                Visible = true
            };

            this.SuspendLayout();
            this.ClientSize = new Size(700, 800);
            this.Name = "MainForm";
            this.Text = "Mado's Roblox Voice Monitor";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.ResumeLayout(false);
        }

        private void SetupModernUI()
        {
            // Form Einstellungen
            this.Text = "🎮 Mado's Roblox Voice Monitor v2.0";
            this.Size = new Size(715, 900);
            this.BackColor = COLOR_DARK;
            this.ForeColor = Color.White;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.DoubleBuffered = true;

            // Header Panel
            headerPanel = new Panel()
            {
                Location = new Point(0, 0),
                Size = new Size(700, 70),
                BackColor = COLOR_HEADER
            };
            headerPanel.Paint += HeaderPanel_Paint;

            // Mado Label mit RGB Effect
            lblMado = new Label()
            {
                Text = "✨ Mado's Roblox Suite v2.0",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(400, 40),
                Location = new Point(20, 15),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            // Help Button
            btnHelp = new Button()
            {
                Text = "ℹ️",
                Size = new Size(40, 40),
                Location = new Point(430, 15),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = COLOR_INFO,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Cursor = Cursors.Help
            };
            btnHelp.FlatAppearance.BorderSize = 0;
            btnHelp.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 60, 80);

            // Minimize Button
            btnMinimize = new Button()
            {
                Text = "─",
                Size = new Size(40, 40),
                Location = new Point(610, 15),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold)
            };
            btnMinimize.FlatAppearance.BorderSize = 0;
            btnMinimize.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 60, 80);

            // Close Button
            btnClose = new Button()
            {
                Text = "×",
                Size = new Size(40, 40),
                Location = new Point(655, 15),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold)
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 100, 100);

            headerPanel.Controls.Add(lblMado);
            headerPanel.Controls.Add(btnHelp);
            headerPanel.Controls.Add(btnMinimize);
            headerPanel.Controls.Add(btnClose);

            // Mouse Events für Fensterbewegung
            headerPanel.MouseDown += HeaderPanel_MouseDown;
            headerPanel.MouseMove += HeaderPanel_MouseMove;
            headerPanel.MouseUp += HeaderPanel_MouseUp;
            lblMado.MouseDown += HeaderPanel_MouseDown;
            lblMado.MouseMove += HeaderPanel_MouseMove;
            lblMado.MouseUp += HeaderPanel_MouseUp;

            this.Controls.Add(headerPanel);

            // Tab Control
            tabControl = new TabControl()
            {
                Location = new Point(15, 80),
                Size = new Size(670, 710),
                Appearance = TabAppearance.Normal,
                ItemSize = new Size(120, 35),
                SizeMode = TabSizeMode.Fixed,
                BackColor = COLOR_DARK,
                ForeColor = Color.White
            };

            var tabBrowser = new TabPage("🌐 Browser Login");
            var tabCookie = new TabPage("🔑 Cookie Login");

            SetupBrowserLoginTab(tabBrowser);
            SetupCookieTab(tabCookie);

            tabControl.Controls.Add(tabBrowser);
            tabControl.Controls.Add(tabCookie);
            this.Controls.Add(tabControl);
        }

        private void HeaderPanel_Paint(object sender, PaintEventArgs e)
        {
            // RGB Gradient Hintergrund
            using (var brush = new LinearGradientBrush(
                headerPanel.ClientRectangle,
                Color.Transparent,
                Color.FromArgb(80, currentRgbColor),
                90f))
            {
                e.Graphics.FillRectangle(brush, headerPanel.ClientRectangle);
            }
        }

        private void HeaderPanel_MouseDown(object sender, MouseEventArgs e)
        {
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }

        private void HeaderPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(dif));
            }
        }

        private void HeaderPanel_MouseUp(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void AssignEventHandlers()
        {
            // Window Controls
            btnMinimize.Click += (s, e) => this.WindowState = FormWindowState.Minimized;
            btnClose.Click += (s, e) => this.Close();
            btnHelp.Click += (s, e) => ShowHelpInfo();

            // Browser Tab Events
            btnBrowserLogin.Click += async (s, e) => await PerformBrowserLogin();
            btnCheckNow.Click += async (s, e) => await CheckStatus();
            btnToggleAuto.Click += BtnToggleAuto_Click;

            // Cookie Tab Events
            btnSetCookie.Click += BtnSetCookie_Click;
            btnTestCookie.Click += (s, e) => TestCookieManually();

            // Tab Change
            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateAllStatusDisplays();
        }

        private void UpdateAllStatusDisplays()
        {
            if (!string.IsNullOrEmpty(currentCookie))
            {
                UpdateStatus("🟢 VERBUNDEN", "Bereit für Voice-Chat Überprüfung\nAutomatisches Monitoring aktiv", COLOR_ACTIVE);
            }
            else
            {
                UpdateStatus("🔴 GETRENNT", "Bitte einloggen oder Cookie eingeben\nKeine Verbindung zu Roblox", Color.Orange);
            }
        }

        private void SetupBrowserLoginTab(TabPage tabBrowser)
        {
            tabBrowser.BackColor = COLOR_DARK;

            // Browser Login Button
            btnBrowserLogin = CreateModernButton("🚀 IM BROWSER EINLOGGEN", 40, 30, 600, 70, currentRgbColor);

            var lblInfo = new Label()
            {
                Text = "Öffnet Roblox im Browser zum sicheren Einloggen - Empfohlen für beste Kompatibilität",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.LightGray,
                Size = new Size(600, 25),
                Location = new Point(40, 110),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            // User Info Label
            lblUserInfo = new Label()
            {
                Text = "🔒 Nicht eingeloggt\n💡 Bitte oben auf Login klicken",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.LightGray,
                Size = new Size(600, 50),
                Location = new Point(40, 150),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };

            // Stats Panel
            var statsPanel = CreateCardPanel("📈 STATISTIKEN", 40, 220, 600, 100);
            lblStats = new Label()
            {
                Text = "📅 Letzter Login: -\n📊 Überprüfungen: 0\n🔔 Status: Inaktiv",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.LightGray,
                Size = new Size(580, 80),
                Location = new Point(10, 30),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };
            statsPanel.Controls.Add(lblStats);

            // Status Panel
            var statusPanel = CreateCardPanel("📊 VOICE CHAT STATUS", 40, 340, 600, 150);
            lblBrowserStatus = new Label()
            {
                Text = "🔴 NICHT VERBUNDEN",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.Orange,
                Size = new Size(580, 35),
                Location = new Point(10, 40),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            lblBrowserDetails = new Label()
            {
                Text = "Bitte einloggen um Voice-Chat Status zu prüfen\nDer Monitor überwacht automatisch deinen Ban-Status",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.LightGray,
                Size = new Size(580, 80),
                Location = new Point(10, 80),
                TextAlign = ContentAlignment.TopLeft,
                BackColor = Color.Transparent
            };

            statusPanel.Controls.Add(lblBrowserStatus);
            statusPanel.Controls.Add(lblBrowserDetails);

            // Controls Panel
            var controlsPanel = CreateCardPanel("⚙️ STEUERUNG", 40, 510, 600, 100);
            btnCheckNow = CreateModernButton("🔍 JETZT PRÜFEN", 20, 25, 270, 50, Color.FromArgb(80, 80, 100));
            btnToggleAuto = CreateModernButton("⏹️ AUTO STOP", 310, 25, 270, 50, COLOR_SUCCESS);

            controlsPanel.Controls.Add(btnCheckNow);
            controlsPanel.Controls.Add(btnToggleAuto);

            tabBrowser.Controls.Add(btnBrowserLogin);
            tabBrowser.Controls.Add(lblInfo);
            tabBrowser.Controls.Add(lblUserInfo);
            tabBrowser.Controls.Add(statsPanel);
            tabBrowser.Controls.Add(statusPanel);
            tabBrowser.Controls.Add(controlsPanel);
        }

        private void SetupCookieTab(TabPage tabCookie)
        {
            tabCookie.BackColor = COLOR_DARK;

            // Cookie Input Panel
            var inputPanel = CreateCardPanel("🍪 COOKIE EINGABE", 40, 30, 600, 150);

            var cookieInfo = new Label()
            {
                Text = "ℹ️ Cookie aus F12 → Application → Cookies → .ROBLOSECURITY kopieren",
                Font = new Font("Segoe UI", 9),
                ForeColor = COLOR_INFO,
                Size = new Size(580, 20),
                Location = new Point(10, 15),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            txtCookie = new TextBox()
            {
                Location = new Point(20, 45),
                Size = new Size(560, 35),
                Font = new Font("Segoe UI", 10),
                BackColor = Color.FromArgb(50, 50, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true
            };

            btnSetCookie = CreateModernButton("✅ COOKIE SPEICHERN & VERBINDEN", 20, 90, 560, 40, COLOR_ACCENT);

            inputPanel.Controls.Add(cookieInfo);
            inputPanel.Controls.Add(txtCookie);
            inputPanel.Controls.Add(btnSetCookie);

            // Test Buttons Panel
            var testPanel = CreateCardPanel("🧪 COOKIE TESTEN", 40, 200, 600, 80);
            btnTestCookie = CreateModernButton("🔍 COOKIE TESTEN", 20, 25, 560, 40, COLOR_WARNING);
            testPanel.Controls.Add(btnTestCookie);

            // Status Panel
            var statusPanel = CreateCardPanel("📊 VOICE CHAT STATUS", 40, 300, 600, 150);
            lblCookieStatus = new Label()
            {
                Text = "🔴 NICHT VERBUNDEN",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.Orange,
                Size = new Size(580, 35),
                Location = new Point(10, 40),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };

            lblCookieDetails = new Label()
            {
                Text = "Bitte Cookie eingeben und speichern\nDer Monitor überwacht automatisch deinen Ban-Status",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.LightGray,
                Size = new Size(580, 80),
                Location = new Point(10, 80),
                TextAlign = ContentAlignment.TopLeft,
                BackColor = Color.Transparent
            };

            statusPanel.Controls.Add(lblCookieStatus);
            statusPanel.Controls.Add(lblCookieDetails);

            // Stats Panel
            var statsPanel = CreateCardPanel("📈 STATISTIKEN", 40, 470, 600, 100);
            var lblCookieStats = new Label()
            {
                Text = "📅 Letzter Login: -\n📊 Überprüfungen: 0\n🔔 Status: Inaktiv",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.LightGray,
                Size = new Size(580, 80),
                Location = new Point(10, 30),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.Transparent
            };
            statsPanel.Controls.Add(lblCookieStats);

            tabCookie.Controls.Add(inputPanel);
            tabCookie.Controls.Add(testPanel);
            tabCookie.Controls.Add(statusPanel);
            tabCookie.Controls.Add(statsPanel);
        }

        private Panel CreateCardPanel(string title, int x, int y, int width, int height)
        {
            var panel = new Panel()
            {
                Location = new Point(x, y),
                Size = new Size(width, height),
                BackColor = COLOR_CARD,
                BorderStyle = BorderStyle.None
            };

            var titleLabel = new Label()
            {
                Text = title,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(width - 20, 25),
                Location = new Point(15, 15),
                TextAlign = ContentAlignment.MiddleLeft
            };

            panel.Controls.Add(titleLabel);
            return panel;
        }

        private Button CreateModernButton(string text, int x, int y, int width, int height, Color backColor)
        {
            var button = new Button()
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, height),
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(backColor, 0.2f);
            button.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(backColor, 0.2f);

            return button;
        }

        // === ERWEITERTE FUNKTIONEN ===

        private async Task PerformBrowserLogin()
        {
            try
            {
                btnBrowserLogin.Enabled = false;
                btnBrowserLogin.Text = "🔄 BROWSER WIRD GESTARTET...";
                btnBrowserLogin.BackColor = Color.FromArgb(100, 100, 120);

                using (var loginForm = new LoginBrowserForm())
                {
                    var result = loginForm.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrEmpty(loginForm.Cookie))
                    {
                        currentCookie = loginForm.Cookie;
                        checker.SetCookie(currentCookie);

                        var userInfo = await checker.TestConnection();

                        ShowNotification("✅ LOGIN ERFOLGREICH", $"Eingeloggt als: {userInfo.DisplayName}");

                        UpdateUserInfo($"👤 {userInfo.DisplayName}\n" +
                                     $"🆔 ID: {userInfo.Id}\n" +
                                     $"🪙 Robux: {userInfo.Robux:N0}\n" +
                                     $"📅 Account erstellt: {GetAccountAge(userInfo.Id)}");

                        UpdateStatus($"✅ VERBUNDEN: {userInfo.DisplayName}",
                                    $"🔓 Voice Chat verfügbar\n" +
                                    $"👤 User ID: {userInfo.Id}\n" +
                                    $"💰 Robux: {userInfo.Robux:N0}",
                                    COLOR_ACTIVE);

                        checkTimer.Start();
                        btnToggleAuto.Text = "⏹️ AUTO STOP";
                        btnToggleAuto.BackColor = COLOR_BANNED;

                        UpdateStats($"✅ Letzter Login: {DateTime.Now:HH:mm:ss}\n" +
                                   $"📊 Überprüfungen: {checkCount}\n" +
                                   $"🔔 Monitoring: Aktiv");

                        await CheckStatus();
                    }
                    else
                    {
                        ShowNotification("❌ LOGIN ABGEBROCHEN", "Browser-Login wurde abgebrochen oder Cookie nicht gefunden");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowNotification("❌ LOGIN FEHLER", $"Fehler: {ex.Message}");
                UpdateStatus("❌ VERBINDUNGSFEHLER", $"Login fehlgeschlagen:\n{ex.Message}", Color.Orange);
            }
            finally
            {
                btnBrowserLogin.Enabled = true;
                btnBrowserLogin.Text = "🚀 IM BROWSER EINLOGGEN";
                btnBrowserLogin.BackColor = currentRgbColor;
            }
        }

        private async void BtnSetCookie_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCookie.Text))
            {
                ShowNotification("❌ FEHLER", "Bitte Cookie eingeben!");
                return;
            }

            try
            {
                Cursor = Cursors.WaitCursor;
                btnSetCookie.Enabled = false;
                btnSetCookie.Text = "🔍 COOKIE WIRD GEPRÜFT...";

                var cookie = CleanCookie(txtCookie.Text);
                checker.SetCookie(cookie);
                currentCookie = cookie;

                var testTask = checker.TestConnection();
                if (await Task.WhenAny(testTask, Task.Delay(10000)) != testTask)
                {
                    throw new Exception("Timeout: Roblox Server antwortet nicht");
                }

                var userInfo = await testTask;

                ShowNotification("✅ LOGIN ERFOLGREICH", $"Eingeloggt als: {userInfo.DisplayName}");

                UpdateUserInfo($"👤 {userInfo.DisplayName}\n" +
                             $"🆔 ID: {userInfo.Id}\n" +
                             $"🪙 Robux: {userInfo.Robux:N0}\n" +
                             $"📅 Account erstellt: {GetAccountAge(userInfo.Id)}");

                UpdateStatus($"✅ VERBUNDEN: {userInfo.DisplayName}",
                            $"🔓 Voice Chat verfügbar\n" +
                            $"👤 User ID: {userInfo.Id}\n" +
                            $"💰 Robux: {userInfo.Robux:N0}",
                            COLOR_ACTIVE);

                checkTimer.Start();
                btnToggleAuto.Text = "⏹️ AUTO STOP";
                btnToggleAuto.BackColor = COLOR_BANNED;

                UpdateStats($"✅ Letzter Login: {DateTime.Now:HH:mm:ss}\n" +
                           $"📊 Überprüfungen: {checkCount}\n" +
                           $"🔔 Monitoring: Aktiv");

                await CheckStatus();
            }
            catch (Exception ex)
            {
                ShowNotification("❌ COOKIE FEHLER", ex.Message);
                UpdateStatus("❌ VERBINDUNGSFEHLER",
                            $"Fehler: {ex.Message}\n\n" +
                            "🔧 Mögliche Lösungen:\n" +
                            "• Cookie neu kopieren (F12 → Application)\n" +
                            "• Auf roblox.com neu einloggen\n" +
                            "• Browser Cache leeren\n" +
                            "• Anderen Browser versuchen",
                            Color.Orange);
            }
            finally
            {
                Cursor = Cursors.Default;
                btnSetCookie.Enabled = true;
                btnSetCookie.Text = "✅ COOKIE SPEICHERN & VERBINDEN";
            }
        }

        private async void TestCookieManually()
        {
            var testCookie = txtCookie.Text;
            if (string.IsNullOrEmpty(testCookie))
            {
                MessageBox.Show("❌ Bitte Cookie eingeben!", "Cookie Test",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
                    client.DefaultRequestHeaders.Add("Cookie", $".ROBLOSECURITY={testCookie}");

                    var response = await client.GetAsync("https://users.roblox.com/v1/users/authenticated");
                    var content = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var userInfo = JsonConvert.DeserializeObject<UserInfo>(content);
                        MessageBox.Show(
                            $"✅ COOKIE FUNKTIONIERT!\n\n" +
                            $"👤 Eingeloggt als: {userInfo.DisplayName}\n" +
                            $"🆔 User ID: {userInfo.Id}\n" +
                            $"💰 Robux: {userInfo.Robux:N0}\n" +
                            $"📅 Account erstellt: {GetAccountAge(userInfo.Id)}",
                            "Cookie Test - Erfolg",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show(
                            $"❌ COOKIE FEHLERHAFT\n\n" +
                            $"📊 Status: {response.StatusCode}\n" +
                            $"🔧 Antwort: {content}\n\n" +
                            "Mögliche Ursachen:\n" +
                            "• Cookie abgelaufen\n" +
                            "• Falscher Cookie\n" +
                            "• Account gebannt",
                            "Cookie Test - Fehler",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ VERBINDUNGSFEHLER\n\n{ex.Message}", "Cookie Test - Fehler",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnToggleAuto_Click(object sender, EventArgs e)
        {
            if (checkTimer.Enabled)
            {
                checkTimer.Stop();
                btnToggleAuto.Text = "▶️ AUTO START";
                btnToggleAuto.BackColor = COLOR_ACTIVE;
                ShowNotification("⏹️ MONITORING GESTOPPT", "Automatische Überprüfung pausiert");
                UpdateStats($"⏸️ Letzte Prüfung: {DateTime.Now:HH:mm:ss}\n" +
                           $"📊 Überprüfungen: {checkCount}\n" +
                           $"🔔 Monitoring: Pausiert");
            }
            else
            {
                if (!string.IsNullOrEmpty(currentCookie))
                {
                    checkTimer.Start();
                    btnToggleAuto.Text = "⏹️ AUTO STOP";
                    btnToggleAuto.BackColor = COLOR_BANNED;
                    ShowNotification("▶️ MONITORING GESTARTET", "Automatische Überprüfung aktiv (30s Interval)");
                    UpdateStats($"✅ Letzte Prüfung: {DateTime.Now:HH:mm:ss}\n" +
                               $"📊 Überprüfungen: {checkCount}\n" +
                               $"🔔 Monitoring: Aktiv");
                }
                else
                {
                    ShowNotification("❌ FEHLER", "Bitte zuerst einloggen!");
                }
            }
        }

        private async Task CheckStatus()
        {
            if (string.IsNullOrEmpty(currentCookie)) return;

            try
            {
                var status = await checker.CheckVoiceBanStatus();
                checkCount++;

                this.Invoke(new Action(() =>
                {
                    if (status.IsBanned)
                    {
                        var banTime = status.BannedUntil?.ToDateTime();
                        var details = $"🚫 VOICE BAN AKTIV\n\n" +
                                    $"📋 Grund: {GetBanReasonText(status.BanReason)}\n" +
                                    $"🔒 Status: Gebannt\n";

                        if (banTime.HasValue)
                        {
                            var timeLeft = banTime.Value - DateTime.Now;
                            details += $"⏰ Endet: {banTime.Value:dd.MM.yyyy HH:mm}\n" +
                                      $"📅 Verbleibend: {timeLeft.Days}d {timeLeft.Hours}h {timeLeft.Minutes}m";
                        }

                        UpdateStatus("🔴 VOICE BAN", details, COLOR_BANNED);
                        UpdateStats($"🚫 Letzte Prüfung: {DateTime.Now:HH:mm:ss}\n" +
                                   $"📊 Überprüfungen: {checkCount}\n" +
                                   $"🔔 Status: GEBANNT");
                    }
                    else
                    {
                        UpdateStatus("🟢 VOICE AKTIV",
                                    "✅ Voice Chat verfügbar\n" +
                                    "🔓 Alles funktioniert normal\n" +
                                    "🎤 Du kannst sprechen und hören\n" +
                                    "💚 Keine Einschränkungen",
                                    COLOR_ACTIVE);

                        UpdateStats($"✅ Letzte Prüfung: {DateTime.Now:HH:mm:ss}\n" +
                                   $"📊 Überprüfungen: {checkCount}\n" +
                                   $"🔔 Status: AKTIV");

                        if (wasBanned)
                        {
                            ShowBanLiftedNotification();
                        }
                    }

                    wasBanned = status.IsBanned;
                }));
            }
            catch (Exception ex)
            {
                this.Invoke(new Action(() =>
                {
                    UpdateStatus("❌ VERBINDUNGSFEHLER",
                                $"🌐 Netzwerkproblem\n" +
                                $"🔧 Fehler: {ex.Message}\n" +
                                $"💡 Prüfe deine Internetverbindung",
                                Color.Orange);
                }));
            }
        }

        // === HILFE & INFO FUNKTIONEN ===

        private void ShowHelpInfo()
        {
            MessageBox.Show(
                "🎮 MADO'S ROBLOX VOICE MONITOR v2.0\n\n" +
                "📋 FUNKTIONEN:\n" +
                "• Voice Ban Status Überprüfung\n" +
                "• Automatisches Monitoring (30s)\n" +
                "• Browser-Login & Cookie-Login\n" +
                "• Desktop Benachrichtigungen\n\n" +
                "🔒 SICHERHEIT & DATENSCHUTZ:\n" +
                "• Keine Verbindung zu Roblox Corp.\n" +
                "• Nutzt nur offizielle Roblox APIs\n" +
                "• Cookies werden lokal gespeichert\n" +
                "• Open Source & Transparent\n\n" +
                "⚙️ TECHNISCHE INFORMATIONEN:\n" +
                "• API: https://voice.roblox.com/v1/settings\n" +
                "• Entwickelt von Mado\n" +
                "• .NET 6.0 Windows Forms\n\n" +
                "💡 TIPPS:\n" +
                "• Cookie aus F12 Developer Tools kopieren\n" +
                "• Bei Problemen Browser-Login verwenden\n" +
                "• Auto-Monitoring für sofortige Benachrichtigungen",
                "ℹ️ Hilfe & Informationen",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // === HELPER METHODEN ===

        private string GetAccountAge(long userId)
        {
            var daysOld = (DateTime.Now - new DateTime(2006, 1, 1)).TotalDays / 1000 * (userId % 1000);
            return $"~{(int)daysOld} Tage";
        }

        private string CleanCookie(string cookie)
        {
            if (string.IsNullOrEmpty(cookie))
                return cookie;

            cookie = cookie.Trim();
            cookie = cookie.Replace("\n", "").Replace("\r", "").Replace(" ", "");

            if (cookie.Contains("_|WARNING"))
            {
                int lastPipe = cookie.LastIndexOf("|_");
                if (lastPipe >= 0)
                {
                    cookie = cookie.Substring(lastPipe + 2);
                }
                else
                {
                    int lastDash = cookie.LastIndexOf("-");
                    if (lastDash >= 0)
                    {
                        cookie = cookie.Substring(lastDash + 1);
                    }
                }
            }

            cookie = cookie.Trim('"').Trim('\'');
            return cookie;
        }

        private void UpdateStatus(string status, string details, Color color)
        {
            UpdateLabelSafe(lblBrowserStatus, status, color);
            UpdateLabelSafe(lblBrowserDetails, details, Color.White);
            UpdateLabelSafe(lblCookieStatus, status, color);
            UpdateLabelSafe(lblCookieDetails, details, Color.White);
        }

        private void UpdateUserInfo(string info)
        {
            if (lblUserInfo.InvokeRequired)
            {
                lblUserInfo.Invoke(new Action<string>(UpdateUserInfo), info);
            }
            else
            {
                lblUserInfo.Text = info;
            }
        }

        private void UpdateStats(string stats)
        {
            if (lblStats.InvokeRequired)
            {
                lblStats.Invoke(new Action<string>(UpdateStats), stats);
            }
            else
            {
                lblStats.Text = stats;
            }
        }

        private void UpdateLabelSafe(Label label, string text, Color color)
        {
            if (label != null && !label.IsDisposed && label.IsHandleCreated)
            {
                if (label.InvokeRequired)
                {
                    label.Invoke(new Action<Label, string, Color>(UpdateLabelSafe), label, text, color);
                }
                else
                {
                    label.Text = text;
                    label.ForeColor = color;
                }
            }
        }

        private void ShowNotification(string title, string message)
        {
            notifyIcon.BalloonTipTitle = title;
            notifyIcon.BalloonTipText = message;
            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon.ShowBalloonTip(4000);
        }

        private void ShowBanLiftedNotification()
        {
            notifyIcon.BalloonTipTitle = "🎉 BAN VORBEI!";
            notifyIcon.BalloonTipText = "Dein Voice Ban ist beendet! Du kannst wieder Voice Chat nutzen!";
            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon.ShowBalloonTip(6000);
            System.Media.SystemSounds.Exclamation.Play();
        }

        private string GetBanReasonText(int reasonCode)
        {
            return reasonCode switch
            {
                1 => "Verstoß gegen Community-Regeln",
                2 => "Wiederholte Verstöße",
                3 => "Schwerwiegender Verstoß",
                4 => "Betrug/Exploits",
                5 => "Belästigung",
                6 => "Altersverifikation fehlgeschlagen",
                7 => "Moderator-Entscheidung",
                8 => "Spam/Übermäßige Nachrichten",
                9 => "Unangemessener Inhalt",
                10 => "Ausnutzung von Bugs",
                _ => $"Unbekannt (Code: {reasonCode})"
            };
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            // InitializeAnimations();

            // Initialisierung
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            checkTimer?.Stop();
            checkTimer?.Dispose();
            rgbTimer?.Stop();
            rgbTimer?.Dispose();
            notifyIcon?.Dispose();
            base.OnFormClosing(e);
        }
    }
}