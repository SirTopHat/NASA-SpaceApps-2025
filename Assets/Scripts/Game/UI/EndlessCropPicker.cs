using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using NasaSpaceApps.FarmFromSpace.Content;
using NasaSpaceApps.FarmFromSpace.Game.Systems;
using NasaSpaceApps.FarmFromSpace.Game.Data;
using NasaSpaceApps.FarmFromSpace.Common;
using System.Collections.Generic;

namespace NasaSpaceApps.FarmFromSpace.Game.UI
{
	public class EndlessCropPicker : MonoBehaviour
	{
		[Header("UI References")]
		[SerializeField] private Transform cropButtonParent;
		[SerializeField] private GameObject cropButtonPrefab;
		[SerializeField] private TextMeshProUGUI selectedCropInfo;
		[SerializeField] private CropSuitabilityUI suitabilityUI;
		[SerializeField] private GameObject cropPickerPanel; // Main panel that gets shown/hidden
        [SerializeField] private UnityEngine.UI.Button closeButton; // button to close the panel

        [Header("Universal Crops")]
		[SerializeField] private CropDefinition[] universalCrops;

        [Header("Grid Layout")]
        [SerializeField] private int columns = 3;
        [SerializeField] private int rows = 2;
        [SerializeField] private float buttonSpacing = 10f;

        [Header("References")]
		[SerializeField] private EndlessGameRunner gameRunner;

		private EndlessTurnManager turnManager;
		private List<GameObject> cropButtons = new List<GameObject>();
		private bool isVisible = false;
		private CropDefinition currentlyHoveredCrop = null;

		private void Start()
		{
			if (gameRunner == null)
				gameRunner = FindFirstObjectByType<EndlessGameRunner>();

			CreateCropButtons();
			
			// Start hidden
			HideCropPicker();

            // Setup close button
            if (closeButton != null) {
                closeButton.onClick.AddListener(HideCropPicker);
            }
        }

		private void Update()
		{
			if (gameRunner == null) return;

			turnManager = gameRunner.GetTurnManager();
			if (turnManager == null) return;

			UpdateCropButtons();
		}

        private void CreateCropButtons() {
            // Clear existing buttons
            foreach (var button in cropButtons) {
                if (button != null) DestroyImmediate(button);
            }
            cropButtons.Clear();

            // Create buttons for each universal crop
            for (int i = 0; i < universalCrops.Length; i++) {
                var crop = universalCrops[i];
                if (crop == null) continue;

                GameObject buttonObj = Instantiate(cropButtonPrefab, cropButtonParent);
                cropButtons.Add(buttonObj);

                // Setup button
                Button button = buttonObj.GetComponent<Button>();
                if (button != null) {
                    button.onClick.AddListener(() => OnCropSelected(crop));
                }

                // Add hover events
                var eventTrigger = buttonObj.GetComponent<EventTrigger>();
                if (eventTrigger == null) {
                    eventTrigger = buttonObj.AddComponent<EventTrigger>();
                }

                // Hover enter event
                var hoverEnter = new EventTrigger.Entry();
                hoverEnter.eventID = EventTriggerType.PointerEnter;
                hoverEnter.callback.AddListener((eventData) => OnCropHoverEnter(crop));
                eventTrigger.triggers.Add(hoverEnter);

                // Hover exit event
                var hoverExit = new EventTrigger.Entry();
                hoverExit.eventID = EventTriggerType.PointerExit;
                hoverExit.callback.AddListener((eventData) => OnCropHoverExit());
                eventTrigger.triggers.Add(hoverExit);

                // Setup button content
                SetupCropButton(buttonObj, crop);

                // Position buttons in grid layout
                var rectTransform = buttonObj.GetComponent<RectTransform>();
                if (rectTransform != null) {
                    // Calculate grid position
                    int row = i / columns;
                    int col = i % columns;

                    // Calculate position (centered grid)
                    float buttonWidth = 120f; // Adjust based on your button size
                    float buttonHeight = 120f;

                    float startX = -(columns - 1) * (buttonWidth + buttonSpacing) * 0.5f;
                    float startY = (rows - 1) * (buttonHeight + buttonSpacing) * 0.5f;

                    float x = startX + col * (buttonWidth + buttonSpacing);
                    float y = startY - row * (buttonHeight + buttonSpacing);

                    rectTransform.anchoredPosition = new Vector2(x, y);
                    rectTransform.sizeDelta = new Vector2(buttonWidth, buttonHeight);
                }
            }
        }

