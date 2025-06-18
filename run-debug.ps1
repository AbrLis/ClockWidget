# Устанавливаем переменные окружения для логирования
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:CONSOLE_LOG_LEVEL = "WARN"  # В консоль только предупреждения и ошибки
$env:FILE_LOG_LEVELS = "WARN,ERROR"  # В файл записываем DEBUG, WARN и ERROR сообщения

# Запускаем приложение
dotnet run --project ClockWidgetApp 