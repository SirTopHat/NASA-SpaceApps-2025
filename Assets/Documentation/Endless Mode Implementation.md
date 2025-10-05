# Endless Mode Implementation Guide

## Overview
This document describes the major overhauls implemented to transform the farming game from a fixed-season system to an endless, multi-region farming experience.

## Key Changes

### 1. Endless Mode vs Fixed Seasons
- **Before**: Fixed 20-week seasons with automatic harvest
- **After**: Endless gameplay with per-plot crop cycles (plant → manage → harvest → replant)
- **Implementation**: `EndlessTurnManager.cs` replaces `TurnManager.cs`

### 2. Per-Plot Crop Cycles
- Each plot operates independently with its own crop lifecycle
- Crops have maturity targets (e.g., Rice: 110 GP, Maize: 100 GP)
- Individual harvest timing based on growth accumulation
- Automatic plot reset after harvest for replanting

### 3. Universal Crops with Suitability
- **Before**: Region-specific crop lists
- **After**: Universal crop list with suitability ratings per region
- **Suitability Levels**:
  - **Good**: Normal growth and standard responses
  - **Marginal**: -20% growth rate, stronger sensitivity to mistakes
  - **Poor**: -40% growth rate, risk of failure within 2-3 weeks

### 4. Multi-Region System
- 3 unlockable regions with global synced time
- Region unlock costs: Region 2 (250 Gold), Region 3 (500 Gold)
- Global gold pool shared across regions
- Independent plot states per region

### 5. Enhanced Plot System
- **Before**: 2-4 plots maximum
- **After**: 2-9 plots per region with escalating costs
- Cost sequence: 60 → 75 → 95 → 120 → 150 → 190 → 240 → 300 → 375 Gold

## New Files Created

### Core Systems
- `Assets/Scripts/Game/Data/GlobalGameState.cs` - Global game state management
- `Assets/Scripts/Game/Systems/EndlessTurnManager.cs` - Endless mode turn management
- `Assets/Scripts/Game/EndlessGameRunner.cs` - Main game controller for endless mode

### UI Systems
- `Assets/Scripts/Game/UI/RegionSwitcherUI.cs` - Region switching interface
- `Assets/Scripts/Game/UI/CropSuitabilityUI.cs` - Suitability display
- `Assets/Scripts/Game/UI/EndlessCropPicker.cs` - Enhanced crop selection

### Data Models
- Updated `PlotState.cs` with maturity tracking and suitability
- Updated `CropDefinition.cs` with suitability data and maturity targets

## Crop Suitability Examples

### Rice
- **Monsoon**: Good (0) - Thrives in wet conditions
- **Steppe**: Poor (2) - Too dry, high failure risk
- **Temperate**: Marginal (1) - Possible with irrigation

### Maize
- **All Regions**: Good (0) - Universal crop

### Sorghum/Millet
- **Monsoon**: Marginal (1) - Too wet, but manageable
- **Steppe**: Good (0) - Drought-resistant
- **Temperate**: Marginal (1) - Possible with timing

### Wheat
- **Monsoon**: Poor (2) - Too wet, high failure risk
- **Steppe**: Marginal (1) - Possible with irrigation
- **Temperate**: Good (0) - Ideal conditions

### Soy
- **Monsoon**: Marginal (1) - Possible with drainage
- **Steppe**: Marginal (1) - Requires irrigation
- **Temperate**: Good (0) - Good conditions

## Save System
- Persistent global state across game sessions
- Per-region save data with independent plot states
- Auto-save after each week and region switch
- JSON-based serialization in `Application.persistentDataPath`

## Migration Guide

### For Existing Games
1. Replace `GameRunner` with `EndlessGameRunner` in scenes
2. Update UI references to use new endless mode components
3. Configure universal crop list in `EndlessCropPicker`
4. Set up region unlock costs in `BalanceConfig`

### For New Games
1. Use `EndlessGameRunner` as the main game controller
2. Configure `RegionSwitcherUI` for multi-region navigation
3. Set up `CropSuitabilityUI` for suitability feedback
4. Configure universal crops with suitability data

## Balance Tuning

### Crop Maturity Targets
- Rice: 110 GP (slower, higher yield)
- Maize: 100 GP (balanced)
- Sorghum: 80 GP (faster, lower yield)
- Wheat: 95 GP (moderate)
- Soy: 100 GP (balanced)

### Suitability Multipliers
- Good: 1.0x growth rate
- Marginal: 0.8x growth rate (-20%)
- Poor: 0.6x growth rate (-40%) + failure risk

### Region Unlock Costs
- Region 2 (Semi-Arid Steppe): 250 Gold
- Region 3 (Temperate Continental): 500 Gold

## Testing Checklist
- [ ] Per-plot crop cycles work independently
- [ ] Suitability affects growth rates correctly
- [ ] Poor suitability crops can fail
- [ ] Region unlocking works with global gold
- [ ] Save/load preserves all state
- [ ] UI updates reflect current state
- [ ] Harvest timing is accurate
- [ ] Plot purchasing respects limits

## Future Enhancements
- Crop rotation bonuses
- Seasonal weather patterns
- Market price fluctuations
- Advanced irrigation systems
- Pest and disease mechanics
- Contract farming opportunities
