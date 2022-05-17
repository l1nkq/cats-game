using UnityEngine;

public class CameraController : MonoBehaviour
{
    Vector3 startPosition;
    int startCameraRowMoving = 1;

    Vector3 targetRowPosition;
    float speed = 8f;
    
    float _targetRotation;

    static float _currentRotationY; //используется, чтобы корректировать относительно этого угла управление котами

    [SerializeField] private LevelGeneratorController levelGeneratorController;
    
    private void Start()
    {
        startPosition = transform.position;
        targetRowPosition = startPosition;

        //подписываемся на событие измения высоты башни
        EventManager.ChangeMaxRowEvent += SetTargetRowPosition;
        //подписываемся на событие поворота камеры
        EventManager.CameraRotateActionRClick += SetTargetRotation;

        _currentRotationY = -45f; //при старте всегда устанавливаем в 45f, чтобы если при перезагрузке сбросилось значение
        _targetRotation = _currentRotationY;
    }
    private void Update()
    {
        //если позиция, где должна быть камера отличается от текущей
        if (transform.position != targetRowPosition)
        {
            //то перемещаем ее туда
            transform.position = Vector3.Lerp(transform.position, targetRowPosition, Time.deltaTime * speed);
        }
        
        CheckIsNeedRotate();
    }

    void SetTargetRowPosition(int row)
    {
        row -= GameController.GainedRow + 1; //убираем расчеты пред. значений
        float rowHeigth = 1f;
        Vector3 genPosition = levelGeneratorController.GetGenPosition();
        targetRowPosition = genPosition + startPosition;
        
        if (row > startCameraRowMoving)
        {
            //(row - startCameraRowMoving) - означает что будет добавляться +1 ряд к начальному.
            //т.е. когда больше чем startCameraRowMoving, то камера поднимется только на один ряд от самого низа
            targetRowPosition += Vector3.up * (row - startCameraRowMoving) * rowHeigth;
        }
    }

    private void OnDestroy()
    {
        //отписывается от событий измения высоты башни
        EventManager.ChangeMaxRowEvent -= SetTargetRowPosition;
    }
    
    void SetTargetRotation(bool clock)
    {
        if (clock)
        {
            _targetRotation += 90f;
        }
        else
        {
            _targetRotation -= 90f;
        }
    }

    void CheckIsNeedRotate()
    {
        //плавный разворот объект, если целевой угол еще не достигнет
        if (transform.rotation.eulerAngles.y != _targetRotation)
        {
            Quaternion targetRot = Quaternion.Euler(0, _targetRotation, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 20f * Time.deltaTime);
            _currentRotationY = transform.eulerAngles.y;
        }
    }

    public static float GetCurrentRotationY()
    {
        return _currentRotationY;
    }
}
