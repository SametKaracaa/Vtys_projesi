# 🎯 HIZLI BAŞLANGIÇ - KAYIT SORUNU ÇÖZÜMÜ

## ⚠️ ÖNEMLİ: İlk Yapılacaklar

### 1. VERİTABANI BAĞLANTISINI TEST ET

Kayıt yapmadan önce MUTLAKA test edin:

```bash
dotnet run --test
```

**Beklenen Çıktı:**
```
=== VERİTABANI BAĞLANTI TESTİ ===
Bağlantı açılıyor...
✅ Bağlantı BAŞARILI!
✅ kullanicilar tablosu mevcut
Mevcut kullanıcı sayısı: X
```

**Hata alıyorsanız:**
- PostgreSQL çalışmıyor olabilir
- Bağlantı bilgileri yanlış olabilir
- Veritabanı oluşturulmamış olabilir

### 2. SQL TESTİ

`database_test.sql` dosyasını çalıştırın:

```bash
psql -U postgres -d Proje -f database_test.sql
```

Bu script:
- Tabloları kontrol eder
- Eksikleri tamamlar
- Test kullanıcısı oluşturur

### 3. BAĞLANTI BİLGİLERİNİ GÜNCELLEYIN

`Program.cs` dosyasında:

```csharp
var connectionString = "Host=localhost;Port=5432;Database=Proje;Username=postgres;Password=BURAYA_SİFRENİZ";
```

## 🔍 KAYIT SORUNUNU ÇÖZME

### Durum 1: "Kayıt başarısız!" Mesajı

**Konsolu kontrol edin:**

```bash
dotnet run
# Konsol penceresini açık tutun!
```

**Loglar şöyle görünmeli:**
```
Kayıt başlatılıyor... Kullanıcı: deneme
Şifre hashleniyor...
Hash tamamlandı. Uzunluk: 60
Veritabanına kayıt yapılıyor...
✅ Kayıt BAŞARILI!
```

**Hata görüyorsanız:**
```
❌ Kayıt BAŞARISIZ!
KAYIT HATASI: [detaylı hata mesajı]
```

Hata mesajına göre:

1. **"duplicate key"** → Kullanıcı adı zaten var
2. **"connection"** → PostgreSQL çalışmıyor
3. **"relation does not exist"** → Tablo yok (database_test.sql çalıştırın)
4. **"authentication"** → Şifre yanlış

### Durum 2: Hiçbir Mesaj Yok

**BCrypt kontrolü:**

```bash
dotnet run --test
```

BCrypt testi BAŞARILI olmalı.

**Değilse:**
```bash
dotnet add package BCrypt.Net-Next --version 4.0.3
dotnet restore
```

### Durum 3: Tablo Bulunamadı

**Manuel tablo oluşturma:**

```sql
CREATE TABLE kullanicilar (
    user_id SERIAL PRIMARY KEY,
    kullanici_adi VARCHAR(100) UNIQUE NOT NULL,
    sifre VARCHAR(255) NOT NULL,
    cinsiyet VARCHAR(20),
    dogum_tarihi DATE,
    rol VARCHAR(20) DEFAULT 'USER',
    kayit_tarihi TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

PostgreSQL'de çalıştırın.

## 🎌 ÖNERİ SİSTEMİ KULLANIMI

### Adım 1: Anime Puanlayın

En az **5-10 anime** puanlayın:
1. Ana ekranda anime seçin
2. Kaydırma çubuğuyla puan verin (0-10)
3. "Puan Ver" butonuna tıklayın

### Adım 2: Favorilere Ekleyin (Opsiyonel ama Önerilen)

**2-3 anime** favorilere ekleyin:
- Favorideki animeler öneri skorunda +2 bonus alır!

### Adım 3: Önerileri Alın

1. "✨ Öneriler" butonuna tıklayın
2. İlk kez kullanıyorsanız "🧠 Modeli Eğit" butonuna tıklayın
3. Öneriler yüklenir

### Her Yenilemede Farklı Sonuçlar!

**Nasıl çalışır:**
- En iyi 15 anime hesaplanır
- İlk 2 anime kesin seçilir (en yüksek skor)
- Kalan 3 anime ağırlıklı rastgele seçilir
- Her yenilemede farklı kombinasyon!

**Öneri Türleri:**
- ⭐ **Favori**: Benzer kullanıcıların favorisi (+2 puan)
- 👥 **Tavsiye**: Benzer kullanıcıların yüksek puanı
- 🔥 **Popüler**: Yeterli veri yoksa

## 🚨 SIKÇA SORULAN SORULAR

### S: Kayıt butonu çalışmıyor?

**C:** Konsol loglarını kontrol edin:
```bash
dotnet run
# Konsol penceresini kapatmayın!
```

### S: PostgreSQL çalışıyor mu nasıl anlarım?

**C:** Terminal'de:
```bash
pg_isready -h localhost -p 5432
```

Veya:
```bash
psql -U postgres -c "SELECT version();"
```

### S: Tablo var mı nasıl kontrol ederim?

**C:**
```bash
psql -U postgres -d Proje -c "\dt"
```

### S: Öneriler hep aynı geliyor?

**C:** 
- Sayfayı yenileyin
- Daha fazla anime puanlayın
- Favorilere anime ekleyin

### S: Öneriler boş geliyor?

**C:**
- En az 5 anime puanladınız mı?
- "Modeli Eğit" butonuna tıkladınız mı?
- Veritabanında başka kullanıcılar var mı?

## 📞 DESTEK ALMAK İÇİN

1. **Test modunu çalıştırın:**
   ```bash
   dotnet run --test
   ```

2. **Çıktıyı kaydedin**

3. **Konsol loglarını kaydedin:**
   ```bash
   dotnet run > log.txt 2>&1
   ```

4. **Veritabanı kontrolü:**
   ```bash
   psql -U postgres -d Proje -f database_test.sql > db_test.txt
   ```

## ✅ BAŞARILI KURULUM KONTROL LİSTESİ

- [ ] PostgreSQL çalışıyor
- [ ] `dotnet run --test` başarılı
- [ ] `database_test.sql` çalıştırıldı
- [ ] Bağlantı bilgileri doğru
- [ ] NuGet paketleri yüklendi
- [ ] Konsol logları görünüyor
- [ ] Test kullanıcısı oluşturuldu

Tümü işaretliyse → Kayıt çalışacaktır! 🎉

## 🎯 SON KONTROL

```bash
# 1. Test
dotnet run --test

# 2. Çalıştır
dotnet run

# 3. Konsol aç - logları izle

# 4. Kayıt ol

# 5. Başarılı! 🎉
```

---

**Sorun mu var?**
- Konsol loglarını kontrol edin
- Test modunu çalıştırın
- README_v2.2.md dosyasına bakın

**Her şey yolunda mı?**
- Animeleri puanlayın
- Favorilere ekleyin
- Önerileri alın
- Keyfi çıkarın! 🎌
