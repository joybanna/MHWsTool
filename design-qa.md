# Game8-style search result design QA

- Source visual truth: `C:/Users/joyba/AppData/Local/Temp/codex-clipboard-b676fd6e-bf0c-459e-b35e-387a3cf54b56.png`
- Source page: `https://game8.co/games/Monster-Hunter-Wilds/archives/500590`
- Final implementation screenshot: `G:/WorkMe/MHWsTool/ui-review/planner-reference-results.png`
- Focused implementation crop: `G:/WorkMe/MHWsTool/ui-review/result-implementation-crop.png`
- Combined comparison: `G:/WorkMe/MHWsTool/ui-review/result-reference-comparison.png`
- Viewport: 1370 × 850 px native WPF window
- Source pixels: 836 × 338; normalized source: 747 × 302; implementation crop: 747 × 302; density factor 1
- State: 30 populated high-rank results; first result contains requested and additional skills, free slots, an equipped decoration, defense, and all five elemental resistances

## Findings

No actionable P0, P1, or P2 differences remain.

- Fonts and typography: the result uses the application's Segoe UI family with the same bold label, regular value, and semibold badge hierarchy as the reference. The source uses a slightly more condensed web font; retaining the desktop app font is acceptable P3 polish.
- Spacing and layout rhythm: the result pane now receives 58% of planner width. The card uses the reference's equipment/details split, compact 24 px equipment rows, right-aligned wrapped badges, dashed separators, and four compact footer rows.
- Colors and visual tokens: dark navy surfaces, blue equipment links, gray additional-skill badges, blue requested-skill borders, near-white values, and colored element icons match the reference hierarchy. The cyan result title follows the existing app accent and is acceptable P3 variation.
- Image quality and asset fidelity: armor, talisman, slot category, slot level, and five element icons use the assets exposed by the user-supplied Game8 reference. They are embedded as offline PNG resources. Slot icons receive the same light treatment used by the reference's dark theme, with clean transparency at 18 px.
- Copy and content: `Search Results 30Sets`, `Skills`, `Free Slots`, `Decoration`, `Total Defense`, and `Elements` follow the source. Decorations use a name-and-level badge with an optional `×N` count. Element order is Fire, Water, Thunder, Ice, Dragon.

## Comparison history

### Pass 1

- Finding: slot-level assets retained their light-theme dark fill and had insufficient contrast on the result card.
- Fix: regenerated slot and decoration assets with a near-white foreground while preserving their original alpha masks.
- Post-fix evidence: `ui-review/result-reference-comparison.png` shows the slot silhouettes and contrast matching the source at the same normalized size.

### Pass 2

- Evidence: source and final implementation are placed side by side in `ui-review/result-reference-comparison.png` at 747 × 302 each.
- Result: no remaining P0/P1/P2 differences. Content-dependent armor names, skill names, values, and decoration counts differ as expected because the implementation shows a real local search result rather than the source's example result.

## Interaction checks

- Verified all 179 Skill, Set, and Group names remain selectable.
- Verified requested skills receive highlighted badges and additional skills retain neutral badges.
- Verified grouped free-slot icons, equipped decoration badge, defense, and all five element icons through UI Automation.
- Verified layout and controls at the 1050 × 690 minimum window size.
- Verified the final self-contained `release/win-x64/MHWsTool.exe` with the complete planner smoke suite.

## Implementation checklist

- [x] Match the reference result header and count format.
- [x] Use the reference armor and talisman icons.
- [x] Highlight requested skill badges.
- [x] Use dashed row separators.
- [x] Group free slots by armor/weapon category and descending slot level.
- [x] Render equipped decorations as name-and-level badges.
- [x] Use the reference element icons and order.
- [x] Embed every result icon for offline use.

final result: passed
