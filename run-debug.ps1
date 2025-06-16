# Запуск приложения с включенным логированием
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:LOG_LEVEL = "Debug"
dotnet run --project ClockWidgetApp 