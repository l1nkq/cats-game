using UnityEngine;
public class ButtonAnimation : MonoBehaviour
{
    [SerializeField] private Transform shadowButton;
    
    private static readonly float ClickOffset = 14f;

    private Vector2 _startPosition;

    private void Start()
    {
        _startPosition = transform.position;
    }
    private void OnPointerDown()
    {
        //transform.position += Vector3.down * ClickOffset;
        transform.position = new Vector2(transform.position.x, shadowButton.transform.position.y);
    }

    private void OnPointerUp()
    {
        transform.position = _startPosition;
    }
}
