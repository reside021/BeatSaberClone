using UnityEngine;
using EzySlice;
using TMPro;
using System;

public class Slicer : MonoBehaviour
{
    public Transform CutPlane;
    public Material CrossMaterial;

    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI NumberComboText;
    public TextMeshProUGUI ProgressProcent;
    public TextMeshProUGUI FinalScore;
    public TextMeshProUGUI MaxCombo;
    public TextMeshProUGUI DestroyCubeProcent;

    public GameObject EndGameDisplay;

    private GameObject _soundManager;
    private AudioSource _audioSource;


    public static int combo;
    private double _score;
    private const int _point = 1;
    private int _maxCombo = 0;
    private float _destroyBlocks;

    private void Awake()
    {
        _soundManager = GameObject.FindGameObjectsWithTag("SoundManager")[0];
        _audioSource = _soundManager.GetComponent<AudioSource>();
    }

    private void Start()
    {
        _destroyBlocks = 0.0f;
        _score = 0;
        combo = 1;
    }

    private void FixedUpdate()
    {
        var mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, transform.position.z);
        Vector3 objPosition = Camera.main.ScreenToWorldPoint(mousePosition);    
        transform.position = objPosition;

        NumberComboText.text = combo.ToString();
        ScoreText.text = _score.ToString();

        DisplayProgressProcent();
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

    private void OnCollisionEnter(Collision otherObj)
    {
        if (otherObj.gameObject.CompareTag("Cube"))
        {
            var angleForCut = Mathf.Atan2(transform.position.y, transform.position.x) * (180 / Mathf.PI);
            CutPlane.transform.eulerAngles = new Vector3(0, 0, angleForCut);


            SlicedHull hull = SliceObject(otherObj.gameObject, CrossMaterial);

            if (hull != null)
            {
                GameObject bottom = hull.CreateLowerHull(otherObj.gameObject, CrossMaterial);
                GameObject top = hull.CreateUpperHull(otherObj.gameObject, CrossMaterial);
                AddHullComponents(bottom);
                AddHullComponents(top);
                Destroy(otherObj.gameObject);
                AddScoreAndCombo();
                _destroyBlocks++;
            }


        }
    }

    private void AddScoreAndCombo()
    {
        _score += _point * combo;
        if (_score % 5 == 0) 
            combo++;
        if (combo > _maxCombo)
            _maxCombo = combo;
    }

    private void AddHullComponents(GameObject go)
    {
        Rigidbody rb = go.AddComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        MeshCollider collider = go.AddComponent<MeshCollider>();
        collider.convex = true;
        go.AddComponent<Destroyer>();
        rb.AddExplosionForce(100, go.transform.position, 20);
        rb.AddForce(Vector3.back * 3, ForceMode.Impulse);
    }

    private SlicedHull SliceObject(GameObject obj, Material crossSectionMaterial = null)
    {
        if (obj.GetComponent<MeshFilter>() == null)
            return null;

        return obj.Slice(CutPlane.position, CutPlane.up, crossSectionMaterial);
    }

}
