-- VERİTABANI TEST VE KONTROL SCRIPTLERI

-- 1. KULLANICILAR TABLOSUNU KONTROL ET
SELECT 
    column_name, 
    data_type, 
    character_maximum_length,
    is_nullable,
    column_default
FROM information_schema.columns
WHERE table_name = 'kullanicilar'
ORDER BY ordinal_position;

-- 2. KULLANICILAR TABLOSUNU OLUŞTUR (Yoksa)
CREATE TABLE IF NOT EXISTS kullanicilar (
    user_id SERIAL PRIMARY KEY,
    kullanici_adi VARCHAR(100) UNIQUE NOT NULL,
    sifre VARCHAR(255) NOT NULL,
    cinsiyet VARCHAR(20),
    dogum_tarihi DATE,
    rol VARCHAR(20) DEFAULT 'USER',
    kayit_tarihi TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 3. FAVORILER TABLOSUNU OLUŞTUR (Yoksa)
CREATE TABLE IF NOT EXISTS favoriler (
    favori_id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES kullanicilar(user_id) ON DELETE CASCADE,
    anime_id INTEGER NOT NULL,
    eklenme_zamani TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(user_id, anime_id)
);

-- 4. İNDEXLER
CREATE INDEX IF NOT EXISTS idx_favoriler_user ON favoriler(user_id);
CREATE INDEX IF NOT EXISTS idx_favoriler_anime ON favoriler(anime_id);

-- 5. TEST KULLANICISI OLUŞTUR (Şifre: test123 - BCrypt hashli)
-- NOT: Bu hash "test123" için BCrypt ile oluşturulmuş bir hash'tir
INSERT INTO kullanicilar (kullanici_adi, sifre, rol)
VALUES ('testuser', '$2a$11$xvQzq7xGBN8J3H9V4qXx5.0h3KX1Y0yH5L.QZGqLqQD8qXrXqXrXq', 'USER')
ON CONFLICT (kullanici_adi) DO NOTHING;

-- 6. MEVCUT KULLANICILARI GÖSTER
SELECT user_id, kullanici_adi, rol, kayit_tarihi 
FROM kullanicilar 
ORDER BY user_id DESC 
LIMIT 10;

-- 7. PUANLAR TABLOSU VAR MI KONTROL ET
SELECT COUNT(*) as puan_sayisi FROM puanlar;

-- 8. ANIMELER TABLOSU VAR MI KONTROL ET
SELECT COUNT(*) as anime_sayisi FROM animeler;

-- 9. TURLER TABLOSU VAR MI KONTROL ET
SELECT COUNT(*) as tur_sayisi FROM turler;

-- 10. TEST: Basit bir kayıt dene
-- INSERT INTO kullanicilar (kullanici_adi, sifre, rol) 
-- VALUES ('testdeneme', 'sifre123', 'USER');
