using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NasaSpaceApps.FarmFromSpace.Game;
using NasaSpaceApps.FarmFromSpace.Game.UI;

namespace NasaSpaceApps.FarmFromSpace.Game.UI
{
	/// <summary>
	/// Helper script to automatically set up UI references for endless mode
	/// Attach this to your main UI Canvas to auto-configure components
	/// </summary>
	public class EndlessUISetupHelper : MonoBehaviour
	{
		[Header("Auto-Setup")]
		[SerializeField] private bool autoSetupOnStart = true;
		
		[Header("UI Components to Setup")]
		[SerializeField] private EndlessGameHUD gameHUD;
		[SerializeField] private RegionSwitcherUI regionSwitcher;
		[SerializeField] private EndlessCropPicker cropPicker;
		[SerializeField] private CropSuitabilityUI suitabilityUI;
		
		[Header("References")]
		[SerializeField] private EndlessGameRunner gameRunner;

		private void Start()
		{
			if (autoSetupOnStart)
			{
				SetupUIComponents();
			}
		}

		[ContextMenu("Setup UI Components")]
		public void SetupUIComponents()
		{
			// Find game runner if not assigned
			if (gameRunner == null)
			{
				gameRunner = FindFirstObjectByType<EndlessGameRunner>();
				if (gameRunner == null)
				{
					Debug.LogError("EndlessUISetupHelper: No EndlessGameRunner found in scene!");
					return;
				}
			}

			// Setup Game HUD
			if (gameHUD == null)
			{
				gameHUD = FindFirstObjectByType<EndlessGameHUD>();
			}

			// Setup Region Switcher
			if (regionSwitcher == null)
			{
				regionSwitcher = FindFirstObjectByType<RegionSwitcherUI>();
			}

			// Setup Crop Picker
			if (cropPicker == null)
			{
				cropPicker = FindFirstObjectByType<EndlessCropPicker>();
			}

			// Setup Suitability UI
			if (suitabilityUI == null)
			{
				suitabilityUI = FindFirstObjectByType<CropSuitabilityUI>();
			}

			// Assign references
			if (regionSwitcher != null)
			{
				regionSwitcher.gameObject.SetActive(true);
			}

			if (cropPicker != null)
			{
				cropPicker.gameObject.SetActive(true);
			}

			if (suitabilityUI != null)
			{
				suitabilityUI.gameObject.SetActive(true);
			}

			Debug.Log("Endless UI components setup complete!");
		}

		[ContextMenu("Create Basic UI Structure")]
		public void CreateBasicUIStructure()
		{
			// This would create a basic UI hierarchy if needed
			// For now, just log what needs to be created
			Debug.Log("Basic UI Structure needed:");
			Debug.Log("1. Canvas with EndlessGameHUD");
			Debug.Log("2. RegionSwitcherUI panel");
			Debug.Log("3. EndlessCropPicker panel");
			Debug.Log("4. CropSuitabilityUI panel");
			Debug.Log("5. Plot selection buttons");
		}
	}
}
