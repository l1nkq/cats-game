using System.Collections;
using UnityEngine;

public class FallingBlockController : MonoBehaviour
{
    // Start is called before the first frame update
    bool isPlayerControlled;
    Rigidbody rb;

    float maxDistance;
    BoxCollider[] colliders;

    public GameObject shadowPrefab;
    GameObject shadowObj;
    float shadowPositionY;

    public Material shadowMaterial;

    float targetRotation;

    float blockCost = 10f; //цена 1x1
    float blockPenaltyMultiplier = 1.5f; //штраф за 1x1
    float bonusOfRow = 0.25f;

    int currentRow; //ряд в котором находится блок

    int cost; //итоговая цена блока

    private bool _isLastMoving = true; //являлось ли в движении

    private ParticleSystem _pS;
    
    private float _fallingSpeed = 4f; //скорость падения
    private bool _isBomb = false;
    
    private bool _isFastFall = false;
    
    private float _afterBlinkElapsedTime; //время прошедщее после последнего мигания
    private float _blinkWaitingTime; //время следующего моргания
    private static readonly float MinBlinkWaitTime = 5f; //минимальное время до след моргания
    private static readonly float MaxBlinkWaitTime = 10f;//максимальное время до след моргания
    private static readonly float BlinkingTime = 1f; //время, которое глаза закрыты
    
