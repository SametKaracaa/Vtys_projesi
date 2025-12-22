using AnimeApp.Database;
using AnimeApp.Models;

namespace AnimeApp.UI
{
    public class TemaYoneticisi
    {
        // Tema renkleri
        public class TemaRenkleri
        {
            public Color ArkaPlan { get; set; }
            public Color Panel { get; set; }
            public Color Yazi { get; set; }
            public Color YaziSecondary { get; set; }
            public Color Buton { get; set; }
            public Color ButonHover { get; set; }
            public Color ButonText { get; set; }
            public Color Border { get; set; }
            public Color Baslik { get; set; }
            public Color Vurgu { get; set; }
            public Color Hata { get; set; }
            public Color Basari { get; set; }
            public Color Input { get; set; }
            public Color InputBorder { get; set; }
        }

        // Tema değişikliği eventi
        public static event EventHandler? TemaDegisti;

        // Light tema
        public static TemaRenkleri LightTema = new TemaRenkleri
        {
            ArkaPlan = Color.FromArgb(245, 247, 250),
            Panel = Color.White,
            Yazi = Color.FromArgb(33, 37, 41),
            YaziSecondary = Color.FromArgb(108, 117, 125),
            Buton = Color.FromArgb(13, 110, 253),
            ButonHover = Color.FromArgb(11, 94, 215),
            ButonText = Color.White,
            Border = Color.FromArgb(222, 226, 230),
            Baslik = Color.FromArgb(13, 110, 253),
            Vurgu = Color.FromArgb(13, 202, 240),
            Hata = Color.FromArgb(220, 53, 69),
            Basari = Color.FromArgb(25, 135, 84),
            Input = Color.White,
            InputBorder = Color.FromArgb(206, 212, 218)
        };

        // Dark tema
        public static TemaRenkleri DarkTema = new TemaRenkleri
        {
            ArkaPlan = Color.FromArgb(18, 18, 18),
            Panel = Color.FromArgb(30, 30, 30),
            Yazi = Color.FromArgb(230, 230, 230),
            YaziSecondary = Color.FromArgb(160, 160, 160),
            Buton = Color.FromArgb(33, 150, 243),
            ButonHover = Color.FromArgb(25, 118, 210),
            ButonText = Color.White,
            Border = Color.FromArgb(60, 60, 60),
            Baslik = Color.FromArgb(100, 181, 246),
            Vurgu = Color.FromArgb(38, 198, 218),
            Hata = Color.FromArgb(244, 67, 54),
            Basari = Color.FromArgb(76, 175, 80),
            Input = Color.FromArgb(40, 40, 40),
            InputBorder = Color.FromArgb(80, 80, 80)
        };

        private static TemaRenkleri aktifTema = LightTema;
        private static bool isDarkMode = false;

        public static bool IsDarkMode => isDarkMode;

        public static TemaRenkleri AktifTema => aktifTema;

        // Temayı değiştir
        public static void TemayiDegistir(bool darkMode)
        {
            isDarkMode = darkMode;
            aktifTema = darkMode ? DarkTema : LightTema;
            
            // Eventi tetikle
            TemaDegisti?.Invoke(null, EventArgs.Empty);
        }

        // Form'a tema uygula
        public static void FormaUygula(Form form)
        {
            if (form == null) return;

            form.BackColor = aktifTema.ArkaPlan;
            form.ForeColor = aktifTema.Yazi;

            UygulaRecursive(form.Controls);
        }

