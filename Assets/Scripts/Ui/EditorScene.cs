using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;

public class EditorScene : MonoBehaviour
{
    public GameObject ElementTrackPreafab;
    public GameObject RedCubePrefab;
    public GameObject BlueCubePrefab;
    public GameObject BombPrefab;


    private GameObject _soundManager;

    private AudioSource _audioSource;

    private Rigidbody _rb;

    private Vector3 _endTrackRoad = Vector3.zero;

    private int _heightScreen;

    private bool _isDragging = false;
    private bool _wasPlaying = false;

    private float _prevPos = 0f;

    private void Awake()
    {
        _soundManager = GameObject.FindGameObjectsWithTag("SoundManager")[0];
        _audioSource = _soundManager.GetComponent<AudioSource>();
    }
    void Start()
    {
        SaveData.dataLvL.Clear();
        LoadGame();
        CreateTrackRoad();
        _rb = GetComponent<Rigidbody>();
        _heightScreen = Screen.height;
    }

    private void LoadGame()
    {
        var mySubFolder = Path.Combine(Application.persistentDataPath, "dataLVL");
        if (!Directory.Exists(mySubFolder))
            return;

        var myFile = mySubFolder + $"/{_audioSource.clip.name}.dat";

        if (!File.Exists(myFile))
            return;

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(myFile, FileMode.Open);
        SaveData.dataLvL = bf.Deserialize(file) as Dictionary<int, ItemTrack>;
        file.Close();
    }

    private void CreateTrackRoad()
    {
        int lengthTrack = Mathf.RoundToInt(_audioSource.clip.length);

        var blockOffset = 10.1f;

        Vector3 position = new Vector3(0, 0, 0);

        for (var i = 0; i < lengthTrack; i++)
        {
            var item = Instantiate(ElementTrackPreafab, position, Quaternion.identity, transform);

            if (SaveData.dataLvL.ContainsKey(i))
            {
                var dataAboutElement = SaveData.dataLvL[i];

                var blockPositionUnder = GetVector3(dataAboutElement.BlockPositionUnder);
                var blockPositionUpper = GetVector3(dataAboutElement.BlockPositionUpper);
                var typeBlockUnder = dataAboutElement.typeBlockUnder;
                var typeBlockUpper = dataAboutElement.typeBlockUpper;

                var underLVL = item.transform.Find("Under");
                var upperLVL = item.transform.Find("Upper");

                for (var j = 0; j < 12; j++)
                {
                    if (blockPositionUnder[j] != Vector3.zero)
                    {
                        var currentPrefab = GetCurrentPrefab(typeBlockUnder[j]);

                        var underBlock = Instantiate(currentPrefab, blockPositionUnder[j], Quaternion.identity, underLVL);
                        underBlock.transform.localPosition = blockPositionUnder[j];
                    }

                    if (blockPositionUpper[j] != Vector3.zero)
                    {
                        var currentPrefab = GetCurrentPrefab(typeBlockUpper[j]);

                        var upperBlock = Instantiate(currentPrefab, blockPositionUpper[j], Quaternion.identity, upperLVL);
                        upperBlock.transform.localPosition = blockPositionUpper[j];
                    }
                }
            }

            var textNumItem = item.GetComponentInChildren<TextMeshPro>();
            textNumItem.text = i.ToString();
            if (i < 3)
                item.GetComponent<MeshRenderer>().materials[0].color = Color.gray;

            position.x -= blockOffset;
        }

        _endTrackRoad = -(position += new Vector3(blockOffset, 0.0f, 0.0f));
    }

    GameObject GetCurrentPrefab(TypeBlock typeBlock)
    {
        GameObject result = typeBlock switch
        {
            TypeBlock.Blue => BlueCubePrefab,
            TypeBlock.Red => RedCubePrefab,
            TypeBlock.Bomb => BombPrefab
        };
        return result;
    }

    private Vector3[] GetVector3(CustomVector3[] vectorForTrans)
    {
        Vector3[] tempArray = new Vector3[12];

        for (var i = 0; i < 12; i++)
        {
            tempArray[i] = new Vector3(vectorForTrans[i].x, vectorForTrans[i].y, vectorForTrans[i].z);
        }

        return tempArray;
    }

    private void FixedUpdate()
    {

        if (_isDragging)
        {
            var offset = _prevPos - Input.mousePosition.y;

            var coef = offset / _heightScreen * 1.5f;

            var newX = transform.position.x + coef;

            transform.position = new Vector3(newX, 0.0f, 0.0f);
        }

        if (_rb.position.x < -0.1f)
        {
            SmoothDrag(Vector3.zero, Time.fixedDeltaTime * 5);
        }

        if (_rb.position.x > (_endTrackRoad.x + 0.1f))
        {
            SmoothDrag(_endTrackRoad, Time.fixedDeltaTime * 10);
        }
    }


    private void OnMouseDrag()
    {
        _isDragging = true;
    }

    public void SmoothDrag(Vector3 targetPosition, float elapsed)
    {
        var oldPosition = _rb.position;

        var remainingPosition = targetPosition - oldPosition;
        var newRemainingPosition = Vector3.Lerp(remainingPosition, Vector3.zero, elapsed);

        _rb.position = oldPosition + remainingPosition - newRemainingPosition;
    }


    private void OnMouseDown()
    {
        _wasPlaying = _audioSource.isPlaying;
        _audioSource.Pause();
        _prevPos = Input.mousePosition.y;
    }

    private void OnMouseUp()
    {
        if (_wasPlaying)
            _audioSource.Play();
        _isDragging = false;
        _wasPlaying = _audioSource.isPlaying;
    }

}
