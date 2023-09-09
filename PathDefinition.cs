using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Path Data", menuName = "Levels/Path Definition", order = 1)]
public class PathDefinition : ScriptableObject
{
    [Header("Path Settings")]
    public TileType pathType = TileType.dirt;
    public int lifetime = -1;
    public float radius = 1;
    
    [Header("Path Direction Chances")]
    [Range(0.0f, 1.0f)]
    public float upChance = 1;
    [Range(0.0f, 1.0f)]
    public float downChance = 0;
    [Range(0.0f, 1.0f)]
    public float leftChance = 0.1f;
    [Range(0.0f, 1.0f)]
    public float rightChance = 0.1f;

    public Vector2 HorizontalChance
    {
        get{return new Vector2(leftChance, rightChance);}
    }
    public Vector2 VerticalChance
    {
        get{return new Vector2(upChance, downChance);}
    }
    public PathMaker ToPathMaker(LevelData _level, Vector2Int _startPos)
    {
        return new PathMaker(_level,
        lifetime,
        pathType,
        radius,
        _startPos,
        new Vector2Int(_level.Bounds.x,_level.Bounds.y), //Bounding box
        new Vector2(leftChance, rightChance),
        new Vector2(upChance, downChance));
    }
}

[System.Serializable]
public class PathsToMake
{
    public PathDefinition path;
    public int maxInstances;
}
