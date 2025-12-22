using OfficeOpenXml;
using AnimeApp.Models;
using AnimeApp.Database;

namespace AnimeApp.Utilities
{
    public class ExcelManager
    {
        public static bool ExportToExcel(List<Dictionary<string, object>> data, string filePath)
        {
            try
            {
                // EPPlus lisans ayarı
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Animeler");

                // Başlıkları yaz
                var headers = data.FirstOrDefault()?.Keys.ToList();
                if (headers == null) return false;

                for (int i = 0; i < headers.Count; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                    worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                    worksheet.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    worksheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                }

                // Verileri yaz
                for (int row = 0; row < data.Count; row++)
                {
                    var item = data[row];
                    for (int col = 0; col < headers.Count; col++)
                    {
                        worksheet.Cells[row + 2, col + 1].Value = item[headers[col]];
                    }
                }

                // Otomatik genişlik
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Dosyayı kaydet
                var file = new FileInfo(filePath);
                package.SaveAs(file);

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Excel oluşturulurken hata: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static bool ExportUserRatings(DatabaseManager db, int userId, string filePath)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Puanlarım");

                // Başlıklar
                worksheet.Cells[1, 1].Value = "Anime ID";
                worksheet.Cells[1, 2].Value = "Anime İsmi";
                worksheet.Cells[1, 3].Value = "Verdiğim Puan";
                worksheet.Cells[1, 4].Value = "Puanlama Tarihi";

                // Stil
                using (var range = worksheet.Cells[1, 1, 1, 4])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
                }

                // Verileri al
                var animes = db.GetUserRatedAnimes(userId);
                int row = 2;
                foreach (var anime in animes)
                {
                    var puan = db.GetUserPuan(userId, anime.AnimeId);
                    worksheet.Cells[row, 1].Value = anime.AnimeId;
                    worksheet.Cells[row, 2].Value = anime.Isim;
                    worksheet.Cells[row, 3].Value = puan ?? 0;
                    worksheet.Cells[row, 4].Value = DateTime.Now.ToShortDateString();
                    row++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                package.SaveAs(new FileInfo(filePath));

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Excel oluşturulurken hata: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public static bool ExportFavorites(DatabaseManager db, int userId, string filePath)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Favorilerim");

                // Başlıklar
                worksheet.Cells[1, 1].Value = "Anime ID";
                worksheet.Cells[1, 2].Value = "Anime İsmi";
                worksheet.Cells[1, 3].Value = "İngilizce İsim";
                worksheet.Cells[1, 4].Value = "Puan";
                worksheet.Cells[1, 5].Value = "Tür";

                // Stil
                using (var range = worksheet.Cells[1, 1, 1, 5])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Pink);
                }

                // Verileri al
                var favoriler = db.GetFavoriteAnimes(userId);
                int row = 2;
                foreach (var anime in favoriler)
                {
                    worksheet.Cells[row, 1].Value = anime.AnimeId;
                    worksheet.Cells[row, 2].Value = anime.Isim;
                    worksheet.Cells[row, 3].Value = anime.IngilizceIsim ?? "";
                    worksheet.Cells[row, 4].Value = anime.Puan?.ToString("F2") ?? "N/A";
                    worksheet.Cells[row, 5].Value = anime.Tip ?? "";
                    row++;
                }

                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                package.SaveAs(new FileInfo(filePath));

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Excel oluşturulurken hata: {ex.Message}", "Hata",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
