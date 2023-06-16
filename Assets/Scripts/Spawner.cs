using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;


enum PlaceSpawn
{
    Left,
    Center,
    Right,
    TopLeft, 
    TopCenter,
    TopRight,
    None
}

class ElementForSpawn
{
    public int index;
    public int row;
    public int nextIndex;
    public int nextRow;
    public List<Dictionary<PlaceSpawn, TypeBlock>> places;

    public ElementForSpawn(int index, int row, int nextIndex, int nextRow, List<Dictionary<PlaceSpawn, TypeBlock>> places)
    {
        this.index = index;
        this.row = row;
        this.nextIndex = nextIndex;
        this.nextRow = nextRow;
        this.places = places;
    }
}

public class Spawner : MonoBehaviour
{
    public GameObject SpawnSectorPrefab;
    public GameObject CubePrefab;
    public GameObject RedCubePrefab;
    public GameObject BlueCubePrefab;
    public GameObject BombPrefab;

    private GameObject _soundManager;

    private AudioSource _audioSource;

    private readonly float _speed = 2f;

    private Dictionary<int, ItemTrack> _blockDataOnLvL;
    private List<ElementForSpawn> _elementForSpawns;

    public static float TotalBlocks;

    private void Awake()
    {
        _soundManager = GameObject.FindGameObjectsWithTag("SoundManager")[0];
        _audioSource = _soundManager.GetComponent<AudioSource>();
    }

    void Start()
    {
        TotalBlocks = 0.0f;
        _blockDataOnLvL = new Dictionary<int, ItemTrack>();
        _elementForSpawns = new List<ElementForSpawn>();
        LoadLvL();
        CreateDataAboutCubeForSpawns();
        if (_blockDataOnLvL.Count == 0) return;
        CreateCubeLine();
        StartCoroutine(PlayMusic());
    }

    void FixedUpdate()
    {
        MovementLogic();
    }

    IEnumerator PlayMusic()
    {
        yield return new WaitForSeconds(2.0f);
        _audioSource.Play();
    }

    private void MovementLogic()
    {
        Vector3 movement = new Vector3(0f, 0.0f, -1f);

        transform.Translate(movement * _speed * Time.fixedDeltaTime);
    }

