using UnityEngine;
using NasaSpaceApps.FarmFromSpace.Common;
using NasaSpaceApps.FarmFromSpace.Config;
using NasaSpaceApps.FarmFromSpace.Game.Services;
using NasaSpaceApps.FarmFromSpace.Game.Systems;
using NasaSpaceApps.FarmFromSpace.Game.UI;
using NasaSpaceApps.FarmFromSpace.Game.Data;
using System;

namespace NasaSpaceApps.FarmFromSpace.Game
{
	public class EndlessGameRunner : MonoBehaviour
	{
	[SerializeField] private BalanceConfig balanceConfig;
	[SerializeField] private NasaDataConfig nasaDataConfig; // NASA data configuration
	[SerializeField] private RegionType currentRegion = RegionType.TropicalMonsoon;
	[SerializeField] private SoilTexture soil = SoilTexture.Loam;

		[Header("Plot Rendering")]
		[SerializeField] private GameObject plotPrefab;
		[SerializeField] private EndlessCropPicker cropPicker;
		[SerializeField] private Vector2 plotStart = new Vector2(-3f, 0f);
		[SerializeField] private Vector2 plotSpacing = new Vector2(3f, 0f);
		[SerializeField] private Vector2 plotSize = new Vector2(2f, 2f);
		[SerializeField] private Transform plotsParent; // Parent object to group all plots

		[Header("Save System")]
		[SerializeField] private string saveFileName = "endless_farm_save.json";

		private EndlessTurnManager turnManager;
		private EnhancedDataService dataService;
		private GlobalGameState globalState;
		private Sprite squareSprite;
		private GameObject[] plotObjects;
		private RegionDataManager regionDataManager;

		public EndlessTurnManager GetTurnManager() => turnManager;
		public GlobalGameState GetGlobalState() => globalState;

		private void Start()
		{
			// Ensure game state is loaded (in case Awake didn't already)
			if (globalState == null)
			{
				LoadGame();
			}
			EnsurePlotsParent();
			InitializeGame();
		}

		private void Awake()
		{
			// Load save/state as early as possible so other components can query safely
			if (globalState == null)
			{
				LoadGame();
			}
		}

		private void EnsurePlotsParent()
		{
			// Create plots parent if it doesn't exist
			if (plotsParent == null)
			{
				GameObject parentObj = new GameObject("Plots");
				plotsParent = parentObj.transform;
			}
		}

		private void LoadGame()
		{
			// Try to load existing save
			string savePath = System.IO.Path.Combine(Application.persistentDataPath, saveFileName);
			if (System.IO.File.Exists(savePath))
			{
				try
				{
					string json = System.IO.File.ReadAllText(savePath);
					globalState = JsonUtility.FromJson<GlobalGameState>(json);
					
					// Debug: Log unlocked plots after loading
					var regionState = globalState.GetRegionState(currentRegion);
					//Debug.Log($"Game loaded - Unlocked plots for {currentRegion}: {string.Join(", ", regionState.unlockedPlots)}");
				}
				catch (System.Exception e)
				{
					Debug.LogError($"Failed to load game: {e.Message}");
					globalState = new GlobalGameState();
				}
			}
			else
			{
				globalState = new GlobalGameState();
			}

			// Set starting gold from balance config if this is a new game
			if (globalState.universalGold == 60) // Default value, means new game
			{
				globalState.universalGold = balanceConfig.startingGold;
			}
		}

		public void SaveGame()
		{
			try
			{
				// Sync region states data before saving
				globalState.SyncRegionStatesData();
				
				// Debug: Log unlocked plots before saving
				var regionState = globalState.GetRegionState(currentRegion);
				//Debug.Log($"Saving game - Unlocked plots for {currentRegion}: {string.Join(", ", regionState.unlockedPlots)}");
				
				string json = JsonUtility.ToJson(globalState, true);
				string savePath = System.IO.Path.Combine(Application.persistentDataPath, saveFileName);
				System.IO.File.WriteAllText(savePath, json);
				//Debug.Log("Game saved successfully");
			}
			catch (System.Exception e)
			{
				Debug.LogError($"Failed to save game: {e.Message}");
			}
		}

