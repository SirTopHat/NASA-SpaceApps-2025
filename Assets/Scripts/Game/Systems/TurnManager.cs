using UnityEngine;
using NasaSpaceApps.FarmFromSpace.Common;
using NasaSpaceApps.FarmFromSpace.Config;
using NasaSpaceApps.FarmFromSpace.Game.Data;
using NasaSpaceApps.FarmFromSpace.Game.Services;
using NasaSpaceApps.FarmFromSpace.Game.Utils;
using System;
using System.Collections.Generic;

namespace NasaSpaceApps.FarmFromSpace.Game.Systems
{
	public enum TurnPhase
	{
		Planning,
		Resolving,
		End,
		Harvest
	}

	public class TurnManager
	{
		public WeeklyState State { get; private set; }
		public WeeklyEnvironment Env { get; private set; }
		public TurnPhase Phase { get; private set; }

		private readonly IDataService dataService;
		private readonly BalanceConfig balance;
		private readonly SoilTexture soilTexture;
		private readonly int totalWeeks;
		private readonly DateTime gameStartDate;

		public List<PlotState> Plots { get; private set; }
		public int SelectedPlotIndex { get; private set; }
		public int Gold { get; private set; }
		public int StartingGold { get; private set; }
		public int IrrigationCost => balance.irrigationCost;
		public int NitrogenPartACost => balance.nitrogenPartACost;
		public int NitrogenPartBCost => balance.nitrogenPartBCost;
		
		// Harvest and scoring
		public int HarvestGold { get; private set; }
		public int FinalScore { get; private set; }
		public bool IsGameComplete => Phase == TurnPhase.Harvest;

		// Date-related properties
		public DateTime CurrentDate => DateUtils.GetDateForTurn(gameStartDate, State.weekIndex);
		public string CurrentWeekFormatted => DateUtils.GetFormattedWeek(gameStartDate, State.weekIndex);
		public string CurrentDateRange => DateUtils.GetFormattedDateRange(gameStartDate, State.weekIndex);
		public string CurrentSeason => DateUtils.GetSeason(CurrentDate);
		public int CurrentWeekInMonth => DateUtils.GetWeekInMonth(CurrentDate);

		private float pendingIrrigationMm;
		private bool irrigatedLastWeek;

		public TurnManager(IDataService dataService, BalanceConfig balance, SoilTexture soilTexture, int totalWeeks = 20, int plots = 2, DateTime? startDate = null)
		{
			this.dataService = dataService;
			this.balance = balance;
			this.soilTexture = soilTexture;
			this.totalWeeks = Mathf.Max(1, totalWeeks);
			this.gameStartDate = startDate ?? new DateTime(2024, 3, 1); // Default to March 1, 2024

			Phase = TurnPhase.Planning;
			State = new WeeklyState { soilTank = 0.6f, runoffEventsThisSeason = 0, yieldAccumulated = 0f, weekIndex = 0 };
			Env = dataService.GetWeeklyEnvironment(State.weekIndex);
			StartingGold = balance.startingGold;
            Gold = StartingGold;

			Plots = new List<PlotState>(plots);
			for (int i = 0; i < plots; i++)
			{
				Plots.Add(new PlotState
				{
					id = i,
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
					totalIrrigationMm = 0f
				});
			}
			SelectedPlotIndex = 0;
		}

		public void QueueIrrigation(float mm = 20f)
		{
			if (Phase != TurnPhase.Planning) return;
			int cost = balance.irrigationCost;
			if (Gold < cost) return;
			
			var plot = Plots[SelectedPlotIndex];
			
			// Check for runoff penalty before applying irrigation
			float soilFraction = plot.soilTank;
			bool willCauseRunoff = soilFraction >= balance.runoffThreshold;
			
			if (willCauseRunoff)
			{
				// Apply runoff penalty immediately
				Gold -= balance.runoffPenalty;
				Debug.Log($"Runoff penalty: -{balance.runoffPenalty} Gold (soil at {soilFraction:P0})");
			}
			
			// Calculate diminishing returns based on irrigation count this week
			float efficiency = CalculateIrrigationEfficiency(plot.irrigationsThisWeek);
			float effectiveMm = mm * efficiency;
			
			// Update plot irrigation tracking
			plot.irrigationsThisWeek++;
			plot.totalIrrigationMm += effectiveMm;
			Plots[SelectedPlotIndex] = plot;
			
			// Apply irrigation to selected plot
			pendingIrrigationMm += effectiveMm;
			Gold -= cost;
			
			Debug.Log($"Irrigated +{effectiveMm:F1}mm (efficiency: {efficiency:P0}) for {cost} Gold. Plot irrigations this week: {plot.irrigationsThisWeek}");
		}

		private float CalculateIrrigationEfficiency(int irrigationCount)
		{
			// Diminishing returns: 1st = 100%, 2nd = 70%, 3rd+ = 40%
			if (irrigationCount == 0) return 1.0f; // First irrigation
			if (irrigationCount == 1) return balance.secondIrrigationEfficiency; // Second irrigation
			return balance.thirdPlusIrrigationEfficiency; // Third and beyond
		}

