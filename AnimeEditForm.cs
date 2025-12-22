using AnimeApp.Database;
using AnimeApp.Models;
using AnimeApp.UI;

namespace AnimeApp.Forms
{
    public partial class AnimeEditForm : Form
    {
        private readonly DatabaseManager db;
        private readonly Anime? existingAnime;
        private readonly bool isEditMode;

        private TextBox txtId;
        private TextBox txtIsim;
        private TextBox txtIngilizce;
        private TextBox txtBolum;
        private TextBox txtTip;
        private TextBox txtYayin;
        private TextBox txtResimUrl;
        private CheckedListBox clbTurler;
        private Button btnKaydet;
        private Button btnIptal;

        public AnimeEditForm(DatabaseManager database, Anime? anime = null)
        {
            db = database;
            existingAnime = anime;
            isEditMode = anime != null;
            InitializeComponent();
            
            // Temayı uygula
            TemaYoneticisi.FormaUygula(this);
            
            LoadData();
        }

        private void InitializeComponent()
        {
            this.ClientSize = new Size(600, 650);
            this.Text = isEditMode ? "Anime Düzenle" : "Yeni Anime Ekle";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.White;

            // Başlık
            var lblBaslik = new Label
            {
                Text = isEditMode ? "ANİME DÜZENLE" : "YENİ ANİME EKLE",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                Location = new Point(20, 20),
                Size = new Size(560, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblBaslik);

            int yPos = 70;

            // ID
            AddLabel("Anime ID:", yPos);
            txtId = AddTextBox(yPos);
            txtId.ReadOnly = isEditMode;
            if (!isEditMode)
            {
                txtId.Text = db.GetNextAnimeId().ToString();
            }
            yPos += 50;

            // İsim
            AddLabel("İsim (Türkçe):", yPos);
            txtIsim = AddTextBox(yPos);
            yPos += 50;

            // İngilizce İsim
            AddLabel("İsim (İngilizce):", yPos);
            txtIngilizce = AddTextBox(yPos);
            yPos += 50;

            // Bölüm Sayısı
            AddLabel("Bölüm Sayısı:", yPos);
            txtBolum = AddTextBox(yPos);
            yPos += 50;

            // Tip
            AddLabel("Tip (TV, Movie, OVA):", yPos);
            txtTip = AddTextBox(yPos);
            yPos += 50;

            // Yayın Tarihi
            AddLabel("Yayın Tarihi:", yPos);
            txtYayin = AddTextBox(yPos);
            yPos += 50;

            // Resim URL
            AddLabel("Resim URL:", yPos);
            txtResimUrl = AddTextBox(yPos);
            yPos += 50;

            // Türler
            var lblTurler = new Label
            {
                Text = "Türler:",
                Location = new Point(50, yPos),
                Size = new Size(150, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            this.Controls.Add(lblTurler);

            clbTurler = new CheckedListBox
            {
                Location = new Point(50, yPos + 25),
                Size = new Size(500, 120),
                Font = new Font("Segoe UI", 9),
                CheckOnClick = true
            };
            this.Controls.Add(clbTurler);

            // Butonlar
            btnKaydet = new Button
            {
                Text = isEditMode ? "Güncelle" : "Ekle",
                Location = new Point(180, 590),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnKaydet.Click += BtnKaydet_Click;
            this.Controls.Add(btnKaydet);

            btnIptal = new Button
            {
                Text = "İptal",
                Location = new Point(320, 590),
                Size = new Size(120, 40),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnIptal.Click += (s, e) => this.Close();
            this.Controls.Add(btnIptal);
        }

        private void AddLabel(string text, int yPos)
        {
            var label = new Label
            {
                Text = text,
                Location = new Point(50, yPos),
                Size = new Size(150, 20),
                Font = new Font("Segoe UI", 9)
            };
            this.Controls.Add(label);
        }

        private TextBox AddTextBox(int yPos)
        {
            var textBox = new TextBox
            {
                Location = new Point(50, yPos + 23),
                Size = new Size(500, 25),
                Font = new Font("Segoe UI", 10)
            };
            this.Controls.Add(textBox);
            return textBox;
        }

        private void LoadData()
        {
            // Türleri yükle
            var turler = db.GetAllTurler();
            foreach (var tur in turler)
            {
                clbTurler.Items.Add(tur, false);
            }
            clbTurler.DisplayMember = "TurAdi";

            if (isEditMode && existingAnime != null)
            {
                txtId.Text = existingAnime.AnimeId.ToString();
                txtIsim.Text = existingAnime.Isim;
                txtIngilizce.Text = existingAnime.IngilizceIsim ?? "";
                txtBolum.Text = existingAnime.BolumSayisi ?? "";
                txtTip.Text = existingAnime.Tip ?? "";
                txtYayin.Text = existingAnime.YayinTarihi ?? "";
                txtResimUrl.Text = existingAnime.ResimUrl ?? "";

                // Anime türlerini işaretle
                var animeTurleri = db.GetAnimeTurleri(existingAnime.AnimeId);
                for (int i = 0; i < clbTurler.Items.Count; i++)
                {
                    var tur = (Tur)clbTurler.Items[i];
                    if (animeTurleri.Any(t => t.TurId == tur.TurId))
                    {
                        clbTurler.SetItemChecked(i, true);
                    }
                }
            }
        }

        private void BtnKaydet_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIsim.Text))
            {
                MessageBox.Show("Anime ismi boş bırakılamaz!", "Uyarı", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtId.Text, out int animeId))
            {
                MessageBox.Show("Geçerli bir Anime ID giriniz!", "Uyarı", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedTurIds = new List<int>();
            foreach (var item in clbTurler.CheckedItems)
            {
                selectedTurIds.Add(((Tur)item).TurId);
            }

            if (selectedTurIds.Count == 0)
            {
                MessageBox.Show("En az bir tür seçmelisiniz!", "Uyarı", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var anime = new Anime
            {
                AnimeId = animeId,
                Isim = txtIsim.Text.Trim(),
                IngilizceIsim = string.IsNullOrWhiteSpace(txtIngilizce.Text) ? null : txtIngilizce.Text.Trim(),
                BolumSayisi = string.IsNullOrWhiteSpace(txtBolum.Text) ? null : txtBolum.Text.Trim(),
                Tip = string.IsNullOrWhiteSpace(txtTip.Text) ? null : txtTip.Text.Trim(),
                YayinTarihi = string.IsNullOrWhiteSpace(txtYayin.Text) ? null : txtYayin.Text.Trim(),
                ResimUrl = string.IsNullOrWhiteSpace(txtResimUrl.Text) ? null : txtResimUrl.Text.Trim()
            };

            bool success = isEditMode 
                ? db.UpdateAnime(anime, selectedTurIds)
                : db.AddAnime(anime, selectedTurIds);

            if (success)
            {
                MessageBox.Show(
                    isEditMode ? "Anime başarıyla güncellendi!" : "Anime başarıyla eklendi!", 
                    "Başarılı", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show(
                    isEditMode ? "Anime güncellenemedi!" : "Anime eklenemedi! Bu ID zaten kullanılıyor olabilir.", 
                    "Hata", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
        }
    }
}
