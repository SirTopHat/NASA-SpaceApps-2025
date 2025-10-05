using UnityEngine;
using NasaSpaceApps.FarmFromSpace.Common;

namespace NasaSpaceApps.FarmFromSpace.Game.Data
{
	[System.Serializable]
	public struct WeeklyEnvironment
	{
		public float rainMm;
		public float etMm;
		public NdviPhase ndviPhase;
	}

	[System.Serializable]
	public struct WeeklyState
	{
		[Range(0f, 1f)] public float soilTank; // 0-1 fraction of capacity
		public int runoffEventsThisSeason;
		public float yieldAccumulated;
		public int weekIndex;
	}
}


