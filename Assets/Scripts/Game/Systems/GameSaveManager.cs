using System.IO;
using UnityEngine;

namespace NasaSpaceApps.FarmFromSpace.Game.Systems
{
	public static class GameSaveManager
	{
		// Matches EndlessGameRunner default filename
		private const string EndlessSaveFileName = "endless_farm_save.json";

		public static void DeleteEndlessModeSave()
		{
			string path = Path.Combine(Application.persistentDataPath, EndlessSaveFileName);
			if (File.Exists(path))
			{
				try { File.Delete(path); }
				catch (IOException e) { Debug.LogError($"Failed to delete save: {e.Message}"); }
			}
		}

		public static void DeleteAllGameData()
		{
			// Delete json saves in persistent data path
			try
			{
				if (Directory.Exists(Application.persistentDataPath))
				{
					var files = Directory.GetFiles(Application.persistentDataPath, "*.json");
					for (int i = 0; i < files.Length; i++)
					{
						try { File.Delete(files[i]); } catch (IOException e) { Debug.LogError($"Delete failed: {e.Message}"); }
					}
				}
			}
			catch (System.Exception e)
			{
				Debug.LogError($"Error while deleting game data: {e.Message}");
			}

			// Optional: clear PlayerPrefs if used for settings/progress
			PlayerPrefs.DeleteAll();
			PlayerPrefs.Save();
		}
	}
}


