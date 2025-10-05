# Enhanced Region Switching with NASA Data

## Overview

This document explains the enhanced region switching system that allows players to switch between different farm regions while maintaining synchronized NASA satellite data across all regions. The system ensures that when you switch regions, the NASA data is properly updated to reflect the new region's coordinates and environmental conditions.

## Key Features

### 1. Global Date Synchronization
- All regions use the same global date and week
- NASA data is synchronized across all regions
- Date changes affect all regions simultaneously

### 2. Region-Specific NASA Data
- Each region has its own coordinates for NASA data retrieval
- **Tropical Monsoon**: Sibu, Malaysia (1.5°N, 110.3°E)
- **Semi-Arid Steppe**: Dodoma, Tanzania (-6.2°S, 35.7°E)
- **Temperate Continental**: Canberra, Australia (-35.3°S, 149.1°E)

### 3. Automatic Data Refresh
- NASA data cache is cleared when switching regions
- Fresh data is fetched for the new region
- Fallback to RNG data if NASA APIs are unavailable

### 4. UI Feedback
- Loading indicators when switching regions
- Progress bars for NASA data loading
- Status messages showing data fetch progress

## System Components

### RegionDataManager
The main controller for region switching and NASA data management.

**Key Methods:**
- `SwitchRegion(RegionType newRegion)` - Switches to a new region and updates NASA data
- `UpdateNasaDataForRegion(RegionType region)` - Refreshes NASA data for a specific region
- `GetCurrentRegionData()` - Gets data for the current region

### GlobalDateManager
Manages global date synchronization across all regions.

**Key Methods:**
- `GetCurrentGlobalDate()` - Gets the current global date
- `AdvanceGlobalWeek()` - Advances the global week by one
- `SynchronizeAllRegions()` - Ensures all regions use the same date

### EnhancedDataService
Provides NASA data with fallback to RNG data.

**Key Methods:**
- `ClearCache()` - Clears cached data to force refresh
- `UpdateRegion(RegionType newRegion)` - Updates the region for data service
- `PreloadData(int startWeek, int endWeek)` - Preloads data for better performance

### RegionSwitcherUI
UI component for region switching with loading feedback.

**Key Methods:**
- `SelectRegion(RegionType newRegion)` - Switches to a new region
- `ShowNasaDataLoading(RegionType newRegion)` - Shows loading UI
- `UpdateNasaDataLoadingProgress(float progress, string status)` - Updates loading progress

## Usage Examples

### Basic Region Switching
```csharp
// Get the region data manager
RegionDataManager regionManager = FindFirstObjectByType<RegionDataManager>();

// Switch to Temperate Continental (Australia)
regionManager.SwitchRegion(RegionType.TemperateContinental);

// Get NASA data for current week
WeeklyEnvironment env = regionManager.GetWeeklyEnvironment(0);
```

### Global Date Management
```csharp
// Get the global date manager
GlobalDateManager dateManager = GlobalDateManager.Instance;

// Get current global date
DateTime currentDate = dateManager.GetCurrentGlobalDate();

// Advance to next week
dateManager.AdvanceGlobalWeek();

// Get date for specific week
DateTime week5Date = dateManager.GetDateForWeek(5);
```

### NASA Data Loading with UI Feedback
```csharp
// Get the region switcher UI
RegionSwitcherUI regionSwitcher = FindFirstObjectByType<RegionSwitcherUI>();

// Switch region with loading feedback
regionSwitcher.SelectRegion(RegionType.TemperateContinental);

// Update loading progress
regionSwitcher.UpdateNasaDataLoadingProgress(0.5f, "Fetching precipitation data...");
```

## Configuration

### NASA Data Configuration
The system uses `NasaDataConfig` ScriptableObject for configuration:

- **Use Real NASA Data**: Enable/disable real data usage
- **Start Date**: Set the global start date (default: March 1, 2024)
- **Data Refresh Interval**: How often to refresh data from NASA APIs
- **Cache Settings**: Configure data caching for performance

### Region Coordinates
Each region has specific coordinates for NASA data retrieval:

```csharp
// Tropical Monsoon - Sibu, Malaysia
RegionCoordinates(1.5f, 110.3f)

// Semi-Arid Steppe - Dodoma, Tanzania  
RegionCoordinates(-6.2f, 35.7f)

// Temperate Continental - Canberra, Australia
RegionCoordinates(-35.3f, 149.1f)
```

## NASA Data Sources

The system integrates with three NASA data sources:

### 1. IMERG (Integrated Multi-satellitE Retrievals for GPM)
- **Purpose**: Global precipitation estimates
- **Resolution**: 0.1° × 0.1° (~10km)
- **Temporal**: 30-minute, daily, monthly
- **API**: NASA GPM Precipitation Measurement Missions

### 2. NASA POWER (Prediction of Worldwide Energy Resources)
- **Purpose**: Evapotranspiration and meteorological data
- **Resolution**: 0.5° × 0.5° (~50km)
- **Temporal**: Daily
- **API**: POWER API

### 3. MODIS NDVI (Moderate Resolution Imaging Spectroradiometer)
- **Purpose**: Vegetation health and phenology
- **Resolution**: 500m × 500m
- **Temporal**: 16-day composites
- **API**: NASA Earthdata

## Implementation Details

### Data Flow
1. Player selects a new region
2. RegionDataManager switches to the new region
3. NASA data cache is cleared
4. Fresh data is fetched for the new region's coordinates
5. UI shows loading progress
6. Game continues with new region's data

### Error Handling
- If NASA APIs are unavailable, system falls back to RNG data
- Loading indicators show data fetch progress
- Error messages are logged for debugging

### Performance Optimization
- Data is cached to avoid repeated API calls
- Preloading is available for better performance
- Fallback data is generated locally

## Testing

### Context Menu Testing
The RegionSwitcherUI includes context menu options for testing:

- **Test Region Switch - Tropical Monsoon**
- **Test Region Switch - Semi-Arid Steppe**  
- **Test Region Switch - Temperate Continental**

### Debug Information
The system provides extensive debug logging:

```csharp
Debug.Log($"RegionDataManager: Switching from {currentRegion} to {newRegion}");
Debug.Log($"RegionDataManager: NASA data updated for {region}");
Debug.Log($"EnhancedDataService: Cache cleared for {regionType}");
```

## Troubleshooting

### Common Issues

1. **NASA Data Not Loading**
   - Check internet connection
   - Verify NASA API credentials
   - Check fallback data is enabled

2. **Region Switching Not Working**
   - Ensure RegionDataManager is properly initialized
   - Check that all regions are unlocked
   - Verify UI references are assigned

3. **Date Synchronization Issues**
   - Check GlobalDateManager is initialized
   - Verify all regions use the same start date
   - Ensure date changes are propagated to all regions

### Debug Commands
```csharp
// Check current region
RegionType currentRegion = regionManager.GetCurrentRegion();

// Check NASA data availability
bool isUsingRealData = regionManager.IsUsingRealData;

// Check global date
DateTime globalDate = GlobalDateManager.Instance.GetCurrentGlobalDate();
```

## Future Enhancements

### Planned Features
- Real-time NASA data integration
- Historical data access
- Custom region coordinates
- Data visualization tools
- Performance metrics

### API Integration
- Full IMERG API integration
- POWER API authentication
- MODIS data processing
- Data quality validation

This enhanced region switching system provides a robust foundation for managing multiple farm regions with synchronized NASA satellite data, ensuring players can experience authentic environmental conditions for each region while maintaining a consistent global timeline.
