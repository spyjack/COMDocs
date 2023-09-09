using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileData : MonoBehaviour
{
    [SerializeField]
    TileType tileToReplace = TileType.empty;
    [SerializeField]
    bool randomizeTexture = true;
    [SerializeField]
    Vector2 textureOffset = Vector2.zero;
    [SerializeField]
    Renderer matRend;
    [SerializeField]
    Material tileMat;
    public TileType Tile
    {
        get{return tileToReplace;}
    }
    public Material Material
    {
        get{return tileMat;}
    }
    // Start is called before the first frame update
    void Start()
    {
        
        matRend = this.GetComponentInChildren<Renderer>();
        if(tileMat == null)
        {
            tileMat = matRend.material;
        }
        RandomizeTexture();
    }

    void RandomizeTexture()
    {
        textureOffset.x = Mathf.Round (Random.Range(0.0f,1.0f) * 4) / 4;
        textureOffset.y = Mathf.Round (Random.Range(0.0f,1.0f) * 4) / 4;
        matRend.material = tileMat;
        matRend.material.mainTextureOffset = textureOffset;
    }
}
