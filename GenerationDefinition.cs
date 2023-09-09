using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Gen Data", menuName = "Levels/Generation Definition", order = 1)]
public class GenerationDefinition : ScriptableObject
{
    [SerializeField]string locationTheme;
    [Header("Core Settings")]
    [SerializeField]
    private TileType baseTile = TileType.grass;
    [SerializeField]
    private int chunkSize = 16;
    [SerializeField]
    private Vector2Int minSize = new Vector2Int(10,10);
    [SerializeField]
    private Vector2Int maxSize = new Vector2Int(10,10);

    [Header("Field Settings")]
    [SerializeField] private FieldDef[] fields;

    [Header("Road Settings")]
    [SerializeField]
    private Vector2 roadStartOffset = Vector2.zero;
    [SerializeField]
    private int maxPaths = 1;
    [SerializeField]
    private PathsToMake[] paths;

    [Header("Water Settings")]
    [SerializeField] Vector2 waterStartOffset = Vector2.zero;
    [SerializeField]
    private int randomizeWater = -1; //<0 for false, anything more is how many to take from the water systems list.
    [SerializeField]
    private WaterDefinition[] waterSystems;

    [Header("Environment Settings")]
    [SerializeField] VegetationDef[] vegetations;

    [Header("POI Settings")]
    [SerializeField]
    private List<POISpecification> specifiedBuildings;

    public TileType BaseTile
    {
        get{return baseTile;}
    }
    public int ChunkSize
    {
        get{return chunkSize;}
    }
    public Vector2 RoadOffset
    {
        get{return roadStartOffset;}
    }

    public Vector2 WaterOffset
    {
        get{return waterStartOffset;}
    }

    public int MaxPaths
    {
        get{return maxPaths;}
    }

    public PathDefinition MainPath
    {
        get{return paths[0].path;}
    }

    public FieldDef[] FieldDefs
    {
        get { return fields; }
    }

    public VegetationDef[] VegDef
    {
        get{return vegetations;}
    }

    public POISpecification[] SceneryPresets
    {
        get{return specifiedBuildings.ToArray();}
    }

    public string Theme
    {
        get{return locationTheme;}
    }

    public List<WaterSystem> GetWaterSystems(LevelData _level)
    {
        List<WaterSystem> returnList = new List<WaterSystem>();
        if(randomizeWater < 0)
        {
            foreach (WaterDefinition _def in waterSystems)
            {
                returnList.Add(_def.ToWaterSystem(_level));
            }
        }else
        {
            for (int i = 0; i < randomizeWater; i++)
            {//Currently just randomly chooses the amount specified by randomizeWater, eventually rewrite to take chance into account
                returnList.Add(waterSystems[Random.Range(0,waterSystems.Length)].ToWaterSystem(_level));
            }
        }
        return returnList;
    }

    public PathDefinition GetSubPath()
    {
        if(paths.Length <= 1) {return null;}
        PathDefinition pathDef = paths[Random.Range(1, paths.Length-1)].path;
        return pathDef;
    }

    public Vector2Int GetSize()
    {
        Vector2Int finalSize = Vector2Int.one;
        finalSize.x = Random.Range(minSize.x, maxSize.x+1);
        finalSize.y = Random.Range(minSize.y, maxSize.y+1);
        return finalSize;
    }

    public Vector2Int GetGridSize()
    {
        Vector2Int finalSize = Vector2Int.one;
        finalSize.x = Random.Range(minSize.x, maxSize.x);
        finalSize.y = Random.Range(minSize.y, maxSize.y);
        return finalSize * chunkSize;
    }

    public Vector2Int RoadOffsetToCoords(float _xMax, float _yMax)
    {
        Debug.Log("Starting X: " + Mathf.RoundToInt(_xMax*roadStartOffset.x));
        return new Vector2Int(Mathf.RoundToInt(_xMax*roadStartOffset.x), Mathf.RoundToInt(_yMax*roadStartOffset.y));
    }
}

[System.Serializable]
public struct FieldDef
{
    public string id;
    public TileType tile;
    [Range(0.0f, 1.0f)]
    public float frequency;
    [Range(0.0f, 1.0f)]
    public float maxFrequency;
    [Range(0.0f, 100.0f)]
    public float minPositionY;
    [Range(0.0f, 10.0f)]
    public float scale;
}

[System.Serializable]
public struct POISpecification
{
    public string specificName;
    public string[] themes;
    public Vector2 positionMin;
    public Vector2 positionMax;
    [Range(0.0f, 1.0f)]
    public float chance;
}

[System.Serializable]
public struct VegetationDef
{
    public ObjectType objectType;
    public TileType tileRequirement;
    [Range(0.0f, 1.0f)]
    public float frequency;
    [Range(0.0f, 1.0f)]
    public float maxFrequency;
    [Range(0.0f, 100.0f)]
    public float minPositionY;
    [Range(0.0f, 1.0f)]
    public float chance;
}
