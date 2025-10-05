using UnityEngine;
using NasaSpaceApps.FarmFromSpace.Common;

namespace NasaSpaceApps.FarmFromSpace.Content
{
	[CreateAssetMenu(fileName = "Region", menuName = "FarmFromSpace/Content/Region", order = 0)]
	public class RegionDefinition : ScriptableObject
	{
		public string displayName;
		public RegionType regionType;

		[Header("Crops")]
		public CropDefinition[] availableCrops;

		[Header("Environment")]
		public SoilTexture dominantSoilTexture = SoilTexture.Loam;

		[Tooltip("Modifier applied to irrigation efficiency for this region (e.g., Steppe drip kit synergy).")]
		[Range(0.5f, 1.5f)] public float irrigationEfficiencyModifier = 1.0f;

		[Tooltip("Baseline weekly ET adjustment (adds to ET calculation).")]
		public float etBias = 0f;
	}
}


