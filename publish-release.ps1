# Очистка предыдущих сборок
Write-Host "Очистка предыдущих сборок..."
dotnet clean
Remove-Item -Path "bin" -Recurse -Force -ErrorAction SilentlyContinue

# Сборка и публикация
Write-Host "Сборка и публикация..."
dotnet publish ClockWidgetApp -c Release -o ./bin/Release

# Проверяем результат
$publishPath = "bin/Release"
if (Test-Path $publishPath) {
    Write-Host "Удаление лишних файлов..."
    # Удаляем все файлы кроме основного .exe
    Get-ChildItem -Path $publishPath -File | 
        Where-Object { 
            $_.Name -ne "ClockWidgetApp.exe" 
        } | 
        Remove-Item -Force
    
    # Удаляем все папки
    Get-ChildItem -Path $publishPath -Directory | 
        Remove-Item -Recurse -Force

    # Удаляем папку obj/Release
    Remove-Item -Path "obj/Release" -Recurse -Force -ErrorAction SilentlyContinue

    Write-Host "Публикация успешно завершена!"
    Write-Host "Итоговые файлы находятся в: $publishPath"
    
    # Показываем содержимое папки публикации
    Write-Host "`nСодержимое папки публикации:"
    Get-ChildItem $publishPath | ForEach-Object {
        Write-Host "- $($_.Name) ($($_.Length) байт)"
    }
} else {
    Write-Host "Ошибка: папка публикации не найдена!"
    Write-Host "Ожидаемый путь: $publishPath"
    exit 1
} 