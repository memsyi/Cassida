using UnityEngine;
using System.Collections.Generic;

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
    public TerrainType Terrain { get; private set; }
    public ObjectiveType Objective { get; set; }
    public FleetController Fleet { get; set; }
    public Transform TerrainObject { get; set; }
    public Transform ObjectveObject { get; set; }

    public Tile(Vector2 position, TerrainType terrain, ObjectiveType objective, Transform terrainObject, Transform objectiveObject)
    {
        Position = position;
        Terrain = terrain;
        Objective = objective;
        TerrainObject = terrainObject;
        ObjectveObject = objectiveObject;
    }
}

public class MapManager : MonoBehaviour
{
    public Dictionary<Vector2, Tile> TileDictionary { get; private set; }



    private void GenerateMap()
    {
        if (!GameObject.FindGameObjectWithTag(Tags.Generators).GetComponent<MapGenerator>())
        {
            Debug.LogError("Add MapGenerator to Generators object");
            return;
        }

        GameObject.FindGameObjectWithTag(Tags.Generators).GetComponent<MapGenerator>().GenerateMap(TileDictionary);
    }

    private void Init()
    {
        TileDictionary = new Dictionary<Vector2, Tile>();
        GenerateMap();
    }

    private void Start()
    {
        Init();
    }

    private void Update()
    {

    }
}
