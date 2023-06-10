using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System.Collections;

public class TrackEditorButton : MonoBehaviour
{
    public Button BtnBack;
    public Button BtnOpenLevelEditor;

    private void Start()
    {
        BtnBack.onClick.AddListener(BackToMenu);
        BtnOpenLevelEditor.onClick.AddListener(OpenEditor);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
    public void OpenEditor()
    {
        SceneManager.LoadScene("Editor");
    }


}
