# Planner options removal

Status: passed, 2026-09-30.

Removed the complete red-circled “Weapon, talisman and search options” section: the weapon picker, talisman picker, custom talisman button, and exclusion buttons. Removed their event handlers and references from the window. Updated the manual contribution help and README to reflect the current controls.

The Required / On Weapon / On Talisman catalog controls and “Skills on weapon / talisman” editor remain available. Existing saved custom talismans still load for automatic talisman search.

## Verification

- Release solution build: zero warnings and errors.
- Published self-contained executable: `release/win-x64-ui-update/MHWsTool.exe`, also copied to the standard `release/win-x64/MHWsTool.exe` path after the previous app session closed. SHA-256 hashes match.
- Existing native planner UI Automation checks passed against that executable: catalog name selection, level editing, combined Skill / Set / Group search, equipment requirement credit, result rendering, Clear, and minimum window size.
- Inspected `ui-review/planner-without-equipment-options.png`: the circled section is absent, and the catalog, selected requirement area and Search remain visible.

The previous executable was open during publishing, so the new executable was initially published to a separate folder. The user session closed before delivery, allowing the standard executable to be updated without interrupting it.
