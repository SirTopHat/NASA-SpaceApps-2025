using UnityEngine;
using NasaSpaceApps.FarmFromSpace.Game.UI;

namespace NasaSpaceApps.FarmFromSpace.Game.UI
{
	/// <summary>
	/// Test component to verify WeekDisplay is working correctly
	/// Attach this to a GameObject to test WeekDisplay functionality
	/// </summary>
	public class WeekDisplayTest : MonoBehaviour
	{
		[Header("Test Settings")]
		[SerializeField] private bool runTestOnStart = true;
		[SerializeField] private float testInterval = 2f;

		private WeekDisplay weekDisplay;
		private float lastTestTime;

		private void Start()
		{
			weekDisplay = GetComponent<WeekDisplay>();
			if (weekDisplay == null)
			{
				Debug.LogError("WeekDisplayTest: No WeekDisplay component found on this GameObject!");
				return;
			}

			if (runTestOnStart)
			{
				TestWeekDisplay();
			}
		}

		private void Update()
		{
			if (runTestOnStart && Time.time - lastTestTime > testInterval)
			{
				TestWeekDisplay();
				lastTestTime = Time.time;
			}
		}

		[ContextMenu("Test WeekDisplay")]
		public void TestWeekDisplay()
		{
			if (weekDisplay == null)
			{
				Debug.LogError("WeekDisplayTest: WeekDisplay component is null!");
				return;
			}

			Debug.Log("WeekDisplayTest: Testing WeekDisplay component...");
			
			// Test manual refresh
			weekDisplay.RefreshDisplay();
			
			Debug.Log("WeekDisplayTest: WeekDisplay test completed");
		}

		[ContextMenu("Force Find Turn Managers")]
		public void ForceFindTurnManagers()
		{
			if (weekDisplay == null)
			{
				Debug.LogError("WeekDisplayTest: WeekDisplay component is null!");
				return;
			}

			Debug.Log("WeekDisplayTest: Forcing WeekDisplay to find turn managers...");
			
			// This will trigger the FindTurnManager method
			weekDisplay.RefreshDisplay();
		}
	}
}
