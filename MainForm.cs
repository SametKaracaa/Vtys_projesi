using AnimeApp.Database;
using AnimeApp.Models;
using AnimeApp.UI;
using System.Globalization;
using System.IO;

namespace AnimeApp.Forms
{
    public partial class MainForm : Form
    {
        private readonly DatabaseManager db;
        private readonly Kullanici currentUser;
        private DataGridView dgvAnime;
        private TextBox txtAra;
        private ComboBox cmbTurFiltre;
        private Button btnAra;
        private Button btnYenile;
        private Button btnPuanladiklarim;
        private Button btnTumAnimeler;
        private Button btnAdmin;
        private Button btnCikis;
        private Button btnTema; // Tema değiştirme butonu
        private Label lblKullanici;
        private PictureBox picAnime;
        private Panel pnlDetay;
        private Panel pnlSol;
        private Label lblAnimeBaslik;
        private Label lblPuan;
        private Label lblBolum;
        private Label lblTip;
        private Label lblTarih;
        private Cursor normalCursor;
        private Cursor clickCursor;
        private Label lblTurler;
        private TrackBar trackPuan;
        private Button btnPuanVer;
        private Label lblUserPuan;
        private TextBox txtYorum;
        private Label lblYorum;

        private List<Anime> currentAnimeList = new();
        private Anime? selectedAnime;
        private string? currentSortColumn = null;
        private bool sortAscending = true;

        public MainForm(DatabaseManager database, Kullanici user)
        {
            db = database;
            currentUser = user;
            InitializeComponent();

            // Temayı yükle ve uygula
            TemaYoneticisi.YukleVeUygula(db, currentUser.UserId, this);

            // --- İMLEÇ KODU BAŞLANGIÇ ---
            try
            {
                // Normal cursor (killer.png)
                string killerPath = System.IO.Path.Combine(Application.StartupPath, "killer.png");
                if (System.IO.File.Exists(killerPath))
                {
                    Bitmap killerBitmap = new Bitmap(killerPath);
                    Bitmap resizedKiller = new Bitmap(killerBitmap, new Size(32, 32));
                    IntPtr killerPtr = resizedKiller.GetHicon();
                    normalCursor = new Cursor(killerPtr);
                    this.Cursor = normalCursor;
                }

                // Tıklanabilir cursor (queen.png)
                string queenPath = System.IO.Path.Combine(Application.StartupPath, "queen.png");
                if (System.IO.File.Exists(queenPath))
                {
                    Bitmap queenBitmap = new Bitmap(queenPath);
                    Bitmap resizedQueen = new Bitmap(queenBitmap, new Size(32, 32));
                    IntPtr queenPtr = resizedQueen.GetHicon();
                    clickCursor = new Cursor(queenPtr);
                }

                ApplyCursorToAllControls(this, normalCursor);
                SetClickableCursors();
            }
            catch
            {
                this.Cursor = Cursors.Default;
            }
            // --- İMLEÇ KODU BİTİŞ ---

            LoadData();
            
            // Yeni özellikleri ekle
            AddNewFeatureButtons();
            AddFavoriteButtonToDetailPanel();
        }

        private void InitializeComponent()
        {
            this.Text = "Anime Veritabanı";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.White;

            // Üst Panel
            var pnlUst = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(41, 128, 185)
            };
            this.Controls.Add(pnlUst);

            // Başlık
            var lblBaslik = new Label
            {
                Text = "ANİME VERİTABANI",
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
            };
            pnlUst.Controls.Add(lblBaslik);


            // Kullanıcı Bilgisi - Tam görünsün
            lblKullanici = new Label
            {
                Text = $"Hoş geldin, {currentUser.KullaniciAdi}",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Location = new Point(20, 50),
                AutoSize = true,
                MaximumSize = new Size(700, 0)
            };
            pnlUst.Controls.Add(lblKullanici);












