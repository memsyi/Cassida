using UnityEngine;
using System.Collections.Generic;

// enums
public enum EdgeLength { Three = 3, Fife = 5, Seven = 7, Nine = 9, Eleven = 11, Thirteen = 13 }
public enum MapForms { Hexagon, CuttedDiamond, Diamond }

public class MapGenerator : MonoBehaviour
{
    [SerializeField]
    private EdgeLength _bottomEdgeLength = EdgeLength.Fife;

    [SerializeField]
    private MapForms _mapForm = MapForms.Hexagon;

    [SerializeField]
    private string
        //_tileBorder = null,
        _asteroidsTerrain = "Asteroids Terrain";

    #region Terrains
    //public Transform TileBorder
    //{
    //    get { return _tileBorder; }
    //    set { _tileBorder = value; }
    //}
    public string AsteroidsTerrain
    {
        get { return _asteroidsTerrain; }
        set { _asteroidsTerrain = value; }
    }
    #endregion

    #region Variables
    private MapForms MapForm
    {
        get { return _mapForm; }
        set { _mapForm = value; }
    }

    private int BottomEdgeLength
    {
        get { return (int)_bottomEdgeLength; }
        set { _bottomEdgeLength = (EdgeLength)value; }
    }
    #endregion

    public void GenerateMap(List<Tile> tileList)
    {
        if (MapForm == MapForms.CuttedDiamond)
        {
            BottomEdgeLength = BottomEdgeLength - BottomEdgeLength / 2 + 1;
        }
        else if (MapForm == MapForms.Diamond)
        {
            BottomEdgeLength = BottomEdgeLength / 2 + 1;
        }

        for (int x = -BottomEdgeLength + 1; x < BottomEdgeLength; x++)
        {
            for (int y = -BottomEdgeLength + 1; y < BottomEdgeLength; y++)
            {
                // Set tile in dependence on the map form
                if ((MapForm == MapForms.Hexagon && Mathf.Abs(x + y) < BottomEdgeLength)
                 || (MapForm == MapForms.CuttedDiamond && Mathf.Abs(x + y) <= BottomEdgeLength * 2 - 4)
                 || MapForm == MapForms.Diamond)
                {
                    Vector2 position = new Vector2(x, y);
                    tileList.Add(new Tile(position, InstantiateTileObject(position, "TileParent"), CalculateTerrainType(), CalculateObjectiveType()));
                }
            }
        }
    }

    public Transform InstantiateTileObject(Vector2 position, string modelName)
    {
        if (modelName.Length == 0)
        {
            Debug.LogError("You need to assign all objects on the map generator.");
        }

        // Instantiate tile
        var tileObject = PhotonNetwork.Instantiate(
            modelName,
            // Calculate tile position
            new Vector3(position.x * 1.75f + position.y * 0.875f, 0, position.y * 1.515f),
            Quaternion.identity, 0).transform;

        tileObject.name = position.ToString();
        tileObject.SetParent(this.transform);

        return tileObject;
    }

    private TerrainType CalculateTerrainType()
    {
        // TODO not mirrowed!!!!!
        int _randomValue = Random.Range(0, 4);
        if (_randomValue == 0)
        {
            return TerrainType.Asteroids;
        }

        return TerrainType.Empty;
    }

    private ObjectiveType CalculateObjectiveType()
    {
        return ObjectiveType.Empty;
    }

    private void Init()
    {

    }

    private void Start()
    {
        Init();
    }

    private void Update()
    {

    }
}