		public void SelectPlot(int index)
		{
			if (index >= 0 && index < Plots.Count)
			{
				SelectedPlotIndex = index;
			}
		}

		public void PlantSelectedPlot(Content.CropDefinition crop)
		{
			if (Phase != TurnPhase.Planning) return;
			var plot = Plots[SelectedPlotIndex];
			if (plot.plantedWeek >= 0) return;
			
			// Get crop cost based on crop type
			int cost = GetCropCost(crop);
			if (Gold < cost) return;
			
			plot.plantedCrop = crop;
			plot.plantedCropName = crop != null ? crop.displayName : "Maize";
			plot.plantedWeek = State.weekIndex;
			Plots[SelectedPlotIndex] = plot;
			Gold -= cost;
		}

		private int GetCropCost(Content.CropDefinition crop)
		{
			if (crop == null) return 15; // Default fallback
			return crop.plantingCost;
		}

		public void ApplyNitrogenPartA()
		{
			if (Phase != TurnPhase.Planning) return;
			int cost = balance.nitrogenPartACost;
			if (Gold < cost) return;
			
			var plot = Plots[SelectedPlotIndex];
			if (plot.plantedWeek < 0) return; // Can't apply N to unplanted plot
			if (plot.nPartAWeek >= 0) return; // Part A already applied
			
			// Check for leaching penalty
			bool willCauseLeaching = plot.soilTank >= balance.runoffThreshold;
			if (willCauseLeaching)
			{
				// Schedule leaching penalty for next week
				plot.leachingPenaltyNextWeek += balance.goldLeachingPenalty;
				Debug.Log($"Leaching penalty scheduled: -{balance.goldLeachingPenalty} Gold next week (soil at {plot.soilTank:P0})");
			}
			
			plot.nPartsApplied++;
			plot.nPartAWeek = State.weekIndex;
			Plots[SelectedPlotIndex] = plot;
			Gold -= cost;
		}

		public void ApplyNitrogenPartB()
		{
			if (Phase != TurnPhase.Planning) return;
			int cost = balance.nitrogenPartBCost;
			if (Gold < cost) return;
			
			var plot = Plots[SelectedPlotIndex];
			if (plot.plantedWeek < 0) return; // Can't apply N to unplanted plot
			if (plot.nPartBWeek >= 0) return; // Part B already applied
			
			// Check for leaching penalty
			bool willCauseLeaching = plot.soilTank >= balance.runoffThreshold;
			if (willCauseLeaching)
			{
				// Schedule leaching penalty for next week
				plot.leachingPenaltyNextWeek += balance.goldLeachingPenalty;
				Debug.Log($"Leaching penalty scheduled: -{balance.goldLeachingPenalty} Gold next week (soil at {plot.soilTank:P0})");
			}
			
			plot.nPartsApplied++;
			plot.nPartBWeek = State.weekIndex;
			Plots[SelectedPlotIndex] = plot;
			Gold -= cost;
		}

		public void BuyNewPlot()
		{
			if (Phase != TurnPhase.Planning) return;
			
			// Calculate escalating cost using balance config
			int plotCount = Plots.Count - 2; // Subtract initial 2 plots
			int cost = Mathf.RoundToInt(balance.basePlotCost * Mathf.Pow(balance.plotCostMultiplier, plotCount));
			
			if (Gold < cost) return;
			
			Plots.Add(new PlotState
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
				totalIrrigationMm = 0f
			});
			Gold -= cost;
			Debug.Log($"Bought new plot for {cost} Gold. Remaining Gold: {Gold}");
		}

		public int GetNextPlotCost()
		{
			int plotCount = Plots.Count - 2; // Subtract initial 2 plots
			return Mathf.RoundToInt(balance.basePlotCost * Mathf.Pow(balance.plotCostMultiplier, plotCount));
		}

		public void EndPlanningAndResolve()
		{
			if (Phase != TurnPhase.Planning) return;
			Phase = TurnPhase.Resolving;

			ResolveTurn();
		}

