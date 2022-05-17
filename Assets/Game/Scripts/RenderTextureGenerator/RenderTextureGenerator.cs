using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RenderTextureGenerator : MonoBehaviour
{
    [SerializeField] private string materialName;

    private static float[] RotateAngels = new float[] { 0f, 90f, -90f, 180f }; 
    
    private void Start()
    {
        float onePartSize = 64f;
        int partsInGenTexture = 8; //кол-во клеток в развертке по ширине
        //необходимо еще задавать размер (x, y * partsInGenTexture) в RenderTexture в инспекторе, но это не нужно динамически менять
        
        GameObject genCanvas = transform.Find("RenderTextureCanvas").gameObject;
        Sprite[] sourceTextures = Resources.LoadAll<Sprite>("Textures/Platform/0001/" + materialName);
        for (var i = 0; i < partsInGenTexture; i++)
        {
            for (var j = 0; j < partsInGenTexture; j++)
            {
                GameObject imageObj = new GameObject();
                Image image = imageObj.AddComponent<Image>();
                float zAngel = RotateAngels[0];
                if (Random.Range(0, 10) < 3) //вероятность 30%
                {
                    image.sprite = sourceTextures[0]; //ставим 0 картинку (чисто цвет)
                }
                else
                {
                    image.sprite = sourceTextures[Random.Range(0, sourceTextures.Length)];
                    zAngel = RotateAngels[Random.Range(1, RotateAngels.Length)];
                }
                
                RectTransform rectTransform = image.GetComponent<RectTransform>();
                rectTransform.SetParent(genCanvas.transform, false);
                rectTransform.sizeDelta = new Vector2(onePartSize, onePartSize);
                //установка pivot и anchor в венхний левый угол
                rectTransform.anchorMax = Vector2.up;
                rectTransform.anchorMin = Vector2.up;
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                //установка позиции
                Vector2 pivotOffset = new Vector2(onePartSize, -onePartSize) / 2f; //смещение по точке pivot
                rectTransform.anchoredPosition = new Vector2(i,-j) * onePartSize + pivotOffset;
                rectTransform.eulerAngles = new Vector3(0f, 0f, zAngel);
                imageObj.SetActive(true);
            }
        }
    }
}
