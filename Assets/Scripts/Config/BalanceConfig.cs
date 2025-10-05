using UnityEngine;

namespace NasaSpaceApps.FarmFromSpace.Config
{
	[CreateAssetMenu(fileName = "BalanceConfig", menuName = "FarmFromSpace/Balance Config", order = 0)]
	public class BalanceConfig : ScriptableObject
	{
		[Header("Economy")]
		[Tooltip("Starting gold for new games.")]
		public int startingGold = 60;

		[Tooltip("Base cost for irrigation (+10mm water).")]
		public int irrigationCost = 5;

		[Tooltip("Cost for nitrogen Part A application.")]
		public int nitrogenPartACost = 8;

		[Tooltip("Cost for nitrogen Part B application.")]
		public int nitrogenPartBCost = 8;

		[Tooltip("Base cost for purchasing the first plot.")]
		public int basePlotCost = 60;

		[Tooltip("Multiplier for plot cost escalation (1.25 = +25% each purchase).")]
		[Range(1.1f, 2.0f)] public float plotCostMultiplier = 1.25f;

		[Header("Water Balance Penalties")]
		[Tooltip("Gold penalty for runoff when soil is saturated and adding water.")]
		public int runoffPenalty = 3;

		[Tooltip("Gold penalty for leaching when soil is saturated and applying nitrogen.")]
		public int goldLeachingPenalty = 5;

		[Header("Diminishing Irrigation")]
		[Tooltip("Efficiency multiplier for 2nd irrigation per plot per week (0.7 = 70% efficiency).")]
		[Range(0.1f, 1.0f)] public float secondIrrigationEfficiency = 0.7f;

		[Tooltip("Efficiency multiplier for 3rd+ irrigation per plot per week (0.4 = 40% efficiency).")]
		[Range(0.1f, 1.0f)] public float thirdPlusIrrigationEfficiency = 0.4f;

		[Header("Soil & Water")]
		[Tooltip("Maximum soil water capacity as a percentage for each soil texture (0-1). Index by SoilTexture enum.")]
		public float[] fieldCapacityBySoilTexture = new float[] { 0.5f, 0.6f, 0.7f };

		[Tooltip("Runoff threshold as soil tank fraction above which additional water causes runoff.")]
		[Range(0f, 1f)] public float runoffThreshold = 0.85f;

		[Tooltip("Base irrigation efficiency (modified by relics/region).")]
		[Range(0.1f, 1f)] public float baseIrrigationEfficiency = 0.8f;

		[Header("Evapotranspiration (ET)")]
		[Tooltip("Simple linear proxy: weekly ET = a * temperature + b * solar + c. Order: a,b,c")]
		public Vector3 etLinearCoefficients = new Vector3(0.1f, 0.02f, 5f);

		[Header("Diminishing Returns")]
		[Tooltip("Multiplier applied when heavy irrigation occurs on consecutive weeks.")]
		[Range(0.5f, 1f)] public float consecutiveIrrigationPenalty = 0.9f;

		[Header("NDVI Stage Multipliers")]
		[Tooltip("Yield stage multipliers by NDVI phase. Index by NdviPhase enum order.")]
		public float[] stageMultipliers = new float[] { 0.6f, 1.0f, 1.1f, 0.7f };

		[Header("Nitrogen Timing")]
		[Tooltip("Bonus when N is split across two weeks and aligned with active stages.")]
		[Range(0f, 0.5f)] public float splitNitrogenBonus = 0.1f;

		[Tooltip("Penalty applied if heavy N is applied while soil is saturated; applied with 1-2 week lag.")]
		[Range(0f, 0.5f)] public float leachingPenalty = 0.1f;
		
		[Header("Endless Mode")]
		[Tooltip("Maximum number of plots per region.")]
		public int maxPlotsPerRegion = 9;
		
		[Tooltip("Cost to unlock Region 2 (Semi-Arid Steppe).")]
		public int region2UnlockCost = 250;
		
		[Tooltip("Cost to unlock Region 3 (Temperate Continental).")]
		public int region3UnlockCost = 500;
		
		[Header("Crop Suitability")]
		[Tooltip("Growth multiplier for Marginal suitability crops.")]
		[Range(0.5f, 1.0f)] public float marginalSuitabilityMultiplier = 0.8f;
		
		[Tooltip("Growth multiplier for Poor suitability crops.")]
		[Range(0.3f, 0.8f)] public float poorSuitabilityMultiplier = 0.6f;
		
		[Tooltip("Refund percentage when Poor suitability crops fail.")]
		[Range(0.0f, 1.0f)] public float cropFailureRefund = 0.5f;
	}
}


