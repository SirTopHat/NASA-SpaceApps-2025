using UnityEngine;
using NasaSpaceApps.FarmFromSpace.Common;

namespace NasaSpaceApps.FarmFromSpace.Content
{
	public enum RelicModifierTarget
	{
		IrrigationEfficiency,
		RunoffSeverityDelta,
		ForecastRangeNarrowing,
		FrostImpactReduction,
		SplitNitrogenBonus
	}

	[System.Serializable]
	public struct RelicModifier
	{
		public RelicModifierTarget target;
		public float value;
		[Tooltip("If true, value is added; if false, value is treated as a multiplier.")]
		public bool additive;
	}

	[CreateAssetMenu(fileName = "Relic", menuName = "FarmFromSpace/Content/Relic", order = 0)]
	public class RelicDefinition : ScriptableObject
	{
		public string displayName;
		public RelicType relicType;
		[TextArea]
		public string description;

		[Header("Modifiers")]
		public RelicModifier[] modifiers;

		[Header("Region Affinity (optional)")]
		public RegionType[] preferredRegions;
	}
}


