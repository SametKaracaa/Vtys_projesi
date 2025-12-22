using AnimeApp.Models;
using AnimeApp.Database;

namespace AnimeApp.ML
{
    public class AnimeRecommendationEngine
    {
        private readonly DatabaseManager db;
        private static Dictionary<int, List<(int animeId, int rating)>> userRatingsCache = new();
        private static DateTime lastCacheUpdate = DateTime.MinValue;
        private static readonly TimeSpan cacheLifetime = TimeSpan.FromMinutes(5);

        public AnimeRecommendationEngine(DatabaseManager database)
        {
            db = database;
        }

        public bool TrainModel()
        {
            // Cache'i temizle ve yeniden yükle
            RefreshCache();
            return true;
        }

        private void RefreshCache()
        {
            try
            {
                var allRatings = db.GetAllRatingsForML();
                userRatingsCache.Clear();

                // Kullanıcı bazında grupla (LINQ optimizasyonu)
                foreach (var rating in allRatings)
                {
                    if (!userRatingsCache.ContainsKey(rating.userId))
                    {
                        userRatingsCache[rating.userId] = new List<(int, int)>();
                    }
                    userRatingsCache[rating.userId].Add((rating.animeId, rating.rating));
                }

                lastCacheUpdate = DateTime.Now;
            }
            catch
            {
                userRatingsCache.Clear();
            }
        }

        private void EnsureCacheValid()
        {
            if (DateTime.Now - lastCacheUpdate > cacheLifetime || userRatingsCache.Count == 0)
            {
                RefreshCache();
            }
        }

        public List<AnimeOnerisi> GetRecommendations(int userId, int topN = 5, bool randomize = false)
        {
            try
            {
                EnsureCacheValid();

                // Kullanıcının puanladığı animeleri al
                var myRatings = userRatingsCache.ContainsKey(userId) 
                    ? userRatingsCache[userId] 
                    : new List<(int animeId, int rating)>();
                
                var myRatedAnimeIds = new HashSet<int>(myRatings.Select(r => r.animeId));
                
                // Kullanıcının favorilerini al
                var myFavorites = db.GetFavoriteAnimes(userId).Select(a => a.AnimeId).ToHashSet();
                
                // PUANLANAN VE FAVORİ ANİMELERİ HARİÇ TUT
                var excludedAnimeIds = new HashSet<int>(myRatedAnimeIds);
                foreach (var favId in myFavorites)
                {
                    excludedAnimeIds.Add(favId);
                }

                // Yeterli veri yoksa popüler animeler öner
                if (userRatingsCache.Count < 2 || myRatings.Count == 0)
                {
                    return GetPopularRecommendationsExcluding(topN, excludedAnimeIds, randomize);
                }

                // FAZLADAN öneri al (randomize için daha büyük havuz)
                int fetchCount = randomize ? topN * 3 : topN;
                
                // SADECE Collaborative Filtering (HIZLI)
                var recommendations = GetCollaborativeRecommendationsFast(userId, myRatings, excludedAnimeIds, fetchCount);
                
                if (recommendations.Count == 0)
                {
                    return GetPopularRecommendationsExcluding(topN, excludedAnimeIds, randomize);
                }
                
                // Randomize modunda rastgele seç
                if (randomize && recommendations.Count > topN)
                {
                    var random = new Random();
                    recommendations = recommendations.OrderBy(x => random.Next()).Take(topN).ToList();
                }
                
                return recommendations.Take(topN).ToList();
            }
            catch (Exception ex)
            {
                return GetPopularRecommendations(topN);
            }
        }

        // HIZLI Collaborative Filtering - favori sorgularını kaldırdık
        private List<AnimeOnerisi> GetCollaborativeRecommendationsFast(
            int userId, 
            List<(int animeId, int rating)> myRatings,
            HashSet<int> excludedAnimeIds,
            int topN)
        {
            // En benzer 5 kullanıcıyı bul (daha az kullanıcı = daha hızlı)
            var similarUsers = new List<(int userId, double similarity)>();
            
            foreach (var otherUserId in userRatingsCache.Keys)
            {
                if (otherUserId == userId) continue;

                var similarity = CalculateSimilarityOptimized(myRatings, userRatingsCache[otherUserId]);
                if (similarity > 0.3) // Eşik yükselttik, daha az eşleşme
                {
                    similarUsers.Add((otherUserId, similarity));
                }
            }

            similarUsers = similarUsers
                .OrderByDescending(u => u.similarity)
                .Take(5) // Sadece 5 benzer kullanıcı (10'dan düşürdük)
                .ToList();

            if (similarUsers.Count == 0)
            {
                return new List<AnimeOnerisi>();
            }

            var animeScores = new Dictionary<int, (float score, int count)>();

            foreach (var (otherUserId, similarity) in similarUsers)
            {
                var theirRatings = userRatingsCache[otherUserId];
                
                foreach (var (animeId, rating) in theirRatings)
                {
                    // Zaten izlediğimiz animeleri ATLA
                    if (excludedAnimeIds.Contains(animeId)) continue;
                    
                    if (rating >= 7) // Eşik yükselttik (6'dan 7'ye)
                    {
                        var weightedScore = rating * (float)similarity;
                        
                        if (!animeScores.ContainsKey(animeId))
                        {
                            animeScores[animeId] = (weightedScore, 1);
                        }
                        else
                        {
                            var current = animeScores[animeId];
                            animeScores[animeId] = (current.score + weightedScore, current.count + 1);
                        }
                    }
                }
            }

            // Önerileri oluştur
            var allAnimes = db.GetAnimeList();
            var recommendations = new List<AnimeOnerisi>();

            foreach (var kvp in animeScores.OrderByDescending(x => x.Value.score).Take(topN))
            {
                var anime = allAnimes.FirstOrDefault(a => a.AnimeId == kvp.Key);
                if (anime != null)
                {
                    var scoreData = kvp.Value;
                    var avgScore = scoreData.score / scoreData.count;
                    
                    var reason = scoreData.count > 1
                        ? $"👥 {scoreData.count} benzer kullanıcı tavsiye ediyor"
                        : "👥 Benzer kullanıcı tavsiye ediyor";
                        
                    recommendations.Add(new AnimeOnerisi
                    {
                        Anime = anime,
                        TahminPuan = avgScore,
                        OneriNedeni = reason
                    });
                }
            }

            return recommendations;
        }

