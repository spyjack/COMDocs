using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/* TO-DO:
// - Add support for walls
// - Revamp Vegetation generation to support different types/biomes
// - Add Objective Spawning - DONE
// - Add Prop Spawning (Probably connected to Vegetation types) - DONE
//
//
//
*/

public class LevelInstantiator : MonoBehaviour
{
     [SerializeField]
    private Vector2 TileSizeToUnits = Vector2.one;
    public Vector2 TileSize{get{return TileSizeToUnits;}}
    [SerializeField]
    private float tileHeight = 2f;
    private static LevelInstantiator levelInstantiatorInstance;
    public static LevelInstantiator Instantiator { get { return levelInstantiatorInstance; } }
    public GenerationDefinition genData;

    private LevelDifficultyData difficultyData;
    [SerializeField]
    private LevelData lvl;
    public bool generateLevel = false;
    public bool levelInstantiated = false;
    [Header("Level Assets")]
    [SerializeField]
    List<Transform> terrainTiles = new List<Transform>();
    [SerializeField]
    List<Transform> objectTiles = new List<Transform>();
    [SerializeField]
    Light sun;
    [Header("Level Bounds")]
    [SerializeField] BoxCollider levelExit;
    [SerializeField] BoxCollider wallLeft;
    [SerializeField] BoxCollider wallRight;
    [SerializeField] BoxCollider wallTop;
    [SerializeField] BoxCollider wallBottom;
    [Header("NavMesh Data")]
    [SerializeField] LayerMask navigationLayers;
    [SerializeField] private int navSettingsId;
    private NavMeshData navMeshData;
    private NavMeshDataInstance navMeshDataInstance;
    // Start is called before the first frame update
    void Awake()
    {
        if (levelInstantiatorInstance != null && levelInstantiatorInstance != this)
        {
            Destroy(this.gameObject);
            print("Destroying self");
        } else {
            levelInstantiatorInstance = this;
        }
        DontDestroyOnLoad(levelInstantiatorInstance);
        //difficultyData = GenerateDifficultyData();
        //LevelGenerator.Generator.CreateNewLevelData(genData, difficultyData);
        generateLevel = true;
        LoadAssetResources("Tiles", terrainTiles);
        LoadAssetResources("Objects", objectTiles);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool BeginLevelInstatiation()
    {
        if(LevelGenerator.Generator.Level != null && !LevelGenerator.Generator.IsGenerating && generateLevel)
        {
            generateLevel = false;
            levelInstantiated = false;
            LevelGenerator.Generator.Level.DebugPrintQuantities();
            InstantiateLevelData(LevelGenerator.Generator.Level);
            return true;
        }
        return false;
    }

    public Transform InstantiateLevelData(LevelData _level)
    {
        Transform levelParent = new GameObject().transform;
        levelParent.name = "Game Level";

        //InstantiateTileGrid(_level.floorGrid, levelParent);
        CreateDiscreteTilesFromGrid(_level, levelParent);

        CreateObjectsFromChunks(_level, levelParent);

        BuildNavMeshData(levelParent);

        SetBoundries(_level);
        SetExitType();

        _level.difficultyData.AddObjectivesToMain();

        _level.RefreshEnemySpawns();
        CreateEnemies(_level, levelParent);

        _level.playerSpawnpoint *= TileSizeToUnits * 2;
        _level.playerSpawnpoint += new Vector2(0,1);

        if(sun != null)
        {
            sun.intensity = ProgressionManager.ProgressionInstance.GetTimeLightscale();
        }

        return levelParent;
    }

    void LoadAssetResources(string _dir, List<Transform> _listToAddTo)
    {
        Object[] _tileAssets = Resources.LoadAll(_dir, typeof(Transform));
        foreach (var asset in _tileAssets)
        {
            _listToAddTo.Add((Transform)asset);
        }
    }

    void CreateDiscreteTilesFromGrid(LevelData _level, Transform _levelParent)
    {
        //Setting an array of tiletype mesh data to the length of the level's tile quantity dictionary.
        //Then going through that dictionary, turning all the key values into new tile mesh data to use.
        
        //Loop through all chunks,
        for (int chunkX = 0; chunkX < _level.chunkGrid.GetLength(0); chunkX++)
        {
            for (int chunkY = 0; chunkY < _level.chunkGrid.GetLength(1); chunkY++)
            {
                TileTypeMeshData[] tileMeshes = new TileTypeMeshData[_level.chunkGrid[chunkX,chunkY].tileQuantity.Count];
                int tmIndex = 0;
                foreach (KeyValuePair<TileType, int> item in _level.chunkGrid[chunkX,chunkY].tileQuantity)
                {//Get the area of the tile type, multiplied by the number of points needed.
                    int faces = 1;
                    bool isWall = false;
                    if(IsWallTile(item.Key)) //If it is a special wall tile, set the multiplier to 5
                    {
                        faces = 5;
                        isWall = true;
                    }
                    
                    tileMeshes[tmIndex] = new TileTypeMeshData(
                    item.Key,
                    new Vector3[item.Value * item.Value * 4 * faces], //Total amount of tiles in each chunk * the amount of verticies per tile(4) * the amount of possible sides(5)
                    new Vector2[item.Value * item.Value * 4 * faces], //Total amount of tiles in each chunk * the amount of UVs per tile (4) * the amount of possible sides(5)
                    new int[item.Value * item.Value * 6 * faces] //Total amount of tiles in each chunk * the amount of verticies(3) per triangle(2) per tile(6) * the amount of possible sides(5)
                    );

                    tileMeshes[tmIndex].isWall = isWall;

                    tmIndex++;
                }

                //Loop through chunk tile grid X & Y
                for (int x = 0; x < _level.chunkSize; x++)
                {
                    for (int y = 0; y < _level.chunkSize; y++)
                    {
                        foreach (TileTypeMeshData tm in tileMeshes)
                        {
                            if(tm.Tile == TileType.empty) {continue;}
                            Vector2Int worldCoords = new Vector2Int(x + (chunkX * _level.chunkSize), y + (chunkY * _level.chunkSize));
                            if(tm.Tile == _level.GetTile(new Vector2Int(worldCoords.x,worldCoords.y))) {//if not the tile for the corresponding tile mesh, skip to next tile mesh option
                                
                                Vector3 cellPosition = new Vector3((x + worldCoords.x) * TileSizeToUnits.x + TileSizeToUnits.x, 0, (y + worldCoords.y) * TileSizeToUnits.y + TileSizeToUnits.y);
                                Vector2 textureOffset = new Vector2(Mathf.Round(Random.Range(0.0f, 1.0f) * 4) / 4, Mathf.Round(Random.Range(0.0f, 1.0f) * 4) / 4);
                                
                                if (!tm.isWall) //All this shit needs to be put into a function
                                {
                                    tm.vertices[tm.vTrack] = new Vector3(-TileSizeToUnits.x, 0, -TileSizeToUnits.y) + cellPosition;
                                    tm.vertices[tm.vTrack + 1] = new Vector3(-TileSizeToUnits.x, 0, TileSizeToUnits.y) + cellPosition;
                                    tm.vertices[tm.vTrack + 2] = new Vector3(TileSizeToUnits.x, 0, -TileSizeToUnits.y) + cellPosition;
                                    tm.vertices[tm.vTrack + 3] = new Vector3(TileSizeToUnits.x, 0, TileSizeToUnits.y) + cellPosition;

                                    tm.uv[tm.uvTrack] = new Vector2((-TileSizeToUnits.x + cellPosition.x) / 4 + textureOffset.x, (-TileSizeToUnits.y + cellPosition.z) / 4 + textureOffset.y);
                                    tm.uv[tm.uvTrack + 1] = new Vector2((-TileSizeToUnits.x + cellPosition.x) / 4 + textureOffset.x, (TileSizeToUnits.y + cellPosition.z) / 4 + textureOffset.y);
                                    tm.uv[tm.uvTrack + 2] = new Vector2((TileSizeToUnits.x + cellPosition.x) / 4 + textureOffset.x, (-TileSizeToUnits.y + cellPosition.z) / 4 + textureOffset.y);
                                    tm.uv[tm.uvTrack + 3] = new Vector2((TileSizeToUnits.x + cellPosition.x) / 4 + textureOffset.x, (TileSizeToUnits.y + cellPosition.z) / 4 + textureOffset.y);

                                    tm.triangles[tm.tTrack] = tm.vTrack;
                                    tm.triangles[tm.tTrack + 1] = tm.vTrack + 1;
                                    tm.triangles[tm.tTrack + 2] = tm.vTrack + 2;
                                    tm.triangles[tm.tTrack + 3] = tm.vTrack + 2;
                                    tm.triangles[tm.tTrack + 4] = tm.vTrack + 1;
                                    tm.triangles[tm.tTrack + 5] = tm.vTrack + 3;

                                    tm.vTrack += 4;
                                    tm.uvTrack += 4;
                                    tm.tTrack += 6;
                                    break;
                                }else //If it IS a wall
                                {
                                    //Set wall texture offset
                                    Vector2 wallTextureOffset = new Vector2(Mathf.Round(Random.Range(0.25f, 0.5f) * 4) / 4, Mathf.Round(Random.Range(0.25f, 0.5f) * 4) / 4);
                                    bool isUp = IsWallTile(_level.GetTile(new Vector2Int(worldCoords.x, worldCoords.y) + Vector2Int.up));
                                    bool isDown = IsWallTile(_level.GetTile(new Vector2Int(worldCoords.x, worldCoords.y) + Vector2Int.down));
                                    bool isLeft = IsWallTile(_level.GetTile(new Vector2Int(worldCoords.x, worldCoords.y) + Vector2Int.left));
                                    bool isRight = IsWallTile(_level.GetTile(new Vector2Int(worldCoords.x, worldCoords.y) + Vector2Int.right));

                                    if(isUp && isDown && isRight && isLeft)
                                    {
                                        wallTextureOffset = new Vector2(Mathf.Round(Random.Range(0.25f, 0.5f) * 4) / 4, Mathf.Round(Random.Range(0.25f, 0.5f) * 4) / 4);
                                    }
                                    else if(isUp && isDown && isRight)//Left edge tiles
                                    {
                                        wallTextureOffset = new Vector2(0, Mathf.Round(Random.Range(0.25f, 0.5f) * 4) / 4);
                                        //wallTextureOffset = new Vector2(0f, 0.25f);
                                    }
                                    else if(isUp && isDown && isLeft)//Right edge tiles
                                    {
                                        wallTextureOffset = new Vector2(0.75f, Mathf.Round(Random.Range(0.25f, 0.5f) * 4) / 4);
                                        //wallTextureOffset = new Vector2(0.75f, 0.25f);
                                    }
                                    else if(isDown && isLeft && isRight)//Top edge tiles
                                    {
                                        wallTextureOffset = new Vector2(Mathf.Round(Random.Range(0.25f, 0.5f) * 4) / 4, 0.75f);
                                        //wallTextureOffset = new Vector2(0.25f, 0.75f);
                                    }
                                    else if(isUp && isLeft && isRight)//Bottom edge tiles
                                    {
                                        wallTextureOffset = new Vector2(Mathf.Round(Random.Range(0.25f, 0.5f) * 4) / 4, 0);
                                        //wallTextureOffset = new Vector2(0.25f, 0f);
                                    }else if(isDown && isRight && !isLeft && !isUp)//Top Left Corner
                                    {
                                        wallTextureOffset = new Vector2(0f, 0.75f);
                                    }
                                    else if (isDown && isLeft && !isRight && !isUp)//Top Right Corner
                                    {
                                        wallTextureOffset = new Vector2(0.75f, 0.75f);
                                    }
                                    else if (isUp && isRight && !isLeft && !isDown)//Bottom Left Corner
                                    {
                                        wallTextureOffset = new Vector2(0f, 0f);
                                    }
                                    else if (isUp && isLeft && !isRight && !isDown)//Bottom Right Corner
                                    {
                                        wallTextureOffset = new Vector2(0.75f, 0f);
                                    }else if(isDown && !isLeft && !isRight && !isUp)
                                    {
                                        wallTextureOffset = new Vector2(Mathf.Round(Random.Range(0.25f, 0.5f) * 4) / 4, 0.75f);
                                    }
                                    else if ((!isDown && !isLeft && !isRight && isUp) || (isDown && !isLeft && isRight && isUp) || (!isDown && isLeft && !isRight && !isUp))
                                    {
                                        wallTextureOffset = new Vector2(Mathf.Round(Random.Range(0.25f, 0.5f) * 4) / 4, 0f);
                                    }

                                    //Top face
                                    tm.vertices[tm.vTrack] = new Vector3(-TileSizeToUnits.x, tileHeight, -TileSizeToUnits.y) + cellPosition;
                                    tm.vertices[tm.vTrack + 1] = new Vector3(-TileSizeToUnits.x, tileHeight, TileSizeToUnits.y) + cellPosition;
                                    tm.vertices[tm.vTrack + 2] = new Vector3(TileSizeToUnits.x, tileHeight, -TileSizeToUnits.y) + cellPosition;
                                    tm.vertices[tm.vTrack + 3] = new Vector3(TileSizeToUnits.x, tileHeight, TileSizeToUnits.y) + cellPosition;

                                    tm.uv[tm.uvTrack] = new Vector2((-TileSizeToUnits.x / 4) + wallTextureOffset.x + 0.125f, (-TileSizeToUnits.y / 4) + wallTextureOffset.y + 0.125f);
                                    tm.uv[tm.uvTrack + 1] = new Vector2((-TileSizeToUnits.x / 4) + wallTextureOffset.x + 0.125f, (TileSizeToUnits.y / 4) + wallTextureOffset.y + 0.125f);
                                    tm.uv[tm.uvTrack + 2] = new Vector2((TileSizeToUnits.x / 4) + wallTextureOffset.x + 0.125f, (-TileSizeToUnits.y / 4) + wallTextureOffset.y + 0.125f);
                                    tm.uv[tm.uvTrack + 3] = new Vector2((TileSizeToUnits.x / 4) + wallTextureOffset.x + 0.125f, (TileSizeToUnits.y / 4) + wallTextureOffset.y + 0.125f);

                                    tm.triangles[tm.tTrack] = tm.vTrack;
                                    tm.triangles[tm.tTrack + 1] = tm.vTrack + 1;
                                    tm.triangles[tm.tTrack + 2] = tm.vTrack + 2;
                                    tm.triangles[tm.tTrack + 3] = tm.vTrack + 2;
                                    tm.triangles[tm.tTrack + 4] = tm.vTrack + 1;
                                    tm.triangles[tm.tTrack + 5] = tm.vTrack + 3;

                                    //North Face
                                    if (!isUp)
                                    {
                                        tm.vertices[tm.vTrack + 4] = new Vector3(-TileSizeToUnits.x, tileHeight, TileSizeToUnits.y) + cellPosition;
                                        tm.vertices[tm.vTrack + 5] = new Vector3(-TileSizeToUnits.x, 0, TileSizeToUnits.y) + cellPosition;
                                        tm.vertices[tm.vTrack + 6] = new Vector3(TileSizeToUnits.x, tileHeight, TileSizeToUnits.y) + cellPosition;
                                        tm.vertices[tm.vTrack + 7] = new Vector3(TileSizeToUnits.x, 0, TileSizeToUnits.y) + cellPosition;

                                        tm.uv[tm.uvTrack + 4] = new Vector2((-TileSizeToUnits.x + cellPosition.x) / 4 + textureOffset.x, (-TileSizeToUnits.y + cellPosition.z) / 4 + textureOffset.y);
                                        tm.uv[tm.uvTrack + 5] = new Vector2((-TileSizeToUnits.x + cellPosition.x) / 4 + textureOffset.x, (TileSizeToUnits.y + cellPosition.z) / 4 + textureOffset.y);
                                        tm.uv[tm.uvTrack + 6] = new Vector2((TileSizeToUnits.x + cellPosition.x) / 4 + textureOffset.x, (-TileSizeToUnits.y + cellPosition.z) / 4 + textureOffset.y);
                                        tm.uv[tm.uvTrack + 7] = new Vector2((TileSizeToUnits.x + cellPosition.x) / 4 + textureOffset.x, (TileSizeToUnits.y + cellPosition.z) / 4 + textureOffset.y);

                                        tm.triangles[tm.tTrack + 6] = tm.vTrack + 4;
                                        tm.triangles[tm.tTrack + 7] = tm.vTrack + 5;
                                        tm.triangles[tm.tTrack + 8] = tm.vTrack + 6;
                                        tm.triangles[tm.tTrack + 9] = tm.vTrack + 6;
                                        tm.triangles[tm.tTrack + 10] = tm.vTrack + 5;
                                        tm.triangles[tm.tTrack + 11] = tm.vTrack + 7;
                                    }

                                    //East Face
                                    if (!isRight)
                                    {
                                        tm.vertices[tm.vTrack + 8] = new Vector3(TileSizeToUnits.x, tileHeight, TileSizeToUnits.y) + cellPosition;
                                        tm.vertices[tm.vTrack + 9] = new Vector3(TileSizeToUnits.x, 0, TileSizeToUnits.y) + cellPosition;
                                        tm.vertices[tm.vTrack + 10] = new Vector3(TileSizeToUnits.x, tileHeight, -TileSizeToUnits.y) + cellPosition;
                                        tm.vertices[tm.vTrack + 11] = new Vector3(TileSizeToUnits.x, 0, -TileSizeToUnits.y) + cellPosition;

                                        tm.uv[tm.uvTrack + 8] = new Vector2((-TileSizeToUnits.x + cellPosition.x) / 4 + textureOffset.x, (-TileSizeToUnits.y + cellPosition.z) / 4 + textureOffset.y);
                                        tm.uv[tm.uvTrack + 9] = new Vector2((-TileSizeToUnits.x + cellPosition.x) / 4 + textureOffset.x, (TileSizeToUnits.y + cellPosition.z) / 4 + textureOffset.y);
                                        tm.uv[tm.uvTrack + 10] = new Vector2((TileSizeToUnits.x + cellPosition.x) / 4 + textureOffset.x, (-TileSizeToUnits.y + cellPosition.z) / 4 + textureOffset.y);
                                        tm.uv[tm.uvTrack + 11] = new Vector2((TileSizeToUnits.x + cellPosition.x) / 4 + textureOffset.x, (TileSizeToUnits.y + cellPosition.z) / 4 + textureOffset.y);

                                        tm.triangles[tm.tTrack + 12] = tm.vTrack + 8;
                                        tm.triangles[tm.tTrack + 13] = tm.vTrack + 9;
                                        tm.triangles[tm.tTrack + 14] = tm.vTrack + 10;
                                        tm.triangles[tm.tTrack + 15] = tm.vTrack + 10;
                                        tm.triangles[tm.tTrack + 16] = tm.vTrack + 9;
                                        tm.triangles[tm.tTrack + 17] = tm.vTrack + 11;
                                    }

                                    //South Face
                                    if (!isDown)
                                    {
                                        tm.vertices[tm.vTrack + 12] = new Vector3(TileSizeToUnits.x, tileHeight, -TileSizeToUnits.y) + cellPosition;
                                        tm.vertices[tm.vTrack + 13] = new Vector3(TileSizeToUnits.x, 0, -TileSizeToUnits.y) + cellPosition;
                                        tm.vertices[tm.vTrack + 14] = new Vector3(-TileSizeToUnits.x, tileHeight, -TileSizeToUnits.y) + cellPosition;
                                        tm.vertices[tm.vTrack + 15] = new Vector3(-TileSizeToUnits.x, 0, -TileSizeToUnits.y) + cellPosition;

                                        tm.uv[tm.uvTrack + 12] = new Vector2((-TileSizeToUnits.x + cellPosition.x) / 4 + textureOffset.x, (-TileSizeToUnits.y + cellPosition.z) / 4 + textureOffset.y);
                                        tm.uv[tm.uvTrack + 13] = new Vector2((-TileSizeToUnits.x + cellPosition.x) / 4 + textureOffset.x, (TileSizeToUnits.y + cellPosition.z) / 4 + textureOffset.y);
                                        tm.uv[tm.uvTrack + 14] = new Vector2((TileSizeToUnits.x + cellPosition.x) / 4 + textureOffset.x, (-TileSizeToUnits.y + cellPosition.z) / 4 + textureOffset.y);
                                        tm.uv[tm.uvTrack + 15] = new Vector2((TileSizeToUnits.x + cellPosition.x) / 4 + textureOffset.x, (TileSizeToUnits.y + cellPosition.z) / 4 + textureOffset.y);

                                        tm.triangles[tm.tTrack + 18] = tm.vTrack + 12;
                                        tm.triangles[tm.tTrack + 19] = tm.vTrack + 13;
                                        tm.triangles[tm.tTrack + 20] = tm.vTrack + 14;
                                        tm.triangles[tm.tTrack + 21] = tm.vTrack + 14;
                                        tm.triangles[tm.tTrack + 22] = tm.vTrack + 13;
                                        tm.triangles[tm.tTrack + 23] = tm.vTrack + 15;
                                    }

                                    //West Face
                                    if (!isLeft)
                                    {
                                        tm.vertices[tm.vTrack + 16] = new Vector3(-TileSizeToUnits.x, tileHeight, -TileSizeToUnits.y) + cellPosition;
                                        tm.vertices[tm.vTrack + 17] = new Vector3(-TileSizeToUnits.x, 0, -TileSizeToUnits.y) + cellPosition;
                                        tm.vertices[tm.vTrack + 18] = new Vector3(-TileSizeToUnits.x, tileHeight, TileSizeToUnits.y) + cellPosition;
                                        tm.vertices[tm.vTrack + 19] = new Vector3(-TileSizeToUnits.x, 0, TileSizeToUnits.y) + cellPosition;

                                        tm.uv[tm.uvTrack + 16] = new Vector2((-TileSizeToUnits.x + cellPosition.x) / 4 + textureOffset.x, (-TileSizeToUnits.y + cellPosition.z) / 4 + textureOffset.y);
                                        tm.uv[tm.uvTrack + 17] = new Vector2((-TileSizeToUnits.x + cellPosition.x) / 4 + textureOffset.x, (TileSizeToUnits.y + cellPosition.z) / 4 + textureOffset.y);
                                        tm.uv[tm.uvTrack + 18] = new Vector2((TileSizeToUnits.x + cellPosition.x) / 4 + textureOffset.x, (-TileSizeToUnits.y + cellPosition.z) / 4 + textureOffset.y);
                                        tm.uv[tm.uvTrack + 19] = new Vector2((TileSizeToUnits.x + cellPosition.x) / 4 + textureOffset.x, (TileSizeToUnits.y + cellPosition.z) / 4 + textureOffset.y);

                                        tm.triangles[tm.tTrack + 24] = tm.vTrack + 16;
                                        tm.triangles[tm.tTrack + 25] = tm.vTrack + 17;
                                        tm.triangles[tm.tTrack + 26] = tm.vTrack + 18;
                                        tm.triangles[tm.tTrack + 27] = tm.vTrack + 18;
                                        tm.triangles[tm.tTrack + 28] = tm.vTrack + 17;
                                        tm.triangles[tm.tTrack + 29] = tm.vTrack + 19;
                                    }

                                    tm.vTrack += 20;
                                    tm.uvTrack += 20;
                                    tm.tTrack += 30;
                                    break;
                                }
   
                            }
                        }
                    }
                }

                //loop through tile mesh data and instantiate objects!!
                foreach (TileTypeMeshData tm in tileMeshes)
                {
                    if(tm.Tile == TileType.empty) {continue;}
                    GameObject newTileObject = new GameObject(tm.Tile.ToString() + "_" + chunkX + "_" + chunkY, typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider));
                    newTileObject.transform.SetParent(_levelParent);
                    newTileObject.transform.position = new Vector3(chunkX * TileSizeToUnits.x * _level.chunkSize, 0, chunkY * TileSizeToUnits.y * _level.chunkSize);
                    Mesh newMesh = new Mesh();
                    tm.UpdateMesh(ref newMesh);
                    newTileObject.GetComponent<MeshFilter>().mesh = newMesh;
                    newTileObject.GetComponent<MeshCollider>().sharedMesh = newMesh;
                    if(GetTile(tm.Tile).GetComponent<TileData>().Material != null)
                    {
                        Material matToUse = GetTile(tm.Tile).GetComponent<TileData>().Material;
                        newTileObject.GetComponent<MeshRenderer>().material = matToUse;
                    }
                    newTileObject.isStatic = true;

                    if(tm.Tile == TileType.water)
                    {
                        newTileObject.tag = "Water";
                    }
                }
            }
        }
        
        levelInstantiated = true;
        
    }

