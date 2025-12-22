using AnimeApp.Database;
using AnimeApp.Forms;
using AnimeApp.Testing;

namespace AnimeApp
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Veritabanı bağlantı bilgileri
            // Bu bilgileri kendi PostgreSQL veritabanınıza göre güncelleyin
            var connectionString = "Host=localhost;Port=5432;Database=Proje;Username=postgres;Password=123456";

            // Test modu kontrolü
            if (args.Length > 0 && args[0] == "--test")
            {
                Console.WriteLine("TEST MODU BAŞLATILIYOR...\n");
                DatabaseConnectionTest.TestConnection(connectionString);
                DatabaseConnectionTest.TestBCrypt();
                DatabaseConnectionTest.TestUserRegistration(connectionString);
                Console.WriteLine("\nTesti sonlandırmak için bir tuşa basın...");
                Console.ReadKey();
                return;
            }

            ApplicationConfiguration.Initialize();

            var db = new DatabaseManager(connectionString);

            // Konsol logları için
            Console.WriteLine("Anime Veritabanı Uygulaması Başlatılıyor...");
            Console.WriteLine($"Bağlantı: {connectionString.Replace("Password=123456", "Password=***")}");
            Console.WriteLine("\nKonsol loglarını görmek için bu pencereyi açık tutun.\n");

            // Login formu göster
            using var loginForm = new LoginForm(db);
            if (loginForm.ShowDialog() == DialogResult.OK && loginForm.LoggedInUser != null)
            {
                Console.WriteLine($"✅ Giriş başarılı: {loginForm.LoggedInUser.KullaniciAdi}");
                // Ana formu aç
                Application.Run(new MainForm(db, loginForm.LoggedInUser));
            }
            
            Console.WriteLine("Uygulama kapatıldı.");
        }
    }
}
