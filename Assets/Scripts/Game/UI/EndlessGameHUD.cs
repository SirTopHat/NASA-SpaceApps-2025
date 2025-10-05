using UnityEngine;
using TMPro;
using NasaSpaceApps.FarmFromSpace.Game.Systems;
using NasaSpaceApps.FarmFromSpace.Game.Data;
using NasaSpaceApps.FarmFromSpace.Game;
using NasaSpaceApps.FarmFromSpace.Common;

namespace NasaSpaceApps.FarmFromSpace.Game.UI
{
	public class EndlessGameHUD : MonoBehaviour
	{
		[Header("HUD Elements")]
		[SerializeField] private TextMeshProUGUI weekInfoText;
        [SerializeField] private TextMeshProUGUI weekPhaseText;
        [SerializeField] private TextMeshProUGUI environmentText;
		[SerializeField] private TextMeshProUGUI statusText;
		[SerializeField] private TextMeshProUGUI helpText;
		[SerializeField] private TextMeshProUGUI selectedPlotText;
		[SerializeField] private TextMeshProUGUI goldText;
		[SerializeField] private TextMeshProUGUI regionText;

		[Header("Week Display")]
		[SerializeField] private WeekDisplay weekDisplay;

		[Header("NASA Data Card")]
		[SerializeField] private EndlessNasaDataCard nasaDataCard;

		[Header("Action Buttons")]
		[SerializeField] private UnityEngine.UI.Button irrigateButton;
		[SerializeField] private UnityEngine.UI.Button plantButton;
		[SerializeField] private UnityEngine.UI.Button nitrogenPartAButton;
		[SerializeField] private UnityEngine.UI.Button nitrogenPartBButton;
		// [SerializeField] private UnityEngine.UI.Button buyPlotButton; // Removed - plots are now bought by clicking on them
		[SerializeField] private UnityEngine.UI.Button harvestButton;
		[SerializeField] private UnityEngine.UI.Button skipButton;
		[SerializeField] private UnityEngine.UI.Button resolveButton;

		[Header("Crop Picker")]
		[SerializeField] private EndlessCropPicker cropPicker;

		[Header("Plot Selection")]
		[SerializeField] private UnityEngine.UI.Button[] plotButtons;

		private EndlessGameRunner runner;
		private EndlessTurnManager turnManager;
		private GlobalGameState globalState;

		private void Start()
		{
			runner = FindFirstObjectByType<EndlessGameRunner>();
			if (runner == null)
			{
				Debug.LogError("EndlessGameHUD: No EndlessGameRunner found in scene!");
				return;
			}

			SetupButtons();
			SetupWeekDisplay();
		}

		private void SetupButtons()
		{
			// Action buttons
			if (irrigateButton != null)
				irrigateButton.onClick.AddListener(() => runner.GetTurnManager()?.QueueIrrigation());
			
			if (plantButton != null)
				plantButton.onClick.AddListener(() => ShowCropPicker());
			
			if (nitrogenPartAButton != null)
				nitrogenPartAButton.onClick.AddListener(() => runner.GetTurnManager()?.ApplyNitrogenPartA());
			
			if (nitrogenPartBButton != null)
				nitrogenPartBButton.onClick.AddListener(() => runner.GetTurnManager()?.ApplyNitrogenPartB());
			
			// Buy plot button removed - plots are now bought by clicking on them directly
			
			if (harvestButton != null)
				harvestButton.onClick.AddListener(() => runner.GetTurnManager()?.HarvestSelectedPlot());
			
			if (skipButton != null)
				skipButton.onClick.AddListener(() => { /* Skip action - no cost */ });
			
			if (resolveButton != null)
				resolveButton.onClick.AddListener(() => HandleResolveButton());

			// Plot selection buttons
			if (plotButtons != null)
			{
				for (int i = 0; i < plotButtons.Length; i++)
				{
					int plotIndex = i; // Capture for closure
					if (plotButtons[i] != null)
						plotButtons[i].onClick.AddListener(() => runner.GetTurnManager()?.SelectPlot(plotIndex));
				}
			}
		}

		private void SetupWeekDisplay()
		{
			// Setup WeekDisplay component
			if (weekDisplay != null)
			{
				// WeekDisplay will auto-find the EndlessTurnManager
				//Debug.Log("EndlessGameHUD: WeekDisplay component found and configured");
			}
			else
			{
				// Try to find WeekDisplay component on this GameObject
				weekDisplay = GetComponent<WeekDisplay>();
				if (weekDisplay == null)
				{
					//Debug.LogWarning("EndlessGameHUD: No WeekDisplay component found. Add one to display week information.");
				}
			}
		}

