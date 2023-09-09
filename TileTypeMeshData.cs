using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileTypeMeshData
{
    [SerializeField]
    private TileType tile;
    public Vector3[] vertices;
    public Vector2[] uv;
    public int[] triangles;
    public int vTrack = 0;
    public int uvTrack = 0;
    public int tTrack = 0;
    public bool isWall = false;

    public TileType Tile
    {
        get{return tile;}
    }

    //Constructor
    public TileTypeMeshData(TileType _tile, Vector3[] _verts, Vector2[] _uvs, int[] _tris)
    {
        tile = _tile;
        vertices = _verts;
        uv = _uvs;
        triangles = _tris;
        isWall = false;
    }

    //Updates mesh, sets verts, tris, and uvs. Recalculates normals.
    public void UpdateMesh(ref Mesh _mesh)
    {
        _mesh.Clear();

        _mesh.vertices = vertices;
        _mesh.uv = uv;
        _mesh.triangles = triangles;

        _mesh.RecalculateNormals();
    }

}