    private void AddNavMeshData(NavMeshData _navMeshData)
    {
        if (_navMeshData != null)
        {
            if (navMeshDataInstance.valid)
            {
                NavMesh.RemoveNavMeshData(navMeshDataInstance);
            }
            navMeshDataInstance = NavMesh.AddNavMeshData(_navMeshData);
        }
    }

    private void BuildNavMeshData(Transform _levelParent)
    {
        navMeshData = NavMeshBuilder.BuildNavMeshData(GetNavMeshSettings(navSettingsId), GetNavBuildSources(_levelParent), 
        //Navmesh Bounds - Will need to be updated later
        new Bounds(Vector3.zero, new Vector3(2500, 2500, 2500)),
        Vector3.zero, 
        Quaternion.identity);
        AddNavMeshData(navMeshData);
    }
 
    private List<NavMeshBuildSource> GetNavBuildSources(Transform _levelParent)
    {
        List<NavMeshBuildSource> navSources = new List<NavMeshBuildSource>();
        NavMeshBuilder.CollectSources(_levelParent, navigationLayers, NavMeshCollectGeometry.PhysicsColliders, 0, new List<NavMeshBuildMarkup>(), navSources);
        //NavMeshBuilder.CollectSources(GetBounds(), BuildMask, NavMeshCollectGeometry.PhysicsColliders, 0, new List<NavMeshBuildMarkup>(), navSources);
        Debug.LogFormat("Sources {0}", navSources.Count);
        return navSources;
    }
 
