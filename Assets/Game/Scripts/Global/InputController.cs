using UnityEngine;
using UnityEngine.SceneManagement;

public class InputController : MonoBehaviour
{
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            // нажата кнопка назад на телефоне
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                switch (SceneManager.GetActiveScene().buildIndex)
                {
                    case (int)SceneCodes.Menu:
                        //на главном экране - выход из игры
                        Application.Quit();
                        break;

                    case (int)SceneCodes.Level:
                        //во время игры, выход в меню
                        SceneManager.LoadScene((int)SceneCodes.Menu);
                        break;
                }
                
            }
        }
    }
}
