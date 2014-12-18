using UnityEngine;
using System.Collections.Generic;

// enums
public enum EdgeLength { Three = 3, Fife = 5, Seven = 7, Nine = 9, Eleven = 11, Thirteen = 13 }
public enum MapForms { Hexagon, CuttedDiamond, Diamond }

[RequireComponent(typeof(PhotonView))]
public class MapGenerator : Photon.MonoBehaviour
{
    [SerializeField]
    private EdgeLength _bottomEdgeLength = EdgeLength.Fife;

    [SerializeField]
    private MapForms _mapForm = MapForms.Hexagon;

    [SerializeField]
    private Transform
        _tileParent = null,
        _asteroidsTerrain = null;

    #region Terrains
    public Transform TileParent
    {
        get { return _tileParent; }
        set { _tileParent = value; }
    }
    public Transform AsteroidsTerrain
    {
        get { return _asteroidsTerrain; }
        set { _asteroidsTerrain = value; }
    }
    #endregion

    private WorldManager WorldManager { get; set; }

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

    public void GenerateMap()
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
                    photonView.RPC("InstantiateTileParentObject", PhotonTargets.AllBufferedViaServer, position, CalculateTerrainType().GetHashCode(), CalculateObjectiveType().GetHashCode());
                }
            }
        }
    }

    [RPC]
    private void InstantiateTileParentObject(Vector2 position, int terrainType, int objectiveType)
    {
        // Instantiate tile
        var tileObject = Instantiate(
            TileParent,
            // Calculate tile position
            new Vector3(position.x * 1.75f + position.y * 0.875f, 0, position.y * 1.515f),
            Quaternion.identity) as Transform;

        tileObject.name = position.ToString();
        tileObject.renderer.material.color = WorldManager.TileSettings.DefaultColor;
        tileObject.SetParent(this.transform);

        WorldManager.TileList.Add(new Tile(position, tileObject, (TerrainType)terrainType, (ObjectiveType)objectiveType));
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
        WorldManager = GameObject.FindGameObjectWithTag(Tags.Manager).GetComponent<WorldManager>();
    }

    private void Start()
    {
        
    }

    private void Awake()
    {
        Init();
    }

    private void Update()
    {

    }
}
