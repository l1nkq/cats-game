using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField]
    Text scoreText;

    [SerializeField]
    Text leftNumberText;

    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject mainPausePanel;
    [SerializeField] GameObject levelEndPanel;

    [SerializeField]
    Text endScoreText;

    [SerializeField] private GameObject spawnedObjects;
    [SerializeField] private GameObject trashObjects; //отработанные объекты

    int score;

    int _lastMaxRow;
    
    public static LevelData levelData;
    public static GameObject[] availableLevelCats;
    public static int CurrentSpiralPartNumber;
    public static bool IsPlayerControlFallingBlock;
    public static bool IsWaitingSpawn;
    public static float GenY;
    public static int GainedRow; //это несгораемый набранный уровень, фиксируется при появлении новой платформы
    public static bool IsWaitingExplosionEnd;

    private float _firstPartMinBlocks = 15f; //на первой части нужно собрать столько блоков
    private float _plusPartBlocks = 2f; //по сколько блоков на каждой части увеличивать

    [SerializeField]
    GameObject[] catsPrefabs;

    int targetScore;
    
    string _targetScoreString;

    private int[] _levelGoals; //цели на уровень
    
    [SerializeField] 
    private Text levelGoalsText;
    
    [SerializeField] 
    private Text goaledStarsCountText;
    
    [SerializeField] 
    private GameObject levelGoalsUIObjects;

    [SerializeField] private LevelGeneratorController levelGeneratorController;
    
    [Space]

    private int bestScores;

    private void Awake()
    {
        IsPlayerControlFallingBlock = false;
        IsWaitingSpawn = false;
        IsWaitingExplosionEnd = false;
        if (GlobalGameState.instance)
        {
            //levelNumber можно использовать, если на экране нужно вывести номер уровня текущий
            int levelNumber = GlobalGameState.instance.GetCurrentLevel();

            if (!IsArcadeMode())
            {
                //загрузка уровня из файла данных
                
                levelData = GlobalGameState.instance.GetCurrentLevelData();
                int[] levelCatsIndexes = levelData.available_cat_indexes;

                //создаем массив блоков, доступных на текущем уровней
                availableLevelCats = new GameObject[levelCatsIndexes.Length];
                for (int i = 0; i < levelCatsIndexes.Length; i++)
                {
                    //блоки, доступные для текущего уровня
                    availableLevelCats[i] = catsPrefabs[levelCatsIndexes[i]];
                }
            }
            else
            {
                //аркадный режим
                CurrentSpiralPartNumber = 0;
                
                levelData = new LevelData();
                levelData.platform = new []
                {
                    new[] {1,1,1,1,1},
                    new[] {1,1,1,1,1},
                    new[] {1,1,1,1,1},
                    new[] {1,1,1,1,1},
                    new[] {1,1,1,1,1}
                };
                levelData.cats_count = -1; //бесконечное кол-во
                //доступные все блоки
                availableLevelCats = catsPrefabs;
                //отключаем итоговые цели для режима аркады
                levelGoalsUIObjects.SetActive(false);
            }
        }

        GenY = levelGeneratorController.GetGenPosition().y;
    }
    
    private void Start()
    {
        GainedRow = 0;
        Time.timeScale = 1f;
        _lastMaxRow = 0;
        //подписываемся на события измения очков
        EventManager.ChangeScoreActionEvent += ChangeScore;

        //подписываемся на события измения кол-ва оставшихся блоков
        EventManager.ChangeLeftNumberEvent += ChangeLeftNumber;

        //подписываемся на события окончания уровня
        EventManager.LevelEndEvent += LevelEnd;

        //рассчитываем требуемое кол-во очков для уровня
        int totalBlocksOfAvailableCats = 0;
        foreach (GameObject availableCat in availableLevelCats)
        {
           
            totalBlocksOfAvailableCats += availableCat.GetComponent<BlockPrefab>().GetBlockCount();
            
        }
        float averageBlockCount = (float)totalBlocksOfAvailableCats / availableLevelCats.Length;
        float bonusOfRow = 0.25f; //TODO заменить на глобальный параметр игры
        float blockCost = 10f; //TODO заменить на глобальный параметр игры
        int multiplierRowSum = 0;
        for (int i = 0; i < levelData.cats_count; i++)
        {
            multiplierRowSum += i;
        }

        targetScore = Mathf.CeilToInt(averageBlockCount * ( levelData.cats_count + bonusOfRow * multiplierRowSum) * blockCost);
        //берем 50% от этой суммы, т.к. эта сумма при очень хороших условиях только можно набрать, если поставить все в одну башню
        int topTargetScore = targetScore /= 2;

        _levelGoals = new int[]
        {
            topTargetScore * 80 / 100,
            topTargetScore * 90 / 100,
            topTargetScore
        };
        
        //отбрасываем единицы в целях, согласно логике, поставленной в задаче по игре, чтобы были более "красивые" числа
        for (int i = 0; i < _levelGoals.Length; i++)
        {
            _levelGoals[i] = _levelGoals[i] - _levelGoals[i] % 10; //обрасываем остаток от деления на 10, это и будут единицы
        }

        if (!IsArcadeMode())
        {
            _targetScoreString = _levelGoals[2].ToString(); //выводим на экране во время игры кол-во очков для трех звезд
            scoreText.text = "0 / " + _targetScoreString;
        }
        else
        {
            _targetScoreString = "";
        }
        
        
        levelGoalsText.text = "1 star: " + _levelGoals[0] + "\n" + "2 stars: " + _levelGoals[1] + "\n" +  "3 star: " + _levelGoals[2]; //выводим на панели, которая появится в конце уровня сколько нужно было набрать для каждой звезды
    }

    private void OnDestroy()
    {
        //отписываем от событий перед сбросом сцены
        EventManager.ChangeScoreActionEvent -= ChangeScore;
        EventManager.ChangeLeftNumberEvent -= ChangeLeftNumber;
        EventManager.LevelEndEvent -= LevelEnd;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitToMenu()
    {
        SceneManager.LoadScene((int)SceneCodes.Menu);
    }

    void ChangeScore(int sum)
    {
        score += sum;

        if (score < 0)
        {
            score = 0;
        }
        string scoreString = score.ToString();
        if (_targetScoreString != "")
        {
            scoreString += " / " + _targetScoreString;
        }
        scoreText.text = scoreString;
        
        
        //ряд может изменится, только если было событие, которое вызвало ChangeScore
        //поэтому логика проверка высоты ряды теперь расположена в этом методе
        int currentMaxRow = GetTopRowNumber();
        if (_lastMaxRow != currentMaxRow)
        {
            _lastMaxRow = currentMaxRow;
            
            if (_lastMaxRow > GainedRow + (_firstPartMinBlocks - 1f) + _plusPartBlocks * CurrentSpiralPartNumber)
            {
                GoToNextLevelPart(currentMaxRow - GainedRow);
                GainedRow = _lastMaxRow;
                EventManager.WaitingSpawn(0.51f);
            }
            
            EventManager.ChangeMaxRow(_lastMaxRow);
            
        }

        if (!IsWaitingSpawn)
        {
            if (!IsPlayerControlFallingBlock)
            {
                EventManager.WaitingSpawn(0f);
            }
        }
    }

    void ChangeLeftNumber()
    {
        int[] leftData = SpawnController.instance.GetLeftData();

        if (!IsArcadeMode())
        {
            leftNumberText.text = leftData[0].ToString() + "/" + leftData[1].ToString();
        }
        else
        {
            leftNumberText.text = leftData[0].ToString();
        }
    }

    int GetTopRowNumber()
    {
        int maxRow = 0;
        foreach (Transform t in spawnedObjects.transform)
        {
            FallingBlockController fBC = t.gameObject.GetComponent<FallingBlockController>();
            if (fBC)
            {
                int blockRow = fBC.GetCurrentRow();
                if(blockRow > maxRow)
                {
                    maxRow = blockRow;
                }
            }
        }

        return maxRow;
    }

    void LevelEnd()
    {
        StartCoroutine("LevelEndTimer");
    }

    public bool IsAllBlocksStoped()
    {
        bool allStoped = true;
        foreach (Transform t in spawnedObjects.transform)
        {
            FallingBlockController fBC = t.gameObject.GetComponent<FallingBlockController>();
            if (fBC)
            {
                if (!fBC.IsStop())
                {
                    allStoped = false;
                }
            }
        }

        return allStoped;
    }

    IEnumerator LevelEndTimer()
    {
        //таймер используется для того чтобы некоторое время отдать на то чтобы башня точно не упала
        //потому что последний блок может не просто стать, а начать падать и сделать разрушение
        //но максимум ожидания ограничено leftWaitSeconds
        if (!IsArcadeMode()) //в аркадном режиме сразу останавливается игра
        {
            float leftWaitSeconds = 5;
            while (!IsAllBlocksStoped() && leftWaitSeconds > 0)
            {
                yield return new WaitForSeconds(.5f);
                leftWaitSeconds--;
            }
        }

        endScoreText.text = score.ToString();
        goaledStarsCountText.text = GetGoaledStars().ToString();
        levelEndPanel.SetActive(true);

        if (score > bestScores)
        {
            bestScores = score;
            PlayerPrefs.SetInt("Best", bestScores);
            PlayerPrefs.Save();
        }
        
        Time.timeScale = 0;
    }

    int GetGoaledStars()
    {
        int result = 0;
        foreach (var goal in _levelGoals)
        {
            //если больше цели, то получается звезда
            if (score > goal)
            {
                result++;
            }
        }
        return result;
    }

    public static bool IsArcadeMode()
    {
        return GlobalGameState.instance.GetCurrentLevel() == -1;
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        pausePanel.SetActive(true);
        mainPausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        mainPausePanel.SetActive(false);
    }

    void GoToNextLevelPart(float rowChanged)
    {
        //перед тем как рисовать новую часть спирали, блокируем все пред объекты
        int childCount = spawnedObjects.transform.childCount;
        //foreach нельзя использовать, когда есть изменение списка 
        //в данном случае перемещается в trashObject
        for (int i = 0; i < childCount; i++)
        {
            Transform t = spawnedObjects.transform.GetChild(0); //всегда 0, потому что убирается постоянно
            Rigidbody rb = t.gameObject.GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.detectCollisions = false;
            //перемещаем в отработанные объекты, чтобы исключить их из перебора
            t.parent = trashObjects.transform;
        }
        
        CurrentSpiralPartNumber++; //увеличиваем номер элемента спирали
        levelGeneratorController.DrawNextSpiralLevelPart(levelData, rowChanged);
        Vector3 genPosition = levelGeneratorController.GetGenPosition();
        SpawnController.instance.SetSpawnPointStartPosition( genPosition);
        GenY = levelGeneratorController.GetGenPosition().y;
    }
}
