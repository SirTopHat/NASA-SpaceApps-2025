using System.Collections.Generic;
using UnityEngine;
using NasaSpaceApps.FarmFromSpace.Common;

namespace NasaSpaceApps.FarmFromSpace.Game.Data
{
	[System.Serializable]
	public class GlobalGameState
	{
		[Header("Global State")]
		public int globalWeek = 0;
		public int globalGold = 60; // Deprecated - use universalGold
		public List<RegionType> unlockedRegions = new List<RegionType> { RegionType.TropicalMonsoon }; // Start with one region
		
		[Header("Region States")]
		public List<RegionStateData> regionStatesData = new List<RegionStateData>();
		
		// Helper dictionary for runtime access (not serialized)
		private Dictionary<RegionType, RegionGameState> regionStates = new Dictionary<RegionType, RegionGameState>();
		
		[Header("Universal Gold")]
		[Tooltip("Single gold pool shared across all regions")]
		public int universalGold = 60;
		
		[Header("Unlock Costs")]
		public int region2UnlockCost = 250;
		public int region3UnlockCost = 500;
		
		public GlobalGameState()
		{
			// Initialize with default region
			InitializeRegionStates();
		}
		
		private void InitializeRegionStates()
		{
			regionStates[RegionType.TropicalMonsoon] = new RegionGameState();
			SyncRegionStatesData();
		}
		
		public void SyncRegionStatesData()
		{
			// Convert dictionary to serializable list
			regionStatesData.Clear();
			foreach (var kvp in regionStates)
			{
				regionStatesData.Add(new RegionStateData
				{
					regionType = kvp.Key,
					regionState = kvp.Value
				});
			}
		}
		
		private void LoadRegionStatesFromData()
		{
			// Convert serializable list back to dictionary
			regionStates.Clear();
			foreach (var data in regionStatesData)
			{
				regionStates[data.regionType] = data.regionState;
			}
		}
		
		public bool IsRegionUnlocked(RegionType region)
		{
			return unlockedRegions.Contains(region);
		}
		
		public bool CanUnlockRegion(RegionType region)
		{
			if (IsRegionUnlocked(region)) return false;
			
			int cost = GetRegionUnlockCost(region);
			return universalGold >= cost;
		}
		
		public bool UnlockRegion(RegionType region)
		{
			if (!CanUnlockRegion(region)) return false;
			
			int cost = GetRegionUnlockCost(region);
			universalGold -= cost;
			unlockedRegions.Add(region);
			
			// Initialize region state if not exists
			if (!regionStates.ContainsKey(region))
			{
				regionStates[region] = new RegionGameState();
			}
			
			return true;
		}
		
		public int GetRegionUnlockCost(RegionType region)
		{
			switch (region)
			{
				case RegionType.TemperateContinental:
					return region2UnlockCost;
				case RegionType.SemiAridSteppe:
					return region3UnlockCost;
				default:
					return 0; // Already unlocked or invalid
			}
		}
		
		public RegionGameState GetRegionState(RegionType region)
		{
			// Load from serialized data if dictionary is empty
			if (regionStates.Count == 0 && regionStatesData.Count > 0)
			{
				LoadRegionStatesFromData();
			}
			
			if (!regionStates.ContainsKey(region))
			{
				regionStates[region] = new RegionGameState();
				SyncRegionStatesData(); // Sync after adding new region
			}
			
			// Ensure the region state is properly initialized
			regionStates[region].EnsureInitialized();
			return regionStates[region];
		}
	}
	
	[System.Serializable]
	public class RegionStateData
	{
		public RegionType regionType;
		public RegionGameState regionState;
	}
	
	[System.Serializable]
	public class RegionGameState
	{
		[Header("Region State")]
		public int lastNdviUpdateWeek = -1;
		public List<PlotState> plots = new List<PlotState>();
		
		[Header("Plot Unlocking")]
		public List<bool> unlockedPlots = new List<bool>(); // Track which of the 9 plots are unlocked
		
		[Header("Pending Penalties")]
		public List<PendingPenalty> pendingPenalties = new List<PendingPenalty>();
		
		public RegionGameState()
		{
			// Only initialize if this is a new region state
			InitializeNewRegion();
		}

		private void InitializeNewRegion()
		{
			// Initialize with 2 empty plots
			for (int i = 0; i < 2; i++)
			{
				plots.Add(new PlotState
				{
					id = i,
					soilTexture = SoilTexture.Loam,
					soilTank = 0.6f,
					plantedWeek = -1,
					yieldAccumulated = 0f,
					runoffEvents = 0,
					nPartsApplied = 0,
					splitNApplied = false,
					nPartAWeek = -1,
					nPartBWeek = -1,
					leachingPenaltyNextWeek = 0,
					irrigationsThisWeek = 0,
					totalIrrigationMm = 0f,
					maturityTarget = 100f,
					isReadyForHarvest = false,
					cropAgeWeeks = 0,
					suitability = CropSuitability.Good
				});
			}
			
			// Initialize unlocked plots (first 2 are unlocked by default)
			unlockedPlots = new List<bool>();
			for (int i = 0; i < 9; i++)
			{
				unlockedPlots.Add(i < 2); // First 2 plots are unlocked
			}
		}

		public void EnsureInitialized()
		{
			// Ensure unlockedPlots is initialized if it's empty (for loaded saves)
			if (unlockedPlots == null || unlockedPlots.Count == 0)
			{
				unlockedPlots = new List<bool>();
				for (int i = 0; i < 9; i++)
				{
					unlockedPlots.Add(i < 2); // First 2 plots are unlocked
				}
			}

			// Ensure plots list is initialized if it's empty
			if (plots == null || plots.Count == 0)
			{
				InitializeNewRegion();
			}
		}
		
		public bool IsPlotUnlocked(int plotIndex)
		{
			if (plotIndex < 0 || plotIndex >= unlockedPlots.Count) return false;
			return unlockedPlots[plotIndex];
		}
		
		public bool UnlockPlot(int plotIndex)
		{
			if (plotIndex < 0 || plotIndex >= unlockedPlots.Count) return false;
			if (unlockedPlots[plotIndex]) return false; // Already unlocked
			
			unlockedPlots[plotIndex] = true;
			return true;
		}
		
		public int GetUnlockedPlotCount()
		{
			int count = 0;
			foreach (bool unlocked in unlockedPlots)
			{
				if (unlocked) count++;
			}
			return count;
		}
	}
	
	[System.Serializable]
	public class PendingPenalty
	{
		public int plotId;
		public int penaltyAmount;
		public int weeksRemaining;
	}
}
