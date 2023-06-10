using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SliderDragHandler : MonoBehaviour, IDragHandler
{
    public Slider sliderPlayingTrack;
    public TextMeshProUGUI nowLengthTrack;
    public TextMeshProUGUI lengthTrack;
    public Transform TrackRoad;
    private GameObject soundManager;
    private AudioSource audioSource;

    public void OnDrag(PointerEventData eventData)
    {
        var newtime = Math.Floor(sliderPlayingTrack.value * audioSource.clip.length);
        audioSource.time = (float)newtime;
        nowLengthTrack.text = TimeSpan.FromSeconds(audioSource.time).ToString(@"mm\:ss");
        var newPosition = (float)newtime * 10.1f;
        TrackRoad.position = new Vector3(newPosition, 0.0f, 0.0f);
    }


    void Start()
    {
        soundManager = GameObject.FindGameObjectsWithTag("SoundManager")[0];
        audioSource = soundManager.GetComponent<AudioSource>();
        lengthTrack.text = TimeSpan.FromSeconds(audioSource.clip.length).ToString(@"mm\:ss");
    }


    private void FixedUpdate()
    {
        if (audioSource.isPlaying)
        {
            sliderPlayingTrack.value = audioSource.time / audioSource.clip.length;
            nowLengthTrack.text = TimeSpan.FromSeconds(audioSource.time).ToString(@"mm\:ss");
        }
    }
}
