# KIK Test Pack

Example cfg files for testing KerbalInstructionsKit in-game.

## Installation

Copy the `KIK_TestPack/` folder to `GameData/` and rename the `.cfg.example` files to `.cfg`:

```
GameData/KIK_TestPack/lessons.cfg
GameData/KIK_TestPack/triggers.cfg
GameData/KIK_TestPack/contract.cfg   (requires ContractConfigurator)
```

## Contents

- 5 example lessons across 3 categories (Getting Started, Aviation, Advanced)
- 4 trigger definitions (3 game-start, 1 contract-based)
- 2 KIK_CONTRACT_LESSON bindings demonstrating Mission Control link injection

## Testing Checklist

1. Start a new sandbox game
2. Open the KIK toolbar button — archive should show 4 lessons (Rocket Basics, Plane Basics, EVA Guide, and potentially Orbital Mechanics if UnmannedOrbit is offered)
3. Navigate between lessons using links
4. Verify "Next: Orbital Mechanics" link is disabled until that lesson is unlocked
5. If ContractConfigurator is installed, check Mission Control for the "View Instructions" link
