using AnimeApp.Database;
using AnimeApp.Models;
using AnimeApp.UI;

namespace AnimeApp.Forms
{
    public partial class AdminForm : Form
    {
        private readonly DatabaseManager db;
        private DataGridView dgvAnime;
        private Button btnEkle;
        private Button btnDuzenle;
        private Button btnSil;
        private Button btnKapat;
        private Label lblIstatistik;

        public AdminForm(DatabaseManager database)
        {
            db = database;
            InitializeComponent();
            
            // Temayı uygula
            TemaYoneticisi.FormaUygula(this);
            
            LoadData();
        }

        private void InitializeComponent()
        {
            this.ClientSize = new Size(900, 600);
            this.Text = "Admin Paneli";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Başlık
            var lblBaslik = new Label
            {
                Text = "ADMİN PANELİ - ANİME YÖNETİMİ",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(192, 57, 43),
                Location = new Point(20, 20),
                Size = new Size(860, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblBaslik);

            // İstatistikler
            lblIstatistik = new Label
            {
                Location = new Point(20, 60),
                Size = new Size(860, 25),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(44, 62, 80),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblIstatistik);

            // DataGridView
            dgvAnime = new DataGridView
            {
                Location = new Point(20, 100),
                Size = new Size(860, 420),
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            this.Controls.Add(dgvAnime);

            // Butonlar
            var btnPanel = new Panel
            {
                Location = new Point(20, 530),
                Size = new Size(860, 50),
                BackColor = Color.FromArgb(236, 240, 241)
            };
            this.Controls.Add(btnPanel);

            btnEkle = new Button
            {
                Text = "Yeni Anime Ekle",
                Location = new Point(20, 10),
                Size = new Size(150, 30),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnEkle.Click += BtnEkle_Click;
            btnPanel.Controls.Add(btnEkle);

            btnDuzenle = new Button
            {
                Text = "Düzenle",
                Location = new Point(190, 10),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnDuzenle.Click += BtnDuzenle_Click;
            btnPanel.Controls.Add(btnDuzenle);

            btnSil = new Button
            {
                Text = "Sil",
                Location = new Point(330, 10),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSil.Click += BtnSil_Click;
            btnPanel.Controls.Add(btnSil);

            btnKapat = new Button
            {
                Text = "Kapat",
                Location = new Point(730, 10),
                Size = new Size(110, 30),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnKapat.Click += (s, e) => this.Close();
            btnPanel.Controls.Add(btnKapat);
        }

        private void LoadData()
        {
            var stats = db.GetStatistics();
            lblIstatistik.Text = $"📊 Toplam: {stats["ToplamAnime"]} Anime | {stats["ToplamKullanici"]} Kullanıcı | {stats["ToplamPuanlama"]} Puanlama";

            var animeList = db.GetAnimeList();
            dgvAnime.DataSource = null;
            dgvAnime.Columns.Clear();

            var bindingList = animeList.Select(a => new
            {
                ID = a.AnimeId,
                Anime = a.Isim,
                İngilizce = a.IngilizceIsim ?? "-",
                Puan = a.Puan?.ToString("0.00") ?? "N/A",
                Bölüm = a.BolumSayisi ?? "-",
                Tip = a.Tip ?? "-",
                Yayın = a.YayinTarihi ?? "-"
            }).ToList();

            dgvAnime.DataSource = bindingList;
        }

        private void BtnEkle_Click(object? sender, EventArgs e)
        {
            using var animeForm = new AnimeEditForm(db);
            if (animeForm.ShowDialog() == DialogResult.OK)
            {
                LoadData();
            }
        }

        private void BtnDuzenle_Click(object? sender, EventArgs e)
        {
            if (dgvAnime.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen düzenlemek için bir anime seçin!", "Uyarı", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var animeId = (int)dgvAnime.SelectedRows[0].Cells["ID"].Value;
            var anime = db.GetAnimeList().FirstOrDefault(a => a.AnimeId == animeId);

            if (anime != null)
            {
                using var animeForm = new AnimeEditForm(db, anime);
                if (animeForm.ShowDialog() == DialogResult.OK)
                {
                    LoadData();
                }
            }
        }

        private void BtnSil_Click(object? sender, EventArgs e)
        {
            if (dgvAnime.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen silmek için bir anime seçin!", "Uyarı", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var animeId = (int)dgvAnime.SelectedRows[0].Cells["ID"].Value;
            var animeName = dgvAnime.SelectedRows[0].Cells["Anime"].Value.ToString();

            var result = MessageBox.Show(
                $"'{animeName}' anime'sini silmek istediğinizden emin misiniz?\n\nBu işlem geri alınamaz!",
                "Silme Onayı",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                if (db.DeleteAnime(animeId))
                {
                    MessageBox.Show("Anime başarıyla silindi!", "Başarılı", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                }
                else
                {
                    MessageBox.Show("Anime silinemedi!", "Hata", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
