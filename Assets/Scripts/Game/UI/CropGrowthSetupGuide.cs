using UnityEngine;
using NasaSpaceApps.FarmFromSpace.Game.UI;
using NasaSpaceApps.FarmFromSpace.Content;

namespace NasaSpaceApps.FarmFromSpace.Game.UI
{
	/// <summary>
	/// Setup guide and helper for crop growth visualization system
	/// </summary>
	public class CropGrowthSetupGuide : MonoBehaviour
	{
		[Header("Setup Instructions")]
		[TextArea(10, 20)]
		public string setupInstructions = @"
CROP GROWTH VISUALIZATION SETUP GUIDE:

1. PLOT PREFAB SETUP:
   - Add CropGrowthVisualizer component to your plot prefab
   - Create a child GameObject for crop visuals
   - Add a SpriteRenderer component to the child GameObject
   - Assign the SpriteRenderer to the 'cropSpriteRenderer' field in CropGrowthVisualizer
   - Assign the child GameObject to the 'cropVisualParent' field

2. CROP DEFINITION SETUP (NEW METHOD):
   - Open your CropDefinition ScriptableObjects (Apples, Carrots, Rice, etc.)
   - In the 'Growth Sprites' section, assign 5 sprites for each crop:
     [0] = Seed stage
     [1] = Seedling stage  
     [2] = Growing stage
     [3] = Mature stage
     [4] = Ready for harvest
   - Each crop can have different sprites for visual variety

3. AUTOMATIC SPRITE SWITCHING:
   - The system automatically switches sprites when crops are planted
   - No manual setup required - sprites come from CropDefinition
   - Different crops will show their unique growth sprites

4. TESTING:
   - Plant different crops on plots
   - Watch each crop show its unique growth sprites
   - Check console for growth stage updates

GROWTH STAGES:
- 0-20% progress: Seed
- 20-40% progress: Seedling  
- 40-70% progress: Growing
- 70-100% progress: Mature
- 100% progress: Ready for harvest

NOTE: CropSpriteSetup component is now deprecated. Use CropDefinition.growthSprites instead.
";

		[Header("Debug")]
		[SerializeField] private bool showDebugInfo = true;

		private void Start()
		{
			if (showDebugInfo)
			{
				Debug.Log(setupInstructions);
			}
		}

		[ContextMenu("Test Crop Growth")]
		public void TestCropGrowth()
		{
			Debug.Log("Testing crop growth visualization system...");
			
			// Find all crop definitions in the project
			var cropDefinitions = Resources.FindObjectsOfTypeAll<CropDefinition>();
			Debug.Log($"Found {cropDefinitions.Length} crop definitions:");
			
			foreach (var crop in cropDefinitions)
			{
				if (crop.growthSprites != null && crop.growthSprites.Length == 5)
				{
					int validSprites = 0;
					for (int i = 0; i < 5; i++)
					{
						if (crop.growthSprites[i] != null) validSprites++;
					}
					Debug.Log($"  {crop.displayName}: {validSprites}/5 sprites assigned");
				}
				else
				{
					Debug.LogWarning($"  {crop.displayName}: Missing or invalid growth sprites!");
				}
			}
			
			Debug.Log("Crop growth test completed. Check the console for results.");
		}

		[ContextMenu("Setup All Crop Visualizers")]
		public void SetupAllCropVisualizers()
		{
			var visualizers = FindObjectsByType<CropGrowthVisualizer>(FindObjectsSortMode.None);
			
			foreach (var visualizer in visualizers)
			{
				// Set default sprites if none are assigned
				if (visualizer.GetComponent<CropGrowthVisualizer>() != null)
				{
					Debug.Log($"Setup crop visualizer: {visualizer.name}");
				}
			}
		}
	}
}
