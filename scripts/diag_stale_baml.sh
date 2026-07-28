#!/usr/bin/env bash
set -uo pipefail
cd /c/Users/Princ/BioCentri

echo '=== TIMESTAMPS ==='
echo -n "Source XAML modified:  "
stat -c '%y' app/BioCentri.App/src/components/auth/AuthenticationOverlay.xaml 2>&1
echo -n "Latest commit:         "
git log -1 --format='%h %ai %s' -- app/BioCentri.App/src/components/auth/AuthenticationOverlay.xaml
echo -n "EXE built:             "
stat -c '%y' app/BioCentri.App/bin/Debug/net8.0-windows10.0.19041.0/BioCentri.App.exe 2>&1
echo -n "BAML for AuthOverlay:  "
stat -c '%y' app/BioCentri.App/obj/Debug/net8.0-windows10.0.19041.0/src/components/auth/AuthenticationOverlay.baml 2>&1

echo
echo '=== STALE BAML CHECK (pre-clean): count TargetName AuthRoot in BAML ==='
if [ -f app/BioCentri.App/obj/Debug/net8.0-windows10.0.19041.0/src/components/auth/AuthenticationOverlay.baml ]; then
  grep -c 'AuthRoot' app/BioCentri.App/obj/Debug/net8.0-windows10.0.19041.0/src/components/auth/AuthenticationOverlay.baml 2>&1 || echo "0 matches"
else
  echo "(BAML not present - not yet built)"
fi
