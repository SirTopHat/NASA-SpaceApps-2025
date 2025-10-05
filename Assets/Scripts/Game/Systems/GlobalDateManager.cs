using UnityEngine;
using System;
using System.Collections.Generic;
using NasaSpaceApps.FarmFromSpace.Common;
using NasaSpaceApps.FarmFromSpace.Game.Services;

namespace NasaSpaceApps.FarmFromSpace.Game.Systems
{
	/// <summary>
	/// Manages global date synchronization across all regions
	/// Ensures all regions use the same date for NASA data consistency
	/// </summary>
	public class GlobalDateManager : MonoBehaviour
	{
		[Header("Global Date Settings")]
		[SerializeField] private DateTime globalStartDate = new DateTime(2024, 3, 1);
		[SerializeField] private int currentGlobalWeek = 0;
		[SerializeField] private bool useRealTime = false;

	[Header("Region Data")]
	[SerializeField] private Dictionary<RegionType, DateTime> regionStartDates = new Dictionary<RegionType, DateTime>();

		// Events
		public event Action<int> OnGlobalWeekChanged;
		public event Action<DateTime> OnGlobalDateChanged;
		public event Action<RegionType, DateTime> OnRegionDateUpdated;

		// Singleton pattern for easy access
		public static GlobalDateManager Instance { get; private set; }

		private void Awake()
		{
			// Singleton setup
			if (Instance == null)
			{
				Instance = this;
				DontDestroyOnLoad(gameObject);
			}
			else
			{
				Destroy(gameObject);
				return;
			}

			InitializeGlobalDate();
		}

		private void Start()
		{
			// Initialize all regions with the global start date
			InitializeAllRegions();
		}

		private void InitializeGlobalDate()
		{
			// Set initial global date
			globalStartDate = new DateTime(2024, 3, 1); // March 1st, 2024
			currentGlobalWeek = 0;

			Debug.Log($"GlobalDateManager: Initialized with global start date: {globalStartDate:MMMM d, yyyy}");
		}

		private void InitializeAllRegions()
		{
			// Initialize all regions with the same start date
			foreach (RegionType region in Enum.GetValues(typeof(RegionType)))
			{
				regionStartDates[region] = globalStartDate;
				Debug.Log($"GlobalDateManager: Initialized {region} with start date: {globalStartDate:MMMM d, yyyy}");
			}
		}

		/// <summary>
		/// Gets the current global date
		/// </summary>
		public DateTime GetCurrentGlobalDate()
		{
			if (useRealTime)
			{
				return DateTime.Now;
			}
			else
			{
				return globalStartDate.AddDays(currentGlobalWeek * 7);
			}
		}

		/// <summary>
		/// Gets the current global week
		/// </summary>
		public int GetCurrentGlobalWeek()
		{
			return currentGlobalWeek;
		}

		/// <summary>
		/// Gets the start date for a specific region
		/// </summary>
		public DateTime GetRegionStartDate(RegionType region)
		{
			if (regionStartDates.ContainsKey(region))
			{
				return regionStartDates[region];
			}
			return globalStartDate;
		}

		/// <summary>
		/// Advances the global week by one
		/// </summary>
		public void AdvanceGlobalWeek()
		{
			currentGlobalWeek++;
			Debug.Log($"GlobalDateManager: Advanced to week {currentGlobalWeek}, date: {GetCurrentGlobalDate():MMMM d, yyyy}");

			// Notify all regions of the date change
			OnGlobalWeekChanged?.Invoke(currentGlobalWeek);
			OnGlobalDateChanged?.Invoke(GetCurrentGlobalDate());

			// Update all region data services with the new date
			UpdateAllRegionsWithNewDate();
		}

		/// <summary>
		/// Sets the global week to a specific value
		/// </summary>
		public void SetGlobalWeek(int week)
		{
			currentGlobalWeek = week;
			Debug.Log($"GlobalDateManager: Set to week {currentGlobalWeek}, date: {GetCurrentGlobalDate():MMMM d, yyyy}");

			// Notify all regions of the date change
			OnGlobalWeekChanged?.Invoke(currentGlobalWeek);
			OnGlobalDateChanged?.Invoke(GetCurrentGlobalDate());

			// Update all region data services with the new date
			UpdateAllRegionsWithNewDate();
		}

		/// <summary>
		/// Updates all regions with the current global date
		/// </summary>
		private void UpdateAllRegionsWithNewDate()
		{
			DateTime currentDate = GetCurrentGlobalDate();
			
			// Update all regions to use the same date
			foreach (RegionType region in Enum.GetValues(typeof(RegionType)))
			{
				regionStartDates[region] = currentDate;
				OnRegionDateUpdated?.Invoke(region, currentDate);
				
				Debug.Log($"GlobalDateManager: Updated {region} to use date: {currentDate:MMMM d, yyyy}");
			}
		}

		/// <summary>
		/// Gets the date for a specific week in the current global timeline
		/// </summary>
		public DateTime GetDateForWeek(int weekIndex)
		{
			return globalStartDate.AddDays(weekIndex * 7);
		}

		/// <summary>
		/// Gets the week index for a specific date
		/// </summary>
		public int GetWeekIndexForDate(DateTime date)
		{
			return (int)((date - globalStartDate).TotalDays / 7);
		}

		/// <summary>
		/// Synchronizes all regions to use the same date
		/// </summary>
		public void SynchronizeAllRegions()
		{
			DateTime syncDate = GetCurrentGlobalDate();
			
			foreach (RegionType region in Enum.GetValues(typeof(RegionType)))
			{
				regionStartDates[region] = syncDate;
				Debug.Log($"GlobalDateManager: Synchronized {region} to {syncDate:MMMM d, yyyy}");
			}
		}

		/// <summary>
		/// Gets information about the current global state
		/// </summary>
		public GlobalDateInfo GetGlobalDateInfo()
		{
			return new GlobalDateInfo
			{
				GlobalStartDate = globalStartDate,
				CurrentGlobalWeek = currentGlobalWeek,
				CurrentGlobalDate = GetCurrentGlobalDate(),
				UseRealTime = useRealTime,
				RegionCount = regionStartDates.Count
			};
		}

		// Public properties for external access
		public DateTime GlobalStartDate => globalStartDate;
		public int CurrentGlobalWeek => currentGlobalWeek;
		public bool UseRealTime => useRealTime;
	}

	/// <summary>
	/// Information about the current global date state
	/// </summary>
	[System.Serializable]
	public class GlobalDateInfo
	{
		public DateTime GlobalStartDate;
		public int CurrentGlobalWeek;
		public DateTime CurrentGlobalDate;
		public bool UseRealTime;
		public int RegionCount;
	}
}