        private void SetupCropButton(GameObject buttonObj, CropDefinition crop) {
            // Set crop icon directly on the button's main image
            Image buttonImage = buttonObj.GetComponent<Image>();
            if (buttonImage != null && crop.cropIcon != null) {
                buttonImage.sprite = crop.cropIcon;
            }

            // Set crop name
            TextMeshProUGUI nameText = buttonObj.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null) {
                nameText.text = crop.displayName;
            }

            // Set cost
            TextMeshProUGUI costText = buttonObj.transform.Find("Cost")?.GetComponent<TextMeshProUGUI>();
            if (costText != null) {
                costText.text = $"{crop.plantingCost} Gold";
            }
        }

        private void UpdateCropButtons()
		{
			// Update button states based on affordability and plot availability
			for (int i = 0; i < cropButtons.Count && i < universalCrops.Length; i++)
			{
				var crop = universalCrops[i];
				var buttonObj = cropButtons[i];
				
				if (buttonObj == null || crop == null) continue;

				Button button = buttonObj.GetComponent<Button>();
				if (button == null) continue;

				// Check if we can afford this crop
				bool canAfford = turnManager.Gold >= crop.plantingCost;
				
				// Check if selected plot is available for planting
				bool plotAvailable = false;
				if (turnManager.Plots.Count > turnManager.SelectedPlotIndex)
				{
					var plot = turnManager.Plots[turnManager.SelectedPlotIndex];
					plotAvailable = plot.plantedWeek < 0; // Not planted
				}

				button.interactable = canAfford && plotAvailable;

				// Update visual state
				UpdateButtonVisuals(buttonObj, canAfford, plotAvailable);
			}
		}

		private void UpdateButtonVisuals(GameObject buttonObj, bool canAfford, bool plotAvailable)
		{
			// Update button color based on state
			Image buttonImage = buttonObj.GetComponent<Image>();
			if (buttonImage == null) return;

			if (!plotAvailable)
			{
				buttonImage.color = Color.gray; // Plot not available
			}
			else if (!canAfford)
			{
				buttonImage.color = Color.red; // Can't afford
			}
			else
			{
				buttonImage.color = Color.white; // Available
			}
		}

		private void OnCropHoverEnter(CropDefinition crop)
		{
			currentlyHoveredCrop = crop;
			UpdateSuitabilityDisplay(crop);
		}

		private void OnCropHoverExit()
		{
			currentlyHoveredCrop = null;
			HideSuitabilityDisplay();
		}
		
		public void RefreshCropButtons()
		{
			// Force refresh of crop button states
			UpdateCropButtons();
		}

		private void UpdateSuitabilityDisplay(CropDefinition crop)
		{
			if (suitabilityUI != null && turnManager != null)
			{
				RegionType currentRegion = turnManager.GetCurrentRegion();
				suitabilityUI.UpdateSuitability(crop, currentRegion);
			}

			// Update selected crop info
			if (selectedCropInfo != null)
			{
				selectedCropInfo.text = $"Cost: {crop.plantingCost} Gold\nMaturity: {crop.maturityTarget} GP";
			}
		}

		private void HideSuitabilityDisplay()
		{
			if (suitabilityUI != null)
			{
				suitabilityUI.gameObject.SetActive(false);
			}

			// Clear selected crop info
			if (selectedCropInfo != null)
			{
				selectedCropInfo.text = "";
			}
		}

		private void OnCropSelected(CropDefinition crop)
		{
			if (turnManager == null) return;

			// Plant the selected crop
			turnManager.PlantSelectedPlot(crop);
			SoundManager.Instance.PlaySfx("planting");

			// Hide the crop picker after selection
			HideCropPicker();
		}

		public void ShowCropPicker()
		{
			// Show the crop picker panel
			if (cropPickerPanel != null)
			{
				cropPickerPanel.SetActive(true);
			}
			else
			{
				// Fallback: show the whole GameObject
				gameObject.SetActive(true);
			}
			isVisible = true;
			
			// Hide suitability display when showing picker
			HideSuitabilityDisplay();
			
			//Debug.Log("Crop picker shown");
		}

		public void HideCropPicker()
		{
			// Hide the crop picker panel
			if (cropPickerPanel != null)
			{
				cropPickerPanel.SetActive(false);
			}
			else
			{
				// Fallback: hide the whole GameObject
				gameObject.SetActive(false);
			}
			isVisible = false;
			
			// Clear hovered crop and hide suitability display
			currentlyHoveredCrop = null;
			HideSuitabilityDisplay();
			
			//Debug.Log("Crop picker hidden");
		}

		public bool IsVisible => isVisible;
	}
}
