# NASA Data Integration Guide

## Overview

This guide explains how to integrate real NASA satellite data into your farming game, replacing the current RNG-based simulation with authentic satellite data from IMERG, POWER, and MODIS.

## Available NASA Data Sources

### 1. IMERG (Integrated Multi-satellitE Retrievals for GPM)
- **Purpose**: Global precipitation estimates
- **Resolution**: 0.1° × 0.1° (~10km)
- **Temporal**: 30-minute, daily, monthly
- **API**: NASA GPM Precipitation Measurement Missions
- **URL**: https://gpm.nasa.gov/data/imerg

### 2. NASA POWER (Prediction of Worldwide Energy Resources)
- **Purpose**: Evapotranspiration and meteorological data
- **Resolution**: 0.5° × 0.5° (~50km)
- **Temporal**: Daily
- **API**: POWER API
- **URL**: https://power.larc.nasa.gov/api/temporal/daily/point

### 3. MODIS NDVI (Moderate Resolution Imaging Spectroradiometer)
- **Purpose**: Vegetation health and phenology
- **Resolution**: 500m × 500m
- **Temporal**: 16-day composites
- **API**: NASA Earthdata
- **URL**: https://e4ftl01.cr.usgs.gov/MOLT/MOD13A1.006/

## Implementation Steps

### Step 1: Setup NASA Data Configuration

1. Create a `NasaDataConfig` ScriptableObject in your project
2. Configure the following settings:
   - **Use Real NASA Data**: Enable/disable real data usage
   - **Start Date**: Set your game's starting date (e.g., March 1, 2024)
   - **Earthdata Credentials**: Add your NASA Earthdata username/password
   - **Cache Settings**: Configure data caching for performance

### Step 2: Replace DataService in Game Systems

```csharp
// Old way (RNG-based)
DataService dataService = new DataService(regionType, 12345);

// New way (NASA data with fallback)
NasaDataConfig config = Resources.Load<NasaDataConfig>("NasaDataConfig");
EnhancedDataService dataService = new EnhancedDataService(regionType, config);
```

### Step 3: Update Game Runners

Modify your `GameRunner` and `EndlessGameRunner` to use the new data service:

```csharp
// In GameRunner.cs
private void Start()
{
    NasaDataConfig config = Resources.Load<NasaDataConfig>("NasaDataConfig");
    EnhancedDataService dataService = new EnhancedDataService(region, config);
    turnManager = new TurnManager(dataService, balanceConfig, soil, totalWeeks, plots);
    // ... rest of initialization
}
```

### Step 4: Handle Data Availability

The system automatically falls back to RNG data when NASA data is unavailable:

```csharp
// Check if using real data
if (dataService.IsUsingRealData)
{
    Debug.Log("Using authentic NASA satellite data");
}
else
{
    Debug.Log("Using fallback RNG data");
}
```

## Data Processing Pipeline

### 1. Data Fetching
- **IMERG**: Fetch daily precipitation, aggregate to weekly totals
- **POWER**: Fetch daily ET, calculate weekly averages
- **MODIS**: Determine NDVI phase from 16-day composites

### 2. Data Validation
- Check for reasonable value ranges
- Validate temporal consistency
- Handle missing or corrupted data

### 3. Regional Mapping
- Map global coordinates to your game regions
- Apply regional climate characteristics
- Handle coordinate transformations

## Configuration Examples

### Basic Configuration
```csharp
[CreateAssetMenu(fileName = "Basic Nasa Config", menuName = "FarmFromSpace/NASA Data Config")]
public class BasicNasaConfig : NasaDataConfig
{
    public BasicNasaConfig()
    {
        useRealNasaData = true;
        startDate = new DateTime(2024, 3, 1);
        enableDataCaching = true;
        maxCacheWeeks = 20;
        enableFallbackData = true;
    }
}
```

### Advanced Configuration
```csharp
[CreateAssetMenu(fileName = "Advanced Nasa Config", menuName = "FarmFromSpace/NASA Data Config")]
public class AdvancedNasaConfig : NasaDataConfig
{
    public AdvancedNasaConfig()
    {
        useRealNasaData = true;
        startDate = new DateTime(2024, 3, 1);
        enableDataCaching = true;
        maxCacheWeeks = 50;
        dataRefreshIntervalHours = 6;
        enableAutoUpdates = true;
        earthdataUsername = "your_username";
        earthdataPassword = "your_password";
        enableDebugLogging = true;
        minDataQuality = 0.8f;
        enableDataValidation = true;
    }
}
```

