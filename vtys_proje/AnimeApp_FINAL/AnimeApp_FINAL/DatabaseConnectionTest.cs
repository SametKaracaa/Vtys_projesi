using Npgsql;
using BCrypt.Net;

namespace AnimeApp.Testing
{
    public class DatabaseConnectionTest
    {
        public static void TestConnection(string connectionString)
        {
            Console.WriteLine("=== VERİTABANI BAĞLANTI TESTİ ===\n");
            
            try
            {
                Console.WriteLine($"Bağlantı dizesi: {connectionString}\n");
                
                using var conn = new NpgsqlConnection(connectionString);
                Console.WriteLine("Bağlantı açılıyor...");
                conn.Open();
                Console.WriteLine("✅ Bağlantı BAŞARILI!\n");
                
                // Veritabanı versiyonunu al
                using var cmd = new NpgsqlCommand("SELECT version();", conn);
                var version = cmd.ExecuteScalar()?.ToString();
                Console.WriteLine($"PostgreSQL Versiyonu:\n{version}\n");
                
                // Kullanıcılar tablosunu kontrol et
                Console.WriteLine("Kullanıcılar tablosu kontrol ediliyor...");
                cmd.CommandText = @"
                    SELECT COUNT(*) 
                    FROM information_schema.tables 
                    WHERE table_name = 'kullanicilar'";
                var tableExists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                
                if (tableExists)
                {
                    Console.WriteLine("✅ kullanicilar tablosu mevcut");
                    
                    // Sütunları listele
                    cmd.CommandText = @"
                        SELECT column_name, data_type 
                        FROM information_schema.columns 
                        WHERE table_name = 'kullanicilar' 
                        ORDER BY ordinal_position";
                    
                    using var reader = cmd.ExecuteReader();
                    Console.WriteLine("\nTablo yapısı:");
                    while (reader.Read())
                    {
                        Console.WriteLine($"  - {reader.GetString(0)} ({reader.GetString(1)})");
                    }
                    reader.Close();
                    
                    // Kullanıcı sayısını göster
                    cmd.CommandText = "SELECT COUNT(*) FROM kullanicilar";
                    var userCount = Convert.ToInt32(cmd.ExecuteScalar());
                    Console.WriteLine($"\nMevcut kullanıcı sayısı: {userCount}");
                    
                    // İlk 5 kullanıcıyı göster
                    if (userCount > 0)
                    {
                        cmd.CommandText = "SELECT user_id, kullanici_adi, rol FROM kullanicilar LIMIT 5";
                        using var userReader = cmd.ExecuteReader();
                        Console.WriteLine("\nKayıtlı kullanıcılar (ilk 5):");
                        while (userReader.Read())
                        {
                            Console.WriteLine($"  - ID: {userReader.GetInt32(0)}, " +
                                            $"Kullanıcı: {userReader.GetString(1)}, " +
                                            $"Rol: {userReader.GetString(2)}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("❌ kullanicilar tablosu bulunamadı!");
                    Console.WriteLine("\nTablo oluşturmak için database_test.sql dosyasını çalıştırın.");
                }
                
                // Diğer tabloları kontrol et
                Console.WriteLine("\n=== DİĞER TABLOLAR ===");
                var tables = new[] { "animeler", "turler", "puanlar", "favoriler" };
                foreach (var table in tables)
                {
                    cmd.CommandText = $"SELECT COUNT(*) FROM {table}";
                    try
                    {
                        var count = Convert.ToInt32(cmd.ExecuteScalar());
                        Console.WriteLine($"✅ {table}: {count} kayıt");
                    }
                    catch
                    {
                        Console.WriteLine($"❌ {table}: Tablo bulunamadı!");
                    }
                }
                
                Console.WriteLine("\n=== BAĞLANTI TESTİ TAMAMLANDI ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n❌ BAĞLANTI HATASI!\n");
                Console.WriteLine($"Hata: {ex.Message}");
                Console.WriteLine($"\nDetaylar:\n{ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"\nİç Hata: {ex.InnerException.Message}");
                }
                
                Console.WriteLine("\n=== ÇÖZÜM ÖNERİLERİ ===");
                Console.WriteLine("1. PostgreSQL servisinin çalıştığından emin olun");
                Console.WriteLine("2. Bağlantı dizesindeki bilgileri kontrol edin (Host, Port, Database, Username, Password)");
                Console.WriteLine("3. Veritabanının oluşturulduğundan emin olun");
                Console.WriteLine("4. Kullanıcı izinlerini kontrol edin");
            }
        }
        
        public static void TestBCrypt()
        {
            Console.WriteLine("\n=== BCRYPT TESTİ ===\n");
            
            try
            {
                var testPassword = "test123";
                Console.WriteLine($"Test şifresi: {testPassword}");
                
                Console.WriteLine("Hash oluşturuluyor...");
                var hash = BCrypt.Net.BCrypt.HashPassword(testPassword);
                Console.WriteLine($"✅ Hash oluşturuldu: {hash}");
                Console.WriteLine($"Hash uzunluğu: {hash.Length}");
                
                Console.WriteLine("\nDoğrulama testi...");
                var isValid = BCrypt.Net.BCrypt.Verify(testPassword, hash);
                Console.WriteLine(isValid ? "✅ Doğrulama BAŞARILI!" : "❌ Doğrulama BAŞARISIZ!");
                
                var wrongPassword = "wrong123";
                var isInvalid = BCrypt.Net.BCrypt.Verify(wrongPassword, hash);
                Console.WriteLine(isInvalid ? "❌ Yanlış şifre doğrulandı! (BUG!)" : "✅ Yanlış şifre reddedildi!");
                
                Console.WriteLine("\n=== BCRYPT TESTİ TAMAMLANDI ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ BCRYPT HATASI: {ex.Message}");
            }
        }
        
        public static void TestUserRegistration(string connectionString)
        {
            Console.WriteLine("\n=== KAYIT TESTİ ===\n");
            
            try
            {
                using var conn = new NpgsqlConnection(connectionString);
                conn.Open();
                
                var testUser = $"testuser_{DateTime.Now.Ticks}";
                var testPass = "test123";
                var hashedPass = BCrypt.Net.BCrypt.HashPassword(testPass);
                
                Console.WriteLine($"Test kullanıcısı: {testUser}");
                Console.WriteLine($"Şifre: {testPass}");
                Console.WriteLine($"Hash: {hashedPass.Substring(0, 30)}...");
                
                using var cmd = new NpgsqlCommand(
                    @"INSERT INTO kullanicilar (kullanici_adi, sifre, rol) 
                      VALUES (@user, @pass, 'USER') RETURNING user_id", conn);
                cmd.Parameters.AddWithValue("user", testUser);
                cmd.Parameters.AddWithValue("pass", hashedPass);
                
                var userId = Convert.ToInt32(cmd.ExecuteScalar());
                Console.WriteLine($"\n✅ Kullanıcı başarıyla oluşturuldu! ID: {userId}");
                
                // Temizle
                cmd.CommandText = "DELETE FROM kullanicilar WHERE user_id = @id";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("id", userId);
                cmd.ExecuteNonQuery();
                Console.WriteLine("Test kullanıcısı silindi.");
                
                Console.WriteLine("\n=== KAYIT TESTİ BAŞARILI ===");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ KAYIT TESTİ BAŞARISIZ!\n{ex.Message}");
            }
        }
    }
}
