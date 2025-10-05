using UnityEngine;
using TMPro;
using NasaSpaceApps.FarmFromSpace.Game.Systems;
using NasaSpaceApps.FarmFromSpace.Game.Data;
using NasaSpaceApps.FarmFromSpace.Game;
using NasaSpaceApps.FarmFromSpace.Common;

namespace NasaSpaceApps.FarmFromSpace.Game.UI
{
	/// <summary>
	/// NASA Data Card UI component for endless mode that displays current week's satellite data
	/// with resolution and cadence information for educational purposes
	/// </summary>
	public class EndlessNasaDataCard : MonoBehaviour
	{
	[Header("Data Card Display")]
	[SerializeField] private GameObject dataCardMenu; // The menu GameObject to show/hide
	[SerializeField] private TextMeshProUGUI dataCardText;
	[SerializeField] private bool showResolutionChips = true;
	[SerializeField] private bool showCadenceChips = true;
	[SerializeField] private bool isDataCardVisible = false;

		[Header("References")]
		[SerializeField] private EndlessGameRunner gameRunner;

		private EndlessTurnManager turnManager;

		private void Start()
		{
			// Get turn manager reference
			if (gameRunner != null)
			{
				turnManager = gameRunner.GetTurnManager();
			}

			// Initialize menu state
			InitializeMenuState();
		}

		private void InitializeMenuState()
		{
			// Set initial menu visibility
			if (dataCardMenu != null)
			{
				dataCardMenu.SetActive(isDataCardVisible);
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
			if (turnManager == null) 
			{
				Debug.LogWarning("EndlessNasaDataCard: TurnManager is null, cannot update display");
				return;
			}

			// Show/hide the menu
			if (dataCardMenu != null)
			{
				dataCardMenu.SetActive(isDataCardVisible);
			}

			// Update the text content when visible
			if (isDataCardVisible && dataCardText != null)
			{
				try
				{
					var env = turnManager.Env;
					// Check if environment data is valid (structs can't be null, so check for reasonable values)
					if (IsValidEnvironmentData(env))
					{
						string dataCard = FormatNasaDataCard(env);
						dataCardText.text = dataCard;
						Debug.Log($"EndlessNasaDataCard: Updated display with data - Rain: {env.rainMm:F1}mm, ET: {env.etMm:F1}mm, Phase: {env.ndviPhase}");
					}
					else
					{
						Debug.LogWarning("EndlessNasaDataCard: Environment data is invalid or not loaded");
						dataCardText.text = "NASA Data: Loading...";
					}
				}
				catch (System.Exception e)
				{
					Debug.LogError($"EndlessNasaDataCard: Error updating display: {e.Message}");
					dataCardText.text = "NASA Data: Error loading data";
				}
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
			return $"{rainData}\n\n{etData}\n\n{stageData}\n\n{waterBalanceData}";
		}

		private string FormatRainData(float rainMm)
		{
			string rainLevel = rainMm < 5f ? "None" : (rainMm < 15f ? "Light" : "Heavy");
			string resolutionChip = showResolutionChips ? " [~10km]" : "";
			string cadenceChip = showCadenceChips ? " [~30min]" : "";
			
			return $"Rain (IMERG): {rainLevel} ({rainMm:F1}mm){resolutionChip}{cadenceChip}";
		}

		private string FormatETData(float etMm)
		{
			string etLevel = etMm < 10f ? "Low" : (etMm < 15f ? "Medium" : "High");
			string resolutionChip = showResolutionChips ? " [~9-36km]" : "";
			
			return $"ET (POWER): {etLevel} ({etMm:F1}mm){resolutionChip}";
		}

		private string FormatStageData(NdviPhase phase)
		{
			string stageName = phase switch
			{
				NdviPhase.Dormant => "Dormant",
				NdviPhase.GreenUp => "Green-up",
				NdviPhase.Peak => "Peak",
				NdviPhase.Senescence => "Senescence",
				_ => "Unknown"
			};
			
			string resolutionChip = showResolutionChips ? " [~9-36km]" : "";
			string cadenceChip = showCadenceChips ? " [~16-day]" : "";
			
			return $"Growth Stage (NDVI): {stageName}{resolutionChip}{cadenceChip}";
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
			
			string resolutionChip = showResolutionChips ? " [~9-36km]" : "";
			string cadenceChip = showCadenceChips ? " [Modeled]" : "";
			
			return $"Water Balance (SMAP): {changeType} ({changeValue}, {percentageValue}){resolutionChip}{cadenceChip}";
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
			// For now, use a default value - this might need adjustment based on your EndlessTurnManager implementation
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

		// Public methods for external control
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

		/// <summary>
		/// Called when the region changes to update the data card with new region data
		/// </summary>
		public void OnRegionChanged(RegionType newRegion, EndlessTurnManager newTurnManager)
		{
			Debug.Log($"EndlessNasaDataCard: Region changed to {newRegion}");
			
			// Update the turn manager reference
			turnManager = newTurnManager;
			
			// Force refresh the display with new data
			RefreshDisplay();
			
			// Force a data refresh to ensure we get the latest NASA data
			ForceDataRefresh();
			
			Debug.Log($"EndlessNasaDataCard: Updated with new region data for {newRegion}");
		}

		/// <summary>
		/// Forces a refresh of the NASA data to ensure latest data is displayed
		/// </summary>
		public void ForceDataRefresh()
		{
			Debug.Log("EndlessNasaDataCard: Forcing data refresh...");
			
			// Wait a frame to ensure the turn manager has been updated
			StartCoroutine(RefreshDataAfterFrame());
		}

		private System.Collections.IEnumerator RefreshDataAfterFrame()
		{
			// Wait one frame to ensure turn manager is fully updated
			yield return null;
			
			// Force refresh the display
			RefreshDisplay();
			
			Debug.Log("EndlessNasaDataCard: Data refresh completed");
		}

		/// <summary>
		/// Checks if the environment data is valid and contains reasonable values
		/// </summary>
		private bool IsValidEnvironmentData(WeeklyEnvironment env)
		{
			// Check for reasonable data ranges
			if (env.rainMm < 0 || env.rainMm > 200) return false; // Rain should be 0-200mm
			if (env.etMm < 0 || env.etMm > 50) return false; // ET should be 0-50mm
			
			// Check if NDVI phase is valid
			if (!System.Enum.IsDefined(typeof(NdviPhase), env.ndviPhase)) return false;
			
			return true;
		}
	}
}
