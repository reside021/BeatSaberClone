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
    private GameObject _soundManager;
    private AudioSource _audioSource;

    public void OnDrag(PointerEventData eventData)
    {
        var newtime = Math.Floor(sliderPlayingTrack.value * _audioSource.clip.length);
        _audioSource.time = (float)newtime;
        nowLengthTrack.text = TimeSpan.FromSeconds(_audioSource.time).ToString(@"mm\:ss");
        var newPosition = (float)newtime * 10.1f;
        TrackRoad.position = new Vector3(newPosition, 0.0f, 0.0f);
    }


    void Start()
    {
        _soundManager = GameObject.FindGameObjectsWithTag("SoundManager")[0];
        _audioSource = _soundManager.GetComponent<AudioSource>();
        lengthTrack.text = TimeSpan.FromSeconds(_audioSource.clip.length).ToString(@"mm\:ss");
    }


    private void FixedUpdate()
    {
        if (_audioSource.isPlaying)
        {
            sliderPlayingTrack.value = _audioSource.time / _audioSource.clip.length;
            nowLengthTrack.text = TimeSpan.FromSeconds(_audioSource.time).ToString(@"mm\:ss");
        }
    }
}
