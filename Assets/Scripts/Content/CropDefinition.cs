using UnityEngine;
using NasaSpaceApps.FarmFromSpace.Common;

namespace NasaSpaceApps.FarmFromSpace.Content
{
	[CreateAssetMenu(fileName = "Crop", menuName = "FarmFromSpace/Content/Crop", order = 0)]
	public class CropDefinition : ScriptableObject
	{
		public string displayName;
	[Header("Visual")]
	public Sprite cropIcon;
	
	[Header("Growth Sprites")]
	[Tooltip("5 sprites for different growth stages: 0=seed, 1=seedling, 2=growing, 3=mature, 4=ready")]
	public Sprite[] growthSprites = new Sprite[5];

		[Tooltip("Yield response multipliers mapped to NDVI phases; overrides BalanceConfig if non-empty.")]
		public float[] stageMultipliersOverride;

		[Header("Economy")]
		[Tooltip("Cost in gold to plant this crop.")]
		public int plantingCost = 15;

		[Header("Growth & Response")]
		[Tooltip("Base weekly growth value used in yield accumulation before multipliers.")]
		public float baseWeeklyGrowth = 1.0f;

		[Tooltip("Optimal soil water fraction for this crop (0-1).")]
		[Range(0f, 1f)] public float optimalSoilWater = 0.7f;

		[Tooltip("Penalty when soil water deviates from optimal. Higher values increase sensitivity.")]
		[Range(0f, 5f)] public float waterDeficitSensitivity = 1.0f;

		[Header("Nitrogen Response")]
		[Tooltip("Additional multiplier if nitrogen is split across two weeks and aligned with active phase.")]
		[Range(0f, 0.5f)] public float splitNBonusOverride = 0.0f;
		
		[Header("Crop Cycle (Endless Mode)")]
		[Tooltip("Growth points needed to reach maturity for harvest.")]
		public float maturityTarget = 100f;
		
		[Tooltip("Maximum age in weeks before crop fails (for Poor suitability).")]
		public int maxAgeWeeks = 20;
		
		[Header("Regional Suitability")]
		[Tooltip("Suitability for Monsoon regions (0=Good, 1=Marginal, 2=Poor).")]
		public int monsoonSuitability = 0;
		
		[Tooltip("Suitability for Semi-Arid Steppe regions (0=Good, 1=Marginal, 2=Poor).")]
		public int steppeSuitability = 0;
		
		[Tooltip("Suitability for Temperate Continental regions (0=Good, 1=Marginal, 2=Poor).")]
		public int temperateSuitability = 0;
	}
}


