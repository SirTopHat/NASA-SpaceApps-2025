using UnityEngine;
using NasaSpaceApps.FarmFromSpace.Game;
using NasaSpaceApps.FarmFromSpace.Game.Data;

namespace NasaSpaceApps.FarmFromSpace.Game.UI
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class EndlessPlotView : MonoBehaviour
	{
		public int plotIndex;
		public EndlessGameRunner runner;

		private SpriteRenderer sr;
		private CropGrowthVisualizer cropVisualizer;

		private void Awake()
		{
			sr = GetComponent<SpriteRenderer>();
			cropVisualizer = GetComponentInChildren<CropGrowthVisualizer>();
		}

		private void OnMouseDown()
		{
			// Check if any menus are open - if so, don't process plot clicks
			if (IsAnyMenuOpen())
			{
				return;
			}
			
			//Debug.Log($"OnMouseDown called on plot {plotIndex}");
			if (runner != null)
			{
				var turnManager = runner.GetTurnManager();
				if (turnManager != null)
				{
					if (turnManager.IsPlotUnlocked(plotIndex))
					{
						//Debug.Log($"Selecting unlocked plot {plotIndex}");
						turnManager.SelectPlot(plotIndex);
					}
					else
					{
						// Plot is locked - show purchase dialog
						//Debug.Log($"Plot {plotIndex} is locked - showing purchase dialog");
						ShowPurchaseDialog(plotIndex);
					}
				}
			}
		}
		
		private bool IsAnyMenuOpen()
		{
			// Check if plot purchase dialog is open
			var purchaseDialog = FindFirstObjectByType<PlotPurchaseDialog>();
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

		private void ShowPurchaseDialog(int plotIndex)
		{
			// Find the purchase dialog
			var purchaseDialog = FindFirstObjectByType<PlotPurchaseDialog>();
			if (purchaseDialog != null)
			{
				purchaseDialog.ShowDialog(plotIndex);
			}
			else
			{
				Debug.LogWarning("PlotPurchaseDialog not found! Cannot show purchase dialog.");
			}
		}

		public void SetColor(Color c)
		{
			if (sr == null) sr = GetComponent<SpriteRenderer>();
			sr.color = c;
		}

		public void UpdateVisualState()
		{
			if (runner == null || runner.GetTurnManager() == null) return;

			var turnManager = runner.GetTurnManager();
			bool isUnlocked = turnManager.IsPlotUnlocked(plotIndex);
			bool isSelected = (plotIndex == turnManager.SelectedPlotIndex);
			bool menuOpen = IsAnyMenuOpen();

			Color plotColor;
			
			if (!isUnlocked)
			{
				// Locked plot - gray color
				plotColor = Color.gray;
			}
			else if (plotIndex >= turnManager.Plots.Count || turnManager.Plots[plotIndex] == null)
			{
				// Unlocked but no plot data yet
				plotColor = Color.white;
			}
			else
			{
				// Unlocked plot with data
				var plot = turnManager.Plots[plotIndex];
				// Use preview soil fraction during planning so irrigation shows immediately
				float previewSoil = turnManager.GetPreviewSoilFraction(plotIndex);
				plotColor = GetPlotColorFromSoil(previewSoil);
			}
			
			// Highlight selected plot
			if (isSelected)
			{
                plotColor = Color.Lerp(plotColor, Color.cyan, 0.4f);
			}
			
			// Dim plots when menu is open to indicate they're not clickable
			if (menuOpen)
			{
				plotColor = Color.Lerp(plotColor, Color.gray, 0.5f);
			}

			SetColor(plotColor);
			
			// Update crop growth visualizer
				if (cropVisualizer != null)
				{
					cropVisualizer.SetPlotIndex(plotIndex);
				}
		}

		private Color GetPlotColorFromSoil(float soilFraction)
		{
			// Color based on soil moisture
			if (soilFraction < 0.3f)
				return Color.red; // Dry
			else if (soilFraction > 0.8f)
				return Color.blue; // Wet
			else
				return Color.white; // Good moisture
		}
	}
}

