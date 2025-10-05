using UnityEngine;
using TMPro;
using NasaSpaceApps.FarmFromSpace.Game.Systems;
using NasaSpaceApps.FarmFromSpace.Game.Data;
using NasaSpaceApps.FarmFromSpace.Common;

namespace NasaSpaceApps.FarmFromSpace.Game.UI
{
	/// <summary>
	/// NASA Data Card UI component that displays current week's satellite data
	/// with resolution and cadence information for educational purposes
	/// </summary>
	public class GameUINasaDataCard : MonoBehaviour
	{
		[Header("Data Card Display")]
		[SerializeField] private TextMeshProUGUI dataCardText;
		[SerializeField] private bool showResolutionChips = true;
		[SerializeField] private bool showCadenceChips = true;
		[SerializeField] private bool isDataCardVisible = true;

		[Header("References")]
		[SerializeField] private GameRunner gameRunner;

		private TurnManager turnManager;

		private void Start()
		{
			// Get turn manager reference
			if (gameRunner != null)
			{
				turnManager = gameRunner.GetTurnManager();
			}
		}

		private void Update()
		{
			// Update turn manager reference in case of restart
			if (turnManager == null && gameRunner != null)
			{
				turnManager = gameRunner.GetTurnManager();
			}

			// Update data card display
			if (turnManager != null && dataCardText != null)
			{
				UpdateDataCardDisplay();
			}
		}

		private void UpdateDataCardDisplay()
		{
			if (turnManager == null || dataCardText == null) return;

			if (isDataCardVisible)
			{
				var env = turnManager.Env;
				string dataCard = FormatNasaDataCard(env);
				dataCardText.text = dataCard;
			}
			else
			{
				dataCardText.text = ""; // Hide the data card
			}
		}

		private string FormatNasaDataCard(WeeklyEnvironment env)
		{
			// Format rain data
			string rainData = FormatRainData(env.rainMm);
			
			// Format ET data  
			string etData = FormatETData(env.etMm);
			
			// Format NDVI stage data
			string stageData = FormatStageData(env.ndviPhase);
			
			// Format water balance estimation
			string waterBalanceData = FormatWaterBalanceData(env);

			// Combine all data
			return $"{rainData}\n{etData}\n{stageData}\n{waterBalanceData}";
		}

		private string FormatRainData(float rainMm)
		{
			string intensity = GetRainIntensity(rainMm);
			string resolution = showResolutionChips ? " [10km res, 30min cadence]" : "";
			return $"Rain: {intensity} ({rainMm:F1}mm){resolution}";
		}

		private string FormatETData(float etMm)
		{
			string intensity = GetETIntensity(etMm);
			string cadence = showCadenceChips ? " [Daily updates]" : "";
			return $"ET: {intensity} ({etMm:F1}mm){cadence}";
		}

		private string FormatStageData(Common.NdviPhase ndviPhase)
		{
			string stage = GetStageName(ndviPhase);
			string resolution = showResolutionChips ? " [250m res, 16-day cadence]" : "";
			return $"Stage: {stage}{resolution}";
		}

		private string GetRainIntensity(float rainMm)
		{
			if (rainMm <= 0f) return "None";
			if (rainMm <= 10f) return "Light";
			if (rainMm <= 25f) return "Moderate";
			return "Heavy";
		}

		private string GetETIntensity(float etMm)
		{
			if (etMm <= 8f) return "Low";
			if (etMm <= 12f) return "Medium";
			return "High";
		}

		private string GetStageName(Common.NdviPhase ndviPhase)
		{
			return ndviPhase switch
			{
				Common.NdviPhase.Dormant => "Dormant",
				Common.NdviPhase.GreenUp => "Green-Up",
				Common.NdviPhase.Peak => "Peak Growth",
				Common.NdviPhase.Senescence => "Senescence",
				_ => "Unknown"
			};
		}

