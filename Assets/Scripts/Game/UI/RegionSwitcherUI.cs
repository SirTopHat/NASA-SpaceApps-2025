using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NasaSpaceApps.FarmFromSpace.Common;
using NasaSpaceApps.FarmFromSpace.Game;
using NasaSpaceApps.FarmFromSpace.Game.Systems;
using System;

namespace NasaSpaceApps.FarmFromSpace.Game.UI
{
	/// <summary>
	/// UI component for switching between different regions
	/// Handles region selection, data switching, and UI updates
	/// </summary>
	public class RegionSwitcherUI : MonoBehaviour
	{
	[Header("UI References")]
	[SerializeField] private Button regionSwitchButton;
	[SerializeField] private GameObject regionMenuPanel;
	[SerializeField] private Button[] regionButtons;
	[SerializeField] private TMP_Text[] regionButtonTexts;
	[SerializeField] private Image[] regionButtonImages;
	[SerializeField] private TMP_Text currentRegionText;
	[SerializeField] private Button closeMenuButton;
        [SerializeField] private SpriteRenderer farmBackgroundSprRend;

	[Header("NASA Data Loading UI")]
	[SerializeField] private GameObject nasaDataLoadingPanel;
	[SerializeField] private TMP_Text nasaDataLoadingText;
	[SerializeField] private UnityEngine.UI.Slider nasaDataLoadingProgress;
	[SerializeField] private TMP_Text nasaDataStatusText;

	[Header("Region Buying UI")]
	[SerializeField] private GameObject regionBuyDialog;
	[SerializeField] private TMP_Text buyDialogTitle;
	[SerializeField] private TMP_Text buyDialogDescription;
	[SerializeField] private TMP_Text buyDialogCost;
	[SerializeField] private Button buyConfirmButton;
	[SerializeField] private Button buyCancelButton;
	[SerializeField] private TMP_Text[] regionButtonCostTexts; // Cost text for each region button

	[Header("Region Info Panel UI")]
	[SerializeField] private GameObject regionInfoPanel;
	[SerializeField] private TMP_Text regionInfoTitle;
	[SerializeField] private TMP_Text regionInfoDescription;

        [Header("Region Configuration")]
		[SerializeField] private RegionType[] availableRegions = { RegionType.TropicalMonsoon, RegionType.SemiAridSteppe, RegionType.TemperateContinental };
		[SerializeField] private string[] regionDisplayNames = { "Tropical Monsoon", "Semi-Arid Steppe", "Temperate Continental" };
		[SerializeField] private Sprite[] regionSprites;
        [SerializeField] private Sprite[] farmBackgrounds;

        [Header("Game References")]
		[SerializeField] private EndlessGameRunner gameRunner;
		[SerializeField] private EndlessGameHUD gameHUD;

		private bool isMenuOpen = false;
		private RegionType currentRegion;
		private bool isNasaDataLoading = false;
		private RegionType pendingBuyRegion;

		private void Start()
		{
			InitializeUI();
			SetupEventListeners();
		}

		private void InitializeUI()
		{
			// Find game references if not assigned
			if (gameRunner == null)
				gameRunner = FindFirstObjectByType<EndlessGameRunner>();
			
			if (gameHUD == null)
				gameHUD = FindFirstObjectByType<EndlessGameHUD>();

			// Get current region from game runner
			if (gameRunner != null)
			{
				currentRegion = gameRunner.GetCurrentRegion();
			}

			// Initialize region buttons
			SetupRegionButtons();
			
			// Update current region display
			UpdateCurrentRegionDisplay();
			
			// Hide menu initially
			if (regionMenuPanel != null)
				regionMenuPanel.SetActive(false);

			// Hide NASA data loading panel initially
			if (nasaDataLoadingPanel != null)
				nasaDataLoadingPanel.SetActive(false);

			// Hide region buy dialog initially
			if (regionBuyDialog != null)
				regionBuyDialog.SetActive(false);

			// Hide region info panel initially
			if (regionInfoPanel != null)
				regionInfoPanel.SetActive(false);
		}

