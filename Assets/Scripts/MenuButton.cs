using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void ExitGame()
    {
        // TODO: Сделать перевод на сцену с уточнением хочет ли игрок выйти и закрыть игру
        Application.Quit();
        Debug.Log("Exit pressed!");
    }

    public void TrackEditor()
    {
        // TODO: Сделать переход на сцену редактора треков
        //SceneManager.LoadScene("TrackEditor");
    }

    public void Records()
    {
        // TODO: Сделать переход на сцену рекордов
        //SceneManager.LoadScene("Records");
    }

}
