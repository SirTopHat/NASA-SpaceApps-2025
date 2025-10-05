using NasaSpaceApps.FarmFromSpace.Common;
using NasaSpaceApps.FarmFromSpace.Content;

namespace NasaSpaceApps.FarmFromSpace.Game.Data
{
	[System.Serializable]
	public class PlotState
	{
		public int id;
		public SoilTexture soilTexture;
		public float soilTank; // 0-1
		public int runoffEvents;
		public float yieldAccumulated;

		public CropDefinition plantedCrop;
		public string plantedCropName;
		public int plantedWeek; // -1 if not planted

		public int nPartsApplied;
		public bool splitNApplied;
		public int nPartAWeek; // Week when Part A was applied (-1 if not applied)
		public int nPartBWeek; // Week when Part B was applied (-1 if not applied)
		
		// Penalty tracking
		public int leachingPenaltyNextWeek; // Gold penalty to apply next week due to leaching
		
		// Irrigation tracking
		public int irrigationsThisWeek; // Number of times this plot was irrigated this week
		public float totalIrrigationMm; // Total irrigation applied this week (for diminishing returns)
		
		// New: Per-plot crop cycle management
		public float maturityTarget; // Growth points needed to reach maturity
		public bool isReadyForHarvest; // True when yieldAccumulated >= maturityTarget
		public int cropAgeWeeks; // Weeks since planting (for crop-specific logic)
		
		// New: Suitability system
		public CropSuitability suitability; // Suitability rating for this crop in this region
	}
	
	[System.Serializable]
	public enum CropSuitability
	{
		Good = 0,
		Marginal = 1,
		Poor = 2
	}
}


