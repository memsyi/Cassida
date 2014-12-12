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
    public Transform TileObject { get; private set; }
    public TerrainType Terrain
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
    public ObjectiveType Objective { get; set; }

    public FleetController Fleet { get; set; }

    public TerrainController TerrainController { get; private set; }
    public ObjectiveController ObjectiveController { get; private set; }

    public Tile(Vector2 position, Transform tileObject, TerrainType terrain, ObjectiveType objective)
    {
        Position = position;
        TileObject = tileObject;
        Terrain = terrain;
        Objective = objective;
    }

    private void SetCorrectTerrain()
    {
        if (Terrain == TerrainType.Empty)
        {
            return;
        }

        TerrainController = TileObject.GetComponent<TerrainController>();
        TerrainController.Type = Terrain;
    }

    private void SetCorrectObjective()
    {
        if (ObjectiveController)
        {
            ObjectiveController.DeletObjective();
            ObjectiveController = null;
        }

        if (Objective == ObjectiveType.Empty)
        {
            return;
        }

        ObjectiveController = TileObject.GetComponent<ObjectiveController>();
        ObjectiveController.Type = Objective;
    }
}

public class MapController : MonoBehaviour
{
    public Tile NearestTileToMousePosition { get { return FindNearestTileToMousePosition(); } }

    private MouseController MouseController { get; set; }

    private WorldManager WorldManager { get; set; }

    private Tile FindNearestTileToMousePosition()
    {
        var mousePosition = MouseController.MousePositionOnMap;

        var shortestDistance = 1000f;
        Tile nearestTile = null;

        foreach (var tile in WorldManager.TileList)
        {
            var distance = Vector2.Distance(mousePosition, new Vector2(tile.TileObject.position.x, tile.TileObject.position.z));

            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestTile = tile;
            }
        }

        return nearestTile;
    }

    public void GenerateMap(List<Tile> tileList)
    {
        var mapGenerator = GameObject.FindGameObjectWithTag(Tags.Generators).GetComponent<MapGenerator>();

        if (!mapGenerator)
        {
            Debug.LogError("Add MapGenerator to Generators object");
            return;
        }

        mapGenerator.GenerateMap(tileList);
    }

    private void Init()
    {
        MouseController = GameObject.FindGameObjectWithTag(Tags.GameController).GetComponent<MouseController>();
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
