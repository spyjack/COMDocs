using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelDifficultyData
{
    [SerializeField] int enemyCount;

    public BehaviorType enemyAgression = BehaviorType.Wander;

    public int travelTime = 1;
    
    [SerializeField] List<EnemySpawnPB> enemyPrefabs;

    [SerializeField] List<Objective> extraObjectives;

    int[,] enemyPresence;

    public int[,] EnemyPresence
    {
        get{return enemyPresence;}
    }

    public Transform GetEnemy(int _ID)
    {
        Mathf.Clamp(_ID, 0, enemyPresence.GetLength(0)-1);
        return enemyPrefabs[_ID].EnemyPrefab;
    }
    public EnemySpawnPB GetEnemyInfo(int _ID)
    {
        Mathf.Clamp(_ID, 0, enemyPrefabs.Count - 1);
        return enemyPrefabs[_ID];
    }

    public LevelDifficultyData(int _count, List<EnemySpawnPB> _enemyPrefabs)
    {
        enemyCount = _count;
        enemyPrefabs = _enemyPrefabs;

        enemyPresence = CreateEnemyPresence(1, _count, _enemyPrefabs);
    }

    public LevelDifficultyData(int _count, List<EnemySpawnPB> _enemyPrefabs, List<Objective> _objectives)
    {
        enemyCount = _count;
        enemyPrefabs = _enemyPrefabs;
        extraObjectives = _objectives;

        enemyPresence = CreateEnemyPresence(1, _count, _enemyPrefabs);
    }

    public int GetEnemyCount()
    {
        return Mathf.RoundToInt(enemyCount * ProgressionManager.ProgressionInstance.ReinforcementLevel);
    }

    /// <summary>
    /// Adds all extra objectives to the main ObjectiveManager instance.
    /// </summary>
    public void AddObjectivesToMain()
    {
        foreach (Objective item in extraObjectives)
        {
            ObjectiveManager.AddObjective(item);
        }
    }

    public void AddTravelTime(int _travelTime)
    {
        ProgressionManager.ProgressionInstance.AddTravelTime(_travelTime);
    }

    /// <summary>
    /// Get the array of ObjectiveLayouts for a specified objective. Defaults to 0.
    /// </summary>
    /// <param name="_objectiveIndex">The index of extra objectives. Defaults to 0.</param>
    /// <returns>Returns an array reference or an empty array of ObjectiveLayouts.</returns>
    public ObjectiveLayout[] GetObjectivePresets(int _objectiveIndex = 0)
    {
        if (extraObjectives.Count <= 0) { return new ObjectiveLayout[0]; }
        return extraObjectives[_objectiveIndex].ObjectiveLayouts;
    }

    int[,] CreateEnemyPresence(int _minCount, int _count, List<EnemySpawnPB> _enemyPrefabs)
    {
        int[,] _newPresence = new int[_enemyPrefabs.Count , 2];
        int _reinforcedCount = Mathf.RoundToInt((float)_count * ProgressionManager.ProgressionInstance.ReinforcementLevel);

        //Run through and set all IDs
        for (int prefabID = 0; prefabID < _enemyPrefabs.Count; prefabID++)
        {
            _newPresence[prefabID,0] = prefabID;
            _newPresence[prefabID,1] = _minCount;
        }

        while (_reinforcedCount > 0)
        {
            _reinforcedCount--;
            //Choose a random ID in the list of enemies
            int i = Random.Range(0, _newPresence.GetLength(0)-1);
            //If the enemy ID is spawn required and hasn't been set, set it to -1 otherwise;
            if(GetEnemyInfo(i).IsSpawnRequired && _newPresence[i, 1] > -1)
            {
                _newPresence[i, 1] = -1;
            }
            else
            {
                _newPresence[i, 1]++;
            }
        }

        return _newPresence;
    }
}

[System.Serializable]
public class EnemySpawnPB
{
    [SerializeField] Transform enemyPrefab = null;
    public Transform EnemyPrefab { get { return enemyPrefab; } }

    [SerializeField] bool isSpwnReq = false;
    public bool IsSpawnRequired { get { return isSpwnReq; } set { isSpwnReq = value; } }

    [SerializeField] int wgt = 1;
    public int Weight { get { return wgt; } }

    /// <summary>
    /// Creates a new Enemy Spawn Prefab object to be used for enemy instantation.
    /// </summary>
    /// <param name="_prefab">Enemy prefab transform to be used.</param>
    /// <param name="_weight">Enemy prefab weight in spawn chance.</param>
    /// <param name="_isSpawnRequired">Force enemy to spawn if true, like a boss.</param>
    public EnemySpawnPB(Transform _prefab, int _weight, bool _isSpawnRequired)
    {
        enemyPrefab = _prefab;
        isSpwnReq = _isSpawnRequired;
        wgt = _weight;
    }

    /// <summary>
    /// Creates a new Enemy Spawn Prefab object to be used for enemy instantation.
    /// </summary>
    /// <param name="_prefab">Enemy prefab transform to be used.</param>
    /// <param name="_isSpawnRequired">Force enemy to spawn if true, like a boss.</param>
    public EnemySpawnPB(Transform _prefab, bool _isSpawnRequired)
    {
        enemyPrefab = _prefab;
        isSpwnReq = _isSpawnRequired;
        wgt = 1;
    }

    /// <summary>
    /// Creates a new Enemy Spawn Prefab object to be used for enemy instantation.
    /// </summary>
    /// <param name="_prefab">Enemy prefab transform to be used.</param>
    public EnemySpawnPB(Transform _prefab)
    {
        enemyPrefab = _prefab;
        isSpwnReq = false;
        wgt = 1;
    }

    /// <summary>
    /// Sets the weight of the enemy spawn prefab.
    /// </summary>
    /// <param name="_weight">The new desired weight.</param>
    public void SetWeight(int _weight)
    {
        wgt = _weight;
    }
}