		private void SetupEventListeners() {
			// Region switch button
			if (regionSwitchButton != null) {
                regionSwitchButton.onClick.AddListener(ToggleRegionMenu);
			}

			// Close menu button
			if (closeMenuButton != null)
				closeMenuButton.onClick.AddListener(CloseRegionMenu);

			// Region selection buttons
			if (regionButtons != null)
			{
				for (int i = 0; i < regionButtons.Length && i < availableRegions.Length; i++)
				{
					int regionIndex = i; // Capture for closure
					if (regionButtons[i] != null)
					{
						regionButtons[i].onClick.AddListener(() => SelectRegion(availableRegions[regionIndex]));
					}
				}
			}

			// Region buy dialog buttons
			if (buyConfirmButton != null)
				buyConfirmButton.onClick.AddListener(ConfirmRegionPurchase);
			
			if (buyCancelButton != null)
				buyCancelButton.onClick.AddListener(CancelRegionPurchase);
		}

		private void SetupRegionButtons()
		{
			if (regionButtons == null || regionButtonTexts == null) return;

			for (int i = 0; i < regionButtons.Length && i < availableRegions.Length; i++)
			{
				RegionType region = availableRegions[i];
				bool isUnlocked = IsRegionUnlocked(region);
				bool isCurrentRegion = region == currentRegion;

				// Set button text
				if (regionButtonTexts[i] != null)
				{
					regionButtonTexts[i].text = regionDisplayNames[i];
				}

				// Set button image if available
				if (regionButtonImages != null && regionButtonImages[i] != null && regionSprites != null && i < regionSprites.Length)
				{
					regionButtonImages[i].sprite = regionSprites[i];
				}

				// Update button state based on unlock status
				UpdateRegionButtonState(i, region, isUnlocked, isCurrentRegion);

				// Update cost text for locked regions
				UpdateRegionCostText(i, region, isUnlocked);
			}
		}

		private void UpdateRegionButtonState(int buttonIndex, RegionType region, bool isUnlocked, bool isCurrentRegion)
		{
			if (regionButtons == null || buttonIndex >= regionButtons.Length) return;

			var button = regionButtons[buttonIndex];
			if (button == null) return;

			// Set button interactable based on unlock status
			button.interactable = isUnlocked || !isCurrentRegion;

			// Update button colors
			var colors = button.colors;
			if (isCurrentRegion)
			{
				colors.normalColor = Color.green;
				colors.highlightedColor = Color.green;
			}
			else if (isUnlocked)
			{
				colors.normalColor = Color.white;
				colors.highlightedColor = Color.white;
			}
			else
			{
				colors.normalColor = Color.gray;
				colors.highlightedColor = Color.gray;
			}
			button.colors = colors;
		}

		private void UpdateRegionCostText(int buttonIndex, RegionType region, bool isUnlocked)
		{
			if (regionButtonCostTexts == null || buttonIndex >= regionButtonCostTexts.Length) return;

			var costText = regionButtonCostTexts[buttonIndex];
			if (costText == null) return;

			if (isUnlocked)
			{
				costText.text = "Unlocked";
				costText.color = Color.green;
			}
			else
			{
				int cost = GetRegionUnlockCost(region);
				costText.text = $"Cost: {cost} Gold";
				costText.color = Color.yellow;
			}
		}

		private void HighlightCurrentRegion(int buttonIndex)
		{
			if (regionButtons != null && buttonIndex < regionButtons.Length)
			{
				// Add visual indication that this is the current region
				var button = regionButtons[buttonIndex];
				if (button != null)
				{
					// You can add visual effects here, like changing color or adding a border
					var colors = button.colors;
					colors.normalColor = Color.white;
					button.colors = colors;
				}
			}
		}

		public void ToggleRegionMenu()
		{
            if (regionMenuPanel == null) return;

			isMenuOpen = !isMenuOpen;
			regionMenuPanel.SetActive(isMenuOpen);

			if (isMenuOpen)
			{
				UpdateRegionMenu();
			}
		}

