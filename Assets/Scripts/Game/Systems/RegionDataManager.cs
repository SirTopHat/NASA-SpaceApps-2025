using UnityEngine;
using System;
using System.Collections.Generic;
using NasaSpaceApps.FarmFromSpace.Common;
using NasaSpaceApps.FarmFromSpace.Game.Data;
using NasaSpaceApps.FarmFromSpace.Game.Services;
using NasaSpaceApps.FarmFromSpace.Config;
using NasaSpaceApps.FarmFromSpace.Content;

namespace NasaSpaceApps.FarmFromSpace.Game.Systems
{
	/// <summary>
	/// Manages region-specific data and switching between regions
	/// Handles NASA data, crops, and environmental conditions for each region
	/// </summary>
	public class RegionDataManager
	{
		private readonly Dictionary<RegionType, RegionData> regionData;
		private readonly NasaDataConfig nasaDataConfig;
		private RegionType currentRegion;

		public RegionDataManager(NasaDataConfig config)
		{
			nasaDataConfig = config;
			regionData = new Dictionary<RegionType, RegionData>();
			currentRegion = RegionType.TropicalMonsoon; // Default region

			InitializeRegions();
		}

		private void InitializeRegions()
		{
			// Initialize data for all available regions
			foreach (RegionType region in Enum.GetValues(typeof(RegionType)))
			{
				regionData[region] = new RegionData
				{
					regionType = region,
					dataService = new EnhancedDataService(region, nasaDataConfig),
					plots = new List<PlotState>(),
					unlockedPlots = new HashSet<int> { 0, 1 }, // Start with 2 plots unlocked
					regionSpecificCrops = GetRegionSpecificCrops(region)
				};

				Debug.Log($"RegionDataManager: Initialized {region} with {regionData[region].dataService.DataSource}");
			}
		}

		public RegionData GetRegionData(RegionType region)
		{
			if (regionData.ContainsKey(region))
			{
				return regionData[region];
			}

			Debug.LogError($"RegionDataManager: Region {region} not found!");
			return null;
		}

		public RegionData GetCurrentRegionData()
		{
			return GetRegionData(currentRegion);
		}

	public void SwitchRegion(RegionType newRegion)
	{
		if (newRegion == currentRegion)
		{
			Debug.Log($"RegionDataManager: Already in region {newRegion}");
			return;
		}

		Debug.Log($"RegionDataManager: Switching from {currentRegion} to {newRegion}");

		// Save current region state if needed
		// (This could include saving plot states, etc.)

		// Switch to new region
		currentRegion = newRegion;

		// Update NASA data for the new region
		UpdateNasaDataForRegion(newRegion);

		// Log region-specific information
		var newRegionData = GetRegionData(newRegion);
		if (newRegionData != null)
		{
			Debug.Log($"RegionDataManager: Switched to {newRegion}");
			Debug.Log($"RegionDataManager: Data Source: {newRegionData.dataService.DataSource}");
			Debug.Log($"RegionDataManager: Available Crops: {newRegionData.regionSpecificCrops.Count}");
		}
	}

	/// <summary>
	/// Updates NASA data for the specified region, clearing cache and refreshing data
	/// </summary>
	private void UpdateNasaDataForRegion(RegionType region)
	{
		var regionData = GetRegionData(region);
		if (regionData?.dataService != null)
		{
			// Clear any cached data to force refresh
			regionData.dataService.ClearCache();
			
			// Update the data service with new region coordinates
			regionData.dataService.UpdateRegion(region);
			
			Debug.Log($"RegionDataManager: NASA data updated for {region}");
		}
	}

		public RegionType GetCurrentRegion()
		{
			return currentRegion;
		}

		public WeeklyEnvironment GetWeeklyEnvironment(int weekIndex)
		{
			var currentRegionData = GetCurrentRegionData();
			if (currentRegionData != null)
			{
				return currentRegionData.dataService.GetWeeklyEnvironment(weekIndex);
			}

			Debug.LogError("RegionDataManager: No current region data available!");
			return new WeeklyEnvironment(); // Return empty environment as fallback
		}

		public List<CropDefinition> GetAvailableCrops()
		{
			var currentRegionData = GetCurrentRegionData();
			if (currentRegionData != null)
			{
				return currentRegionData.regionSpecificCrops;
			}

			return new List<CropDefinition>();
		}

		public bool IsPlotUnlocked(int plotIndex)
		{
			var currentRegionData = GetCurrentRegionData();
			if (currentRegionData != null)
			{
				return currentRegionData.unlockedPlots.Contains(plotIndex);
			}

			return false;
		}

		public void UnlockPlot(int plotIndex)
		{
			var currentRegionData = GetCurrentRegionData();
			if (currentRegionData != null)
			{
				currentRegionData.unlockedPlots.Add(plotIndex);
				Debug.Log($"RegionDataManager: Unlocked plot {plotIndex} in {currentRegion}");
			}
		}

		private List<CropDefinition> GetRegionSpecificCrops(RegionType region)
		{
			// This would typically load from ScriptableObjects or configuration
			// For now, return a basic list - you can expand this based on your crop system
			var crops = new List<CropDefinition>();

			switch (region)
			{
				case RegionType.TropicalMonsoon:
					// Rice, Sugarcane, Bananas, etc.
					// crops.Add(LoadCropDefinition("Rice"));
					// crops.Add(LoadCropDefinition("Sugarcane"));
					break;

				case RegionType.SemiAridSteppe:
					// Wheat, Barley, Sorghum, etc.
					// crops.Add(LoadCropDefinition("Wheat"));
					// crops.Add(LoadCropDefinition("Barley"));
					break;

				case RegionType.TemperateContinental:
					// Corn, Soybeans, Wheat, etc.
					// crops.Add(LoadCropDefinition("Corn"));
					// crops.Add(LoadCropDefinition("Soybeans"));
					break;
			}

			Debug.Log($"RegionDataManager: Loaded {crops.Count} crops for {region}");
			return crops;
		}

		// Public properties for external access
		public Dictionary<RegionType, RegionData> AllRegionData => regionData;
		public int TotalRegions => regionData.Count;
		public bool IsUsingRealData => GetCurrentRegionData()?.dataService.IsUsingRealData ?? false;
		public string CurrentDataSource => GetCurrentRegionData()?.dataService.DataSource ?? "Unknown";
	}

	/// <summary>
	/// Data structure for region-specific information
	/// </summary>
	[System.Serializable]
	public class RegionData
	{
		public RegionType regionType;
		public EnhancedDataService dataService;
		public List<PlotState> plots;
		public HashSet<int> unlockedPlots;
		public List<CropDefinition> regionSpecificCrops;
		public DateTime lastSwitchTime;
		public int totalSwitches;

		public RegionData()
		{
			plots = new List<PlotState>();
			unlockedPlots = new HashSet<int>();
			regionSpecificCrops = new List<CropDefinition>();
			lastSwitchTime = DateTime.Now;
			totalSwitches = 0;
		}
	}
}