    private NavMeshBuildSettings GetNavMeshSettings(int _settingsID)
    {
        NavMeshBuildSettings settings = NavMesh.GetSettingsByID(_settingsID);
        return settings;
    }
    
    bool npcVendorSpawned = false;
    /// <summary>
    /// Loops through the level creating the object grid for each and every chunk.
    /// </summary>
    /// <param name="_level">The level asset being generated.</param>
    /// <param name="_levelParent">The object storing level objects.</param>
    void CreateObjectsFromChunks(LevelData _level, Transform _levelParent)
    {
        npcVendorSpawned = false;
        for (int chunkX = 0; chunkX < _level.chunkGrid.GetLength(0); chunkX++)
        {
            for (int chunkY = 0; chunkY < _level.chunkGrid.GetLength(1); chunkY++)
            {
                InstantiateObjectGrid(_level.chunkGrid[chunkX,chunkY], _level, _levelParent);
            }
        }
    }

    /// <summary>
    /// Loops through every tile in a chunk and instantiates an object/prop based the on the corresponding object map.
    /// </summary>
    /// <param name="_chunk">The chunk being looped through.</param>
    /// <param name="_level">The level asset being generated.</param>
    /// <param name="_levelParent">The object storing level objects.</param>
    void InstantiateObjectGrid(GridChunk _chunk, LevelData _level, Transform _levelParent)
    {

        for (int x = 0; x < _chunk.objGrid.GetLength(0); x++)
        {
            for (int y = 0; y < _chunk.objGrid.GetLength(1); y++)
            {
                //Where in the level are we instantiating. This takes into account other chunks.
                Vector2 levelCoords = new Vector2(x + (_chunk.position.x * _level.chunkSize), y + (_chunk.position.y * _level.chunkSize));

                //Where in the WORLD SPACE are we instantiating.
                Vector3 worldPosition = new Vector3(levelCoords.x * (TileSizeToUnits.x * 2), 0, levelCoords.y * (TileSizeToUnits.x * 2));
                
                //Get the correct object.
                Transform newObj = GetTile(_chunk.objGrid[x, y]);

                //If we failed to find an object, skip it.
                if(newObj == null){continue;}
                
                Transform gmToMake = Instantiate(newObj, worldPosition, Quaternion.identity);
                gmToMake.SetParent(_levelParent);
            }
        }
    }

