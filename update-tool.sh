#!/bin/bash
set -e
dotnet build GSharp.CLI -c Release --no-restore
dotnet pack GSharp.CLI -c Release --no-build -o ./nupkg
dotnet tool uninstall -g GSharp.CLI 2>/dev/null || true
dotnet tool install -g --add-source ./nupkg GSharp.CLI
