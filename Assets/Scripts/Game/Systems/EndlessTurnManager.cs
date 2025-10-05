using UnityEngine;
using NasaSpaceApps.FarmFromSpace.Common;
using NasaSpaceApps.FarmFromSpace.Config;
using NasaSpaceApps.FarmFromSpace.Game.Data;
using NasaSpaceApps.FarmFromSpace.Game.Services;
using NasaSpaceApps.FarmFromSpace.Game.Utils;
using NasaSpaceApps.FarmFromSpace.Content;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NasaSpaceApps.FarmFromSpace.Game.Systems
{
	public enum EndlessTurnPhase
	{
		Planning,
		Resolving,
		End
	}

	public class EndlessTurnManager
	{
		public WeeklyState State { get; private set; }
		public WeeklyEnvironment Env { get; private set; }
		public EndlessTurnPhase Phase { get; private set; }

		private readonly IDataService dataService;
		private readonly BalanceConfig balance;
		private readonly SoilTexture soilTexture;
		private readonly RegionType regionType;
		private readonly GlobalGameState globalState;
		private readonly RegionGameState regionState;
		private readonly DateTime gameStartDate;

		public List<PlotState> Plots => regionState.plots;
		public int SelectedPlotIndex { get; private set; }
		public int Gold => globalState.universalGold;
		public int IrrigationCost => balance.irrigationCost;
		public int NitrogenPartACost => balance.nitrogenPartACost;
		public int NitrogenPartBCost => balance.nitrogenPartBCost;
		
		// Plot purchasing
		public int MaxPlots => 9;
		public int GetNextPlotCost()
		{
			int plotCount = Plots.Count - 2; // Subtract initial 2 plots
			return Mathf.RoundToInt(balance.basePlotCost * Mathf.Pow(balance.plotCostMultiplier, plotCount));
		}

		public RegionType GetCurrentRegion()
		{
			return regionType;
		}

		// Date-related properties
		public DateTime CurrentDate => DateUtils.GetDateForTurn(gameStartDate, State.weekIndex);
		public string CurrentWeekFormatted => DateUtils.GetFormattedWeek(gameStartDate, State.weekIndex);
		public string CurrentDateRange => DateUtils.GetFormattedDateRange(gameStartDate, State.weekIndex);
		public string CurrentSeason => DateUtils.GetSeason(CurrentDate);
		public int CurrentWeekInMonth => DateUtils.GetWeekInMonth(CurrentDate);

		private float[] pendingIrrigationMm; // Array to store irrigation for each plot

		public EndlessTurnManager(IDataService dataService, BalanceConfig balance, SoilTexture soilTexture, 
			RegionType regionType, GlobalGameState globalState, DateTime? startDate = null)
		{
			this.dataService = dataService;
			this.balance = balance;
			this.soilTexture = soilTexture;
			this.regionType = regionType;
			this.globalState = globalState;
			this.regionState = globalState.GetRegionState(regionType);
			this.gameStartDate = startDate ?? new DateTime(2024, 3, 1); // Default to March 1, 2024

			Phase = EndlessTurnPhase.Planning;
			State = new WeeklyState { soilTank = 0.6f, runoffEventsThisSeason = 0, yieldAccumulated = 0f, weekIndex = globalState.globalWeek };
			Env = dataService.GetWeeklyEnvironment(State.weekIndex);
			
			// Ensure all unlocked plots exist in the Plots list
			InitializeUnlockedPlots();
		}

		/// <summary>
		/// Returns the preview soil fraction for a plot during Planning, including any pending irrigation queued this turn.
		/// During other phases, returns the current plot soilTank.
		/// </summary>
		public float GetPreviewSoilFraction(int plotIndex)
		{
			if (plotIndex < 0 || plotIndex >= Plots.Count) return 0f;
			var plot = Plots[plotIndex];
			if (plot == null) return 0f;
			if (Phase != EndlessTurnPhase.Planning)
			{
				return plot.soilTank;
			}

			float fieldCapacity = GetFieldCapacity();
			float soilWaterMm = plot.soilTank * fieldCapacity * 100f;
			float pending = 0f;
			if (pendingIrrigationMm != null && plotIndex < pendingIrrigationMm.Length)
			{
				pending = Mathf.Max(0f, pendingIrrigationMm[plotIndex]);
			}
			float newSoilMm = Mathf.Clamp(soilWaterMm + pending, 0f, fieldCapacity * 100f);
			float fraction = (fieldCapacity <= 0f) ? plot.soilTank : newSoilMm / (fieldCapacity * 100f);
			return fraction;
		}

		private void InitializeUnlockedPlots()
		{
			// Create plots for all unlocked plot indices
			for (int i = 0; i < 9; i++)
			{
				if (regionState.IsPlotUnlocked(i))
				{
					EnsurePlotExists(i);
					//Debug.Log($"Initialized unlocked plot {i}");
				}
			}
			//Debug.Log($"Total plots in Plots list: {Plots.Count}");
		}

		public void SelectPlot(int index)
		{
			if (index >= 0 && index < 9 && regionState.IsPlotUnlocked(index))
			{
				SelectedPlotIndex = index;
			}
		}

		public void PlantSelectedPlot(CropDefinition crop)
		{
			if (Phase != EndlessTurnPhase.Planning) return;
			var plot = Plots[SelectedPlotIndex];
			if (plot.plantedWeek >= 0) return; // Already planted
			
			int cost = GetCropCost(crop);
			if (globalState.universalGold < cost) return;
			
			// Calculate suitability
			CropSuitability suitability = CalculateSuitability(crop, regionType);
			
			plot.plantedCrop = crop;
			plot.plantedCropName = crop.displayName;
			plot.plantedWeek = globalState.globalWeek;
			plot.maturityTarget = crop.maturityTarget;
			plot.isReadyForHarvest = false;
			plot.cropAgeWeeks = 0;
			plot.suitability = suitability;
			plot.yieldAccumulated = 0f; // Reset yield for new crop
			
			Plots[SelectedPlotIndex] = plot;
			globalState.universalGold -= cost;
		}

		private int GetCropCost(CropDefinition crop)
		{
			if (crop == null) return 15; // Default fallback
			return crop.plantingCost;
		}

		private CropSuitability CalculateSuitability(CropDefinition crop, RegionType region)
		{
			int suitabilityValue = 0;
			switch (region)
			{
				case RegionType.TropicalMonsoon:
					suitabilityValue = crop.monsoonSuitability;
					break;
				case RegionType.SemiAridSteppe:
					suitabilityValue = crop.steppeSuitability;
					break;
				case RegionType.TemperateContinental:
					suitabilityValue = crop.temperateSuitability;
					break;
			}
			
			return (CropSuitability)suitabilityValue;
		}

		public void HarvestSelectedPlot()
		{
			if (Phase != EndlessTurnPhase.Planning) return;
			var plot = Plots[SelectedPlotIndex];
			if (!plot.isReadyForHarvest) return;
			
			// Convert yield to gold (1 Growth Point = 1 Gold)
			int harvestGold = Mathf.RoundToInt(plot.yieldAccumulated);
			globalState.universalGold += harvestGold;
			
			// Reset plot for replanting
			plot.plantedCrop = null;
			plot.plantedCropName = "";
			plot.plantedWeek = -1;
			plot.yieldAccumulated = 0f;
			plot.isReadyForHarvest = false;
			plot.cropAgeWeeks = 0;
			plot.nPartsApplied = 0;
			plot.splitNApplied = false;
			plot.nPartAWeek = -1;
			plot.nPartBWeek = -1;
			plot.leachingPenaltyNextWeek = 0;

            SoundManager.Instance.PlaySfx("harvest");

            Plots[SelectedPlotIndex] = plot;
			Debug.Log($"Harvested {plot.plantedCropName}: {harvestGold} Gold");
		}

		public void QueueIrrigation()
		{
			if (Phase != EndlessTurnPhase.Planning) return;
			var plot = Plots[SelectedPlotIndex];
			if (globalState.universalGold < IrrigationCost) return;
			
			// Initialize irrigation array if needed
			if (pendingIrrigationMm == null || pendingIrrigationMm.Length != Plots.Count)
			{
				pendingIrrigationMm = new float[Plots.Count];
			}
			
			// Apply diminishing returns
			float efficiency = CalculateIrrigationEfficiency(plot.irrigationsThisWeek);
			float irrigationAmount = 10f * efficiency; // 10mm base irrigation
			
			// Add irrigation to the selected plot (accumulate if irrigated multiple times)
			pendingIrrigationMm[SelectedPlotIndex] += irrigationAmount;
			
			// Update plot irrigation tracking
			plot.irrigationsThisWeek++;
			plot.totalIrrigationMm += irrigationAmount;
			Plots[SelectedPlotIndex] = plot;
			
			globalState.universalGold -= IrrigationCost;
			
			// Play watering sound effect when irrigation is queued
			if (global::SoundManager.Instance != null)
				global::SoundManager.Instance.PlaySfx("watering");
			
			Debug.Log($"Irrigated Plot {SelectedPlotIndex}: +{irrigationAmount:F1}mm (efficiency: {efficiency:P0}). Total pending: {pendingIrrigationMm[SelectedPlotIndex]:F1}mm");
		}

		private float CalculateIrrigationEfficiency(int irrigationCount)
		{
			// Diminishing returns: 1st = 100%, 2nd = 70%, 3rd+ = 40%
			if (irrigationCount == 0) return 1.0f;
			if (irrigationCount == 1) return balance.secondIrrigationEfficiency;
			return balance.thirdPlusIrrigationEfficiency;
		}

		public void ApplyNitrogenPartA()
		{
			if (Phase != EndlessTurnPhase.Planning) return;
			var plot = Plots[SelectedPlotIndex];
			if (plot.plantedWeek < 0 || plot.nPartsApplied >= 2) return;
			if (globalState.universalGold < NitrogenPartACost) return;
			
			plot.nPartsApplied++;
			plot.nPartAWeek = globalState.globalWeek;
			Plots[SelectedPlotIndex] = plot;
			globalState.universalGold -= NitrogenPartACost;

			SoundManager.Instance.PlaySfx("fertilizer");
		}

		public void ApplyNitrogenPartB()
		{
			if (Phase != EndlessTurnPhase.Planning) return;
			var plot = Plots[SelectedPlotIndex];
			if (plot.plantedWeek < 0 || plot.nPartsApplied >= 2) return;
			if (globalState.universalGold < NitrogenPartBCost) return;
			
			plot.nPartsApplied++;
			plot.nPartBWeek = globalState.globalWeek;
			Plots[SelectedPlotIndex] = plot;
			globalState.universalGold -= NitrogenPartBCost;
            SoundManager.Instance.PlaySfx("fertilizer");
        }

		public void BuyNewPlot()
		{
			if (Phase != EndlessTurnPhase.Planning) return;
			if (Plots.Count >= MaxPlots) return;
			
			int cost = GetNextPlotCost();
			if (globalState.universalGold < cost) return;
			
			var newPlot = new PlotState
			{
				id = Plots.Count,
				soilTexture = soilTexture,
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
			};
			
			Plots.Add(newPlot);
			globalState.universalGold -= cost;
			Debug.Log($"Bought new plot for {cost} Gold. Remaining Gold: {globalState.universalGold}");
		}

		public bool CanUnlockPlot(int plotIndex)
		{
			if (Phase != EndlessTurnPhase.Planning) return false;
			if (plotIndex < 0 || plotIndex >= 9) return false;
			if (regionState.IsPlotUnlocked(plotIndex)) return false; // Already unlocked
			
			int cost = GetPlotUnlockCost(plotIndex);
			return globalState.universalGold >= cost;
		}

		public bool UnlockPlot(int plotIndex)
		{
			if (!CanUnlockPlot(plotIndex)) return false;
			
			int cost = GetPlotUnlockCost(plotIndex);
			globalState.universalGold -= cost;
			
			// Unlock the plot
			regionState.UnlockPlot(plotIndex);
			
			// Always ensure the plot exists in the Plots list
			EnsurePlotExists(plotIndex);
			
			Debug.Log($"Unlocked plot {plotIndex} for {cost} Gold. Remaining Gold: {globalState.universalGold}");
			return true;
		}

		private void EnsurePlotExists(int plotIndex)
		{
			// Ensure we have enough plots in the list
			while (Plots.Count <= plotIndex)
			{
				Plots.Add(null);
			}
			
			// Create the plot if it doesn't exist yet
			if (Plots[plotIndex] == null)
			{
				var newPlot = new PlotState
				{
					id = plotIndex,
					soilTexture = soilTexture,
					soilTank = 0.6f,
					plantedWeek = -1, // Empty plot
					plantedCrop = null, // No crop planted
					plantedCropName = "", // No crop name
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
				};
				
				Plots[plotIndex] = newPlot;
				//Debug.Log($"Created new empty plot {plotIndex} (plantedWeek: {newPlot.plantedWeek}, plantedCrop: {newPlot.plantedCrop})");
			}
			else
			{
				//Debug.Log($"Plot {plotIndex} already exists in Plots list");
			}
		}

		public int GetPlotUnlockCost(int plotIndex)
		{
			if (plotIndex < 0 || plotIndex >= 9) return 0;
			
			// Calculate cost based on how many plots are already unlocked
			int unlockedCount = regionState.GetUnlockedPlotCount();
			return Mathf.RoundToInt(balance.basePlotCost * Mathf.Pow(balance.plotCostMultiplier, unlockedCount - 2));
		}

		public bool IsPlotUnlocked(int plotIndex)
		{
			return regionState.IsPlotUnlocked(plotIndex);
		}

		public void EndPlanningAndResolve()
		{
			if (Phase != EndlessTurnPhase.Planning) return;
			Phase = EndlessTurnPhase.Resolving;
			ResolveTurn();
		}

		public void NextWeek()
		{
			if (Phase != EndlessTurnPhase.End) return;
			
			// Apply leaching penalties from previous week
			ApplyLeachingPenalties();
			
			// Reset irrigation tracking for all plots
			ResetIrrigationTracking();
			
			// Advance global week
			globalState.globalWeek++;
			State = new WeeklyState
			{
				soilTank = State.soilTank,
				runoffEventsThisSeason = State.runoffEventsThisSeason,
				yieldAccumulated = State.yieldAccumulated,
				weekIndex = globalState.globalWeek
			};

			Env = dataService.GetWeeklyEnvironment(State.weekIndex);
			// Clear irrigation array for new turn
			if (pendingIrrigationMm != null)
			{
				for (int i = 0; i < pendingIrrigationMm.Length; i++)
				{
					pendingIrrigationMm[i] = 0f;
				}
			}
			Phase = EndlessTurnPhase.Planning;
		}

		private void ApplyLeachingPenalties()
		{
			int totalPenalty = 0;
			for (int i = 0; i < Plots.Count; i++)
			{
				var plot = Plots[i];
				if (plot.leachingPenaltyNextWeek > 0)
				{
					totalPenalty += plot.leachingPenaltyNextWeek;
					plot.leachingPenaltyNextWeek = 0; // Reset after applying
					Plots[i] = plot;
				}
			}
			
			if (totalPenalty > 0)
			{
				globalState.universalGold -= totalPenalty;
				Debug.Log($"Leaching penalties applied: -{totalPenalty} Gold");
			}
		}

		private void ResetIrrigationTracking()
		{
			for (int i = 0; i < Plots.Count; i++)
			{
				var plot = Plots[i];
				plot.irrigationsThisWeek = 0;
				plot.totalIrrigationMm = 0f;
				Plots[i] = plot;
			}
		}

		private void ResolveTurn()
		{
			float fieldCapacity = GetFieldCapacity();

			for (int i = 0; i < Plots.Count; i++)
			{
				var plot = Plots[i];
				
				// Calculate irrigation for this specific plot
				float plotIrrigation = 0f;
				if (pendingIrrigationMm != null && i < pendingIrrigationMm.Length && pendingIrrigationMm[i] > 0f)
				{
					plotIrrigation = pendingIrrigationMm[i];
					// Note: irrigation tracking is now done in QueueIrrigation()
				}
				
				float waterIn = Env.rainMm + plotIrrigation;
				float waterOut = Env.etMm;

				float soilWaterMm = plot.soilTank * fieldCapacity * 100f;
				float newSoilMm = Mathf.Clamp(soilWaterMm + waterIn - waterOut, 0f, fieldCapacity * 100f);

				bool runoff = false;
				float preAddFraction = soilWaterMm / (fieldCapacity * 100f + Mathf.Epsilon);
				if (preAddFraction >= balance.runoffThreshold && (waterIn - waterOut) > 0f)
				{
					runoff = true;
					globalState.universalGold -= balance.runoffPenalty;
				}

				float soilFraction = (fieldCapacity <= 0f) ? 0f : newSoilMm / (fieldCapacity * 100f);
				float stageMultiplier = GetStageMultiplier(Env.ndviPhase);
				
				// Improved water adequacy calculation - less restrictive
				float optimalWater = plot.plantedCrop?.optimalSoilWater ?? 0.7f;
				float waterAdequacy = Mathf.Clamp01(1f - Mathf.Abs(soilFraction - optimalWater) * 2f);
				
				float weeklyGrowth = 0f;
				if (plot.plantedWeek >= 0 && plot.plantedCrop != null)
				{
					// Age the crop
					plot.cropAgeWeeks++;
					
					// Check for crop failure (Poor suitability)
					if (plot.suitability == CropSuitability.Poor && plot.cropAgeWeeks > plot.plantedCrop.maxAgeWeeks)
					{
						// Crop fails - refund 50% of planting cost
						int refund = Mathf.RoundToInt(plot.plantedCrop.plantingCost * 0.5f);
						globalState.universalGold += refund;
						Debug.Log($"Crop failed due to poor suitability. Refunded {refund} Gold.");
						
						// Reset plot
						plot.plantedCrop = null;
						plot.plantedCropName = "";
						plot.plantedWeek = -1;
						plot.yieldAccumulated = 0f;
						plot.isReadyForHarvest = false;
						plot.cropAgeWeeks = 0;
					}
					else
					{
						// Apply suitability modifiers
						float suitabilityMultiplier = GetSuitabilityMultiplier(plot.suitability);
						
						// Improved growth calculation - more generous water adequacy
						weeklyGrowth = plot.plantedCrop.baseWeeklyGrowth * stageMultiplier * (0.7f + 0.3f * waterAdequacy) * suitabilityMultiplier;
						
						// Apply split-N timing bonus
						float splitNBonus = CalculateSplitNBonus(plot);
						weeklyGrowth *= (1f + splitNBonus);
						
						plot.yieldAccumulated += weeklyGrowth;
						
						// Check for maturity
						if (plot.yieldAccumulated >= plot.maturityTarget)
						{
							plot.isReadyForHarvest = true;
						}
					}
				}

				plot.soilTank = soilFraction;
				plot.runoffEvents += (runoff ? 1 : 0);
				Plots[i] = plot;
			}

			Phase = EndlessTurnPhase.End;
			
			// Clear irrigation array for next turn
			if (pendingIrrigationMm != null)
			{
				for (int i = 0; i < pendingIrrigationMm.Length; i++)
				{
					pendingIrrigationMm[i] = 0f;
				}
			}
		}

		private float GetSuitabilityMultiplier(CropSuitability suitability)
		{
			switch (suitability)
			{
				case CropSuitability.Good:
					return 1.0f;
				case CropSuitability.Marginal:
					return 0.8f; // -20% growth rate
				case CropSuitability.Poor:
					return 0.6f; // -40% growth rate, plus failure risk
				default:
					return 1.0f;
			}
		}

		private float GetFieldCapacity()
		{
			return balance.fieldCapacityBySoilTexture[(int)soilTexture];
		}

		private float GetStageMultiplier(NdviPhase phase)
		{
			return balance.stageMultipliers[(int)phase];
		}

		private float CalculateSplitNBonus(PlotState plot)
		{
			if (!plot.splitNApplied || plot.nPartAWeek == plot.nPartBWeek) return 0f;
			
			// Check if both applications were during active stages
			bool partADuringActive = IsActiveStage(plot.nPartAWeek);
			bool partBDuringActive = IsActiveStage(plot.nPartBWeek);
			
			if (partADuringActive && partBDuringActive)
			{
				return balance.splitNitrogenBonus;
			}
			
			return 0f;
		}

		private bool IsActiveStage(int week)
		{
			// Simplified: assume Green-up and Peak are active stages
			var phase = dataService.GetWeeklyEnvironment(week).ndviPhase;
			return phase == NdviPhase.GreenUp || phase == NdviPhase.Peak;
		}
	}
}
