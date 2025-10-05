# Region Buying System Setup Guide

## Overview

This guide explains how to set up the region buying system that allows players to purchase locked regions using gold. The system includes visual feedback, cost display, and confirmation dialogs.

## Features Implemented

### ✅ **Core Functionality:**
- **Locked Region Detection:** Automatically detects which regions are locked
- **Visual Feedback:** Greyed out buttons for locked regions with cost display
- **Buying Dialog:** Confirmation dialog with cost and gold balance
- **Gold Integration:** Uses existing universal gold system
- **Automatic Switching:** Switches to newly unlocked region after purchase

### ✅ **UI Components:**
- **Region Buttons:** Show locked/unlocked status with visual indicators
- **Cost Display:** Shows unlock cost for each region
- **Buy Dialog:** Confirmation dialog with cost breakdown
- **Gold Display:** Shows current gold vs. required cost

## Setup Instructions

### 1. UI Setup in Unity

#### Region Switcher UI Components:
1. **Region Buy Dialog Panel:**
   - Create a new GameObject for the buy dialog
   - Add the following child components:
     - `TMP_Text` for title (e.g., "Unlock Semi-Arid Steppe")
     - `TMP_Text` for description (region benefits)
     - `TMP_Text` for cost display (cost vs. current gold)
     - `Button` for confirm purchase
     - `Button` for cancel

2. **Region Button Cost Text:**
   - Add `TMP_Text` components to each region button
   - These will display "Cost: X Gold" or "Unlocked"

#### Assign References in RegionSwitcherUI:
```csharp
[Header("Region Buying UI")]
[SerializeField] private GameObject regionBuyDialog;
[SerializeField] private TMP_Text buyDialogTitle;
[SerializeField] private TMP_Text buyDialogDescription;
[SerializeField] private TMP_Text buyDialogCost;
[SerializeField] private Button buyConfirmButton;
[SerializeField] private Button buyCancelButton;
[SerializeField] private TMP_Text[] regionButtonCostTexts; // Cost text for each region button
```

### 2. Region Costs Configuration

The system uses the existing `GlobalGameState` costs:
- **Semi-Arid Steppe:** 250 Gold (region2UnlockCost)
- **Temperate Continental:** 500 Gold (region3UnlockCost)
- **Tropical Monsoon:** Free (starting region)

### 3. Visual States

#### Button States:
- **Current Region:** Green color, highlighted
- **Unlocked Region:** White color, clickable
- **Locked Region:** Grey color, shows buy dialog on click

#### Cost Text States:
- **Unlocked:** "Unlocked" in green
- **Locked:** "Cost: X Gold" in yellow
- **Insufficient Gold:** Red text in buy dialog

## How It Works

### 1. Region Selection Flow:
```
Player clicks region button
    ↓
Is region unlocked?
    ↓                    ↓
   Yes                  No
    ↓                    ↓
Switch to region    Show buy dialog
    ↓                    ↓
Update UI          Player confirms?
                        ↓
                   Purchase region
                        ↓
                   Switch to new region
```

### 2. Buy Dialog Flow:
```
Show buy dialog
    ↓
Update content (cost, gold, description)
    ↓
Player clicks confirm
    ↓
Check if player has enough gold
    ↓                    ↓
   Yes                  No
    ↓                    ↓
Deduct gold          Show error
Unlock region
    ↓
Switch to region
Update UI
```

## Code Structure

### Key Methods in RegionSwitcherUI:

#### Region Selection:
- `SelectRegion(RegionType newRegion)` - Main entry point
- `SwitchToRegion(RegionType newRegion)` - Handles unlocked regions
- `ShowRegionBuyDialog(RegionType region)` - Shows buy dialog for locked regions

#### Buy Dialog Management:
- `UpdateBuyDialogContent(RegionType region)` - Updates dialog with region info
- `ConfirmRegionPurchase()` - Processes the purchase
- `CancelRegionPurchase()` - Cancels the purchase

