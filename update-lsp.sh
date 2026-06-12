#!/bin/bash
set -e
# Stop any running language server first — an attached editor (e.g. nvim) locks the
# installed tool files and makes `dotnet tool uninstall` fail with "access denied".
pkill -f gsharp-lsp 2>/dev/null || true
dotnet restore GSharp.LanguageServer
dotnet build GSharp.LanguageServer -c Release --no-restore
dotnet pack GSharp.LanguageServer -c Release --no-build -o ./nupkg
dotnet tool uninstall -g GSharp.LanguageServer 2>/dev/null || true
dotnet tool install -g --add-source ./nupkg GSharp.LanguageServer
