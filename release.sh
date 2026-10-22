#!/bin/bash
DOTNET_VERSION='10.0'
RELEASE_SCRIPT_URL="https://raw.githubusercontent.com/hmlendea/dotnet-release-scripts/refs/heads/master/dotnet/${DOTNET_VERSION}.sh"
curl -sSL "${RELEASE_SCRIPT_URL}" | bash -s ${1} --no-trim