    void CreateEnemies(LevelData _level, Transform _levelParent)
    {
        if(_level.difficultyData.GetEnemyCount() <= 0) {return;}
        int _spawnpointIndex = 0;
        List<Vector2Int> _enemySpawns = new List<Vector2Int>(_level.EnemySpawns);
        //Go through every enemy that is spawning
        for (int enemyID = 0; enemyID < _level.difficultyData.EnemyPresence.GetLength(0); enemyID++)
        {
            //If the number is -1 then it is required to spawn.
            if (_level.difficultyData.EnemyPresence[enemyID, 1] - 1 <= -1)
            {
                _spawnpointIndex = Random.Range(0, _enemySpawns.Count);
                if (_enemySpawns.Count < 1) { return; }
                Transform gmToMake = Instantiate(_level.difficultyData.GetEnemy(enemyID), GetEnemySpawnpoint(_enemySpawns[_spawnpointIndex]), Quaternion.identity);
                gmToMake.GetComponent<EnemyController>().enemyBehavior = _level.difficultyData.enemyAgression;
                gmToMake.SetParent(_levelParent);
                _enemySpawns.RemoveAt(_spawnpointIndex);
                continue;
            }
            //Check to see if that specified number has been spawned, if not then spawn another one and lower the count.
            for (int enemiesToSpawn = _level.difficultyData.EnemyPresence[enemyID,1]-1; enemiesToSpawn > 0; enemiesToSpawn--)
            {
                _spawnpointIndex = Random.Range(0, _enemySpawns.Count);
                if(_enemySpawns.Count < 1){return;}
                Transform gmToMake = Instantiate(_level.difficultyData.GetEnemy(enemyID), GetEnemySpawnpoint(_enemySpawns[_spawnpointIndex]), Quaternion.identity);
                gmToMake.GetComponent<EnemyController>().enemyBehavior = _level.difficultyData.enemyAgression;
                gmToMake.SetParent(_levelParent);
                _enemySpawns.RemoveAt(_spawnpointIndex);
            }
            
        }
        //ObjectiveManager.Objectives.ResetKillObjective();
    }

