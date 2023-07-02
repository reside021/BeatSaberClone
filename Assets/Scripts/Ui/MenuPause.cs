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
    public Button BtnGameOverRepeat;
    public Button BtnGameOverMenu;

    private GameObject _soundManager;
    private AudioSource _audioSource;

    void Start()
    {
        BtnPause.onClick.AddListener(PauseGame);
        BtnContinue.onClick.AddListener(ContinueGame);
        BtnRepeat.onClick.AddListener(RestartLevel);
        BtnMenu.onClick.AddListener(GoToMenu);
        BtnGameOverRepeat.onClick.AddListener(RestartLevel);
        BtnGameOverMenu.onClick.AddListener(GoToMenu);

        _soundManager = GameObject.FindGameObjectsWithTag("SoundManager")[0];
        _audioSource = _soundManager.GetComponent<AudioSource>();
    }

    private void PauseGame()
    {
        _audioSource.Pause();
        panelPause.SetActive(true);
        Time.timeScale = 0f;
    }

    private void ContinueGame()
    {
        panelPause.SetActive(false);
        Time.timeScale = 1f;
        _audioSource.UnPause();
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
        _audioSource.Stop();
        _audioSource.time = 0;
        _audioSource.Play();
    }

}
