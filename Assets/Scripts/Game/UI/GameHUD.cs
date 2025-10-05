using UnityEngine;
using TMPro;
using NasaSpaceApps.FarmFromSpace.Game.Systems;
using NasaSpaceApps.FarmFromSpace.Game.Data;

namespace NasaSpaceApps.FarmFromSpace.Game.UI
{
	public class GameHUD : MonoBehaviour
	{
		[Header("HUD Elements")]
		[SerializeField] private TextMeshProUGUI weekInfoText;
		[SerializeField] private TextMeshProUGUI environmentText; // Deprecated - use nasaDataCard instead
		[SerializeField] private TextMeshProUGUI statusText;
		[SerializeField] private TextMeshProUGUI helpText;
		[SerializeField] private TextMeshProUGUI selectedPlotText;
		[SerializeField] private TextMeshProUGUI goldText;

		[Header("Week Display")]
		[SerializeField] private WeekDisplay weekDisplay;

		[Header("NASA Data Card")]
		[SerializeField] private GameUINasaDataCard nasaDataCard;

		[Header("Action Buttons")]
		[SerializeField] private UnityEngine.UI.Button irrigateButton;
		[SerializeField] private UnityEngine.UI.Button plantButton;
		[SerializeField] private UnityEngine.UI.Button nitrogenPartAButton;
		[SerializeField] private UnityEngine.UI.Button nitrogenPartBButton;
		[SerializeField] private UnityEngine.UI.Button buyPlotButton;
		[SerializeField] private UnityEngine.UI.Button skipButton;
		[SerializeField] private UnityEngine.UI.Button resolveButton;

		[Header("Crop Picker")]
		[SerializeField] private GameUICropPicker cropPicker;

		[Header("Plot Selection")]
		[SerializeField] private UnityEngine.UI.Button[] plotButtons;

		private GameRunner runner;

		private void Start()
		{
			runner = FindFirstObjectByType<GameRunner>();
			if (runner == null)
			{
				Debug.LogError("GameHUD: No GameRunner found in scene!");
				return;
			}

			SetupButtons();
			SetupWeekDisplay();
		}

		private void SetupButtons()
		{
			// Action buttons
			if (irrigateButton != null)
				irrigateButton.onClick.AddListener(() => runner.UI_Irrigate10());
			
			if (plantButton != null)
				plantButton.onClick.AddListener(() => ShowCropPicker());
			
			if (nitrogenPartAButton != null)
				nitrogenPartAButton.onClick.AddListener(() => runner.UI_ApplyNitrogenPartA());
			
			if (nitrogenPartBButton != null)
				nitrogenPartBButton.onClick.AddListener(() => runner.UI_ApplyNitrogenPartB());
			
			if (buyPlotButton != null)
				buyPlotButton.onClick.AddListener(() => runner.UI_BuyPlot());
			
			if (skipButton != null)
				skipButton.onClick.AddListener(() => { /* Skip action - no cost */ });
			
			if (resolveButton != null)
				resolveButton.onClick.AddListener(() => runner.UI_ResolveOrNext());

			// Plot selection buttons
			if (plotButtons != null)
			{
				for (int i = 0; i < plotButtons.Length; i++)
				{
					int plotIndex = i; // Capture for closure
					if (plotButtons[i] != null)
						plotButtons[i].onClick.AddListener(() => runner.UI_SelectPlot(plotIndex));
				}
			}
		}

		private void SetupWeekDisplay()
		{
			// Setup WeekDisplay component
			if (weekDisplay != null)
			{
				// WeekDisplay will auto-find the TurnManager
				Debug.Log("GameHUD: WeekDisplay component found and configured");
			}
			else
			{
				// Try to find WeekDisplay component on this GameObject
				weekDisplay = GetComponent<WeekDisplay>();
				if (weekDisplay == null)
				{
					Debug.LogWarning("GameHUD: No WeekDisplay component found. Add one to display week information.");
				}
			}
		}

		private void Update()
		{
			if (runner == null) return;

			UpdateHUD();
			UpdateButtonStates();
			UpdateWeekDisplay();
		}

