using NasaSpaceApps.FarmFromSpace.Game.Data;

namespace NasaSpaceApps.FarmFromSpace.Game.Services
{
	/// <summary>
	/// Interface for data services that provide weekly environment data
	/// Allows both DataService and EnhancedDataService to be used interchangeably
	/// </summary>
	public interface IDataService
	{
		WeeklyEnvironment GetWeeklyEnvironment(int weekIndex);
	}
}
