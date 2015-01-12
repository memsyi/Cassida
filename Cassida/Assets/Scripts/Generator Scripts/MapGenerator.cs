using UnityEngine;
using System.Collections.Generic;

// enums
public enum EdgeLength { Three = 3, Fife = 5, Seven = 7, Nine = 9, Eleven = 11, Thirteen = 13 }
public enum MapForms { Hexagon, CuttedDiamond, Diamond }

[RequireComponent(typeof(PhotonView))]
public class MapGenerator : Photon.MonoBehaviour
{
    #region Variables
    [SerializeField]
    private EdgeLength _bottomEdgeLength = EdgeLength.Fife;

    [SerializeField]
    private MapForms _mapForm = MapForms.Hexagon;

    [SerializeField] // TODO struct !! [Serialize..]
    private Transform
        _tileParent = null,
        _asteroidsTerrain = null;

    #region Terrains
    public Transform TileParent
    {
        get { return _tileParent; }
        private set { _tileParent = value; }
    }
    public Transform AsteroidsTerrain
    {
        get { return _asteroidsTerrain; }
        private set { _asteroidsTerrain = value; }
    }
    #endregion

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

    // Lists
    List<Tile> TileList { get { return TileManager.Get().TileList; } }
    #endregion

    public void GenerateMap()
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

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
                    photonView.RPC(RPCs.InstantiateTile, PhotonTargets.All, x, y, (int)CalculateTerrainType(), (int)CalculateObjectiveType());
                }
            }
        }
    }

    public void InstatiateAllExistingTilesAtPlayer(PhotonPlayer player)
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        foreach (var tile in TileList)
        {
            photonView.RPC(RPCs.InstantiateTile, player, tile.Position.X, tile.Position.Y, tile.TerrainType.GetHashCode(), tile.ObjectiveType.GetHashCode());
        }
    }

    [RPC]
    private void InstantiateTile(int positionX, int positionY, int terrainType, int objectiveType, PhotonMessageInfo info)
    {
        var position = new Position(positionX, positionY);

        if (!info.sender.isMasterClient || TileList.Exists(t => t.Position == position))
        {
            return;
        }

        // Instantiate tile
        var tileObject = Instantiate(
            TileParent,
            // Calculate tile position
            new Vector3(position.X * 1.75f + position.Y * 0.875f, 0, position.Y * 1.515f),
            Quaternion.identity) as Transform;

        tileObject.name = position.ToString();
        tileObject.renderer.material.color = TileManager.Get().TileColor.DefaultColor;
        tileObject.SetParent(this.transform);

        TileList.Add(new Tile(position, tileObject, (TerrainType)terrainType, (ObjectiveType)objectiveType));
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

    private void Awake()
    {
        //Check for Singleton
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Debug.LogError("Second instance!");
            return;
        }
    }

    private void Update()
    {

    }

    private static MapGenerator _instance = null;
    public static MapGenerator Get()
    {
        if (_instance == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag(Tags.Map);

            if (obj.GetComponent<MapGenerator>() == null)
            {
                _instance = obj.AddComponent<MapGenerator>();
            }
            else
            {
                _instance = obj.GetComponent<MapGenerator>();
            }
        }

        return _instance;
    }
}