		public void NextWeek()
		{
			if (Phase != TurnPhase.End) return;
			
			// Apply leaching penalties from previous week
			ApplyLeachingPenalties();
			
			// Reset irrigation tracking for all plots
			ResetIrrigationTracking();
			
			State = new WeeklyState
			{
				soilTank = State.soilTank,
				runoffEventsThisSeason = State.runoffEventsThisSeason,
				yieldAccumulated = State.yieldAccumulated,
				weekIndex = State.weekIndex + 1
			};

			if (State.weekIndex >= totalWeeks)
			{
				// Season complete - trigger harvest
				TriggerHarvest();
				return;
			}

			Env = dataService.GetWeeklyEnvironment(State.weekIndex);
			pendingIrrigationMm = 0f;
			Phase = TurnPhase.Planning;
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
				Gold -= totalPenalty;
				Debug.Log($"Leaching penalties applied: -{totalPenalty} Gold");
			}
		}

		private void TriggerHarvest()
		{
			Phase = TurnPhase.Harvest;
			
			// Calculate harvest gold from all plots
			HarvestGold = 0;
			for (int i = 0; i < Plots.Count; i++)
			{
				var plot = Plots[i];
				if (plot.plantedWeek >= 0 && plot.yieldAccumulated > 0f)
				{
					// Convert yield to gold (1 Growth Point = 1 Gold)
					int plotHarvestGold = Mathf.RoundToInt(plot.yieldAccumulated);
					HarvestGold += plotHarvestGold;
					Debug.Log($"Plot {i + 1} ({plot.plantedCropName}): {plot.yieldAccumulated:F1} yield → {plotHarvestGold} Gold");
				}
			}
			
			// Calculate final score: Harvest Gold + Unspent Gold
			FinalScore = HarvestGold + Gold;
			
			Debug.Log($"=== HARVEST COMPLETE ===");
			Debug.Log($"Harvest Gold: {HarvestGold}");
			Debug.Log($"Unspent Gold: {Gold}");
			Debug.Log($"Final Score: {FinalScore}");
		}

		private void ResetIrrigationTracking()
		{
			// Reset irrigation tracking for all plots at the start of each week
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
				if (i == SelectedPlotIndex && pendingIrrigationMm > 0f)
				{
					// Diminishing returns are already applied in QueueIrrigation
					plotIrrigation = pendingIrrigationMm;
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
				}

				float soilFraction = (fieldCapacity <= 0f) ? 0f : newSoilMm / (fieldCapacity * 100f);
				float stageMultiplier = GetStageMultiplier(Env.ndviPhase);
				float waterAdequacy = Mathf.Clamp01(1f - Mathf.Abs(soilFraction - 0.7f));
				float weeklyGrowth = 0f;
				if (plot.plantedWeek >= 0 && plot.plantedCrop != null)
				{
					weeklyGrowth = plot.plantedCrop.baseWeeklyGrowth * stageMultiplier * (0.5f + 0.5f * waterAdequacy);
					
					// Apply split-N timing bonus
					float splitNBonus = CalculateSplitNBonus(plot);
					weeklyGrowth *= (1f + splitNBonus);
				}

				plot.soilTank = soilFraction;
				plot.runoffEvents += (runoff ? 1 : 0);
				plot.yieldAccumulated += weeklyGrowth;
				Plots[i] = plot;
			}

			float totalYield = 0f;
			int totalRunoff = 0;
			float avgSoil = 0f;
			for (int i = 0; i < Plots.Count; i++)
			{
				totalYield += Plots[i].yieldAccumulated;
				totalRunoff += Plots[i].runoffEvents;
				avgSoil += Plots[i].soilTank;
			}
			avgSoil /= Mathf.Max(1, Plots.Count);

			State = new WeeklyState
			{
				soilTank = avgSoil,
				runoffEventsThisSeason = totalRunoff,
				yieldAccumulated = totalYield,
				weekIndex = State.weekIndex
			};

			// Track if any plot was irrigated this week
			irrigatedLastWeek = pendingIrrigationMm > 0f;
			Phase = TurnPhase.End;
		}

		private float GetFieldCapacity()
		{
			int idx = Mathf.Clamp((int)soilTexture, 0, balance.fieldCapacityBySoilTexture.Length - 1);
			return balance.fieldCapacityBySoilTexture[idx];
		}

		private float GetStageMultiplier(NdviPhase phase)
		{
			int idx = Mathf.Clamp((int)phase, 0, balance.stageMultipliers.Length - 1);
			return balance.stageMultipliers[idx];
		}

		private float CalculateSplitNBonus(PlotState plot)
		{
			// Check if both parts were applied
			if (plot.nPartAWeek < 0 || plot.nPartBWeek < 0) return 0f;
			
			// Check if they were applied in different weeks
			if (plot.nPartAWeek == plot.nPartBWeek) return 0f;
			
			// Check if both parts were applied during active growth phases
			bool partAInActivePhase = WasPhaseActive(plot.nPartAWeek);
			bool partBInActivePhase = WasPhaseActive(plot.nPartBWeek);
			
			if (partAInActivePhase && partBInActivePhase)
			{
				// Apply split-N bonus from balance config
				float baseBonus = balance.splitNitrogenBonus;
				if (plot.plantedCrop != null && plot.plantedCrop.splitNBonusOverride > 0f)
				{
					baseBonus = plot.plantedCrop.splitNBonusOverride;
				}
				return baseBonus;
			}
			
			return 0f;
		}

		private bool WasPhaseActive(int weekIndex)
		{
			// Check if the week had an active growth phase (GreenUp or Peak)
			// For now, we'll use a simple check - can be made more sophisticated
			return weekIndex >= 0 && weekIndex < totalWeeks;
		}
	}
}


