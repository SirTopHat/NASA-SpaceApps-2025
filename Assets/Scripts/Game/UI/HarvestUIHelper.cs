using UnityEngine;
using NasaSpaceApps.FarmFromSpace.Game.Systems;

namespace NasaSpaceApps.FarmFromSpace.Game.UI
{
	/// <summary>
	/// Helper script to demonstrate how to integrate harvest UI with the game system
	/// Attach this to a GameObject in your scene to automatically handle harvest UI
	/// </summary>
	public class HarvestUIHelper : MonoBehaviour
	{
		[Header("References")]
		[SerializeField] private GameRunner gameRunner;
		[SerializeField] private IHarvestUI harvestUI;

		private TurnManager turnManager;
		private bool wasHarvestVisible = false;

		private void Start()
		{
			// Get references
			if (gameRunner == null)
				gameRunner = FindFirstObjectByType<GameRunner>();
			
			if (harvestUI == null)
				harvestUI = GetComponent<IHarvestUI>();
		}

		private void Update()
		{
			// Get turn manager reference
			if (turnManager == null && gameRunner != null)
			{
				turnManager = gameRunner.GetTurnManager();
			}

			// Handle harvest UI visibility
			if (turnManager != null && harvestUI != null)
			{
				bool isHarvestVisible = turnManager.IsGameComplete;
				
				// Show harvest UI when game completes
				if (isHarvestVisible && !wasHarvestVisible)
				{
					harvestUI.ShowHarvest(
						turnManager.HarvestGold,
						turnManager.Gold,
						turnManager.FinalScore
					);
				}
				// Hide harvest UI when game restarts
				else if (!isHarvestVisible && wasHarvestVisible)
				{
					harvestUI.HideHarvest();
				}
				
				wasHarvestVisible = isHarvestVisible;
			}
		}

		// Public methods for manual control
		public void ShowHarvestManually()
		{
			if (harvestUI != null && turnManager != null)
			{
				harvestUI.ShowHarvest(
					turnManager.HarvestGold,
					turnManager.Gold,
					turnManager.FinalScore
				);
			}
		}

		public void HideHarvestManually()
		{
			if (harvestUI != null)
			{
				harvestUI.HideHarvest();
			}
		}
	}
}
