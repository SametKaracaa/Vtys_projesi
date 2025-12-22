using AnimeApp.Database;
using AnimeApp.UI;
using BCrypt.Net;

namespace AnimeApp.Forms
{
    public partial class KayitForm : Form
    {
        private readonly DatabaseManager db;
        private TextBox txtKullaniciAdi;
        private TextBox txtSifre;
        private TextBox txtSifreTekrar;
        private ComboBox cmbCinsiyet;
        private DateTimePicker dtpDogumTarihi;
        private Button btnKayit;
        private Button btnIptal;

        public KayitForm(DatabaseManager database)
        {
            db = database;
            InitializeComponent();
            
            // Temayı uygula
            TemaYoneticisi.FormaUygula(this);
        }

        private void InitializeComponent()
        {
            this.txtKullaniciAdi = new TextBox();
            this.txtSifre = new TextBox();
            this.txtSifreTekrar = new TextBox();
            this.cmbCinsiyet = new ComboBox();
            this.dtpDogumTarihi = new DateTimePicker();
            this.btnKayit = new Button();
            this.btnIptal = new Button();

            // Form
            this.ClientSize = new Size(400, 400);
            this.Text = "Yeni Kullanıcı Kaydı";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(240, 240, 240);

            // Başlık
            var lblBaslik = new Label
            {
                Text = "YENİ KULLANICI KAYDI",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                AutoSize = false,
                Size = new Size(350, 30),
                Location = new Point(25, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblBaslik);

            // Kullanıcı Adı
            var lblKullanici = new Label
            {
                Text = "Kullanıcı Adı:",
                Location = new Point(50, 70),
                Size = new Size(100, 20),
                Font = new Font("Segoe UI", 9)
            };
            this.Controls.Add(lblKullanici);

            txtKullaniciAdi.Location = new Point(50, 93);
            txtKullaniciAdi.Size = new Size(300, 25);
            txtKullaniciAdi.Font = new Font("Segoe UI", 10);
            this.Controls.Add(txtKullaniciAdi);

            // Şifre
            var lblSifre = new Label
            {
                Text = "Şifre:",
                Location = new Point(50, 128),
                Size = new Size(100, 20),
                Font = new Font("Segoe UI", 9)
            };
            this.Controls.Add(lblSifre);

            txtSifre.Location = new Point(50, 151);
            txtSifre.Size = new Size(300, 25);
            txtSifre.PasswordChar = '●';
            txtSifre.Font = new Font("Segoe UI", 10);
            this.Controls.Add(txtSifre);

            // Şifre Tekrar
            var lblSifreTekrar = new Label
            {
                Text = "Şifre (Tekrar):",
                Location = new Point(50, 186),
                Size = new Size(100, 20),
                Font = new Font("Segoe UI", 9)
            };
            this.Controls.Add(lblSifreTekrar);

            txtSifreTekrar.Location = new Point(50, 209);
            txtSifreTekrar.Size = new Size(300, 25);
            txtSifreTekrar.PasswordChar = '●';
            txtSifreTekrar.Font = new Font("Segoe UI", 10);
            this.Controls.Add(txtSifreTekrar);

            // Cinsiyet
            var lblCinsiyet = new Label
            {
                Text = "Cinsiyet (Opsiyonel):",
                Location = new Point(50, 244),
                Size = new Size(150, 20),
                Font = new Font("Segoe UI", 9)
            };
            this.Controls.Add(lblCinsiyet);

            cmbCinsiyet.Location = new Point(50, 267);
            cmbCinsiyet.Size = new Size(300, 25);
            cmbCinsiyet.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCinsiyet.Font = new Font("Segoe UI", 10);
            cmbCinsiyet.Items.AddRange(new object[] { "Belirtmek İstemiyorum", "Erkek", "Kadın", "Diğer" });
            cmbCinsiyet.SelectedIndex = 0;
            this.Controls.Add(cmbCinsiyet);

            // Doğum Tarihi
            var lblDogum = new Label
            {
                Text = "Doğum Tarihi (Opsiyonel):",
                Location = new Point(50, 302),
                Size = new Size(180, 20),
                Font = new Font("Segoe UI", 9)
            };
            this.Controls.Add(lblDogum);

            dtpDogumTarihi.Location = new Point(50, 325);
            dtpDogumTarihi.Size = new Size(300, 25);
            dtpDogumTarihi.Font = new Font("Segoe UI", 10);
            dtpDogumTarihi.MaxDate = DateTime.Now.AddYears(-10);
            dtpDogumTarihi.Value = DateTime.Now.AddYears(-20);
            this.Controls.Add(dtpDogumTarihi);

            // Kayıt Butonu
            btnKayit.Text = "Kayıt Ol";
            btnKayit.Location = new Point(50, 360);
            btnKayit.Size = new Size(140, 35);
            btnKayit.BackColor = Color.FromArgb(46, 204, 113);
            btnKayit.ForeColor = Color.White;
            btnKayit.FlatStyle = FlatStyle.Flat;
            btnKayit.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnKayit.Cursor = Cursors.Hand;
            btnKayit.Click += BtnKayit_Click;
            this.Controls.Add(btnKayit);

            // İptal Butonu
            btnIptal.Text = "İptal";
            btnIptal.Location = new Point(210, 360);
            btnIptal.Size = new Size(140, 35);
            btnIptal.BackColor = Color.FromArgb(231, 76, 60);
            btnIptal.ForeColor = Color.White;
            btnIptal.FlatStyle = FlatStyle.Flat;
            btnIptal.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnIptal.Cursor = Cursors.Hand;
            btnIptal.Click += (s, e) => this.Close();
            this.Controls.Add(btnIptal);
        }

        private void BtnKayit_Click(object? sender, EventArgs e)
        {
            var kullaniciAdi = txtKullaniciAdi.Text.Trim();
            var sifre = txtSifre.Text;
            var sifreTekrar = txtSifreTekrar.Text;

            // Validasyonlar
            if (string.IsNullOrEmpty(kullaniciAdi) || string.IsNullOrEmpty(sifre))
            {
                MessageBox.Show("Kullanıcı adı ve şifre boş bırakılamaz!", "Uyarı", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (sifre != sifreTekrar)
            {
                MessageBox.Show("Şifreler eşleşmiyor!", "Uyarı", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSifreTekrar.Clear();
                txtSifreTekrar.Focus();
                return;
            }

            if (kullaniciAdi.Length < 3)
            {
                MessageBox.Show("Kullanıcı adı en az 3 karakter olmalıdır!", "Uyarı", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (sifre.Length < 4)
            {
                MessageBox.Show("Şifre en az 4 karakter olmalıdır!", "Uyarı", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Console.WriteLine("\n=== KAYIT İŞLEMİ BAŞLIYOR ===");
                Console.WriteLine($"Kullanıcı adı: {kullaniciAdi}");
                
                string? cinsiyet = cmbCinsiyet.SelectedIndex == 0 ? null : cmbCinsiyet.Text;
                DateTime? dogumTarihi = dtpDogumTarihi.Value;

                // ÖNEMLİ: Şimdilik BCrypt KULLLANMIYORUZ - test için
                // Çalıştıktan sonra BCrypt ekleyeceğiz
                Console.WriteLine("UYARI: BCrypt devre dışı (test modu)");
                string hashedPassword = sifre; // Şimdilik düz şifre
                
                // BCrypt kullanmak için bu satırı açın:
                // string hashedPassword = BCrypt.Net.BCrypt.HashPassword(sifre);

                Console.WriteLine("Veritabanına kayıt yapılıyor...");
                bool sonuc = db.KayitOl(kullaniciAdi, hashedPassword, cinsiyet, dogumTarihi);
                
                if (sonuc)
                {
                    Console.WriteLine("✅✅✅ KAYIT BAŞARILI! ✅✅✅\n");
                    MessageBox.Show(
                        $"✅ Kayıt başarılı!\n\n" +
                        $"Kullanıcı: {kullaniciAdi}\n\n" +
                        $"Şimdi giriş yapabilirsiniz.", 
                        "Başarılı", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    Console.WriteLine("❌❌❌ KAYIT BAŞARISIZ! ❌❌❌");
                    Console.WriteLine("Konsol'daki hata mesajlarını kontrol edin!\n");
                    MessageBox.Show(
                        "❌ Kayıt başarısız!\n\n" +
                        "KONSOL PENCERESİNİ KONTROL EDİN!\n\n" +
                        "Detaylı hata mesajı orada görünüyor.\n\n" +
                        "Olası sebepler:\n" +
                        "• PostgreSQL çalışmıyor\n" +
                        "• Veritabanı bağlantı hatası\n" +
                        "• Bu kullanıcı adı zaten var\n" +
                        "• Tablo bulunamadı", 
                        "Hata", 
                        MessageBoxButtons.OK, 
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌❌❌ EXCEPTION! ❌❌❌");
                Console.WriteLine($"Hata: {ex.Message}");
                Console.WriteLine($"StackTrace:\n{ex.StackTrace}\n");
                
                MessageBox.Show(
                    $"❌ Beklenmeyen hata!\n\n{ex.Message}\n\n" +
                    $"KONSOL'U KONTROL EDİN!", 
                    "Kritik Hata", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
        }
    }
}
