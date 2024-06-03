using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject UIPanel;
    [SerializeField] private TextMeshProUGUI TourText;
    [SerializeField] private GameObject SkipButton;

    [SerializeField] private GameObject FinsihPanel;
    [SerializeField] private GameObject Win;
    [SerializeField] private GameObject Lose;

    [SerializeField] private int GameSceneNum = 1;

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

    public bool IsFinsih(bool playerWin)
    {
        UIPanel.SetActive(false);
        FinsihPanel.SetActive(true);

        if (playerWin)
        {
            Win.SetActive(true);
            Lose.SetActive(false);
        }
        else
        {
            Win.SetActive(false);
            Lose.SetActive(true);
        }

        return true;
    }

    public void ClickRestartButton()
    {
        SceneManager.LoadScene(GameSceneNum);
    }

    public void UpdateTourText(bool tour)
    {
        if (tour) TourText.text = "Player Tour";
        else TourText.text = "AI Tour";
    }
}
