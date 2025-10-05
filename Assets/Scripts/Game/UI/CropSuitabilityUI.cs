using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NasaSpaceApps.FarmFromSpace.Content;
using NasaSpaceApps.FarmFromSpace.Game.Data;
using NasaSpaceApps.FarmFromSpace.Common;

namespace NasaSpaceApps.FarmFromSpace.Game.UI
{
	public class CropSuitabilityUI : MonoBehaviour
	{
		[Header("UI References")]
		[SerializeField] private TextMeshProUGUI suitabilityText;
		[SerializeField] private Image suitabilityIcon;
		[SerializeField] private TextMeshProUGUI suitabilityDescription;
		
		[Header("Crop Stats (Optional)")]
		[SerializeField] private TextMeshProUGUI growthStatsText; // Shows base growth, optimal soil water, water deficit sensitivity
		[SerializeField] private TextMeshProUGUI regionLabelText; // Shows current region + suitability

		[Header("Suitability Colors")]
		[SerializeField] private Color goodColor = Color.green;
		[SerializeField] private Color marginalColor = Color.yellow;
		[SerializeField] private Color poorColor = Color.red;

		[Header("Suitability Icons")]
		[SerializeField] private Sprite goodIcon;
		[SerializeField] private Sprite marginalIcon;
		[SerializeField] private Sprite poorIcon;

		public void UpdateSuitability(CropDefinition crop, RegionType region)
		{
			if (crop == null)
			{
				HideSuitability();
				return;
			}

			CropSuitability suitability = CalculateSuitability(crop, region);
			ShowSuitability(suitability);
			
			// Populate extra stats if fields are wired
			if (growthStatsText != null)
			{
				growthStatsText.text =
					$"Growth: {crop.baseWeeklyGrowth:F1} GP/wk\n" +
					$"Optimal Soil: {(crop.optimalSoilWater * 100f):F0}%\n" +
					$"Water Sensitivity: {crop.waterDeficitSensitivity:F1}";
			}

			if (regionLabelText != null)
			{
				regionLabelText.text = $"Region: {region} ({suitability})";
			}
		}

		private CropSuitability CalculateSuitability(CropDefinition crop, RegionType region)
		{
			int suitabilityValue = 0;
			switch (region)
			{
				case RegionType.TropicalMonsoon:
					suitabilityValue = crop.monsoonSuitability;
					break;
				case RegionType.SemiAridSteppe:
					suitabilityValue = crop.steppeSuitability;
					break;
				case RegionType.TemperateContinental:
					suitabilityValue = crop.temperateSuitability;
					break;
			}
			
			return (CropSuitability)suitabilityValue;
		}

		private void ShowSuitability(CropSuitability suitability)
		{
			gameObject.SetActive(true);

			switch (suitability)
			{
				case CropSuitability.Good:
					SetSuitabilityDisplay("Good", "Normal growth and standard water/N responses.", goodColor, goodIcon);
					break;
				case CropSuitability.Marginal:
					SetSuitabilityDisplay("Marginal", "-20% growth rate; stronger sensitivity to water and timing mistakes.", marginalColor, marginalIcon);
					break;
				case CropSuitability.Poor:
					SetSuitabilityDisplay("Poor", "Seedling may fail within 2-3 weeks unless conditions are compensated. On fail, refund 50% of planting cost.", poorColor, poorIcon);
					break;
			}
		}

		private void SetSuitabilityDisplay(string title, string description, Color color, Sprite icon)
		{
			if (suitabilityText != null)
			{
				suitabilityText.text = title;
				suitabilityText.color = color;
			}

			if (suitabilityDescription != null)
			{
				suitabilityDescription.text = description;
			}

			if (suitabilityIcon != null && icon != null)
			{
				suitabilityIcon.sprite = icon;
				suitabilityIcon.color = color;
			}
		}

		private void HideSuitability()
		{
			gameObject.SetActive(false);
		}
	}
}
