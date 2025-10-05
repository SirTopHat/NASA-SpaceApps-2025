# NASA Data Card Debugging Guide

## Problem Description
The NASA data card manager doesn't seem to update with the latest data when switching regions, and it's unclear if the data has been successfully fetched.

## Root Cause Analysis
The issue was that NASA data card components were getting their data from `turnManager.Env`, but when switching regions, the `turnManager` reference in the NASA data card components wasn't being updated with the new region's data.

## Solution Implemented

### 1. Enhanced EndlessGameRunner
- Added `NotifyNasaDataCardsOfRegionChange()` method
- Automatically finds and notifies all NASA data card components when regions switch
- Ensures proper turn manager reference updates

### 2. Enhanced NASA Data Card Components
- Added `OnRegionChanged()` method to both `EndlessNasaDataCard` and `GameUINasaDataCard`
- Added `ForceDataRefresh()` method to ensure data is properly refreshed
- Added comprehensive error handling and logging
- Added null checks and fallback displays

### 3. Data Flow After Region Switch
1. **Region Switch Initiated** → `EndlessGameRunner.SwitchRegion()`
2. **New Turn Manager Created** → `ReinitializeForNewRegion()`
3. **NASA Data Cards Notified** → `NotifyNasaDataCardsOfRegionChange()`
4. **Turn Manager Reference Updated** → `OnRegionChanged()`
5. **Display Refreshed** → `ForceDataRefresh()`

## Debugging Steps

### 1. Check Console Logs
Look for these debug messages when switching regions:

```
EndlessGameRunner: Switching from [OldRegion] to [NewRegion]
EndlessGameRunner: Notified X NASA data cards of region change to [NewRegion]
EndlessNasaDataCard: Region changed to [NewRegion]
EndlessNasaDataCard: Updated with new region data for [NewRegion]
EndlessNasaDataCard: Updated display with data - Rain: X.Xmm, ET: X.Xmm, Phase: [Phase]
```

### 2. Verify NASA Data Card References
Ensure your NASA data card components are properly assigned:
- Check that `EndlessNasaDataCard` components are in the scene
- Verify `dataCardText` and `dataCardMenu` references are assigned
- Confirm `gameRunner` reference is set in the NASA data card

### 3. Check Data Source
Verify the data source is correct:
```csharp
// In EndlessGameRunner, check these logs:
Debug.Log($"EndlessGameRunner: Data Source: {dataService.DataSource}");
Debug.Log($"EndlessGameRunner: Using Real Data: {dataService.IsUsingRealData}");
```

### 4. Test Data Fetching
Add this debug code to test if data is being fetched:

```csharp
// In EndlessNasaDataCard.UpdateDataCardDisplay()
var env = turnManager.Env;
Debug.Log($"Current Environment Data:");
Debug.Log($"  Rain: {env.rainMm}mm");
Debug.Log($"  ET: {env.etMm}mm");
Debug.Log($"  NDVI Phase: {env.ndviPhase}");
Debug.Log($"  Data Source: {turnManager.DataService?.DataSource}");
```

## Common Issues and Solutions

### Issue 1: NASA Data Card Not Updating
**Symptoms:** Data card shows old data after region switch
**Solution:** 
- Check that `OnRegionChanged()` is being called
- Verify turn manager reference is updated
- Ensure `ForceDataRefresh()` is called

### Issue 2: "TurnManager is null" Warning
**Symptoms:** Console shows "TurnManager is null, cannot update display"
**Solution:**
- Check that `gameRunner` reference is assigned in NASA data card
- Verify `EndlessGameRunner` is properly initialized
- Ensure region switching is working correctly

### Issue 3: Data Shows "Loading..." or "Error loading data"
**Symptoms:** NASA data card shows loading message or error
**Solution:**
- Check that `turnManager.Env` is not null
- Verify NASA data service is properly initialized
- Check for exceptions in data fetching

### Issue 4: Same Data Across Regions
**Symptoms:** Different regions show identical data
**Solution:**
- Verify region coordinates are different
- Check that NASA data service is clearing cache
- Ensure `UpdateRegion()` is called on data service

## Testing Commands

### Manual Testing
Use these context menu options to test region switching:

```csharp
[ContextMenu("Test Region Switch - Tropical Monsoon")]
[ContextMenu("Test Region Switch - Semi-Arid Steppe")]
[ContextMenu("Test Region Switch - Temperate Continental")]
```

### Debug Commands
Add these to test data fetching:

```csharp
// Test current region data
Debug.Log($"Current Region: {gameRunner.GetCurrentRegion()}");
Debug.Log($"Data Source: {gameRunner.GetDataSource()}");
Debug.Log($"Using Real Data: {gameRunner.IsUsingRealData()}");

// Test NASA data card
var nasaCard = FindObjectOfType<EndlessNasaDataCard>();
nasaCard.ForceDataRefresh();
```

## Expected Behavior

### When Switching Regions:
1. **Console Logs:** Should show region switch messages
2. **NASA Data Card:** Should update with new region's data
3. **Data Values:** Should reflect region-specific coordinates
4. **Display:** Should show current week's environmental data

### Data Differences by Region:
- **Tropical Monsoon:** Higher rainfall, moderate ET
- **Semi-Arid Steppe:** Lower rainfall, higher ET  
- **Temperate Continental:** Moderate rainfall and ET (Australian data)

## Troubleshooting Checklist

- [ ] NASA data card components are in the scene
- [ ] `gameRunner` reference is assigned in NASA data card
- [ ] Region switching logs appear in console
- [ ] NASA data card receives `OnRegionChanged()` call
- [ ] Turn manager reference is updated
- [ ] Data display refreshes after region switch
- [ ] Different regions show different data values
- [ ] No null reference exceptions in console

## Performance Considerations

- NASA data cards are found using `FindObjectsOfType()` which can be expensive
- Consider caching NASA data card references for better performance
- Data refresh uses coroutines to avoid blocking the main thread

## Future Improvements

1. **Event System:** Use UnityEvents instead of direct method calls
2. **Caching:** Cache NASA data card references for better performance
3. **Validation:** Add data validation to ensure data quality
4. **Visual Feedback:** Add loading indicators during data refresh

This debugging guide should help you identify and resolve any issues with NASA data card updates when switching regions.
