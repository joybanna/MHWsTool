# Equipment skills reduce build requirements

final result: passed

## Behavior

The shared Skill / Set / Group catalog has three destinations: Required, On Weapon and On Talisman. Each destination keeps its own names and values. Weapon and talisman contributions are credited before armor candidates are ranked and before decorations are placed.

Ordinary skills contribute their entered levels. A Set or Group on one equipment item contributes one piece toward the bonus threshold. For example, a four-piece set tier needs two armor pieces when the same bonus is supplied by both the weapon and talisman.

Manual source lists replace that item's catalog skills while retaining its slots, preventing duplicate skill credit. Disabling manual mode restores the catalog source and retains the entered list. Manual talisman mode fixes the talisman rather than trying other saved talismans. An explicitly empty manual list supplies no skills.

## UI evidence

- `planner-equipment-skills.png`: shared catalog and selected requirements with equipment credit.
- `planner-equipment-remaining.png`: remaining ordinary levels and Set / Group piece requirements.
- `planner-equipment-results.png`: matching results with the supplied equipment contributions included.
- `planner-equipment-minimum.png`: minimum-size layout with fixed Search and scrollable requirements.

These captures were produced from the final self-contained executable. Inputs retain the planner's Damage Calculator typography, colors, spacing and rounded corners. Entering a filter brings matching catalog names into view; adding a name brings its level editor into view. No actionable P0/P1/P2 issue was found in the changed flow.

## Verification

The Release build and self-contained Windows publish succeeded. The catalog/search-engine checks passed, including targeted cases for:

- Manual weapon and talisman skills replacing catalog data without double counting.
- Adding ordinary levels from both sources and allowing armor with no target skills when gear supplies the requirement.
- Explicit empty overrides and the no-talisman option.
- Reduced Set / Group piece requirements, correct unlocked tiers and rejection of an insufficient three-piece set for a four-piece tier.
- Filling only missing levels with decorations and retaining slots when gear already supplies the requirement.

Native Windows UI Automation passed against development and the final executable:

- Independent weapon, talisman and required-level lists.
- Combining supplied levels, updating the displayed Need value, disabling and restoring manual source credit.
- Adding a Set and Group to both equipment sources and searching with the reduced armor requirements.
- Attack Boost 5 on the weapon and Poison Attack 3 on the talisman reaching Search without selected item slots.
- Removing a supplied bonus restoring the remaining piece requirement and clearing stale results.
- Clear resetting requirements, both source lists and both manual modes.
- Filtered catalog names being visible for selection and Search staying visible at minimum size.

The existing result-card presentation and Info tab in the workspace remain part of the tested application. Calculator navigation was outside this request and is not claimed as verified here.
