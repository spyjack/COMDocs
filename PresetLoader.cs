using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PresetLoader : MonoBehaviour
{
    [SerializeField]
    private List<SceneryChunk> sceneryChunkLibrary = new List<SceneryChunk>();
    [SerializeField]
    private List<Texture2D> sceneryChunksTextures = new List<Texture2D>();
    [SerializeField]    
    private List<Texture2D> sceneryChunksObjTextures = new List<Texture2D>();
    [SerializeField]    
    private List<SceneryChunkInfo> sceneryChunksInfoList = new List<SceneryChunkInfo>();
    private Object[] chunkTextures;

    private List<SceneryChunk> presetQuery;

    [SerializeField] private TileColor[] imageColorDefinitions;

    public List<SceneryChunk> SceneryChunkLibrary
    {
        get {return sceneryChunkLibrary;}
    }
    // Start is called before the first frame update
    void Start()
    {
        LoadSceneryChunkResources("World");
        InitializeSceneryChunkLibrary();
    }

    // Update is called once per frame
    void Update()
    {
        
        
    }

    //Loads from the resouces folders
    void LoadSceneryChunkResources(string dir)
    {
        chunkTextures = Resources.LoadAll(dir, typeof(Texture2D));
        Object[] allChunkInfo = Resources.LoadAll(dir, typeof(TextAsset));
        foreach (var ct in chunkTextures)
        {
            if (ct != null)
            {
                if(ct.name.EndsWith("_Obj"))
                {
                    //Texture files ending with _Obj designate the objects above the base ground, optional
                    sceneryChunksObjTextures.Add((Texture2D)ct);
                }else
                {
                    sceneryChunksTextures.Add((Texture2D)ct);
                }
            }
        }

        foreach (var ci in allChunkInfo)
        {
            if (ci != null)
            {
                TextAsset txt = (TextAsset)ci;
                SceneryChunkInfoContainer scJsonInfo = JsonUtility.FromJson<SceneryChunkInfoContainer>(txt.text);
 
                foreach (SceneryChunkInfo chunkInfoText in scJsonInfo.chunksInfo)
                {
                    sceneryChunksInfoList.Add(chunkInfoText);
                }
            }
        }
    }

    //Go through each one, collect the info and store it together
    void InitializeSceneryChunkLibrary()
    {
        for (int i = 0; i < sceneryChunksTextures.Count; i++)
        {
            SceneryChunk newScenery = new SceneryChunk();
            newScenery.chunkName = sceneryChunksTextures[i].name;
            newScenery.chunkWidth = sceneryChunksTextures[i].width;
            newScenery.chunkHeight = sceneryChunksTextures[i].height;
            newScenery.layer = new ChunkLayer();

            newScenery.layer.tileGrid = TextureToTileGrid(sceneryChunksTextures[i]);

            for (int ot = 0; ot < sceneryChunksObjTextures.Count; ot++)
            {
                if(newScenery.chunkName + "_Obj" == sceneryChunksObjTextures[ot].name)
                {
                    print("Assigning " + newScenery.chunkName + " predetermined obj layer.");
                    //Set new scenery object grid to the object's image file
                    newScenery.layer.objectGrid = TextureToObjectGrid(sceneryChunksObjTextures[ot]);
                    break;
                }
                
                if(ot + 1 >= sceneryChunksObjTextures.Count) {newScenery.layer.objectGrid = GetEmptyGrid(newScenery.chunkWidth, newScenery.chunkHeight);}
            }
            
            foreach (SceneryChunkInfo info in sceneryChunksInfoList)
            {
                if(newScenery.chunkName == info.chunkName)
                {
                    newScenery.chunkTheme = info.chunkTheme;
                }
                break;
            }

            sceneryChunkLibrary.Add(newScenery);
        }
    }

    //Turns an Image to a tile grid
    public TileType[,] TextureToTileGrid(Texture2D _tex)
    {
        TileType[,] newGrid = new TileType[_tex.width, _tex.height];
        Color32[] pixels = _tex.GetPixels32();

        for (int X = 0; X < newGrid.GetLength(0); X++)
        {
            for (int Y = 0; Y < newGrid.GetLength(1); Y++)
            {
                Color32 color = pixels[(Y * _tex.width) + X];
                newGrid[X,Y] = SetTileFromColor(color);
            }
        }

        return newGrid;
    }

    //Turns an Image to a object grid
    public ObjectType[,] TextureToObjectGrid(Texture2D _tex)
    {
        ObjectType[,] newGrid = new ObjectType[_tex.width, _tex.height];
        Color32[] pixels = _tex.GetPixels32();

        for (int X = 0; X < newGrid.GetLength(0); X++)
        {
            for (int Y = 0; Y < newGrid.GetLength(1); Y++)
            {
                Color32 color = pixels[(Y * _tex.width) + X];
                newGrid[X,Y] = SetObjFromColor(color);
            }
        }

        return newGrid;
    }

    public bool GetSceneryByName(string _name, out SceneryChunk _scenery)
    {
        for (int i = 0; i < sceneryChunkLibrary.Count; i++)
        {
            if(sceneryChunkLibrary[i].chunkName == _name)
            {
                _scenery = sceneryChunkLibrary[i];
                return true;
            }
        }
        _scenery = new SceneryChunk();
        return false;
    }

    public bool GetSceneryByTheme(string _theme, out SceneryChunk _scenery)
    {
        List<SceneryChunk> _query = GenerateSceneryQuery(_theme);
        int _index = Random.Range(0,presetQuery.Count);
        if(presetQuery[_index].chunkTheme.Contains(_theme))
        {
            _scenery = sceneryChunkLibrary[_index];
            return true;
        }
        _scenery = new SceneryChunk();
        return false;
    }

    public List<SceneryChunk> GenerateSceneryQuery(string _themes)
    {
        List<SceneryChunk> _query = new List<SceneryChunk>();

        for (int i = 0; i < SceneryChunkLibrary.Count; i++)
        {
            if(_themes.Contains(SceneryChunkLibrary[i].chunkTheme + ","))
            {
                _query.Add(SceneryChunkLibrary[i]);
            }
        }

        return _query;
    }

    ObjectType[,] GetEmptyGrid(int width, int height)
    {
        ObjectType[,] _grid = new ObjectType[width, height];
        for (int X = 0; X < _grid.GetLength(0); X++)
        {
            for (int Y = 0; Y < _grid.GetLength(1); Y++)
            {
                _grid[X,Y] = ObjectType.empty;
            }
        }
        return _grid;
    }

    TileType SetTileFromColor(Color _color)
    {
        for (int i = 0; i < imageColorDefinitions.Length; i++)
        {
            if(imageColorDefinitions[i].tileColor == _color)
            {
                print(imageColorDefinitions[i].tileType.ToString());
                print(imageColorDefinitions[i].tileColor);
                return imageColorDefinitions[i].tileType;
            }
        }
        return TileType.empty;
    }

    ObjectType SetObjFromColor(Color _color)
    {
        for (int i = 0; i < imageColorDefinitions.Length; i++)
        {
            if(imageColorDefinitions[i].tileColor == _color)
            {
                return imageColorDefinitions[i].objType;
            }
        }
        return ObjectType.empty;
    }
}

//All scenery chunk data
[System.Serializable]
public class SceneryChunk
{
    public string chunkName;
    public int chunkHeight, chunkWidth;

    public ChunkLayer layer;

    //public List<int> doorOffsetsNorth, doorOffsetsEast, doorOffsetsSouth, doorOffsetsWest;

    public string chunkTheme;
}

[System.Serializable]
public class ChunkLayer
{
    public TileType[,] tileGrid;

    public ObjectType[,] objectGrid;
}

[System.Serializable]
public class SceneryChunkInfo
{
    public string chunkName;
    public string chunkTheme;
    public float chunkWeight;
}

[System.Serializable]
public class SceneryChunkInfoContainer
{
    public SceneryChunkInfo[] chunksInfo;
}

[System.Serializable]
public class TileColor
{
    public TileType tileType;
    public ObjectType objType;
    public Color32 tileColor;
}
