#!/usr/bin/env bash
set -euo pipefail

project_root="$(cd "$(dirname "$0")/.." && pwd)"
cd "$project_root"
export DOTNET_CLI_HOME="$project_root/.cli"
export NUGET_PACKAGES="$project_root/.nuget/packages"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=1

if [[ -x "$project_root/.dotnet/dotnet" ]]; then
  export DOTNET_ROOT="$project_root/.dotnet"
  exec "$DOTNET_ROOT/dotnet" "$@"
fi

exec dotnet "$@"
