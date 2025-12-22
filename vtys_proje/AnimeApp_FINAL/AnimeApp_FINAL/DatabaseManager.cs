using Npgsql;
using AnimeApp.Models;
using System.Data;
using BCrypt.Net;

namespace AnimeApp.Database
{
    public class DatabaseManager
    {
        private readonly string connectionString;

        public DatabaseManager(string connectionString)
        {
            this.connectionString = connectionString;
        }

        // Kullanıcı Giriş Kontrolü (BCrypt ile - backward compatible)
        public Kullanici? Login(string kullaniciAdi, string sifre)
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand(
                "SELECT user_id, kullanici_adi, cinsiyet, dogum_tarihi, rol, sifre FROM kullanicilar WHERE kullanici_adi = @kad",
                conn);
            cmd.Parameters.AddWithValue("kad", kullaniciAdi);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var storedPassword = reader.GetString(5);
                bool isValidPassword = false;

                // Önce BCrypt hash kontrolü yap
                try
                {
                    isValidPassword = BCrypt.Net.BCrypt.Verify(sifre, storedPassword);
                }
                catch
                {
                    // BCrypt başarısız oldu, düz metin kontrolü yap (eski kullanıcılar için)
                    isValidPassword = (sifre == storedPassword);
                }

                if (isValidPassword)
                {
                    return new Kullanici
                    {
                        UserId = reader.GetInt32(0),
                        KullaniciAdi = reader.GetString(1),
                        Cinsiyet = reader.IsDBNull(2) ? null : reader.GetString(2),
                        DogumTarihi = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                        Rol = reader.GetString(4)
                    };
                }
            }
            return null;
        }

        // Yeni Kullanıcı Kaydı - BASİT VERSİYON
        public bool KayitOl(string kullaniciAdi, string sifre, string? cinsiyet, DateTime? dogumTarihi)
        {
            Console.WriteLine("\n╔════════════════════════════════════════╗");
            Console.WriteLine("║   VERİTABANI KAYIT İŞLEMİ BAŞLIYOR    ║");
            Console.WriteLine("╚════════════════════════════════════════╝\n");
            
            NpgsqlConnection? conn = null;
            
            try
            {
                // ADIM 1: Bağlantı oluştur
                Console.WriteLine("📡 ADIM 1: Bağlantı oluşturuluyor...");
                Console.WriteLine($"   Connection String: {connectionString.Replace("Password=123456", "Password=***")}");
                
                conn = new NpgsqlConnection(connectionString);
                
                // ADIM 2: Bağlantıyı aç
                Console.WriteLine("\n🔓 ADIM 2: Bağlantı açılıyor...");
                conn.Open();
                Console.WriteLine("   ✅ Bağlantı başarıyla açıldı!");
                
                // ADIM 3: Veritabanı versiyonu
                Console.WriteLine("\n🐘 ADIM 3: PostgreSQL versiyonu kontrol ediliyor...");
                using (var versionCmd = new NpgsqlCommand("SELECT version()", conn))
                {
                    var version = versionCmd.ExecuteScalar()?.ToString() ?? "Unknown";
                    var displayVersion = version.Length > 50 ? version.Substring(0, 50) : version;
                    Console.WriteLine($"   ✅ PostgreSQL çalışıyor: {displayVersion}...");
                }
                
                // ADIM 4: Kullanıcı adı kontrolü
                Console.WriteLine($"\n🔍 ADIM 4: '{kullaniciAdi}' kullanıcı adı kontrol ediliyor...");
                using (var checkCmd = new NpgsqlCommand(
                    "SELECT COUNT(*) FROM kullanicilar WHERE kullanici_adi = @kad", conn))
                {
                    checkCmd.Parameters.AddWithValue("kad", kullaniciAdi);
                    var count = Convert.ToInt32(checkCmd.ExecuteScalar());
                    Console.WriteLine($"   Bulunan kayıt sayısı: {count}");
                    
                    if (count > 0)
                    {
                        Console.WriteLine("   ❌ Bu kullanıcı adı zaten kullanılıyor!");
                        return false;
                    }
                    Console.WriteLine("   ✅ Kullanıcı adı kullanılabilir!");
                }
                
                // ADIM 5: Tablo yapısını kontrol et
                Console.WriteLine("\n📋 ADIM 5: kullanicilar tablosu yapısı kontrol ediliyor...");
                var columns = new List<string>();
                using (var colCmd = new NpgsqlCommand(@"
                    SELECT column_name 
                    FROM information_schema.columns 
                    WHERE table_name = 'kullanicilar' 
                    ORDER BY ordinal_position", conn))
                {
                    using var reader = colCmd.ExecuteReader();
                    while (reader.Read())
                    {
                        columns.Add(reader.GetString(0));
                    }
                }
                
                if (columns.Count == 0)
                {
                    Console.WriteLine("   ❌ HATA: kullanicilar tablosu bulunamadı!");
                    Console.WriteLine("   → database_test.sql dosyasını çalıştırın!");
                    return false;
                }
                
                Console.WriteLine($"   ✅ Tablo bulundu! Sütunlar ({columns.Count} adet):");
                foreach (var col in columns)
                {
                    Console.WriteLine($"      • {col}");
                }
                
                // ADIM 6: INSERT sorgusu hazırla
                Console.WriteLine("\n💾 ADIM 6: INSERT sorgusu hazırlanıyor...");
                bool hasRol = columns.Contains("rol");
                bool hasKayitTarihi = columns.Contains("kayit_tarihi");
                
                string insertQuery;
                if (hasRol && hasKayitTarihi)
                {
                    insertQuery = @"INSERT INTO kullanicilar (kullanici_adi, sifre, cinsiyet, dogum_tarihi, rol, kayit_tarihi) 
                                   VALUES (@kad, @sifre, @cinsiyet, @dogum, 'USER', CURRENT_TIMESTAMP)";
                }
                else if (hasRol)
                {
                    insertQuery = @"INSERT INTO kullanicilar (kullanici_adi, sifre, cinsiyet, dogum_tarihi, rol) 
                                   VALUES (@kad, @sifre, @cinsiyet, @dogum, 'USER')";
                }
                else
                {
                    insertQuery = @"INSERT INTO kullanicilar (kullanici_adi, sifre, cinsiyet, dogum_tarihi) 
                                   VALUES (@kad, @sifre, @cinsiyet, @dogum)";
                }
                
                Console.WriteLine($"   Sorgu: {insertQuery}");
                
                // ADIM 7: Parametreleri hazırla
                Console.WriteLine("\n📝 ADIM 7: Parametreler hazırlanıyor...");
                Console.WriteLine($"   • kullanici_adi: '{kullaniciAdi}'");
                Console.WriteLine($"   • sifre: '{new string('*', sifre.Length)}' (uzunluk: {sifre.Length})");
                Console.WriteLine($"   • cinsiyet: {cinsiyet ?? "NULL"}");
                Console.WriteLine($"   • dogum_tarihi: {dogumTarihi?.ToString("yyyy-MM-dd") ?? "NULL"}");
                if (hasRol) Console.WriteLine($"   • rol: USER");
                
                // ADIM 8: INSERT komutunu çalıştır
                Console.WriteLine("\n🚀 ADIM 8: INSERT komutu çalıştırılıyor...");
                using var cmd = new NpgsqlCommand(insertQuery, conn);
                cmd.Parameters.AddWithValue("kad", kullaniciAdi);
                cmd.Parameters.AddWithValue("sifre", sifre);
                cmd.Parameters.AddWithValue("cinsiyet", cinsiyet ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("dogum", dogumTarihi ?? (object)DBNull.Value);
                
                int rowsAffected = cmd.ExecuteNonQuery();
                
                Console.WriteLine($"   ✅ Komut başarılı! {rowsAffected} satır eklendi.");
                
                // ADIM 9: Doğrulama
                Console.WriteLine("\n✔️ ADIM 9: Kayıt doğrulanıyor...");
                using (var verifyCmd = new NpgsqlCommand(
                    "SELECT user_id, kullanici_adi, rol FROM kullanicilar WHERE kullanici_adi = @kad", conn))
                {
                    verifyCmd.Parameters.AddWithValue("kad", kullaniciAdi);
                    using var verifyReader = verifyCmd.ExecuteReader();
                    if (verifyReader.Read())
                    {
                        var userId = verifyReader.GetInt32(0);
                        var userName = verifyReader.GetString(1);
                        var role = verifyReader.IsDBNull(2) ? "N/A" : verifyReader.GetString(2);
                        
                        Console.WriteLine($"   ✅ Kullanıcı veritabanında bulundu!");
                        Console.WriteLine($"      • ID: {userId}");
                        Console.WriteLine($"      • Kullanıcı Adı: {userName}");
                        Console.WriteLine($"      • Rol: {role}");
                    }
                }
                
                Console.WriteLine("\n╔════════════════════════════════════════╗");
                Console.WriteLine("║     ✅ KAYIT BAŞARIYLA TAMAMLANDI!    ║");
                Console.WriteLine("╚════════════════════════════════════════╝\n");
                
                return true;
            }
            catch (NpgsqlException pgEx)
            {
                Console.WriteLine("\n╔════════════════════════════════════════╗");
                Console.WriteLine("║    ❌ POSTGRESQL HATASI!              ║");
                Console.WriteLine("╚════════════════════════════════════════╝\n");
                Console.WriteLine($"Hata Kodu: {pgEx.SqlState}");
                Console.WriteLine($"Hata Mesajı: {pgEx.Message}");
                Console.WriteLine($"\nDetaylar:\n{pgEx.ToString()}\n");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n╔════════════════════════════════════════╗");
                Console.WriteLine("║       ❌ GENEL HATA!                  ║");
                Console.WriteLine("╚════════════════════════════════════════╝\n");
                Console.WriteLine($"Hata Tipi: {ex.GetType().Name}");
                Console.WriteLine($"Hata Mesajı: {ex.Message}");
                Console.WriteLine($"\nStackTrace:\n{ex.StackTrace}\n");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"İç Hata: {ex.InnerException.Message}\n");
                }
                
                return false;
            }
            finally
            {
                if (conn != null)
                {
                    Console.WriteLine("🔒 Bağlantı kapatılıyor...");
                    conn.Close();
                    conn.Dispose();
                    Console.WriteLine("   ✅ Bağlantı kapatıldı.\n");
                }
            }
        }

        // Tüm Animeleri Getir
        public List<Anime> GetAnimeList(string? searchText = null, int? turId = null)
        {
            var animeList = new List<Anime>();
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            var query = @"SELECT DISTINCT a.anime_id, a.isim, a.ingilizce_isim, a.puan, 
                         a.bolum_sayisi, a.tip, a.yayin_tarihi, a.resim_url
                         FROM animeler a
                         LEFT JOIN anime_turler at ON a.anime_id = at.anime_id
                         WHERE 1=1";

            if (!string.IsNullOrEmpty(searchText))
            {
                query += " AND (LOWER(a.isim) LIKE LOWER(@search) OR LOWER(a.ingilizce_isim) LIKE LOWER(@search))";
            }

            if (turId.HasValue && turId.Value > 0)
            {
                query += " AND at.tur_id = @turId";
            }

            query += " ORDER BY a.puan DESC NULLS LAST";

            using var cmd = new NpgsqlCommand(query, conn);
            if (!string.IsNullOrEmpty(searchText))
                cmd.Parameters.AddWithValue("search", $"%{searchText}%");
            if (turId.HasValue)
                cmd.Parameters.AddWithValue("turId", turId.Value);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                animeList.Add(new Anime
                {
                    AnimeId = reader.GetInt32(0),
                    Isim = reader.GetString(1),
                    IngilizceIsim = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Puan = reader.IsDBNull(3) ? null : reader.GetDouble(3),
                    BolumSayisi = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Tip = reader.IsDBNull(5) ? null : reader.GetString(5),
                    YayinTarihi = reader.IsDBNull(6) ? null : reader.GetString(6),
                    ResimUrl = reader.IsDBNull(7) ? null : reader.GetString(7)
                });
            }

            return animeList;
        }

        // Anime Türlerini Getir
        public List<Tur> GetAnimeTurleri(int animeId)
        {
            var turler = new List<Tur>();
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand(
                @"SELECT t.tur_id, t.tur_adi FROM turler t
                  INNER JOIN anime_turler at ON t.tur_id = at.tur_id
                  WHERE at.anime_id = @id",
                conn);
            cmd.Parameters.AddWithValue("id", animeId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                turler.Add(new Tur
                {
                    TurId = reader.GetInt32(0),
                    TurAdi = reader.GetString(1)
                });
            }

            return turler;
        }

        // Tüm Türleri Getir
        public List<Tur> GetAllTurler()
        {
            var turler = new List<Tur>();
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand("SELECT tur_id, tur_adi FROM turler ORDER BY tur_adi", conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                turler.Add(new Tur
                {
                    TurId = reader.GetInt32(0),
                    TurAdi = reader.GetString(1)
                });
            }

            return turler;
        }

        // Kullanıcının Verdiği Puanı Getir
        public int? GetUserPuan(int userId, int animeId)
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand(
                "SELECT verilen_puan FROM puanlar WHERE user_id = @uid AND anime_id = @aid",
                conn);
            cmd.Parameters.AddWithValue("uid", userId);
            cmd.Parameters.AddWithValue("aid", animeId);

            var result = cmd.ExecuteScalar();
            return result != null ? (int)result : null;
        }

        // Kullanıcının yorumunu getir
        public string? GetUserYorum(int userId, int animeId)
        {
            try
            {
                using var conn = new NpgsqlConnection(connectionString);
                conn.Open();

                using var cmd = new NpgsqlCommand(
                    "SELECT yorum FROM puanlar WHERE user_id = @uid AND anime_id = @aid",
                    conn);
                cmd.Parameters.AddWithValue("uid", userId);
                cmd.Parameters.AddWithValue("aid", animeId);

                var result = cmd.ExecuteScalar();
                return result != DBNull.Value && result != null ? result.ToString() : null;
            }
            catch
            {
                // Yorum sütunu yoksa veya başka bir hata varsa null döndür
                return null;
            }
        }

        // Puan Ekle veya Güncelle
        public bool SetUserPuan(int userId, int animeId, int puan, string yorum = null)
        {
            try
            {
                using var conn = new NpgsqlConnection(connectionString);
                conn.Open();

                // Önce kontrol et
                using var checkCmd = new NpgsqlCommand(
                    "SELECT puan_id FROM puanlar WHERE user_id = @uid AND anime_id = @aid",
                    conn);
                checkCmd.Parameters.AddWithValue("uid", userId);
                checkCmd.Parameters.AddWithValue("aid", animeId);

                var existingId = checkCmd.ExecuteScalar();

                if (existingId != null)
                {
                    // Güncelle - önce yorum ile dene
                    try
                    {
                        using var updateCmd = new NpgsqlCommand(
                            "UPDATE puanlar SET verilen_puan = @puan, yorum = @yorum, puanlama_zamani = CURRENT_TIMESTAMP WHERE user_id = @uid AND anime_id = @aid",
                            conn);
                        updateCmd.Parameters.AddWithValue("puan", puan);
                        updateCmd.Parameters.AddWithValue("yorum", (object)yorum ?? DBNull.Value);
                        updateCmd.Parameters.AddWithValue("uid", userId);
                        updateCmd.Parameters.AddWithValue("aid", animeId);
                        updateCmd.ExecuteNonQuery();
                    }
                    catch
                    {
                        // Yorum sütunu yoksa, sadece puan güncelle
                        using var updateCmd = new NpgsqlCommand(
                            "UPDATE puanlar SET verilen_puan = @puan, puanlama_zamani = CURRENT_TIMESTAMP WHERE user_id = @uid AND anime_id = @aid",
                            conn);
                        updateCmd.Parameters.AddWithValue("puan", puan);
                        updateCmd.Parameters.AddWithValue("uid", userId);
                        updateCmd.Parameters.AddWithValue("aid", animeId);
                        updateCmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    // Ekle - önce yorum ile dene
                    try
                    {
                        using var insertCmd = new NpgsqlCommand(
                            "INSERT INTO puanlar (user_id, anime_id, verilen_puan, yorum) VALUES (@uid, @aid, @puan, @yorum)",
                            conn);
                        insertCmd.Parameters.AddWithValue("uid", userId);
                        insertCmd.Parameters.AddWithValue("aid", animeId);
                        insertCmd.Parameters.AddWithValue("puan", puan);
                        insertCmd.Parameters.AddWithValue("yorum", (object)yorum ?? DBNull.Value);
                        insertCmd.ExecuteNonQuery();
                    }
                    catch
                    {
                        // Yorum sütunu yoksa, sadece puan ekle
                        using var insertCmd = new NpgsqlCommand(
                            "INSERT INTO puanlar (user_id, anime_id, verilen_puan) VALUES (@uid, @aid, @puan)",
                            conn);
                        insertCmd.Parameters.AddWithValue("uid", userId);
                        insertCmd.Parameters.AddWithValue("aid", animeId);
                        insertCmd.Parameters.AddWithValue("puan", puan);
                        insertCmd.ExecuteNonQuery();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                // Hatayı logla ve göster
                System.Diagnostics.Debug.WriteLine($"SetUserPuan Error: {ex.Message}");
                MessageBox.Show($"Puan kaydedilirken hata oluştu: {ex.Message}", "Veritabanı Hatası",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Kullanıcının yorumunu getir
        public string GetUserComment(int userId, int animeId)
        {
            try
            {
                using var conn = new NpgsqlConnection(connectionString);
                conn.Open();

                using var cmd = new NpgsqlCommand(
                    "SELECT yorum FROM puanlar WHERE user_id = @uid AND anime_id = @aid",
                    conn);
                cmd.Parameters.AddWithValue("uid", userId);
                cmd.Parameters.AddWithValue("aid", animeId);

                var result = cmd.ExecuteScalar();
                return result != null && result != DBNull.Value ? result.ToString() : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        // Anime'nin tüm yorumlarını getir
        public List<(string kullaniciAdi, int puan, string yorum, DateTime tarih)> GetAnimeComments(int animeId)
        {
            var comments = new List<(string, int, string, DateTime)>();
            try
            {
                using var conn = new NpgsqlConnection(connectionString);
                conn.Open();

                using var cmd = new NpgsqlCommand(
                    @"SELECT k.kullanici_adi, p.verilen_puan, p.yorum, p.puanlama_zamani
                      FROM puanlar p
                      INNER JOIN kullanicilar k ON p.user_id = k.user_id
                      WHERE p.anime_id = @aid AND p.yorum IS NOT NULL AND p.yorum != ''
                      ORDER BY p.puanlama_zamani DESC",
                    conn);
                cmd.Parameters.AddWithValue("aid", animeId);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    comments.Add((
                        reader.GetString(0),
                        reader.GetInt32(1),
                        reader.GetString(2),
                        reader.GetDateTime(3)
                    ));
                }
            }
            catch { }

            return comments;
        }

        // Admin: Anime Ekle
        public bool AddAnime(Anime anime, List<int> turIds)
        {
            try
            {
                using var conn = new NpgsqlConnection(connectionString);
                conn.Open();
                using var transaction = conn.BeginTransaction();

                try
                {
                    // Anime ekle
                    using var cmd = new NpgsqlCommand(
                        @"INSERT INTO animeler (anime_id, isim, ingilizce_isim, bolum_sayisi, tip, yayin_tarihi, resim_url) 
                          VALUES (@id, @isim, @eng, @bolum, @tip, @yayin, @resim)",
                        conn);
                    cmd.Parameters.AddWithValue("id", anime.AnimeId);
                    cmd.Parameters.AddWithValue("isim", anime.Isim);
                    cmd.Parameters.AddWithValue("eng", (object?)anime.IngilizceIsim ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("bolum", (object?)anime.BolumSayisi ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("tip", (object?)anime.Tip ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("yayin", (object?)anime.YayinTarihi ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("resim", (object?)anime.ResimUrl ?? DBNull.Value);
                    cmd.ExecuteNonQuery();

                    // Türleri ekle
                    foreach (var turId in turIds)
                    {
                        using var turCmd = new NpgsqlCommand(
                            "INSERT INTO anime_turler (anime_id, tur_id) VALUES (@aid, @tid)",
                            conn);
                        turCmd.Parameters.AddWithValue("aid", anime.AnimeId);
                        turCmd.Parameters.AddWithValue("tid", turId);
                        turCmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch
            {
                return false;
            }
        }

        // Admin: Anime Güncelle
        public bool UpdateAnime(Anime anime, List<int> turIds)
        {
            try
            {
                using var conn = new NpgsqlConnection(connectionString);
                conn.Open();
                using var transaction = conn.BeginTransaction();

                try
                {
                    // Anime güncelle
                    using var cmd = new NpgsqlCommand(
                        @"UPDATE animeler SET isim = @isim, ingilizce_isim = @eng, 
                          bolum_sayisi = @bolum, tip = @tip, yayin_tarihi = @yayin, resim_url = @resim
                          WHERE anime_id = @id",
                        conn);
                    cmd.Parameters.AddWithValue("id", anime.AnimeId);
                    cmd.Parameters.AddWithValue("isim", anime.Isim);
                    cmd.Parameters.AddWithValue("eng", (object?)anime.IngilizceIsim ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("bolum", (object?)anime.BolumSayisi ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("tip", (object?)anime.Tip ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("yayin", (object?)anime.YayinTarihi ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("resim", (object?)anime.ResimUrl ?? DBNull.Value);
                    cmd.ExecuteNonQuery();

                    // Eski türleri sil
                    using var deleteCmd = new NpgsqlCommand(
                        "DELETE FROM anime_turler WHERE anime_id = @id", conn);
                    deleteCmd.Parameters.AddWithValue("id", anime.AnimeId);
                    deleteCmd.ExecuteNonQuery();

                    // Yeni türleri ekle
                    foreach (var turId in turIds)
                    {
                        using var turCmd = new NpgsqlCommand(
                            "INSERT INTO anime_turler (anime_id, tur_id) VALUES (@aid, @tid)",
                            conn);
                        turCmd.Parameters.AddWithValue("aid", anime.AnimeId);
                        turCmd.Parameters.AddWithValue("tid", turId);
                        turCmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch
            {
                return false;
            }
        }

        // Admin: Anime Sil
        public bool DeleteAnime(int animeId)
        {
            try
            {
                using var conn = new NpgsqlConnection(connectionString);
                conn.Open();
                using var transaction = conn.BeginTransaction();

                try
                {
                    // Türleri sil
                    using var deleteTurCmd = new NpgsqlCommand(
                        "DELETE FROM anime_turler WHERE anime_id = @id", conn);
                    deleteTurCmd.Parameters.AddWithValue("id", animeId);
                    deleteTurCmd.ExecuteNonQuery();

                    // Puanları sil
                    using var deletePuanCmd = new NpgsqlCommand(
                        "DELETE FROM puanlar WHERE anime_id = @id", conn);
                    deletePuanCmd.Parameters.AddWithValue("id", animeId);
                    deletePuanCmd.ExecuteNonQuery();

                    // Anime sil
                    using var deleteCmd = new NpgsqlCommand(
                        "DELETE FROM animeler WHERE anime_id = @id", conn);
                    deleteCmd.Parameters.AddWithValue("id", animeId);
                    deleteCmd.ExecuteNonQuery();

                    transaction.Commit();
                    return true;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch
            {
                return false;
            }
        }

        // Sonraki Anime ID'yi al
        public int GetNextAnimeId()
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand("SELECT COALESCE(MAX(anime_id), 0) + 1 FROM animeler", conn);
            return (int)cmd.ExecuteScalar()!;
        }

        // Kullanıcının Puanladığı Animeleri Getir
        public List<Anime> GetUserRatedAnimes(int userId)
        {
            var animeList = new List<Anime>();
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            var query = @"SELECT a.anime_id, a.isim, a.ingilizce_isim, a.puan, 
                         a.bolum_sayisi, a.tip, a.yayin_tarihi, a.resim_url
                         FROM animeler a
                         INNER JOIN puanlar p ON a.anime_id = p.anime_id
                         WHERE p.user_id = @userId
                         ORDER BY p.puanlama_zamani DESC";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("userId", userId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                animeList.Add(new Anime
                {
                    AnimeId = reader.GetInt32(0),
                    Isim = reader.GetString(1),
                    IngilizceIsim = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Puan = reader.IsDBNull(3) ? null : reader.GetDouble(3),
                    BolumSayisi = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Tip = reader.IsDBNull(5) ? null : reader.GetString(5),
                    YayinTarihi = reader.IsDBNull(6) ? null : reader.GetString(6),
                    ResimUrl = reader.IsDBNull(7) ? null : reader.GetString(7)
                });
            }

            return animeList;
        }

        // İstatistikler
        public Dictionary<string, int> GetStatistics()
        {
            var stats = new Dictionary<string, int>();
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            using var cmd1 = new NpgsqlCommand("SELECT COUNT(*) FROM animeler", conn);
            stats["ToplamAnime"] = Convert.ToInt32(cmd1.ExecuteScalar());

            using var cmd2 = new NpgsqlCommand("SELECT COUNT(*) FROM kullanicilar", conn);
            stats["ToplamKullanici"] = Convert.ToInt32(cmd2.ExecuteScalar());

            using var cmd3 = new NpgsqlCommand("SELECT COUNT(*) FROM puanlar", conn);
            stats["ToplamPuanlama"] = Convert.ToInt32(cmd3.ExecuteScalar());

            return stats;
        }

        // ==================== YENİ ÖZELLİKLER ====================

        // FAVORİLER
        public bool FavoriEkle(int userId, int animeId)
        {
            try
            {
                using var conn = new NpgsqlConnection(connectionString);
                conn.Open();
                using var cmd = new NpgsqlCommand(
                    "INSERT INTO favoriler (user_id, anime_id) VALUES (@uid, @aid) ON CONFLICT DO NOTHING",
                    conn);
                cmd.Parameters.AddWithValue("uid", userId);
                cmd.Parameters.AddWithValue("aid", animeId);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
        }

        public bool FavoriCikar(int userId, int animeId)
        {
            try
            {
                using var conn = new NpgsqlConnection(connectionString);
                conn.Open();
                using var cmd = new NpgsqlCommand(
                    "DELETE FROM favoriler WHERE user_id = @uid AND anime_id = @aid",
                    conn);
                cmd.Parameters.AddWithValue("uid", userId);
                cmd.Parameters.AddWithValue("aid", animeId);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch { return false; }
        }

        public bool IsFavorite(int userId, int animeId)
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(
                "SELECT COUNT(*) FROM favoriler WHERE user_id = @uid AND anime_id = @aid",
                conn);
            cmd.Parameters.AddWithValue("uid", userId);
            cmd.Parameters.AddWithValue("aid", animeId);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public List<Anime> GetFavoriteAnimes(int userId)
        {
            var animeList = new List<Anime>();
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            var query = @"SELECT a.anime_id, a.isim, a.ingilizce_isim, a.puan, 
                         a.bolum_sayisi, a.tip, a.yayin_tarihi, a.resim_url
                         FROM animeler a
                         INNER JOIN favoriler f ON a.anime_id = f.anime_id
                         WHERE f.user_id = @userId
                         ORDER BY f.eklenme_zamani DESC";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("userId", userId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                animeList.Add(new Anime
                {
                    AnimeId = reader.GetInt32(0),
                    Isim = reader.GetString(1),
                    IngilizceIsim = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Puan = reader.IsDBNull(3) ? null : reader.GetDouble(3),
                    BolumSayisi = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Tip = reader.IsDBNull(5) ? null : reader.GetString(5),
                    YayinTarihi = reader.IsDBNull(6) ? null : reader.GetString(6),
                    ResimUrl = reader.IsDBNull(7) ? null : reader.GetString(7)
                });
            }
            return animeList;
        }

        // KULLANICI AYARLARI
        public KullaniciAyarlari? GetKullaniciAyarlari(int userId)
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand(
                "SELECT ayar_id, user_id, tema, dil FROM kullanici_ayarlari WHERE user_id = @uid",
                conn);
            cmd.Parameters.AddWithValue("uid", userId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new KullaniciAyarlari
                {
                    AyarId = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    Tema = reader.GetString(2),
                    Dil = reader.GetString(3)
                };
            }
            return null;
        }

        public bool UpdateKullaniciAyarlari(int userId, string tema, string dil)
        {
            try
            {
                using var conn = new NpgsqlConnection(connectionString);
                conn.Open();

                using var cmd = new NpgsqlCommand(
                    @"INSERT INTO kullanici_ayarlari (user_id, tema, dil) 
                      VALUES (@uid, @tema, @dil)
                      ON CONFLICT (user_id) DO UPDATE 
                      SET tema = @tema, dil = @dil",
                    conn);
                cmd.Parameters.AddWithValue("uid", userId);
                cmd.Parameters.AddWithValue("tema", tema);
                cmd.Parameters.AddWithValue("dil", dil);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch { return false; }
        }

        // KULLANICI İSTATİSTİKLERİ
        public KullaniciIstatistik? GetKullaniciIstatistikleri(int userId)
        {
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand(
                "SELECT * FROM kullanici_istatistikleri WHERE user_id = @uid",
                conn);
            cmd.Parameters.AddWithValue("uid", userId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new KullaniciIstatistik
                {
                    UserId = reader.GetInt32(0),
                    KullaniciAdi = reader.GetString(1),
                    PuanlananAnimeSayisi = reader.IsDBNull(2) ? 0 : Convert.ToInt32(reader.GetInt64(2)),
                    OrtalamaPuan = reader.IsDBNull(3) ? 0 : Convert.ToDouble(reader.GetDecimal(3)),
                    FavoriSayisi = reader.IsDBNull(4) ? 0 : Convert.ToInt32(reader.GetInt64(4)),
                    IzlemeListesiSayisi = reader.IsDBNull(5) ? 0 : Convert.ToInt32(reader.GetInt64(5))
                };
            }
            return null;
        }

        // ML İÇİN VERİ
        public List<(int userId, int animeId, int rating)> GetAllRatingsForML()
        {
            var ratings = new List<(int, int, int)>();
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand("SELECT user_id, anime_id, verilen_puan FROM puanlar", conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                ratings.Add((reader.GetInt32(0), reader.GetInt32(1), reader.GetInt32(2)));
            }
            return ratings;
        }

        // EXCEL EXPORT
        public List<Dictionary<string, object>> GetAllAnimesForExport()
        {
            var data = new List<Dictionary<string, object>>();
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            var query = @"SELECT a.anime_id, a.isim, a.ingilizce_isim, a.puan, 
                         a.bolum_sayisi, a.tip, a.yayin_tarihi,
                         STRING_AGG(t.tur_adi, ', ') as turler
                         FROM animeler a
                         LEFT JOIN anime_turler at ON a.anime_id = at.anime_id
                         LEFT JOIN turler t ON at.tur_id = t.tur_id
                         GROUP BY a.anime_id
                         ORDER BY a.isim";

            using var cmd = new NpgsqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var row = new Dictionary<string, object>
                {
                    ["ID"] = reader.GetInt32(0),
                    ["İsim"] = reader.GetString(1),
                    ["İngilizce İsim"] = reader.IsDBNull(2) ? "" : reader.GetString(2),
                    ["Puan"] = reader.IsDBNull(3) ? "" : reader.GetDouble(3).ToString(),
                    ["Bölüm Sayısı"] = reader.IsDBNull(4) ? "" : reader.GetString(4),
                    ["Tip"] = reader.IsDBNull(5) ? "" : reader.GetString(5),
                    ["Yayın Tarihi"] = reader.IsDBNull(6) ? "" : reader.GetString(6),
                    ["Türler"] = reader.IsDBNull(7) ? "" : reader.GetString(7)
                };
                data.Add(row);
            }
            return data;
        }

        // EN POPÜLER ANİMELER
        public List<Anime> GetPopularAnimes(int limit = 10)
        {
            var animeList = new List<Anime>();
            using var conn = new NpgsqlConnection(connectionString);
            conn.Open();

            var query = @"SELECT a.anime_id, a.isim, a.ingilizce_isim, a.puan, 
                         a.bolum_sayisi, a.tip, a.yayin_tarihi, a.resim_url,
                         COUNT(p.puan_id) as puan_sayisi
                         FROM animeler a
                         LEFT JOIN puanlar p ON a.anime_id = p.anime_id
                         GROUP BY a.anime_id
                         ORDER BY puan_sayisi DESC, a.puan DESC NULLS LAST
                         LIMIT @limit";

            using var cmd = new NpgsqlCommand(query, conn);
            cmd.Parameters.AddWithValue("limit", limit);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                animeList.Add(new Anime
                {
                    AnimeId = reader.GetInt32(0),
                    Isim = reader.GetString(1),
                    IngilizceIsim = reader.IsDBNull(2) ? null : reader.GetString(2),
                    Puan = reader.IsDBNull(3) ? null : reader.GetDouble(3),
                    BolumSayisi = reader.IsDBNull(4) ? null : reader.GetString(4),
                    Tip = reader.IsDBNull(5) ? null : reader.GetString(5),
                    YayinTarihi = reader.IsDBNull(6) ? null : reader.GetString(6),
                    ResimUrl = reader.IsDBNull(7) ? null : reader.GetString(7)
                });
            }
            return animeList;
        }
    }
}