#### UI Updates:
- `SetupRegionButtons()` - Updates all region button states
- `UpdateRegionButtonState()` - Updates individual button appearance
- `UpdateRegionCostText()` - Updates cost display text

### Key Methods in EndlessGameRunner:

#### Region Management:
- `IsRegionUnlocked(RegionType region)` - Checks unlock status
- `GetRegionUnlockCost(RegionType region)` - Gets unlock cost
- `GetCurrentGold()` - Gets current gold amount
- `UnlockRegion(RegionType region)` - Unlocks region (existing method)

## Testing

### Manual Testing:
1. **Start Game:** Only Tropical Monsoon should be unlocked
2. **Click Locked Region:** Should show buy dialog
3. **Check Costs:** Semi-Arid Steppe (250), Temperate Continental (500)
4. **Test Purchase:** Buy region with sufficient gold
5. **Test Insufficient Gold:** Try to buy with insufficient gold
6. **Test Switching:** Switch between unlocked regions

### Debug Commands:
```csharp
// Test region unlock status
Debug.Log($"Semi-Arid Steppe unlocked: {gameRunner.IsRegionUnlocked(RegionType.SemiAridSteppe)}");

// Test region costs
Debug.Log($"Semi-Arid Steppe cost: {gameRunner.GetRegionUnlockCost(RegionType.SemiAridSteppe)}");

// Test current gold
Debug.Log($"Current gold: {gameRunner.GetCurrentGold()}");
```

## Customization

### Changing Region Costs:
Edit the `GlobalGameState` values:
```csharp
public int region2UnlockCost = 250;  // Semi-Arid Steppe
public int region3UnlockCost = 500;  // Temperate Continental
```

### Adding New Regions:
1. Add new region to `RegionType` enum
2. Add to `availableRegions` array in RegionSwitcherUI
3. Add display name to `regionDisplayNames` array
4. Add cost logic to `GetRegionUnlockCost()` method

### Customizing UI:
- **Button Colors:** Modify `UpdateRegionButtonState()` method
- **Dialog Content:** Modify `UpdateBuyDialogContent()` method
- **Cost Display:** Modify `UpdateRegionCostText()` method

## Troubleshooting

### Common Issues:

1. **Buy Dialog Not Showing:**
   - Check that `regionBuyDialog` is assigned
   - Verify `ShowRegionBuyDialog()` is being called
   - Check console for error messages

2. **Buttons Not Updating:**
   - Ensure `SetupRegionButtons()` is called after region changes
   - Check that `IsRegionUnlocked()` returns correct values
   - Verify button references are assigned

3. **Purchase Not Working:**
   - Check that `gameRunner.UnlockRegion()` is working
   - Verify gold is being deducted correctly
   - Check that region is added to unlocked regions list

4. **Cost Display Issues:**
   - Ensure `regionButtonCostTexts` array is properly assigned
   - Check that `GetRegionUnlockCost()` returns correct values
   - Verify text components are enabled and visible

### Debug Logging:
The system includes comprehensive debug logging:
```
RegionSwitcherUI: Showing buy dialog for SemiAridSteppe
RegionSwitcherUI: Successfully unlocked SemiAridSteppe
RegionSwitcherUI: Failed to unlock TemperateContinental - insufficient gold
```

## Performance Considerations

- **Button Updates:** Only updates when region menu is opened
- **Dialog Management:** Dialog is shown/hidden as needed
- **Gold Checks:** Cached to avoid repeated calculations

## Future Enhancements

### Planned Features:
- **Region Previews:** Show region benefits before purchase
- **Discounts:** Special offers for certain conditions
- **Achievements:** Unlock regions through achievements
- **Regional Bonuses:** Special bonuses for each region

This region buying system provides a complete solution for monetizing region access while maintaining a smooth user experience with clear visual feedback and intuitive controls.
