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
        _tileObject = null,
        _asteroidsTerrainObject = null;

    #region Terrains
    public Transform TileObject
    {
        get { return _tileObject; }
        //private set { _tileParentObject = value; }
    }
    public Transform AsteroidsTerrainObject
    {
        get { return _asteroidsTerrainObject; }
        //private set { _asteroidsTerrainObject = value; }
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

    public static Transform InstatiateParentObject(Position position)
    {
        var tileParent = new GameObject(position.ToString()).transform;
        tileParent.position = new Vector3(position.X * 1.75f + position.Y * 0.875f, 0, position.Y * 1.515f);
        tileParent.SetParent(MapGenerator.Get().transform);
        return tileParent;
    }

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
                    var position = new Position(x, y);
                    photonView.RPC(RPCs.AddTile, PhotonTargets.All, x, y, (int)CalculateTerrainType(position), (int)CalculateObjectiveType(position));
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
            photonView.RPC(RPCs.AddTile, player, tile.Position.X, tile.Position.Y, (int)tile.TerrainType, (int)tile.ObjectiveType);
        }
    }

    [RPC]
    private void AddTile(int positionX, int positionY, int terrainType, int objectiveType, PhotonMessageInfo info)
    {
        var position = new Position(positionX, positionY);

        if (!info.sender.isMasterClient || TileList.Exists(t => t.Position == position))
        {
            return;
        }

        TileList.Add(new Tile(position, (TerrainType)terrainType, (ObjectiveType)objectiveType));
    }

    public Transform InstatiateTileObject(Transform tileParent)
    {
        var tileObject = Instantiate(TileObject, tileParent.position, Quaternion.identity) as Transform;
        tileObject.name = "Tile object";
        tileObject.renderer.material.color = TileManager.Get().TileColor.DefaultColor;
        tileObject.SetParent(tileParent);
        return tileObject;
    }

    private TerrainType CalculateTerrainType(Position position)
    {
        // TODO not mirrowed!!!!!
        int _randomValue = Random.Range(0, 4);
        if (_randomValue == 0)
        {
            return TerrainType.Asteroids;
        }

        return TerrainType.Empty;
    }

    private ObjectiveType CalculateObjectiveType(Position position)
    {
        if(position.IsSameAs(new Position(3, 0))) return ObjectiveType.Base;
        return ObjectiveType.Empty; // TODO calculate tiles where bases can be place
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
