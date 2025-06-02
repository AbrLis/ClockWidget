# Сборка проекта в единый файл.

Файл будет находится в папку ./bin/Release/publish

```bash
dotnet publish -c Release -r win-x64 `
    --self-contained true `
    /p:PublishSingleFile=true `
    /p:IncludeAllContentForSelfExtract=true `
    /p:DebugType=None
```
