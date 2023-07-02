using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


enum TypeBlock
{
    Red,
    Blue,
    Bomb,
    Empty,
    Zero
}

[Serializable]
static class SaveData
{
    public static Dictionary<int, ItemTrack> dataLvL = new Dictionary<int, ItemTrack>();
}

[Serializable]
class ItemTrack
{
    public CustomVector3[] BlockPositionUpper = new CustomVector3[12];
    public CustomVector3[] BlockPositionUnder = new CustomVector3[12];
    public TypeBlock[] typeBlockUpper = new TypeBlock[12];
    public TypeBlock[] typeBlockUnder = new TypeBlock[12];

    public ItemTrack(Vector3[] BlockPositionUpper, Vector3[] BlockPositionUnder, TypeBlock[] typeBlockUpper, TypeBlock[] typeBlockUnder)
    {
        this.BlockPositionUpper = GetCustomVector3(BlockPositionUpper);
        this.BlockPositionUnder = GetCustomVector3(BlockPositionUnder);
        this.typeBlockUpper = typeBlockUpper;
        this.typeBlockUnder = typeBlockUnder;
    }

    private CustomVector3[] GetCustomVector3(Vector3[] vectorForTrans)
    {
        CustomVector3[] tempArray= new CustomVector3[12];

        for (var i = 0; i < 12; i++)
        {
            tempArray[i] = new CustomVector3(vectorForTrans[i]);
        }

        return tempArray;
    }
}

[Serializable]
class CustomVector3
{
    public float x;
    public float y;
    public float z;

    public CustomVector3(Vector3 vector3)
    {
        x = vector3.x;
        y = vector3.y;
        z = vector3.z;
    }
}

public class EditorUI : MonoBehaviour
{
    public Button BtnBack;
    public Button BtnUp;
    public Button BtnDown;
    public Button BtnLvLUp;
    public Button BtnLvLDown;
    public Button BtnLeftArrow;
    public Button BtnRightArrow;
    public Button BtnRedCube;
    public Button BtnBlueCube;
    public Button BtnBomb;
    public Button BtnDeleteBlock;
    public Button BtnControlMusic;
    public Button BtnSaveLvL;
    public Button BtnDeleteLvL;

    public Slider SliderPlayingTrack;
    public TextMeshProUGUI NowLengthTrack;

    public Rigidbody TrackRoad;

    public GameObject RedCubePrefab;
    public GameObject BlueCubePrefab;
    public GameObject BombPrefab;
    public GameObject PlayMusicControl;
    public GameObject PauseMusicControl;

    private GameObject _prevItemTrack = null;
    private GameObject _currentActiveCube;
    private GameObject _currentItemTrack = null;
    private GameObject _soundManager;

    private AudioSource _audioSource;

    private Transform _currentActiveLvL = null;

    private Vector3 _ray_start_position = Vector3.zero;
    private Vector3[] _setBlockPositionUpper = new Vector3[12];
    private Vector3[] _setBlockPositionUnder = new Vector3[12];
    private Vector3[] _setBlockPositionCurrent = new Vector3[12];

    private TypeBlock[] _typeBlockCurrent = new TypeBlock[12];
    private TypeBlock[] _typeBlockUnder = new TypeBlock[12];
    private TypeBlock[] _typeBlockUpper = new TypeBlock[12];


    private Dictionary<int, Vector3> _positionBlock;


    private int _currentIndexPositionCube = 0;
    private readonly int _layerMaskOnlyElementTrack = 1 << 6;
    private readonly int _layerMaskOnlyCube = 1 << 7;

    private int CurrentIndexPositionCube
    {
        get
        {
            return _currentIndexPositionCube;
        }
        set
        {
            if (value >= 0 && value < 12)
                _currentIndexPositionCube = value;
        }
    }

    private void Awake()
    {
        _soundManager = GameObject.FindGameObjectsWithTag("SoundManager")[0];
        _audioSource = _soundManager.GetComponent<AudioSource>();
        _audioSource.Stop();
        _audioSource.Play();
        _audioSource.Pause();
    }

    void Start()
    {
        AddListenerForButton();

        _ray_start_position = new Vector3(Screen.width / 2, Screen.height / 2.9f, 0);

        CreateGridPositionBlock();
    }