    Vector3 GetEnemySpawnpoint(LevelData _level)
    {
        return new Vector3(Random.Range(0, _level.chunkSize * _level.levelSize.x * (TileSizeToUnits.x*2)),1,Random.Range(0, _level.chunkSize * _level.levelSize.y * (TileSizeToUnits.y*2)));
    }

    Vector3 GetEnemySpawnpoint(LevelData _level, int _index)
    {
        return new Vector3(_level.EnemySpawns[_index].x, 1, _level.EnemySpawns[_index].y);
    }

    Vector3 GetEnemySpawnpoint(Vector2Int _pos)
    {
        return new Vector3(_pos.x, 1, _pos.y);
    }

    Transform GetTile(TileType _tile)
    {
        foreach (Transform tileAsset in terrainTiles)
        {
            if(tileAsset.GetComponent<TileData>().Tile == _tile)
            {
                return tileAsset;
            }
        }
        return null;
    }

    /// <summary>
    /// Return an object prefab transform based on the ObjectType.
    /// </summary>
    /// <param name="_obj">The type of object to check for.</param>
    /// <returns>Returns a transform component on a prefab.</returns>
    Transform GetTile(ObjectType _obj)
    {
        switch(_obj)
        {
            case ObjectType.empty:
                return null;
            case ObjectType.prop:
                return PropLoader.GrabProp(PropCategories.Container).transform;
            case ObjectType.tree:
                return PropLoader.GrabProp(PropCategories.Tree).transform;
            case ObjectType.grass:
                return PropLoader.GrabProp(PropCategories.Grass).transform;
            case ObjectType.tallgrass:
                return PropLoader.GrabProp(PropCategories.Grass).transform;
            case ObjectType.playerSpawn:
                return null;
            case ObjectType.enemySpawn:
                return null;
            case ObjectType.npcSpawn:
                if (!npcVendorSpawned)
                {
                    //If it is a vendor NPC of some sort, pull from a special list with special critera.
                    npcVendorSpawned = true;
                    return ProgressionManager.ProgressionInstance.GetUnlockedNPC();
                }else
                {
                    return null;
                }
            case ObjectType.campfire:
                return PropLoader.GrabProp(PropCategories.Generic, "Refugee Campfire").transform;
            case ObjectType.relay:
                return PropLoader.GrabProp(PropCategories.Objective, "Relay").transform;
            case ObjectType.cage:
                return PropLoader.GrabProp(PropCategories.Objective, "Refugee Cage").transform;
        }
        return null;
    }

