using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SimpleFileBrowser;
using System.IO;
using System.Collections;

public class TrackEditorButton : MonoBehaviour
{
    public Button BtnBack;


    private void Start()
    {
        BtnBack.onClick.AddListener(BackToMenu);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }


}
