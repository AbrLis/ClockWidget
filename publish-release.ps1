# Очистка предыдущих сборок
Write-Host "Очистка предыдущих сборок..."
dotnet clean ClockWidgetApp

# Сборка и публикация
Write-Host "Сборка и публикация..."
dotnet publish ClockWidgetApp -c Release -r win-x64 `
    --self-contained true `
    /p:PublishSingleFile=true `
    /p:IncludeAllContentForSelfExtract=true `
    /p:DebugType=None

# Удаление лишних файлов после сборки
$publishPath = "ClockWidgetApp/bin/Release/net9.0-windows/win-x64/publish"
$targetDir = "ClockWidgetApp/bin/Release/net9.0-windows/win-x64"

if (Test-Path $targetDir) {
    Write-Host "Удаление лишних файлов и папок, кроме publish..."
    Get-ChildItem -Path $targetDir -Exclude "publish" | Remove-Item -Recurse -Force
} else {
    Write-Host "Папка сборки не найдена: $targetDir"
}

Write-Host "Готово! Итоговые файлы находятся в: $publishPath" 