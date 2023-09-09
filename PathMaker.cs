using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathMaker
{
    public TileType pathType = TileType.dirt;
    public int lifetime = -1;
    public Vector2Int position = Vector2Int.zero;
    public Vector2Int bounds = Vector2Int.one;
    public Vector2 horizontalChance = Vector2.one;
    public Vector2 verticalChance = Vector2.up;
    public float pathRadius = 1;
    private LevelData level;
    public bool isAlive = true;
    public bool canMakeNewPaths = false;
    public bool pathSpawnReady = false;
    public float newPathChance = 0.1f;

    private int edgeBumps = 0;

    public PathMaker(LevelData _level, int _lifetime, TileType _pathType, float _radius, Vector2Int _startPosition, Vector2Int _boundingBox, Vector2 _hChance, Vector2 _vChance)
    {
        level = _level;
        
        position = _startPosition;
        bounds = _boundingBox;
        lifetime = _lifetime;
        pathType = _pathType;
        pathRadius = _radius;
        
        horizontalChance = _hChance;
        verticalChance = _vChance;
    }

     public PathMaker(LevelData _level)
    {
        level = _level;
    }

    public void Walk()
    {
        if(!isAlive){return;}

        Vector2 _hChance = new Vector2(Random.Range(0f,1f),Random.Range(0f,1f));
        Vector2 _vChance = new Vector2(Random.Range(0f,1f),Random.Range(0f,1f));
        bool _hasMoved = false;

        if(_hChance.x <= horizontalChance.x)
        {
            if(InsideBounds(new Vector2Int(position.x-1, position.y)))
            {
                position.x--;
                _hasMoved = true;
            }else
            {
                edgeBumps++;
            }
        }else if(_hChance.y <= horizontalChance.y)
        {
            if(InsideBounds(new Vector2Int(position.x+1, position.y)))
            {
                position.x++;
                _hasMoved = true;
            }else
            {
                edgeBumps++;
            }
        }

        if(_vChance.x <= verticalChance.x)
        {
            if(InsideBounds(new Vector2Int(position.x, position.y+1)))
            {
                position.y++;
                _hasMoved = true;
            }else
            {
                edgeBumps++;
            }
        }else if(_vChance.y <= verticalChance.y)
        {
            if(InsideBounds(new Vector2Int(position.x, position.y-1)))
            {
                position.y--;
                _hasMoved = true;
            }else
            {
                edgeBumps++;
            }
        }

        if(_hasMoved)
        {
            PaintPath(pathType, pathRadius);
            DoLifetime();
            CheckCanSpawnPaths();
        }
    }

    public void Walk(Vector2Int _offset)
    {
        if(InsideBounds(position+_offset) && isAlive)
        {
            position += _offset;
            PaintPath(pathType, pathRadius);
            DoLifetime();
        }
    }

    public void PaintPath(TileType _path, float _radius)
    {
        Vector2Int _pos = Vector2Int.zero;
        for (int XX = (position.x - (int)_radius); XX < position.x + (int)_radius; XX++)
        {
            _pos.x = XX;
             for (int YY = position.y - (int)_radius; YY < position.y + (int)_radius; YY++)
            {
                _pos.y = YY;
                if(InsideBounds(_pos) && InsidePathRadius(_pos))
                {
                    //level.UpdateTileQuantity(level.GetTile(new Vector2Int(XX, YY)), -1);

                    level.SetTile(_path, new Vector2Int(XX, YY));

                    //level.UpdateTileQuantity(_path, 1);
                }
            }
        }
    }

    public bool RedeemNewPath()
    {
        bool makeNewPath = pathSpawnReady;
        pathSpawnReady = false;
        return makeNewPath;
    }

    public void TogglePathSpawning()
    {
        if(canMakeNewPaths)
        {
            canMakeNewPaths = false;
        }else
        {
            canMakeNewPaths = true;
        }
    }

    public void TogglePathSpawning(bool _enabled, float _spawnChance)
    {
        canMakeNewPaths = _enabled;
        newPathChance = _spawnChance;
    }

    void CheckCanSpawnPaths()
    {
        if(canMakeNewPaths)
        {
            float chance = Random.Range(0.0f, 1.0f);
            if(chance <= newPathChance)
            {
                //Create a new path maker, add it to level data?
                pathSpawnReady = true;
            }
        }
    }

    void DoLifetime()
    {
        if(edgeBumps >= 3)
        {
            isAlive = false;
        }

        if(lifetime>0)
        {
            lifetime--;
        }else if(lifetime == 0)
        {
            isAlive = false;
        }
    }

    bool InsidePathRadius(Vector2Int _pos)
    {
        if (Vector2.Distance(_pos, position) <= pathRadius)
        {
            return true;
        }
        return false;
    }

    bool InsideBounds(Vector2Int _pos)
    {
        if (_pos.x < 0 || _pos.y < 0 || _pos.x >= bounds.x || _pos.y >= bounds.y)
        {
            return false;
        }
        return true;
    }
}
