#!/bin/bash
set -e
# Rebuilds and reinstalls both global tools (gs CLI and gsharp-lsp).
# The LSP goes first because update-lsp.sh kills any running server, which
# would otherwise lock the installed tool files.
cd "$(dirname "$0")"
./update-lsp.sh
./update-tool.sh
