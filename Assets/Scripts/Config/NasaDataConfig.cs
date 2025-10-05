using UnityEngine;
using System;

namespace NasaSpaceApps.FarmFromSpace.Config
{
	[CreateAssetMenu(fileName = "NasaDataConfig", menuName = "FarmFromSpace/NASA Data Config", order = 0)]
	public class NasaDataConfig : ScriptableObject
	{
		[Header("Data Source Settings")]
		[Tooltip("Use real NASA satellite data instead of RNG simulation")]
		public bool useRealNasaData = true;
		
		[Tooltip("Enable data caching for better performance")]
		public bool enableDataCaching = true;
		
		[Tooltip("Maximum number of weeks to cache in memory")]
		[Range(10, 100)] public int maxCacheWeeks = 50;

		[Header("Game Start Date")]
		[Tooltip("Starting date for the farming season (affects which NASA data is used)")]
		public DateTime startDate = new DateTime(2024, 3, 1); // March 1st, 2024

		[Header("Data Update Settings")]
		[Tooltip("How often to refresh data from NASA APIs (in hours)")]
		[Range(1, 168)] public int dataRefreshIntervalHours = 24;
		
		[Tooltip("Enable automatic data updates during gameplay")]
		public bool enableAutoUpdates = false;

		[Header("Fallback Settings")]
		[Tooltip("Use fallback RNG data when NASA APIs are unavailable")]
		public bool enableFallbackData = true;
		
		[Tooltip("Show warnings when fallback data is used")]
		public bool showFallbackWarnings = true;

		[Header("API Settings")]
		[Tooltip("NASA Earthdata username (required for data access)")]
		public string earthdataUsername = "";
		
		[Tooltip("NASA Earthdata password (required for data access)")]
		public string earthdataPassword = "";
		
		[Tooltip("Enable debug logging for data requests")]
		public bool enableDebugLogging = false;

		[Header("Data Quality Settings")]
		[Tooltip("Minimum data quality threshold (0-1)")]
		[Range(0f, 1f)] public float minDataQuality = 0.7f;
		
		[Tooltip("Enable data validation checks")]
		public bool enableDataValidation = true;

		[Header("Performance Settings")]
		[Tooltip("Maximum concurrent API requests")]
		[Range(1, 10)] public int maxConcurrentRequests = 3;
		
		[Tooltip("Request timeout in seconds")]
		[Range(5, 60)] public int requestTimeoutSeconds = 30;

		// Validation methods
		public bool IsValidConfiguration()
		{
			if (useRealNasaData)
			{
				if (string.IsNullOrEmpty(earthdataUsername) || string.IsNullOrEmpty(earthdataPassword))
				{
					Debug.LogWarning("NASA Data Config: Earthdata credentials required for real data access");
					return false;
				}
			}
			
			return true;
		}

		public void ValidateSettings()
		{
			if (startDate < new DateTime(2000, 1, 1))
			{
				Debug.LogWarning("NASA Data Config: Start date is too early, some data may not be available");
			}
			
			if (startDate > DateTime.Now)
			{
				Debug.LogWarning("NASA Data Config: Start date is in the future, using forecast data");
			}
			
			if (maxCacheWeeks < 10)
			{
				Debug.LogWarning("NASA Data Config: Cache size is very small, may impact performance");
			}
		}

		// Helper methods
		public DateTime GetWeekStartDate(int weekIndex)
		{
			return startDate.AddDays(weekIndex * 7);
		}

		public int GetWeekIndex(DateTime date)
		{
			return (int)((date - startDate).TotalDays / 7);
		}

		public bool ShouldUseRealData()
		{
			return useRealNasaData && IsValidConfiguration();
		}
	}
}
