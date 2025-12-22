@echo off
chcp 65001 >nul
echo ================================================
echo   Anime Veritabanı v3.0 - Temiz Build
echo ================================================
echo.

echo [1/6] Eski derlemeler temizleniyor...
if exist bin rmdir /s /q bin 2>nul
if exist obj rmdir /s /q obj 2>nul
if exist .vs rmdir /s /q .vs 2>nul
del /s /q *.user 2>nul
del /s /q *.suo 2>nul
echo ✓ Temizlik tamamlandı!
echo.

echo [2/6] NuGet cache temizleniyor...
dotnet nuget locals all --clear >nul 2>&1
echo ✓ Cache temizlendi!
echo.

echo [3/6] NuGet paketleri yükleniyor...
dotnet restore --force
if errorlevel 1 (
    echo ✗ HATA: NuGet paketleri yüklenemedi!
    pause
    exit /b 1
)
echo ✓ NuGet paketleri yüklendi!
echo.

echo [4/6] Proje derleniyor...
dotnet build --no-incremental --no-restore
if errorlevel 1 (
    echo ✗ HATA: Proje derlenemedi!
    echo.
    echo Hata detayları yukarıda görünüyor.
    echo.
    pause
    exit /b 1
)
echo ✓ Derleme başarılı!
echo.

echo [5/6] Arka plan görseli kopyalanıyor...
if exist login_background.png (
    if exist bin\Debug\net10.0-windows (
        copy /Y login_background.png bin\Debug\net10.0-windows\ >nul 2>&1
        echo ✓ Görsel Debug klasörüne kopyalandı!
    )
    if exist bin\Release\net10.0-windows (
        copy /Y login_background.png bin\Release\net10.0-windows\ >nul 2>&1
        echo ✓ Görsel Release klasörüne kopyalandı!
    )
) else (
    echo ⚠ Uyarı: login_background.png bulunamadı!
    echo   Giriş ekranı arka plan resmi olmadan açılacak.
)
echo.

echo [6/6] Uygulama başlatılıyor...
echo.
echo ================================================
echo   UYGULAMA ÇALIŞIYOR
echo ================================================
echo.
dotnet run --no-build

pause
