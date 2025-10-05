using UnityEngine;
using NasaSpaceApps.FarmFromSpace.Game.Data;
using NasaSpaceApps.FarmFromSpace.Game.Systems;
using NasaSpaceApps.FarmFromSpace.Content;

namespace NasaSpaceApps.FarmFromSpace.Game.UI
{
	/// <summary>
	/// Handles the visual representation of crop growth stages on a plot
	/// </summary>
	public class CropGrowthVisualizer : MonoBehaviour
	{
		[Header("Growth Sprites")]
		[SerializeField] private Sprite[] growthSprites = new Sprite[5]; // 0=seed, 1=seedling, 2=growing, 3=mature, 4=ready
		[SerializeField] private SpriteRenderer cropSpriteRenderer;
		[SerializeField] private GameObject cropVisualParent; // Parent object that can be enabled/disabled

		[Header("References")]
		[SerializeField] private EndlessGameRunner gameRunner;
		[SerializeField] private int plotIndex;

		private EndlessTurnManager turnManager;
		private PlotState currentPlot;
		private int lastGrowthStage = -1;

		private void Start()
		{
			if (gameRunner == null)
				gameRunner = FindFirstObjectByType<EndlessGameRunner>();

			// Hide crop visual initially
			if (cropVisualParent != null)
				cropVisualParent.SetActive(false);
		}

		private void Update()
		{
			if (gameRunner == null) return;

			turnManager = gameRunner.GetTurnManager();
			if (turnManager == null) return;

			// Get current plot data
			if (plotIndex < turnManager.Plots.Count)
			{
				currentPlot = turnManager.Plots[plotIndex];
				UpdateCropVisual();
			}
		}

		private void UpdateCropVisual()
		{
			if (currentPlot == null)
			{
				HideCropVisual();
				return;
			}

			// Check if plot has a crop planted
			if (currentPlot.plantedWeek < 0 || currentPlot.plantedCrop == null)
			{
				HideCropVisual();
				return;
			}

			// Update sprites if crop changed
			if (currentPlot.plantedCrop != null)
			{
				UpdateCropSprites(currentPlot.plantedCrop);
			}

			// Calculate growth stage
			int growthStage = CalculateGrowthStage(currentPlot);
			
			// Only update if growth stage changed
			if (growthStage != lastGrowthStage)
			{
				UpdateGrowthSprite(growthStage);
				lastGrowthStage = growthStage;
			}
		}

		private int CalculateGrowthStage(PlotState plot)
		{
			if (plot.plantedCrop == null) return 0;

			// Use actual yield accumulation instead of time-based calculation
			// This matches the actual growth system in EndlessTurnManager
			float growthProgress = plot.yieldAccumulated / plot.maturityTarget;
			growthProgress = Mathf.Clamp01(growthProgress);

			// Determine growth stage based on progress
			if (growthProgress < 0.2f)
				return 0; // Seed
			else if (growthProgress < 0.4f)
				return 1; // Seedling
			else if (growthProgress < 0.7f)
				return 2; // Growing
			else if (growthProgress < 1.0f)
				return 3; // Mature
			else
				return 4; // Ready for harvest
		}

		private void UpdateGrowthSprite(int growthStage)
		{
			// Show crop visual
			if (cropVisualParent != null)
			{
				cropVisualParent.SetActive(true);
			}

			// Update sprite
			if (cropSpriteRenderer != null && growthStage >= 0 && growthStage < growthSprites.Length)
			{
				if (growthSprites[growthStage] != null)
				{
					cropSpriteRenderer.sprite = growthSprites[growthStage];
				}
			}
		}

		private void HideCropVisual()
		{
			if (cropVisualParent != null)
				cropVisualParent.SetActive(false);
			
			lastGrowthStage = -1;
		}

		public void SetPlotIndex(int index)
		{
			plotIndex = index;
		}

		public void SetGrowthSprites(Sprite[] sprites)
		{
			if (sprites != null && sprites.Length == 5)
			{
				growthSprites = sprites;
			}
		}

		// Method to update sprites when crop changes
		private void UpdateCropSprites(CropDefinition crop)
		{
			if (crop == null || crop.growthSprites == null || crop.growthSprites.Length != 5) 
			{
				Debug.LogWarning($"Invalid growth sprites for crop: {crop?.displayName}");
				return;
			}

			// Check if sprites have changed to avoid unnecessary updates
			bool spritesChanged = false;
			for (int i = 0; i < 5; i++)
			{
				if (growthSprites[i] != crop.growthSprites[i])
				{
					spritesChanged = true;
					break;
				}
			}

			if (spritesChanged)
			{
				growthSprites = crop.growthSprites;
			}
		}

		// Method to manually set crop sprites based on crop type
		public void SetCropSprites(CropDefinition crop)
		{
			if (crop == null) return;
			UpdateCropSprites(crop);
		}

	}
}
