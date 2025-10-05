using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NasaSpaceApps.FarmFromSpace.Game.UI
{
	public class GameUI : MonoBehaviour
	{
		[SerializeField] private GameRunner runner; // will set via inspector
		[SerializeField] private TMP_Text topLine;
		[SerializeField] private TMP_Text midLine;
		[SerializeField] private TMP_Text bottomLine;
		[SerializeField] private Button irrigate10Button;
		[SerializeField] private Button resolveNextButton;

		private void Awake()
		{
			if (irrigate10Button != null)
			{
				irrigate10Button.onClick.AddListener(() => runner.UI_Irrigate10());
			}
			if (resolveNextButton != null)
			{
				resolveNextButton.onClick.AddListener(() => runner.UI_ResolveOrNext());
			}
		}

		private void Update()
		{
			if (runner == null) return;
			
			var turnManager = runner.GetTurnManager();
			if (turnManager == null) return;

			// Update text with new system
			if (topLine != null)
			{
				topLine.text = $"Week {turnManager.State.weekIndex + 1} / {runner.GetTotalWeeks()} - Phase: {turnManager.Phase}";
			}
			if (midLine != null)
			{
				midLine.text = $"Rain: {turnManager.Env.rainMm:F1} mm | ET: {turnManager.Env.etMm:F1} mm | NDVI Phase: {turnManager.Env.ndviPhase}";
			}
			if (bottomLine != null)
			{
				bottomLine.text = $"Soil Tank: {(turnManager.State.soilTank * 100f):F0}% | Runoff Events: {turnManager.State.runoffEventsThisSeason} | Yield: {turnManager.State.yieldAccumulated:F2}";
			}
		}
	}
}


