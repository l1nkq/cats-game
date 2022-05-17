using UnityEngine;
using Newtonsoft.Json;

public class GlobalGameState : MonoBehaviour
{
    public static GlobalGameState instance;
    
    [SerializeField]
    TextAsset levelsJsonFile;
    
    JsonData jsonData;

    int currentLevel;
    private void Awake()
    {
        instance = this;
        
        jsonData = JsonConvert.DeserializeObject<JsonData>(levelsJsonFile.text);
        
        DontDestroyOnLoad(gameObject);
        
    }
    
    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public void SetCurrentLevel(int number)
    {
        currentLevel = number;
    }

    public LevelData GetCurrentLevelData()
    {
        return jsonData.levels[GetCurrentLevel()];
    }

    public LevelData[] GetLevelDatas()
    {
        return jsonData.levels;
    }
}
