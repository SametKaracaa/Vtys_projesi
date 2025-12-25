# Anime Veritabanı Masaüstü Uygulaması


## Özellikler

### Kullanıcı Özellikleri
- ✅ Kullanıcı kayıt ve giriş sistemi
- ✅ Anime listesini görüntüleme
- ✅ Anime arama ve filtreleme (isim ve türe göre)
- ✅ Anime detaylarını görüntüleme (resim, açıklama, türler)
- ✅ Anime puanlama (0-10 arası)
- ✅ Kullanıcının verdiği puanları görüntüleme

### Admin Özellikleri
- 🔧 Yeni anime ekleme
- 🔧 Mevcut animeleri düzenleme
- 🔧 Anime silme
- 📊 İstatistikleri görüntüleme

## Gereksinimler

- .NET 10 SDK veya üzeri
- PostgreSQL 12 veya üzeri
- Windows işletim sistemi

## Kurulum

### 1. Veritabanı Kurulumu

PostgreSQL'de `Proje` adında bir veritabanı oluşturun ve `proje.sql` dosyasını içe aktarın:

```bash
psql -U postgres -d Proje -f proje.sql
```

### 2. Bağlantı Ayarları

`Program.cs` dosyasındaki bağlantı dizesini (connection string) güncelleyin:

```csharp
var connectionString = "Host=localhost;Port=5432;Database=Proje;Username=postgres;Password=YourPassword";
```

### 3. Projeyi Derleme

Terminal veya komut istemcisinde proje klasöründe:

```bash
dotnet build
```

### 4. Uygulamayı Çalıştırma

```bash
dotnet run
```

veya Visual Studio ile projeyi açıp F5 ile çalıştırabilirsiniz.

## Kullanım

### İlk Giriş

1. Uygulama açıldığında giriş ekranı görünür
2. **Kayıt Ol** butonuna tıklayarak yeni kullanıcı oluşturun
3. Kullanıcı adı ve şifre ile giriş yapın

### Anime Listeleme ve Puanlama

1. Ana ekranda tüm animeler listelenir
2. Arama kutusuna yazarak veya tür filtresini kullanarak anime arayabilirsiniz
3. Listeden bir anime seçtiğinizde sağ tarafta detayları görünür
4. Kaydırma çubuğu (slider) ile 0-10 arası puan verebilirsiniz
5. **Puan Ver** butonuna tıklayarak puanınızı kaydedin

### Admin İşlemleri

Admin kullanıcılar için (veritabanında `rol = 'ADMIN'` olan kullanıcılar):

1. Sağ üstteki **Admin Panel** butonuna tıklayın
2. Yeni anime ekleyebilir, mevcut animeleri düzenleyebilir veya silebilirsiniz
3. İstatistikleri görüntüleyebilirsiniz

### Admin Kullanıcı Oluşturma

Veritabanında admin kullanıcı oluşturmak için PostgreSQL'de:

```sql
INSERT INTO kullanicilar (kullanici_adi, sifre, rol) 
VALUES ('admin', 'admin123', 'ADMIN');
```

## Proje Yapısı

```
AnimeApp/
├── AnimeApp.csproj           # Proje dosyası
├── Program.cs                # Ana giriş noktası
├── Models.cs                 # Veri modelleri
├── DatabaseManager.cs        # Veritabanı işlemleri
├── LoginForm.cs             # Giriş formu
├── KayitForm.cs             # Kayıt formu
├── MainForm.cs              # Ana uygulama formu
├── AdminForm.cs             # Admin paneli
└── AnimeEditForm.cs         # Anime ekleme/düzenleme formu
```

## Veritabanı Şeması

- **animeler**: Anime bilgileri
- **kullanicilar**: Kullanıcı bilgileri
- **turler**: Anime türleri
- **anime_turler**: Anime-tür ilişkisi
- **puanlar**: Kullanıcıların verdiği puanlar

## Teknolojiler

- **C# 12** - Programlama dili
- **.NET 10** - Framework
- **Windows Forms** - UI framework
- **Npgsql 6.0.11** - PostgreSQL bağlantı kütüphanesi
- **PostgreSQL** - Veritabanı

## Özellikler Detayı

### Güvenlik
- Şifreler düz metin olarak saklanır (production ortamında hash kullanılmalıdır)
- SQL injection koruması (parameterized queries)

### UI/UX
- Modern ve temiz arayüz
- Renkli butonlar ve paneller
- Responsive tasarım
- Anime resimleri otomatik yüklenir

### Performans
- Veritabanı bağlantıları using bloklarıyla otomatik kapatılır
- Asenkron resim yükleme
- Efficient SQL sorguları

## Gelecek Geliştirmeler

- [ ] Şifre hash'leme (bcrypt veya PBKDF2)
- [ ] Kullanıcı profil sayfası
- [ ] Favori anime listesi
- [ ] Anime önerileri
- [ ] Export/Import özelliği
- [ ] Dark mode
- [ ] Çoklu dil desteği

## Lisans

Bu proje eğitim amaçlı geliştirilmiştir.

## Sorun Giderme

### Bağlantı Hatası
- PostgreSQL servisinin çalıştığından emin olun
- Bağlantı dizesindeki bilgileri kontrol edin
- Firewall ayarlarını kontrol edin

### Npgsql Paketi Bulunamadı
```bash
dotnet add package Npgsql --version 6.0.11
```

### Resimler Görünmüyor
- Internet bağlantınızı kontrol edin
- Resim URL'lerinin geçerli olduğundan emin olun
