using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NasaSpaceApps.FarmFromSpace.Content;

namespace NasaSpaceApps.FarmFromSpace.Game.UI
{
	public class GameUICropPicker : MonoBehaviour
	{
		[SerializeField] private Transform buttonContainer;
		[SerializeField] private GameObject buttonPrefab;
		[SerializeField] private GameRunner runner;
		[SerializeField] private RegionDefinition regionDefinition;
		[SerializeField] private GameObject panel; // the main panel that shows/hides
		[SerializeField] private UnityEngine.UI.Button closeButton; // button to close the panel
		
		[Header("Grid Layout")]
		[SerializeField] private int columns = 3;
		[SerializeField] private int rows = 2;
		[SerializeField] private float buttonSpacing = 10f;

		private void Start()
		{
			PopulateCropButtons();
			HidePanel(); // start hidden
			
			// Setup close button
			if (closeButton != null)
			{
				closeButton.onClick.AddListener(HidePanel);
			}
		}

		private void PopulateCropButtons()
		{
			if (regionDefinition == null || regionDefinition.availableCrops == null) return;

			// Clear existing buttons
			foreach (Transform child in buttonContainer)
			{
				Destroy(child.gameObject);
			}

			// Debug: log the number of crops
			//Debug.Log($"Creating {regionDefinition.availableCrops.Length} crop buttons in {columns}x{rows} grid");

			// Create buttons for each available crop in grid layout
			for (int i = 0; i < regionDefinition.availableCrops.Length; i++)
			{
				var crop = regionDefinition.availableCrops[i];
				//Debug.Log($"Creating button for crop: {crop.displayName}");
				
				GameObject buttonObj;
				if (buttonPrefab != null)
				{
					buttonObj = Instantiate(buttonPrefab, buttonContainer);
					// Update ONLY the text content, preserve all other properties
					var text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
					if (text != null)
					{
						text.text = crop.displayName; // Only change the text, keep position, color, font, etc.

					}
					
					// Update image if the prefab has one
					var image = buttonObj.GetComponentInChildren<UnityEngine.UI.Image>();
					if (image != null && crop.cropIcon != null)
					{
						image.sprite = crop.cropIcon;
					}
				}
				else
				{
					buttonObj = new GameObject($"CropButton_{crop.displayName}");
					buttonObj.transform.SetParent(buttonContainer);
					var button = buttonObj.AddComponent<Button>();
					var text = buttonObj.AddComponent<TextMeshProUGUI>();
					text.text = crop.displayName;
					text.color = Color.black;
					button.targetGraphic = text;
				}

				// Position buttons in grid layout
				var rectTransform = buttonObj.GetComponent<RectTransform>();
				if (rectTransform != null)
				{
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

				var buttonComponent = buttonObj.GetComponent<Button>();
				if (buttonComponent != null)
				{
					buttonComponent.onClick.AddListener(() => PlantCrop(crop));
				}
			}
		}

		private void PlantCrop(CropDefinition crop)
		{
			if (runner != null)
			{
				runner.UI_PlantCrop(crop);
				HidePanel(); // hide after planting
			}
		}

		public void ShowPanel()
		{
			if (panel != null)
			{
				panel.SetActive(true);
				// Disable raycast on plot views to prevent double-clicks
				if (runner != null)
				{
					runner.SetPlotRaycastEnabled(false);
				}
			}
		}

		public void HidePanel()
		{
			if (panel != null)
			{
				panel.SetActive(false);
				// Re-enable raycast on plot views
				if (runner != null)
				{
					runner.SetPlotRaycastEnabled(true);
				}
			}
		}

		// Method to show panel when "Plant" button is clicked
		public void ShowPanelForPlanting()
		{
			if (runner != null && runner.GetTurnManager() != null)
			{
				var turnManager = runner.GetTurnManager();
				// Only show if selected plot is not planted
				if (turnManager.SelectedPlotIndex < turnManager.Plots.Count && 
				    turnManager.Plots[turnManager.SelectedPlotIndex].plantedWeek < 0)
				{
					ShowPanel();
				}
			}
		}
	}
}
