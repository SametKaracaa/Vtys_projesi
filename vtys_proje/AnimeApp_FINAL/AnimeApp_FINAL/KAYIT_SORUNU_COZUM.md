# 🚨 KAYIT SORUNU ÇÖZÜMÜ - ADIM ADIM

## ⚠️ ÖNEMLİ: İLK OKUYUN!

Bu versiyon **TEST MODU**ndadır. BCrypt şifreleme geçici olarak devre dışı bırakıldı.
Kayıt çalıştıktan sonra BCrypt'i tekrar aktif edeceğiz.

## 🎯 AMACIMIZ

Kayıt işleminin neden çalışmadığını bulmak ve düzeltmek.

## 📋 ADIM ADIM TALİMATLAR

### 1. KONSOL PENCERESİNİ AÇ

**ÇOK ÖNEMLİ!** Uygulamayı Visual Studio'dan çalıştırıyorsanız:

- View → Output (veya Ctrl+Alt+O)
- Veya doğrudan Command Prompt'tan çalıştırın:
  ```bash
  cd [ProjeKlasörü]
  dotnet run
  ```

**Konsol penceresini KAPATAMAYIN!** Tüm hata mesajları orada görünecek.

### 2. UYGULAMAYI ÇALIŞTIR

```bash
dotnet run
```

### 3. KAYIT FORMUNU AÇ

- Ana ekranda "Kayıt Ol" butonuna tıklayın

### 4. BİLGİLERİ GİRİN

- **Kullanıcı adı:** test123 (veya istediğiniz)
- **Şifre:** 1234 (veya en az 4 karakter)
- **Şifre (Tekrar):** 1234 (aynı şifre)
- **Cinsiyet:** İsteğe bağlı
- **Doğum Tarihi:** İsteğe bağlı

### 5. "KAYIT OL" BUTONU

Butona tıkladığınızda KONSOL'DA şunları göreceksiniz:

```
╔════════════════════════════════════════╗
║   VERİTABANI KAYIT İŞLEMİ BAŞLIYOR    ║
╚════════════════════════════════════════╝

📡 ADIM 1: Bağlantı oluşturuluyor...
   Connection String: Host=localhost;Port=5432;Database=Proje;Username=postgres;Password=***

🔓 ADIM 2: Bağlantı açılıyor...
   ✅ Bağlantı başarıyla açıldı!

🐘 ADIM 3: PostgreSQL versiyonu kontrol ediliyor...
   ✅ PostgreSQL çalışıyor: PostgreSQL 14.1...

🔍 ADIM 4: 'test123' kullanıcı adı kontrol ediliyor...
   Bulunan kayıt sayısı: 0
   ✅ Kullanıcı adı kullanılabilir!

📋 ADIM 5: kullanicilar tablosu yapısı kontrol ediliyor...
   ✅ Tablo bulundu! Sütunlar (7 adet):
      • user_id
      • kullanici_adi
      • sifre
      • cinsiyet
      • dogum_tarihi
      • rol
      • kayit_tarihi

💾 ADIM 6: INSERT sorgusu hazırlanıyor...
   Sorgu: INSERT INTO kullanicilar (kullanici_adi, sifre, cinsiyet, dogum_tarihi, rol, kayit_tarihi) 
          VALUES (@kad, @sifre, @cinsiyet, @dogum, 'USER', CURRENT_TIMESTAMP)

📝 ADIM 7: Parametreler hazırlanıyor...
   • kullanici_adi: 'test123'
   • sifre: '****' (uzunluk: 4)
   • cinsiyet: NULL
   • dogum_tarihi: NULL
   • rol: USER

🚀 ADIM 8: INSERT komutu çalıştırılıyor...
   ✅ Komut başarılı! 1 satır eklendi.

✔️ ADIM 9: Kayıt doğrulanıyor...
   ✅ Kullanıcı veritabanında bulundu!
      • ID: 1
      • Kullanıcı Adı: test123
      • Rol: USER

╔════════════════════════════════════════╗
║     ✅ KAYIT BAŞARIYLA TAMAMLANDI!    ║
╚════════════════════════════════════════╝
```

## 🔴 HATA SENARYOLARI

### Senaryo 1: PostgreSQL Çalışmıyor

**Konsol'da göreceğiniz:**
```
📡 ADIM 1: Bağlantı oluşturuluyor...
🔓 ADIM 2: Bağlantı açılıyor...

╔════════════════════════════════════════╗
║    ❌ POSTGRESQL HATASI!              ║
╚════════════════════════════════════════╝

Hata Kodu: ...
Hata Mesajı: connection to server at "localhost" (::1), port 5432 failed
```

