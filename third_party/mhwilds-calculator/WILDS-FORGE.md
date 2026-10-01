# Wilds Forge integration

Upstream: https://github.com/chanleyou/mhwilds-calculator
Imported commit: bf5f6cf78386e374ee5d15c8d9193ff31532e8b1
(GitHub main snapshot, 2026-09-30).
Copyright (c) 2025 Chan Le You. See LICENSE.md (MIT).

This source is vendored to preserve the complete damage model and game data.
Local adaptations enable Next.js static export, remove the website navigation
and Google font download, and apply Wilds Forge's Segoe UI / navy / cyan theme.
The game formulas are unchanged.
Inputs receive accessible names and tooltips, and numeric values respect their
declared bounds. Unused upstream imports were removed for a clean build.

`test/offline.test.ts` covers the current damage API. The original upstream
tests reference older function signatures and React hooks outside components;
they remain untouched as imported source and are not the integration test gate.

Build with ../../tools/Build-Calculator.ps1 on native Windows. The exported
assets and MIT notice are packed into MHWsTool/Data/calculator.zip and embedded
in the executable. The calculator is served locally through WebView2 virtual
host mapping; it does not connect to Netlify or require a local server.
