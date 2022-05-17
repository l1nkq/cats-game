using UnityEngine;
using UnityEngine.SceneManagement;

//этот класс используется в редакторе Unity, чтобы при запуске проекта всегда стартовала начальная сцена, а не открытая в текущий момент
public static class SceneAutoLoader
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoadRuntimeMethod()
    {
        //если это LevelScene - то открываем тогда 0 сцену проекта, потому что LevelScene не может загрузиться без логики в 0 сцене
        if (SceneManager.GetActiveScene().name == "LevelScene")
        {
            //загружаем сцену, которая в настройках проектах самая первая
            SceneManager.LoadScene(0);
        }
        
    }
}
