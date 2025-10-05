using UnityEngine;
using NasaSpaceApps.FarmFromSpace.Common;
using NasaSpaceApps.FarmFromSpace.Config;
using NasaSpaceApps.FarmFromSpace.Game.Data;

namespace NasaSpaceApps.FarmFromSpace.Game.Services
{
	/// <summary>
	/// Test component to verify EnhancedDataService is working correctly
	/// Attach this to a GameObject to test EnhancedDataService functionality
	/// </summary>
	public class EnhancedDataServiceTest : MonoBehaviour
	{
		[Header("Test Configuration")]
		[SerializeField] private NasaDataConfig nasaDataConfig;
		[SerializeField] private RegionType testRegion = RegionType.TropicalMonsoon;
		[SerializeField] private bool runTestOnStart = true;
		[SerializeField] private int testWeeks = 5;

		private EnhancedDataService dataService;

		private void Start()
		{
			if (runTestOnStart)
			{
				TestEnhancedDataService();
			}
		}

		[ContextMenu("Test EnhancedDataService")]
		public void TestEnhancedDataService()
		{
			Debug.Log("EnhancedDataServiceTest: Starting EnhancedDataService test...");

			// Create EnhancedDataService
			dataService = new EnhancedDataService(testRegion, nasaDataConfig);

			// Test basic properties
			Debug.Log($"EnhancedDataServiceTest: Region = {dataService.RegionType}");
			Debug.Log($"EnhancedDataServiceTest: Start Date = {dataService.StartDate:MMMM d, yyyy}");
			Debug.Log($"EnhancedDataServiceTest: Data Source = {dataService.DataSource}");
			Debug.Log($"EnhancedDataServiceTest: Using Real Data = {dataService.IsUsingRealData}");
			Debug.Log($"EnhancedDataServiceTest: Data Available = {dataService.IsDataAvailable}");

			// Test data retrieval for multiple weeks
			for (int week = 0; week < testWeeks; week++)
			{
				try
				{
					var env = dataService.GetWeeklyEnvironment(week);
					Debug.Log($"EnhancedDataServiceTest: Week {week + 1} - Rain: {env.rainMm:F1}mm, ET: {env.etMm:F1}mm, NDVI: {env.ndviPhase}");
				}
				catch (System.Exception e)
				{
					Debug.LogError($"EnhancedDataServiceTest: Error getting data for week {week + 1}: {e.Message}");
				}
			}

			Debug.Log("EnhancedDataServiceTest: EnhancedDataService test completed!");
		}

		[ContextMenu("Test NASA Data Only")]
		public void TestNasaDataOnly()
		{
			if (nasaDataConfig == null)
			{
				Debug.LogError("EnhancedDataServiceTest: No NasaDataConfig assigned!");
				return;
			}

			// Force NASA data usage
			nasaDataConfig.useRealNasaData = true;
			
			Debug.Log("EnhancedDataServiceTest: Testing with NASA data enabled...");
			TestEnhancedDataService();
		}

		[ContextMenu("Test RNG Data Only")]
		public void TestRngDataOnly()
		{
			if (nasaDataConfig == null)
			{
				Debug.LogError("EnhancedDataServiceTest: No NasaDataConfig assigned!");
				return;
			}

			// Force RNG data usage
			nasaDataConfig.useRealNasaData = false;
			
			Debug.Log("EnhancedDataServiceTest: Testing with RNG data only...");
			TestEnhancedDataService();
		}
	}
}
