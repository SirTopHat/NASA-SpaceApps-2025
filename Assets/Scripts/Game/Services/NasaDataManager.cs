using UnityEngine;
using System;
using System.Collections;
using NasaSpaceApps.FarmFromSpace.Common;
using NasaSpaceApps.FarmFromSpace.Game.Data;
using NasaSpaceApps.FarmFromSpace.Config;

namespace NasaSpaceApps.FarmFromSpace.Game.Services
{
	/// <summary>
	/// Manages NASA data integration and provides a unified interface for the game
	/// Handles data fetching, caching, and fallback mechanisms
	/// </summary>
	public class NasaDataManager : MonoBehaviour
	{
		[Header("Configuration")]
		[SerializeField] private NasaDataConfig nasaDataConfig;
		[SerializeField] private RegionType regionType;
		[SerializeField] private DateTime gameStartDate;

		[Header("Data Services")]
		[SerializeField] private NasaDataService nasaDataService;
		[SerializeField] private DataService fallbackDataService;

		[Header("Status")]
		[SerializeField] private bool isInitialized = false;
		[SerializeField] private bool isDataAvailable = false;
		[SerializeField] private int cachedWeeksCount = 0;

		// Events
		public event Action<WeeklyEnvironment> OnDataUpdated;
		public event Action<string> OnDataError;
		public event Action<bool> OnDataAvailabilityChanged;

		private void Awake()
		{
			InitializeDataServices();
		}

		private void Start()
		{
			if (nasaDataConfig != null)
			{
				StartCoroutine(InitializeDataAsync());
			}
			else
			{
				Debug.LogError("NasaDataManager: NASA Data Config is not assigned!");
				InitializeFallback();
			}
		}

		private void InitializeDataServices()
		{
			// Initialize NASA data service
			nasaDataService = new NasaDataService(
				regionType, 
				gameStartDate, 
				nasaDataConfig?.ShouldUseRealData() ?? false
			);

			// Initialize fallback service
			fallbackDataService = new DataService(regionType, 12345);
		}

		private IEnumerator InitializeDataAsync()
		{
			Debug.Log("NasaDataManager: Initializing NASA data services...");
			
			// Validate configuration
			nasaDataConfig.ValidateSettings();
			
			if (!nasaDataConfig.IsValidConfiguration())
			{
				Debug.LogWarning("NasaDataManager: Invalid configuration, using fallback data");
				InitializeFallback();
				yield break;
			}

			// Preload initial data
			yield return StartCoroutine(PreloadInitialData());
			
			isInitialized = true;
			Debug.Log("NasaDataManager: NASA data services initialized successfully");
		}

		private IEnumerator PreloadInitialData()
		{
			// Preload data for the first 10 weeks
			int preloadWeeks = Mathf.Min(10, nasaDataConfig.maxCacheWeeks);
			
			for (int week = 0; week < preloadWeeks; week++)
			{
				bool success = PreloadWeekData(week);
				
				// Yield every few weeks to prevent blocking
				if (week % 3 == 0)
				{
					yield return new WaitForSeconds(0.1f);
				}
			}
			
			isDataAvailable = cachedWeeksCount > 0;
			OnDataAvailabilityChanged?.Invoke(isDataAvailable);
		}

		private bool PreloadWeekData(int week)
		{
			try
			{
				var env = GetWeeklyEnvironment(week);
				cachedWeeksCount++;
				return true;
			}
			catch (Exception e)
			{
				Debug.LogError($"NasaDataManager: Failed to preload data for week {week}: {e.Message}");
				OnDataError?.Invoke($"Failed to load data for week {week}");
				return false;
			}
		}

		private void InitializeFallback()
		{
			Debug.Log("NasaDataManager: Using fallback RNG data service");
			isDataAvailable = true;
			isInitialized = true;
			OnDataAvailabilityChanged?.Invoke(true);
		}

