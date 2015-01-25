using UnityEngine;
using System.Collections.Generic;

// enums
public enum EdgeLength { Three = 3, Fife = 5, Seven = 7, Nine = 9, Eleven = 11, Thirteen = 13 }
public enum MapForms { Hexagon, CuttedDiamond, Diamond }

[RequireComponent(typeof(PhotonView))]
public class MapGenerator : Photon.MonoBehaviour
{
    #region Variables
    [SerializeField] // TODO outsource as struct
    private Transform
        _tileObject = null,
        _asteroidsTerrainObject = null;

    public Transform TileObject
    {
        get { return _tileObject; }
    }

    #region Terrains
    public Transform AsteroidsTerrainObject
    {
        get { return _asteroidsTerrainObject; }
    }
    #endregion

    // Lists
    List<Tile> TileList { get { return TileManager.Get().TileList; } }
    #endregion

    public static Transform InstatiateTile(Position position)
    {
        var tileParent = new GameObject(position.ToString()).transform;
        var positionInWorld = new Vector3(position.X * 1.75f + position.Y * 0.875f, 0, position.Y * 1.515f);
        tileParent.position = positionInWorld;
        tileParent.SetParent(MapGenerator.Get().transform);
        return tileParent;
    }

    public void GenerateMap(int bottomEdgeLength, MapForms mapForm)
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        if (mapForm == MapForms.CuttedDiamond)
        {
            bottomEdgeLength = bottomEdgeLength - bottomEdgeLength / 2 + 1;
        }
        else if (mapForm == MapForms.Diamond)
        {
            bottomEdgeLength = bottomEdgeLength / 2 + 1;
        }

        for (int x = -bottomEdgeLength + 1; x < bottomEdgeLength; x++)
        {
            for (int y = -bottomEdgeLength + 1; y < bottomEdgeLength; y++)
            {
                // Set tile in dependence on the map form
                if ((mapForm == MapForms.Hexagon && Mathf.Abs(x + y) < bottomEdgeLength)
                 || (mapForm == MapForms.CuttedDiamond && Mathf.Abs(x + y) <= bottomEdgeLength * 2 - 4)
                 || mapForm == MapForms.Diamond)
                {
                    var position = new Position(x, y);
                    // Only set terrain on tiles without an objective
                    var objectiveType = CalculateObjectiveType(position);
                    var terrainType = objectiveType == ObjectiveType.Empty ? (int)CalculateTerrainType(position) : (int)TerrainType.Empty;

                    photonView.RPC(RPCs.AddTile, PhotonTargets.All, x, y, (int)objectiveType, (int)terrainType);
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
            photonView.RPC(RPCs.AddTile, player, tile.Position.X, tile.Position.Y, (int)tile.ObjectiveType, (int)tile.TerrainType);
        }
    }

    [RPC]
    private void AddTile(int positionX, int positionY, int objectiveType, int terrainType, PhotonMessageInfo info)
    {
        var position = new Position(positionX, positionY);

        if (!info.sender.isMasterClient || TileList.Exists(t => t.Position == position))
        {
            return;
        }

        TileList.Add(new Tile(position, (ObjectiveType)objectiveType, (TerrainType)terrainType));
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
        // TODO complete calculation
        int _randomValue = Random.Range(0, 4);
        if (_randomValue == 0)
        {
            return TerrainType.Asteroids;
        }

        return TerrainType.Empty;
    }

    private ObjectiveType CalculateObjectiveType(Position position)
    {
        // TODO complete calculation
        if (position == new Position(3, 0) || position == new Position(-3, 0)) return ObjectiveType.Base; // TODO calculate tiles for bases
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
            _instance = obj.AddComponent<MapGenerator>();
        }

        return _instance;
    }
}
