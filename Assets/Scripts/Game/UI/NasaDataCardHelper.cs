using UnityEngine;
using NasaSpaceApps.FarmFromSpace.Game.Systems;

namespace NasaSpaceApps.FarmFromSpace.Game.UI
{
	/// <summary>
	/// Helper script to demonstrate how to integrate NASA Data Card with the game system
	/// Attach this to a GameObject in your scene to automatically handle NASA Data Card
	/// </summary>
	public class NasaDataCardHelper : MonoBehaviour
	{
		[Header("References")]
		[SerializeField] private GameRunner gameRunner;
		[SerializeField] private GameUINasaDataCard nasaDataCard;

		[Header("Settings")]
		[SerializeField] private bool enableResolutionChips = true;
		[SerializeField] private bool enableCadenceChips = true;

		private void Start()
		{
			// Get references
			if (gameRunner == null)
				gameRunner = FindFirstObjectByType<GameRunner>();
			
			if (nasaDataCard == null)
				nasaDataCard = GetComponent<GameUINasaDataCard>();

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
