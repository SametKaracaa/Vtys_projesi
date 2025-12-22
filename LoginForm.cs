using AnimeApp.Database;
using AnimeApp.Models;
using AnimeApp.UI;
using System.Reflection;

namespace AnimeApp.Forms
{
    public partial class LoginForm : Form
    {
        private readonly DatabaseManager db;
        private TextBox txtKullaniciAdi;
        private TextBox txtSifre;

        // Görsel üstüne tıklama alanı için şeffaf overlay paneller
        private ClickableOverlayPanel btnGirisYapPanel;
        private ClickableOverlayPanel btnKayitOlPanel;
        private ClickableOverlayPanel btnAdminGirisiPanel;

        // Parşömen input alanları (panel + borderless textbox)
        private ParchmentInputPanel pnlKullanici;
        private ParchmentInputPanel pnlSifre;
        
        // Label'lar ve alt çizgileri
        private Label lblKullaniciText;
        private Label lblSifreText;
        private Panel lineKullanici;
        private Panel lineSifre;

        // Kaynak arka plan görselinin (login_background.png) piksel boyutu
        private const int BgW = 2000;
        private const int BgH = 1068;

        // Butonların kaynak görsel üzerindeki hedef alanları (piksel). Bu değerler görsel analiz ile doğru olarak ayarlanmıştır.
        // Amaç: Form üzerinde BackgroundImageLayout=Zoom ile çizilen görselin üzerine birebir oturtmak.
        private static readonly Rectangle BgRectGiris = new Rectangle(473, 898, 341, 133);
        private static readonly Rectangle BgRectKayit = new Rectangle(859, 898, 341, 133);
        private static readonly Rectangle BgRectAdmin = new Rectangle(1245, 898, 341, 133);

        // Input alanlarının (TextBox) kaynak görsel üzerindeki hedef alanları (piksel)
        // Not: Bu alanlar "Kullanıcı" ve "Şifre" parşömen şeritlerinin tam üstüne oturtulmuştur.
        private static readonly Rectangle BgRectUserBox = new Rectangle(707, 495, 585, 52);
        private static readonly Rectangle BgRectPassBox = new Rectangle(707, 586, 585, 52);

        public Kullanici? LoggedInUser { get; private set; }
        public bool IsAdminLogin { get; private set; }

        public LoginForm(DatabaseManager database)
        {
            db = database;
            InitializeComponent();
            
            // Not: Login ekranı özel tasarımlı olduğu için tema uygulamıyoruz
            // İsterseniz burayı açabilirsiniz:
            // TemaYoneticisi.FormaUygula(this);
        }