		public void OpenRegionMenu()
		{
			if (regionMenuPanel != null)
			{
				isMenuOpen = true;
				regionMenuPanel.SetActive(true);
				UpdateRegionMenu();
			}
		}

		public void CloseRegionMenu()
		{
			if (regionMenuPanel != null)
			{
				isMenuOpen = false;
				regionMenuPanel.SetActive(false);
			}
		}

		private void UpdateRegionMenu()
		{
			// Update button states based on current region
			SetupRegionButtons();
		}

	public void SelectRegion(RegionType newRegion)
	{
		if (newRegion == currentRegion)
		{
			CloseRegionMenu();
			return;
		}

		// Check if region is unlocked
		if (!IsRegionUnlocked(newRegion))
		{
			// Show buy dialog for locked region
			ShowRegionBuyDialog(newRegion);
			return;
		}

		// Region is unlocked, proceed with switching
		SwitchToRegion(newRegion);
	}

	private void SwitchToRegion(RegionType newRegion)
	{
		// Show NASA data loading UI
		ShowNasaDataLoading(newRegion);

		// Switch region in game runner
		if (gameRunner != null)
		{
			gameRunner.SwitchRegion(newRegion);
		}

		// Update current region
		currentRegion = newRegion;
		farmBackgroundSprRend.sprite = farmBackgrounds[(int)newRegion];
		UpdateCurrentRegionDisplay();

		// If info panel is open, refresh its contents for the new region
		if (regionInfoPanel != null && regionInfoPanel.activeSelf)
		{
			RefreshRegionInfoPanel();
		}

		// Close menu
		CloseRegionMenu();

		// Notify HUD of region change
		if (gameHUD != null)
		{
			gameHUD.OnRegionChanged(newRegion);
		}

		// Hide NASA data loading after a short delay
		StartCoroutine(HideNasaDataLoadingAfterDelay());
	}

		private void UpdateCurrentRegionDisplay()
		{
			if (currentRegionText != null)
			{
				int regionIndex = Array.IndexOf(availableRegions, currentRegion);
				if (regionIndex >= 0 && regionIndex < regionDisplayNames.Length)
				{
					currentRegionText.text = $"Current Region: {regionDisplayNames[regionIndex]}";
				}
				else
				{
					currentRegionText.text = $"Current Region: {currentRegion}";
				}
			}
		}

		// Public methods for external control
		public void SetCurrentRegion(RegionType region)
		{
			currentRegion = region;
			UpdateCurrentRegionDisplay();
			SetupRegionButtons();
		}

		public RegionType GetCurrentRegion()
		{
			return currentRegion;
		}

		public bool IsMenuOpen()
		{
			return isMenuOpen;
		}

		/// <summary>
		/// Shows NASA data loading UI when switching regions
		/// </summary>
		private void ShowNasaDataLoading(RegionType newRegion)
		{
			if (nasaDataLoadingPanel != null)
			{
				isNasaDataLoading = true;
				nasaDataLoadingPanel.SetActive(true);

				// Update loading text
				if (nasaDataLoadingText != null)
				{
					int regionIndex = System.Array.IndexOf(availableRegions, newRegion);
					string regionName = regionIndex >= 0 && regionIndex < regionDisplayNames.Length 
						? regionDisplayNames[regionIndex] 
						: newRegion.ToString();
					
					nasaDataLoadingText.text = $"Loading NASA data for {regionName}...";
				}

				// Update status text
				if (nasaDataStatusText != null)
				{
					nasaDataStatusText.text = "Fetching satellite data from NASA APIs...";
				}

				// Reset progress bar
				if (nasaDataLoadingProgress != null)
				{
					nasaDataLoadingProgress.value = 0f;
				}

				Debug.Log($"RegionSwitcherUI: Showing NASA data loading for {newRegion}");
			}
		}

