using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour
{
    public GameObject _exitMenu;

    public Button BtnPlay;
    public Button BtnEditor;
    public Button BtnRecords;
    public Button BtnExit;
    public Button BtnAccept;
    public Button BtnCancel;

    private void Start()
    {
        BtnPlay.onClick.AddListener(PlayGame);
        BtnEditor.onClick.AddListener(TrackEditor);
        BtnRecords.onClick.AddListener(OpenRecords);
        BtnExit.onClick.AddListener(ExitMessageMenu);
        BtnAccept.onClick.AddListener(ExitGame);
        BtnCancel.onClick.AddListener(CancelExit);
    }

    private void PlayGame()
    {
        SceneManager.LoadScene("Game");
    }

    private void TrackEditor()
    {
        SceneManager.LoadScene("TrackEditor");
    }

    private void OpenRecords()
    {
        SceneManager.LoadScene("Records");
    }


    private void ExitMessageMenu()
    {
        _exitMenu.SetActive(true);
    }

    private void ExitGame()
    {
        Application.Quit();
        Debug.Log("Exit!");
    }

    private void CancelExit()
    {
        _exitMenu.SetActive(false);
    }

}
