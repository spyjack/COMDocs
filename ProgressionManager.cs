using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionManager : MonoBehaviour
{
    private static ProgressionManager instance;
    public static ProgressionManager ProgressionInstance { get { return instance; } }
    [Header("World Timeline")]
    [SerializeField] private WorldArea[] worldTimeline;
    [SerializeField] private Vector2Int playerProgress = Vector2Int.zero;
    [SerializeField] private NPCTracker unlockedNPCs;

    [SerializeField] private float reinforcementLevel;
    [SerializeField] private int travelTime;

    [SerializeField] bool hasWon = false;

    /// <summary>
    /// How far into the game the player(s) have progressed. X is area sub progression, Y is area progression.
    /// </summary>
    public Vector2Int CurrentProgress {get{return playerProgress;} set{playerProgress = value;}}
    /// <summary>
    /// Returns the world timeline array.
    /// </summary>
    public WorldArea[] WorldTimeline {get{return worldTimeline;}}
    /// <summary>
    /// Return the reinforcement multiplier.
    /// </summary>
    public float ReinforcementLevel { get { return reinforcementLevel; } }
    /// <summary>
    /// Return the amount of time traveled compared to the time before reinforcements
    /// </summary>
    public int ReinforcementTime { get { return worldTimeline[playerProgress.y].timeBeforeReinforcements - travelTime; } }
    // Start is called before the first frame update
    void Awake()
    {
        if (ProgressionInstance != null && ProgressionInstance != this)
        {
            Destroy(this.gameObject);
        } else {
            instance = this;
        }
        DontDestroyOnLoad(ProgressionInstance);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BeginGame()
    {
        ResetProgress();
        LevelGenerator.Generator.transitioner.SetFadeOpacity(1);
        Destroy(GameObject.Find("Game Level"));
        if (PlayerManager.Main.completedTutorial)
        {
            LevelGenerator.Generator.transitioner.StartTransition(new Vector3(-173, 1, 18), 0);
            NPCManager.instance.SetTutorialTalkIndicatorActive(false);

        }
        else
        {
           LevelGenerator.Generator.transitioner.StartTransition(new Vector3(-195.15f, 1f, 15.5f), 3);
            NPCManager.instance.SetTutorialTalkIndicatorActive(true);
        }
        //playerProgress = Vector2Int.left;
        PlayerManager.Main.MakePlayersMove(true);
    }

    /*public void BeginLevel()
    {   
        //LevelGenerator.Generator.transitioner.StartTransition(new Vector3(-173, 1, 18));
        LevelGenerator.Generator.transitioner.StartTransition();
    }*/

    /// <summary>
    /// Progress the player through the timeline.
    /// </summary>
    public void Progress()
    {
        if(playerProgress.x + 1 < worldTimeline[playerProgress.y].areaTimeline.Length)
        {
            //Increase local area progress, aka playerProgress.x
            playerProgress += Vector2Int.right;
            if (reinforcementLevel > 1) { reinforcementLevel++; }
            Debug.Log("Player has reached " + worldTimeline[playerProgress.y].areaTimeline[playerProgress.x].sectionName);
        }else if(playerProgress.y + 1 < worldTimeline.Length)
        {
            playerProgress = new Vector2Int(0, playerProgress.y + 1);
            reinforcementLevel = 0;
            travelTime = 0;
        }else
        {
            Debug.LogWarning("Player has surpassed the available levels!");
            UIManager.UIInstance.canPause = false;
            hasWon = true;
        }

        unlockedNPCs.UpdateSpawnableNPCS();
    }

    /// <summary>
    /// Increases the area's time traveled, and adds to the reinforcement level if it is over the limit.
    /// </summary>
    /// <param name="_tt">Integer of Travel Time representing how long the path is.</param>
    public void AddTravelTime(int _tt)
    {
        travelTime += _tt;
        if(travelTime > worldTimeline[playerProgress.y].timeBeforeReinforcements)
        {
            reinforcementLevel += 0.5f;
        }
    }

    public bool HasWon()
    {
        return hasWon;
    }

    public void ResetProgress()
    {
        playerProgress = Vector2Int.zero;
        hasWon = false;
        unlockedNPCs.UpdateSpawnableNPCS();
        travelTime = 0;
        reinforcementLevel = 1;
    }

    public GenerationDefinition GetGenerationDefinition()
    {
        return worldTimeline[playerProgress.y].areaTimeline[playerProgress.x].GetGenerationDefinition();
    }

    public void GetGenerationDefinition(out GenerationDefinition _choiceA, out GenerationDefinition _choiceB)
    {
        _choiceA = worldTimeline[playerProgress.y].areaTimeline[playerProgress.x].GetGenerationDefinition();
        _choiceB = worldTimeline[playerProgress.y].areaTimeline[playerProgress.x].GetGenerationDefinition();
    }

    /// <summary>
    /// Returns a random level difficulty dataset from a difficulty definition.
    /// </summary>
    public LevelDifficultyData GetLevelDifficultyData()
    {
        return worldTimeline[playerProgress.y].areaTimeline[playerProgress.x].GetDifficultyData();
    }

    public void GetLevelDifficultyData(out LevelDifficultyData _choiceA, out LevelDifficultyData _choiceB)
    {
        _choiceA = worldTimeline[playerProgress.y].areaTimeline[playerProgress.x].GetDifficultyData();
        _choiceB = worldTimeline[playerProgress.y].areaTimeline[playerProgress.x].GetDifficultyData();
    }

    public Transform GetUnlockedNPC()
    {
        return unlockedNPCs.GetRandomNPC();
    }

    public void RemoveNPC(string _npcToRemove)
    {
        unlockedNPCs.RemoveNPC(_npcToRemove);
    }

    /// <summary>
    /// Get the lighting intensity based on the lighting curve and a time.
    /// </summary>
    /// <param name="_lightingCurve">The lighting intensity curve.</param>
    /// <param name="_time">The time to check.</param>
    /// <returns>Returns an inensity for use on lights.</returns>
    public static float GetTimeLightscale(AnimationCurve _lightingCurve, int _time)
    {
        float intensity = _lightingCurve.Evaluate((float)_time / 24);
        return intensity;
    }

    public float GetTimeLightscale()
    {
        float intensity = worldTimeline[playerProgress.y].lightingCurve.Evaluate((float)travelTime / 24);
        return intensity;
    }
}

[System.Serializable]
public class WorldArea
{
    public string areaName;
    public AnimationCurve lightingCurve;
    public int timeBeforeReinforcements = 100;
    //public GenerationDefinition crossroadsDef;
    public SectionChoices[] areaTimeline;
}

[System.Serializable]
public class SectionChoices
{
    public string sectionName;
    public bool sendToCrossroads = false;
    public int musicTrackIndex = -1;
    public GenerationDefinition[] generationChoices;
    public DifficultyDefinition[] difficultyChoices;

    int lastChosenGenDef = -1;
    int lastChosenDifDef = -1;

    public GenerationDefinition GetGenerationDefinition()
    {
        if(generationChoices.Length == 1){return generationChoices[0];}

        int randIndex = Random.Range(0,generationChoices.Length);
        while(randIndex == lastChosenGenDef)
        {
            randIndex = Random.Range(0,generationChoices.Length);
        }

        lastChosenGenDef = randIndex;
        return generationChoices[randIndex];
    }

    /// <summary>
    /// Returns a random level difficulty dataset from a difficulty definition.
    /// </summary>
    public LevelDifficultyData GetDifficultyData()
    {
        if(difficultyChoices.Length == 1){return difficultyChoices[0].ToDifficultyData();}

        int randIndex = Random.Range(0,difficultyChoices.Length);
        while(randIndex == lastChosenDifDef)
        {
            randIndex = Random.Range(0,difficultyChoices.Length);
        }

        lastChosenDifDef = randIndex;
        return difficultyChoices[randIndex].ToDifficultyData();
    }

    /// <summary>
    /// Returns a random difficulty definition.
    /// </summary>
    public DifficultyDefinition GetDifficultyDefinition()
    {
        if(difficultyChoices.Length == 1){return difficultyChoices[0];}
        return difficultyChoices[Random.Range(0,difficultyChoices.Length)];
    }
}

[System.Serializable]
public class NPCTracker
{
    public List<Transform> spawnableNPCs;
    public List<NPCInstance> npcCache;

    int lastSpawnedNPC = -1;

    public void UpdateSpawnableNPCS()
    {
        for (int i = 0; i < npcCache.Count; i++)
        {
            if(npcCache[i].minimumProgress.x >= ProgressionManager.ProgressionInstance.CurrentProgress.x && npcCache[i].minimumProgress.y >= ProgressionManager.ProgressionInstance.CurrentProgress.y)
            {
                spawnableNPCs.Add(npcCache[i].npcPrefab);
                npcCache.RemoveAt(i);
                //Mathf.Clamp(i, 0, i--);
            }
        }
    }

    public void RemoveNPC(string _name)
    {
        for (int i = 0; i < spawnableNPCs.Count; i++)
        {
            if(spawnableNPCs[i].name == _name)
            {
                spawnableNPCs.RemoveAt(i);
                break;
            }
        }
    }

    public Transform GetRandomNPC()
    {
        int newNPC = Random.Range(0, spawnableNPCs.Count);
        while(newNPC == lastSpawnedNPC && spawnableNPCs.Count > 1)
        {
            newNPC = Random.Range(0, spawnableNPCs.Count);
        }

        lastSpawnedNPC = newNPC;
        return spawnableNPCs[newNPC];
    }
}

[System.Serializable]
public class NPCInstance
{
    public Transform npcPrefab;
    public Vector2Int minimumProgress;
}
