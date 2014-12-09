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
    private Transform
        _tileBorder = null,
        _emptyTerrain = null;

    #region Terrains
    public Transform TileBorder
    {
        get { return _tileBorder; }
        set { _tileBorder = value; }
    }
    private Transform EmptyTerrain
    {
        get { return _emptyTerrain; }
        set { _emptyTerrain = value; }
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
                    tileList.Add(new Tile(new Vector2(x, y), CalculateTerrainType(), CalculateObjectiveType()));
                }
            }
        }

        // Instanciate all objects of every tile
        foreach (var tile in tileList)
        {
            tile.TileBorder = InstantiateTileObject(tile.Position, TileBorder);

            if (tile.Terrain != TerrainType.Empty)
            {
                tile.TerrainObject = InstantiateTileObject(tile.Position, GetTerrainModelByType(tile.Terrain));
            }

            if (tile.Objective != ObjectiveType.Empty)
            {
                tile.ObjectveObject = InstantiateTileObject(tile.Position, GetObjectiveModelByType(tile.Objective));
            }
        }
    }

    private Transform InstantiateTileObject(Vector2 position, Transform model)
    {
        if(!model)
        {
            Debug.LogError("You need to assign all objects on the map generator.");
        }

        // Instantiate tile
        Transform tileObject = Instantiate(
            model,
            // Calculate tile position
            new Vector3(position.x * 1.75f + position.y * 0.875f, 0, position.y * model.localScale.z * 1.515f),
            model.localRotation) as Transform;

        tileObject.name = position.ToString();
        tileObject.SetParent(GameObject.FindGameObjectWithTag(Tags.Map).transform);

        return tileObject;
    }

    private TerrainType CalculateTerrainType()
    {
        return TerrainType.Empty;
    }

    private ObjectiveType CalculateObjectiveType()
    {
        return ObjectiveType.Empty;
    }

    private Transform GetTerrainModelByType(TerrainType terrainType)
    {
        switch (terrainType)
        {
            case TerrainType.Empty:
                return null;
            case TerrainType.Whatever:
                return null;
            default:
                return null;
        }
    }

    private Transform GetObjectiveModelByType(ObjectiveType objectiveType)
    {
        switch (objectiveType)
        {
            case ObjectiveType.Empty:
                return null;
            case ObjectiveType.Foo:
                return null;
            default:
                return null;
        }
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
