using UnityEngine;
using NasaSpaceApps.FarmFromSpace.Common;

namespace NasaSpaceApps.FarmFromSpace.Content
{
	public enum EventEffectTarget
	{
		RainVariance,
		MaxIrrigationCap,
		ETSpike,
		NdviDelay,
		StageRegression,
		FertilizerCostIncrease
	}

	[System.Serializable]
	public struct EventEffect
	{
		public EventEffectTarget target;
		public float value;
		public int durationWeeks;
	}

	[CreateAssetMenu(fileName = "Event", menuName = "FarmFromSpace/Content/Event", order = 0)]
	public class EventDefinition : ScriptableObject
	{
		public string displayName;
		public GameEventType eventType;
		public RegionType[] applicableRegions;

		[TextArea]
		public string description;

		[Header("Effects")]
		public EventEffect[] effects;

		[Header("Triggering")]
		[Tooltip("Optional growth stage constraint for this event to be eligible.")]
		public NdviPhase[] stageConstraint;
	}
}