		private void Update()
		{
			if (runner == null) return;

			turnManager = runner.GetTurnManager();
			globalState = runner.GetGlobalState();
			
			if (turnManager == null || globalState == null) return;

			UpdateHUD();
			UpdateWeekDisplay();
			UpdateButtonStates();
		}

		private void UpdateHUD()
		{
			// Week info (now global week)
			if (weekInfoText != null)
			{
                //weekInfoText.text = $"Week {globalState.globalWeek + 1} - Phase: {turnManager.Phase}";
                weekInfoText.text = $"{globalState.globalWeek + 1}";
                weekPhaseText.text = $"Phase: {turnManager.Phase}";
            }

			// Environment data
			if (environmentText != null)
			{
				if (nasaDataCard == null)
				{
					environmentText.text = $"Rain: {turnManager.Env.rainMm:F1} mm | ET: {turnManager.Env.etMm:F1} mm | NDVI Phase: {turnManager.Env.ndviPhase}";
				}
				else
				{
					environmentText.text = "";
				}
			}

			// Region info
			if (regionText != null)
			{
				string regionName = GetRegionDisplayName();
				regionText.text = $"Region: {regionName}";
			}

			// Gold display (universal gold)
			if (goldText != null)
			{
				goldText.text = $"Gold: {turnManager.Gold}";
			}

			// Status (plot-specific info)
			if (statusText != null)
			{
				var selectedPlot = GetSelectedPlot();
				if (selectedPlot != null)
				{
					string cropInfo = selectedPlot.plantedWeek >= 0 ? 
						$"Crop: {selectedPlot.plantedCropName} (Age: {selectedPlot.cropAgeWeeks}w)" : 
						"Empty Plot";
					
					string maturityInfo = selectedPlot.plantedWeek >= 0 ? 
						$" | Progress: {selectedPlot.yieldAccumulated:F1}/{selectedPlot.maturityTarget:F1} GP" : "";
					
					string suitabilityInfo = selectedPlot.plantedWeek >= 0 ? 
						$" | Suitability: {selectedPlot.suitability}" : "";
					
					statusText.text = $"{cropInfo}{maturityInfo}{suitabilityInfo}";
				}
				else
				{
					statusText.text = "No plot selected";
				}
			}

			// Help text
			if (helpText != null)
			{
				string help = turnManager.Phase switch
				{
					EndlessTurnPhase.Planning => $"Select plot → Plant crop → Manage water/N → Space: Resolve",
					EndlessTurnPhase.End => "Press Space: Next Week",
					_ => "Press Space: Next Week"
				};
				helpText.text = help;
			}

			// Selected plot info
			if (selectedPlotText != null)
			{
				var selectedPlot = GetSelectedPlot();
				if (selectedPlot != null)
				{
					// Use preview soil fraction during Planning so irrigation updates are visible immediately
					float soilFraction = turnManager.Phase == EndlessTurnPhase.Planning
						? turnManager.GetPreviewSoilFraction(turnManager.SelectedPlotIndex)
						: selectedPlot.soilTank;
					string soilInfo = $"Soil: {(soilFraction * 100f):F0}%";
					string cropInfo = selectedPlot.plantedWeek >= 0 ? 
						$" | {selectedPlot.plantedCropName} (Week {selectedPlot.cropAgeWeeks})" : 
						" | Empty";
					string yieldInfo = selectedPlot.plantedWeek >= 0 ? 
						$" | Yield: {selectedPlot.yieldAccumulated:F1}/{selectedPlot.maturityTarget:F1} GP" : "";
					string harvestInfo = selectedPlot.isReadyForHarvest ? " | READY FOR HARVEST!" : "";
					
					selectedPlotText.text = $"{soilInfo}{cropInfo}{yieldInfo}{harvestInfo}";
				}
				else
				{
					selectedPlotText.text = "No plot selected";
				}
			}
		}

