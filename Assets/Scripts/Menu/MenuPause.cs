using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPause : MonoBehaviour
{
    public GameObject panelPause;

    public Button BtnPause;
    public Button BtnContinue;
    public Button BtnRepeat;
    public Button BtnMenu;

    void Start()
    {
        BtnPause.onClick.AddListener(PauseGame);
        BtnContinue.onClick.AddListener(ContinueGame);
        BtnRepeat.onClick.AddListener(RestartLevel);
        BtnMenu.onClick.AddListener(GoToMenu);
    }

    private void PauseGame()
    {
        panelPause.SetActive(true);
        Time.timeScale = 0f;
    }

    private void ContinueGame()
    {
        panelPause.SetActive(false);
        Time.timeScale = 1f;
    }

    private void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f;
    }

    private void GoToMenu()
    {
        SceneManager.LoadScene("Menu");
        Time.timeScale = 1f;
    }

}
