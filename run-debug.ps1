# Устанавливаем переменные окружения для логирования
$env:FILE_LOG_LEVELS = "DEBUG,INFO,WARN,ERROR"  # В файл записываем все уровни для отладки

# Запускаем приложение
dotnet run --project ClockWidgetApp 