    /// <summary>
    /// Returns the maximum X and Y coordinates used for the level.
    /// </summary>
    public Vector3 GetBoundries()
    {
        return new Vector3(wallRight.center.x, 5, wallTop.center.z);
    }

    /// <summary>
    /// Sets the wall boundries for the level, including the exit area.
    /// </summary>
    /// <param name="_level">The level asset to generate from.</param>
    void SetBoundries(LevelData _level)
    {
        wallTop.size = wallBottom.size = levelExit.size = new Vector3(((float)_level.Bounds.x + 2) * (TileSizeToUnits.x * 2),5,1);
        wallLeft.size = wallRight.size = new Vector3(1,5,((float)_level.Bounds.y + 2) * (TileSizeToUnits.y * 2));

        levelExit.center = new Vector3(((float)_level.Bounds.x) * (TileSizeToUnits.x * 2) * 0.5f, 2.5f, ((float)_level.Bounds.y) * (TileSizeToUnits.y * 2));

        wallTop.center = new Vector3(((float)_level.Bounds.x) * (TileSizeToUnits.x * 2) * 0.5f, 2.5f, ((float)_level.Bounds.y + 1) * (TileSizeToUnits.y * 2));
        wallBottom.center = new Vector3(((float)_level.Bounds.x) * (TileSizeToUnits.x * 2) * 0.5f, 2.5f, -1);

        wallRight.center = new Vector3(((float)_level.Bounds.x + 1) * (TileSizeToUnits.x * 2), 2.5f, ((float)_level.Bounds.y) * (TileSizeToUnits.y * 2) * 0.5f);
        wallLeft.center = new Vector3(-1, 2.5f, ((float)_level.Bounds.y) * (TileSizeToUnits.y * 2) * 0.5f);
    }

    void SetExitType()
    {
        LevelExitArea _exitArea = levelExit.transform.GetComponent<LevelExitArea>();
        if(ProgressionManager.ProgressionInstance.WorldTimeline[ProgressionManager.ProgressionInstance.CurrentProgress.y].areaTimeline[ProgressionManager.ProgressionInstance.CurrentProgress.x].sendToCrossroads)
        {
            _exitArea.exitType = ExitAreaType.splitPath;
        }else
        {
            _exitArea.exitType = ExitAreaType.encounter;
        }
    }

    /// <summary>
    /// Checks a TileType to see if it is a wall or not.
    /// </summary>
    /// <param name="_tile">The TileType to check.</param>
    /// <returns>Returns true if it is a wall type, or false</returns>
    public static bool IsWallTile(TileType _tile)
    {
        if(_tile.ToString().ToLower().Contains("wall"))
        {
            return true;
        }
        return false;
    }
}
