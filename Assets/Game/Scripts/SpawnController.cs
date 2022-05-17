using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class SpawnController : MonoBehaviour
{
    int levelCatsCount; //кол-во блоков на уровне
    GameObject[] spawnCats; //блоки, которые которые могут появится в текущем Spawn
    int currentCatIndex = 0;
    private int toBombLeft; //кол-во до следующего кота бомбы

    [SerializeField]
    private Material shadowMaterial;

    //ToDo работу с цветом можно заменить на использование ColorUtility.TryParseHtmlString
    string[][] colorsSchemes = new string[][] {
        //бело-серый
        new string[]
        {
            "eaeaea",
            "d3d3d3",
            "7b7b7b",
            "ffffff",
            "383838", //черный
            "fa9292" //розовый
        },
        //черный
        new string[]
        {
            "383838",
            "d3d3d3",
            "7b7b7b",
            "ffffff",
            "3df361", //черный
            "fa9292" //розовый
        },
        //розовый
        new string[]
        {
            "ff88e1",
            "ffd0f3",
            "d83caa",
            "ffffff",
            "383838", //черный
            "fa9292" //розовый
        },
        //голубой
        new string[]
        {
            "3bccfb", //фон
            "b9ebfd", //светлый оттенок
            "0082b5", //темный оттенок
            "ffffff", //белый
            "383838", //черный
            "fa9292" //розовый
        },
        //оранжевый
        new string[]
        {
            "ff825e", //фон
            "ffa185", //светлый оттенок
            "e8633f", //темный оттенок
            "ffffff", //белый
            "383838", //черный
            "fa9292" //розовый
        },
        //коричневый
        new string[]
        {
            "d5b68a",
            "e5d1b3",
            "80532d",
            "ffffff",
            "383838", //черный
            "fa9292" //розовый
        },

    };

    private Texture2D[] _faceTextures;
    private Texture2D[] _bombTextures;
    private int _simpleTexturesCount;
    private int _uninqueTexturesCount;

    public static SpawnController instance;

    GameObject currentBlock;

    GameObject nextBlock;
    GameObject nextBlockShadowPrefab;

    //начальная позиция Spawn
    private Vector3 _spawnPointStartPosition;

    private Vector3 _spawnPointOffset; //расстояние от основания
    
    [SerializeField] private GameObject spawnPoint;
    [SerializeField] private GameObject spawnedObjects;
    
    [SerializeField] private GameObject explosionPrefab;

    private void Start()
    {

        //инициализация счетчика появляения бомбы
        toBombLeft = Random.Range(1, 4);
        
        //!!!TODO - для оптимизации перенести в класс загрузки (еще не создан), чтобы загружать только один раз
        _faceTextures = Resources.LoadAll<Texture2D>("Textures/CatSimple/Face");
        _bombTextures = Resources.LoadAll<Texture2D>("Textures/CatSpecial/Bomb");

        //получаем кол-во папок текстур
        _simpleTexturesCount = 2; //DirectoryInfo не работало на android, пока просто вручную задано будет
        //todo подумать над автоматизацией расчета этих чисел
        _uninqueTexturesCount = 2;

        instance = this;

        //установки работы с позицией spawn по вертикали
        _spawnPointStartPosition = spawnPoint.transform.position;
        _spawnPointOffset = _spawnPointStartPosition; //offset поястоянный остается, но равен первой позиции,
                                                      //потому что первое основание в 0,0,0
        EventManager.ChangeMaxRowEvent += SetRowPosition;
        //подписываемся на событие ожидания игрой нового Spawn
        EventManager.WaitingSpawnEvent += DelaySpawn;
        
       
        levelCatsCount = GameController.levelData.cats_count;
        GameObject[] availableLevelCats = GameController.availableLevelCats;
        if (levelCatsCount != -1)
        {
            //создание колоды котов для уровня, для уровней, где заканчиваются коты
            spawnCats = new GameObject[levelCatsCount];
            for (int i = 0; i < levelCatsCount; i++)
            {
                spawnCats[i] = availableLevelCats[Random.Range(0, availableLevelCats.Length)];
            }
        }
        else
        {
            //здесь тогда не колода, а просто набор для генерации
            spawnCats = availableLevelCats;
        }

        //генерация первого и второго (который в подсказке) блока
        CreateNextBlock();
        Spawn();
    }

    public void Spawn()
    {
        //после Spawn блоки распологаются внутри объекта spawnedObjects, а не в корне сцены
        nextBlock.transform.parent = spawnedObjects.transform;
        
        GameController.IsPlayerControlFallingBlock = true;

        if(nextBlock != null)
        {
            if (toBombLeft > 0)
            {
                currentBlock = nextBlock;
                nextBlock = null; //очищаем ссылку в nextBlock, она задается заново, если будет выполнено CreateNextBlock

                //перемещаем в точку Spawn
                currentBlock.transform.position = spawnPoint.transform.position;

                //FallingBlockController добавляет динамически, чтобы не задавать пока вручную, т.к. prefabs несколько (10 шт.).
                //В будущем возможно заменится prefab на один с генерацией mesh внутри, тогда в нем можно задать сразу FallingBlockController
                FallingBlockController spawnObjecsFBC = currentBlock.AddComponent<FallingBlockController>();
                spawnObjecsFBC.shadowPrefab = nextBlockShadowPrefab;
                spawnObjecsFBC.shadowMaterial = shadowMaterial;

                currentCatIndex++;
                EventManager.ChangeLeftNumber();
                
                //если еще есть блоки, то создаем следующий или если кол-во неограничено
                if (currentCatIndex < levelCatsCount || levelCatsCount == -1)
                {
                    CreateNextBlock();
                }
                
                toBombLeft--;
            }
            else
            {
                //генерация бомбы
                GameObject bombPrefab = spawnCats[0];
                GameObject bombCat = Instantiate(bombPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);; //пока бомба только 1х1х1
                
                FallingBlockController spawnObjecsFBC = bombCat.AddComponent<FallingBlockController>();
                spawnObjecsFBC.shadowPrefab = bombPrefab;
                spawnObjecsFBC.shadowMaterial = shadowMaterial;
                
                spawnObjecsFBC.SetIsBomb(true); //устанавливаем что это бомба
                
                //установка текстур для кота бомбы
                //генерация индексов материалов
                var renderer = BlockPrefab.GetBlockRenderer(bombCat);
                if (renderer)
                {
                    Dictionary<string, int> materialsIndexes = new Dictionary<string, int>();
                    int materialIndex = 0;
                    foreach (Material material in renderer.materials)
                    {
                        materialsIndexes.Add(material.name, materialIndex);
                        materialIndex++;
                    }

                    foreach (var bombTexture in _bombTextures)
                    {
                        string materialName = bombTexture.name[0].ToString().ToUpper() + bombTexture.name.Substring(1) +
                                              " (Instance)";
                        renderer.materials[materialsIndexes[materialName]].mainTexture = bombTexture;
                        renderer.materials[materialsIndexes[materialName]].SetInt("_DisableRecoloring", 1);
                    }
                    
                }
                
                toBombLeft = Random.Range(1, 4); //установка нового значения
            }
        }
        else
        {
            EventManager.LevelEnd();
        }

    }
    
    public void DelaySpawn(float sec)
    {
        StartCoroutine(DelaySpawnCoroutine(sec));
    }
    
    public System.Collections.IEnumerator DelaySpawnCoroutine(float sec)
    {
        GameController.IsWaitingSpawn = true;
        yield return new WaitForSeconds (sec);
        Spawn();
        GameController.IsWaitingSpawn = false;
    }

    void CreateNextBlock()
    {
        Vector3 nextBlockTempPostion = new Vector3(-100f, 0, 0);
        GameObject spawnCat;
        if (levelCatsCount != -1)
        {
            //коты в самом начале тусуются, а потом один за одним берутся
            spawnCat = spawnCats[currentCatIndex];
        }
        else
        {
            spawnCat = spawnCats[Random.Range(0, spawnCats.Length)];
        }
        
        bool isUnique = Random.Range(0, 10) > 7; //вероятность 20%
        int catTextureIndexRandomMaxRange = _simpleTexturesCount;
        if (isUnique)
        {
            catTextureIndexRandomMaxRange = _uninqueTexturesCount;
        }
        int catTextureIndex = Random.Range(1, catTextureIndexRandomMaxRange + 1);
        
        string textureIndexFolder = "000" + catTextureIndex; //todo переработать в формирование ведущих нулей
        string textureMainFolder = "Textures/";
        if (isUnique)
        {
            //это уникальный кот
            textureMainFolder += "CatUnique/";
        }
        else
        {
            textureMainFolder += "CatSimple/";
        }
        textureMainFolder += textureIndexFolder + "/";
        
        nextBlock = Instantiate(spawnCat, nextBlockTempPostion, spawnPoint.transform.rotation);
        BlockPrefab nextBlockBP = nextBlock.GetComponent<BlockPrefab>();
        nextBlockBP.SetTextureMainFolder(textureMainFolder);

        string[] randColorData = colorsSchemes[Random.Range(0, colorsSchemes.Length)];
        var renderer = BlockPrefab.GetBlockRenderer(nextBlock);
        if (renderer)
        {
            Material[] materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                ColorUtility.TryParseHtmlString("#" + randColorData[0], out Color parseColorA);
                ColorUtility.TryParseHtmlString("#" + randColorData[1], out Color parseColorB);
                ColorUtility.TryParseHtmlString("#" + randColorData[2], out Color parseColorC);
                ColorUtility.TryParseHtmlString("#" + randColorData[3], out Color parseColorD);
                ColorUtility.TryParseHtmlString("#" + randColorData[4], out Color parseColorE);
                ColorUtility.TryParseHtmlString("#" + randColorData[5], out Color parseColorF);
                materials[i].SetColor("_ColorA", parseColorA);
                materials[i].SetColor("_ColorB", parseColorB);
                materials[i].SetColor("_ColorC", parseColorC);
                materials[i].SetColor("_ColorD", parseColorD);
                materials[i].SetColor("_ColorE", parseColorE);
                materials[i].SetColor("_ColorF", parseColorF);
            }
            renderer.materials = materials;
            
            //renderer.material.SetColor("_Color", randColor);
            //nextBlock.transform.Find("CatTail").Find("TailMesh").gameObject.GetComponent<Renderer>().material.SetColor("_Color", randColor);
            
            nextBlockBP.SetTextureMainFolder(textureMainFolder);
            
            //замена текстур котов на текстуры из папки текстур выбранного индекса
            foreach (Material mat in renderer.materials)
            {
                //удаляем из имени окончание " (Instance)"
                string matName = mat.name;
                int suffixPos = matName.IndexOf(" (Instance)");
                //TODO можно поменять на более простую логику перебора строк для path, сделано просто чтобы работало
                matName = matName.Remove(suffixPos);
                string[] paths = matName.Split('_');
                string partFolder = paths[0];
                string fileName = "";
                if (paths.Length > 1)
                {
                    for (int i = 1; i < paths.Length; i++)
                    {
                        fileName += "_" + paths[i];
                    }
                }
                else
                {
                    fileName += "_";
                }
                string path = textureMainFolder + partFolder + "/" + fileName;
                mat.mainTexture = Resources.Load<Texture2D>(path);
                if (isUnique)
                {
                    //уникальным отключаем перекрашиваение
                    mat.SetInt("_DisableRecoloring", 1);
                }
                
            }




        }

        nextBlockShadowPrefab = spawnCat;
    }

    public int[] GetLeftData()
    {
        return new int[] { currentCatIndex, levelCatsCount };
    }
    
    void SetRowPosition(int row)
    {
        if (row < GameController.GainedRow)
        {
            row = GameController.GainedRow;
        }
        spawnPoint.transform.position = _spawnPointStartPosition + Vector3.up * (row - GameController.GainedRow);
    }

    private void OnDestroy()
    {
        //отписывается от событий измения высоты башни
        EventManager.ChangeMaxRowEvent -= SetRowPosition;
        //и от события ожидания игрой нового spawn
        EventManager.WaitingSpawnEvent -= DelaySpawn;
    }

    public GameObject GetExplosionPrefab()
    {
        return explosionPrefab;
    }

    public void SetSpawnPointStartPosition(Vector3 pos)
    {
        spawnPoint.transform.position = pos + _spawnPointOffset;
        _spawnPointStartPosition = spawnPoint.transform.position;
    }
}