        private void InitializeComponent()
        {
            // Form ayarları
            // Not: Arka plan görseli yüksek çözünürlüklü olduğu için formu 16:9'e yakın tuttuk.
            // BackgroundImageLayout = Zoom ile görsel oranı bozulmaz; tıklama alanlarını
            // arka planın çizildiği gerçek dikdörtgene göre konumlandırıyoruz.
            this.Size = new Size(1536, 864);
            this.Text = "Anime Veritabanı";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.DoubleBuffered = true;

            // Daha akıcı çizim
            this.DoubleBuffered = true;

            // Arka plan görselini EMBEDDED RESOURCE'dan yükle
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "AnimeApp.login_background.png";
                
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        this.BackgroundImage = Image.FromStream(stream);
                        this.BackgroundImageLayout = ImageLayout.Zoom;
                    }
                    else
                    {
                        // Resource bulunamazsa mavi arka plan
                        this.BackColor = Color.FromArgb(41, 128, 185);
                    }
                }
            }
            catch
            {
                this.BackColor = Color.FromArgb(41, 128, 185);
            }

            // "Kullanıcı" label ve alt çizgisi
            lblKullaniciText = new Label
            {
                Text = "Kullanıcı",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(70, 40, 20),
                BackColor = Color.Transparent,
                AutoSize = true
            };
            this.Controls.Add(lblKullaniciText);

            lineKullanici = new Panel
            {
                BackColor = Color.FromArgb(180, 70, 40, 20),
                Height = 1
            };
            this.Controls.Add(lineKullanici);

            // "Şifre" label ve alt çizgisi
            lblSifreText = new Label
            {
                Text = "Şifre",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(70, 40, 20),
                BackColor = Color.Transparent,
                AutoSize = true
            };
            this.Controls.Add(lblSifreText);

            lineSifre = new Panel
            {
                BackColor = Color.FromArgb(180, 70, 40, 20),
                Height = 1
            };
            this.Controls.Add(lineSifre);

            // Parşömen input alanları: Borderless TextBox + çizilmiş ince çerçeve
            // Bu yaklaşım standart TextBox'ın şeffaflık limitlerini aşmak için kullanılır.
            pnlKullanici = new ParchmentInputPanel();
            txtKullaniciAdi = pnlKullanici.InnerTextBox;
            txtKullaniciAdi.Font = new Font("Segoe UI", 14);
            this.Controls.Add(pnlKullanici);

            pnlSifre = new ParchmentInputPanel();
            txtSifre = pnlSifre.InnerTextBox;
            txtSifre.Font = new Font("Segoe UI", 14);
            txtSifre.PasswordChar = '●';
            txtSifre.KeyPress += TxtSifre_KeyPress;
            this.Controls.Add(pnlSifre);

            // "Giriş Yap" butonu (Sol mavi buton üzerine) - konumu PositionClickableAreas() ile ayarlanır
            btnGirisYapPanel = new ClickableOverlayPanel
            {
                Location = new Point(0, 0),
                Size = new Size(10, 10),
                Cursor = Cursors.Hand
            };
            btnGirisYapPanel.Click += BtnGirisYap_Click;
            this.Controls.Add(btnGirisYapPanel);

            // "Kayıt Ol" butonu (Ortadaki yeşil buton üzerine) - konumu PositionClickableAreas() ile ayarlanır
            btnKayitOlPanel = new ClickableOverlayPanel
            {
                Location = new Point(0, 0),
                Size = new Size(10, 10),
                Cursor = Cursors.Hand
            };
            btnKayitOlPanel.Click += BtnKayitOl_Click;
            this.Controls.Add(btnKayitOlPanel);

            // "Admin Girişi" butonu (Sağdaki mor buton üzerine) - konumu PositionClickableAreas() ile ayarlanır
            btnAdminGirisiPanel = new ClickableOverlayPanel
            {
                Location = new Point(0, 0),
                Size = new Size(10, 10),
                Cursor = Cursors.Hand
            };
            btnAdminGirisiPanel.Click += BtnAdminGirisi_Click;
            this.Controls.Add(btnAdminGirisiPanel);

            // İlk çizimde tıklama alanlarını görselin üstüne oturt
            this.Shown += (s, e) =>
            {
                PositionClickableAreas();
                txtKullaniciAdi.Focus();
            };

            // (İleride boyut değişirse) hizalamayı koru
            this.Resize += (s, e) => PositionClickableAreas();
        }

        /// <summary>
        /// BackgroundImageLayout=Zoom iken arka planın form üzerinde çizildiği gerçek dikdörtgeni hesaplar.
        /// </summary>
        private Rectangle GetZoomedImageRectangle()
        {
            if (this.BackgroundImage == null)
            {
                return this.ClientRectangle;
            }

            // Zoom: oran korunur, boşluklar yanlarda veya üst-alt oluşabilir.
            float imgAspect = (float)this.BackgroundImage.Width / this.BackgroundImage.Height;
            float clientAspect = (float)this.ClientSize.Width / this.ClientSize.Height;

            int drawW, drawH, offsetX, offsetY;
            if (clientAspect > imgAspect)
            {
                // Client daha geniş: yükseklik tam, genişlik oranla
                drawH = this.ClientSize.Height;
                drawW = (int)Math.Round(drawH * imgAspect);
                offsetX = (this.ClientSize.Width - drawW) / 2;
                offsetY = 0;
            }
            else
            {
                // Client daha dar: genişlik tam, yükseklik oranla
                drawW = this.ClientSize.Width;
                drawH = (int)Math.Round(drawW / imgAspect);
                offsetX = 0;
                offsetY = (this.ClientSize.Height - drawH) / 2;
            }

            return new Rectangle(offsetX, offsetY, drawW, drawH);
        }

        /// <summary>
        /// Paylaşılan görseldeki butonların piksel alanlarını, form üzerindeki zoomlanmış görsel alanına ölçekler.
        /// Böylece buton tıklama alanları görselin üstüne birebir oturur.
        /// </summary>
        private void PositionClickableAreas()
        {
            Rectangle drawRect = GetZoomedImageRectangle();
            float sx = (float)drawRect.Width / BgW;
            float sy = (float)drawRect.Height / BgH;

            Rectangle Map(Rectangle src)
            {
                int x = drawRect.Left + (int)Math.Round(src.X * sx);
                int y = drawRect.Top + (int)Math.Round(src.Y * sy);
                int w = (int)Math.Round(src.Width * sx);
                int h = (int)Math.Round(src.Height * sy);
                return new Rectangle(x, y, w, h);
            }

            btnGirisYapPanel.Bounds = Map(BgRectGiris);
            btnKayitOlPanel.Bounds = Map(BgRectKayit);
            btnAdminGirisiPanel.Bounds = Map(BgRectAdmin);

            // Input alanlarını da arka planın üstüne birebir oturt
            pnlKullanici.Bounds = Map(BgRectUserBox);
            pnlSifre.Bounds = Map(BgRectPassBox);

            // Label'ları ve çizgileri konumlandır
            // "Kullanıcı" label'ı input kutusunun üstünde
            var userLabelRect = Map(new Rectangle(707, 463, 195, 24));
            lblKullaniciText.Location = new Point(userLabelRect.X, userLabelRect.Y);
            
            // "Kullanıcı" label'ının altında çizgi
            var userLineRect = Map(new Rectangle(707, 487, 585, 1));
            lineKullanici.Bounds = userLineRect;

            // "Şifre" label'ı input kutusunun üstünde
            var passLabelRect = Map(new Rectangle(707, 554, 195, 24));
            lblSifreText.Location = new Point(passLabelRect.X, passLabelRect.Y);
            
            // "Şifre" label'ının altında çizgi
            var passLineRect = Map(new Rectangle(707, 578, 585, 1));
            lineSifre.Bounds = passLineRect;

            // Z-index: Label'lar ve çizgiler en önde
            lblKullaniciText.BringToFront();
            lineKullanici.BringToFront();
            lblSifreText.BringToFront();
            lineSifre.BringToFront();
            pnlKullanici.BringToFront();
            pnlSifre.BringToFront();
            btnGirisYapPanel.BringToFront();
            btnKayitOlPanel.BringToFront();
            btnAdminGirisiPanel.BringToFront();
        }

        private void BtnGirisYap_Click(object? sender, EventArgs e)
        {
            IsAdminLogin = false;
            PerformLogin();
        }

        private void BtnAdminGirisi_Click(object? sender, EventArgs e)
        {
            IsAdminLogin = true;
            PerformLogin();
        }

        private void PerformLogin()
        {
            string kullaniciAdi = txtKullaniciAdi.Text.Trim();
            string sifre = txtSifre.Text;

            if (string.IsNullOrEmpty(kullaniciAdi) || string.IsNullOrEmpty(sifre))
            {
                MessageBox.Show("Lütfen kullanıcı adı ve şifre girin!", "Uyarı",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var kullanici = db.Login(kullaniciAdi, sifre);

            if (kullanici != null)
            {
                // Admin kontrolü
                if (IsAdminLogin && kullanici.Rol != "ADMIN")
                {
                    MessageBox.Show("Bu hesap admin değil!", "Hata",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!IsAdminLogin && kullanici.Rol == "ADMIN")
                {
                    MessageBox.Show("Admin hesabı için 'Admin Girişi' butonunu kullanın!",
                        "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                LoggedInUser = kullanici;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("Kullanıcı adı veya şifre hatalı!", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnKayitOl_Click(object? sender, EventArgs e)
        {
            var kayitForm = new KayitForm(db);
            if (kayitForm.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show("Kayıt başarılı! Şimdi giriş yapabilirsiniz.", "Başarılı",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void TxtSifre_KeyPress(object? sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                BtnGirisYap_Click(sender, e);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.BackgroundImage != null)
                {
                    this.BackgroundImage.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Arka plan görselinin üstüne tıklama alanı koymak için kullanılan gerçek şeffaf overlay panel.
    /// Standart Panel, BackgroundImage üzerinde bazen gri blok gibi görünebildiği için özel kontrol kullanıyoruz.
    /// </summary>
    internal sealed class ClickableOverlayPanel : Panel
    {
        public ClickableOverlayPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.SupportsTransparentBackColor, true);
            BackColor = Color.Transparent;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                // Gerçek şeffaf çizim davranışı
                cp.ExStyle |= 0x00000020; // WS_EX_TRANSPARENT
                return cp;
            }
        }
    }

    /// <summary>
    /// Parşömen şeridinin üstüne oturan, ince çerçeveli ve daha doğal görünümlü input paneli.
    /// İçinde borderless TextBox bulunur.
    /// </summary>
    internal sealed class ParchmentInputPanel : Panel
    {
        public TextBox InnerTextBox { get; }

        public ParchmentInputPanel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.SupportsTransparentBackColor, true);

            BackColor = Color.Transparent;

            InnerTextBox = new TextBox
            {
                BorderStyle = BorderStyle.None,
                BackColor = Color.FromArgb(246, 236, 214),
                ForeColor = Color.FromArgb(92, 62, 33),
                Font = new Font("Segoe UI", 14),
            };

            Controls.Add(InnerTextBox);
            Padding = new Padding(14, 12, 14, 12);
            UpdateInnerBounds();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateInnerBounds();
        }

        private void UpdateInnerBounds()
        {
            // TextBox yüksekliği fonta göre; panel içini doldursun
            InnerTextBox.Location = new Point(Padding.Left, Padding.Top);
            InnerTextBox.Width = Math.Max(10, Width - Padding.Left - Padding.Right);
            InnerTextBox.Height = Math.Max(10, Height - Padding.Top - Padding.Bottom);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // Hafif yarı saydam dolgu + ince çerçeve (textbox'un bembeyaz blok gibi görünmesini engeller)
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            int radius = Math.Min(14, Math.Min(Width, Height) / 3);

            using var path = RoundedRect(rect, radius);
            using var fill = new SolidBrush(Color.FromArgb(70, 255, 255, 255));
            using var border = new Pen(Color.FromArgb(150, 90, 60, 30), 1);

            g.FillPath(fill, path);
            g.DrawPath(border, path);
        }

        private static System.Drawing.Drawing2D.GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
