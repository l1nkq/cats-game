using DG.Tweening;
using UnityEngine;

public class LevelGeneratorController : MonoBehaviour
{
    [SerializeField] GameObject levelBlockPrefab;
    private float _levelBlockHeight = 20f;

    //поля для построния спирали
    private Vector3 _direction;
    int _drawCountInDirection = 0;
    int _rotateCount = 0;
    int _offsetCountInDirection = 1; //в начале по 1
    Vector3 _genPosition = Vector3.zero; //в этой точке была первая генерация
    int _directionIndex = 0;

    [SerializeField] private GameObject levelBottom;
    private float _partHeightDifference = 2f; //разница между уровнем башни и новой части спирали
    
    // Start is called before the first frame update
    void Start()
    {
        LevelData levelData = GameController.levelData;
        if (levelData != null)
        {
            GenerateLevelBlocks(levelData);
        }
    }
    
    public void GenerateLevelBlocks(LevelData levelData, Vector3 centerPosition = default(Vector3))
    {
        //создаем объект для группирования деталей контректной части уровня
        GameObject newLevel = new GameObject();
        newLevel.name = "level" + GameController.CurrentSpiralPartNumber;
        newLevel.transform.parent = gameObject.transform;
        newLevel.transform.position = centerPosition;
        //размещаем площадку низа уровня под сентральной позицией уровня
        levelBottom.transform.position = centerPosition + Vector3.down * (_partHeightDifference + 0.5f);

        float step = 1f; //толщина блока
        int xCount = levelData.platform.Length;
        for (int i = 0; i < levelData.platform.Length; i++)
        {
            int zCount = levelData.platform[i].Length;
            float[] startPosition = new float[] { -(xCount - 1) * step / 2f, (zCount - 1) * step / 2f };

            for (int j = 0; j < levelData.platform[i].Length; j++)
            {
                if(levelData.platform[i][j] == 1)
                {
                    Vector3 prefPosition = new Vector3(startPosition[0] + step * j, 0, startPosition[1] - step * i);

                    GameObject newObj = Instantiate(levelBlockPrefab, newLevel.transform, false);
                    //Vector3.down - смещение вниз, чтобы верхушка блока совпадала с Y level группы 
                    newObj.transform.position += prefPosition + Vector3.down * _levelBlockHeight / 2f; 
                }
            }
        }

        if (GameController.CurrentSpiralPartNumber > 0)
        {
            float movingLength = 20f;
            //первую часть сразу на нужной высоте показываем
            //части спирали будут выезжать вверх, но в начале опускаем ниже
            newLevel.transform.position += Vector3.down * movingLength; 
            newLevel.transform.DOMoveY(newLevel.transform.position.y + movingLength, 0.5f);
        }
    }
    public void DrawNextSpiralLevelPart(LevelData levelData, float rowChanged)
    {
        Vector3[] levelGeneratorDirections = new Vector3[]
        {
            Vector3.forward, Vector3.left, Vector3.back, Vector3.right
        };

        float step = 5f;
        
        _direction = levelGeneratorDirections[_directionIndex];

        //смещаем вверх на кол-во рядов построенных + _partHeightDifference и смещаем в сторону _direction на шаг step
        _genPosition += Vector3.up * (rowChanged + _partHeightDifference) + _direction * step;
        GenerateLevelBlocks(levelData, _genPosition);
        
        //подготовка данных для следующей части спирали
        _drawCountInDirection++;
        if (_drawCountInDirection == _offsetCountInDirection)
        {
            //поворачиваем
            _directionIndex = (_directionIndex + 1)&(levelGeneratorDirections.Length - 1);
            _direction = levelGeneratorDirections[_directionIndex];
            _rotateCount++;
            if (_rotateCount == 2)
            {
                //каждый два поврота увеличиваем кол-во ходов на 1
                _offsetCountInDirection++;
                _rotateCount = 0;
            }

            _drawCountInDirection = 0;
        }
    }

    public Vector3 GetGenPosition()
    {
        return _genPosition;
    }
}