    void CreateCubeLine()
    {
        float spawnPoint = 0.0f;
        foreach (var el in _elementForSpawns)
        {
            if (el.index != 0 || el.index == 0 && el.row != 0)
            {
                var sp = el.index * 2.0f + el.row * 0.5f;
                spawnPoint = sp;
            }

            var newPosition = new Vector3(0.0f, 0.0f, spawnPoint + 0.25f);
            var newSector = Instantiate(SpawnSectorPrefab, transform).transform;
            newSector.localPosition = newPosition;

            foreach (var place in el.places)
            {
                var dict = place.First();
                var transformForSpawn = TranslatePositionPlacesInCoordinates(dict.Key, newSector);
                var currentPrefab = GetCurrentPrefab(dict.Value);
                var newCube = Instantiate(currentPrefab, transformForSpawn).transform;
                newCube.localPosition = Vector3.zero;
                newCube.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                TotalBlocks++;
            }

            if (el.index != el.nextIndex)
            {
                var addPoint = (float)(el.nextIndex - el.index);
                addPoint *= 2;
                spawnPoint += addPoint;
            }

            if (el.index == el.nextIndex)
            {
                if (el.row != el.nextRow)
                {
                    var addPoint = (float)(el.nextRow - el.row);
                    addPoint *= 0.5f;
                    spawnPoint += addPoint;
                }
            }
        }
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

    Transform TranslatePositionPlacesInCoordinates(PlaceSpawn placeSpawn, Transform newSector)
    {
        Transform result = placeSpawn switch
        {
            PlaceSpawn.Center => newSector.GetChild(0),
            PlaceSpawn.Right => newSector.GetChild(1),
            PlaceSpawn.Left => newSector.GetChild(2),
            PlaceSpawn.TopCenter => newSector.GetChild(3),
            PlaceSpawn.TopLeft => newSector.GetChild(4),
            PlaceSpawn.TopRight => newSector.GetChild(5),
        };

        return result;
    }

    private void LoadLvL()
    {
        var mySubFolder = Path.Combine(Application.persistentDataPath, "dataLVL");
        if (!Directory.Exists(mySubFolder))
            return;

        var myFile = mySubFolder + $"/{_audioSource.clip.name}.dat";

        if (!File.Exists(myFile))
            return;

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(myFile, FileMode.Open);
        _blockDataOnLvL = bf.Deserialize(file) as Dictionary<int, ItemTrack>;
        file.Close();
    }


    private void CreateDataAboutCubeForSpawns()
    {
        var sizeDict = _blockDataOnLvL.Count;

        for (var i = 0; i < sizeDict; i++)
        {
            var itemTrack = _blockDataOnLvL[i];

            var nextIndex = i + 1 == sizeDict ? i : i + 1;

            var up = GetVector3(itemTrack.BlockPositionUpper);
            var upType = itemTrack.typeBlockUpper;
            var under = GetVector3(itemTrack.BlockPositionUnder);
            var underType = itemTrack.typeBlockUnder;

            var placeSpawns = new List<Dictionary<PlaceSpawn, TypeBlock>>();

            for (var j = 0; j < 12; j++)
            {
                int row = GetRowNumber(j);
                int nextRow = GetRowNumber(j + 1);

                if (up[j] != Vector3.zero)
                {
                    PlaceSpawn placeSpawn = GetUpperPlaceSpawnFromIndex(j);
                    TypeBlock typeBlock = upType[j];
                    var elemDict = new Dictionary<PlaceSpawn, TypeBlock>() { [placeSpawn] = typeBlock };
                    placeSpawns.Add(elemDict);
                }

                if (under[j] != Vector3.zero)
                {
                    PlaceSpawn placeSpawn = GetUnderPlaceSpawnFromIndex(j);
                    TypeBlock typeBlock = underType[j];
                    var elemDict = new Dictionary<PlaceSpawn, TypeBlock>() { [placeSpawn] = typeBlock };
                    placeSpawns.Add(elemDict);
                }

                switch (j)
                {
                    case 2:
                    case 5:
                    case 8:
                    case 11:
                        {
                            if (placeSpawns.Count == 0)
                                break;
                            var currentPlaceSpawns = new List<Dictionary<PlaceSpawn, TypeBlock>>(placeSpawns);
                            _elementForSpawns.Add(new ElementForSpawn(i, row, nextIndex, nextRow, currentPlaceSpawns));
                            placeSpawns.Clear();
                            break;
                        }
                }
            }
        }
    }

    private PlaceSpawn GetUnderPlaceSpawnFromIndex(int indexElem)
    {
        switch (indexElem)
        {
            case 0:
            case 3:
            case 6:
            case 9:
                {
                    return PlaceSpawn.Right;
                }
            case 1:
            case 4:
            case 7:
            case 10:
                {
                    return PlaceSpawn.Center;
                }
            case 2:
            case 5:
            case 8:
            case 11:
                {
                    return PlaceSpawn.Left;
                }
            default: return PlaceSpawn.None;
        }
    }

    private PlaceSpawn GetUpperPlaceSpawnFromIndex(int indexElem)
    {
        switch (indexElem)
        {
            case 0:
            case 3:
            case 6:
            case 9:
                {
                    return PlaceSpawn.TopRight;
                }
            case 1:
            case 4:
            case 7:
            case 10:
                {
                    return PlaceSpawn.TopCenter;
                }
            case 2:
            case 5:
            case 8:
            case 11:
                {
                    return PlaceSpawn.TopLeft;
                }
            default: return PlaceSpawn.None;
        }
    }

    private int GetRowNumber(int indexElem)
    {
        int result = indexElem switch
        {
            0 => 0,
            1 => 0,
            2 => 0,
            3 => 1,
            4 => 1,
            5 => 1,
            6 => 2,
            7 => 2,
            8 => 2,
            9 => 3,
            10 => 3,
            11 => 3,
            _ => -1
        };
        return result;
    }

    private Vector3[] GetVector3(CustomVector3[] vectorForTrans)
    {
        Vector3[] resultArray = new Vector3[12];

        for (var i = 0; i < 12; i++)
        {
            resultArray[i] = new Vector3(vectorForTrans[i].x, vectorForTrans[i].y, vectorForTrans[i].z);
        }

        return resultArray;
    }

}
