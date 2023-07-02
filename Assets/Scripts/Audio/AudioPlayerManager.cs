using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class AudioPlayerManager : MonoBehaviour
{
    private static AudioPlayerManager instance = null;

    public AudioSource audioSource;

    public AudioClip[] defaultTracks;


    private Action<AudioClip> onResponsed;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            var mySubFolder = Path.Combine(Application.persistentDataPath, "MusicForGame");
            if (!Directory.Exists(mySubFolder))
            {
                Directory.CreateDirectory(mySubFolder);
            }

            MusicData.audioClips.Clear();

            LoadDefaultTrack();
            LoadCustomTrack();
            SetStartTrack();

            return;
        }
        if (instance == this)
            return;
        Destroy(gameObject);
    }

    private void OnEnable()
    {
        onResponsed += LoadMusic;
    }

    private void OnDisable()
    {
        onResponsed -= LoadMusic;
    }


    private void LoadDefaultTrack()
    {
        MusicData.audioClips.AddRange(defaultTracks);
    }

    private void LoadCustomTrack()
    {
        var mySubFolder = Path.Combine(Application.persistentDataPath, "MusicForGame");

        Directory
            .GetFiles(mySubFolder, "*.mp3", SearchOption.AllDirectories)
            .ToList()
            .ForEach(f =>
            {
                string filePath = "file:///" + f;
                StartCoroutine(LoadAudioFromServer(filePath, AudioType.MPEG, onResponsed));
            });
    }



    public void LoadMusic(AudioClip track)
    {
        if (track == null) return;
        MusicData.audioClips.Add(track);
    }

    IEnumerator LoadAudioFromServer(string filePath, AudioType audioType, Action<AudioClip> response)
    {
        using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(filePath, audioType))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                var audioClip = DownloadHandlerAudioClip.GetContent(request);
                audioClip.name = fileName;
                response(audioClip);
            }
            else
            {
                Debug.LogErrorFormat("error request [{0}, {1}]", filePath, request.error);

                response(null);
            }
        }

    }

    private void SetStartTrack()
    {
        var audioClip = MusicData.audioClips[UnityEngine.Random.Range(0, MusicData.audioClips.Count - 1)];
        audioSource.clip = audioClip;
        audioSource.Play();
    }

}
