using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    [SerializeField]
    private GameObject _exitMenu;

    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void TrackEditor()
    {
        SceneManager.LoadScene("TrackEditor");
    }

    public void Records()
    {
        SceneManager.LoadScene("Records");
    }


    public void ExitMessageMenu()
    {
        _exitMenu.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Exit!");
    }

    public void CancelExit()
    {
        _exitMenu.SetActive(false);
    }

}
