using AnimeApp.Database;
using AnimeApp.Models;
using AnimeApp.ML;
using AnimeApp.Utilities;

namespace AnimeApp.Forms
{
    // MainForm için yeni özellikleri içeren partial class
    public partial class MainForm
    {
        private Button? btnFavoriler;
        private Button? btnProfil;
        private Button? btnExport;
        private Button? btnOneriler;
        private Button? btnFavoriEkle;

        // Yeni butonları ekle
        private void AddNewFeatureButtons()
        {
            // Bu metodun pnlArama oluşturulduktan sonra çağrılması gerekiyor
            var pnlArama = pnlSol.Controls.OfType<Panel>().FirstOrDefault();
            if (pnlArama == null) return;

            // Favorilerim Butonu
            btnFavoriler = new Button
            {
                Text = "❤️ Favoriler",
                Location = new Point(670, 11),
                Size = new Size(100, 28),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnFavoriler.Click += BtnFavoriler_Click;
            pnlArama.Controls.Add(btnFavoriler);

            // Öneriler Butonu
            btnOneriler = new Button
            {
                Text = "Öneriler",
                Location = new Point(780, 11),
                Size = new Size(100, 28),
                BackColor = Color.FromArgb(241, 196, 15),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnOneriler.Click += BtnOneriler_Click;
            pnlArama.Controls.Add(btnOneriler);

            // Profil butonu üst panele eklenecek
            var pnlUst = this.Controls.OfType<Panel>().FirstOrDefault(p => p.Dock == DockStyle.Top);
            if (pnlUst != null)
            {
                btnProfil = new Button
                {
                    Text = "👤 Profil",
                    Size = new Size(90, 30),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    BackColor = Color.FromArgb(155, 89, 182),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnProfil.Click += BtnProfil_Click;
                pnlUst.Controls.Add(btnProfil);

                // Export butonu
                btnExport = new Button
                {
                    Text = "📊 Export",
                    Size = new Size(90, 30),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    BackColor = Color.FromArgb(46, 204, 113),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Cursor = Cursors.Hand
                };
                btnExport.Click += BtnExport_Click;
                pnlUst.Controls.Add(btnExport);
            }
        }

        // Detay paneline favori ekleme butonu ekle
        private void AddFavoriteButtonToDetailPanel()
        {
            btnFavoriEkle = new Button
            {
                Text = "❤️ Favorilere Ekle",
                Location = new Point(15, 500),
                Size = new Size(370, 35),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Visible = false
            };
            btnFavoriEkle.Click += BtnFavoriEkle_Click;
            pnlDetay.Controls.Add(btnFavoriEkle);
        }

        // Favori ekleme/çıkarma
        private void BtnFavoriEkle_Click(object? sender, EventArgs e)
        {
            if (selectedAnime == null) return;

            if (db.IsFavorite(currentUser.UserId, selectedAnime.AnimeId))
            {
                // Favoriden çıkar
                if (db.FavoriCikar(currentUser.UserId, selectedAnime.AnimeId))
                {
                    MessageBox.Show("Favorilerden çıkarıldı!", "Başarılı",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateFavoriteButton();
                }
            }
            else
            {
                // Favorilere ekle
                if (db.FavoriEkle(currentUser.UserId, selectedAnime.AnimeId))
                {
                    MessageBox.Show("Favorilere eklendi!", "Başarılı",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    UpdateFavoriteButton();
                }
            }
        }

        private void UpdateFavoriteButton()
        {
            if (selectedAnime == null || btnFavoriEkle == null) return;

            if (db.IsFavorite(currentUser.UserId, selectedAnime.AnimeId))
            {
                btnFavoriEkle.Text = "💔 Favorilerden Çıkar";
                btnFavoriEkle.BackColor = Color.FromArgb(189, 195, 199);
            }
            else
            {
                btnFavoriEkle.Text = "❤️ Favorilere Ekle";
                btnFavoriEkle.BackColor = Color.FromArgb(231, 76, 60);
            }
        }

        // Favorileri göster
        private void BtnFavoriler_Click(object? sender, EventArgs e)
        {
            var favoriler = db.GetFavoriteAnimes(currentUser.UserId);
            currentAnimeList = favoriler;
            
            // DataGridView'i güncelle
            dgvAnime.DataSource = null;
            dgvAnime.DataSource = favoriler.Select(a => new
            {
                a.AnimeId,
                Anime = a.Isim,
                İngilizce = a.IngilizceIsim ?? "-",
                Puan = a.Puan.HasValue ? a.Puan.Value.ToString("0.00") : "N/A",
                Bölüm = a.BolumSayisi ?? "-",
                Tip = a.Tip ?? "-"
            }).ToList();
            
            if (dgvAnime.Columns["AnimeId"] != null)
                dgvAnime.Columns["AnimeId"].Visible = false;
            
            lblKullanici.Text = $"Favorilerim ({favoriler.Count} anime)";
        }

        // Profil formunu aç
        private void BtnProfil_Click(object? sender, EventArgs e)
        {
            var profilForm = new ProfilForm(db, currentUser);
            profilForm.ShowDialog();
        }

        // Export menüsü
        private void BtnExport_Click(object? sender, EventArgs e)
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add("Tüm Animeler (Excel)", null, (s, ev) => ExportAllAnimes());
            menu.Items.Add("Puanladıklarım (Excel)", null, (s, ev) => ExportUserRatings());
            menu.Items.Add("Favorilerim (Excel)", null, (s, ev) => ExportFavorites());
            menu.Show(btnExport, new Point(0, btnExport.Height));
        }

        private void ExportAllAnimes()
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = "AnimeListe.xlsx",
                Title = "Anime Listesini Kaydet"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                var data = db.GetAllAnimesForExport();
                if (ExcelManager.ExportToExcel(data, saveDialog.FileName))
                {
                    MessageBox.Show("Excel dosyası başarıyla oluşturuldu!", "Başarılı",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void ExportUserRatings()
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = "Puanlarim.xlsx",
                Title = "Puanlarımı Kaydet"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                if (ExcelManager.ExportUserRatings(db, currentUser.UserId, saveDialog.FileName))
                {
                    MessageBox.Show("Excel dosyası başarıyla oluşturuldu!", "Başarılı",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void ExportFavorites()
        {
            var saveDialog = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = "Favorilerim.xlsx",
                Title = "Favorilerimi Kaydet"
            };

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                if (ExcelManager.ExportFavorites(db, currentUser.UserId, saveDialog.FileName))
                {
                    MessageBox.Show("Excel dosyası başarıyla oluşturuldu!", "Başarılı",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        // Öneriler formunu aç
        private void BtnOneriler_Click(object? sender, EventArgs e)
        {
            var onerilerForm = new OnerilerForm(db, currentUser);
            onerilerForm.ShowDialog();
        }
    }
}
