using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
public class MenuController : MonoBehaviour
{
    [SerializeField] 
    private GameObject levelsScreen;
    [SerializeField]
    private GameObject levelButtons;
    
    [SerializeField] 
    private GameObject levelStartButton;

    [SerializeField] private GameObject[] allStartUI;

    [SerializeField] private float startingAnim;

    [SerializeField] private Text bestScoreText;

    [SerializeField] private Transform _platform;

    private int bestScores;
    
    private void Start()
    {
        Time.timeScale = 1f;

        bestScores = PlayerPrefs.GetInt("Best");
        
        if (bestScores > 0)
        {
            bestScoreText.text = "BEST SCORE " + bestScores;
        }
        else
        {
            bestScoreText.text = null;
        }
        
        LevelData[] levelDatas = GlobalGameState.instance.GetLevelDatas();
        int levelNumber = 0;
        int levelCount = levelDatas.Length;
        int inRowCount = 1;
        

        float padding = 200;
        float startPosition = - padding * (inRowCount - 1) / 2;
        
        for (int i=0; i*inRowCount < levelCount; i++)
        {
            for (int j = 0; j < inRowCount; j++)
            {
                levelNumber++;
                if (levelNumber <= levelCount)
                {
                    Vector3 buttonPosition = new Vector3(startPosition + padding*j, -padding*i, 0);
                    GameObject button = Instantiate(levelStartButton, buttonPosition, Quaternion.identity);
                    button.transform.SetParent(levelButtons.transform, false);
                    string levelText = "level " + levelNumber.ToString();
                    button.transform.Find("LevelDescriptionBlock").Find("Text").GetComponent<Text>().text = levelText;
                    var number = levelNumber - 1;
                    string path = string.Format("LevelImages/level_{0}", number);
                    Sprite levelImageSprite  = Resources.Load<Sprite>(path);
                    button.transform.Find("LevelImage").GetComponent<Image>().sprite = levelImageSprite;
                    
                    button.GetComponent<Button>().onClick.AddListener(delegate { LoadLevel(number); });
                }
            }
        };
    }

    public void LoadLevel(int number)
    {
        GlobalGameState.instance.SetCurrentLevel(number);
        SceneManager.LoadScene((int)SceneCodes.Level);
    }

    public void StartArcadeModeGame()
    {
        LoadLevel(-1);
    }

    public void ShowLevelsScreen()
    {
        levelsScreen.SetActive(true);
    }
    
    public void HideLevelsScreen()
    {
        levelsScreen.SetActive(false);
    }

    public void Play()
    {
        for (int i = 0; i < allStartUI.Length; i++)
        {
            Color color = new Color(1, 1, 1, 0);
            
            if (allStartUI[i].GetComponent<Image>())
            {
                allStartUI[i].GetComponent<Image>().DOColor(color, startingAnim);
            }
            else
            {
                allStartUI[i].GetComponent<Text>().DOColor(color, startingAnim);
            }
        }

        Vector3 pos = new Vector3(_platform.transform.position.x, 250, 0);
        
        _platform.DOMove(pos, startingAnim).OnComplete(() => LoadLevel(-1));
    }

    public void ResetTeach()
    {
        int _teaching = 0;
        PlayerPrefs.SetInt("Teaching", _teaching);
    }
}