#!/usr/bin/env bash
set -euo pipefail

pnpm install --frozen-lockfile
pnpm build
pnpm dist:win
pnpm dist:mac