**ÇÖZÜM:**
```bash
# Windows:
"C:\Program Files\PostgreSQL\14\bin\pg_ctl.exe" start -D "C:\Program Files\PostgreSQL\14\data"

# Linux/Mac:
sudo service postgresql start
# veya
pg_ctl -D /usr/local/var/postgres start
```

### Senaryo 2: Veritabanı Yok

**Konsol'da göreceğiniz:**
```
╔════════════════════════════════════════╗
║    ❌ POSTGRESQL HATASI!              ║
╚════════════════════════════════════════╝

Hata Mesajı: database "Proje" does not exist
```

**ÇÖZÜM:**
```bash
# PostgreSQL'e bağlan
psql -U postgres

# Veritabanını oluştur
CREATE DATABASE "Proje";

# Çık
\q

# SQL dosyasını yükle
psql -U postgres -d Proje -f proje_vtys.sql
```

### Senaryo 3: Tablo Yok

**Konsol'da göreceğiniz:**
```
📋 ADIM 5: kullanicilar tablosu yapısı kontrol ediliyor...
   ❌ HATA: kullanicilar tablosu bulunamadı!
   → database_test.sql dosyasını çalıştırın!
```

**ÇÖZÜM:**
```bash
psql -U postgres -d Proje -f database_test.sql
```

### Senaryo 4: Kullanıcı Adı Zaten Var

**Konsol'da göreceğiniz:**
```
🔍 ADIM 4: 'test123' kullanıcı adı kontrol ediliyor...
   Bulunan kayıt sayısı: 1
   ❌ Bu kullanıcı adı zaten kullanılıyor!
```

**ÇÖZÜM:**
Farklı bir kullanıcı adı deneyin (örn: test456, deneme, vb.)

### Senaryo 5: Şifre Yanlış (Program.cs'de)

**Konsol'da göreceğiniz:**
```
╔════════════════════════════════════════╗
║    ❌ POSTGRESQL HATASI!              ║
╚════════════════════════════════════════╝

Hata Mesajı: password authentication failed for user "postgres"
```

**ÇÖZÜM:**
`Program.cs` dosyasını açın ve şifreyi güncelleyin:
```csharp
var connectionString = "Host=localhost;Port=5432;Database=Proje;Username=postgres;Password=BURAYA_GERCeK_SİFRENİZ";
```

## 📊 BAŞARILI KAYIT SONRASI

Kayıt başarılı olduğunda:

1. ✅ Konsol'da "KAYIT BAŞARIYLA TAMAMLANDI!" göreceksiniz
2. ✅ Ekranda "Kayıt başarılı!" mesajı çıkacak
3. ✅ Giriş ekranına döneceksiniz
4. ✅ Yeni kullanıcınızla giriş yapabileceksiniz

**GİRİŞ YAPARKEN:**
- Kullanıcı adı: test123 (kayıtta kullandığınız)
- Şifre: 1234 (kayıtta kullandığınız)

**NOT:** Şimdilik BCrypt devre dışı, düz şifre kullanıyoruz.
Kayıt çalıştıktan sonra BCrypt'i tekrar aktif edeceğiz.

## 🧪 MANUEL TEST

Konsol'dan doğrudan veritabanını test edebilirsiniz:

```bash
# PostgreSQL'e bağlan
psql -U postgres -d Proje

# Kullanıcıları listele
SELECT * FROM kullanicilar;

# Tabloyu temizle (gerekirse)
DELETE FROM kullanicilar WHERE kullanici_adi = 'test123';

# Çık
\q
```

## 🔧 SORUN ÇÖZÜMDE MÜ?

Şu bilgileri toplayın:

1. **Konsol çıktısının tamamı** (en önemlisi!)
2. **Hata mesajının ekran görüntüsü**
3. **PostgreSQL versiyonu:**
   ```bash
   psql --version
   ```
4. **Tablo var mı kontrolü:**
   ```bash
   psql -U postgres -d Proje -c "\dt"
   ```

## ✅ BAŞARI KRİTERLERİ

Kayıt başarılı sayılır eğer:

- [x] PostgreSQL çalışıyor
- [x] Veritabanı "Proje" mevcut
- [x] "kullanicilar" tablosu mevcut
- [x] Konsol'da 9 adımın hepsi ✅ ile geçildi
- [x] "KAYIT BAŞARIYLA TAMAMLANDI!" mesajı çıktı
- [x] Kullanıcı veritabanında göründü
- [x] Giriş yapılabildi

## 🎯 SONRAKİ ADIM

Kayıt başarılı olduktan sonra:

1. BCrypt'i tekrar aktif edeceğiz
2. Güvenli şifre saklama yapacağız
3. Eski kullanıcılar için migration yazacağız

**ŞU ANDA:** Kayıt çalışsın yeter! Güvenlik sonra.

---

**Soru/Sorun için:** Konsol çıktısının SCREENSHOT'unu gönderin!
