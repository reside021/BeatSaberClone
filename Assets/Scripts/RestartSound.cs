using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestartSound : MonoBehaviour
{
    private GameObject soundManager;
    private AudioSource audioSource;
    void Start()
    {
        soundManager = GameObject.FindGameObjectsWithTag("SoundManager")[0];
        audioSource = soundManager.GetComponent<AudioSource>();
        audioSource.Stop();
        audioSource.Play();
    }

}