		private void UpdateHUD()
		{
			var turnManager = runner.GetTurnManager();
			if (turnManager == null) return;

			// Week info
			if (weekInfoText != null)
			{
				weekInfoText.text = $"Week {turnManager.State.weekIndex + 1} / {runner.GetTotalWeeks()} - Phase: {turnManager.Phase}";
			}

			// Environment data - now handled by NASA Data Card
			if (environmentText != null)
			{
				// Keep old format as fallback if NASA Data Card is not available
				if (nasaDataCard == null)
				{
					environmentText.text = $"Rain: {turnManager.Env.rainMm:F1} mm | ET: {turnManager.Env.etMm:F1} mm | NDVI Phase: {turnManager.Env.ndviPhase}";
				}
				else
				{
					// Hide old environment text when NASA Data Card is active
					environmentText.text = "";
				}
			}

			// Status
			if (statusText != null)
			{
				statusText.text = $"Soil Tank: {(turnManager.State.soilTank * 100f):F0}% | Runoff Events: {turnManager.State.runoffEventsThisSeason} | Yield: {turnManager.State.yieldAccumulated:F2}";
			}

			// Help text
			if (helpText != null)
			{
				string help = turnManager.Phase switch
				{
					TurnPhase.Planning => $"Gold: {turnManager.Gold}  |  Press I: Irrigate +20mm (5 Gold)  |  Space: Resolve",
					TurnPhase.End => "Press Space: Next Week",
					TurnPhase.Harvest => $"HARVEST COMPLETE! Final Score: {turnManager.FinalScore}  |  Press R: Restart",
					_ => "Press Space: Next Week"
				};
				helpText.text = help;
			}

			// Gold display
			if (goldText != null)
			{
				if (turnManager.Phase == TurnPhase.Harvest)
				{
					goldText.text = $"Harvest: {turnManager.HarvestGold} | Unspent: {turnManager.Gold} | Total: {turnManager.FinalScore}";
				}
				else
				{
					goldText.text = $"Gold: {turnManager.Gold}";
				}
			}

			// Selected plot info
			if (selectedPlotText != null && turnManager.SelectedPlotIndex < turnManager.Plots.Count)
			{
				var selectedPlot = turnManager.Plots[turnManager.SelectedPlotIndex];
				selectedPlotText.text = $"Selected: {selectedPlot.plantedCropName} (Week {selectedPlot.plantedWeek + 1}) | Soil: {(selectedPlot.soilTank * 100f):F0}% | Yield: {selectedPlot.yieldAccumulated:F2}";
			}
		}

		private void UpdateButtonStates()
		{
			var turnManager = runner.GetTurnManager();
			if (turnManager == null) return;

			bool canIrrigate = turnManager.Phase == TurnPhase.Planning && turnManager.Gold >= GetIrrigationCost();
			bool canPlant = turnManager.Phase == TurnPhase.Planning && 
				turnManager.SelectedPlotIndex < turnManager.Plots.Count && 
				turnManager.Plots[turnManager.SelectedPlotIndex].plantedWeek < 0;
			
			// Disable all actions during harvest
			if (turnManager.Phase == TurnPhase.Harvest)
			{
				canIrrigate = false;
				canPlant = false;
			}

			// Check nitrogen button states
			bool canApplyN = turnManager.Phase == TurnPhase.Planning && turnManager.Gold >= GetNitrogenCost();
			bool plotPlanted = turnManager.SelectedPlotIndex < turnManager.Plots.Count && 
				turnManager.Plots[turnManager.SelectedPlotIndex].plantedWeek >= 0;
			
			var selectedPlot = turnManager.SelectedPlotIndex < turnManager.Plots.Count ? 
				turnManager.Plots[turnManager.SelectedPlotIndex] : null;
			bool canApplyPartA = canApplyN && plotPlanted && selectedPlot != null && selectedPlot.nPartAWeek < 0;
			bool canApplyPartB = canApplyN && plotPlanted && selectedPlot != null && selectedPlot.nPartBWeek < 0;
			
			// Disable nitrogen actions during harvest
			if (turnManager.Phase == TurnPhase.Harvest)
			{
				canApplyPartA = false;
				canApplyPartB = false;
			}

            // Update button interactability
            if (irrigateButton != null)
				irrigateButton.interactable = canIrrigate;
			
			if (plantButton != null)
				plantButton.interactable = canPlant;
			
			if (nitrogenPartAButton != null)
				nitrogenPartAButton.interactable = canApplyPartA;
			
			if (nitrogenPartBButton != null)
				nitrogenPartBButton.interactable = canApplyPartB;
			
			// Buy plot button state
			int nextPlotCost = turnManager.GetNextPlotCost();
			bool canBuyPlot = turnManager.Phase == TurnPhase.Planning && turnManager.Gold >= nextPlotCost;
			
			// Disable buy plot during harvest
			if (turnManager.Phase == TurnPhase.Harvest)
			{
				canBuyPlot = false;
			}
			if (buyPlotButton != null)
			{
				buyPlotButton.interactable = canBuyPlot;
				// Update button text to show cost
				var buttonText = buyPlotButton.GetComponentInChildren<TextMeshProUGUI>();
				if (buttonText != null)
				{
					buttonText.text = $"Buy Plot ({nextPlotCost} Gold)";
				}
			}
			
			if (skipButton != null)
				skipButton.interactable = turnManager.Phase == TurnPhase.Planning;
			
			if (resolveButton != null)
				resolveButton.interactable = turnManager.Phase == TurnPhase.Planning || turnManager.Phase == TurnPhase.End;
		}

		private void ShowCropPicker()
		{
			if (cropPicker != null)
			{
				cropPicker.ShowPanelForPlanting();
			}
		}

		private int GetIrrigationCost()
		{
			var turnManager = runner.GetTurnManager();
			return turnManager?.IrrigationCost ?? 5;
		}

		private int GetNitrogenCost()
		{
			var turnManager = runner.GetTurnManager();
			return turnManager?.NitrogenPartACost ?? 8;
		}

		private void UpdateWeekDisplay()
		{
			// Update WeekDisplay with current turn manager
			if (weekDisplay != null)
			{
				var turnManager = runner.GetTurnManager();
				if (turnManager != null)
				{
					weekDisplay.SetTurnManager(turnManager);
				}
			}
		}
	}
}
