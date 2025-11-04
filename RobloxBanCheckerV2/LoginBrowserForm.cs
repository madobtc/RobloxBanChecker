using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RobloxBanCheckerV2
{
    public partial class LoginBrowserForm : Form
    {
        private WebView2 webView;
        private Button btnClose;
        private Label lblStatus;
        private System.Windows.Forms.Timer cookieCheckTimer;
        private string extractedCookie = "";

        public string Cookie => extractedCookie;

        public LoginBrowserForm()
        {
            InitializeComponent1();
            InitializeWebView();
            InitializeTimer();
        }
        private void InitializeComponent1()
        {
            this.SuspendLayout();

            // Form
            this.Text = "🔐 Roblox Login";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 46);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Status Label
            lblStatus = new Label()
            {
                Text = "📡 Lade Roblox Login...",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                Size = new Size(780, 30),
                Location = new Point(10, 10),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Close Button
            btnClose = new Button()
            {
                Text = "❌ Abbrechen",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(220, 53, 69),
                Size = new Size(120, 35),
                Location = new Point(330, 520),
                FlatStyle = FlatStyle.Flat
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.Add(lblStatus);
            this.Controls.Add(btnClose);

            this.ResumeLayout(false);
        }

        private void InitializeTimer()
        {
            cookieCheckTimer = new System.Windows.Forms.Timer();
            cookieCheckTimer.Interval = 1000; // 1 Sekunde
            cookieCheckTimer.Tick += async (s, e) => await CheckForRobloxCookie();
        }

        private async void InitializeWebView()
        {
            try
            {
                webView = new WebView2()
                {
                    Location = new Point(10, 50),
                    Size = new Size(780, 460),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
                };

                this.Controls.Add(webView);

                // WebView2 Environment initialisieren
                var environment = await CoreWebView2Environment.CreateAsync(null, "WebView2Cache");
                await webView.EnsureCoreWebView2Async(environment);

                // Events
                webView.NavigationStarting += WebView_NavigationStarting;
                webView.NavigationCompleted += WebView_NavigationCompleted;
                webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;

                // Roblox Login Seite laden
                webView.Source = new Uri("https://www.roblox.com/login");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"WebView2 Fehler: {ex.Message}\n\nStelle sicher dass WebView2 installiert ist.", "Fehler",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
        }

        private void WebView_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                // DevTools für Debugging aktivieren
                webView.CoreWebView2.Settings.AreDevToolsEnabled = true;

                // Starte Timer für Cookie-Check
                cookieCheckTimer.Start();

                // Event für Navigation um Cookie-Check zu triggern
                webView.CoreWebView2.NavigationCompleted += (s, args) =>
                {
                    _ = CheckForRobloxCookie();
                };
            }
        }

        private async Task CheckForRobloxCookie()
        {
            try
            {
                if (webView?.CoreWebView2?.CookieManager == null)
                    return;

                var cookies = await webView.CoreWebView2.CookieManager.GetCookiesAsync("https://www.roblox.com");

                foreach (var cookie in cookies)
                {
                    if (cookie.Name == ".ROBLOSECURITY" && !string.IsNullOrEmpty(cookie.Value))
                    {
                        extractedCookie = cookie.Value;

                        // Cookie gefunden - schließe das Form
                        this.Invoke(new Action(() =>
                        {
                            cookieCheckTimer.Stop();
                            lblStatus.Text = "✅ Login erfolgreich! Schließe...";
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }));
                        break;
                    }
                }

                // Zusätzlich: Prüfe ob wir auf der Hauptseite sind (erfolgreicher Login)
                if (webView.Source != null &&
                    (webView.Source.AbsoluteUri.Contains("roblox.com/home") ||
                     webView.Source.AbsoluteUri.Contains("roblox.com/games") ||
                     webView.Source.AbsoluteUri.Contains("roblox.com/discover")))
                {
                    // Auf Hauptseite aber kein Cookie gefunden? Nochmal versuchen
                    await Task.Delay(1000);
                    await CheckForRobloxCookie();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cookie Check Fehler: {ex.Message}");
            }
        }

        private void WebView_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            this.Invoke(new Action(() =>
            {
                lblStatus.Text = "🌐 Lade Seite...";
            }));
        }

        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            this.Invoke(new Action(() =>
            {
                if (webView.Source.AbsoluteUri.Contains("roblox.com/home") ||
                    webView.Source.AbsoluteUri.Contains("roblox.com/games") ||
                    webView.Source.AbsoluteUri.Contains("roblox.com/discover"))
                {
                    lblStatus.Text = "🔍 Überprüfe Login...";
                    // Auf der Hauptseite angekommen - Cookie suchen
                    _ = CheckForRobloxCookie();
                }
                else if (webView.Source.AbsoluteUri.Contains("roblox.com/login"))
                {
                    lblStatus.Text = "🔐 Bitte einloggen...";
                }
                else if (webView.Source.AbsoluteUri.Contains("roblox.com/twostepverification"))
                {
                    lblStatus.Text = "🔒 2FA erforderlich...";
                }
                else
                {
                    lblStatus.Text = $"🌐 {webView.Source.Host}...";
                }
            }));
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            cookieCheckTimer?.Stop();
            cookieCheckTimer?.Dispose();
            webView?.Dispose();
            base.OnFormClosing(e);
        }
        private void LoginBrowserForm_Load(object sender, EventArgs e)
        {

        }
    }
}
