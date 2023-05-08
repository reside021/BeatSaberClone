using NLayer;
using System.IO;
using System.Linq;
using UnityEngine;

public class AudioPlayerManager : MonoBehaviour
{
    private static AudioPlayerManager instance = null;

    public AudioSource audioSource;

    public AudioClip[] defaultTracks;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }
        if (instance == this) return;
        Destroy(gameObject);
    }

    private void Start()
    {
        var mySubFolder = Path.Combine(Application.persistentDataPath, "MusicForGame");
        if (!Directory.Exists(mySubFolder))
        {
            Directory.CreateDirectory(mySubFolder);
        }

        LoadDefaultTrack();
        LoadCustomTrack();
        PlayMusic();
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
                AudioClip track = LoadMp3(f);
                MusicData.audioClips.Add(track);
            });
    }


    public AudioClip LoadMp3(string filePath)
    {
        string filename = Path.GetFileNameWithoutExtension(filePath);

        MpegFile mpegFile = new MpegFile(filePath);

        AudioClip ac = AudioClip.Create(filename,
                                        (int)(mpegFile.Length / sizeof(float) / mpegFile.Channels),
                                        mpegFile.Channels,
                                        mpegFile.SampleRate,
                                        true,
                                        data => { int actualReadCount = mpegFile.ReadSamples(data, 0, data.Length); },
                                        position => { mpegFile = new MpegFile(filePath); });

        return ac;
    }

    private void PlayMusic()
    {
        var audioClip = MusicData.audioClips[Random.Range(0, MusicData.audioClips.Count - 1)];
        audioSource.clip = audioClip;
        audioSource.Play();
    }

}
