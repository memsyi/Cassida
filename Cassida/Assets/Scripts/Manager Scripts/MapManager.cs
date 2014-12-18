using UnityEngine;
using System.Collections.Generic;

public enum TerrainType
{
    Empty,
    Asteroids,
    Nebula,
    EnergyField,
    BlackHole
}

public enum ObjectiveType
{
    Empty,
    Rubble,
    Village,
    Town,
    TradingStation,
    Outpost
}

public class Tile
{
    private TerrainType _terrain;
    private ObjectiveType _objective;

    public Vector2 Position { get; private set; }
    public Transform TileParent { get; private set; }
    public TerrainType TerrainType
    {
        get
        {
            return _terrain;
        }
        private set
        {
            _terrain = value;
            SetCorrectTerrain();
        }
    }
    public ObjectiveType ObjectiveType
    {
        get
        {
            return _objective;
        }
        private set
        {
            _objective = value;
            SetCorrectObjective();
        }
    }

    public Fleet Fleet { get; set; }

    public TerrainController TerrainController { get; private set; }
    public ObjectiveController ObjectiveController { get; private set; }

    public Tile(Vector2 position, Transform tileParent, TerrainType terrain, ObjectiveType objective)
    {
        Position = position;
        TileParent = tileParent;
        TerrainType = terrain;
        ObjectiveType = objective;
    }

    private void SetCorrectTerrain()
    {
        if (TerrainType == TerrainType.Empty)
        {
            return;
        }

        TerrainController = TileParent.gameObject.AddComponent<TerrainController>();
        TerrainController.Type = TerrainType;
    }

    private void SetCorrectObjective()
    {
        if (ObjectiveController)
        {
            ObjectiveController.DeletObjective();
            ObjectiveController = null;
        }

        if (ObjectiveType == ObjectiveType.Empty)
        {
            return;
        }

        ObjectiveController = TileParent.gameObject.AddComponent<ObjectiveController>();
        ObjectiveController.Type = ObjectiveType;
    }
}

public class MapManager : MonoBehaviour
{
    public Tile NearestTileToMousePosition { get { return FindNearestTileToMousePosition(); } }

    private MouseController MouseController { get; set; }

    private WorldManager WorldManager { get; set; }

    private Tile FindNearestTileToMousePosition()
    {
        if (WorldManager.TileList == null || WorldManager.TileList.Count == 0)
        {
            return null;
        }

        var mousePosition = MouseController.MousePositionOnMap;

        var shortestDistance = 1000f;
        Tile nearestTile = null;

        foreach (var tile in WorldManager.TileList)
        {
            var distance = Vector2.Distance(mousePosition, new Vector2(tile.TileParent.position.x, tile.TileParent.position.z));

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestTile = tile;
            }
        }

        return nearestTile;
    }

    public void GenerateMap()
    {
        var mapGenerator = GameObject.FindGameObjectWithTag(Tags.Map).GetComponent<MapGenerator>();

        if (!mapGenerator)
        {
            Debug.LogError("Add MapGenerator to Map object");
            return;
        }

        mapGenerator.GenerateMap();
    }

    private void Init()
    {
        MouseController = GameObject.FindGameObjectWithTag(Tags.GameController).GetComponent<MouseController>();
        WorldManager = GameObject.FindGameObjectWithTag(Tags.Manager).GetComponent<WorldManager>();

        if (!MouseController || !WorldManager)
        {
            Debug.LogError("MissedComponents!");
        }
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
