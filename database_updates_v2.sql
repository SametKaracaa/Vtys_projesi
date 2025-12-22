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

-- 2. Puanlar tablosuna yorum kolonu ekle (opsiyonel yorum)
ALTER TABLE puanlar ADD COLUMN IF NOT EXISTS yorum TEXT;

-- 3. İzleme Listesi Tablosu (Gelecek özellik)
CREATE TABLE IF NOT EXISTS izleme_listesi (
    liste_id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL,
    anime_id INTEGER NOT NULL,
    durum VARCHAR(20) DEFAULT 'İzleniyor',
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
CREATE INDEX IF NOT EXISTS idx_puanlar_anime ON puanlar(anime_id);

-- Başarılı mesajı
SELECT 'Veritabanı güncellemeleri başarıyla tamamlandı!' as mesaj;
