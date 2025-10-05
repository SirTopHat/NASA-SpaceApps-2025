using UnityEngine;
using NasaSpaceApps.FarmFromSpace.Common;
using NasaSpaceApps.FarmFromSpace.Game.Data;

namespace NasaSpaceApps.FarmFromSpace.Game.Services
{
	/// <summary>
	/// Legacy DataService that provides RNG-based environmental data
	/// Now serves as a fallback when NASA data is unavailable
	/// </summary>
	public class DataService : IDataService
	{
		private readonly RegionType regionType;
		private readonly System.Random random;

		public DataService(RegionType regionType, int seed = 12345)
		{
			this.regionType = regionType;
			this.random = new System.Random(seed);
		}

		public WeeklyEnvironment GetWeeklyEnvironment(int weekIndex)
		{
			// Mock simple region-flavored values - IMPROVED RAIN VERSION
			switch (regionType)
			{
				case RegionType.TropicalMonsoon:
					return new WeeklyEnvironment
					{
						rainMm = RandomRange(5f, 25f),  // Increased for more varied rain
						etMm = RandomRange(12f, 20f),  // Kept balanced
						ndviPhase = WeekToPhase(weekIndex)
					};
				case RegionType.SemiAridSteppe:
					return new WeeklyEnvironment
					{
						rainMm = RandomRange(0f, 15f),  // Increased for more rain events
						etMm = RandomRange(15f, 25f),  // Kept balanced for arid conditions
						ndviPhase = WeekToPhase(weekIndex)
					};
				default:
					return new WeeklyEnvironment
					{
						rainMm = RandomRange(3f, 18f),  // Increased for more varied weather
						etMm = RandomRange(10f, 18f),   // Kept balanced
						ndviPhase = WeekToPhase(weekIndex)
					};
			}
		}

		private NdviPhase WeekToPhase(int weekIndex)
		{
			if (weekIndex <= 1) return NdviPhase.GreenUp;
			if (weekIndex <= 3) return NdviPhase.Peak;
			return NdviPhase.Senescence;
		}

		private float RandomRange(float min, float max)
		{
			return (float)(min + random.NextDouble() * (max - min));
		}
	}
}