		public WeeklyEnvironment GetWeeklyEnvironment(int weekIndex)
		{
			if (!isInitialized)
			{
				Debug.LogWarning("NasaDataManager: Not initialized, returning fallback data");
				return fallbackDataService.GetWeeklyEnvironment(weekIndex);
			}

			try
			{
				WeeklyEnvironment env;
				
				if (nasaDataConfig.ShouldUseRealData() && nasaDataService != null)
				{
					env = nasaDataService.GetWeeklyEnvironment(weekIndex);
					
					// Validate data quality
					if (nasaDataConfig.enableDataValidation && !ValidateDataQuality(env))
					{
						Debug.LogWarning($"NasaDataManager: Poor data quality for week {weekIndex}, using fallback");
						env = fallbackDataService.GetWeeklyEnvironment(weekIndex);
					}
				}
				else
				{
					env = fallbackDataService.GetWeeklyEnvironment(weekIndex);
				}

				OnDataUpdated?.Invoke(env);
				return env;
			}
			catch (Exception e)
			{
				Debug.LogError($"NasaDataManager: Error getting data for week {weekIndex}: {e.Message}");
				OnDataError?.Invoke($"Data error for week {weekIndex}: {e.Message}");
				
				// Return fallback data
				return fallbackDataService.GetWeeklyEnvironment(weekIndex);
			}
		}

		private bool ValidateDataQuality(WeeklyEnvironment env)
		{
			// Check for reasonable data ranges
			if (env.rainMm < 0 || env.rainMm > 100) return false;
			if (env.etMm < 0 || env.etMm > 50) return false;
			
			// Additional quality checks can be added here
			return true;
		}

		public void RefreshData(int weekIndex)
		{
			if (nasaDataService != null)
			{
				StartCoroutine(RefreshDataAsync(weekIndex));
			}
		}

		private IEnumerator RefreshDataAsync(int weekIndex)
		{
			Debug.Log($"NasaDataManager: Refreshing data for week {weekIndex}");
			
			try
			{
				// Clear cached data for this week
				if (nasaDataService != null)
				{
					// The NasaDataService would need a method to clear specific week cache
					// For now, we'll just re-fetch
					var env = nasaDataService.GetWeeklyEnvironment(weekIndex);
					OnDataUpdated?.Invoke(env);
				}
			}
			catch (Exception e)
			{
				Debug.LogError($"NasaDataManager: Failed to refresh data for week {weekIndex}: {e.Message}");
				OnDataError?.Invoke($"Refresh failed for week {weekIndex}");
			}
			
			yield return null;
		}

		public void ClearCache()
		{
			if (nasaDataService != null)
			{
				nasaDataService.ClearCache();
				cachedWeeksCount = 0;
				Debug.Log("NasaDataManager: Cache cleared");
			}
		}

		public void PreloadDataRange(int startWeek, int endWeek)
		{
			if (nasaDataService != null)
			{
				StartCoroutine(PreloadDataRangeAsync(startWeek, endWeek));
			}
		}

		private IEnumerator PreloadDataRangeAsync(int startWeek, int endWeek)
		{
			Debug.Log($"NasaDataManager: Preloading data for weeks {startWeek}-{endWeek}");
			
			for (int week = startWeek; week <= endWeek; week++)
			{
				bool success = PreloadWeekData(week);
				
				// Yield every few weeks
				if (week % 5 == 0)
				{
					yield return new WaitForSeconds(0.1f);
				}
			}
		}

		// Public properties
		public bool IsInitialized => isInitialized;
		public bool IsDataAvailable => isDataAvailable;
		public int CachedWeeksCount => cachedWeeksCount;
		public bool IsUsingRealData => nasaDataConfig?.ShouldUseRealData() ?? false;

		// Debug methods
		[ContextMenu("Test Data Fetch")]
		public void TestDataFetch()
		{
			if (isInitialized)
			{
				var env = GetWeeklyEnvironment(0);
				Debug.Log($"Test data fetch - Rain: {env.rainMm:F1}mm, ET: {env.etMm:F1}mm, NDVI: {env.ndviPhase}");
			}
		}

		[ContextMenu("Clear All Cache")]
		public void ClearAllCache()
		{
			ClearCache();
		}
	}
}
