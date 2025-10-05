using UnityEngine;
using NasaSpaceApps.FarmFromSpace.Config;

namespace NasaSpaceApps.FarmFromSpace.Game.UI
{
	/// <summary>
	/// Helper script to easily edit starting gold in the inspector
	/// Attach this to any GameObject in your scene to modify starting gold
	/// </summary>
	public class StartingGoldEditor : MonoBehaviour
	{
		[Header("Starting Gold Settings")]
		[SerializeField] private BalanceConfig balanceConfig;
		[SerializeField] private int newStartingGold = 100;
		
		[Header("Actions")]
		[SerializeField] private bool applyChanges = false;

		private void OnValidate()
		{
			if (applyChanges && balanceConfig != null)
			{
				balanceConfig.startingGold = newStartingGold;
				applyChanges = false;
				Debug.Log($"Starting gold updated to {newStartingGold}");
			}
		}

		[ContextMenu("Update Starting Gold")]
		public void UpdateStartingGold()
		{
			if (balanceConfig != null)
			{
				balanceConfig.startingGold = newStartingGold;
				Debug.Log($"Starting gold updated to {newStartingGold}");
			}
			else
			{
				Debug.LogError("BalanceConfig not assigned!");
			}
		}
	}
}
