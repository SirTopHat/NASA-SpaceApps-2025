using UnityEngine;
using System;
using NasaSpaceApps.FarmFromSpace.Common;
using NasaSpaceApps.FarmFromSpace.Game.Data;
using NasaSpaceApps.FarmFromSpace.Config;

namespace NasaSpaceApps.FarmFromSpace.Game.Services
{
	/// <summary>
	/// Enhanced DataService that can use either NASA satellite data or RNG fallback
	/// Provides a unified interface for the game systems
	/// </summary>
	public class EnhancedDataService : IDataService
	{
		private readonly NasaDataService nasaDataService;
		private readonly DataService fallbackService;
		private readonly bool useNasaData;
		private readonly RegionType regionType;
		private readonly DateTime startDate;

		public EnhancedDataService(RegionType regionType, NasaDataConfig config = null)
		{
			this.regionType = regionType;
			this.startDate = config?.startDate ?? new DateTime(2024, 3, 1);
			
			// Determine if we should use NASA data
			useNasaData = config?.ShouldUseRealData() ?? false;
			
			// Initialize fallback service
			fallbackService = new DataService(regionType, 12345);
			
			// Initialize NASA data service if using real data
			if (useNasaData && config != null)
			{
				try
				{
					// Create NASA data service directly
					nasaDataService = new NasaDataService(regionType, startDate, true);
					
					Debug.Log($"EnhancedDataService: NASA data integration initialized for {regionType} starting {startDate:MMMM d, yyyy}");
				}
				catch (Exception e)
				{
					Debug.LogError($"EnhancedDataService: Failed to initialize NASA data service: {e.Message}");
					Debug.Log("EnhancedDataService: Falling back to RNG data");
					nasaDataService = null;
				}
			}
			else
			{
				Debug.Log("EnhancedDataService: Using fallback RNG data (NASA data disabled or config missing)");
			}
		}

		public WeeklyEnvironment GetWeeklyEnvironment(int weekIndex)
		{
			if (useNasaData && nasaDataService != null)
			{
				try
				{
					return nasaDataService.GetWeeklyEnvironment(weekIndex);
				}
				catch (Exception e)
				{
					Debug.LogWarning($"EnhancedDataService: NASA data unavailable for week {weekIndex}, using fallback: {e.Message}");
					return fallbackService.GetWeeklyEnvironment(weekIndex);
				}
			}
			else
			{
				return fallbackService.GetWeeklyEnvironment(weekIndex);
			}
		}

	public bool IsUsingRealData => useNasaData && nasaDataService != null;
	public bool IsDataAvailable => nasaDataService != null;
	
	// Additional properties for debugging and status
	public RegionType RegionType => regionType;
	public DateTime StartDate => startDate;
	public string DataSource => IsUsingRealData ? "NASA Satellite Data" : "RNG Fallback";

	/// <summary>
	/// Clears all cached data to force fresh data retrieval
	/// </summary>
	public void ClearCache()
	{
		if (nasaDataService != null)
		{
			nasaDataService.ClearCache();
			Debug.Log($"EnhancedDataService: Cache cleared for {regionType}");
		}
	}

	/// <summary>
	/// Updates the region for NASA data service (if using real data)
	/// </summary>
	public void UpdateRegion(RegionType newRegion)
	{
		if (nasaDataService != null)
		{
			// Clear cache first
			ClearCache();
			
			// Update the NASA data service with new region
			nasaDataService.UpdateRegion(newRegion);
			
			Debug.Log($"EnhancedDataService: Region updated to {newRegion}");
		}
	}

	/// <summary>
	/// Preloads data for a range of weeks to improve performance
	/// </summary>
	public void PreloadData(int startWeek, int endWeek)
	{
		if (nasaDataService != null)
		{
			nasaDataService.PreloadData(startWeek, endWeek);
		}
	}

	/// <summary>
	/// Checks if data is available for a specific week
	/// </summary>
	public bool IsDataAvailableForWeek(int weekIndex)
	{
		if (nasaDataService != null)
		{
			return nasaDataService.IsDataAvailable(weekIndex);
		}
		return false;
	}

        public static implicit operator EnhancedDataService(DataService v) {
            throw new NotImplementedException();
        }
    }
}
