using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private int GameSceneNum = 1;

    [SerializeField] private GameObject InfoPanel;

    private void Start()
    {
        InfoPanel.SetActive(false);
    }

    public void ClickStartBTN()
    {
        CancelInvoke();
        SceneManager.LoadScene(GameSceneNum);
    }

    public void ClickInfoBTN()
    {
        if (InfoPanel.activeSelf) InfoPanel.SetActive(false);
        else InfoPanel.SetActive(true);
    }

    public void ClickQuitBTN()
    {
        CancelInvoke();
        Application.Quit();
    }
}