    private void Start()
    {
        if (Tutorial.IsActive())
        {
            _fallingSpeed = 0f;
        }
        
        rb = gameObject.AddComponent<Rigidbody>();

        float partMultiplier = GameController.CurrentSpiralPartNumber * 1.15f; //15% за каждую часть
        if (partMultiplier == 0)
        {
            partMultiplier = 1f;
        }
        
        if (_isBomb)
        {
            _fallingSpeed *= 2f; //бомба быстрее в N раз
        }

        _fallingSpeed *= partMultiplier;
        
        //подписываемся на события
        EventManager.TurnLActionClick += SetTargetRotation;
        EventManager.FastFallActionClick += SetFastFall;

        shadowObj = Instantiate(shadowPrefab, Vector3.zero, transform.rotation);
        
        shadowObj.transform.localScale = shadowObj.transform.localScale * 0.999f;

        Renderer shadowRenderer = BlockPrefab.GetBlockRenderer(shadowObj);
        Material[] materials = shadowRenderer.materials;
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i] = shadowMaterial;
        }

        shadowRenderer.materials = materials;
         

        //у тени не должно быть коллайдера, потому что тогда будет считать возможно падение на нее
        Destroy(shadowObj.transform.Find("colliders").gameObject);
        //удаляем у тени лицо
        //Destroy(shadowObj.transform.Find("CatFace").gameObject);
        //удаляем у тени хвост
        //Destroy(shadowObj.transform.Find("CatTail").gameObject);
        
        shadowObj.SetActive(false);

        

        maxDistance = 300.0f;
        colliders = GetComponent<BlockPrefab>().GetColliders();
        SetCollidersIsTrigger(true);
        
        _pS = GetComponent<BlockPrefab>().GetParticleSystem();

        isPlayerControlled = true;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.isKinematic = true;
        rb.angularDrag = 0.5f; //более устойчивые к налонам, не так сильно падает башня
    }

    // Update is called once per frame

    private void Update()
    {
        float speed = 1.6f;

        if (isPlayerControlled && Input.touchCount > 0)
        {

            Touch touch = Input.GetTouch(0);

            //if(touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Ended)
            //{
                Vector2 touchMovenent = Input.GetTouch(0).deltaPosition;

                if (Tutorial.IsActive())
                {
                    touchMovenent.y = 0f;
                }

                if (Mathf.Abs(touchMovenent.magnitude) > 1f) //исключение случайных небольших касаний
                {

                    Vector3 movePosition = new Vector3(touchMovenent.x, 0, touchMovenent.y);

                    //поворачиваем его относительно камеры
                    movePosition = Quaternion.Euler(0, CameraController.GetCurrentRotationY(), 0) * movePosition;

                    #if UNITY_EDITOR
                        movePosition *= 2f; //в эмуляторе юнити скорость более мендленная, чем на телефоне
                    #endif

                //rb.AddForce(movePosition * touch.deltaTime, ForceMode.VelocityChange);
                //rb.MovePosition(transform.position + movePosition * Time.deltaTime);

                transform.position = Vector3.MoveTowards(transform.position, transform.position + movePosition * Time.deltaTime * speed, 0.35f);
            }
            //}
        }

        if (isPlayerControlled)
        {
            Fall();
            CheckIsNeedRotate();
            DrawShadow();
        }
        else
        {
            //обязательно не должно быть ожидания конца взрыва
            if (IsStop() && !GameController.IsWaitingExplosionEnd)
            {
                //если это первая проверка, у детали, которая считалась в движении
                //TODO можно в будущем попробовать исключить это из Update, чтобы не нужно было использовать _isLastMoving
                if (_isLastMoving)
                {
                    //проверка изменилось ли кол-во очков блока
                    CheckCost();

                    //проверка плохого падения
                    CheckBadFall();
                }

                _isLastMoving = false;
            }else
            {
                _isLastMoving = true;
            }
            
            //моргание котов, которые уже установлены
            _afterBlinkElapsedTime += Time.deltaTime;
            if (_afterBlinkElapsedTime > _blinkWaitingTime)
            {
                BlinkEyes();
            }

            //когда потеряно управление, то уничтожаем тень
            DestroyShadow();
        }
    }

    void FixedUpdate()
    {
        if (isPlayerControlled)
        {
            //ищем наиболее ближайшее попадание, значит именно там будет коллизия, чтобы отрисовать потом в этом месте тень
            bool hitDetect;
            RaycastHit hit;
            float colPositionY = -maxDistance; 
            foreach(BoxCollider collider in colliders)
            {
                //симуляция падения блока, чтобы отрисовать там тень
                Vector3 colliderSizes = collider.size / 2f;
                hitDetect = Physics.BoxCast(collider.bounds.center, colliderSizes, Vector3.down, out hit, transform.rotation, maxDistance);
                
                if (hitDetect)
                {
                    float hitColTopPositionY = hit.collider.bounds.max.y + BlockPrefab.BlockHeight / 2f;
                    if(colPositionY < hitColTopPositionY)
                    {
                        colPositionY = hitColTopPositionY;
                    }
                }

            }

            shadowPositionY = colPositionY;
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        //отписываемся от событий
        EventManager.TurnLActionClick -= SetTargetRotation;
        EventManager.FastFallActionClick -= SetFastFall;
        
        //TODO сделать уточнение объектов, с которыми произошла коллизия

        if (isPlayerControlled 
            && (other.gameObject.CompareTag("Ground") || other.gameObject.name == "LevelBottom"
            || (other.transform.parent && other.transform.parent.gameObject.GetComponent<FallingBlockController>() != null))
            )
        {
            
            rb.useGravity = true; //включаем гравитацию
            rb.isKinematic = false; //выключаем свойство Kinematic
            SetCollidersIsTrigger(false); //выключаем коллайдерам isTrigger
            //убираем из зоны trigger-а, чтобы не было эффекта выталкивания фигуры
            //эта зона равняется полвысоты
            float triggerZoneOffset = BlockPrefab.BlockHeight / 2f; 
            transform.position = new Vector3(
                transform.position.x, other.bounds.max.y + triggerZoneOffset, transform.position.z); 
           

            isPlayerControlled = false;
            GameController.IsPlayerControlFallingBlock = false;
            
            if (_isBomb && other.gameObject.name != "LevelBottom")
            {
                Boom();
            }

        }

    }

    public void DestroyShadow()
    {
        Destroy(shadowObj);
    }

    private void OnDestroy()
    {
        //всегда уничтожаем тень, если уничтожается сам объект
        DestroyShadow();
        
        //отписываемся от событий
        EventManager.TurnLActionClick -= SetTargetRotation;
        EventManager.FastFallActionClick -= SetFastFall;
    }

    void DrawShadow()
    {
        //рисуем тень
        if (shadowPositionY > -maxDistance)
        {
            shadowObj.transform.position = new Vector3(transform.position.x, shadowPositionY, transform.position.z);
            shadowObj.transform.rotation = transform.rotation;
            shadowObj.SetActive(true);
        }
        else
        {
            shadowObj.SetActive(false);
        }
    }

    void SetTargetRotation(bool clock)
    {
        if (clock)
        {
            targetRotation += 90f;
        }
        else
        {
            targetRotation -= 90f;
        }
    }

    void CheckIsNeedRotate()
    {
        //плавный разворот объект, если целевой угол еще не достигнет
        if (transform.rotation.eulerAngles.y != targetRotation)
        {
            Quaternion targetRot = Quaternion.Euler(0, targetRotation, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 20f * Time.deltaTime);
        }
    }

    //плохо ли упала деталь
    void CheckBadFall()
    {
        float badAngle = 15; //угол, под которым деталь считается кривой
        //и угол по X вышел за рамки (сейчас ось X - потому что изначально детали повернуты вместе со Spawn, т.к. они в другой плоскости из Blender)
        float xAngel = transform.rotation.eulerAngles.x;
        if (xAngel > 180)
        {
            //если больше 180, то нужно взять частно 360 - этот угол.
            xAngel = 360 - xAngel;
        }
        float zAngel = transform.rotation.eulerAngles.z;
        if (zAngel > 180)
        {
            //если больше 180, то нужно взять частно 360 - этот угол.
            zAngel = 360 - zAngel;
        }

        if(xAngel > 0 + badAngle || xAngel < 0 - badAngle ||
           zAngel > 0 + badAngle || zAngel < 0 - badAngle)
        {
            Debug.Log("Bad Fall" + transform.rotation.eulerAngles.x + "/" + transform.rotation.eulerAngles.z);
            
            //удаляем очки, которые за этот блок были добавлены
            EventManager.ChangeScore(-cost);

            //но штраф не должен добавится за такой блок

            Destroy(gameObject);
        }
        
    }

    public bool IsStop()
    {
        //считаем что если очень маленькая скорость по оси Y и небольшая по другим осям это уже остановка, потому что есть некоторые колебания
        //возможно в будущем на другой логике будет основываться эта проверка или что-то измениться еще, поэтому пока проще так
        return Mathf.Abs(rb.velocity.y) < 0.03f && Mathf.Abs(rb.velocity.x) + Mathf.Abs(rb.velocity.z) < 0.75f;
    }

    //итоговая стоимость всего блока целиком с учетом всех бонусов
    public int GetTotalCost()
    {
        int result = 0;

        if (!isPlayerControlled)
        {
            //т.к. цена блока зависит от места, где он находится, а он может упасть в процессе игры, то его сумма очков динамическая
            currentRow = (int)transform.position.y - (int)GameController.GenY + 1 + GameController.GainedRow;

            if (currentRow >= 0) //значит он не ниже, чем платформа
            {
                result = Mathf.CeilToInt(GetBlockCount() * blockCost * (1 + (currentRow - 1) * bonusOfRow));
            }
            
        }

        return result;
    }

    //кол-во блоков 1x1
    int GetBlockCount()
    {
        //кол-во блока берется из отвечаючего за это компонента
        return GetComponent<BlockPrefab>().GetBlockCount();
    }

    void CheckCost()
    {
        int newCost = GetTotalCost();
        if(newCost != cost)
        {
            //если посчиталась новая цена
            int dif = newCost - cost;
            EventManager.ChangeScore(dif);
            cost = newCost;
            
            //после установки цены запускаем эффекты установленного блока
            ShowEffects();
            //моргает глазами
            BlinkEyes();
        }
    }

    public void DoPenalty()
    {
        //если это штраф для блока, который не был установлен, то его цена получается как для первого ряда
        if (cost == 0)
        {
            cost = (int)(GetBlockCount() * blockCost);
        }

        int penalty = (int)(cost * blockPenaltyMultiplier);
        EventManager.ChangeScore(-penalty);
    }

    public int GetCurrentRow()
    {
        return currentRow;
    }

    void ShowEffects()
    {
        if (_pS)
        {
            _pS.Play();
        }
    }

    void BlinkEyes()
    {
        _blinkWaitingTime = Random.Range(MinBlinkWaitTime, MaxBlinkWaitTime);
        _afterBlinkElapsedTime = 0f;
        StartCoroutine(BlinkEyesAnimation());
        
    }

    IEnumerator BlinkEyesAnimation()
    {
        var renderer = BlockPrefab.GetBlockRenderer(gameObject);
        BlockPrefab bP = GetComponent<BlockPrefab>();
        string textureMainFolder = bP.GetTextureMainFolder();
        string path = textureMainFolder + "Face/_blink";
        int materialIndex = BlockPrefab.GetMaterialIndexByName("Face (Instance)", gameObject);
        Texture prevTexture = renderer.materials[materialIndex].mainTexture;
        renderer.materials[materialIndex].mainTexture = Resources.Load<Texture2D>(path);
        yield return new WaitForSeconds (BlinkingTime);
        renderer.materials[materialIndex].mainTexture = prevTexture;

    }

    public void SetIsBomb(bool val)
    {
        _isBomb = val;
    }
    
    public bool GetIsBomb()
    {
        return _isBomb;
    }

    void Fall()
    {
        //если установлено FastFall, то фиксированно умножается на 4f
        ////ToDo возможно заменить на параметр вместо фиксированного
        transform.position += Vector3.down * _fallingSpeed * (_isFastFall? 4f : 1f) * Time.deltaTime;
    }

    void SetFastFall()
    {
        _isFastFall = true;
        
        if (!Tutorial.IsActive())
        {
            _fallingSpeed = 4f;
        }
    }

    void SetCollidersIsTrigger(bool val)
    {
        foreach (var col in colliders)
        {
            col.isTrigger = val;
        }
    }

    void Boom()
    {
        //создаем зону взрыва
        float radius = 6f;
        float power = 300f;
        Vector3 explosionPos = transform.position + Vector3.down * 2.5f;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
                
        //модификатор взрыва, чтобы немного смешались точки взрыва для большей хаотичности
        Vector3[] customModArray = new Vector3[]
            { Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
                
        foreach (Collider hit in colliders)
        {
            //коллайдеры находятся в дочернем элементе, поэтому rigidbody берется у parent
            if (hit.transform.parent)
            {
                Rigidbody explodeRb = hit.transform.parent.gameObject.GetComponent<Rigidbody>();

                if (explodeRb != null)
                {
                    Vector3 customMod = customModArray[Random.Range(0, customModArray.Length)] *
                                        Random.Range(0f, 0.25f);
                    explodeRb.AddExplosionForce(power, explosionPos + customMod, radius, 0.0f);
                }
            }
        }
                
        //отображение взрыва
        GameObject explosion = Instantiate(SpawnController.instance.GetExplosionPrefab(), transform.position,
            Quaternion.identity);
        explosion.AddComponent<ExplosionController>();

        Destroy(gameObject);
    }
}
