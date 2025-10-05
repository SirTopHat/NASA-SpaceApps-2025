using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NasaSpaceApps.FarmFromSpace.Game.Systems;

namespace NasaSpaceApps.FarmFromSpace.Game.UI
{
	public class PlotPurchaseDialog : MonoBehaviour
	{
		[Header("UI References")]
		[SerializeField] private GameObject dialogPanel;
		[SerializeField] private TextMeshProUGUI titleText;
		[SerializeField] private TextMeshProUGUI descriptionText;
		[SerializeField] private TextMeshProUGUI costText;
		[SerializeField] private Button confirmButton;
		[SerializeField] private Button cancelButton;
		[SerializeField] private TextMeshProUGUI confirmButtonText;

		[Header("References")]
		[SerializeField] private EndlessGameRunner gameRunner;

		private int targetPlotIndex = -1;
		private EndlessTurnManager turnManager;

		private void Start()
		{
			if (gameRunner == null)
				gameRunner = FindFirstObjectByType<EndlessGameRunner>();

			// Setup button listeners
			if (confirmButton != null)
				confirmButton.onClick.AddListener(ConfirmPurchase);
			
			if (cancelButton != null)
				cancelButton.onClick.AddListener(CancelPurchase);

			// Start hidden
			HideDialog();
		}

		private void Update()
		{
			if (gameRunner == null) return;

			turnManager = gameRunner.GetTurnManager();
			if (turnManager == null) return;

			// Update confirm button state
			if (targetPlotIndex >= 0 && confirmButton != null)
			{
				bool canAfford = turnManager.CanUnlockPlot(targetPlotIndex);
				confirmButton.interactable = canAfford;
				
				if (confirmButtonText != null)
				{
					confirmButtonText.text = canAfford ? "Purchase" : "Not Enough Gold";
				}
			}
		}

		public void ShowDialog(int plotIndex)
		{
			if (turnManager == null) return;

			targetPlotIndex = plotIndex;
			
			// Update dialog content
			if (titleText != null)
			{
				titleText.text = $"Unlock Plot {plotIndex + 1}";
			}

			if (descriptionText != null)
			{
				descriptionText.text = $"Do you want to unlock this farm plot? This will allow you to plant crops and manage this plot.";
			}

			int cost = turnManager.GetPlotUnlockCost(plotIndex);
			if (costText != null)
			{
				costText.text = $"Cost: {cost} Gold";
			}

			// Show the dialog
			if (dialogPanel != null)
			{
				dialogPanel.SetActive(true);
			}
			else
			{
				gameObject.SetActive(true);
			}
		}

		public void HideDialog()
		{
			targetPlotIndex = -1;
			
			if (dialogPanel != null)
			{
				dialogPanel.SetActive(false);
			}
			else
			{
				gameObject.SetActive(false);
			}
		}

		private void ConfirmPurchase()
		{
			if (turnManager == null || targetPlotIndex < 0) return;

			if (turnManager.UnlockPlot(targetPlotIndex))
			{
				SoundManager.Instance.PlaySfx("buying");
                HideDialog();
				
				// Notify the game runner to update the UI and save the game
				if (gameRunner != null)
				{
					gameRunner.RefreshPlotUI();
					gameRunner.SaveGame(); // Save after unlocking plot
				}
				
				// Force refresh of all UI components that depend on plot state
				RefreshAllPlotUI();
			}
			else
			{
				Debug.LogWarning($"Failed to unlock plot {targetPlotIndex}");
			}
		}
		
		private void RefreshAllPlotUI()
		{
			// Debug: Check plot state after purchase
			if (turnManager != null && targetPlotIndex >= 0)
			{
				var plot = turnManager.Plots.Count > targetPlotIndex ? turnManager.Plots[targetPlotIndex] : null;
				Debug.Log($"Plot {targetPlotIndex} state after purchase: plantedWeek={plot?.plantedWeek}, plantedCrop={plot?.plantedCrop?.displayName ?? "null"}, soilTank={plot?.soilTank:F2}");
			}
			
			// Refresh plot button manager
			var plotButtonManager = FindFirstObjectByType<EndlessPlotButtonManager>();
			if (plotButtonManager != null)
			{
				plotButtonManager.RefreshPlotButtons();
			}
			
			// Refresh crop picker
			var cropPicker = FindFirstObjectByType<EndlessCropPicker>();
			if (cropPicker != null)
			{
				cropPicker.RefreshCropButtons();
			}
			
			// Refresh HUD
			var hud = FindFirstObjectByType<EndlessGameHUD>();
			if (hud != null)
			{
				hud.RefreshUI();
			}
		}

		private void CancelPurchase()
		{
			//SoundManager.Instance.PlaySfx("click");
			HideDialog();
		}

		public bool IsVisible => dialogPanel != null ? dialogPanel.activeInHierarchy : gameObject.activeInHierarchy;
	}
}
