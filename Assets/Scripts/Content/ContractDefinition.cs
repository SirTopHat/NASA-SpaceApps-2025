using UnityEngine;
using NasaSpaceApps.FarmFromSpace.Common;

namespace NasaSpaceApps.FarmFromSpace.Content
{
	public enum ContractMetric
	{
		Yield,
		WaterUseEfficiency,
		RunoffEvents,
		TimingIQ
	}

	[System.Serializable]
	public struct ContractThreshold
	{
		public ContractMetric metric;
		public float target;
		[Tooltip("If true, player must be <= target; else must be >= target.")]
		public bool lessOrEqual;
	}

	[CreateAssetMenu(fileName = "Contract", menuName = "FarmFromSpace/Content/Contract", order = 0)]
	public class ContractDefinition : ScriptableObject
	{
		public string displayName;
		public ContractType contractType;
		public RegionType[] applicableRegions;

		[TextArea]
		public string description;

		[Header("Targets")]
		public ContractThreshold[] thresholds;
	}
}


