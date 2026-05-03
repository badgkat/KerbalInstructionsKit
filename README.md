# Kerbal Instructions Kit

Content-driven instructional panel for KSP 1.12.x. Lesson content is defined entirely in KSP cfg files; KIK provides the rendering, state tracking, and unlock trigger system.

## Features

- In-game IMGUI panel with multi-page lessons, images, captions, and cross-links
- Archive view with search and category grouping
- Lesson unlock triggers: game start, game events, contract state changes, flags
- ContractConfigurator integration: attach lessons to CC contracts via `KIK_CONTRACT_LESSON` nodes
- Harmony patch injects "View Instructions" link into Mission Control contract details
- Expression-based visibility gating (`visibleIf = LSN_Basics.unlocked && advanced_mode.set`)
- Per-save state persistence via ScenarioModule
- Pause/resume support in flight scene

## Dependencies

- Harmony (000_Harmony)
- ClickThroughBlocker (000_ClickThroughBlocker)
- ToolbarControl (001_ToolbarControl)

## Optional Dependencies

- ContractConfigurator — enables `KIK_CONTRACT_LESSON` bindings and contract-panel link injection

## Building

```
dotnet build src/KerbalInstructionsKit/KerbalInstructionsKit.csproj -c Release
```

Output goes to `Plugins/KerbalInstructionsKit.dll`.

## Testing

```
dotnet test tests/KerbalInstructionsKit.Tests/KerbalInstructionsKit.Tests.csproj
```
