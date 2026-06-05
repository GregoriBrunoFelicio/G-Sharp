# Stop any running language server first — an attached editor (e.g. nvim) locks the
# installed tool files and makes `dotnet tool uninstall` fail with "access denied".
Get-Process gsharp-lsp -ErrorAction SilentlyContinue | Stop-Process -Force
dotnet build GSharp.LanguageServer -c Release --no-restore
dotnet pack GSharp.LanguageServer -c Release --no-build -o ./nupkg
dotnet tool uninstall -g GSharp.LanguageServer
dotnet tool install -g --add-source ./nupkg GSharp.LanguageServer
