# Очистка папок сборки
Remove-Item -Path ".\obj" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path ".\bin" -Recurse -Force -ErrorAction SilentlyContinue

# Восстановление пакетов и сборка проекта
dotnet clean
dotnet restore
dotnet build 