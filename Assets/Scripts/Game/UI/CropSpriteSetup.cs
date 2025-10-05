using UnityEngine;
using NasaSpaceApps.FarmFromSpace.Content;
using NasaSpaceApps.FarmFromSpace.Game.UI;

namespace NasaSpaceApps.FarmFromSpace.Game.UI
{
	/// <summary>
	/// DEPRECATED: Use CropDefinition.growthSprites instead
	/// Helper script to set up crop sprites for different crop types
	/// This component is now deprecated in favor of storing sprites directly in CropDefinition ScriptableObjects
	/// </summary>
	[System.Obsolete("This component is deprecated. Use CropDefinition.growthSprites instead.")]
	public class CropSpriteSetup : MonoBehaviour
	{
		[Header("Crop Sprite Sets")]
		[SerializeField] private CropSpriteSet[] cropSpriteSets;

		[System.Serializable]
		public class CropSpriteSet
		{
			public CropDefinition crop;
			public Sprite[] growthSprites = new Sprite[5]; // 0=seed, 1=seedling, 2=growing, 3=mature, 4=ready
		}

		[System.Obsolete("This method is deprecated. Use CropDefinition.growthSprites instead.")]
		public Sprite[] GetCropSprites(CropDefinition crop)
		{
			Debug.LogWarning("CropSpriteSetup.GetCropSprites is deprecated. Use CropDefinition.growthSprites instead.");
			if (crop == null) return null;

			foreach (var spriteSet in cropSpriteSets)
			{
				if (spriteSet.crop == crop)
				{
					return spriteSet.growthSprites;
				}
			}

			// Return default sprites if no specific set found
			Debug.LogWarning($"No sprite set found for crop: {crop.displayName}");
			return null;
		}

		[System.Obsolete("This method is deprecated. CropGrowthVisualizer now automatically uses CropDefinition.growthSprites.")]
		public void SetupCropVisualizer(CropGrowthVisualizer visualizer, CropDefinition crop)
		{
			Debug.LogWarning("CropSpriteSetup.SetupCropVisualizer is deprecated. CropGrowthVisualizer now automatically uses CropDefinition.growthSprites.");
			if (visualizer == null || crop == null) return;

			Sprite[] sprites = GetCropSprites(crop);
			if (sprites != null)
			{
				visualizer.SetGrowthSprites(sprites);
				visualizer.SetCropSprites(crop);
			}
		}
	}
}
