using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Blade : MonoBehaviour
{

    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI NumberComboText;
    public TextMeshProUGUI ProgressProcent;
    public TextMeshProUGUI FinalScore;
    public TextMeshProUGUI MaxCombo;
    public TextMeshProUGUI DestroyCubeProcent;

    public GameObject EndGameDisplay;

    private Camera mainCamera;
    private Collider bladeCollider;
    private TrailRenderer bladeTrail;
    private GameObject _soundManager;
    private AudioSource _audioSource;

    public float minSliceVelocity = 0.01f;

    public static int combo;
    private bool slicing;
    private readonly int _layerMaskOnlyCube = 1 << 7;
    private double _score;
    private const int _point = 1;
    private int _maxCombo = 0;
    private float _destroyBlocks;

    public Vector3 direction
    {
        get; private set;
    }
    private void Awake()
    {
        _soundManager = GameObject.FindGameObjectsWithTag("SoundManager")[0];
        _audioSource = _soundManager.GetComponent<AudioSource>();
        mainCamera = Camera.main;
        bladeCollider = GetComponent<Collider>();
        bladeTrail = GetComponentInChildren<TrailRenderer>();
    }
    private void Start()
    {
        _destroyBlocks = 0.0f;
        _score = 0;
        combo = 1;
    }

    private void OnEnable()
    {
        StopSlicing();
    }

    private void OnDisable()
    {
        StopSlicing();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartSlicing();
        } else if (Input.GetMouseButtonUp(0))
        {
             StopSlicing();
        } else if (slicing)
        {
            ContinueSlicing();
        }
        NumberComboText.text = combo.ToString();
        ScoreText.text = _score.ToString();

        DisplayProgressProcent();
    }

    private void StartSlicing()
    {
        var mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.5f);
        var newPosition = mainCamera.ScreenToWorldPoint(mousePosition);
        transform.position = newPosition;

        slicing = true;
        bladeCollider.enabled = true;
        bladeTrail.enabled = true;
        bladeTrail.Clear();
    }

    private void StopSlicing()
    {
        slicing = false;
        bladeCollider.enabled = false;
        bladeTrail.enabled = false;
    }

    private void ContinueSlicing()
    {
        var mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0.5f);
        Vector3 newPosition = mainCamera.ScreenToWorldPoint(mousePosition);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(ray.origin, ray.direction);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 1.5f, _layerMaskOnlyCube))
        {
            if (hit.collider.tag.Equals("Set_Cube"))
            {
                Slice(hit.collider.transform);
            }
        }

        direction = newPosition - transform.position;

        float velocity = direction.magnitude / Time.deltaTime;

        bladeCollider.enabled = velocity > minSliceVelocity;

        transform.position = newPosition;
    }

    private void Slice(Transform set_cube)
    {
        var cubeCollider = set_cube.gameObject.GetComponent<Collider>();
        var cubeRigidbody = set_cube.gameObject.GetComponent<Rigidbody>();
        var audioSource = set_cube.gameObject.GetComponent<AudioSource>();
        audioSource.Play();
        var whole = set_cube.GetChild(0).gameObject;
        var sliced = set_cube.GetChild(1).gameObject;

        whole.SetActive(false);
        sliced.SetActive(true);

        cubeCollider.enabled = false;

        Rigidbody[] slices = sliced.GetComponentsInChildren<Rigidbody>();

        foreach (var slice in slices)
        {
            slice.AddExplosionForce(100, slice.transform.position, 20);
        }
        AddScoreAndCombo();
        _destroyBlocks++;
    }

    void DisplayProgressProcent()
    {
        var length = _audioSource.clip.length - _audioSource.clip.length / 100.0f;
        var value = Math.Truncate(_audioSource.time / length * 100);
        if (value == 101)
        {
            DisplayGameOver();
            return;
        }
        ProgressProcent.text = value.ToString();
    }

    void DisplayGameOver()
    {
        _audioSource.Stop();
        Time.timeScale = 0f;
        FinalScore.text = ScoreText.text;
        MaxCombo.text = _maxCombo.ToString();
        var destroyCubeProcent = Math.Truncate(_destroyBlocks / Spawner.TotalBlocks * 100);
        DestroyCubeProcent.text = $"{destroyCubeProcent} %";
        EndGameDisplay.SetActive(true);
    }
    private void AddScoreAndCombo()
    {
        _score += _point * combo;
        if (_score % 5 == 0)
            combo++;
        if (combo > _maxCombo)
            _maxCombo = combo;
    }
}