		/// <summary>
		/// Hides NASA data loading UI after a delay
		/// </summary>
		private System.Collections.IEnumerator HideNasaDataLoadingAfterDelay()
		{
			// Simulate loading progress
			if (nasaDataLoadingProgress != null)
			{
				float progress = 0f;
				while (progress < 1f)
				{
					progress += Time.deltaTime * 2f; // 0.5 second loading
					nasaDataLoadingProgress.value = progress;
					yield return null;
				}
			}

			// Wait a bit more to show completion
			yield return new WaitForSeconds(0.5f);

			// Hide loading panel
			if (nasaDataLoadingPanel != null)
			{
				nasaDataLoadingPanel.SetActive(false);
				isNasaDataLoading = false;
				Debug.Log("RegionSwitcherUI: NASA data loading completed");
			}
		}

		/// <summary>
		/// Updates NASA data loading progress
		/// </summary>
		public void UpdateNasaDataLoadingProgress(float progress, string status = "")
		{
			if (nasaDataLoadingProgress != null)
			{
				nasaDataLoadingProgress.value = progress;
			}

			if (nasaDataStatusText != null && !string.IsNullOrEmpty(status))
			{
				nasaDataStatusText.text = status;
			}
		}

		/// <summary>
		/// Checks if NASA data is currently loading
		/// </summary>
		public bool IsNasaDataLoading()
		{
			return isNasaDataLoading;
		}

		/// <summary>
		/// Shows the region buy dialog for a locked region
		/// </summary>
		private void ShowRegionBuyDialog(RegionType region)
		{
			if (regionBuyDialog == null) return;

			pendingBuyRegion = region;
			regionBuyDialog.SetActive(true);

			// Update dialog content
			UpdateBuyDialogContent(region);

			Debug.Log($"RegionSwitcherUI: Showing buy dialog for {region}");
		}

		/// <summary>
		/// Updates the buy dialog content with region information
		/// </summary>
		private void UpdateBuyDialogContent(RegionType region)
		{
			int regionIndex = Array.IndexOf(availableRegions, region);
			string regionName = regionIndex >= 0 && regionIndex < regionDisplayNames.Length 
				? regionDisplayNames[regionIndex] 
				: region.ToString();

			int cost = GetRegionUnlockCost(region);
			int currentGold = GetCurrentGold();

			// Update dialog title
			if (buyDialogTitle != null)
			{
				buyDialogTitle.text = $"Unlock {regionName}";
			}

			// Update dialog description with region characteristics
			if (buyDialogDescription != null)
			{
				buyDialogDescription.text = GetRegionDescription(region);
			}

			// Update cost display
			if (buyDialogCost != null)
			{
				buyDialogCost.text = $"Cost: {cost} Gold\nYour Gold: {currentGold}";
				buyDialogCost.color = currentGold >= cost ? Color.white : Color.red;
			}

			// Update confirm button state
			if (buyConfirmButton != null)
			{
				buyConfirmButton.interactable = currentGold >= cost;
			}
		}

		/// <summary>
		/// Returns a rich description of the region's farming characteristics
		/// </summary>
		private string GetRegionDescription(RegionType region)
		{
			switch (region)
			{
				case RegionType.TropicalMonsoon:
					return "Soil: Loam / Clay\nExpected Rain: 5–25 mm/week\nExpected ET: 12–20 mm/week\nNDVI: Green-up to Peak during wet season\nData cadence: IMERG ~30min rain, POWER daily ET, MODIS 16-day NDVI";
				case RegionType.SemiAridSteppe:
					return "Soil: Sandy / Loam\nExpected Rain: 0–15 mm/week\nExpected ET: 15–25 mm/week\nNDVI: Short peak, long dormant periods\nData cadence: IMERG ~30min rain, POWER daily ET, MODIS 16-day NDVI";
				case RegionType.TemperateContinental:
					return "Soil: Loam\nExpected Rain: 3–18 mm/week\nExpected ET: 10–18 mm/week\nNDVI: Clear seasons with strong peak growth\nData cadence: IMERG ~30min rain, POWER daily ET, MODIS 16-day NDVI";
				default:
					return "Soil: Loam\nExpected Rain: 3–18 mm/week\nExpected ET: 10–18 mm/week\nNDVI: Seasonal\nData cadence: IMERG ~30min, POWER daily, MODIS 16-day";
			}
		}

