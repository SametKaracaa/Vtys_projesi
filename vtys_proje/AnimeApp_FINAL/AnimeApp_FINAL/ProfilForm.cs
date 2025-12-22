using AnimeApp.Database;
using AnimeApp.Models;
using AnimeApp.UI;

namespace AnimeApp.Forms
{
    public partial class ProfilForm : Form
    {
        private readonly DatabaseManager db;
        private readonly Kullanici currentUser;
        private Label lblKullaniciAdi;
        private Label lblStats;
        private Button btnKapat;
        private DataGridView dgvFavoriler;
        private DataGridView dgvPuanlar;

        public ProfilForm(DatabaseManager database, Kullanici user)
        {
            db = database;
            currentUser = user;
            InitializeComponent();
            
            // Temayı uygula
            TemaYoneticisi.FormaUygula(this);
            
            LoadUserData();
        }

        private void InitializeComponent()
        {
            this.Text = "Profil";
            this.Size = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Ana Panel
            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Padding = new Padding(20)
            };
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            this.Controls.Add(mainPanel);

            // Sol Panel - Kullanıcı Bilgileri
            var leftPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(15)
            };
            mainPanel.Controls.Add(leftPanel, 0, 0);

            // Başlık
            var lblBaslik = new Label
            {
                Text = "PROFİL BİLGİLERİ",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                Location = new Point(15, 15),
                Size = new Size(280, 30)
            };
            leftPanel.Controls.Add(lblBaslik);

            // Kullanıcı Adı
            lblKullaniciAdi = new Label
            {
                Text = $"Kullanıcı: {currentUser.KullaniciAdi}",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Location = new Point(15, 60),
                Size = new Size(280, 25)
            };
            leftPanel.Controls.Add(lblKullaniciAdi);

            // İstatistikler
            lblStats = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 10),
                Location = new Point(15, 100),
                Size = new Size(280, 200),
                AutoSize = false
            };
            leftPanel.Controls.Add(lblStats);

            // Kapat Butonu
            btnKapat = new Button
            {
                Text = "Kapat",
                Location = new Point(15, 320),
                Size = new Size(280, 40),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnKapat.Click += (s, e) => this.Close();
            leftPanel.Controls.Add(btnKapat);

            // Sağ Panel - Listeler
            var rightPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            mainPanel.Controls.Add(rightPanel, 1, 0);

            // TabControl
            var tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10)
            };
            rightPanel.Controls.Add(tabControl);

            // Favoriler Tab
            var tabFavoriler = new TabPage("Favorilerim");
            dgvFavoriler = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            tabFavoriler.Controls.Add(dgvFavoriler);
            tabControl.TabPages.Add(tabFavoriler);

            // Puanladıklarım Tab
            var tabPuanlar = new TabPage("Puanladıklarım");
            dgvPuanlar = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None
            };
            tabPuanlar.Controls.Add(dgvPuanlar);
            tabControl.TabPages.Add(tabPuanlar);
        }

        private void LoadUserData()
        {
            // İstatistikleri yükle
            var stats = db.GetKullaniciIstatistikleri(currentUser.UserId);
            if (stats != null)
            {
                lblStats.Text = $@"📊 İSTATİSTİKLER

📺 Puanlanan Anime: {stats.PuanlananAnimeSayisi}
⭐ Ortalama Puan: {stats.OrtalamaPuan:F2}
❤️ Favori Sayısı: {stats.FavoriSayisi}
📋 İzleme Listesi: {stats.IzlemeListesiSayisi}

Kayıt Tarihi: {currentUser.DogumTarihi?.ToShortDateString() ?? "Belirtilmemiş"}
Cinsiyet: {currentUser.Cinsiyet ?? "Belirtilmemiş"}";
            }

            // Favorileri yükle
            var favoriler = db.GetFavoriteAnimes(currentUser.UserId);
            dgvFavoriler.DataSource = favoriler.Select(a => new
            {
                ID = a.AnimeId,
                İsim = a.Isim,
                Puan = a.Puan?.ToString("F2") ?? "N/A",
                Tür = a.Tip
            }).ToList();

            // Puanlamaları yükle
            var puanlar = db.GetUserRatedAnimes(currentUser.UserId);
            dgvPuanlar.DataSource = puanlar.Select(a => new
            {
                ID = a.AnimeId,
                İsim = a.Isim,
                Puan = a.Puan?.ToString("F2") ?? "N/A",
                VerilenPuan = db.GetUserPuan(currentUser.UserId, a.AnimeId),
                Yorum = db.GetUserYorum(currentUser.UserId, a.AnimeId) ?? ""
            }).ToList();
            
            // Yorum sütununu geniş yap
            if (dgvPuanlar.Columns["Yorum"] != null)
            {
                dgvPuanlar.Columns["Yorum"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                dgvPuanlar.Columns["Yorum"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            }
        }
    }
}
