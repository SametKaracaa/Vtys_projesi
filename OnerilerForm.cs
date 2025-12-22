using AnimeApp.Database;
using AnimeApp.Models;
using AnimeApp.ML;
using AnimeApp.UI;

namespace AnimeApp.Forms
{
    public partial class OnerilerForm : Form
    {
        private readonly DatabaseManager db;
        private readonly Kullanici currentUser;
        private readonly AnimeRecommendationEngine recommendationEngine;
        private DataGridView dgvOneriler;
        private Button btnYenile;
        private Button btnKapat;
        private Label lblDurum;
        private PictureBox picAnime;
        private Panel pnlDetay;

        public OnerilerForm(DatabaseManager database, Kullanici user)
        {
            db = database;
            currentUser = user;
            recommendationEngine = new AnimeRecommendationEngine(db);
            InitializeComponent();
            
            // Temayı uygula
            TemaYoneticisi.FormaUygula(this);
        }

        private void InitializeComponent()
        {
            this.Text = "Size Özel Anime Önerileri";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Başlık
            var lblBaslik = new Label
            {
                Text = "SİZE ÖZEL ANİME ÖNERİLERİ",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                Location = new Point(20, 20),
                AutoSize = true
            };
            this.Controls.Add(lblBaslik);

            // Durum label
            lblDurum = new Label
            {
                Text = "💡 Öneriler yükleniyor...",
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 60),
                Size = new Size(940, 25)
            };
            this.Controls.Add(lblDurum);

            // Sol Panel - Öneri Listesi
            var pnlSol = new Panel
            {
                Location = new Point(20, 100),
                Size = new Size(600, 500),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            this.Controls.Add(pnlSol);

            // DataGridView
            dgvOneriler = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false
            };
            dgvOneriler.SelectionChanged += async (s, e) => await DgvOneriler_SelectionChangedAsync(s, e);
            pnlSol.Controls.Add(dgvOneriler);

            // Sağ Panel - Detay
            pnlDetay = new Panel
            {
                Location = new Point(640, 100),
                Size = new Size(320, 500),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(15),
                Visible = false
            };
            this.Controls.Add(pnlDetay);

            // Anime Resmi
            picAnime = new PictureBox
            {
                Location = new Point(15, 15),
                Size = new Size(290, 400),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlDetay.Controls.Add(picAnime);

            // Butonlar
            var btnPanel = new Panel
            {
                Location = new Point(20, 610),
                Size = new Size(940, 50)
            };
            this.Controls.Add(btnPanel);

            btnYenile = new Button
            {
                Text = "🔄 Yenile",
                Location = new Point(0, 0),
                Size = new Size(180, 40),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnYenile.Click += async (s, e) => await BtnYenile_ClickAsync(s, e);
            btnPanel.Controls.Add(btnYenile);

            btnKapat = new Button
            {
                Text = "Kapat",
                Location = new Point(790, 0),
                Size = new Size(150, 40),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnKapat.Click += (s, e) => this.Close();
            btnPanel.Controls.Add(btnKapat);

            // Form yüklenince önerileri getir
            this.Load += async (s, e) => await LoadRecommendationsAsync();
        }

        private async Task LoadRecommendationsAsync(bool randomize = false)
        {
            try
            {
                btnYenile.Enabled = false;

                // HIZLI hesaplama - arka planda
                var recommendations = await Task.Run(() => 
                    recommendationEngine.GetRecommendations(currentUser.UserId, 10, randomize)
                );

                btnYenile.Enabled = true;

                if (recommendations.Count == 0)
                {
                    lblDurum.Text = "⚠️ Henüz öneri yok. Daha fazla anime puanlayın!";
                    lblDurum.ForeColor = Color.FromArgb(220, 53, 69);
                    ShowEmptyMessage();
                    return;
                }

                // Veriyi UI'ye aktar
                dgvOneriler.DataSource = recommendations.Select((r, index) => new
                {
                    Sıra = index + 1,
                    AnimeId = r.Anime.AnimeId,
                    Anime = r.Anime.Isim,
                    Puan = r.Anime.Puan?.ToString("F2") ?? "N/A",
                    TahminPuan = r.TahminPuan.ToString("F2"),
                    Neden = r.OneriNedeni
                }).ToList();

                string message = randomize 
                    ? $"🎲 {recommendations.Count} yeni öneri!" 
                    : $"✅ {recommendations.Count} öneri bulundu!";
                    
                lblDurum.Text = message;
                lblDurum.ForeColor = Color.FromArgb(25, 135, 84);

                if (dgvOneriler.Rows.Count > 0)
                {
                    dgvOneriler.Rows[0].Selected = true;
                }
            }
            catch (Exception ex)
            {
                btnYenile.Enabled = true;
                lblDurum.Text = $"❌ Hata: {ex.Message}";
                lblDurum.ForeColor = Color.FromArgb(220, 53, 69);
                MessageBox.Show($"Öneriler yüklenirken hata oluştu:\n\n{ex.Message}", 
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowEmptyMessage()
        {
            var emptyLabel = new Label
            {
                Text = "📊 ÖNERİ ALMAK İÇİN:\n\n" +
                       "• En az 5-10 anime puanlayın\n" +
                       "• Favorilere 2-3 anime ekleyin\n" +
                       "• 'Yenile' butonuna tıklayın\n\n" +
                       "💡 Daha fazla puan verirseniz\n" +
                       "daha iyi öneriler alırsınız!",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.Gray,
                AutoSize = false,
                Size = new Size(580, 480),
                Location = new Point(10, 10),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.White
            };
            
            dgvOneriler.Parent?.Controls.Add(emptyLabel);
            emptyLabel.BringToFront();
        }

        private async Task BtnYenile_ClickAsync(object? sender, EventArgs e)
        {
            lblDurum.Text = "🔄 Farklı öneriler getiriliyor...";
            lblDurum.ForeColor = Color.FromArgb(255, 193, 7);
            btnYenile.Enabled = false;

            await Task.Run(() =>
            {
                // Cache'i temizle ve modeli yeniden eğit
                AnimeRecommendationEngine.ClearCache();
                recommendationEngine.TrainModel();
            });

            // Önerileri RASTGELE olarak yeniden yükle
            await LoadRecommendationsAsync(randomize: true);
        }

        private async Task DgvOneriler_SelectionChangedAsync(object? sender, EventArgs e)
        {
            if (dgvOneriler.SelectedRows.Count == 0) return;

            try
            {
                var selectedRow = dgvOneriler.SelectedRows[0];
                var animeId = Convert.ToInt32(selectedRow.Cells["AnimeId"].Value);

                // Anime bilgisini al
                var anime = await Task.Run(() => 
                    db.GetAnimeList().FirstOrDefault(a => a.AnimeId == animeId)
                );

                if (anime == null) return;

                pnlDetay.Visible = true;

                // Gerekli kütüphane: using System.Net.Http;

                if (!string.IsNullOrEmpty(anime.ResimUrl))
                {
                    picAnime.Image = null;
                    picAnime.BackColor = Color.LightGray;

                    try
                    {
                        // HttpClient ile veriyi asenkron olarak çekip bekliyoruz (await edilebilir)
                        using (var client = new HttpClient())
                        {
                            var stream = await client.GetStreamAsync(anime.ResimUrl);
                            picAnime.Image = Image.FromStream(stream);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Resim yükleme hatası: {ex.Message}");
                        picAnime.Image = null;
                        picAnime.BackColor = Color.White;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Seçim hatası: {ex.Message}");
            }
        }
    }
}