		private void UpdateButtonStates()
		{
			var selectedPlot = GetSelectedPlot();
			bool hasSelectedPlot = selectedPlot != null;
			bool plotPlanted = hasSelectedPlot && selectedPlot.plantedWeek >= 0;
			bool plotEmpty = hasSelectedPlot && selectedPlot.plantedWeek < 0;
			bool plotReadyForHarvest = hasSelectedPlot && selectedPlot.isReadyForHarvest;

			// Action button states
			bool canIrrigate = turnManager.Phase == EndlessTurnPhase.Planning && 
				turnManager.Gold >= GetIrrigationCost() && hasSelectedPlot;
			
			bool canPlant = turnManager.Phase == EndlessTurnPhase.Planning && plotEmpty;
			
			bool canHarvest = turnManager.Phase == EndlessTurnPhase.Planning && plotReadyForHarvest;
			
			// Nitrogen button states
			bool canApplyN = turnManager.Phase == EndlessTurnPhase.Planning && 
				turnManager.Gold >= GetNitrogenCost() && plotPlanted;
			
			bool canApplyPartA = canApplyN && selectedPlot != null && selectedPlot.nPartAWeek < 0;
			bool canApplyPartB = canApplyN && selectedPlot != null && selectedPlot.nPartBWeek < 0;
			
			// Buy plot button removed - plots are now bought by clicking on them directly

			// Update button interactability
			if (irrigateButton != null)
				irrigateButton.interactable = canIrrigate;
			
			if (plantButton != null)
				plantButton.interactable = canPlant;
			
			if (harvestButton != null)
				harvestButton.interactable = canHarvest;
			
			if (nitrogenPartAButton != null)
				nitrogenPartAButton.interactable = canApplyPartA;
			
			if (nitrogenPartBButton != null)
				nitrogenPartBButton.interactable = canApplyPartB;
			
			// Buy plot button removed - plots are now bought by clicking on them directly
			
			if (skipButton != null)
				skipButton.interactable = turnManager.Phase == EndlessTurnPhase.Planning;
			
			if (resolveButton != null)
			{
				bool canResolve = turnManager.Phase == EndlessTurnPhase.Planning || 
					turnManager.Phase == EndlessTurnPhase.End;
				resolveButton.interactable = canResolve;
				
				// Update button text to show what it will do
				var buttonText = resolveButton.GetComponentInChildren<TextMeshProUGUI>();
				if (buttonText != null)
				{
					buttonText.text = turnManager.Phase == EndlessTurnPhase.Planning ? 
						"Resolve Turn" : "Next Week";
				}
			}
		}

		private PlotState GetSelectedPlot()
		{
			if (turnManager == null || turnManager.SelectedPlotIndex >= turnManager.Plots.Count)
				return null;
			
			return turnManager.Plots[turnManager.SelectedPlotIndex];
		}

		private string GetRegionDisplayName()
		{
			// This would need to be passed from the EndlessGameRunner
			// For now, return a default based on the current region
			return "Sibu (Monsoon)"; // Default, should be dynamic
		}

		private void ShowCropPicker()
		{
			if (cropPicker != null)
			{
				// Show the crop picker UI
				cropPicker.ShowCropPicker();
			}
			else
			{
				Debug.LogWarning("Crop picker not assigned to EndlessGameHUD");
			}
		}

		private void HandleResolveButton()
		{
			if (turnManager == null) return;

			if (turnManager.Phase == EndlessTurnPhase.Planning)
			{
				// Resolve the current turn
				turnManager.EndPlanningAndResolve();
				Debug.Log("Turn resolved");
                SoundManager.Instance.PlaySfx("click");
            }
			else if (turnManager.Phase == EndlessTurnPhase.End)
			{
				// Advance to next week
				turnManager.NextWeek();
				Debug.Log("Advanced to next week");
                SoundManager.Instance.PlaySfx("click");
            }
		}

		private int GetIrrigationCost()
		{
			return turnManager?.IrrigationCost ?? 5;
		}

		private int GetNitrogenCost()
		{
			return turnManager?.NitrogenPartACost ?? 8;
		}
		
		public void RefreshUI()
		{
			// Force refresh of all UI elements
			UpdateHUD();
			UpdateButtonStates();
		}

		private void UpdateWeekDisplay()
		{
			// Update WeekDisplay with current turn manager
			if (weekDisplay != null && turnManager != null)
			{
				weekDisplay.SetEndlessTurnManager(turnManager);
			}
		}

		public void OnRegionChanged(RegionType newRegion)
		{
			Debug.Log($"EndlessGameHUD: Region changed to {newRegion}");
			
			// Update region display
			if (regionText != null)
			{
				regionText.text = $"Region: {newRegion}";
			}

			// Refresh all UI elements
			RefreshUI();
		}
	}
}
