using UnityEngine;
using NasaSpaceApps.FarmFromSpace.Common;

namespace NasaSpaceApps.FarmFromSpace.Content
{
	[CreateAssetMenu(fileName = "Card", menuName = "FarmFromSpace/Content/Card", order = 0)]
	public class CardDefinition : ScriptableObject
	{
		public string displayName;
		public CardType cardType;
		[Tooltip("Action point cost to play this card in a week.")]
		[Range(0, 3)] public int actionPointCost = 1;

		[Header("Parameters")]
		[Tooltip("Generic magnitude parameter: e.g., irrigation mm, mulch ET reduction, etc.")]
		public float magnitude;

		[Tooltip("Optional secondary parameter.")]
		public float parameterB;

		[TextArea]
		public string tooltip;
	}
}


