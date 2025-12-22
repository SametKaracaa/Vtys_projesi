-- Yeni Özellikler için Veritabanı Güncellemeleri

-- 1. Favoriler Tablosu
CREATE TABLE IF NOT EXISTS favoriler (
    favori_id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL,
    anime_id INTEGER NOT NULL,
    eklenme_zamani TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id) REFERENCES kullanicilar(user_id) ON DELETE CASCADE,
    FOREIGN KEY (anime_id) REFERENCES animeler(anime_id) ON DELETE CASCADE,
    UNIQUE (user_id, anime_id)
);

-- 2. Kullanıcı Ayarları Tablosu (Dark Mode vb.)
CREATE TABLE IF NOT EXISTS kullanici_ayarlari (
    ayar_id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL UNIQUE,
    tema VARCHAR(20) DEFAULT 'Light',
    dil VARCHAR(10) DEFAULT 'TR',
    FOREIGN KEY (user_id) REFERENCES kullanicilar(user_id) ON DELETE CASCADE
);

-- 3. İzleme Listesi Tablosu
CREATE TABLE IF NOT EXISTS izleme_listesi (
    liste_id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL,
    anime_id INTEGER NOT NULL,
    durum VARCHAR(20) DEFAULT 'İzleniyor', -- İzleniyor, Tamamlandı, Planlanıyor, Bırakıldı
    baslangic_tarihi DATE,
    bitis_tarihi DATE,
    FOREIGN KEY (user_id) REFERENCES kullanicilar(user_id) ON DELETE CASCADE,
    FOREIGN KEY (anime_id) REFERENCES animeler(anime_id) ON DELETE CASCADE,
    UNIQUE (user_id, anime_id)
);

-- 4. Kullanıcı İstatistikleri View
CREATE OR REPLACE VIEW kullanici_istatistikleri AS
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

-- 5. Index'ler (Performans için)
CREATE INDEX IF NOT EXISTS idx_favoriler_user ON favoriler(user_id);
CREATE INDEX IF NOT EXISTS idx_favoriler_anime ON favoriler(anime_id);
CREATE INDEX IF NOT EXISTS idx_izleme_user ON izleme_listesi(user_id);
CREATE INDEX IF NOT EXISTS idx_puanlar_user ON puanlar(user_id);

-- Mevcut kullanıcılar için varsayılan ayarlar ekle
INSERT INTO kullanici_ayarlari (user_id, tema, dil)
SELECT user_id, 'Light', 'TR'
FROM kullanicilar
WHERE user_id NOT IN (SELECT user_id FROM kullanici_ayarlari);

-- Başarılı mesajı
SELECT 'Veritabanı güncellemeleri başarıyla tamamlandı!' as mesaj;
