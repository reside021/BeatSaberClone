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

    private GameObject soundManager;
    private AudioSource audioSource;

    void Start()
    {
        BtnPause.onClick.AddListener(PauseGame);
        BtnContinue.onClick.AddListener(ContinueGame);
        BtnRepeat.onClick.AddListener(RestartLevel);
        BtnMenu.onClick.AddListener(GoToMenu);

        soundManager = GameObject.FindGameObjectsWithTag("SoundManager")[0];
        audioSource = soundManager.GetComponent<AudioSource>();
    }

    private void PauseGame()
    {
        audioSource.Pause();
        panelPause.SetActive(true);
        Time.timeScale = 0f;
    }

    private void ContinueGame()
    {
        panelPause.SetActive(false);
        Time.timeScale = 1f;
        audioSource.Play();
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
        audioSource.Stop();
        audioSource.Play();
    }

}
