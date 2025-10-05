using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NasaSpaceApps.FarmFromSpace.Game.UI
{
	/// <summary>
	/// Simple button controller for toggling NASA Data Card visibility in endless mode
	/// Attach this to a button to enable/disable the NASA Data Card display
	/// </summary>
	public class EndlessNasaDataCardToggleButton : MonoBehaviour
	{
		[Header("References")]
		[SerializeField] private EndlessNasaDataCard nasaDataCard;
		[SerializeField] private Button toggleButton;
		[SerializeField] private TextMeshProUGUI buttonText;

		[Header("Button Text")]
		[SerializeField] private string showText = "Show NASA Data";
		[SerializeField] private string hideText = "Hide NASA Data";

		private void Start()
		{
			// Get references if not assigned
			if (nasaDataCard == null)
				nasaDataCard = FindFirstObjectByType<EndlessNasaDataCard>();
			
			if (toggleButton == null)
				toggleButton = GetComponent<Button>();
			
			if (buttonText == null)
				buttonText = GetComponentInChildren<TextMeshProUGUI>();

			// Wire up button click
			if (toggleButton != null)
			{
				toggleButton.onClick.AddListener(OnToggleClicked);
			}

			// Update button text based on initial state
			UpdateButtonText();
		}

		private void OnToggleClicked()
		{
			if (nasaDataCard != null)
			{
				nasaDataCard.ToggleDataCard();
				UpdateButtonText();
			}
		}

		private void UpdateButtonText()
		{
			if (buttonText != null && nasaDataCard != null)
			{
				buttonText.text = nasaDataCard.IsDataCardVisible ? hideText : showText;
			}
		}

		// Public methods for external control
		public void ForceShow()
		{
			if (nasaDataCard != null)
			{
				nasaDataCard.ShowDataCard();
				UpdateButtonText();
			}
		}

		public void ForceHide()
		{
			if (nasaDataCard != null)
			{
				nasaDataCard.HideDataCard();
				UpdateButtonText();
			}
		}
	}
}
