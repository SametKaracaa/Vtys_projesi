# Anime Veritabanı - Yeni Özellikler Rehberi

## 📋 İçindekiler
1. [Kurulum](#kurulum)
2. [Yeni Özellikler](#yeni-özellikler)
3. [Veritabanı Güncellemeleri](#veritabanı-güncellemeleri)
4. [Kullanım Kılavuzu](#kullanım-kılavuzu)
5. [Teknik Detaylar](#teknik-detaylar)

---

## 🚀 Kurulum

### 1. NuGet Paketlerini Yükleyin

Proje dizininde aşağıdaki komutu çalıştırın:

```bash
dotnet restore
```

Veya Visual Studio'da:
- Solution Explorer'da projeye sağ tıklayın
- "Manage NuGet Packages" seçin
- Aşağıdaki paketlerin yüklü olduğundan emin olun:
  - Npgsql (6.0.11)
  - BCrypt.Net-Next (4.0.3)
  - EPPlus (7.0.5)
  - Microsoft.ML (3.0.1)
  - Newtonsoft.Json (13.0.3)

### 2. Veritabanını Güncelleyin

PostgreSQL'de `Proje` veritabanında aşağıdaki SQL scriptini çalıştırın:

```bash
psql -U postgres -d Proje -f database_updates.sql
```

Veya pgAdmin'de `database_updates.sql` dosyasını açıp çalıştırın.

### 3. Mevcut Şifreleri Güncelleme (ÖNEMLİ!)

Mevcut kullanıcılarınız varsa, şifrelerini hash'lenmiş formata dönüştürmeniz gerekir.
Yeni kayıtlar otomatik olarak hash'lenecektir.

**Seçenek 1:** Tüm kullanıcıları yeniden kaydedin (önerilir)
**Seçenek 2:** Manuel olarak şifreleri güncelleyin (gelişmiş kullanıcılar için)

---

## ✨ Yeni Özellikler

### 1. 🔐 Şifre Güvenliği (BCrypt Hash)

**Ne Değişti:**
- Şifreler artık düz metin olarak saklanmıyor
- BCrypt algoritması ile güvenli hash'leme
- Salt ve pepper ile ekstra güvenlik

**Kullanıcı Deneyimi:**
- Kayıt ve giriş süreçlerinde değişiklik yok
- Arka planda otomatik olarak çalışır

**Kod Örneği:**
```csharp
// Kayıt
string hashedPassword = BCrypt.Net.BCrypt.HashPassword(sifre);

// Giriş
bool isValid = BCrypt.Net.BCrypt.Verify(sifre, hashedPassword);
```

---

### 2. 👤 Kullanıcı Profil Sayfası

**Özellikler:**
- Kullanıcı istatistikleri görüntüleme
- Tema değiştirme (Light/Dark)
- Favorileri görüntüleme
- Puanlanan animeleri listeleme

**Nasıl Erişilir:**
Ana ekranda üst sağ köşedeki "👤 Profil" butonuna tıklayın.

**Gösterilen İstatistikler:**
- 📺 Puanlanan Anime Sayısı
- ⭐ Ortalama Puan
- ❤️ Favori Sayısı
- 📋 İzleme Listesi Sayısı

---

### 3. ❤️ Favori Listeleri

**Özellikler:**
- Animeleri favorilere ekleme/çıkarma
- Tüm favorileri görüntüleme
- Favori animeleri Excel'e aktarma

**Nasıl Kullanılır:**
1. Anime seçin
2. Detay panelinde "❤️ Favorilere Ekle" butonuna tıklayın
3. Tüm favorilerinizi görmek için "❤️ Favoriler" butonuna tıklayın

**Veritabanı:**
```sql
CREATE TABLE favoriler (
    favori_id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL,
    anime_id INTEGER NOT NULL,
    eklenme_zamani TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

---

### 4. 🌙 Dark Mode ve Tema Sistemi

**Temalar:**
- 🌞 Light Mode (Varsayılan)
- 🌙 Dark Mode

**Nasıl Değiştirilir:**
1. "👤 Profil" butonuna tıklayın
2. Tema açılır menüsünden istediğiniz temayı seçin
3. "Ayarları Kaydet" butonuna tıklayın

**Özellikler:**
- Tüm formlar otomatik olarak temayı uygular
- Ayarlar veritabanında saklanır
- Her oturum açışta kaydedilen tema yüklenir

**Renk Şemaları:**

Light Mode:
- Arka plan: Beyaz
- Yazı: Siyah
- Paneller: Açık Gri
- Butonlar: Mavi

Dark Mode:
- Arka plan: Koyu Gri (#202020)
- Yazı: Açık Beyaz (#DCDCDC)
- Paneller: Koyu Gri (#2D2D2D)
- Butonlar: Koyu Gri (#3C3C3C)

---

### 5. 📊 Excel Export/Import

**Export Seçenekleri:**
1. **Tüm Animeler**: Veritabanındaki tüm animeleri Excel'e aktar
2. **Puanladıklarım**: Sadece puanladığınız animeleri aktar
3. **Favorilerim**: Favori animelerinizi aktar

**Nasıl Kullanılır:**
1. "📊 Export" butonuna tıklayın
2. İstediğiniz export türünü seçin
3. Dosya adı ve konumu belirleyin
4. Kaydet

**Excel Formatı:**
- Başlıklar renklendirilerek vurgulanır
- Otomatik sütun genişliği ayarı
- Her export türü farklı renk şeması kullanır

**Kod Örneği:**
```csharp
// Tüm animeleri export et
var data = db.GetAllAnimesForExport();
ExcelManager.ExportToExcel(data, "AnimeListe.xlsx");

// Favorileri export et
ExcelManager.ExportFavorites(db, userId, "Favorilerim.xlsx");
```

---

### 6. 🤖 Makine Öğrenmesi Tabanlı Anime Önerileri

**Özellikler:**
- Collaborative Filtering (İşbirlikçi Filtreleme)
- Matrix Factorization algoritması
- Beğenilerinize göre kişiselleştirilmiş öneriler
- Benzer türdeki animeleri önerme

**Nasıl Kullanılır:**
1. Ana ekranda "✨ Öneriler" butonuna tıklayın
2. İlk kullanımda "🧠 Modeli Eğit" butonuna tıklayın
3. Model eğitildikten sonra size özel öneriler görüntülenir

**Öneri Türleri:**
- **Beğenilerinize Göre**: ML modeli ile tahmin edilen yüksek puanlı animeler
- **Popüler Animeler**: Yeterli veri yoksa en çok puanlanan animeler
- **Benzer Animeler**: Aynı türdeki yüksek puanlı animeler

**Minimum Gereksinim:**
- En az 10 anime puanlaması gerekir
- Daha fazla puanlama = daha iyi öneriler

**Algoritma:**
```csharp
// Matrix Factorization
var options = new MatrixFactorizationTrainer.Options
{
    MatrixColumnIndexColumnName = "UserId",
    MatrixRowIndexColumnName = "AnimeId",
    LabelColumnName = "Label",
    NumberOfIterations = 20,
    ApproximationRank = 10,
    LearningRate = 0.1
};
```

---

## 💾 Veritabanı Güncellemeleri

### Yeni Tablolar

#### 1. favoriler
```sql
CREATE TABLE favoriler (
    favori_id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL,
    anime_id INTEGER NOT NULL,
    eklenme_zamani TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES kullanicilar(user_id),
    FOREIGN KEY (anime_id) REFERENCES animeler(anime_id),
    UNIQUE (user_id, anime_id)
);
```

#### 2. kullanici_ayarlari
```sql
CREATE TABLE kullanici_ayarlari (
    ayar_id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL UNIQUE,
    tema VARCHAR(20) DEFAULT 'Light',
    dil VARCHAR(10) DEFAULT 'TR',
    FOREIGN KEY (user_id) REFERENCES kullanicilar(user_id)
);
```

#### 3. izleme_listesi (Gelecek Özellik)
```sql
CREATE TABLE izleme_listesi (
    liste_id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL,
    anime_id INTEGER NOT NULL,
    durum VARCHAR(20) DEFAULT 'İzleniyor',
    baslangic_tarihi DATE,
    bitis_tarihi DATE,
    FOREIGN KEY (user_id) REFERENCES kullanicilar(user_id),
    FOREIGN KEY (anime_id) REFERENCES animeler(anime_id),
    UNIQUE (user_id, anime_id)
);
```

### Yeni View

#### kullanici_istatistikleri
```sql
CREATE VIEW kullanici_istatistikleri AS
SELECT 
    k.user_id,
    k.kullanici_adi,
    COUNT(DISTINCT p.anime_id) as puanlanan_anime_sayisi,
    ROUND(AVG(p.verilen_puan)::numeric, 2) as ortalama_puan,
    COUNT(DISTINCT f.anime_id) as favori_sayisi,
    COUNT(DISTINCT il.anime_id) as izleme_listesi_sayisi
FROM kullanicilar k
LEFT JOIN puanlar p ON k.user_id = p.user_id
LEFT JOIN favoriler f ON k.user_id = f.user_id
LEFT JOIN izleme_listesi il ON k.user_id = il.user_id
GROUP BY k.user_id, k.kullanici_adi;
```

### Performans İndexleri
```sql
CREATE INDEX idx_favoriler_user ON favoriler(user_id);
CREATE INDEX idx_favoriler_anime ON favoriler(anime_id);
CREATE INDEX idx_izleme_user ON izleme_listesi(user_id);
CREATE INDEX idx_puanlar_user ON puanlar(user_id);
```

---

## 📚 Kullanım Kılavuzu

### Yeni Bir Kullanıcı İçin

1. **Kayıt Ol**
   - Kullanıcı adı ve şifre belirle (şifre otomatik hash'lenir)
   - Opsiyonel: Cinsiyet ve doğum tarihi ekle

2. **Anime Puanla**
   - En az 5-10 anime puanlayın (ML için)
   - Puanlar 0-10 arası olabilir

3. **Favorilere Ekle**
   - Beğendiğiniz animeleri favorilere ekleyin
   - Favorilerinizi kolayca görüntüleyin

4. **Profil Ayarları**
   - Temayı değiştirin (Light/Dark)
   - İstatistiklerinizi görüntüleyin

5. **Öneriler Al**
   - "Öneriler" butonuna tıklayın
   - İlk kez kullanıyorsanız "Modeli Eğit" butonuna tıklayın
   - Size özel önerileri görüntüleyin

6. **Export**
   - Listelerinizi Excel'e aktarın
   - Offline erişim için kaydedin

---

## 🔧 Teknik Detaylar

### Yeni Dosyalar

1. **TemaYoneticisi.cs**: Tema sistemi
2. **ProfilForm.cs**: Kullanıcı profil formu
3. **OnerilerForm.cs**: Öneri sistemi formu
4. **AnimeRecommendationEngine.cs**: ML öneri motoru
5. **ExcelManager.cs**: Excel export işlemleri
6. **MainFormExtensions.cs**: MainForm için ek metodlar
7. **database_updates.sql**: Veritabanı güncellemeleri

### Değiştirilen Dosyalar

1. **AnimeApp.csproj**: Yeni NuGet paketleri
2. **Models.cs**: Yeni modeller eklendi
3. **DatabaseManager.cs**: Yeni metodlar
4. **KayitForm.cs**: BCrypt entegrasyonu
5. **MainForm.cs**: Yeni butonlar ve özellikler

### Bağımlılıklar

```xml
<PackageReference Include="Npgsql" Version="6.0.11" />
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="EPPlus" Version="7.0.5" />
<PackageReference Include="Microsoft.ML" Version="3.0.1" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.3" />
```

### Mimari

```
AnimeApp/
├── Forms/
│   ├── LoginForm.cs
│   ├── KayitForm.cs
│   ├── MainForm.cs
│   ├── MainFormExtensions.cs (YENİ)
│   ├── ProfilForm.cs (YENİ)
│   ├── OnerilerForm.cs (YENİ)
│   ├── AdminForm.cs
│   └── AnimeEditForm.cs
├── Database/
│   └── DatabaseManager.cs
├── Models/
│   └── Models.cs
├── UI/
│   └── TemaYoneticisi.cs (YENİ)
├── ML/
│   └── AnimeRecommendationEngine.cs (YENİ)
└── Utilities/
    └── ExcelManager.cs (YENİ)
```

---

## 🐛 Sorun Giderme

### Şifre Hatası
**Problem:** Mevcut kullanıcılar giriş yapamıyor
**Çözüm:** Kullanıcıları yeniden kaydettirin veya şifreleri manuel olarak hash'leyin

### ML Modeli Eğitilemedi
**Problem:** "Yeterli veri yok" hatası
**Çözüm:** En az 10 anime puanlayın

### Excel Oluşturulamadı
**Problem:** EPPlus lisans hatası
**Çözüm:** Kod otomatik olarak NonCommercial lisans ayarlar

### Tema Uygulanmadı
**Problem:** Dark mode çalışmıyor
**Çözüm:** Profil sayfasından temayı kaydedin ve uygulamayı yeniden başlatın

---

## 🚀 Gelecek Özellikler (Roadmap)

- [ ] İzleme Listesi (İzleniyor, Tamamlandı, Bırakıldı)
- [ ] Anime Karşılaştırma
- [ ] Gelişmiş Arama (Türlere göre çoklu filtreleme)
- [ ] Kullanıcı Yorumları
- [ ] Social Features (Arkadaş ekleme, öneri paylaşma)
- [ ] Notification Sistemi
- [ ] Mobile App (Xamarin/MAUI)
- [ ] Web Arayüzü (ASP.NET Core)

---

## 📝 Değişiklik Günlüğü

### v2.0.0 (2024-12-20)

#### Eklenenler
- ✅ BCrypt ile şifre hash'leme
- ✅ Kullanıcı profil sayfası
- ✅ Favori listeleri
- ✅ Dark mode ve tema sistemi
- ✅ Excel export (Tüm animeler, puanlar, favoriler)
- ✅ ML tabanlı anime önerileri
- ✅ Kullanıcı istatistikleri
- ✅ Veritabanı performans iyileştirmeleri

#### Değiştirilenler
- 🔄 Login sistemi BCrypt kullanacak şekilde güncellendi
- 🔄 MainForm yeni butonlar ile genişletildi
- 🔄 DatabaseManager yeni metodlar ile güncellendi

#### Güvenlik
- 🔒 Şifreler artık hash'lenerek saklanıyor
- 🔒 SQL injection koruması devam ediyor
- 🔒 Parametreli sorgular kullanılıyor

---

## 👥 Katkıda Bulunanlar

- Proje Sahibi: [Sizin Adınız]
- AI Asistan: Claude (Anthropic)

---

## 📄 Lisans

Bu proje eğitim amaçlıdır.

---

## 📞 İletişim

Sorularınız için:
- GitHub Issues
- Email: [email@example.com]

---

**Keyifli kullanımlar!** 🎉
