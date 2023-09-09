using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaterFlow
{
    public string name;
    public TileType waterTile = TileType.water;
    public TileType shoreTile = TileType.dirt;
    [SerializeField]
    private Vector2Int position = Vector2Int.zero;
    [SerializeField]
    public int lifetime = -1;
    [SerializeField][Range(0.0f, 1.0f)]
    public float splitChance = 0.01f;
    public Vector2 horizontalChance = Vector2.one;
    public Vector2 verticalChance = Vector2.up;
    public float waterRadius = 1;
    public float shoreRadius = 2;
    [SerializeField]
    public bool isWaterSource = false;
    [SerializeField]
    public bool isAlive = true;
    WaterSystem parentSystem;
    [SerializeField]
    LevelData level;
    public WaterFlow(LevelData _level, WaterSystem _mainSystem, int _life, bool _waterSource, Vector2 _hChance, Vector2 _vChance)
    {
        //Constructor
        lifetime = _life;
        isWaterSource = _waterSource;
        parentSystem = _mainSystem;
        level = _level;

        horizontalChance = _hChance;
        verticalChance = _vChance;

    }
    public void Flow()
    {
        //Flow around
        if(!isAlive){return;}

        Vector2 _hChance = new Vector2(Random.Range(0f,1f),Random.Range(0f,1f));
        Vector2 _vChance = new Vector2(Random.Range(0f,1f),Random.Range(0f,1f));
        bool _hasMoved = false;

        if(_hChance.x <= horizontalChance.x)
        {
            if(level.InsideBounds(new Vector2Int(position.x-1, position.y)))
            {
                position.x--;
                _hasMoved = true;
            }else
            {
                isAlive = false;
            }
        }else if(_hChance.y <= horizontalChance.y)
        {
            if(level.InsideBounds(new Vector2Int(position.x+1, position.y)))
            {
                position.x++;
                _hasMoved = true;
            }else
            {
                isAlive = false;
            }
        }

        if(_vChance.x <= verticalChance.x)
        {
            if(level.InsideBounds(new Vector2Int(position.x, position.y+1)))
            {
                position.y++;
                _hasMoved = true;
            }else
            {
                isAlive = false;
            }
        }else if(_vChance.y <= verticalChance.y)
        {
            if(level.InsideBounds(new Vector2Int(position.x, position.y-1)))
            {
                position.y--;
                _hasMoved = true;
            }else
            {
                isAlive = false;
            }
        }

        if(_hasMoved)
        {
            PaintWater(waterTile, shoreTile);
            DoLifetime();
            Branch();
            //CheckCanSpawnPaths();
        }
    }
    public void Branch()
    {
        float chance = Random.Range(0.0f,1.0f);
        if (chance <= splitChance && lifetime > 1 && !parentSystem.IsAtMax)
        {
            //Split, divide up lifetime
            WaterFlow newFlow = new WaterFlow(level, parentSystem, Mathf.CeilToInt((float)lifetime/2),false, new Vector2(Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f)),new Vector2(Random.Range(0.0f,1.0f),Random.Range(0.0f,1.0f)));
            newFlow.waterTile = waterTile;
            newFlow.shoreTile = shoreTile;
            newFlow.waterRadius = waterRadius * 0.75f;
            newFlow.shoreRadius = shoreRadius * 0.75f;
            newFlow.splitChance = splitChance;
            newFlow.SetPosition(position);
            parentSystem.AddWaterFlow(newFlow);
        }else if(chance <= splitChance && lifetime <= -1 && !parentSystem.IsAtMax)
        {
            WaterFlow newFlow = parentSystem.NewRandWaterFlow();
            newFlow.SetPosition(position);
            parentSystem.AddWaterFlow(newFlow);
        }
    }
    public void PaintWater(TileType _water, TileType _shore)
    {
        Vector2Int _pos = Vector2Int.zero;
        int _radius = (int)Mathf.Max(waterRadius, shoreRadius);
        for (int XX = (position.x - _radius); XX < position.x + _radius; XX++)
        {
            _pos.x = XX;
             for (int YY = position.y - _radius; YY < position.y + _radius; YY++)
            {
                _pos.y = YY;
                if(level.InsideBounds(_pos) && InsideRadius(_pos, waterRadius))
                {
                    //level.UpdateTileQuantity(level.GetTile(new Vector2Int(XX, YY)), -1);

                    level.SetTile(_water, new Vector2Int(XX, YY));

                    //level.UpdateTileQuantity(_water, 1);
                }else if(level.InsideBounds(_pos) && InsideRadius(_pos, shoreRadius) && level.GetTile(new Vector2Int(XX, YY)) != _water)
                {
                    //level.UpdateTileQuantity(level.GetTile(new Vector2Int(XX, YY)), -1);

                    level.SetTile(_shore, new Vector2Int(XX, YY));

                    //level.UpdateTileQuantity(_shore, 1);
                }
            }
        }
    }

    public void SetPosition(Vector2Int _pos)
    {
        position = _pos;
        PaintWater(waterTile, shoreTile);
    }
    void DoLifetime()
    {
        //Check if dead or at edge of screen
        if(lifetime>0)
        {
            lifetime--;
        }else if(lifetime == 0)
        {
            isAlive = false;
        }
    }

    bool InsideRadius(Vector2Int _pos, float _radius)
    {
        if (Vector2.Distance(_pos, position) <= _radius)
        {
            return true;
        }
        return false;
    }
}
