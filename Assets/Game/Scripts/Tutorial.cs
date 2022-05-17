using System;
using UnityEngine;
using DG.Tweening;

public class Tutorial : MonoBehaviour
{
    [SerializeField] private GameObject _arrow;
    
    [SerializeField] private GameObject[] _mouse;

    [SerializeField] private float _timeAnimation;

    private int m_IndexNumber = 6;

    private int _teaching;

    private Vector2 _mousePosition;

    private Vector2 _startPos;
    
    private Vector2 _lastPos;

    private bool _firstStep = true;

    private void Start()
    {
        if (IsActive())
        {
            EventManager.FastFallActionClick += FastMoveCat;
            EventManager.CameraRotateActionRClick += CamRotate;
            EventManager.TurnLActionClick += CatRotate;
            
            MoveShadow();
        }
        else
        {
            DestroyAllTeachObjects();
        }
    }

    private void Update()
    {
        if(_firstStep)
        {
            if (Input.touchCount > 0)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                    _startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }
                else if (Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    _lastPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                    if (_startPos.x < _lastPos.x)
                    {
                        _arrow.SetActive(false);
                        _mouse[0].SetActive(false);
                        m_IndexNumber -= 1;
                        _mouse[1].SetActive(true);
                        Vector3 lastPos = GameObject.Find("TurnLButton").transform.position;
                        lastPos.x -= 115f;
                        lastPos.y -= 25f;
                        _mouse[1].GetComponent<Transform>().DOMove(lastPos, _timeAnimation);
                        transform.SetSiblingIndex(m_IndexNumber);
                        _firstStep = false;
                    }
                }
            }
        }
    }

    private void CatRotate(bool rotate) // second step
    {
        _mouse[1].SetActive(false);
        m_IndexNumber -= 1;
        transform.SetSiblingIndex(m_IndexNumber);
        Transform rotateButton = GameObject.Find("TurnLButton").GetComponent<Transform>();
        rotateButton.SetSiblingIndex(0);
        _mouse[2].SetActive(true);
        Vector3 lastPos = GameObject.Find("CameraRotateButton").transform.position;
        lastPos.x += 140f;
        lastPos.y += 5f;
        _mouse[2].GetComponent<Transform>().DOMove(lastPos, _timeAnimation);
        EventManager.TurnLActionClick -= CatRotate;
    }

    private void CamRotate(bool rotate) // Third step
    {
        _mouse[2].SetActive(false);
        m_IndexNumber -= 1;
        transform.SetSiblingIndex(m_IndexNumber);
        Transform cameraRotateButton = GameObject.Find("CameraRotateButton").GetComponent<Transform>();
        cameraRotateButton.SetSiblingIndex(0);
        _mouse[3].SetActive(true);
        Vector3 lastPos = GameObject.Find("FastFallButton").transform.position;
        lastPos.x -= 125f;
        lastPos.y -= 25f;
        _mouse[3].GetComponent<Transform>().DOMove(lastPos, _timeAnimation);
        EventManager.CameraRotateActionRClick -= CamRotate;
    }

    private void MoveShadow() // first step
    {
        _arrow.SetActive(true);
        _mouse[0].SetActive(true);
        Vector3 lastPos = GameObject.Find("Arrow").transform.position;
        lastPos.x += 250;
        lastPos.y -= 75;
        _mouse[0].GetComponent<Transform>().DOMove(lastPos, _timeAnimation);
    }

    private void FastMoveCat() // last step
    {
        m_IndexNumber -= 1;
        transform.SetSiblingIndex(m_IndexNumber);
        EventManager.FastFallActionClick -= FastMoveCat;
        DestroyAllTeachObjects();
    }

    private void DestroyAllTeachObjects()
    {
        int _teaching = 1;
        PlayerPrefs.SetInt("Teaching", _teaching);

        for (int i = 0; i < _mouse.Length; i++)
        {
            Destroy(_mouse[i]);
        }
        
        Destroy(_arrow);
        Destroy(gameObject);
    }

    public static bool IsActive()
    {
        return PlayerPrefs.GetInt("Teaching") == 0;
    }
}
