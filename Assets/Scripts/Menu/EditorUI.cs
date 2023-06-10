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
    public Button BtnEmptyCube;
    public Button BtnDeleteBlock;
    public Button BtnControlMusic;
    public Button BtnSaveLvL;
    public Button BtnDeleteLvL;

    public Slider sliderPlayingTrack;
    public TextMeshProUGUI nowLengthTrack;

    public Rigidbody TrackRoad;

    public GameObject RedCubePrefab;
    public GameObject BlueCubePrefab;
    public GameObject BombPrefab;
    public GameObject PlayMusicControl;
    public GameObject PauseMusicControl;

    private GameObject prevItemTrack = null;
    private GameObject currentActiveCube;
    private GameObject currentItemTrack = null;
    private GameObject soundManager;

    private AudioSource audioSource;

    private Transform currentActiveLvL = null;

    private Vector3 ray_start_position = Vector3.zero;
    private Vector3[] setBlockPositionUpper = new Vector3[12];
    private Vector3[] setBlockPositionUnder = new Vector3[12];
    private Vector3[] setBlockPositionCurrent = new Vector3[12];

    private TypeBlock[] typeBlockCurrent = new TypeBlock[12];
    private TypeBlock[] typeBlockUnder = new TypeBlock[12];
    private TypeBlock[] typeBlockUpper = new TypeBlock[12];


    private Dictionary<int, Vector3> positionBlock;


    private int _currentIndexPositionCube = 0;
    private readonly int layerMaskOnlyElementTrack = 1 << 6;
    private readonly int layerMaskOnlyCube = 1 << 7;

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
        soundManager = GameObject.FindGameObjectsWithTag("SoundManager")[0];
        audioSource = soundManager.GetComponent<AudioSource>();
        audioSource.Stop();
        audioSource.Play();
        audioSource.Pause();
    }

    void Start()
    {
        AddListenerForButton();

        ray_start_position = new Vector3(Screen.width / 2, Screen.height / 2.9f, 0);

        CreateGridPositionBlock();
    }

    private void FixedUpdate()
    {
        releaseRay();

        if (audioSource.isPlaying)
        {
            var newPosition = audioSource.time * 10.1f;
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
        positionBlock = new Dictionary<int, Vector3>()
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
        BtnEmptyCube.onClick.AddListener(EmptyCube);
        BtnDeleteBlock.onClick.AddListener(DeleteBlock);
        BtnControlMusic.onClick.AddListener(ControlMusic);
        BtnSaveLvL.onClick.AddListener(SaveLvL);
        BtnDeleteLvL.onClick.AddListener(DeleteLvL);
    }

    private void DeleteLvL()
    {
        var filePath = Path.Combine(Application.persistentDataPath, "dataLVL", $"{audioSource.clip.name}.dat");
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
            FileStream fileStream = File.Create(Application.persistentDataPath + "/dataLVL/" + $"{audioSource.clip.name}.dat");
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
        if (audioSource.isPlaying)
            PauseMusicActive(); 
        else
            PlayMusicActive();
    }

    private void PauseMusicActive()
    {
        audioSource.Pause();
        PlayImageEnable();
    }
    private void PlayImageEnable()
    {
        PlayMusicControl.SetActive(true);
        PauseMusicControl.SetActive(false);
    }

    private void PlayMusicActive()
    {
        audioSource.Play();
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
        if (currentActiveLvL.name.Equals(lvl))
            return;
        currentActiveLvL.localPosition = new Vector3(0.0f, -3.0f, 0.0f);
        currentActiveLvL = currentItemTrack.transform.Find(lvl);
        currentActiveLvL.localPosition = Vector3.zero;
        currentActiveCube = null;

        if (lvl.Equals("Under"))
        {
            setBlockPositionUpper = setBlockPositionCurrent;
            typeBlockUpper = typeBlockCurrent;
            setBlockPositionCurrent = setBlockPositionUnder;
            typeBlockCurrent = typeBlockUnder;
        }

        if (lvl.Equals("Upper"))
        {
            setBlockPositionUnder = setBlockPositionCurrent;
            typeBlockUnder = typeBlockCurrent;
            setBlockPositionCurrent = setBlockPositionUpper;
            typeBlockCurrent = typeBlockUpper;
        }
    }

    private void DeleteBlock()
    {
        if (currentActiveCube == null) return;


        CurrentIndexPositionCube = positionBlock.First(x => x.Value == currentActiveCube.transform.localPosition).Key;

        Destroy(currentActiveCube); 
        currentActiveCube = null;

        setBlockPositionCurrent[CurrentIndexPositionCube] = Vector3.zero;
        typeBlockCurrent[CurrentIndexPositionCube] = TypeBlock.Zero;
    }

    private void EmptyCube()
    {
        
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
        var indexFreePlace = 0;

        foreach (var pos in setBlockPositionCurrent)
        {
            if (pos != Vector3.zero)
                indexFreePlace++;
            else
                break;
        }

        if (indexFreePlace == 12)
            return;

        CurrentIndexPositionCube = indexFreePlace;

        currentActiveCube = Instantiate(prefabBlock, positionBlock[CurrentIndexPositionCube], Quaternion.identity, currentActiveLvL);
        currentActiveCube.transform.localPosition = positionBlock[CurrentIndexPositionCube];

        setBlockPositionCurrent[CurrentIndexPositionCube] = positionBlock[CurrentIndexPositionCube];
        typeBlockCurrent[CurrentIndexPositionCube] = prefabBlock.name switch
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
        if (currentActiveCube == null) return;

        for (var i = CurrentIndexPositionCube - 1; i >= 0; i--)
        {
            if (ChangeBlockPosition(i))
                break;
        }
    }

    private void LeftArrow()
    {
        if (currentActiveCube == null) return;

        for (var i = CurrentIndexPositionCube + 1; i < 12; i++)
        {
            if (ChangeBlockPosition(i))
                break;
        }
    }

    private bool ChangeBlockPosition(int indexPosition)
    {
        if (setBlockPositionCurrent[indexPosition] == Vector3.zero)
        {
            setBlockPositionCurrent[CurrentIndexPositionCube] = Vector3.zero; // освобождаем место
            var typeBlock = typeBlockCurrent[CurrentIndexPositionCube];
            typeBlockCurrent[CurrentIndexPositionCube] = TypeBlock.Zero;
            CurrentIndexPositionCube = indexPosition; // записываем индекс новой позиции как текущей
            currentActiveCube.transform.localPosition = positionBlock[CurrentIndexPositionCube]; // перемещаем куб на новую позицию (по индексу)
            setBlockPositionCurrent[CurrentIndexPositionCube] = positionBlock[CurrentIndexPositionCube]; // занимаем место под куб
            typeBlockCurrent[CurrentIndexPositionCube] = typeBlock;
            return true;
        }
        return false;
    }

    private void Back()
    {
        SceneManager.LoadScene("TrackEditor");
        audioSource.Play();
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
        Ray ray = Camera.main.ScreenPointToRay(ray_start_position);

        RaycastHit hit;
        Physics.Raycast(ray, out hit, Mathf.Infinity, layerMaskOnlyElementTrack);

        if (hit.collider == null)
            return;

        currentItemTrack = hit.collider.gameObject;

        if (currentItemTrack == prevItemTrack) return;


        if (prevItemTrack != null)
        {
            prevItemTrack.GetComponent<MeshRenderer>().materials[0].color = Color.black;

            WriteDataAboutElementTrack();
        }

        setBlockPositionCurrent = new Vector3[12];
        typeBlockCurrent = new TypeBlock[12];

        currentItemTrack.GetComponent<MeshRenderer>().materials[0].color = Color.red;

        int indexCurrent = GetCurrentIndexElement();

        if (!audioSource.isPlaying)
            ChangeSliderValue(indexCurrent);

        SetBlockPositionData(indexCurrent);

        IdentifyAndLoadActiveLvL();

        prevItemTrack = currentItemTrack;
    }
    
    private void WriteDataAboutElementTrack()
    {
        if (currentActiveLvL.name.Equals("Under"))
        {
            setBlockPositionUnder = setBlockPositionCurrent;
            typeBlockUnder = typeBlockCurrent;
        }

        if (currentActiveLvL.name.Equals("Upper"))
        {
            setBlockPositionUpper = setBlockPositionCurrent;
            typeBlockUpper = typeBlockCurrent;
        }

        var textNumItemInPrev = prevItemTrack.GetComponentInChildren<TextMeshPro>();
        var indexPrev = (Convert.ToInt32(textNumItemInPrev.text));
        var item = new ItemTrack(setBlockPositionUpper, setBlockPositionUnder, typeBlockUpper, typeBlockUnder);
        SaveData.dataLvL[indexPrev] = item;
    }

    private int GetCurrentIndexElement()
    {
        var textNumItemInCurrent = currentItemTrack.GetComponentInChildren<TextMeshPro>();
        var indexCurrent = Convert.ToInt32(textNumItemInCurrent.text);
        return indexCurrent;
    }

    private void ChangeSliderValue(int indexCurrent)
    {
        audioSource.time = indexCurrent;
        sliderPlayingTrack.value = audioSource.time / audioSource.clip.length;
        nowLengthTrack.text = TimeSpan.FromSeconds(audioSource.time).ToString(@"mm\:ss");
    }

    private void SetBlockPositionData(int indexCurrent)
    {
        if (SaveData.dataLvL.ContainsKey(indexCurrent))
        {
            setBlockPositionUpper = GetVector3(SaveData.dataLvL[indexCurrent].BlockPositionUpper);
            typeBlockUpper = SaveData.dataLvL[indexCurrent].typeBlockUpper;
            setBlockPositionUnder = GetVector3(SaveData.dataLvL[indexCurrent].BlockPositionUnder);
            typeBlockUnder = SaveData.dataLvL[indexCurrent].typeBlockUnder;
        }
        else
        {
            setBlockPositionUpper = new Vector3[12];
            typeBlockUpper = new TypeBlock[12];
            setBlockPositionUnder = new Vector3[12];
            typeBlockUnder = new TypeBlock[12];
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
        var underLVL = currentItemTrack.transform.Find("Under");
        var upperLVL = currentItemTrack.transform.Find("Upper");

        if (upperLVL.localPosition == Vector3.zero)
        {
            currentActiveLvL = upperLVL;
            setBlockPositionCurrent = setBlockPositionUpper;
            typeBlockCurrent = typeBlockUpper;
        }

        if (underLVL.localPosition == Vector3.zero)
        {
            currentActiveLvL = underLVL;
            setBlockPositionCurrent = setBlockPositionUnder;
            typeBlockCurrent = typeBlockUnder;
         }
    }

    private void DetectObjectWithRaycast()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMaskOnlyCube))
        {

            if (hit.collider.tag.Equals("Cube"))
            {
                currentActiveCube = hit.collider.gameObject;
                CurrentIndexPositionCube = positionBlock.First(x => x.Value == currentActiveCube.transform.localPosition).Key;
            }
        }
    }
}