        // Benzer kullanıcılardan collaborative filtering ile öneriler


        // Popüler önerileri hariç tutulanlar olmadan getir
        private List<AnimeOnerisi> GetPopularRecommendationsExcluding(int topN, HashSet<int> excludedIds, bool randomize = false)
        {
            int fetchCount = randomize ? topN * 3 : topN;
            
            var popularAnimes = db.GetPopularAnimes(fetchCount)
                .Where(a => !excludedIds.Contains(a.AnimeId))
                .ToList();
            
            // Randomize modunda karıştır
            if (randomize && popularAnimes.Count > topN)
            {
                var random = new Random();
                popularAnimes = popularAnimes.OrderBy(x => random.Next()).Take(topN).ToList();
            }
            else
            {
                popularAnimes = popularAnimes.Take(topN).ToList();
            }
            
            return popularAnimes.Select(a => new AnimeOnerisi
            {
                Anime = a,
                TahminPuan = (float)(a.Puan ?? 7.0),
                OneriNedeni = "🔥 Popüler anime"
            }).ToList();
        }

        // Optimized similarity calculation (sadece ortak animeler için)
        private double CalculateSimilarityOptimized(
            List<(int animeId, int rating)> user1Ratings,
            List<(int animeId, int rating)> user2Ratings)
        {
            // HashSet kullanarak hızlı arama (O(1))
            var user1Dict = user1Ratings.ToDictionary(r => r.animeId, r => r.rating);
            
            double dotProduct = 0;
            double norm1 = 0;
            double norm2 = 0;
            int commonCount = 0;

            foreach (var (animeId, rating2) in user2Ratings)
            {
                if (user1Dict.TryGetValue(animeId, out int rating1))
                {
                    dotProduct += rating1 * rating2;
                    norm1 += rating1 * rating1;
                    norm2 += rating2 * rating2;
                    commonCount++;
                }
            }

            if (commonCount == 0 || norm1 == 0 || norm2 == 0) return 0;

            return dotProduct / (Math.Sqrt(norm1) * Math.Sqrt(norm2));
        }

        private List<AnimeOnerisi> GetPopularRecommendations(int topN)
        {
            var popularAnimes = db.GetPopularAnimes(topN);
            return popularAnimes.Select(a => new AnimeOnerisi
            {
                Anime = a,
                TahminPuan = (float)(a.Puan ?? 7.0),
                OneriNedeni = "Populer anime"
            }).ToList();
        }

        public List<AnimeOnerisi> GetSimilarAnimes(int animeId, int topN = 5)
        {
            var targetAnime = db.GetAnimeList().FirstOrDefault(a => a.AnimeId == animeId);
            if (targetAnime == null) return new List<AnimeOnerisi>();

            var targetGenres = db.GetAnimeTurleri(animeId);
            var targetGenreIds = new HashSet<int>(targetGenres.Select(g => g.TurId));
            
            var allAnimes = db.GetAnimeList();
            var similarAnimes = new List<AnimeOnerisi>();

            foreach (var anime in allAnimes)
            {
                if (anime.AnimeId == animeId) continue;

                var genres = db.GetAnimeTurleri(anime.AnimeId);
                var commonGenres = genres.Count(g => targetGenreIds.Contains(g.TurId));

                if (commonGenres > 0)
                {
                    similarAnimes.Add(new AnimeOnerisi
                    {
                        Anime = anime,
                        TahminPuan = (float)(anime.Puan ?? 5.0) + commonGenres * 2,
                        OneriNedeni = $"{commonGenres} ortak tur"
                    });
                }
            }

            return similarAnimes
                .OrderByDescending(s => s.TahminPuan)
                .Take(topN)
                .ToList();
        }

        // Cache'i manuel temizleme
        public static void ClearCache()
        {
            userRatingsCache.Clear();
            lastCacheUpdate = DateTime.MinValue;
        }
    }
}
