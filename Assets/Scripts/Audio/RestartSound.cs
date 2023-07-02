using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartSound : MonoBehaviour
{
    private GameObject soundManager;
    private AudioSource audioSource;
    void Start()
    {
        soundManager = GameObject.FindGameObjectsWithTag("SoundManager")[0];
        audioSource = soundManager.GetComponent<AudioSource>();
        audioSource.Stop();
        audioSource.time = 0;
        if (SceneManager.GetActiveScene().name == "Game")
            return;
        audioSource.Play();
    }

}