## API Integration Details

### IMERG Data Access
```csharp
private float FetchImergData(RegionCoordinates coords, DateTime date)
{
    // 1. Construct API URL with coordinates and date
    string apiUrl = $"https://gpm1.gesdisc.eosdis.nasa.gov/opendap/GPM_L3/GPM_3IMERGDF.06/{date:yyyy}/{date:MM:dd}/";
    
    // 2. Make HTTP request to NASA API
    // 3. Parse NetCDF/HDF5 response
    // 4. Extract precipitation data for coordinates
    // 5. Aggregate daily data to weekly totals
    
    return weeklyPrecipitationMm;
}
```

### POWER Data Access
```csharp
private float FetchPowerData(RegionCoordinates coords, DateTime date)
{
    // 1. Construct POWER API URL
    string apiUrl = $"https://power.larc.nasa.gov/api/temporal/daily/point?parameters=ET&community=AG&longitude={coords.Longitude}&latitude={coords.Latitude}&start={date:yyyyMMdd}&end={date:yyyyMMdd}";
    
    // 2. Make HTTP request
    // 3. Parse JSON response
    // 4. Extract ET values
    // 5. Calculate weekly average
    
    return weeklyETMm;
}
```

### MODIS Data Access
```csharp
private NdviPhase FetchModisData(RegionCoordinates coords, DateTime date)
{
    // 1. Determine MODIS tile and date
    // 2. Access MODIS NDVI data
    // 3. Calculate NDVI value for coordinates
    // 4. Determine phenological phase based on NDVI trends
    
    return ndviPhase;
}
```

## Performance Considerations

### Caching Strategy
- Cache frequently accessed data in memory
- Implement LRU (Least Recently Used) cache eviction
- Preload data for upcoming weeks
- Use background threads for data fetching

### Data Compression
- Store data in efficient binary formats
- Compress historical data
- Use appropriate data types (float vs double)

### Network Optimization
- Batch API requests when possible
- Implement request queuing
- Handle network timeouts gracefully
- Use connection pooling

## Error Handling

### Network Errors
```csharp
try
{
    var env = nasaDataService.GetWeeklyEnvironment(weekIndex);
}
catch (NetworkException e)
{
    Debug.LogWarning($"Network error: {e.Message}, using fallback data");
    return fallbackService.GetWeeklyEnvironment(weekIndex);
}
```

### Data Validation
```csharp
private bool ValidateDataQuality(WeeklyEnvironment env)
{
    // Check for reasonable ranges
    if (env.rainMm < 0 || env.rainMm > 200) return false;
    if (env.etMm < 0 || env.etMm > 50) return false;
    
    // Check for temporal consistency
    // Check for spatial consistency
    
    return true;
}
```

## Testing and Debugging

### Debug Tools
```csharp
[ContextMenu("Test NASA Data")]
public void TestNasaData()
{
    for (int week = 0; week < 10; week++)
    {
        var env = GetWeeklyEnvironment(week);
        Debug.Log($"Week {week}: Rain={env.rainMm:F1}mm, ET={env.etMm:F1}mm, NDVI={env.ndviPhase}");
    }
}
```

### Data Visualization
- Create graphs showing data trends
- Compare NASA data vs RNG data
- Validate against known weather patterns
- Monitor data quality metrics

## Deployment Considerations

### Build Settings
- Include NASA data configuration in builds
- Handle missing data gracefully
- Provide offline fallback options
- Optimize for different platforms

### User Experience
- Show data source indicators in UI
- Provide data quality information
- Allow users to toggle between real and simulated data
- Display data update status

## Future Enhancements

### Real-time Data
- Implement near real-time data updates
- Use WebSocket connections for live data
- Handle data latency and delays

### Machine Learning
- Use ML models to predict missing data
- Improve data quality through ML
- Generate synthetic data for gaps

### Advanced Analytics
- Historical data analysis
- Climate trend detection
- Seasonal pattern recognition
- Predictive modeling

## Conclusion

Integrating NASA satellite data into your farming game provides:
- **Educational Value**: Players learn about real-world agriculture
- **Authenticity**: Game reflects actual environmental conditions
- **Engagement**: More realistic and challenging gameplay
- **Scientific Accuracy**: Based on real satellite observations

The system is designed to be robust, with automatic fallback to RNG data when NASA data is unavailable, ensuring your game always works regardless of network conditions or API availability.
