using UnityEngine;

namespace NasaSpaceApps.FarmFromSpace.Game.UI
{
	/// <summary>
	/// Helper script to set up the plots parent and ensure proper organization
	/// </summary>
	public class PlotSetupHelper : MonoBehaviour
	{
		[Header("Setup")]
		[SerializeField] private EndlessGameRunner gameRunner;
		[SerializeField] private bool createPlotsParent = true;
		[SerializeField] private string plotsParentName = "Plots";

		[ContextMenu("Setup Plots Parent")]
		public void SetupPlotsParent()
		{
			if (gameRunner == null)
			{
				gameRunner = FindFirstObjectByType<EndlessGameRunner>();
			}

			if (gameRunner == null)
			{
				Debug.LogError("EndlessGameRunner not found!");
				return;
			}

			// Find or create plots parent
			Transform plotsParent = GameObject.Find(plotsParentName)?.transform;
			if (plotsParent == null && createPlotsParent)
			{
				GameObject parentObj = new GameObject(plotsParentName);
				plotsParent = parentObj.transform;
				Debug.Log($"Created plots parent: {plotsParentName}");
			}

			// Set the plots parent in the game runner
			if (plotsParent != null)
			{
				// Use reflection to set the private field
				var field = typeof(EndlessGameRunner).GetField("plotsParent", 
					System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				if (field != null)
				{
					field.SetValue(gameRunner, plotsParent);
					Debug.Log($"Set plots parent in EndlessGameRunner");
				}
			}
		}

		[ContextMenu("Organize Existing Plots")]
		public void OrganizeExistingPlots()
		{
			// Find all plot objects and organize them under the plots parent
			GameObject[] plotObjects = GameObject.FindGameObjectsWithTag("Plot");
			Transform plotsParent = GameObject.Find(plotsParentName)?.transform;

			if (plotsParent == null)
			{
				GameObject parentObj = new GameObject(plotsParentName);
				plotsParent = parentObj.transform;
			}

			foreach (GameObject plot in plotObjects)
			{
				plot.transform.SetParent(plotsParent);
			}

			Debug.Log($"Organized {plotObjects.Length} plots under {plotsParentName}");
		}

		private void Start()
		{
			if (createPlotsParent)
			{
				SetupPlotsParent();
			}
		}
	}
}
