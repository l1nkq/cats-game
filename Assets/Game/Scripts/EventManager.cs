using UnityEngine;

public class EventManager : MonoBehaviour
{
    //событие разворота падающего блока
    public delegate void TurnAction(bool a);
    public static event TurnAction TurnLActionClick;
    
    //событие необходимости быстрого падения
    public delegate void FastFallAction();
    public static event FastFallAction FastFallActionClick;
    
    //событие разворота камеры
    public delegate void CameraRotateAction(bool a);
    public static event CameraRotateAction CameraRotateActionRClick;

    //событие требующее изменение очков
    public delegate void ChangeScoreAction(int sum);
    public static event ChangeScoreAction ChangeScoreActionEvent;

    //событие изменение высоты башни
    public delegate void ChangeMaxRowAction(int row);
    public static event ChangeMaxRowAction ChangeMaxRowEvent;

    //событие изменения оставшихся блоков
    public delegate void ChangeLeftNumberAction();
    public static event ChangeLeftNumberAction ChangeLeftNumberEvent;

    //событие конца уровеня
    public delegate void LevelEndAction();
    public static event LevelEndAction LevelEndEvent;
    
    //событие ожидания игрой spawn нового блока
    public delegate void WaitingSpawnAction(float sec);
    public static event WaitingSpawnAction WaitingSpawnEvent;


    public void TurnLClick()
    {
        if (TurnLActionClick != null)
            TurnLActionClick(true);
    }
    
    public void FastFallClick()
    {
        if (FastFallActionClick != null)
            FastFallActionClick();
    }

    public void CameraRotateRClick()
    {
        if (CameraRotateActionRClick != null)
            CameraRotateActionRClick(false);
    }

    public static void ChangeScore(int sum)
    {
        if (ChangeScoreActionEvent != null)
            ChangeScoreActionEvent(sum);
    }

    public static void ChangeMaxRow(int sum)
    {
        if (ChangeMaxRowEvent != null)
            ChangeMaxRowEvent(sum);
    }

    public static void ChangeLeftNumber()
    {
        if (ChangeLeftNumberEvent != null)
            ChangeLeftNumberEvent();
    }

    public static void LevelEnd()
    {
        if (LevelEndEvent != null)
            LevelEndEvent();
    }

    public static void WaitingSpawn(float sec)
    {
        if (WaitingSpawnEvent != null)
            WaitingSpawnEvent(sec);
    }
}
