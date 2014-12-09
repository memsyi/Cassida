using UnityEngine;

public enum TerrainType
{
    Empty,
    Whatever
}

public enum ObjectiveType
{
    Empty,
    Foo
}

public class Tile
{
    public Vector2 Position { get; private set; }
    public Transform TileBorder { get; set; }
    public TerrainType Terrain { get; private set; }
    public ObjectiveType Objective { get; set; }
    public FleetController Fleet { get; set; }
    public Transform TerrainObject { get; set; }
    public Transform ObjectveObject { get; set; }

    public Tile(Vector2 position, TerrainType terrain, ObjectiveType objective)
    {
        Position = position;
        Terrain = terrain;
        Objective = objective;
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
        Tile nearestTile = null;// new Tile(Vector2.zero, TerrainType.Empty, ObjectiveType.Empty);

        foreach(var tile in WorldManager.TileList)
        {
            var distance = Vector2.Distance(mousePosition, new Vector2(tile.TileBorder.position.x, tile.TileBorder.position.z));

            if(distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestTile = tile;
            }
        }

        return nearestTile;
    }

    private void Init()
    {
        MouseController = GameObject.FindGameObjectWithTag(Tags.GameController).GetComponent<MouseController>();
        WorldManager = GameObject.FindGameObjectWithTag(Tags.Manager).GetComponent<WorldManager>();
    }

    private void Start()
    {
        Init();
    }

    private void Update()
    {

    }
}
