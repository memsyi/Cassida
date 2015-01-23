using UnityEngine;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    #region Variables
    // Tiles
    public Tile NearestTileToMousePosition { get { return FindNearestTileToMousePosition(); } }

    // Lists
    private List<Tile> TileList { get { return TileManager.Get().TileList; } }
    #endregion

    public void InitializeMap(EdgeLength bottomEdgeLength, MapForms mapForm)
    {
        GenerateMap(bottomEdgeLength, mapForm);
        AddBasesToMap();
        AddStartFleets();
    }

    private void GenerateMap(EdgeLength bottomEdgeLength, MapForms mapForm)
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        MapGenerator.Get().GenerateMap((int)bottomEdgeLength, mapForm);
    }

    private void AddBasesToMap()
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        BaseManager.Get().InstantiateBasesForAllPlayer();
    }

    private void AddStartFleets()
    {
        FleetManager.Get().AddStartFleets();
    }

    private Tile FindNearestTileToMousePosition()
    {
        return FindNearestTileToPosition(MouseController.Get().MousePositionOnMap);
    }

    public Tile FindNearestTileToPosition(Vector3 position)
    {
        if (TileList == null || TileList.Count == 0)
        {
            return null;
        }

        var shortestDistance = 1000f;
        Tile nearestTile = null;

        foreach (var tile in TileList)
        {
            var distance = Vector2.Distance(position, new Vector2(tile.TileParent.position.x, tile.TileParent.position.z));

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestTile = tile;
            }
        }

        return nearestTile;
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

    private static MapManager _instance = null;
    public static MapManager Get()
    {
        if (_instance == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag(Tags.Manager);
            _instance = obj.AddComponent<MapManager>();
        }

        return _instance;
    }
}
