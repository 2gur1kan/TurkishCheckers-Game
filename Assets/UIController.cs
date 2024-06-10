using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject UIPanel;
    [SerializeField] private TextMeshProUGUI TourText;
    [SerializeField] private GameObject SkipButton;
    [SerializeField] private GameObject SoundSlider;
    [SerializeField] private AudioSource SMAS;

    [Header("- Finish Panel -")]
    [SerializeField] private GameObject FinsihPanel;
    [SerializeField] private GameObject Win;
    [SerializeField] private GameObject Lose;
    [SerializeField] private GameObject Scoreless;

    [SerializeField] private GameObject Panel;

    [SerializeField] private int GameSceneNum = 1;

    private DamaController DC;

    private void Start()
    {
        DC = GameObject.FindGameObjectWithTag("GameController").GetComponent<DamaController>();
        SMAS = SoundManager.Instance.GetComponent<AudioSource>();
        SoundSlider.GetComponent<Slider>().value = SMAS.volume;

        Panel.SetActive(false);
        SoundSlider.SetActive(false);
        ResetButton();
    }

    private void SetSoundVolume()
    {
        SMAS.volume = SoundSlider.GetComponent<Slider>().value;
    }

    public void BackMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    public void StopGame()
    {
        if(!Panel.activeSelf)
        {
            Time.timeScale = 0;
            Panel.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            Panel.SetActive(false);
        }
    }

    public void QuitGame()
    {
        CancelInvoke();
        Application.Quit();
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

    public void ClickSoundButton()
    {
        if (SoundSlider.activeSelf)
        {
            SoundSlider.SetActive(false);
            CancelInvoke("SetSoundVolume");
        }
        else
        {
            SoundSlider.SetActive(true);
            InvokeRepeating("SetSoundVolume", .1f, .1f);
        }
    }

    public bool IsFinsih(bool playerWin, bool scoreless = false)
    {
        Win.SetActive(false);
        Lose.SetActive(false);
        Scoreless.SetActive(false);

        UIPanel.SetActive(false);
        FinsihPanel.SetActive(true);

        if (scoreless)
        {
            Scoreless.SetActive(true);
            return true;
        }

        if (playerWin) Win.SetActive(true);
        else Lose.SetActive(true);

        return true;
    }

    public void ClickRestartButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(GameSceneNum);
    }

    public void UpdateTourText(bool tour)
    {
        if (tour) TourText.text = "Player Tour";
        else TourText.text = "AI Tour";
    }
}
