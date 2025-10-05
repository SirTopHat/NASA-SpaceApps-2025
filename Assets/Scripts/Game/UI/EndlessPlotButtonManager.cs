using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NasaSpaceApps.FarmFromSpace.Game.Systems;
using NasaSpaceApps.FarmFromSpace.Game.Data;
using NasaSpaceApps.FarmFromSpace.Common;

namespace NasaSpaceApps.FarmFromSpace.Game.UI
{
	public class EndlessPlotButtonManager : MonoBehaviour
	{
		[Header("Plot Buttons")]
		[SerializeField] private Button[] plotButtons = new Button[9]; // 3x3 grid of plot buttons
		[SerializeField] private Image[] plotButtonImages = new Image[9];
		[SerializeField] private TextMeshProUGUI[] plotButtonTexts = new TextMeshProUGUI[9];
		[SerializeField] private GameObject[] lockIcons = new GameObject[9];

		[Header("Visual Settings")]
		[SerializeField] private Color unlockedColor = Color.white;
		[SerializeField] private Color lockedColor = Color.gray;
		[SerializeField] private Color selectedColor = Color.cyan;
		[SerializeField] private Color plantedColor = Color.green;
		[SerializeField] private Color dryColor = Color.yellow;
		[SerializeField] private Color wetColor = Color.blue;

		[Header("References")]
		[SerializeField] private EndlessGameRunner gameRunner;
		[SerializeField] private PlotPurchaseDialog purchaseDialog;

		private EndlessTurnManager turnManager;
		private int selectedPlotIndex = -1;

		private void Start()
		{
			if (gameRunner == null)
				gameRunner = FindFirstObjectByType<EndlessGameRunner>();

			if (purchaseDialog == null)
				purchaseDialog = FindFirstObjectByType<PlotPurchaseDialog>();

			SetupPlotButtons();
		}

		private void Update()
		{
			if (gameRunner == null) return;

			turnManager = gameRunner.GetTurnManager();
			if (turnManager == null) return;

			UpdatePlotButtons();
		}

		private void SetupPlotButtons()
		{
			for (int i = 0; i < plotButtons.Length; i++)
			{
				if (plotButtons[i] != null)
				{
					int plotIndex = i; // Capture for closure
					plotButtons[i].onClick.AddListener(() => OnPlotButtonClicked(plotIndex));
				}
			}
		}

		private void OnPlotButtonClicked(int plotIndex)
		{
			if (turnManager == null) return;
			
			// Check if any menus are open - if so, don't process plot button clicks
			if (IsAnyMenuOpen())
			{
				return;
			}

			if (turnManager.IsPlotUnlocked(plotIndex))
			{
				// Plot is unlocked - select it
				selectedPlotIndex = plotIndex;
				turnManager.SelectPlot(plotIndex);
				Debug.Log($"Selected plot {plotIndex}");
			}
			else
			{
				// Plot is locked - show purchase dialog
				Debug.Log($"Plot {plotIndex} is locked - showing purchase dialog");
				if (purchaseDialog != null)
				{
					purchaseDialog.ShowDialog(plotIndex);
				}
				else
				{
					Debug.LogWarning("Purchase dialog not found!");
				}
			}
		}
		
		private bool IsAnyMenuOpen()
		{
			// Check if plot purchase dialog is open
			if (purchaseDialog != null && purchaseDialog.IsVisible)
			{
				return true;
			}
			
			// Check if crop picker is open
			var cropPicker = FindFirstObjectByType<EndlessCropPicker>();
			if (cropPicker != null && cropPicker.IsVisible)
			{
				return true;
			}
			
			return false;
		}

		private void UpdatePlotButtons()
		{
			for (int i = 0; i < plotButtons.Length; i++)
			{
				UpdatePlotButton(i);
			}
		}

		private void UpdatePlotButton(int plotIndex)
		{
			if (plotButtons[plotIndex] == null) return;

			bool isUnlocked = turnManager.IsPlotUnlocked(plotIndex);
			bool isSelected = (plotIndex == selectedPlotIndex);
			
			// Update button interactability
			plotButtons[plotIndex].interactable = true; // Always clickable (either select or purchase)

			// Update visual state
			Color buttonColor = GetPlotButtonColor(plotIndex, isUnlocked, isSelected);
			if (plotButtonImages[plotIndex] != null)
			{
				plotButtonImages[plotIndex].color = buttonColor;
			}

			// Update lock icon
			if (lockIcons[plotIndex] != null)
			{
				lockIcons[plotIndex].SetActive(!isUnlocked);
			}

			// Update button text
			if (plotButtonTexts[plotIndex] != null)
			{
				if (!isUnlocked)
				{
					int cost = turnManager.GetPlotUnlockCost(plotIndex);
					plotButtonTexts[plotIndex].text = $"${cost}";
				}
				else
				{
					// Show plot status
					var plot = GetPlotState(plotIndex);
					if (plot != null)
					{
						if (plot.plantedWeek >= 0)
						{
							plotButtonTexts[plotIndex].text = plot.plantedCropName ?? "Planted";
						}
						else
						{
							plotButtonTexts[plotIndex].text = "Empty";
						}
					}
					else
					{
						plotButtonTexts[plotIndex].text = "Empty";
					}
				}
			}
		}

		private Color GetPlotButtonColor(int plotIndex, bool isUnlocked, bool isSelected)
		{
			if (!isUnlocked)
			{
				return lockedColor;
			}

			if (isSelected)
			{
				return selectedColor;
			}

			// Get plot state for color based on soil moisture or planting status
			var plot = GetPlotState(plotIndex);
			if (plot != null)
			{
				if (plot.plantedWeek >= 0)
				{
					return plantedColor;
				}
				else
				{
					// Color based on soil moisture
					if (plot.soilTank < 0.3f)
						return dryColor;
					else if (plot.soilTank > 0.8f)
						return wetColor;
					else
						return unlockedColor;
				}
			}

			return unlockedColor;
		}

		private PlotState GetPlotState(int plotIndex)
		{
			if (turnManager == null || turnManager.Plots == null) return null;
			if (plotIndex < 0 || plotIndex >= turnManager.Plots.Count) return null;
			return turnManager.Plots[plotIndex];
		}

		public void RefreshPlotButtons()
		{
			// Force refresh of all plot buttons
			UpdatePlotButtons();
		}

		public void SetSelectedPlot(int plotIndex)
		{
			selectedPlotIndex = plotIndex;
		}
	}
}
