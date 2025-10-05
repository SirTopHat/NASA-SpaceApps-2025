using NasaSpaceApps.FarmFromSpace.Game.Systems;

namespace NasaSpaceApps.FarmFromSpace.Game.UI
{
	/// <summary>
	/// Interface for harvest UI systems to implement
	/// </summary>
	public interface IHarvestUI
	{
		/// <summary>
		/// Show the harvest panel with the given score information
		/// </summary>
		/// <param name="harvestGold">Gold earned from harvest</param>
		/// <param name="unspentGold">Gold remaining unspent</param>
		/// <param name="finalScore">Total final score</param>
		void ShowHarvest(int harvestGold, int unspentGold, int finalScore);

		/// <summary>
		/// Hide the harvest panel
		/// </summary>
		void HideHarvest();

		/// <summary>
		/// Check if the harvest panel is currently visible
		/// </summary>
		bool IsHarvestVisible { get; }
	}
}
