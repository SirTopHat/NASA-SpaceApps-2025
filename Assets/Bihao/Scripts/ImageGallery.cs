using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ImageGallery : MonoBehaviour
{
    public Sprite[] images;            // Put your images here in the Inspector
    public Image displayImage;         // The UI Image component to show them
    public Button nextButton;          // Assign your Next button
    public Button prevButton;          // Assign your Previous button

    private int index = 0;

    void Start()
    {
        ShowImage();
        UpdateButtons();
        
        if (nextButton != null)
            nextButton.onClick.AddListener(NextImage);
        if (prevButton != null)
            prevButton.onClick.AddListener(PrevImage);
    }

    void ShowImage()
    {
        if (images.Length > 0)
        {
            displayImage.sprite = images[index];
        }
    }

    void NextImage()
    {
        if (index < images.Length - 1)
        {
            index++;
            ShowImage();
            UpdateButtons();
        }
    }

    void PrevImage()
    {
        if (index > 0)
        {
            index--;
            ShowImage();
            UpdateButtons();
        }
    }

    void UpdateButtons()
    {
        // Check if prevButton is assigned before accessing it
        if (prevButton != null)
            // Disable Prev button if at first image
            prevButton.interactable = index > 0;

        // Check if nextButton is assigned before accessing it
        if (nextButton != null)
            // Disable Next button if at last image
            nextButton.interactable = index < images.Length - 1;
    }
}