		private string FormatWaterBalanceData(WeeklyEnvironment env)
		{
			if (turnManager == null) return "Water Balance: Unable to calculate";

			// Calculate estimated water balance
			float netWaterChange = CalculateEstimatedWaterBalance(env);
			float percentageChange = CalculatePercentageChange(env);
			
			string changeType = netWaterChange > 0 ? "Gain" : (netWaterChange < 0 ? "Loss" : "Neutral");
			string changeValue = netWaterChange > 0 ? $"+{netWaterChange:F1}mm" : $"{netWaterChange:F1}mm";
			string percentageValue = percentageChange > 0 ? $"+{percentageChange:F1}%" : $"{percentageChange:F1}%";
			
			string resolution = showResolutionChips ? " [Modeled]" : "";
			
			return $"Water Balance: {changeType} ({changeValue}, {percentageValue}){resolution}";
		}

		private float CalculateEstimatedWaterBalance(WeeklyEnvironment env)
		{
			// Get current soil state
			float currentSoilTank = turnManager.State.soilTank;
			float fieldCapacity = GetFieldCapacity();
			
			// Calculate water inputs (rain only, no irrigation in estimation)
			float waterIn = env.rainMm;
			float waterOut = env.etMm;
			
			// Calculate net change
			float netChange = waterIn - waterOut;
			
			// Apply soil capacity constraints
			float currentSoilMm = currentSoilTank * fieldCapacity * 100f;
			float newSoilMm = Mathf.Clamp(currentSoilMm + netChange, 0f, fieldCapacity * 100f);
			
			// Return the actual change that would occur
			return newSoilMm - currentSoilMm;
		}

		private float GetFieldCapacity()
		{
			if (turnManager == null) return 0.6f; // Default loam capacity
			
			// Get soil texture from turn manager (assuming it has this property)
			// For now, use a default value - this might need adjustment based on your TurnManager implementation
			return 0.6f; // Default loam field capacity
		}

		private float CalculatePercentageChange(WeeklyEnvironment env)
		{
			// Get current soil state
			float currentSoilTank = turnManager.State.soilTank;
			float fieldCapacity = GetFieldCapacity();
			
			// Calculate water inputs (rain only, no irrigation in estimation)
			float waterIn = env.rainMm;
			float waterOut = env.etMm;
			
			// Calculate net change
			float netChange = waterIn - waterOut;
			
			// Apply soil capacity constraints
			float currentSoilMm = currentSoilTank * fieldCapacity * 100f;
			float newSoilMm = Mathf.Clamp(currentSoilMm + netChange, 0f, fieldCapacity * 100f);
			
			// Calculate new soil fraction
			float newSoilFraction = (fieldCapacity <= 0f) ? 0f : newSoilMm / (fieldCapacity * 100f);
			
			// Calculate percentage change
			float percentageChange = (newSoilFraction - currentSoilTank) * 100f;
			
			return percentageChange;
		}

		// Public methods for manual control
		public void SetResolutionChipsEnabled(bool enabled)
		{
			showResolutionChips = enabled;
		}

		public void SetCadenceChipsEnabled(bool enabled)
		{
			showCadenceChips = enabled;
		}

		public void RefreshDisplay()
		{
			UpdateDataCardDisplay();
		}

		// Toggle methods for button integration
		public void ToggleDataCard()
		{
			isDataCardVisible = !isDataCardVisible;
			UpdateDataCardDisplay();
		}

		public void ShowDataCard()
		{
			isDataCardVisible = true;
			UpdateDataCardDisplay();
		}

		public void HideDataCard()
		{
			isDataCardVisible = false;
			UpdateDataCardDisplay();
		}

		public bool IsDataCardVisible => isDataCardVisible;

		/// <summary>
		/// Called when the region changes to update the data card with new region data
		/// </summary>
		public void OnRegionChanged(RegionType newRegion, TurnManager newTurnManager)
		{
			Debug.Log($"GameUINasaDataCard: Region changed to {newRegion}");
			
			// Update the turn manager reference
			turnManager = newTurnManager;
			
			// Force refresh the display with new data
			RefreshDisplay();
			
			Debug.Log($"GameUINasaDataCard: Updated with new region data for {newRegion}");
		}
	}
}