    private void FixedUpdate()
    {
        releaseRay();

        if (_audioSource.isPlaying)
        {
            var newPosition = _audioSource.time * 10.1f;
            TrackRoad.position = new Vector3(newPosition, 0.0f, 0.0f);

            PauseImageEnable();
        } 
        else
        {
            PlayImageEnable();
        }
    }


    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            DetectObjectWithRaycast();
    }

    private void CreateGridPositionBlock()
    {
        _positionBlock = new Dictionary<int, Vector3>()
        {
            { 0, new Vector3(3.5f, 0.0f, 3.5f) },
            { 1, new Vector3(3.5f, 0.0f, 0.0f) },
            { 2, new Vector3(3.5f, 0.0f, -3.5f) },
            { 3, new Vector3(1.0f, 0.0f, 3.5f) },
            { 4, new Vector3(1.0f, 0.0f, 0.0f) },
            { 5, new Vector3(1.0f, 0.0f, -3.5f) },
            { 6, new Vector3(-1.5f, 0.0f, 3.5f) },
            { 7, new Vector3(-1.5f, 0.0f, 0.0f) },
            { 8, new Vector3(-1.5f, 0.0f, -3.5f) },
            { 9, new Vector3(-4.0f, 0.0f, 3.5f) },
            { 10, new Vector3(-4.0f, 0.0f, 0.0f) },
            { 11, new Vector3(-4.0f, 0.0f, -3.5f) }
        };

    }

    void AddListenerForButton()
    {
        BtnBack.onClick.AddListener(Back);
        BtnUp.onClick.AddListener(UpTrack);
        BtnDown.onClick.AddListener(DownTrack);
        BtnLvLUp.onClick.AddListener(UpLvL);
        BtnLvLDown.onClick.AddListener(DownLvL);
        BtnLeftArrow.onClick.AddListener(LeftArrow);
        BtnRightArrow.onClick.AddListener(RightArrow);
        BtnRedCube.onClick.AddListener(RedCube);
        BtnBlueCube.onClick.AddListener(BlueCube);
        BtnBomb.onClick.AddListener(Bomb);
        BtnDeleteBlock.onClick.AddListener(DeleteBlock);
        BtnControlMusic.onClick.AddListener(ControlMusic);
        BtnSaveLvL.onClick.AddListener(SaveLvL);
        BtnDeleteLvL.onClick.AddListener(DeleteLvL);
    }

    private void DeleteLvL()
    {
        var filePath = Path.Combine(Application.persistentDataPath, "dataLVL", $"{_audioSource.clip.name}.dat");
        File.Delete(filePath);
        Back();
    }

    private void SaveLvL()
    {
        WriteDataAboutElementTrack();
        CheckExistsSaveFolderOrCreate();
        SaveGame();
    }

    private void CheckExistsSaveFolderOrCreate()
    {
        var mySubFolder = Path.Combine(Application.persistentDataPath, "dataLVL");
        if (!Directory.Exists(mySubFolder))
            Directory.CreateDirectory(mySubFolder);
    }

    private void SaveGame()
    {
        try
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fileStream = File.Create(Application.persistentDataPath + "/dataLVL/" + $"{_audioSource.clip.name}.dat");
            bf.Serialize(fileStream, SaveData.dataLvL);
            fileStream.Close();
            Debug.Log("Game data saved");
        }
        catch (Exception ex)
        {
            Debug.Log($"error: {ex.Message}");
        }
    }


    private void ControlMusic()
    {
        if (_audioSource.isPlaying)
            PauseMusicActive(); 
        else
            PlayMusicActive();
    }

    private void PauseMusicActive()
    {
        _audioSource.Pause();
        PlayImageEnable();
    }
    private void PlayImageEnable()
    {
        PlayMusicControl.SetActive(true);
        PauseMusicControl.SetActive(false);
    }

    private void PlayMusicActive()
    {
        _audioSource.Play();
        PauseImageEnable();
    }

    private void PauseImageEnable()
    {
        PauseMusicControl.SetActive(true);
        PlayMusicControl.SetActive(false);
    }

    private void DownLvL()
    {
        ChangeLvL("Under");
    }

    private void UpLvL()
    {
        ChangeLvL("Upper");
    }

    private void ChangeLvL(string lvl)
    {
        if (_currentActiveLvL.name.Equals(lvl))
            return;
        _currentActiveLvL.localPosition = new Vector3(0.0f, -3.0f, 0.0f);
        _currentActiveLvL = _currentItemTrack.transform.Find(lvl);
        _currentActiveLvL.localPosition = Vector3.zero;
        _currentActiveCube = null;

        if (lvl.Equals("Under"))
        {
            _setBlockPositionUpper = _setBlockPositionCurrent;
            _typeBlockUpper = _typeBlockCurrent;
            _setBlockPositionCurrent = _setBlockPositionUnder;
            _typeBlockCurrent = _typeBlockUnder;
        }

        if (lvl.Equals("Upper"))
        {
            _setBlockPositionUnder = _setBlockPositionCurrent;
            _typeBlockUnder = _typeBlockCurrent;
            _setBlockPositionCurrent = _setBlockPositionUpper;
            _typeBlockCurrent = _typeBlockUpper;
        }
    }

    private void DeleteBlock()
    {
        if (_currentActiveCube == null) return;


        CurrentIndexPositionCube = _positionBlock.First(x => x.Value == _currentActiveCube.transform.localPosition).Key;

        Destroy(_currentActiveCube); 
        _currentActiveCube = null;

        _setBlockPositionCurrent[CurrentIndexPositionCube] = Vector3.zero;
        _typeBlockCurrent[CurrentIndexPositionCube] = TypeBlock.Zero;
    }

    private void Bomb()
    {
        SetBlockOnFreePlace(BombPrefab);
    }

    private void BlueCube()
    {
        SetBlockOnFreePlace(BlueCubePrefab);
    }

    private void RedCube()
    {
        SetBlockOnFreePlace(RedCubePrefab);
    }

    private void SetBlockOnFreePlace(GameObject prefabBlock)
    {
        int indexCurrent = GetIndexElement(_currentItemTrack);
        if (indexCurrent < 3) return;
        var indexFreePlace = 0;

        foreach (var pos in _setBlockPositionCurrent)
        {
            if (pos != Vector3.zero)
                indexFreePlace++;
            else
                break;
        }

        if (indexFreePlace == 12)
            return;

        CurrentIndexPositionCube = indexFreePlace;

        _currentActiveCube = Instantiate(prefabBlock, _positionBlock[CurrentIndexPositionCube], Quaternion.identity, _currentActiveLvL);
        _currentActiveCube.transform.localPosition = _positionBlock[CurrentIndexPositionCube];

        _setBlockPositionCurrent[CurrentIndexPositionCube] = _positionBlock[CurrentIndexPositionCube];
        _typeBlockCurrent[CurrentIndexPositionCube] = prefabBlock.name switch
        {
            "BlueCube" => TypeBlock.Blue,
            "RedCube" => TypeBlock.Red,
            "Bomb" => TypeBlock.Bomb,
            "Empty" => TypeBlock.Empty,
            "" => TypeBlock.Zero    
        };
    }

    private void RightArrow()
    {
        if (_currentActiveCube == null) return;

        for (var i = CurrentIndexPositionCube - 1; i >= 0; i--)
        {
            if (ChangeBlockPosition(i))
                break;
        }
    }

    private void LeftArrow()
    {
        if (_currentActiveCube == null) return;

        for (var i = CurrentIndexPositionCube + 1; i < 12; i++)
        {
            if (ChangeBlockPosition(i))
                break;
        }
    }

    private bool ChangeBlockPosition(int indexPosition)
    {
        if (_setBlockPositionCurrent[indexPosition] == Vector3.zero)
        {
            _setBlockPositionCurrent[CurrentIndexPositionCube] = Vector3.zero; // освобождаем место
            var typeBlock = _typeBlockCurrent[CurrentIndexPositionCube];
            _typeBlockCurrent[CurrentIndexPositionCube] = TypeBlock.Zero;
            CurrentIndexPositionCube = indexPosition; // записываем индекс новой позиции как текущей
            _currentActiveCube.transform.localPosition = _positionBlock[CurrentIndexPositionCube]; // перемещаем куб на новую позицию (по индексу)
            _setBlockPositionCurrent[CurrentIndexPositionCube] = _positionBlock[CurrentIndexPositionCube]; // занимаем место под куб
            _typeBlockCurrent[CurrentIndexPositionCube] = typeBlock;
            return true;
        }
        return false;
    }

    private void Back()
    {
        SceneManager.LoadScene("TrackEditor");
        _audioSource.Play();
    }

    private void UpTrack()
    {
        PauseMusicActive();
        TrackRoad.transform.Translate(new Vector3(10.1f, 0.0f, 0.0f));
    }

    private void DownTrack()
    {
        PauseMusicActive();
        TrackRoad.transform.Translate(new Vector3(-10.1f, 0.0f, 0.0f));
    }

    public void SmoothDrag(Vector3 targetPosition, float elapsed)
    {
        var oldPosition = TrackRoad.transform.position;

        var remainingPosition = targetPosition - oldPosition;
        var newRemainingPosition = Vector3.Lerp(remainingPosition, Vector3.zero, elapsed);

        TrackRoad.transform.position = oldPosition + remainingPosition - newRemainingPosition;
    }

    private void releaseRay()
    {
        Ray ray = Camera.main.ScreenPointToRay(_ray_start_position);

        RaycastHit hit;
        Physics.Raycast(ray, out hit, Mathf.Infinity, _layerMaskOnlyElementTrack);

        if (hit.collider == null)
            return;

        _currentItemTrack = hit.collider.gameObject;


        if (_currentItemTrack == _prevItemTrack) return;


        if (_prevItemTrack != null)
        {
            int prevIndexElement = GetIndexElement(_prevItemTrack);
            if (prevIndexElement > 2)
                _prevItemTrack.GetComponent<MeshRenderer>().materials[0].color = Color.black;

            WriteDataAboutElementTrack();
        }

        _setBlockPositionCurrent = new Vector3[12];
        _typeBlockCurrent = new TypeBlock[12];

        int indexCurrent = GetIndexElement(_currentItemTrack);

        if (indexCurrent > 2)
            _currentItemTrack.GetComponent<MeshRenderer>().materials[0].color = Color.red;


        if (!_audioSource.isPlaying)
            ChangeSliderValue(indexCurrent);

        SetBlockPositionData(indexCurrent);

        IdentifyAndLoadActiveLvL();

        _prevItemTrack = _currentItemTrack;

    }
    
    private void WriteDataAboutElementTrack()
    {
        if (_currentActiveLvL.name.Equals("Under"))
        {
            _setBlockPositionUnder = _setBlockPositionCurrent;
            _typeBlockUnder = _typeBlockCurrent;
        }

        if (_currentActiveLvL.name.Equals("Upper"))
        {
            _setBlockPositionUpper = _setBlockPositionCurrent;
            _typeBlockUpper = _typeBlockCurrent;
        }

        var textNumItemInPrev = _prevItemTrack.GetComponentInChildren<TextMeshPro>();
        var indexPrev = (Convert.ToInt32(textNumItemInPrev.text));
        var item = new ItemTrack(_setBlockPositionUpper, _setBlockPositionUnder, _typeBlockUpper, _typeBlockUnder);
        SaveData.dataLvL[indexPrev] = item;
    }

    private int GetIndexElement(GameObject gameObject)
    {
        var textNumItemInCurrent = gameObject.GetComponentInChildren<TextMeshPro>();
        var result = Convert.ToInt32(textNumItemInCurrent.text);
        return result;
    }

    private void ChangeSliderValue(int indexCurrent)
    {
        _audioSource.time = indexCurrent;
        SliderPlayingTrack.value = _audioSource.time / _audioSource.clip.length;
        NowLengthTrack.text = TimeSpan.FromSeconds(_audioSource.time).ToString(@"mm\:ss");
    }

    private void SetBlockPositionData(int indexCurrent)
    {
        if (SaveData.dataLvL.ContainsKey(indexCurrent))
        {
            _setBlockPositionUpper = GetVector3(SaveData.dataLvL[indexCurrent].BlockPositionUpper);
            _typeBlockUpper = SaveData.dataLvL[indexCurrent].typeBlockUpper;
            _setBlockPositionUnder = GetVector3(SaveData.dataLvL[indexCurrent].BlockPositionUnder);
            _typeBlockUnder = SaveData.dataLvL[indexCurrent].typeBlockUnder;
        }
        else
        {
            _setBlockPositionUpper = new Vector3[12];
            _typeBlockUpper = new TypeBlock[12];
            _setBlockPositionUnder = new Vector3[12];
            _typeBlockUnder = new TypeBlock[12];
        }
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

    private void IdentifyAndLoadActiveLvL()
    {
        var underLVL = _currentItemTrack.transform.Find("Under");
        var upperLVL = _currentItemTrack.transform.Find("Upper");

        if (upperLVL.localPosition == Vector3.zero)
        {
            _currentActiveLvL = upperLVL;
            _setBlockPositionCurrent = _setBlockPositionUpper;
            _typeBlockCurrent = _typeBlockUpper;
        }

        if (underLVL.localPosition == Vector3.zero)
        {
            _currentActiveLvL = underLVL;
            _setBlockPositionCurrent = _setBlockPositionUnder;
            _typeBlockCurrent = _typeBlockUnder;
         }
    }

    private void DetectObjectWithRaycast()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, _layerMaskOnlyCube))
        {

            if (hit.collider.tag.Equals("Cube"))
            {
                _currentActiveCube = hit.collider.gameObject;
                CurrentIndexPositionCube = _positionBlock.First(x => x.Value == _currentActiveCube.transform.localPosition).Key;
            }
        }
    }
}
