using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelGenerator : MonoBehaviour
{
    private static LevelGenerator levelGenInstance;
    public static LevelGenerator Generator { get { return levelGenInstance; } }

    [SerializeField]
    private GenerationDefinition defaultGenData;
    [SerializeField]
    private Transform generatedLevel;
    [SerializeField]
    private LevelData lvl;
    public LevelData Level
    {
        get{return lvl;}
    }
    [SerializeField]
    private List<PathMaker> activePathMakers = new List<PathMaker>();
    private int pathsMade = 0;
    [SerializeField]
    private List<WaterSystem> activeWaterSystems = new List<WaterSystem>();
    private int waterSystemsMade = 0;
    private bool levelIsGenerating = false;
    private bool waterGenerating = false;
    private bool pathGenerating = false;
    [SerializeField]
    private Text debugDisplay;
    [SerializeField] PresetLoader presetLoader;

    public LevelTransitioner transitioner;

    public bool IsGenerating
    {
        get{return levelIsGenerating;}
    }

    // Start is called before the first frame update
    void Awake()
    {
        if (levelGenInstance != null && levelGenInstance != this)
        {
            Destroy(this.gameObject);
        } else {
            levelGenInstance = this;
        }
        DontDestroyOnLoad(levelGenInstance);

        //lvl = CreateNewLevelData(defaultGenData);
        //debugDisplay.text = DebugLevelToPrint(lvl);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateNewLevelData(GenerationDefinition _genData, LevelDifficultyData _difficulty)
    {
        StartCoroutine(CreateNewLevelDataRoutine(_genData, _difficulty));
    }

    IEnumerator CreateNewLevelDataRoutine(GenerationDefinition _genData, LevelDifficultyData _difficulty) 
    {
        print("Generating Level");
        levelIsGenerating = true;
        bool tempGen = true;
        pathGenerating = true;
        waterGenerating = true;
        LevelData _newLevel = SetUpLevelData(_genData, _difficulty);
        //print("Set up finished");
        GenerateFields(_genData, _newLevel);
        GenerateWaterSystemsData(_genData, _newLevel);
        GenerateRoadData(_genData, _newLevel);
        while (tempGen) 
        {
            print("Generation Loop");
            if (waterGenerating || pathGenerating)
            {tempGen = true;}else{tempGen=false;}
            yield return null;
        }
        GenerateVegetation(_genData, _newLevel);
        GenerateSceneryChunks(_genData, _newLevel);
        GeneratePresetSceneryChunks(_genData, _newLevel);
        GenerateObjectiveSceneryChunks(_genData, _newLevel);
        print("Generation Finished");
        levelIsGenerating = false;
        lvl = _newLevel;
        if(debugDisplay != null) {debugDisplay.text = DebugLevelToPrint(lvl);}
    }

    LevelData SetUpLevelData(GenerationDefinition _genData, LevelDifficultyData _difficulty)
    {
        if(_genData == null)
        {
            Debug.LogWarning("World Generation Data Not Found! Reverting to Default");
            _genData = defaultGenData;
        }
        pathsMade = 0;

        LevelData newLevel = new LevelData();
        newLevel.levelSize = _genData.GetSize();
        newLevel.chunkSize = _genData.ChunkSize;
        print("New level is: " + newLevel.levelSize.x + " by " + newLevel.levelSize.y);
        newLevel.chunkGrid = new GridChunk[newLevel.levelSize.x, newLevel.levelSize.y];

        newLevel.difficultyData = _difficulty;

        //Outdated
        //newLevel.floorGrid = new TileType[newLevel.levelSize.x, newLevel.levelSize.y];
        //newLevel.objGrid = new ObjectType[newLevel.levelSize.x, newLevel.levelSize.y];

        //Iterate through every tile in the grid
        //print("Beginning grid loop");
        for (int x = 0; x < newLevel.Bounds.x; x++)
        {
            for (int y = 0; y < newLevel.Bounds.y; y++)
            {
                //Outdated
                //newLevel.floorGrid[x, y] = _genData.BaseTile;
                //newLevel.objGrid[x, y] = ObjectType.empty;
                //Will set all the chunks while looping through every tile.
                if(x < newLevel.chunkGrid.GetLength(0) && y < newLevel.chunkGrid.GetLength(1))
                {
                    newLevel.chunkGrid[x,y].position = new Vector2Int(x,y);
                    newLevel.chunkGrid[x,y].floorGrid = new TileType[newLevel.chunkSize,newLevel.chunkSize];
                    newLevel.chunkGrid[x,y].objGrid = new ObjectType[newLevel.chunkSize,newLevel.chunkSize];
                    newLevel.chunkGrid[x,y].tileQuantity = new Dictionary<TileType, int>();
                }
                newLevel.SetTile(_genData.BaseTile, new Vector2Int(x,y));
                newLevel.SetObject(ObjectType.empty, new Vector2Int(x,y));

                newLevel.UpdateTileQuantity(_genData.BaseTile, 1);
            }
        }
        newLevel.DebugPrintQuantities();
        if(debugDisplay != null) {debugDisplay.text = DebugLevelToPrint(newLevel);}

        return newLevel;
    }

    void GenerateWaterSystemsData(GenerationDefinition _genData, LevelData _level)
    {
        List<WaterSystem> waterSystemsToMake = _genData.GetWaterSystems(_level);

        //If there is no water, skip this step
        if(waterSystemsToMake.Count < 1){waterGenerating = false;return;}

        for (int i = 0; i < waterSystemsToMake.Count; i++)
        {
            if(i == 0 && _genData.WaterOffset.x > 0 && _genData.WaterOffset.y > 0)
            {//If the gen data has an assigned starting position, use it for the very first watersystem
                waterSystemsToMake[i].BeginWaterFlow(PercentToWorldCoords(_genData.WaterOffset, _level));
            }else
            {
                waterSystemsToMake[i].BeginWaterFlow();
            }

            StartCoroutine(DoWaterFlowUpdate(waterSystemsToMake[i], _level));
        }
        
    }

    void GenerateRoadData(GenerationDefinition _genData, LevelData _level)
    {
        PathMaker walker = new PathMaker(_level, 
        _genData.MainPath.lifetime, //Lifetime
        _genData.MainPath.pathType, //Pathtype
        _genData.MainPath.radius, //Radius
        _genData.RoadOffsetToCoords(_level.Bounds.x,_level.Bounds.y), //Road offset aka start position
        new Vector2Int(_level.Bounds.x,_level.Bounds.y), //Bounding box
        _genData.MainPath.HorizontalChance, //Chance to go left or right
        _genData.MainPath.VerticalChance); //Chance to go up or down

        _level.playerSpawnpoint = _genData.RoadOffsetToCoords(_level.Bounds.x,_level.Bounds.y);

        walker.TogglePathSpawning();

        activePathMakers.Add(walker);

        StartCoroutine(DoPathWalking(_genData, _level));
    }

    /// <summary>
    /// Loops through all definied fields and adds them to the level based on a perlin noise function.
    /// </summary>
    /// <param name="_genData">Generation parameters</param>
    /// <param name="_level">The level data</param>
    void GenerateFields(GenerationDefinition _genData, LevelData _level)
    {
        for (int x = 0; x < _level.Bounds.x; x++)
        {
            for (int y = 0; y < _level.Bounds.y; y++)
            {

                foreach (FieldDef field in _genData.FieldDefs)
                {
                    float perlinValue = Mathf.Clamp01(Mathf.PerlinNoise(x * field.scale, y * field.scale));
                    if (perlinValue >= field.frequency && perlinValue <= field.maxFrequency && y >= field.minPositionY)
                    {
                        _level.SetTile(field.tile, new Vector2Int(x, y));
                    }
                }
            }
        }
    }

    void GenerateVegetation(GenerationDefinition _genData, LevelData _level)
    {
        for (int x = 0; x < _level.Bounds.x; x++)
        {
            for (int y = 0; y < _level.Bounds.y; y++)
            {
                
                foreach (VegetationDef veg in _genData.VegDef)
                {
                    if(veg.tileRequirement == TileType.empty || veg.tileRequirement == _level.GetTile(new Vector2Int(x,y)))
                    {
                        float perlinValue = Mathf.Clamp01(Mathf.PerlinNoise(x * 0.1f,y * 0.1f));
                        //print("Perlin Value is: " + perlinValue);
                        if(perlinValue >= veg.frequency && perlinValue <= veg.maxFrequency && y >= veg.minPositionY && Random.Range(0.0f,0.99f) < veg.chance)
                        {
                            _level.SetObject(veg.objectType, new Vector2Int(x,y));
                        }
                    }
                    
                }
            }
        }
    }

    void GenerateSceneryChunks(GenerationDefinition _genData, LevelData _level)
    {
        //TO-DO Generate random scenery chunks.
    }

    void GeneratePresetSceneryChunks(GenerationDefinition _genData, LevelData _level)
    {
        foreach (POISpecification preset in _genData.SceneryPresets)
        {
            if(preset.chance >= Random.Range(0.0f,0.99f))
            {
                Vector2Int presetPlacementOffset = new Vector2Int(0,0);
                presetPlacementOffset.x = Mathf.RoundToInt((float)_level.Bounds.x * Random.Range(preset.positionMin.x, preset.positionMax.x));
                presetPlacementOffset.y = Mathf.RoundToInt((float)_level.Bounds.y * Random.Range(preset.positionMin.y, preset.positionMax.y));
                Debug.Log("Preset placement is now at " + presetPlacementOffset);
                if(preset.specificName != "")
                {
                    //If it returned an actual preset of said name, then...
                    if(presetLoader.GetSceneryByName(preset.specificName, out SceneryChunk scenery))
                    {
                        Debug.Log("Found Scenery piece of the name " + preset.specificName);
                        //Loop through the 2D array of the scenery chunk
                        GenerateSceneryGrid(_level, presetPlacementOffset, scenery);
                    }
                }else
                {
                    string _themesList = "";
                    foreach (string _theme in preset.themes)
                    {
                        _themesList += _theme + ", ";
                    }

                    //If it returned an actual preset of said name, then...
                    if(presetLoader.GetSceneryByTheme(preset.specificName, out SceneryChunk scenery))
                    {
                        Debug.Log("Found Scenery piece of the name " + preset.specificName);
                        //Loop through the 2D array of the scenery chunk
                        GenerateSceneryGrid(_level, presetPlacementOffset, scenery);
                    }
                }
            }
        }
    }

    void GenerateObjectiveSceneryChunks(GenerationDefinition _genData, LevelData _level)
    {
        foreach (ObjectiveLayout _objLayout in _level.difficultyData.GetObjectivePresets())
        {
            for (int i = 0; i < _objLayout.spawnCount; i++)
            {
                GenerateSceneryGrid(_level, _objLayout.GetRandomPosition(_level), _objLayout.GetSceneryChunk());
            }
        }
    }

    void GenerateSceneryGrid(LevelData _level, Vector2Int _offset, SceneryChunk _scenery)
    {
        for (int x = 0; x < _scenery.chunkWidth; x++)
        {
            for (int y = 0; y < _scenery.chunkHeight; y++)
            {
                if(_scenery.layer.tileGrid[x,y] != TileType.empty)
                {
                    //print("Set tile at " + (_offset.x + x) + ", " + (_offset.y + y) + " to " + _scenery.layer.tileGrid[x,y].ToString());
                    _level.SetTile(_scenery.layer.tileGrid[x,y], new Vector2Int((_offset.x + x) - Mathf.RoundToInt(_scenery.chunkWidth * 0.5f), (_offset.y + y) - Mathf.RoundToInt(_scenery.chunkHeight * 0.5f)));
                }
                _level.SetObject(_scenery.layer.objectGrid[x,y], new Vector2Int((_offset.x + x) - Mathf.RoundToInt(_scenery.chunkWidth * 0.5f), (_offset.y + y) - Mathf.RoundToInt(_scenery.chunkHeight * 0.5f)));
            }
        }
    }

    public Vector2Int PercentToWorldCoords(Vector2 _percentCoords, LevelData _level)
    {
        float x = (float)_level.Bounds.x * _percentCoords.x;
        float y = (float)_level.Bounds.y * _percentCoords.y;

        return new Vector2Int(Mathf.RoundToInt(x), Mathf.RoundToInt(y));
    }

    public string DebugLevelToPrint(LevelData _lvl)
    {
        string baseLayerInText = "";
        string objLayerInText = "";
        for (int x = 0; x <= _lvl.Bounds.x; x++)
        {
            for (int y = 0; y <= _lvl.Bounds.y; y++)
            {
                baseLayerInText += (int)_lvl.GetTile(new Vector2Int(x,y));
                baseLayerInText += "\t";
                objLayerInText += (int)_lvl.GetObject(new Vector2Int(x,y));
                objLayerInText += "\t";
            }
            baseLayerInText += "\n";
            objLayerInText += "\n";
        }
        print(baseLayerInText);
        print(objLayerInText);
        return baseLayerInText;
    }

    IEnumerator DoWaterFlowUpdate(WaterSystem _waterSystem, LevelData _level) 
    {
        while (_waterSystem.aliveWaterFlows.Count > 0) 
        {
            for (int steps = 0; steps < 30; steps++)
            {//Speed up generation by having higher step counts
                _waterSystem.DoWaterFlow();
            }

            yield return null;
        }
        //if(debugDisplay != null) {debugDisplay.text = DebugLevelToPrint(lvl);}
        waterGenerating = false;
    }
    IEnumerator DoPathWalking(GenerationDefinition _genData, LevelData _level) 
    {
        while (activePathMakers.Count > 0) 
        {
            for (int i = 0; i < activePathMakers.Count; i++)
            {
                for (int steps = 0; steps < 30; steps++)
                {//Speed up generation by having higher step counts
                    activePathMakers[i].Walk();
                    if(!activePathMakers[i].isAlive){break;}

                    if(pathsMade < _genData.MaxPaths && activePathMakers[i].RedeemNewPath())
                    {
                        //Get a new path to make, and make it
                        pathsMade++;
                        PathDefinition _pathDef = _genData.GetSubPath();
                        if(_pathDef != null) //if there are subpaths
                        {
                            //Debug.LogWarning("Making a new path");
                            activePathMakers.Add(_pathDef.ToPathMaker(_level, activePathMakers[i].position));
                        }
                    }
                }

                

                if(!activePathMakers[i].isAlive)
                {
                    activePathMakers.Remove(activePathMakers[i]);
                }
            }
            yield return null;
        }
        //if(debugDisplay != null) {debugDisplay.text = DebugLevelToPrint(lvl);}
        pathGenerating = false;
    }
}




