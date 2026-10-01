param([string]$AppPath)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Drawing
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;
public static class PlannerWindowPrinter {
  [StructLayout(LayoutKind.Sequential)] public struct Rect { public int Left, Top, Right, Bottom; }
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr handle, out Rect rect);
  [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr handle, IntPtr hdc, uint flags);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr handle);
}
'@
function Controls($root) { $root.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition) }
function FindControl($root, [string]$type, [string]$name) {
  Controls $root | Where-Object { $_.Current.ControlType.ProgrammaticName -eq $type -and $_.Current.Name -eq $name } | Select-Object -First 1
}
function Click([string]$name) {
  $button = FindControl $main 'ControlType.Button' $name
  if ($null -eq $button) { throw "Missing button: $name" }
  $button.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
  Start-Sleep -Milliseconds 200
}
function SetSkillFilter([string]$query) {
  $filterField = FindControl $main 'ControlType.Edit' 'Search skills'
  $filterField.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).SetValue($query)
  Start-Sleep -Milliseconds 200
}
function Choose([string]$skill, [string]$option, [string]$source = 'Required') {
  $fieldName = if ($source -eq 'Required') { "Required level for $skill" } else { "$source contribution for $skill" }
  $combo = FindControl $main 'ControlType.ComboBox' $fieldName
  if ($null -eq $combo) { throw "Missing skill field: $skill" }
  [PlannerWindowPrinter]::SetForegroundWindow($handle) | Out-Null
  $combo.SetFocus()
  $expand = $combo.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)
  $expand.Expand()
  $item = $null
  for ($waitAttempt = 0; $waitAttempt -lt 10 -and $null -eq $item; $waitAttempt++) {
    Start-Sleep -Milliseconds 200
    $item = FindControl $combo 'ControlType.ListItem' $option
    if ($null -eq $item -and $expand.Current.ExpandCollapseState -eq [System.Windows.Automation.ExpandCollapseState]::Collapsed) { $expand.Expand() }
  }
  if ($null -eq $item) { Capture 'planner-dropdown-failure.png'; Controls $combo | ForEach-Object { Write-Output ($_.Current.ControlType.ProgrammaticName + ' ' + $_.Current.Name) }; throw "Missing option: $option" }
  $item.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
  $combo.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern).Collapse()
  Start-Sleep -Milliseconds 200
}
function Selection([string]$skill, [string]$source = 'Required') {
  $fieldName = if ($source -eq 'Required') { "Required level for $skill" } else { "$source contribution for $skill" }
  $combo = FindControl $main 'ControlType.ComboBox' $fieldName
  $combo.GetCurrentPattern([System.Windows.Automation.SelectionPattern]::Pattern).Current.GetSelection()[0].Current.Name
}
function Assert([bool]$condition, [string]$message) {
  if (-not $condition) { throw "FAILED: $message" }
  Write-Output "PASS: $message"
}
function Capture([string]$name) {
  $rect = New-Object PlannerWindowPrinter+Rect
  [PlannerWindowPrinter]::GetWindowRect($handle, [ref]$rect) | Out-Null
  $bitmap = New-Object System.Drawing.Bitmap(($rect.Right-$rect.Left), ($rect.Bottom-$rect.Top))
  $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
  $hdc = $graphics.GetHdc()
  try { if (-not [PlannerWindowPrinter]::PrintWindow($handle, $hdc, 2)) { throw 'Capture failed' } }
  finally { $graphics.ReleaseHdc($hdc) }
  $bitmap.Save((Join-Path $reviewPath $name), [System.Drawing.Imaging.ImageFormat]::Png)
  $graphics.Dispose()
  $bitmap.Dispose()
}
function ToggleSkill([string]$skill) {
  $catalogButton = FindControl $main 'ControlType.Button' $skill
  if ($null -eq $catalogButton) { throw "Missing catalog name: $skill" }
  $queryField = FindControl $main 'ControlType.Edit' 'Search skills'
  if ($queryField.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).Current.Value) {
    Assert (-not $catalogButton.Current.IsOffscreen) "filtered catalog name $skill is visible for selection"
  }
  $catalogButton.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern).Toggle()
  Start-Sleep -Milliseconds 200
}
function CatalogChecked([string]$skill) {
  $catalogButton = FindControl $main 'ControlType.Button' $skill
  $catalogButton.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern).Current.ToggleState -eq [System.Windows.Automation.ToggleState]::On
}
function SourceMode([string]$name) {
  $radio = FindControl $main 'ControlType.RadioButton' $name
  $radio.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
  Start-Sleep -Milliseconds 200
}
function SearchAndWait {
  Click 'SEARCH BUILDS'
  for ($attempt = 0; $attempt -lt 75; $attempt++) {
    Start-Sleep -Milliseconds 200
    $resultCount = Controls $main | Where-Object { $_.Current.ControlType.ProgrammaticName -eq 'ControlType.Text' -and $_.Current.Name -match '^[1-9][0-9]*Sets$' } | Select-Object -First 1
    if ($null -ne $resultCount) { return }
  }
  throw 'Search did not produce matching builds'
}
$workspacePath = Split-Path $PSScriptRoot -Parent
$reviewPath = Join-Path $workspacePath 'ui-review'
$catalogNames = @((Get-Content (Join-Path $workspacePath 'MHWsTool\Data\catalog.json') -Raw | ConvertFrom-Json).skills.name)
if (-not $AppPath) { $AppPath = Join-Path $workspacePath 'MHWsTool\bin\Release\net10.0-windows\MHWsTool.exe' }
$process = Start-Process -FilePath $AppPath -PassThru
try {
  for ($startupAttempt = 0; $startupAttempt -lt 40; $startupAttempt++) {
    Start-Sleep -Milliseconds 250
    $process.Refresh()
    if ($process.MainWindowHandle -ne [IntPtr]::Zero) { break }
  }
  $handle = $process.MainWindowHandle
  Assert ($handle -ne [IntPtr]::Zero) 'planner starts'
  $main = [System.Windows.Automation.AutomationElement]::FromHandle($handle)
  Start-Sleep -Milliseconds 400
  $catalogButtons = @(Controls $main | Where-Object { $_.Current.ControlType.ProgrammaticName -eq 'ControlType.Button' -and $_.Current.Name -in $catalogNames })
  Assert ($catalogButtons.Count -eq 179) 'all 179 Skill, Set and Group names can be selected from the catalog'
  Assert (@(Controls $main | Where-Object { $_.Current.ControlType.ProgrammaticName -eq 'ControlType.ComboBox' }).Count -eq 0) 'level fields appear after selecting names'
  Assert ($null -eq (FindControl $main 'ControlType.Button' 'NEXT →') -and $null -eq (FindControl $main 'ControlType.Button' '← BACK')) 'the planner has no step navigation'
  $searchButton = FindControl $main 'ControlType.Button' 'SEARCH BUILDS'
  Assert ($null -ne $searchButton -and -not $searchButton.Current.IsOffscreen) 'Search is always available'
  Capture 'planner-name-selection-catalog.png'

  ToggleSkill 'Attack Boost'
  Assert ((Selection 'Attack Boost') -eq 'Lv 1') 'selecting a name adds an editable requirement at the first valid level'
  Choose 'Attack Boost' 'Lv 5'
  SetSkillFilter 'Fulgur'
  ToggleSkill "Fulgur Anjanath's Will"
  Choose "Fulgur Anjanath's Will" 'Lv 2 · 4 pieces'
  SetSkillFilter 'Fortifying'
  ToggleSkill 'Fortifying Pelt'
  Assert ((Selection 'Fortifying Pelt') -eq 'Lv 1 · 3 pieces') 'group levels show the required piece threshold'
  SetSkillFilter 'Free Meal'
  ToggleSkill 'Free Meal'
  Choose 'Free Meal' 'Lv 3'
  Assert ($null -ne (FindControl $main 'ControlType.Text' '4 required skills selected')) 'selected names from all categories form one request'
  Assert (@(Controls $main | Where-Object { $_.Current.ControlType.ProgrammaticName -eq 'ControlType.ComboBox' }).Count -eq 4) 'only selected entries have level dropdowns'
  SetSkillFilter 'no_skill_with_this_name'
  Assert ($null -ne (FindControl $main 'ControlType.Text' 'No skills match this search.')) 'empty catalog filter has feedback'
  Assert ((Selection 'Attack Boost') -eq 'Lv 5' -and (Selection "Fulgur Anjanath's Will") -eq 'Lv 2 · 4 pieces' -and (Selection 'Free Meal') -eq 'Lv 3') 'catalog filtering keeps all selected names and levels available'
  SetSkillFilter ''
  Assert ((CatalogChecked 'Attack Boost') -and (CatalogChecked 'Free Meal')) 'selected catalog names have checked states'
  Capture 'planner-name-selection-levels.png'

  Click 'Remove Attack Boost'
  Assert (-not (CatalogChecked 'Attack Boost')) 'Remove also unchecks the catalog name'
  Assert ($null -eq (FindControl $main 'ControlType.ComboBox' 'Required level for Attack Boost')) 'Remove deletes its level field'
  ToggleSkill 'Free Meal'
  Assert ($null -eq (FindControl $main 'ControlType.ComboBox' 'Required level for Free Meal')) 'unchecking a name removes its requirement'
  ToggleSkill 'Free Meal'
  Assert ((Selection 'Free Meal') -eq 'Lv 1') 'reselecting a name adds one fresh requirement'
  Assert (@(Controls $main | Where-Object { $_.Current.ControlType.ProgrammaticName -eq 'ControlType.ComboBox' -and $_.Current.Name -eq 'Required level for Free Meal' }).Count -eq 1) 'reselecting never duplicates a requirement'

  Click 'Clear all settings'
  Assert ($null -ne (FindControl $main 'ControlType.Text' 'No required skills selected')) 'clear resets every requirement'
  Assert (@(Controls $main | Where-Object { $_.Current.ControlType.ProgrammaticName -eq 'ControlType.ComboBox' }).Count -eq 0) 'clear removes every level field'
  Assert (-not (CatalogChecked "Fulgur Anjanath's Will") -and -not (CatalogChecked 'Fortifying Pelt')) 'clear resets selected catalog names'
  Click 'SEARCH BUILDS'
  Assert ($null -ne (FindControl $main 'ControlType.Text' 'Choose a skill, set or group and its required level, then search.')) 'empty Search explains the selection flow'

  SetSkillFilter 'Free Meal'
  ToggleSkill 'Free Meal'
  SetSkillFilter 'Arkveld'
  ToggleSkill "Arkveld's Hunger"
  Choose "Arkveld's Hunger" 'Lv 2 · 4 pieces'
  SetSkillFilter 'Fortifying'
  ToggleSkill 'Fortifying Pelt'
  SetSkillFilter ''
  SearchAndWait
  Assert ($null -ne (Controls $main | Where-Object { $_.Current.ControlType.ProgrammaticName -eq 'ControlType.Text' -and $_.Current.Name -match '^[1-9][0-9]*Sets$' } | Select-Object -First 1)) 'selected Skill, Set and Group levels produce matching builds together'
  Assert ($null -ne (FindControl $main 'ControlType.Text' 'Free Meal Lv1')) 'result shows ordinary skills as level badges'
  Assert ($null -ne (FindControl $main 'ControlType.Text' "Arkveld's Hunger: Hasten Recovery II")) 'result shows the active set-bonus tier name'
  Assert ($null -ne (FindControl $main 'ControlType.Text' 'Fortifying Pelt: Fortify')) 'result shows the active group-bonus name'
  Assert ($null -ne (FindControl $main 'ControlType.Text' 'Free Slots') -and $null -ne (FindControl $main 'ControlType.Text' 'Total Defense') -and $null -ne (FindControl $main 'ControlType.Text' 'Elements')) 'result includes slots, defense and element sections'
  Capture 'planner-name-selection-results.png'
  Choose 'Free Meal' 'Lv 2'
  Assert ($null -ne (FindControl $main 'ControlType.Text' '0Sets')) 'level edits clear stale results'
  Click 'Remove Fortifying Pelt'
  Assert ($null -ne (FindControl $main 'ControlType.Text' '2 required skills selected')) 'removing a group updates the search requirements'

  $main.GetCurrentPattern([System.Windows.Automation.TransformPattern]::Pattern).Resize(1050, 690)
  SetSkillFilter ''
  Start-Sleep -Milliseconds 400
  Capture 'planner-name-selection-minimum.png'
  $searchButton = FindControl $main 'ControlType.Button' 'SEARCH BUILDS'
  $filterField = FindControl $main 'ControlType.Edit' 'Search skills'
  Assert (-not $searchButton.Current.IsOffscreen -and -not $filterField.Current.IsOffscreen) 'Search and catalog filter remain visible at minimum size'
  Choose "Arkveld's Hunger" 'Lv 1 · 2 pieces'
  Assert ((Selection "Arkveld's Hunger") -eq 'Lv 1 · 2 pieces') 'required levels can be edited at minimum size'
  Click 'Clear all settings'
  $main.GetCurrentPattern([System.Windows.Automation.TransformPattern]::Pattern).Resize(1370, 850)
  SetSkillFilter 'Free Meal'
  ToggleSkill 'Free Meal'
  Choose 'Free Meal' 'Lv 3'
  SourceMode 'On Weapon'
  ToggleSkill 'Free Meal'
  Choose 'Free Meal' 'Lv 2' 'Weapon'
  SourceMode 'On Talisman'
  ToggleSkill 'Free Meal'
  Assert ((Selection 'Free Meal' 'Talisman') -eq 'Lv 1') 'talisman has an independent contribution level'
  SourceMode 'Required'
  Assert ($null -ne (FindControl $main 'ControlType.Text' 'From gear Lv 3 · Need Lv 0 more')) 'weapon and talisman levels reduce the remaining requirement together'
  Assert ($null -ne (FindControl $main 'ControlType.Text' '1 required skill selected')) 'equipment skills do not add extra desired requirements'
  SearchAndWait
  Assert ($null -ne (FindControl $main 'ControlType.Text' 'Manual talisman')) 'manual talisman skills are used as a fixed talisman'

  SourceMode 'On Weapon'
  Choose 'Free Meal' 'Lv 1' 'Weapon'
  Assert ($null -ne (FindControl $main 'ControlType.Text' '0Sets')) 'editing weapon contributions invalidates old builds'
  SourceMode 'Required'
  Assert ($null -ne (FindControl $main 'ControlType.Text' 'From gear Lv 2 · Need Lv 1 more')) 'editing a supplied level updates the remaining requirement'
  SourceMode 'On Talisman'
  $manualTalisman = FindControl $main 'ControlType.CheckBox' 'Use manual talisman skills'
  $manualTalisman.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern).Toggle()
  Start-Sleep -Milliseconds 200
  Assert ($null -ne (FindControl $main 'ControlType.Text' 'From gear Lv 1 · Need Lv 2 more')) 'disabling manual talisman skills removes their contribution'
  $manualTalisman.GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern).Toggle()
  Start-Sleep -Milliseconds 200
  Assert ((Selection 'Free Meal' 'Talisman') -eq 'Lv 1') 'manual contributions are retained when disabled and re-enabled'

  SourceMode 'Required'
  SetSkillFilter 'Fulgur'
  ToggleSkill "Fulgur Anjanath's Will"
  Choose "Fulgur Anjanath's Will" 'Lv 2 · 4 pieces'
  SetSkillFilter 'Fortifying'
  ToggleSkill 'Fortifying Pelt'
  SourceMode 'On Weapon'
  ToggleSkill 'Fortifying Pelt'
  Assert ((Selection 'Fortifying Pelt' 'Weapon') -eq '1 piece toward this bonus') 'a group on a weapon contributes one piece'
  SetSkillFilter 'Fulgur'
  ToggleSkill "Fulgur Anjanath's Will"
  SourceMode 'On Talisman'
  ToggleSkill "Fulgur Anjanath's Will"
  SetSkillFilter 'Fortifying'
  ToggleSkill 'Fortifying Pelt'
  SetSkillFilter ''
  Capture 'planner-equipment-skills.png'
  SourceMode 'Required'
  Assert ($null -ne (FindControl $main 'ControlType.Text' 'From gear 2 piece(s) · Need 2 more of 4 pieces')) 'two equipment set contributions reduce a four-piece requirement to two'
  Assert ($null -ne (FindControl $main 'ControlType.Text' 'From gear 2 piece(s) · Need 1 more of 3 pieces')) 'two equipment group contributions reduce a three-piece requirement to one'
  Capture 'planner-equipment-remaining.png'
  SearchAndWait
  Capture 'planner-equipment-results.png'
  $plannerCatalog = Get-Content (Join-Path $workspacePath 'MHWsTool\Data\catalog.json') -Raw | ConvertFrom-Json
  $fulgurTierName = ($plannerCatalog.skills | Where-Object name -eq "Fulgur Anjanath's Will").ranks | Where-Object level -eq 2 | Select-Object -ExpandProperty name
  Assert ($null -ne (FindControl $main 'ControlType.Text' "Fulgur Anjanath's Will: $fulgurTierName") -and $null -ne (FindControl $main 'ControlType.Text' 'Fortifying Pelt: Fortify')) 'reduced set and group requirements still activate the requested tiers'

  SetSkillFilter 'Attack Boost'
  ToggleSkill 'Attack Boost'
  Choose 'Attack Boost' 'Lv 5'
  SourceMode 'On Weapon'
  ToggleSkill 'Attack Boost'
  Choose 'Attack Boost' 'Lv 5' 'Weapon'
  SourceMode 'Required'
  SetSkillFilter 'Poison Attack'
  ToggleSkill 'Poison Attack'
  Choose 'Poison Attack' 'Lv 3'
  SourceMode 'On Talisman'
  ToggleSkill 'Poison Attack'
  Choose 'Poison Attack' 'Lv 3' 'Talisman'
  SourceMode 'Required'
  SearchAndWait
  Assert ($null -ne (FindControl $main 'ControlType.Text' 'Attack Boost Lv5') -and $null -ne (FindControl $main 'ControlType.Text' 'Poison Attack Lv3')) 'both source contributions reach Search even without selected gear or weapon decoration slots'

  SourceMode 'On Weapon'
  Click "Remove Weapon Fulgur Anjanath's Will"
  SourceMode 'Required'
  Assert ($null -ne (FindControl $main 'ControlType.Text' 'From gear 1 piece(s) · Need 3 more of 4 pieces')) 'removing a supplied set skill restores the remaining piece requirement'
  $main.GetCurrentPattern([System.Windows.Automation.TransformPattern]::Pattern).Resize(1050, 690)
  SetSkillFilter ''
  Capture 'planner-equipment-minimum.png'
  Assert (-not (FindControl $main 'ControlType.Button' 'SEARCH BUILDS').Current.IsOffscreen) 'equipment requirements keep Search visible at minimum size'
  Click 'Clear all settings'
  SourceMode 'On Weapon'
  Assert ((FindControl $main 'ControlType.CheckBox' 'Use manual weapon skills').GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern).Current.ToggleState -eq [System.Windows.Automation.ToggleState]::Off) 'Clear resets manual weapon mode'
  Assert ((FindControl $main 'ControlType.CheckBox' 'Use manual talisman skills').GetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern).Current.ToggleState -eq [System.Windows.Automation.ToggleState]::Off) 'Clear resets manual talisman mode'
  Assert (@(Controls $main | Where-Object { $_.Current.ControlType.ProgrammaticName -eq 'ControlType.ComboBox' }).Count -eq 0) 'Clear removes requirements and all equipment contributions'

  SourceMode 'Required'
  $main.GetCurrentPattern([System.Windows.Automation.TransformPattern]::Pattern).Resize(1370, 850)
  foreach ($target in @(
    @('Free Meal', 'Lv 3'),
    @('Speed Eating', 'Lv 3'),
    @('Mushroomancer', 'Lv 3'),
    @('Weakness Exploit', 'Lv 5')
  )) {
    SetSkillFilter $target[0]
    ToggleSkill $target[0]
    Choose $target[0] $target[1]
  }
  SetSkillFilter ''
  SearchAndWait
  Assert ($null -ne (FindControl $main 'ControlType.Text' 'Tenderizer Jewel [3]')) 'equipped decorations use the reference name-and-level badge'
  Assert ($null -ne (FindControl $main 'ControlType.Image' 'armor decoration slots')) 'free slots include the reference armor category icon'
  foreach ($element in @('Fire', 'Water', 'Thunder', 'Ice', 'Dragon')) {
    Assert ($null -ne (FindControl $main 'ControlType.Image' $element)) "$element resistance uses the reference icon"
  }
  Capture 'planner-reference-results.png'
  Write-Output 'Catalog and equipment contribution planner checks passed.'
}
finally { if (-not $process.HasExited) { Stop-Process -Id $process.Id } }