        private static void UygulaRecursive(Control.ControlCollection controls)
        {
            foreach (Control ctrl in controls)
            {
                // Panel ve GroupBox
                if (ctrl is Panel || ctrl is GroupBox)
                {
                    ctrl.BackColor = aktifTema.Panel;
                    ctrl.ForeColor = aktifTema.Yazi;
                }
                // Label
                else if (ctrl is Label lbl)
                {
                    if (lbl.Font != null && lbl.Font.Bold)
                    {
                        lbl.ForeColor = aktifTema.Baslik;
                    }
                    else
                    {
                        lbl.ForeColor = aktifTema.Yazi;
                    }
                }
                // TextBox
                else if (ctrl is TextBox txt)
                {
                    txt.BackColor = aktifTema.Input;
                    txt.ForeColor = aktifTema.Yazi;
                    txt.BorderStyle = BorderStyle.FixedSingle;
                }
                // ComboBox
                else if (ctrl is ComboBox cmb)
                {
                    cmb.BackColor = aktifTema.Input;
                    cmb.ForeColor = aktifTema.Yazi;
                    cmb.FlatStyle = FlatStyle.Flat;
                }
                // Button
                else if (ctrl is Button btn)
                {
                    // Özel renkli butonları koru
                    if (btn.BackColor == Color.FromArgb(220, 53, 69) || // Kırmızı
                        btn.BackColor == Color.FromArgb(231, 76, 60) ||
                        btn.BackColor == Color.FromArgb(244, 67, 54))
                    {
                        // Hata rengi - koru
                        btn.ForeColor = Color.White;
                    }
                    else if (btn.BackColor == Color.FromArgb(25, 135, 84) || // Yeşil
                             btn.BackColor == Color.FromArgb(46, 204, 113) ||
                             btn.BackColor == Color.FromArgb(76, 175, 80))
                    {
                        // Başarı rengi - koru
                        btn.ForeColor = Color.White;
                    }
                    else
                    {
                        // Normal buton
                        btn.BackColor = aktifTema.Buton;
                        btn.ForeColor = aktifTema.ButonText;
                    }
                    btn.FlatStyle = FlatStyle.Flat;
                    btn.FlatAppearance.BorderSize = 0;
                }
                // DataGridView
                else if (ctrl is DataGridView dgv)
                {
                    dgv.BackgroundColor = aktifTema.Panel;
                    dgv.ForeColor = aktifTema.Yazi;
                    dgv.GridColor = aktifTema.Border;
                    dgv.DefaultCellStyle.BackColor = aktifTema.Panel;
                    dgv.DefaultCellStyle.ForeColor = aktifTema.Yazi;
                    dgv.DefaultCellStyle.SelectionBackColor = aktifTema.Vurgu;
                    dgv.DefaultCellStyle.SelectionForeColor = Color.White;
                    dgv.ColumnHeadersDefaultCellStyle.BackColor = aktifTema.Buton;
                    dgv.ColumnHeadersDefaultCellStyle.ForeColor = aktifTema.ButonText;
                    dgv.EnableHeadersVisualStyles = false;
                }
                // TabControl
                else if (ctrl is TabControl tab)
                {
                    tab.BackColor = aktifTema.Panel;
                    foreach (TabPage page in tab.TabPages)
                    {
                        page.BackColor = aktifTema.ArkaPlan;
                        page.ForeColor = aktifTema.Yazi;
                        UygulaRecursive(page.Controls);
                    }
                }
                // ListBox
                else if (ctrl is ListBox lb)
                {
                    lb.BackColor = aktifTema.Panel;
                    lb.ForeColor = aktifTema.Yazi;
                }
                // NumericUpDown
                else if (ctrl is NumericUpDown nud)
                {
                    nud.BackColor = aktifTema.Input;
                    nud.ForeColor = aktifTema.Yazi;
                }
                // DateTimePicker
                else if (ctrl is DateTimePicker dtp)
                {
                    dtp.BackColor = aktifTema.Input;
                    dtp.ForeColor = aktifTema.Yazi;
                }
                // TrackBar
                else if (ctrl is TrackBar tb)
                {
                    tb.BackColor = aktifTema.ArkaPlan;
                }

                // Alt kontrolleri de işle
                if (ctrl.HasChildren)
                {
                    UygulaRecursive(ctrl.Controls);
                }
            }
        }

        // Veritabanından temayı yükle
        public static void YukleVeUygula(DatabaseManager db, int userId, Form form)
        {
            try
            {
                var ayarlar = db.GetKullaniciAyarlari(userId);
                if (ayarlar != null)
                {
                    bool darkMode = ayarlar.Tema?.ToLower() == "dark";
                    TemayiDegistir(darkMode);
                    Console.WriteLine($"Tema yüklendi: {(darkMode ? "Dark" : "Light")}");
                }
                else
                {
                    // Varsayılan: Light tema
                    TemayiDegistir(false);
                    Console.WriteLine("Varsayılan tema (Light) uygulandı");
                }

                FormaUygula(form);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Tema yükleme hatası: {ex.Message}");
                // Hata durumunda varsayılan tema
                TemayiDegistir(false);
                FormaUygula(form);
            }
        }

        // Temayı kaydet
        public static void TemayiKaydet(DatabaseManager db, int userId, bool darkMode)
        {
            try
            {
                string temaAdi = darkMode ? "Dark" : "Light";
                db.UpdateKullaniciAyarlari(userId, temaAdi, null);
                Console.WriteLine($"Tema kaydedildi: {temaAdi}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Tema kaydetme hatası: {ex.Message}");
            }
        }
    }
}
