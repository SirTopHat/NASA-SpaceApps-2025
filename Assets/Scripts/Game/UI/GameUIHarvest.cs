using UnityEngine;
using TMPro;
using UnityEngine.UI;
using NasaSpaceApps.FarmFromSpace.Game.Systems;

namespace NasaSpaceApps.FarmFromSpace.Game.UI
{
	public class GameUIHarvest : MonoBehaviour, IHarvestUI
	{
		[Header("Harvest Panel")]
		[SerializeField] private GameObject harvestPanel;
		[SerializeField] private TextMeshProUGUI titleText;
		[SerializeField] private TextMeshProUGUI scoreText;
		[SerializeField] private Button mainMenuButton;
		[SerializeField] private Button restartButton;

		[Header("References")]
		[SerializeField] private GameRunner gameRunner;

		private TurnManager turnManager;
		private bool isRestarting = false;

		private void Start()
		{
			// Hide panel initially
			if (harvestPanel != null)
				harvestPanel.SetActive(false);

			// Wire up button events
			if (mainMenuButton != null)
				mainMenuButton.onClick.AddListener(OnMainMenuClicked);
			
			if (restartButton != null)
				restartButton.onClick.AddListener(OnRestartClicked);
		}

		private void Update()
		{
			// Get turn manager reference (refresh it in case of restart)
			if (gameRunner != null)
			{
				var newTurnManager = gameRunner.GetTurnManager();
				if (newTurnManager != turnManager)
				{
					turnManager = newTurnManager;
					// If we got a new turn manager, we're no longer restarting
					if (turnManager != null && !turnManager.IsGameComplete)
					{
						isRestarting = false;
					}
				}
			}

			// Show/hide panel based on game state (but not during restart)
			if (turnManager != null && !isRestarting)
			{
				bool shouldShowPanel = turnManager.IsGameComplete;
				if (harvestPanel != null && harvestPanel.activeSelf != shouldShowPanel)
				{
					harvestPanel.SetActive(shouldShowPanel);
					
					if (shouldShowPanel)
					{
						UpdateHarvestDisplay();
					}
				}
			}
		}

		private void UpdateHarvestDisplay()
		{
			if (turnManager == null) return;

			// Update title
			if (titleText != null)
			{
				titleText.text = "HARVEST COMPLETE!";
			}

			// Update score display
			if (scoreText != null)
			{
				scoreText.text = $"Harvest Gold: {turnManager.HarvestGold}\n" +
				                $"Unspent Gold: {turnManager.Gold}\n" +
				                $"Final Score: {turnManager.FinalScore}";
			}
		}

		private void OnMainMenuClicked()
		{
			Debug.Log("Main Menu clicked - implement main menu navigation");
			// TODO: Implement main menu navigation
			// For now, just hide the panel
			if (harvestPanel != null)
				harvestPanel.SetActive(false);
		}

		private void OnRestartClicked()
		{
			// Set restarting flag to prevent automatic showing
			isRestarting = true;
			
			// Hide the panel first
			if (harvestPanel != null) {
                harvestPanel.SetActive(false);
            }
			
			// Trigger game restart
			if (gameRunner != null)
			{
				// Call the restart method on GameRunner
				gameRunner.RestartGame();
			}
		}

		// Public method to manually show/hide the panel (for testing)
		public void ShowHarvestPanel()
		{
			if (harvestPanel != null)
			{
				harvestPanel.SetActive(true);
				UpdateHarvestDisplay();
			}
		}

		public void HideHarvestPanel()
		{
			if (harvestPanel != null)
			{
				harvestPanel.SetActive(false);
			}
		}

		// IHarvestUI Interface Implementation
		public void ShowHarvest(int harvestGold, int unspentGold, int finalScore)
		{
			if (harvestPanel != null)
			{
				harvestPanel.SetActive(true);
				
				// Update display with provided values
				if (titleText != null)
				{
					titleText.text = "🌾 HARVEST COMPLETE! 🌾";
				}

				if (scoreText != null)
				{
					scoreText.text = $"Harvest Gold: {harvestGold}\n" +
					                $"Unspent Gold: {unspentGold}\n" +
					                $"Final Score: {finalScore}";
				}
			}
		}

		public void HideHarvest()
		{
			HideHarvestPanel();
		}

		public bool IsHarvestVisible => harvestPanel != null && harvestPanel.activeSelf;
		
		// Public method to manually set restarting state
		public void SetRestarting(bool restarting)
		{
			isRestarting = restarting;
		}
	}
}
