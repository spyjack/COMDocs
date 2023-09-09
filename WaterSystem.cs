using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaterSystem
{
    public string name;
    public Vector2Int startPosition = Vector2Int.zero;
    //sub systems to be made + chance
    //max subsystems
    //list of active sub systems
    [SerializeField]
    WaterFlow[] possibleWaterFlows;
    [SerializeField]
    int maxWaterFlows = 3; //Maximun amount of water flows that can be made.
    [SerializeField]
    public List<WaterFlow> aliveWaterFlows = new List<WaterFlow>(); //List of all currently alive water flows.
    [SerializeField]
    private LevelData level;
    public int Max
    {
        get{return maxWaterFlows;}
    }
    public bool IsAtMax
    {
        get{return aliveWaterFlows.Count >= maxWaterFlows;}
    }
    public WaterSystem(LevelData _level, WaterFlow[] _waterFlows, int _maxFlows, Vector2Int _startPos)
    {
        //Constructor
        level = _level;
        possibleWaterFlows = _waterFlows;
        maxWaterFlows = _maxFlows;
        startPosition = _startPos;
        //bounds.x = _level.floorGrid.GetLength(0)-1;
        //bounds.y = _level.floorGrid.GetLength(1)-1;
    }

    public void BeginWaterFlow()
    {
        foreach (WaterFlow _flow in possibleWaterFlows)
        {
            if(_flow.isWaterSource)
            {
                WaterFlow newFlow = new WaterFlow(level, this, _flow.lifetime, _flow.isWaterSource, _flow.horizontalChance, _flow.verticalChance);
                newFlow.waterTile = _flow.waterTile;
                newFlow.shoreTile = _flow.shoreTile;
                newFlow.waterRadius = _flow.waterRadius;
                newFlow.shoreRadius = _flow.shoreRadius;
                newFlow.splitChance = _flow.splitChance;
                newFlow.SetPosition(startPosition);
                aliveWaterFlows.Add(newFlow);
            }
            if(aliveWaterFlows.Count >= maxWaterFlows) {break;}
        }
    }

    public void BeginWaterFlow(Vector2Int _startPos)
    {
        foreach (WaterFlow _flow in possibleWaterFlows)
        {
            if(_flow.isWaterSource)
            {
                WaterFlow newFlow = new WaterFlow(level, this, _flow.lifetime, _flow.isWaterSource, _flow.horizontalChance, _flow.verticalChance);
                newFlow.waterTile = _flow.waterTile;
                newFlow.shoreTile = _flow.shoreTile;
                newFlow.waterRadius = _flow.waterRadius;
                newFlow.shoreRadius = _flow.shoreRadius;
                newFlow.splitChance = _flow.splitChance;
                newFlow.SetPosition(_startPos);
                aliveWaterFlows.Add(newFlow);
            }
            if(aliveWaterFlows.Count >= maxWaterFlows) {break;}
        }
    }

    //Update water flow by doing the correct action.
    public void DoWaterFlow()
    {
        for (int i = 0; i < aliveWaterFlows.Count; i++)
        {
            if(!aliveWaterFlows[i].isAlive)
            {
                aliveWaterFlows.Remove(aliveWaterFlows[i]);
            }else{
                aliveWaterFlows[i].Flow();
            }
        }
    }

    public void AddWaterFlow(WaterFlow _flow)
    {
        if(_flow.isAlive)
        {
            aliveWaterFlows.Add(_flow);
        }
    }

    public WaterFlow NewRandWaterFlow()
    {
        WaterFlow _flow = possibleWaterFlows[Random.Range(0,possibleWaterFlows.Length)];
        WaterFlow newFlow = new WaterFlow(level, this, _flow.lifetime, _flow.isWaterSource, _flow.horizontalChance, _flow.verticalChance);
        newFlow.waterTile = _flow.waterTile;
        newFlow.shoreTile = _flow.shoreTile;
        newFlow.waterRadius = _flow.waterRadius;
        newFlow.shoreRadius = _flow.shoreRadius;
        newFlow.splitChance = _flow.splitChance;
        return newFlow;
    }

    public WaterFlow GetRandWaterFlow()
    {
        return possibleWaterFlows[Random.Range(0,possibleWaterFlows.Length)];
    }

}
