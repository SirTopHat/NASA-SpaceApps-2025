using UnityEngine;
using NasaSpaceApps.FarmFromSpace.Common;
using NasaSpaceApps.FarmFromSpace.Config;
using NasaSpaceApps.FarmFromSpace.Game.Services;
using NasaSpaceApps.FarmFromSpace.Game.Systems;
using NasaSpaceApps.FarmFromSpace.Game.UI;
using System;

namespace NasaSpaceApps.FarmFromSpace.Game
{
	public class GameRunner : MonoBehaviour
	{
	[SerializeField] private BalanceConfig balanceConfig;
	[SerializeField] private NasaDataConfig nasaDataConfig; // NASA data configuration
	[SerializeField] private RegionType region = RegionType.SemiAridSteppe;
	[SerializeField] private NasaSpaceApps.FarmFromSpace.Content.RegionDefinition regionDefinition; // optional: provides crop list
	[SerializeField] private SoilTexture soil = SoilTexture.Loam;
	[SerializeField] private int totalWeeks = 6;
	[SerializeField] private int plots = 2;

		[Header("Plot Rendering")]
		[SerializeField] private GameObject plotPrefab; // optional: if set, will be used instead of generated square
		[SerializeField] private GameUICropPicker cropPicker; // reference to crop picker UI
		[SerializeField] private Vector2 plotStart = new Vector2(-3f, 0f);
		[SerializeField] private Vector2 plotSpacing = new Vector2(3f, 0f);
		[SerializeField] private Vector2 plotSize = new Vector2(2f, 2f);

		private TurnManager turnManager;
		private EnhancedDataService dataService;
		private Sprite squareSprite;
		private GameObject[] plotObjects;

		private void Start()
		{
			// Initialize enhanced data service with NASA data support
			dataService = new EnhancedDataService(region, nasaDataConfig);
			
			// Get start date from NASA config or use default
			DateTime startDate = nasaDataConfig?.startDate ?? new DateTime(2024, 3, 1);
			turnManager = new TurnManager(dataService, balanceConfig, soil, totalWeeks, plots, startDate);
			EnsureSquareSprite();
			SpawnPlots();
			
			// Log data source information
			if (dataService.IsUsingRealData)
			{
				Debug.Log($"GameRunner: Using authentic NASA satellite data starting {startDate:MMMM d, yyyy}");
			}
			else
			{
				Debug.Log($"GameRunner: Using fallback RNG data starting {startDate:MMMM d, yyyy}");
			}
			
			// Log current week information
			Debug.Log($"GameRunner: Current week - {turnManager.CurrentWeekFormatted} ({turnManager.CurrentDateRange})");
		}

		private void Update()
		{
			if (turnManager == null) return;

			if (turnManager.Phase == TurnPhase.Planning)
			{
				if (Input.GetKeyDown(KeyCode.I))
				{
					turnManager.QueueIrrigation(20f);
				}
				if (Input.GetKeyDown(KeyCode.Space))
				{
					turnManager.EndPlanningAndResolve();
				}
			}
			else if (turnManager.Phase == TurnPhase.End)
			{
				if (Input.GetKeyDown(KeyCode.Space))
				{
					turnManager.NextWeek();
				}
			}
			else if (turnManager.Phase == TurnPhase.Harvest)
			{
				if (Input.GetKeyDown(KeyCode.R))
				{
					RestartGame();
				}
			}
		}

		// OnGUI removed - now handled by GameHUD component

		public NasaSpaceApps.FarmFromSpace.Content.CropDefinition GetFirstAvailableCrop()
		{
			if (regionDefinition != null && regionDefinition.availableCrops != null && regionDefinition.availableCrops.Length > 0)
			{
				return regionDefinition.availableCrops[0];
			}
			return null; // TurnManager will still set a default name if null
		}

		private void EnsureSquareSprite()
		{
			if (squareSprite != null) return;
			Texture2D tex = new Texture2D(2, 2);
			tex.SetPixels(new Color[] { Color.white, Color.white, Color.white, Color.white });
			tex.Apply();
			squareSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 1f);
		}

		private void SpawnPlots()
		{
			plotObjects = new GameObject[plots];
			for (int i = 0; i < plots; i++)
			{
				GameObject go;
				if (plotPrefab != null)
				{
					go = Instantiate(plotPrefab, transform);
					go.name = $"Plot_{i + 1}";
				}
				else
				{
					go = new GameObject($"Plot_{i + 1}");
					go.transform.SetParent(transform);
					var sr = go.AddComponent<SpriteRenderer>();
					sr.sprite = squareSprite;
					sr.color = Color.gray;
					var col = go.AddComponent<BoxCollider2D>();
					col.isTrigger = true;
				}
				go.transform.position = new Vector3(plotStart.x + i * plotSpacing.x, plotStart.y + i * plotSpacing.y, 0f);
				go.transform.localScale = new Vector3(plotSize.x, plotSize.y, 1f);
				var view = go.GetComponent<NasaSpaceApps.FarmFromSpace.Game.UI.PlotView>();
				if (view == null) view = go.AddComponent<NasaSpaceApps.FarmFromSpace.Game.UI.PlotView>();
				view.plotIndex = i;
				view.runner = this;
				plotObjects[i] = go;
			}
			UpdatePlotColors();
		}

