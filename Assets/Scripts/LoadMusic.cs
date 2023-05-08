using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using NLayer;

public class LoadMusic : MonoBehaviour
{
    public RectTransform itemPrefab;

    public RectTransform contentArea;

    public TextMeshProUGUI trackNowPlayingText;

    private GameObject soundManager;
    private AudioSource audioSource;

    private void Start()
    {
        soundManager = GameObject.FindGameObjectsWithTag("SoundManager")[0];
        audioSource = soundManager.GetComponent<AudioSource>();

        trackNowPlayingText.text = audioSource.clip.name;

        UpdateItems();
    }

    public void UpdateItems()
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
        foreach (Transform child in gameObject.transform)
        {
            if (child.tag == "TrackName")
            {
                child.GetComponent<TextMeshProUGUI>().text = audioClip.name;
            }
            else
            {
                child.GetComponent<Button>().onClick.AddListener(
                    () =>
                    {
                        audioSource.clip = audioClip;
                        trackNowPlayingText.text = audioClip.name;
                        audioSource.Play();
                    }
                );
            }
        }
    }

}
