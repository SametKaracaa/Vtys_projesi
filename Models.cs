namespace AnimeApp.Models
{
    public class Anime
    {
        public int AnimeId { get; set; }
        public string Isim { get; set; } = string.Empty;
        public string? IngilizceIsim { get; set; }
        public double? Puan { get; set; }
        public string? BolumSayisi { get; set; }
        public string? Tip { get; set; }
        public string? YayinTarihi { get; set; }
        public string? ResimUrl { get; set; }
        public List<Tur> Turler { get; set; } = new();
    }

    public class Kullanici
    {
        public int UserId { get; set; }
        public string KullaniciAdi { get; set; } = string.Empty;
        public string? Cinsiyet { get; set; }
        public DateTime? DogumTarihi { get; set; }
        public string Sifre { get; set; } = string.Empty;
        public string Rol { get; set; } = "USER";
    }

    public class Tur
    {
        public int TurId { get; set; }
        public string TurAdi { get; set; } = string.Empty;
    }

    public class Puan
    {
        public int PuanId { get; set; }
        public int UserId { get; set; }
        public int AnimeId { get; set; }
        public int VerilenPuan { get; set; }
        public DateTime PuanlamaZamani { get; set; }
    }

    public class Favori
    {
        public int FavoriId { get; set; }
        public int UserId { get; set; }
        public int AnimeId { get; set; }
        public DateTime EklenmeZamani { get; set; }
    }

    public class KullaniciAyarlari
    {
        public int AyarId { get; set; }
        public int UserId { get; set; }
        public string Tema { get; set; } = "Light";
        public string Dil { get; set; } = "TR";
    }

    public class IzlemeListesi
    {
        public int ListeId { get; set; }
        public int UserId { get; set; }
        public int AnimeId { get; set; }
        public string Durum { get; set; } = "İzleniyor";
        public DateTime? BaslangicTarihi { get; set; }
        public DateTime? BitisTarihi { get; set; }
    }

    public class KullaniciIstatistik
    {
        public int UserId { get; set; }
        public string KullaniciAdi { get; set; } = string.Empty;
        public int PuanlananAnimeSayisi { get; set; }
        public double OrtalamaPuan { get; set; }
        public int FavoriSayisi { get; set; }
        public int IzlemeListesiSayisi { get; set; }
    }

    public class AnimeOnerisi
    {
        public Anime Anime { get; set; } = new();
        public float TahminPuan { get; set; }
        public string OneriNedeni { get; set; } = string.Empty;
    }
}