		private void UpdatePlotColors()
		{
			if (plotObjects == null || turnManager == null) return;
			for (int i = 0; i < plotObjects.Length; i++)
			{
				var sr = plotObjects[i].GetComponent<SpriteRenderer>();
				bool isSelected = (i == turnManager.SelectedPlotIndex);
				var plot = turnManager.Plots[i];
				Color c = plot.plantedWeek < 0 ? new Color(0.5f, 0.5f, 0.5f) : Color.green;
				if (plot.plantedWeek >= 0)
				{
					// Rough growth-stage tint
					switch (turnManager.Env.ndviPhase)
					{
						case Common.NdviPhase.GreenUp: c = new Color(0.3f, 0.8f, 0.3f); break;
						case Common.NdviPhase.Peak: c = new Color(0.1f, 0.6f, 0.1f); break;
						case Common.NdviPhase.Senescence: c = new Color(0.7f, 0.6f, 0.1f); break;
						default: c = new Color(0.5f, 0.5f, 0.5f); break;
					}
				}
				sr.color = isSelected ? Color.Lerp(c, Color.cyan, 0.4f) : c;
			}
		}

		public void UI_SelectPlot(int index)
		{
			turnManager.SelectPlot(index);
			UpdatePlotColors();
			
			// Don't automatically show crop picker - let the Plant button handle it
			if (cropPicker != null)
			{
				cropPicker.HidePanel();
			}
		}

		public void UI_BuyPlot()
		{
			if (turnManager == null) return;
			turnManager.BuyNewPlot();
			// Update plot objects after buying
			UpdatePlotObjects();
		}

		public void SetPlotRaycastEnabled(bool enabled)
		{
			if (plotObjects == null) return;
			foreach (var plotObj in plotObjects)
			{
				if (plotObj != null)
				{
					var collider = plotObj.GetComponent<Collider2D>();
					if (collider != null)
					{
						collider.enabled = enabled;
					}
				}
			}
		}

		public TurnManager GetTurnManager()
		{
			return turnManager;
		}

		public int GetTotalWeeks()
		{
			return totalWeeks;
		}

		public void UI_Irrigate10()
		{
			if (turnManager == null) return;
			turnManager.QueueIrrigation(10f);
		}

		public void UI_PlantCrop(Content.CropDefinition crop)
		{
			if (turnManager == null) return;
			if (turnManager.Phase == TurnPhase.Planning)
			{
				var selectedPlot = turnManager.Plots[turnManager.SelectedPlotIndex];
				if (selectedPlot.plantedWeek < 0) // only plant if not already planted
				{
					turnManager.PlantSelectedPlot(crop);
				}
			}
		}

		public void UI_ApplyNitrogenPartA()
		{
			if (turnManager == null) return;
			turnManager.ApplyNitrogenPartA();
		}

		public void UI_ApplyNitrogenPartB()
		{
			if (turnManager == null) return;
			turnManager.ApplyNitrogenPartB();
		}

		public void RestartGame()
		{
			// Reinitialize the game
			dataService = new DataService(region, 123);
			turnManager = new TurnManager(dataService, balanceConfig, soil, totalWeeks, plots);
			EnsureSquareSprite();
			SpawnPlots();
			Debug.Log("Game restarted! Press R to restart again when harvest is complete.");
		}

		private void UpdatePlotObjects()
		{
			// Resize plot objects array if needed
			int currentPlotCount = turnManager.Plots.Count;
			if (plotObjects == null || plotObjects.Length < currentPlotCount)
			{
				System.Array.Resize(ref plotObjects, currentPlotCount);
			}

			// Create new plot objects for any missing ones
			for (int i = 0; i < currentPlotCount; i++)
			{
				if (plotObjects[i] == null)
				{
					GameObject go;
					if (plotPrefab != null)
					{
						go = Instantiate(plotPrefab, transform);
						go.name = $"Plot_{i + 1}";
					}
					else
					{
						go = new GameObject($"Plot_{i + 1}");
						go.transform.SetParent(transform);
						var sr = go.AddComponent<SpriteRenderer>();
						sr.sprite = squareSprite;
						sr.color = Color.gray;
						var col = go.AddComponent<BoxCollider2D>();
						col.isTrigger = true;
					}
					go.transform.position = new Vector3(plotStart.x + i * plotSpacing.x, plotStart.y + i * plotSpacing.y, 0f);
					go.transform.localScale = new Vector3(plotSize.x, plotSize.y, 1f);
					var view = go.GetComponent<NasaSpaceApps.FarmFromSpace.Game.UI.PlotView>();
					if (view == null) view = go.AddComponent<NasaSpaceApps.FarmFromSpace.Game.UI.PlotView>();
					view.plotIndex = i;
					view.runner = this;
					plotObjects[i] = go;
				}
			}
			UpdatePlotColors();
		}

		public void UI_ResolveOrNext()
		{
			if (turnManager == null) return;
			if (turnManager.Phase == TurnPhase.Planning)
			{
				turnManager.EndPlanningAndResolve();
			}
			else if (turnManager.Phase == TurnPhase.End)
			{
				turnManager.NextWeek();
			}
		}
	}
}


