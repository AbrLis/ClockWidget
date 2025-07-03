# Очистка предыдущих сборок
Write-Host "Очистка предыдущих сборок..."
dotnet clean
Remove-Item -Path "bin","obj" -Recurse -Force -ErrorAction SilentlyContinue

# Сборка и публикация (self-contained)
Write-Host "Сборка автономного приложения..."
dotnet publish ClockWidgetApp `
    -c Release `
    -o ./artifacts/ClockWidgetApp/bin/Release/bin/Release `
    -p:PublishSingleFile=true `
    -p:SelfContained=true `
    -p:DebugType=None `
    -p:DebugSymbols=false `
    -p:EnableCompressionInSingleFile=true `
    -r win-x64 `
    --nologo

# Проверяем результат
$publishPath = "artifacts/ClockWidgetApp/bin/Release/bin/Release"
if (Test-Path "$publishPath/ClockWidgetApp.exe") {
    $exeSize = (Get-Item "$publishPath/ClockWidgetApp.exe").Length / 1MB
    Write-Host "Публикация успешно завершена!"
    Write-Host "Итоговый размер: $([math]::Round($exeSize, 2)) MB"
    
    # Оставляем только EXE-файл
    Get-ChildItem -Path $publishPath -File | 
        Where-Object { $_.Name -ne "ClockWidgetApp.exe" } | 
        Remove-Item -Force
    
    # Удаляем пустые папки
    Get-ChildItem -Path $publishPath -Directory | 
        Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
} else {
    Write-Host "Ошибка: EXE-файл не найден!"
    exit 1
}