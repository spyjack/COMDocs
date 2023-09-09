using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Difficulty Definition", menuName = "Levels/Difficulty Definition", order = 1)]
public class DifficultyDefinition : ScriptableObject
{
    [SerializeField] int enemyCountMin;
    [SerializeField] int enemyCountMax;

    [SerializeField] BehaviorType aggressionLevel = BehaviorType.Wander;
    [SerializeField] int travelTime = 1;

    /// <summary>
    /// Returns a random value between the minimum and maximum enemy count values.
    /// </summary>
    public int EnemiesCount
    {
        get{return Random.Range(enemyCountMin, enemyCountMax+1);}
    }
    
    [SerializeField] EnemySpawnPB[] enemyPrefabs;

    /// <summary>
    /// List of all enemies to spawn on this difficulty.
    /// </summary>
    public EnemySpawnPB[] EnemiesList
    {
        get{return enemyPrefabs;}
    }

    [SerializeField]  Objective[] extraObjectives;

    /// <summary>
    /// Array of all extra objectives.
    /// </summary>
    public Objective[] ExtraObjectives
    {
        get{return extraObjectives;}
    }

    /// <summary>
    /// Turns a difficulty deffinition to a DifficultyData object so it can be modified at runtime.
    /// </summary>
    /// <returns>Returns a LevelDifficultyData based upon the DifficultyData information.</returns>
    public LevelDifficultyData ToDifficultyData()
    {
        LevelDifficultyData _difData = new LevelDifficultyData(EnemiesCount, new List<EnemySpawnPB>(enemyPrefabs), new List<Objective>(extraObjectives));
        _difData.enemyAgression = aggressionLevel;
        _difData.travelTime = travelTime;
        return _difData;
    }
}
