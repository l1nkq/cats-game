using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIAnimationController : MonoBehaviour
{
    [SerializeField] private GameObject animText;

    [SerializeField] private float liveTimeText;
    
    private Color _color = new Color(0.647f, 0.862f, 0.368f, 0);

    private void Start()
    {
        EventManager.ChangeScoreActionEvent += ShowUIChangeScore;
    }
    
    private void ShowUIChangeScore(int sum)
    {
        GameObject scoretext = Instantiate(animText, gameObject.transform);
        
        if(sum >= 0)
        {
            scoretext.GetComponent<Text>().text = "+" + sum;
        }
        else
        {
            scoretext.GetComponent<Text>().text = sum.ToString();
        }
        
        scoretext.GetComponent<Text>().DOColor(_color, liveTimeText).OnComplete(() => Destroy(scoretext));
    }
    
    private void OnDestroy()
    {
        EventManager.ChangeScoreActionEvent -= ShowUIChangeScore;
    }
}
