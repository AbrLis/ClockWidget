# Устанавливаем переменные окружения для логирования
$env:FILE_LOG_LEVELS = "TRACE,DEBUG,INFO,WARN,ERROR"  # Необходимые флаги логирования через запятую.

# Запускаем приложение
dotnet run --project ClockWidgetApp 