using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI TourText;
    [SerializeField] private GameObject SkipButton;

    private DamaController DC;

    private void Start()
    {
        DC = GameObject.FindGameObjectWithTag("GameController").GetComponent<DamaController>();
        ResetButton();
    }

    public void ResetButton()
    {
        SkipButton.SetActive(false);
    }

    public void SetButton()
    {
        SkipButton.SetActive(true);
    }

    public void ClickSkipButton()
    {
        DC.SkipTour();

        ResetButton();
    }

    public void UpdateTourText(bool tour)
    {
        if (tour) TourText.text = "Player Tour";
        else TourText.text = "AI Tour";
    }
}
