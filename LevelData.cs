using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    /// <summary>
    /// Vector2 Int of the chunk count in the level.
    /// </summary>
    public Vector2Int levelSize = Vector2Int.one;

    /// <summary>
    /// The base amount of tiles for each chunk. The total amount is chunksize squared.
    /// </summary>
    public int chunkSize = 16;

    /// <summary>
    /// Information asset about enemies and the difficulty of the level.
    /// </summary>
    public LevelDifficultyData difficultyData;

    /// <summary>
    /// 2D Array of GridChunks that contain info about every chunk.
    /// </summary>
    public GridChunk[,] chunkGrid;

    [SerializeField]
    private List<Vector2Int> enemySpawns;

    /// <summary>
    /// List of all coordinates enemies can spawn on.
    /// </summary>
    public List<Vector2Int> EnemySpawns
    { get { return enemySpawns; } }

    /// <summary>
    /// Spawnpoint of the player(s). Defaults to 0,0
    /// </summary>
    public Vector2 playerSpawnpoint = Vector2.zero;

    //Dictionary to store how many of each tile is in the level, used for mesh generation.
    public Dictionary<TileType, int> tileQuantity = new Dictionary<TileType, int>();
    public Vector2Int Bounds
    {
        get{return new Vector2Int((chunkGrid.GetLength(0)) * chunkSize, (chunkGrid.GetLength(1)) * chunkSize);}
    }

    /// <summary>
    /// Check if a coordinate is inside of the levels boundries.
    /// </summary>
    /// <param name="_pos">The integer Vector2 position to check for.</param>
    /// <returns>Returns false if it outside of the boundries, true if it is.</returns>
    public bool InsideBounds(Vector2Int _pos)
    {
        if (_pos.x < 0 || _pos.y < 0 || _pos.x >= Bounds.x || _pos.y >= Bounds.y)
        {
            return false;
        }
        return true;
    }

    public void UpdateTileQuantity(TileType _tile, int _amount)
    {
        if(tileQuantity.ContainsKey(_tile))
        {
            tileQuantity[_tile] += _amount;
        }else
        {
            tileQuantity.Add(_tile, _amount);
        }
    }
    public void UpdateTileQuantity(TileType _tile, int _amount, Dictionary<TileType, int> _dict)
    {
        if(_dict.ContainsKey(_tile))
        {
            _dict[_tile] += _amount;
            _dict[_tile] = Mathf.Max(0, _dict[_tile]);
        }else
        {
            _dict.Add(_tile, _amount);
        }
    }

    public void SetTile(TileType _tile, Vector2Int _pos)
    {
        if(!InsideBounds(_pos)){return;}

        Vector2Int _chunkPos = new Vector2Int();
        Vector2Int _tilePos = new Vector2Int();

        ToChunkCoords(_pos, out _chunkPos, out _tilePos);
        //Deal with setting and unsetting tile quantities in here
        UpdateTileQuantity(chunkGrid[_chunkPos.x, _chunkPos.y].floorGrid[_tilePos.x, _tilePos.y], -1, chunkGrid[_chunkPos.x, _chunkPos.y].tileQuantity);
        UpdateTileQuantity(_tile, 1, chunkGrid[_chunkPos.x, _chunkPos.y].tileQuantity);

        //Debug.LogWarning("Setting Tile at: " + _pos.x + ", " + _pos.y + ". At chunk: " + _chunkPos.x + ", " + _chunkPos.y + " and sub tile: " + _tilePos.x + ", " + _tilePos.y);
        chunkGrid[_chunkPos.x, _chunkPos.y].floorGrid[_tilePos.x, _tilePos.y] = _tile;
    }

    public TileType GetTile(Vector2Int _pos)
    {
        if(!InsideBounds(_pos)){return TileType.empty;}

        Vector2Int _chunkPos = new Vector2Int();
        Vector2Int _tilePos = new Vector2Int();

        ToChunkCoords(_pos, out _chunkPos, out _tilePos);

        //Debug.LogWarning("Getting Tile at: " + _pos.x + ", " + _pos.y + ". At chunk: " + _chunkPos.x + ", " + _chunkPos.y + " and sub tile: " + _tilePos.x + ", " + _tilePos.y);
        return chunkGrid[_chunkPos.x, _chunkPos.y].floorGrid[_tilePos.x, _tilePos.y];
    }

    public void SetObject(ObjectType _obj, Vector2Int _pos)
    {
        if(!InsideBounds(_pos)){return;}

        Vector2Int _chunkPos = new Vector2Int();
        Vector2Int _objPos = new Vector2Int();

        ToChunkCoords(_pos, out _chunkPos, out _objPos);

        chunkGrid[_chunkPos.x, _chunkPos.y].objGrid[_objPos.x, _objPos.y] = _obj;
    }

    public ObjectType GetObject(Vector2Int _pos)
    {
        Vector2Int _chunkPos = new Vector2Int();
        Vector2Int _objPos = new Vector2Int();

        ToChunkCoords(_pos, out _chunkPos, out _objPos);

        return chunkGrid[_chunkPos.x, _chunkPos.y].objGrid[_objPos.x, _objPos.y];
    }

    /// <summary>
    /// Input general coordinates, output the corresponding chunk position and the chunk's tiles position.
    /// </summary>
    /// <param name="_inCoords">Input coordinates to convert.</param>
    /// <param name="_gridXY">Output of the chunk containing the input coordinates.</param>
    /// <param name="_gridTileXY">Output of the tile position of the coordinates inside the chunk.</param>
    public void ToChunkCoords(Vector2Int _inCoords, out Vector2Int _gridXY, out Vector2Int _gridTileXY)
    {
        Vector2 _chunkXCoords = ToChunkSpace(_inCoords.x);
        Vector2 _chunkYCoords = ToChunkSpace(_inCoords.y);

        _gridXY = new Vector2Int();
        _gridTileXY = new Vector2Int();

        _gridXY.x = Mathf.RoundToInt(_chunkXCoords.x);
        _gridTileXY.x = Mathf.RoundToInt(_chunkXCoords.y);

        _gridXY.y = Mathf.RoundToInt(_chunkYCoords.x);
        _gridTileXY.y = Mathf.RoundToInt(_chunkYCoords.y);
    }

    /// <summary>
    /// Rounds a number to fit in a localized chunk space.
    /// </summary>
    /// <param name="_x"></param>
    /// <returns>X is the rounded Chunk Coordinate, Y is the tile coordinate inside the chunk.</returns>
    public Vector2 ToChunkSpace(float _x)
    {
        Vector2 _output = new Vector2();
        //Chunk coord
        _output.x = Mathf.FloorToInt(_x / chunkSize);
        //Tile coord
        _output.y = ((_x / chunkSize) - Mathf.Floor(_x / chunkSize)) * chunkSize;

        return _output;
    }

    public void DebugPrintQuantities()
    {
        foreach (KeyValuePair<TileType, int> _tile in tileQuantity)
        {
            Debug.Log("There are " + _tile.Value + " " + _tile.Key + " tiles in the level.");
        }
    }

    public void RefreshEnemySpawns()
    {
        enemySpawns = new List<Vector2Int>();
        for (int x = 0; x < Bounds.x; x++)
        {
            for (int y = 0; y < Bounds.y; y++)
            {
                if(GetObject(new Vector2Int(x,y)) == ObjectType.enemySpawn)
                {
                    enemySpawns.Add(new Vector2Int(x,y));
                }
            }
        }
    }
}

/// <summary>
/// An individual chunk in a level.
/// </summary>
public struct GridChunk
{
    /// <summary>
    /// Where in the level this chunk is placed.
    /// </summary>
    public Vector2Int position;

    /// <summary>
    /// 2D array of TileTypes that correspond to the floor of the chunk.
    /// </summary>
    public TileType[,] floorGrid;

    /// <summary>
    /// 2D array of ObjectTypes that correspond to the objects randomly generated above the floor in the chunk.
    /// </summary>
    public ObjectType[,] objGrid;

    /// <summary>
    /// Quantity of each TileType in the level.
    /// </summary>
    public Dictionary<TileType, int> tileQuantity;
}

/// <summary>
/// List of all Terrain Tiles that can be generated in a level.
/// </summary>
public enum TileType
{
    empty,
    grass,
    dirt,
    wall,
    water,
    wood0,
    wheat,
}

/// <summary>
/// List of all object/prop types that can be generated in a level via color coding and randomization. The individual props are picked at random based on these values.
/// </summary>
public enum ObjectType
{
    empty,
    prop,
    tree,
    grass,
    tallgrass,
    playerSpawn,
    enemySpawn,
    npcSpawn,
    campfire,
    relay,
    cage,
}
