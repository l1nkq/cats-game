using System.Collections;
using UnityEngine;

public class ExplosionController : MonoBehaviour
{
    private float _notMovingTime;
    private float _minWaitingAfterStopTime = 0.5f; //время минимум после остановки всех
    private GameController _gC;
    private float _elapsedTime; //сколько прошло времени после взрыва
    private float _minExplosionTime = 1.5f; //не менее секунды время после взрыва должно пройти.
                                          //потому что если сразу все стоят на месте,
                                          //то уже уже _minWaitingAfterStopedTime сработает
    private float _timeToCheckIssetNewBlock = 0.1f;
    private float _maxWaitingTime = 5f; //максимальное время, башня может шататься и не сработает IsAllStop
    
    void Start()
    {
        _notMovingTime = 0f;
        _gC = GameObject.Find("GameController").GetComponent<GameController>();
        GameController.IsWaitingExplosionEnd = true;
    }

    private void Update()
    {
        CheckIsWaitingOver();
    }

    //используется для проверки окончания взрыва.
    //потому что во время взрыва, нельзя сразу просчитывать обычные текущие проверки на остановки 
    void CheckIsWaitingOver()
    {
        _elapsedTime += Time.deltaTime;
        if (_gC.IsAllBlocksStoped())
        {
            _notMovingTime += Time.deltaTime;
        }
        else
        {
            _notMovingTime = 0f;
        }

        if (_notMovingTime > _minWaitingAfterStopTime && _elapsedTime > _minExplosionTime || _elapsedTime > _maxWaitingTime)
        {
            GameController.IsWaitingExplosionEnd = false;
            //проверка, что нужно сделать Spawn после взрыва, если не запустился в другом месте
            StartCoroutine(CheckExistNewBlock());
            //нужно уничтожить текущий объект explosion, потому что он будет мешать другим.
            //чтобы избежать этого - ToDO возможно можно сделать проверку единым чекером, а не каждый раз создавать
            Destroy(gameObject, _timeToCheckIssetNewBlock + 0.1f); //уничтожаем немного позже, чем пройдет проверка
        }
    }

    IEnumerator CheckExistNewBlock()
    {
        yield return new WaitForSeconds(_timeToCheckIssetNewBlock);
        if (!GameController.IsPlayerControlFallingBlock && !GameController.IsWaitingSpawn)
        {
            //если по взрыва не посчитало что нужно появится новому блоку, то запускаем это здесь
            //TODO стоит синхронизировать логику Spawn, чтобы не нужно было здесь проверять
            EventManager.WaitingSpawn(0f);
            //убираем этот explosion
        }
    }
}
