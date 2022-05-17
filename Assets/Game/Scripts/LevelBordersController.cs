using UnityEngine;

public class LevelBordersController : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        FallingBlockController fBC = collision.gameObject.GetComponent<FallingBlockController>();

        if (!fBC.GetIsBomb()) //бомбы игнорируются
        {
            if (!GameController.IsArcadeMode())
            {
            
                if (fBC)
                {
                    fBC.DoPenalty();
                }
            }
            else
            {
                EventManager.LevelEnd();
            }
        }
        

        //уничтожаем все что прилетело на границу уровня
        Destroy(collision.gameObject);
    }
}
