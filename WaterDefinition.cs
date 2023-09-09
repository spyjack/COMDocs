using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Water Data", menuName = "Levels/Water Definition", order = 1)]
public class WaterDefinition : ScriptableObject
{
    [SerializeField][Range(0.0f, 1.0f)]
    float spawnChance = 1;
    [SerializeField]
    WaterFlow[] possibleWaterFlows;
    [SerializeField]
    Vector2 startMinPosition = Vector2.zero;
    [SerializeField]
    Vector2 startMaxPosition = Vector2.zero;
    [SerializeField]
    int maxWaterFlows = 3; //Maximun amount of water flows that can be made.

    public WaterSystem ToWaterSystem(LevelData _level)
    {
        Vector2Int _pos = new Vector2Int(Mathf.RoundToInt((float)_level.Bounds.x * (Random.Range(startMinPosition.x, startMaxPosition.x))), Mathf.RoundToInt((float)_level.Bounds.y * (Random.Range(startMinPosition.y, startMaxPosition.y))));
        WaterSystem newSystem = new WaterSystem(_level, possibleWaterFlows, maxWaterFlows, _pos);
        return newSystem;
    }
}
