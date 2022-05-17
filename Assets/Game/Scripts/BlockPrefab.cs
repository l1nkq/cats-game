using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPrefab : MonoBehaviour
{
    //вынесо в отдельный класс, для того чтобы можно было рассчитывать кол-во блоков в prefab-е, даже если он не является FallingBlock
    [SerializeField]
    int blocksCount; //кол-во блоков 1х1 в текущем, задается вручную в испекторе

    BoxCollider[] colliders;
    [SerializeField] GameObject catParticleSystemPrefab;
    private ParticleSystem _pS;
    
    public static float BlockHeight = 1f;

    private string _textureMainFolder; //используется чтобы в процессе знать из какой папки текстуры у кота

    private void Awake()
    {
        colliders = transform.Find("colliders").gameObject.GetComponents<BoxCollider>();
        
        _pS = transform.Find("CatParticleSystem").GetComponent<ParticleSystem>();
    }

    //кол-во блоков 1x1
    public int GetBlockCount()
    {
        return blocksCount;
    }

    public BoxCollider[] GetColliders()
    {
        return colliders;
    }

    public ParticleSystem GetParticleSystem()
    {
        return _pS;
    }

    public static Renderer GetBlockRenderer(GameObject g)
    {
        return g.transform.Find("model").gameObject.GetComponent<Renderer>();
    }

    public string GetTextureMainFolder()
    {
        return _textureMainFolder;
    }
    
    public void SetTextureMainFolder(string path)
    {
        _textureMainFolder = path;
    }

    public static int GetMaterialIndexByName(string s, GameObject g)
    {
        var renderer = GetBlockRenderer(g);
        //генерация индексов материалов
        Dictionary<string, int> materialsIndexes = new Dictionary<string, int>();
        int materialIndex = 0;
        foreach (Material material in renderer.materials)
        {
            materialsIndexes.Add(material.name, materialIndex);
            materialIndex++;
        }

        return materialsIndexes[s];
    }
}
