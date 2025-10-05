using UnityEngine;
using UnityEngine.UI;
using NasaSpaceApps.FarmFromSpace.Game.Systems;
using NasaSpaceApps.FarmFromSpace.Game;
using TMPro;

namespace NasaSpaceApps.FarmFromSpace.Game.UI
{
	/// <summary>
	/// UI component to display current week information
	/// Shows the formatted week, date range, and season
	/// </summary>
	public class WeekDisplay : MonoBehaviour
	{
		[Header("UI References")]
		[SerializeField] private TMP_Text weekText;
		[SerializeField] private TMP_Text dateRangeText;
		//[SerializeField] private TMP_Text seasonText;
		//[SerializeField] private TMP_Text turnNumberText;

		[Header("Turn Manager Reference")]
		[SerializeField] private TurnManager turnManager;
		[SerializeField] private EndlessTurnManager endlessTurnManager;

		private void Start()
		{
			// Auto-find turn manager if not assigned
			if (turnManager == null && endlessTurnManager == null)
			{
				FindTurnManager();
			}
		}

		private void Update()
		{
			// Try to find turn managers if not found yet
			if (turnManager == null && endlessTurnManager == null)
			{
				FindTurnManager();
			}
			
			UpdateDisplay();
		}

		private void FindTurnManager()
		{
			// Try to find GameRunner first (for standard game mode)
			GameRunner gameRunner = FindFirstObjectByType<GameRunner>();
			if (gameRunner != null)
			{
				turnManager = gameRunner.GetTurnManager();
				if (turnManager != null)
				{

					return;
				}
				else
				{

				}
			}
			else
			{

			}

			// If not found, try EndlessGameRunner (for endless mode)
			EndlessGameRunner endlessGameRunner = FindFirstObjectByType<EndlessGameRunner>();
			if (endlessGameRunner != null)
			{
				endlessTurnManager = endlessGameRunner.GetTurnManager();
				if (endlessTurnManager != null)
				{
					
					return;
				}
				else
				{
					
				}
			}
			else
			{

			}

			// Only show warning if we've tried multiple times
			if (Time.time > 1f) // Only show warning after 1 second
			{
				
			}
		}

		private void UpdateDisplay()
		{
			if (turnManager != null)
			{
				UpdateDisplayFromTurnManager(turnManager);
			}
			else if (endlessTurnManager != null)
			{
				UpdateDisplayFromEndlessTurnManager(endlessTurnManager);
			}
		}

		private void UpdateDisplayFromTurnManager(TurnManager tm)
		{
			if (weekText != null)
				weekText.text = tm.CurrentWeekFormatted;
			
			if (dateRangeText != null)
				dateRangeText.text = tm.CurrentDateRange;
		}

		private void UpdateDisplayFromEndlessTurnManager(EndlessTurnManager etm)
		{
			if (weekText != null)
				weekText.text = etm.CurrentWeekFormatted;
			
			if (dateRangeText != null)
				dateRangeText.text = etm.CurrentDateRange;
		}

		// Public methods for manual updates
		public void SetTurnManager(TurnManager tm)
		{
			turnManager = tm;
			endlessTurnManager = null;
		}

		public void SetEndlessTurnManager(EndlessTurnManager etm)
		{
			endlessTurnManager = etm;
			turnManager = null;
		}

		// Method to refresh display manually
		[ContextMenu("Refresh Display")]
		public void RefreshDisplay()
		{
			UpdateDisplay();
		}
	}
}
