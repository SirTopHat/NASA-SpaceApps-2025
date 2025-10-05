namespace NasaSpaceApps.FarmFromSpace.Common
{
	public enum SoilTexture
	{
		Sandy = 0,
		Loam = 1,
		Clay = 2
	}

	public enum NdviPhase
	{
		Dormant = 0,
		GreenUp = 1,
		Peak = 2,
		Senescence = 3
	}

	public enum RegionType
	{
		TropicalMonsoon = 0,
		SemiAridSteppe = 1,
		TemperateContinental = 2
	}

	public enum CardType
	{
		Irrigation,
		NitrogenSplit,
		DelayIrrigation,
		CoverCrop,
		Mulch,
		SoilProbe,
		ForecastUpgrade,
		PumpMaintenance
	}

	public enum RelicType
	{
		TerraceFields,
		SplitNPlanter,
		SiltTrap,
		DripKit,
		MonsoonAlmanac,
		ColdSnapCover
	}

	public enum GameEventType
	{
		StormWarning,
		CanalQuotaCut,
		HeatPulse,
		CloudyFortnight,
		ColdSnap,
		MarketSpike
	}


	public enum ContractType
	{
		YieldFirst,
		WaterSaver,
		SoilGuardian
	}
}


