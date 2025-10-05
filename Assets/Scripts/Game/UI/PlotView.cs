using UnityEngine;

namespace NasaSpaceApps.FarmFromSpace.Game.UI
{
	[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
	public class PlotView : MonoBehaviour
	{
		public int plotIndex;
		public Game.GameRunner runner;

		private SpriteRenderer sr;

		private void Awake()
		{
			sr = GetComponent<SpriteRenderer>();
		}

		private void OnMouseDown()
		{
			if (runner != null)
			{
				runner.UI_SelectPlot(plotIndex);
			}
		}

		public void SetColor(Color c)
		{
			if (sr == null) sr = GetComponent<SpriteRenderer>();
			sr.color = c;
		}
	}
}