            // Tema Değiştirme Butonu
            btnTema = new Button
            {
                Text = TemaYoneticisi.IsDarkMode ? "☀️ Light" : "🌙 Dark",
                Size = new Size(100, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnTema.FlatAppearance.BorderSize = 0;
            btnTema.Click += BtnTema_Click;
            pnlUst.Controls.Add(btnTema);

            // Çıkış Butonu
            btnCikis = new Button
            {
                Text = "Çıkış",
                Size = new Size(90, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnCikis.Click += (s, e) => Application.Exit();
            pnlUst.Controls.Add(btnCikis);

            // Admin Butonu (sadece adminler için)
            if (currentUser.Rol == "ADMIN")
            {
                btnAdmin = new Button
                {
                    Text = "Admin Panel",
                    Size = new Size(110, 30),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right,
                    BackColor = Color.FromArgb(243, 156, 18),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold)
                };
                btnAdmin.Click += BtnAdmin_Click;
                pnlUst.Controls.Add(btnAdmin);
            }

            // Form resize olduğunda butonları konumlandır
            this.Resize += UpdateButtonPositions;
            this.Load += (s, e) => UpdateButtonPositions(s, e);

            // Sol Panel - Anime Listesi
            pnlSol = new Panel
            {
                Location = new Point(10, 90),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            this.Controls.Add(pnlSol);

            // Sağ Panel - Detay
            pnlDetay = new Panel
            {
                Width = 400,
                Location = new Point(10, 90),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(236, 240, 241)
            };
            this.Controls.Add(pnlDetay);

            // Form resize olduğunda panelleri ayarla
            this.Resize += UpdatePanelSizes;
            this.Load += (s, e) => UpdatePanelSizes(s, e);

            // Arama Panel
            var pnlArama = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(pnlSol.Width, 50),
                BackColor = Color.FromArgb(236, 240, 241),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };
            pnlSol.Controls.Add(pnlArama);

            // Arama TextBox
            txtAra = new TextBox
            {
                Location = new Point(10, 13),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10),
                PlaceholderText = "Anime ara..."
            };
            pnlArama.Controls.Add(txtAra);

            // Tür Filtresi
            cmbTurFiltre = new ComboBox
            {
                Location = new Point(220, 13),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            pnlArama.Controls.Add(cmbTurFiltre);

            // Ara Butonu
            btnAra = new Button
            {
                Text = "Ara",
                Location = new Point(380, 11),
                Size = new Size(70, 28),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnAra.Click += BtnAra_Click;
            pnlArama.Controls.Add(btnAra);

            // Yenile Butonu
            btnYenile = new Button
            {
                Text = "Yenile",
                Location = new Point(460, 11),
                Size = new Size(70, 28),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnYenile.Click += (s, e) => LoadData();
            pnlArama.Controls.Add(btnYenile);

            // Puanladıklarım Butonu
            btnPuanladiklarim = new Button
            {
                Text = "Puanladıklarım",
                Location = new Point(540, 11),
                Size = new Size(120, 28),
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnPuanladiklarim.Click += BtnPuanladiklarim_Click;
            pnlArama.Controls.Add(btnPuanladiklarim);

            // Tüm Animeler Butonu
            btnTumAnimeler = new Button
            {
                Text = "Tüm Animeler",
                Location = new Point(670, 11),
                Size = new Size(100, 28),
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnTumAnimeler.Click += (s, e) => LoadData();
            pnlArama.Controls.Add(btnTumAnimeler);

            // DataGridView
            dgvAnime = new DataGridView
            {
                Location = new Point(0, 50),
                Size = new Size(pnlSol.Width - 2, pnlSol.Height - 50),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false,
                ColumnHeadersVisible = true,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing
            };

            // Sütun başlık stilleri
            dgvAnime.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(52, 152, 219);
            dgvAnime.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvAnime.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvAnime.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvAnime.ColumnHeadersDefaultCellStyle.Padding = new Padding(5);
            dgvAnime.ColumnHeadersHeight = 40;

            dgvAnime.SelectionChanged += DgvAnime_SelectionChanged;
            dgvAnime.ColumnHeaderMouseClick += DgvAnime_ColumnHeaderMouseClick;
            pnlSol.Controls.Add(dgvAnime);

            // SAĞ PANEL İÇERİĞİ
            // Anime Resmi
            picAnime = new PictureBox
            {
                Location = new Point(30, 20),
                Size = new Size(340, 450),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            pnlDetay.Controls.Add(picAnime);

            // Anime Başlık
            lblAnimeBaslik = new Label
            {
                Location = new Point(10, 310),
                Size = new Size(380, 35),
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlDetay.Controls.Add(lblAnimeBaslik);

            // Puan Label
            lblPuan = new Label
            {
                Location = new Point(10, 350),
                Size = new Size(380, 25),
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(230, 126, 34),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlDetay.Controls.Add(lblPuan);

            // Bölüm
            lblBolum = new Label
            {
                Location = new Point(10, 545),
                Size = new Size(380, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(52, 73, 94),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlDetay.Controls.Add(lblBolum);

            // Tip
            lblTip = new Label
            {
                Location = new Point(10, 570),
                Size = new Size(380, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(52, 73, 94),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlDetay.Controls.Add(lblTip);

            // Tarih
            lblTarih = new Label
            {
                Location = new Point(10, 595),
                Size = new Size(380, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(52, 73, 94),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlDetay.Controls.Add(lblTarih);

            // Türler
            lblTurler = new Label
            {
                Location = new Point(10, 620),
                Size = new Size(380, 40),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(41, 128, 185),
                TextAlign = ContentAlignment.TopCenter
            };
            pnlDetay.Controls.Add(lblTurler);

            // Puanlama Bölümü
            var lblPuanlamaBaslik = new Label
            {
                Text = "Anime'yi Puanla:",
                Location = new Point(10, 675),
                Size = new Size(380, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlDetay.Controls.Add(lblPuanlamaBaslik);

            // Kullanıcı Puanı
            lblUserPuan = new Label
            {
                Location = new Point(10, 705),
                Size = new Size(380, 20),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(46, 204, 113),
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlDetay.Controls.Add(lblUserPuan);

            // TrackBar
            trackPuan = new TrackBar
            {
                Location = new Point(40, 735),
                Size = new Size(250, 35),
                Minimum = 0,
                Maximum = 10,
                Value = 5,
                TickFrequency = 1
            };
            trackPuan.ValueChanged += TrackPuan_ValueChanged;
            pnlDetay.Controls.Add(trackPuan);

            // Puan Değeri Label - DÜZELTME: Daha büyük ve daha iyi konumlandırılmış
            var lblPuanDeger = new Label
            {
                Name = "lblPuanDeger",
                Text = "5",
                Location = new Point(310, 738),
                Size = new Size(50, 30),
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(41, 128, 185),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlDetay.Controls.Add(lblPuanDeger);

            // Yorum Label
            lblYorum = new Label
            {
                Text = "Yorumunuz:",
                Location = new Point(10, 775),
                Size = new Size(380, 20),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(44, 62, 80),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlDetay.Controls.Add(lblYorum);

            // Yorum TextBox
            txtYorum = new TextBox
            {
                Location = new Point(10, 798),
                Size = new Size(380, 40),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Segoe UI", 9),
                PlaceholderText = "Anime hakkında düşüncelerinizi yazın..."
            };
            pnlDetay.Controls.Add(txtYorum);

            // Puan Ver Butonu
            btnPuanVer = new Button
            {
                Text = "Puan Ver",
                Location = new Point(100, 845),
                Size = new Size(200, 38),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Enabled = false
            };
            btnPuanVer.Click += BtnPuanVer_Click;
            pnlDetay.Controls.Add(btnPuanVer);
        }

        private void UpdateButtonPositions(object? sender, EventArgs e)
        {
            int rightMargin = 20;
            int buttonWidth = 100;
            int spacing = 10;
            
            btnCikis.Location = new Point(this.ClientSize.Width - rightMargin - buttonWidth, 25);
            
            // Tema butonu (çıkış butonunun solunda)
            if (btnTema != null)
                btnTema.Location = new Point(this.ClientSize.Width - rightMargin - (buttonWidth + spacing) * 2, 25);
            
            if (btnExport != null)
                btnExport.Location = new Point(this.ClientSize.Width - rightMargin - (buttonWidth + spacing) * 3, 25);
            
            if (btnProfil != null)
                btnProfil.Location = new Point(this.ClientSize.Width - rightMargin - (buttonWidth + spacing) * 4, 25);
            
            if (btnAdmin != null && currentUser.Rol == "ADMIN")
                btnAdmin.Location = new Point(this.ClientSize.Width - rightMargin - (buttonWidth + spacing) * 5, 25);
        }
        

        private void UpdatePanelSizes(object? sender, EventArgs e)
        {
            int sagPanelWidth = 400;
            int solPanelWidth = this.ClientSize.Width - sagPanelWidth - 30;
            int height = this.ClientSize.Height - 110;

            pnlSol.Size = new Size(solPanelWidth, height);
            pnlDetay.Location = new Point(solPanelWidth + 20, 90);
            pnlDetay.Height = height;
        }

        private void TrackPuan_ValueChanged(object? sender, EventArgs e)
        {
            var lblPuanDeger = pnlDetay.Controls.Find("lblPuanDeger", false).FirstOrDefault() as Label;
            if (lblPuanDeger != null)
            {
                lblPuanDeger.Text = trackPuan.Value.ToString();
            }
        }

        private void LoadData()
        {
            var turler = db.GetAllTurler();
            cmbTurFiltre.Items.Clear();
            cmbTurFiltre.Items.Add("Tüm Türler");
            foreach (var tur in turler)
            {
                cmbTurFiltre.Items.Add(tur);
            }
            cmbTurFiltre.DisplayMember = "TurAdi";
            cmbTurFiltre.ValueMember = "TurId";
            cmbTurFiltre.SelectedIndex = 0;

            LoadAnimeList();
        }

        private void LoadAnimeList(string? searchText = null, int? turId = null)
        {
            currentAnimeList = db.GetAnimeList(searchText, turId);
            currentSortColumn = null;
            sortAscending = true;

            dgvAnime.DataSource = null;
            dgvAnime.Columns.Clear();

            var bindingList = currentAnimeList.Select(a => new
            {
                a.AnimeId,
                Anime = a.Isim,
                İngilizce = a.IngilizceIsim ?? "-",
                Puan = a.Puan.HasValue ? a.Puan.Value.ToString("0.00", CultureInfo.InvariantCulture) : "N/A",
                Bölüm = a.BolumSayisi ?? "-",
                Tip = a.Tip ?? "-"
            }).ToList();

            dgvAnime.DataSource = bindingList;
            dgvAnime.Columns["AnimeId"].Visible = false;

            // Sütunları düzenle
            if (dgvAnime.Columns["Anime"] != null)
                dgvAnime.Columns["Anime"].HeaderText = "Anime";
            if (dgvAnime.Columns["İngilizce"] != null)
                dgvAnime.Columns["İngilizce"].HeaderText = "İngilizce";
            if (dgvAnime.Columns["Puan"] != null)
                dgvAnime.Columns["Puan"].HeaderText = "Puan";
            if (dgvAnime.Columns["Bölüm"] != null)
                dgvAnime.Columns["Bölüm"].HeaderText = "Bölüm";
            if (dgvAnime.Columns["Tip"] != null)
                dgvAnime.Columns["Tip"].HeaderText = "Tip";

            if (dgvAnime.Rows.Count > 0)
            {
                dgvAnime.Rows[0].Selected = true;
            }
        }

        private void BtnAra_Click(object? sender, EventArgs e)
        {
            var searchText = string.IsNullOrWhiteSpace(txtAra.Text) ? null : txtAra.Text;
            int? turId = null;

            if (cmbTurFiltre.SelectedIndex > 0 && cmbTurFiltre.SelectedItem is Tur tur)
            {
                turId = tur.TurId;
            }

            LoadAnimeList(searchText, turId);
        }

        private void DgvAnime_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgvAnime.SelectedRows.Count == 0) return;

            var row = dgvAnime.SelectedRows[0];
            var animeId = (int)row.Cells["AnimeId"].Value;

            selectedAnime = currentAnimeList.FirstOrDefault(a => a.AnimeId == animeId);
            if (selectedAnime != null)
            {
                DisplayAnimeDetails(selectedAnime);
            }
        }

        private void DgvAnime_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (dgvAnime.Columns.Count == 0 || currentAnimeList.Count == 0) return;

            var columnName = dgvAnime.Columns[e.ColumnIndex].Name;

            // AnimeId sütununa tıklanmışsa işlem yapma
            if (columnName == "AnimeId") return;

            // Aynı sütuna tıklanırsa sıralama yönünü değiştir
            if (currentSortColumn == columnName)
            {
                sortAscending = !sortAscending;
            }
            else
            {
                currentSortColumn = columnName;
                sortAscending = true;
            }

            // Sıralamayı uygula
            SortAnimeList(columnName, sortAscending);
        }

        private void SortAnimeList(string columnName, bool ascending)
        {
            IEnumerable<Anime> sortedList = currentAnimeList;

            switch (columnName)
            {
                case "Anime":
                    sortedList = ascending
                        ? currentAnimeList.OrderBy(a => a.Isim)
                        : currentAnimeList.OrderByDescending(a => a.Isim);
                    break;

                case "İngilizce":
                    sortedList = ascending
                        ? currentAnimeList.OrderBy(a => a.IngilizceIsim ?? "")
                        : currentAnimeList.OrderByDescending(a => a.IngilizceIsim ?? "");
                    break;

                case "Puan":
                    sortedList = ascending
                        ? currentAnimeList.OrderBy(a => a.Puan ?? 0)
                        : currentAnimeList.OrderByDescending(a => a.Puan ?? 0);
                    break;

                case "Bölüm":
                    sortedList = ascending
                        ? currentAnimeList.OrderBy(a => a.BolumSayisi ?? "")
                        : currentAnimeList.OrderByDescending(a => a.BolumSayisi ?? "");
                    break;

                case "Tip":
                    sortedList = ascending
                        ? currentAnimeList.OrderBy(a => a.Tip ?? "")
                        : currentAnimeList.OrderByDescending(a => a.Tip ?? "");
                    break;

                default:
                    return;
            }

            currentAnimeList = sortedList.ToList();

            // DataGridView'i güncelle
            var bindingList = currentAnimeList.Select(a => new
            {
                a.AnimeId,
                Anime = a.Isim,
                İngilizce = a.IngilizceIsim ?? "-",
                Puan = a.Puan.HasValue ? a.Puan.Value.ToString("0.00", CultureInfo.InvariantCulture) : "N/A",
                Bölüm = a.BolumSayisi ?? "-",
                Tip = a.Tip ?? "-"
            }).ToList();

            // Seçili satırı sakla
            int? selectedAnimeId = selectedAnime?.AnimeId;

            dgvAnime.DataSource = null;
            dgvAnime.DataSource = bindingList;
            dgvAnime.Columns["AnimeId"].Visible = false;

            // Sütun başlıklarını ayarla
            dgvAnime.Columns["Anime"].HeaderText = "Anime";
            dgvAnime.Columns["İngilizce"].HeaderText = "İngilizce";
            dgvAnime.Columns["Puan"].HeaderText = "Puan";
            dgvAnime.Columns["Bölüm"].HeaderText = "Bölüm";
            dgvAnime.Columns["Tip"].HeaderText = "Tip";

            // Sıralama göstergesini ekle
            foreach (DataGridViewColumn column in dgvAnime.Columns)
            {
                column.HeaderCell.SortGlyphDirection = SortOrder.None;
            }

            if (dgvAnime.Columns[columnName] != null)
            {
                dgvAnime.Columns[columnName].HeaderCell.SortGlyphDirection =
                    ascending ? SortOrder.Ascending : SortOrder.Descending;
            }

            // DÜZELTME: Seçili satırı geri yükle ama SCROLL YAPMA
            if (selectedAnimeId.HasValue)
            {
                foreach (DataGridViewRow row in dgvAnime.Rows)
                {
                    if ((int)row.Cells["AnimeId"].Value == selectedAnimeId.Value)
                    {
                        row.Selected = true;
                        // SCROLL YAPMASIN - bu satırı kaldırdım
                        // dgvAnime.FirstDisplayedScrollingRowIndex = row.Index;
                        break;
                    }
                }
            }
            else if (dgvAnime.Rows.Count > 0)
            {
                dgvAnime.Rows[0].Selected = true;
            }
        }

        private void DisplayAnimeDetails(Anime anime)
        {
            lblAnimeBaslik.Text = anime.Isim;
            lblPuan.Text = anime.Puan.HasValue
                ? $"⭐ Puan: {anime.Puan.Value.ToString("0.00", CultureInfo.InvariantCulture)}"
                : "Henüz puanlanmamış";
            lblBolum.Text = $"Bölüm: {anime.BolumSayisi ?? "Bilinmiyor"}";
            lblTip.Text = $"Tip: {anime.Tip ?? "Bilinmiyor"}";
            lblTarih.Text = $"Yayın: {anime.YayinTarihi ?? "Bilinmiyor"}";

            var turler = db.GetAnimeTurleri(anime.AnimeId);
            lblTurler.Text = turler.Count > 0
                ? $"Türler: {string.Join(", ", turler.Select(t => t.TurAdi))}"
                : "Tür bilgisi yok";

            LoadAnimeImage(anime.ResimUrl);

            var userPuan = db.GetUserPuan(currentUser.UserId, anime.AnimeId);
            if (userPuan.HasValue)
            {
                lblUserPuan.Text = $"Senin Puanın: {userPuan.Value}/10";
                trackPuan.Value = userPuan.Value;
            }
            else
            {
                lblUserPuan.Text = "Henüz puanlamadın";
                trackPuan.Value = 5;
            }

            // Kullanıcının yorumunu yükle
            var userYorum = db.GetUserYorum(currentUser.UserId, anime.AnimeId);
            txtYorum.Text = userYorum ?? "";

            btnPuanVer.Enabled = true;
            
            // Favori butonunu güncelle
            if (btnFavoriEkle != null)
            {
                btnFavoriEkle.Visible = true;
                UpdateFavoriteButton();
            }
        }

        private async void LoadAnimeImage(string? imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                picAnime.Image = null;
                return;
            }

            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(5);
                var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);
                using var ms = new MemoryStream(imageBytes);
                picAnime.Image = Image.FromStream(ms);
            }
            catch
            {
                picAnime.Image = null;
            }
        }

        private void BtnPuanVer_Click(object? sender, EventArgs e)
        {
            if (selectedAnime == null) return;

            var puan = trackPuan.Value;
            var yorum = string.IsNullOrWhiteSpace(txtYorum.Text) ? null : txtYorum.Text.Trim();
            
            if (db.SetUserPuan(currentUser.UserId, selectedAnime.AnimeId, puan, yorum))
            {
                MessageBox.Show($"Puan ve yorumunuz başarıyla kaydedildi: {puan}/10", "Başarılı",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                lblUserPuan.Text = $"Senin Puanın: {puan}/10";

                LoadAnimeList(
                    string.IsNullOrWhiteSpace(txtAra.Text) ? null : txtAra.Text,
                    cmbTurFiltre.SelectedIndex > 0 && cmbTurFiltre.SelectedItem is Tur tur ? tur.TurId : null
                );
            }
            else
            {
                MessageBox.Show("Puan kaydedilemedi!", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAdmin_Click(object? sender, EventArgs e)
        {
            using var adminForm = new AdminForm(db);
            if (adminForm.ShowDialog() == DialogResult.OK)
            {
                LoadAnimeList();
            }
        }

        private void BtnPuanladiklarim_Click(object? sender, EventArgs e)
        {
            LoadUserRatedAnimes();
        }

        private void LoadUserRatedAnimes()
        {
            currentAnimeList = db.GetUserRatedAnimes(currentUser.UserId);
            currentSortColumn = null;
            sortAscending = true;

            dgvAnime.DataSource = null;
            dgvAnime.Columns.Clear();

            if (currentAnimeList.Count == 0)
            {
                MessageBox.Show("Henüz hiç anime puanlamadınız!", "Bilgi",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var bindingList = currentAnimeList.Select(a => new
            {
                a.AnimeId,
                Anime = a.Isim,
                İngilizce = a.IngilizceIsim ?? "-",
                Puan = a.Puan.HasValue ? a.Puan.Value.ToString("0.00", CultureInfo.InvariantCulture) : "N/A",
                BenimPuan = db.GetUserPuan(currentUser.UserId, a.AnimeId)?.ToString() ?? "-",
                Bölüm = a.BolumSayisi ?? "-",
                Tip = a.Tip ?? "-"
            }).ToList();

            dgvAnime.DataSource = bindingList;
            dgvAnime.Columns["AnimeId"].Visible = false;

            // Sütunları düzenle
            if (dgvAnime.Columns["Anime"] != null)
                dgvAnime.Columns["Anime"].HeaderText = "Anime";
            if (dgvAnime.Columns["İngilizce"] != null)
                dgvAnime.Columns["İngilizce"].HeaderText = "İngilizce";
            if (dgvAnime.Columns["Puan"] != null)
                dgvAnime.Columns["Puan"].HeaderText = "Ortalama Puan";
            if (dgvAnime.Columns["BenimPuan"] != null)
            {
                dgvAnime.Columns["BenimPuan"].HeaderText = "Benim Puanım";
                dgvAnime.Columns["BenimPuan"].DefaultCellStyle.BackColor = Color.FromArgb(255, 250, 205);
                dgvAnime.Columns["BenimPuan"].DefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            }
            if (dgvAnime.Columns["Bölüm"] != null)
                dgvAnime.Columns["Bölüm"].HeaderText = "Bölüm";
            if (dgvAnime.Columns["Tip"] != null)
                dgvAnime.Columns["Tip"].HeaderText = "Tip";

            if (dgvAnime.Rows.Count > 0)
            {
                dgvAnime.Rows[0].Selected = true;
            }
        }

        private void ApplyCursorToAllControls(Control parent, Cursor cursor)
        {
            foreach (Control control in parent.Controls)
            {
                // Butonlara cursor ekleme, onları ayrı ayarlayacağız
                if (!(control is Button || control is DataGridView ||
                      control is ComboBox || control is TextBox || control is TrackBar))
                {
                    control.Cursor = cursor;
                }

                if (control.HasChildren)
                {
                    ApplyCursorToAllControls(control, cursor);
                }
            }
        }

        private void SetClickableCursors()
        {
            if (clickCursor == null) return;

            // Butonlar
            btnAra.Cursor = clickCursor;
            btnYenile.Cursor = clickCursor;
            btnPuanladiklarim.Cursor = clickCursor;
            btnTumAnimeler.Cursor = clickCursor;
            btnPuanVer.Cursor = clickCursor;
            btnCikis.Cursor = clickCursor;
            if (btnAdmin != null)
                btnAdmin.Cursor = clickCursor;

            // DataGridView
            dgvAnime.Cursor = clickCursor;

            // ComboBox
            cmbTurFiltre.Cursor = clickCursor;

            // TextBox
            txtAra.Cursor = clickCursor;

            // TrackBar
            trackPuan.Cursor = clickCursor;

            // Labellar
            lblKullanici.Cursor = clickCursor;
            lblAnimeBaslik.Cursor = clickCursor;
            lblPuan.Cursor = clickCursor;
            lblBolum.Cursor = clickCursor;
            lblTip.Cursor = clickCursor;
            lblTarih.Cursor = clickCursor;
            lblTurler.Cursor = clickCursor;
            lblUserPuan.Cursor = clickCursor;
        }

        // Tema değiştirme
        private void BtnTema_Click(object? sender, EventArgs e)
        {
            try
            {
                // Temayı değiştir
                bool yeniTema = !TemaYoneticisi.IsDarkMode;
                TemaYoneticisi.TemayiDegistir(yeniTema);
                
                // Veritabanına kaydet
                TemaYoneticisi.TemayiKaydet(db, currentUser.UserId, yeniTema);
                
                // Butonu güncelle
                btnTema.Text = yeniTema ? "☀️ Light" : "🌙 Dark";
                
                // Formu yenile
                TemaYoneticisi.FormaUygula(this);
                
                Console.WriteLine($"Tema değiştirildi: {(yeniTema ? "Dark" : "Light")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Tema değiştirme hatası: {ex.Message}");
                MessageBox.Show($"Tema değiştirilemedi: {ex.Message}", "Hata", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    } // Class'ın kapanışı
}
   