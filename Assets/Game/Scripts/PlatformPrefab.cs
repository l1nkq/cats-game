using UnityEngine;

public class PlatformPrefab : MonoBehaviour
{
    private static readonly float UVFaceSize = 0.125f; //размер клетки текстуры в UV развертке
    void Start()
    {
        Renderer r = transform.Find("platform_solid").gameObject.GetComponent<Renderer>();
        int xOffset = Random.Range(0, 100);
        int yOffset = Random.Range(0, 100);
        Vector2 offset = new Vector2(xOffset, yOffset) * UVFaceSize;
        Material[] materials = r.materials;
        materials[0].mainTextureOffset = offset;
        materials[1].mainTextureOffset = offset;
    }
}