		private void InitializeGame()
		{
			// Initialize enhanced data service with NASA data support
			dataService = new EnhancedDataService(currentRegion, nasaDataConfig);
			
			// Get start date from NASA config or use default
			DateTime startDate = nasaDataConfig?.startDate ?? new DateTime(2024, 3, 1);
			turnManager = new EndlessTurnManager(dataService, balanceConfig, soil, currentRegion, globalState, startDate);
			EnsureSquareSprite();
			SpawnPlots();
			
			// Log data source information
			if (dataService.IsUsingRealData)
			{
				Debug.Log($"EndlessGameRunner: Using authentic NASA satellite data starting {startDate:MMMM d, yyyy}");
			}
			else
			{
				Debug.Log($"EndlessGameRunner: Using fallback RNG data starting {startDate:MMMM d, yyyy}");
			}
			
			// Log current week information
			//Debug.Log($"EndlessGameRunner: Current week - {turnManager.CurrentWeekFormatted} ({turnManager.CurrentDateRange})");
		}

		public void RefreshPlotUI()
		{
			// Refresh plot visuals after unlocking
			UpdatePlotVisuals();
		}

		private void Update()
		{
			if (turnManager == null) return;

			if (turnManager.Phase == EndlessTurnPhase.Planning)
			{
				// Handle input for planning phase
				HandlePlanningInput();
			}
			else if (turnManager.Phase == EndlessTurnPhase.End)
			{
				// Handle input for end phase
				HandleEndPhaseInput();
			}

			// Update plot visuals
			UpdatePlotVisuals();
		}

		private void HandlePlanningInput()
		{
			// Handle plot selection
			if (Input.GetMouseButtonDown(0))
			{
				HandlePlotClick();
			}

			// Handle keyboard shortcuts
			if (Input.GetKeyDown(KeyCode.Space))
			{
				turnManager.EndPlanningAndResolve();
			}
		}

