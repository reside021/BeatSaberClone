using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Linq;

public class LoadMusic : MonoBehaviour
{
    public RectTransform itemPrefab;

    public RectTransform contentArea;

    public TextMeshProUGUI trackNowPlayingText;


    public GameObject checkMark;
    public GameObject crossMark;
    private GameObject soundManager;

    private AudioSource audioSource;


    private List<string> filesMap = new List<string>();

    private void Start()
    {
        soundManager = GameObject.FindGameObjectsWithTag("SoundManager")[0];
        audioSource = soundManager.GetComponent<AudioSource>();

        trackNowPlayingText.text = audioSource.clip.name;

        CheckHaveMap();

        UpdateItems();
    }

    private void CheckHaveMap()
    {
        var mySubFolder = Path.Combine(Application.persistentDataPath, "dataLVL");
        if (!Directory.Exists(mySubFolder))
            return;

        Directory.GetFiles(mySubFolder).ToList().ForEach(file => { filesMap.Add(Path.GetFileName(file)); }); 
    }

    private void UpdateItems()
    {
        ClearMusicList();
        LoadTrackInList();
        
    }

    private void ClearMusicList()
    {
        foreach (Transform itemMusic in contentArea)
        {
            Destroy(itemMusic.gameObject);
        }
    }

    private void LoadTrackInList()
    {
        foreach (var track in MusicData.audioClips)
        {
            GameObject instance = Instantiate(itemPrefab.gameObject, contentArea);
            InitializeItemView(instance, track);
        } 
    }


    private void InitializeItemView(GameObject gameObject, AudioClip audioClip)
    {
        gameObject.transform.GetChild(0)
            .GetComponent<TextMeshProUGUI>()
            .text = audioClip.name;

        gameObject.transform.GetChild(1)
            .GetComponent<Button>()
            .onClick
            .AddListener(() =>
                    {
                        audioSource.clip = audioClip;
                        trackNowPlayingText.text = audioClip.name;
                        audioSource.time = 0;
                        audioSource.Play();
                    });

        var imageCheck = gameObject.transform.GetChild(2).Find("ImageCheck");
        var imageCross = gameObject.transform.GetChild(2).Find("ImageCross");

        if (filesMap.Any(x => x.Equals($"{audioClip.name}.dat")))
        {
            imageCross.gameObject.SetActive(false);
            imageCheck.gameObject.SetActive(true);
        }
        
    }

}