		/// <summary>
		/// Populates the region info panel with the current region's title and description
		/// </summary>
		private void RefreshRegionInfoPanel()
		{
			if (regionInfoTitle != null)
			{
				int regionIndex = Array.IndexOf(availableRegions, currentRegion);
				string regionName = regionIndex >= 0 && regionIndex < regionDisplayNames.Length 
					? regionDisplayNames[regionIndex] 
					: currentRegion.ToString();
				regionInfoTitle.text = regionName;
			}

			if (regionInfoDescription != null)
			{
				regionInfoDescription.text = GetRegionDescription(currentRegion);
			}
		}

		/// <summary>
		/// Shows the region info panel. Wire this to your info icon.
		/// </summary>
		public void ShowRegionInfo()
		{
			RefreshRegionInfoPanel();
			if (regionInfoPanel != null)
			{
				regionInfoPanel.SetActive(true);
			}
		}

		/// <summary>
		/// Hides the region info panel.
		/// </summary>
		public void HideRegionInfo()
		{
			if (regionInfoPanel != null)
			{
				regionInfoPanel.SetActive(false);
			}
		}

		/// <summary>
		/// Toggles the region info panel visibility.
		/// </summary>
		public void ToggleRegionInfo()
		{
			if (regionInfoPanel == null) return;
			if (regionInfoPanel.activeSelf)
			{
				HideRegionInfo();
			}
			else
			{
				ShowRegionInfo();
			}
		}

		/// <summary>
		/// Confirms the region purchase
		/// </summary>
		private void ConfirmRegionPurchase()
		{
			if (gameRunner == null) return;

			// Attempt to unlock the region
			if (gameRunner.UnlockRegion(pendingBuyRegion))
			{
				SoundManager.Instance.PlaySfx("buying");
				// Close buy dialog
				CloseRegionBuyDialog();
				
				// Update region buttons
				UpdateRegionMenu();
				
				// Switch to the newly unlocked region
				SwitchToRegion(pendingBuyRegion);
			}
			else
			{
				Debug.LogWarning($"RegionSwitcherUI: Failed to unlock {pendingBuyRegion} - insufficient gold");
			}
		}

		/// <summary>
		/// Cancels the region purchase
		/// </summary>
		private void CancelRegionPurchase()
		{
			CloseRegionBuyDialog();
		}

		/// <summary>
		/// Closes the region buy dialog
		/// </summary>
		private void CloseRegionBuyDialog()
		{
			if (regionBuyDialog != null)
			{
				regionBuyDialog.SetActive(false);
			}
			pendingBuyRegion = RegionType.TropicalMonsoon; // Reset
		}

		/// <summary>
		/// Checks if a region is unlocked
		/// </summary>
		private bool IsRegionUnlocked(RegionType region)
		{
			if (gameRunner == null) return false;
			return gameRunner.IsRegionUnlocked(region);
		}

		/// <summary>
		/// Gets the unlock cost for a region
		/// </summary>
		private int GetRegionUnlockCost(RegionType region)
		{
			if (gameRunner == null) return 0;
			return gameRunner.GetRegionUnlockCost(region);
		}

		/// <summary>
		/// Gets the current gold amount
		/// </summary>
		private int GetCurrentGold()
		{
			if (gameRunner == null) return 0;
			return gameRunner.GetCurrentGold();
		}

		// Context menu methods for testing
		[ContextMenu("Test Region Switch - Tropical Monsoon")]
		public void TestSwitchToTropicalMonsoon()
		{
			SelectRegion(RegionType.TropicalMonsoon);
		}

		[ContextMenu("Test Region Switch - Semi-Arid Steppe")]
		public void TestSwitchToSemiAridSteppe()
		{
			SelectRegion(RegionType.SemiAridSteppe);
		}

		[ContextMenu("Test Region Switch - Temperate Continental")]
		public void TestSwitchToTemperateContinental()
		{
			SelectRegion(RegionType.TemperateContinental);
		}
	}
}