		private void HandleEndPhaseInput()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				turnManager.NextWeek();
				SaveGame(); // Auto-save after each week
			}
		}

		private void HandlePlotClick()
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			
			if (Physics.Raycast(ray, out hit))
			{
				//Debug.Log($"Raycast hit: {hit.collider.gameObject.name}");
				
				// Find which plot was clicked
				for (int i = 0; i < plotObjects.Length; i++)
				{
					if (plotObjects[i] != null && hit.collider.gameObject == plotObjects[i])
					{
						Debug.Log($"Clicked plot {i}");
						turnManager.SelectPlot(i);
						break;
					}
				}
			}
			else
			{
				//Debug.Log("Raycast did not hit anything");
			}
		}

		private void EnsureSquareSprite()
		{
			if (squareSprite == null)
			{
				// Create a simple white square sprite
				Texture2D texture = new Texture2D(64, 64);
				Color[] pixels = new Color[64 * 64];
				for (int i = 0; i < pixels.Length; i++)
				{
					pixels[i] = Color.white;
				}
				texture.SetPixels(pixels);
				texture.Apply();
				squareSprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
			}
		}

		private void SpawnPlots()
		{
			// Clear existing plots
			if (plotObjects != null)
			{
				foreach (var obj in plotObjects)
				{
					if (obj != null) DestroyImmediate(obj);
				}
			}

			// Create or find plots parent
			if (plotsParent == null)
			{
				GameObject parentObj = new GameObject("Plots");
				plotsParent = parentObj.transform;
			}

			// Spawn plots for all 9 positions (3x3 grid)
			plotObjects = new GameObject[9];
			for (int i = 0; i < 9; i++)
			{
				// Calculate 3x3 grid position
				int row = i / 3;
				int col = i % 3;
				
				Vector3 position = new Vector3(
					plotStart.x + col * plotSpacing.x,
					plotStart.y - row * plotSpacing.y, // Negative Y for top-to-bottom
					0f
				);

				GameObject plotObj;
				if (plotPrefab != null)
				{
					plotObj = Instantiate(plotPrefab, position, Quaternion.identity, plotsParent);
					
					// Ensure the prefab has the right collider type for clicking
					var existingCollider2D = plotObj.GetComponent<BoxCollider2D>();
					if (existingCollider2D != null)
					{
						// Keep the 2D collider, ensure it's not a trigger for clicking
						existingCollider2D.isTrigger = false;
						//Debug.Log($"Using existing BoxCollider2D on plot {i + 1}");
					}
					else
					{
						// Add 2D collider for clicking (preferred for 2D games)
						BoxCollider2D collider2D = plotObj.AddComponent<BoxCollider2D>();
						collider2D.size = new Vector2(plotSize.x, plotSize.y);
						collider2D.isTrigger = false; // Not a trigger so we can click on it
					}
				}
				else
				{
					plotObj = new GameObject($"Plot {i + 1}");
					plotObj.transform.SetParent(plotsParent);
					plotObj.transform.position = position;
					
					// Add sprite renderer
					SpriteRenderer renderer = plotObj.AddComponent<SpriteRenderer>();
					renderer.sprite = squareSprite;
					renderer.color = GetPlotColor(i);
					
					// Add 2D collider for clicking (more compatible with 2D sprites)
					BoxCollider2D collider = plotObj.AddComponent<BoxCollider2D>();
					collider.size = new Vector2(plotSize.x, plotSize.y);
					collider.isTrigger = false; // Not a trigger so we can click on it
				}

				// Add EndlessPlotView component
				var plotView = plotObj.GetComponent<EndlessPlotView>();
				if (plotView == null) 
				{
					plotView = plotObj.AddComponent<EndlessPlotView>();
				}
				
				if (plotView != null)
				{
					plotView.plotIndex = i;
					plotView.runner = this;
				}
				else
				{
					//Debug.LogError($"Failed to add EndlessPlotView to plot {i + 1}");
				}

				// Setup crop growth visualizer
				var cropVisualizer = plotObj.GetComponentInChildren<CropGrowthVisualizer>();
				if (cropVisualizer != null)
				{
					cropVisualizer.SetPlotIndex(i);
					//Debug.Log($"Setup crop visualizer for plot {i}");
				}

				plotObjects[i] = plotObj;
			}
		}

		private void UpdatePlotVisuals()
		{
			if (plotObjects == null) return;

			for (int i = 0; i < plotObjects.Length; i++)
			{
				if (plotObjects[i] != null)
				{
					var plotView = plotObjects[i].GetComponent<EndlessPlotView>();
					if (plotView != null)
					{
						plotView.UpdateVisualState();
					}
				}
			}
		}

		private Color GetPlotColor(int plotIndex)
		{
			// Check if plot is unlocked
			if (!turnManager.IsPlotUnlocked(plotIndex))
			{
				return Color.gray; // Locked plot
			}

			// Check if plot exists in the plots list
			if (plotIndex >= turnManager.Plots.Count || turnManager.Plots[plotIndex] == null)
			{
				return Color.white; // Unlocked but no plot data yet
			}

			var plot = turnManager.Plots[plotIndex];
			
			// Color based on soil moisture
			if (plot.soilTank < 0.3f)
				return Color.yellow; // Dry
			else if (plot.soilTank > 0.8f)
				return Color.blue; // Wet
			else
				return Color.green; // Good moisture
		}

		public void SwitchRegion(RegionType newRegion)
		{
			/*
			if (!globalState.IsRegionUnlocked(newRegion))
			{
				Debug.Log($"Region {newRegion} is not unlocked yet!");
				return;
			}
			*/

			if (newRegion == currentRegion)
			{
				Debug.Log($"EndlessGameRunner: Already in region {newRegion}");
				return;
			}

			Debug.Log($"EndlessGameRunner: Switching from {currentRegion} to {newRegion}");

			// Save current region state
			SaveGame();

			// Switch region
			currentRegion = newRegion;

			// Reinitialize with new region data
			ReinitializeForNewRegion();

			Debug.Log($"EndlessGameRunner: Successfully switched to {newRegion}");
		}

		public bool UnlockRegion(RegionType region)
		{
			if (globalState.UnlockRegion(region))
			{
				SaveGame();
				Debug.Log($"Region {region} unlocked!");
				return true;
			}
			return false;
		}

		private void OnApplicationPause(bool pauseStatus)
		{
			if (pauseStatus)
			{
				SaveGame();
			}
		}

		private void OnApplicationFocus(bool hasFocus)
		{
			if (!hasFocus)
			{
				SaveGame();
			}
		}

		private void OnDestroy()
		{
			SaveGame();
		}

		private void ReinitializeForNewRegion()
		{
			// Create new data service for the new region
			dataService = new EnhancedDataService(currentRegion, nasaDataConfig);

			// Get start date from NASA config or use default
			DateTime startDate = nasaDataConfig?.startDate ?? new DateTime(2024, 3, 1);

			// Create new turn manager with new region data
			turnManager = new EndlessTurnManager(dataService, balanceConfig, soil, currentRegion, globalState, startDate);

			// Update plot visuals for new region
			UpdatePlotVisuals();

			// Notify NASA data cards of the region change
			NotifyNasaDataCardsOfRegionChange();

			// Log new region information
			Debug.Log($"EndlessGameRunner: Switched to {currentRegion}");
			Debug.Log($"EndlessGameRunner: Data Source: {dataService.DataSource}");
			Debug.Log($"EndlessGameRunner: Using Real Data: {dataService.IsUsingRealData}");
		}

		/// <summary>
		/// Notifies all NASA data card components of the region change
		/// </summary>
		private void NotifyNasaDataCardsOfRegionChange()
		{
			// Find all NASA data card components in the scene
			var nasaDataCards = FindObjectsOfType<EndlessNasaDataCard>();
			foreach (var card in nasaDataCards)
			{
				if (card != null)
				{
					card.OnRegionChanged(currentRegion, turnManager);
				}
			}

			Debug.Log($"EndlessGameRunner: Notified {nasaDataCards.Length} NASA data cards of region change to {currentRegion}");
		}

		public string GetCurrentRegionDisplayName()
		{
			return currentRegion switch
			{
				RegionType.TropicalMonsoon => "Tropical Monsoon",
				RegionType.SemiAridSteppe => "Semi-Arid Steppe",
				RegionType.TemperateContinental => "Temperate Continental",
				_ => currentRegion.ToString()
			};
		}

		public bool IsUsingRealData()
		{
			return dataService?.IsUsingRealData ?? false;
		}

		public string GetDataSource()
		{
			return dataService?.DataSource ?? "Unknown";
		}

		public RegionType GetCurrentRegion()
		{
			return currentRegion;
		}

		/// <summary>
		/// Checks if a region is unlocked
		/// </summary>
		public bool IsRegionUnlocked(RegionType region)
		{
			return globalState != null && globalState.IsRegionUnlocked(region);
		}

		/// <summary>
		/// Gets the unlock cost for a region
		/// </summary>
		public int GetRegionUnlockCost(RegionType region)
		{
			return globalState != null ? globalState.GetRegionUnlockCost(region) : 0;
		}

		/// <summary>
		/// Gets the current gold amount
		/// </summary>
		public int GetCurrentGold()
		{
			return globalState?.universalGold ?? 0;
		}
	}
}
