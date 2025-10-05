using UnityEngine;
using NasaSpaceApps.FarmFromSpace.Game.Systems;
using NasaSpaceApps.FarmFromSpace.Game;

namespace NasaSpaceApps.FarmFromSpace.Game.UI
{
	/// <summary>
	/// Helper script to integrate NASA Data Card with the endless mode game system
	/// Attach this to a GameObject in your scene to automatically handle NASA Data Card
	/// </summary>
	public class EndlessNasaDataCardHelper : MonoBehaviour
	{
		[Header("References")]
		[SerializeField] private EndlessGameRunner gameRunner;
		[SerializeField] private EndlessNasaDataCard nasaDataCard;

		[Header("Settings")]
		[SerializeField] private bool enableResolutionChips = true;
		[SerializeField] private bool enableCadenceChips = true;

		private void Start()
		{
			// Get references
			if (gameRunner == null)
				gameRunner = FindFirstObjectByType<EndlessGameRunner>();
			
			if (nasaDataCard == null)
				nasaDataCard = GetComponent<EndlessNasaDataCard>();

			// Configure NASA Data Card
			if (nasaDataCard != null)
			{
				nasaDataCard.SetResolutionChipsEnabled(enableResolutionChips);
				nasaDataCard.SetCadenceChipsEnabled(enableCadenceChips);
			}
		}

		// Public methods for manual control
		public void ToggleResolutionChips()
		{
			enableResolutionChips = !enableResolutionChips;
			if (nasaDataCard != null)
			{
				nasaDataCard.SetResolutionChipsEnabled(enableResolutionChips);
			}
		}

		public void ToggleCadenceChips()
		{
			enableCadenceChips = !enableCadenceChips;
			if (nasaDataCard != null)
			{
				nasaDataCard.SetCadenceChipsEnabled(enableCadenceChips);
			}
		}

		public void RefreshDataCard()
		{
			if (nasaDataCard != null)
			{
				nasaDataCard.RefreshDisplay();
			}
		}
	}
}
