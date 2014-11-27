using UnityEngine;
using System.Collections;

public class MapGenerator : MonoBehaviour
{
    [SerializeField]
    private EdgeLength _bottomEdgeLength = EdgeLength.Fife;

    [SerializeField]
    private MapForms _mapForm = MapForms.Hexagon;

    [SerializeField]
    Transform _hexagonTile = null;

    #region Variables
    private Transform HexagonTile
    {
        get { return _hexagonTile; }
        set { _hexagonTile = value; }
    }

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

    // enums
    private enum EdgeLength { Three = 3, Fife = 5, Seven = 7, Nine = 9, Eleven = 11, Thirteen = 13 }
    private enum MapForms { Hexagon, CuttedDiamond, Diamond }

    // map variables
    private Transform Map { get; set; }
    public Transform[,] TileArray { get; private set; }

    private void GenerateMap()
    {
        if (MapForm == MapForms.CuttedDiamond)
        {
            BottomEdgeLength = BottomEdgeLength - BottomEdgeLength / 2 + 1;
        }
        else if (MapForm == MapForms.Diamond)
        {
            BottomEdgeLength = BottomEdgeLength / 2 + 1;
        }

        // Set up map variables
        Map = new GameObject("Map").transform;
        TileArray = new Transform[BottomEdgeLength * 2 - 1, BottomEdgeLength * 2 - 1];

        for (int x = -BottomEdgeLength + 1; x < BottomEdgeLength; x++)
        {
            for (int z = -BottomEdgeLength + 1; z < BottomEdgeLength; z++)
            {
                if ((MapForm == MapForms.Hexagon && Mathf.Abs(x + z) < BottomEdgeLength)
                 || (MapForm == MapForms.CuttedDiamond && Mathf.Abs(x + z) <= BottomEdgeLength * 2 - 4)
                 ||  MapForm == MapForms.Diamond)
                {
                    InstantiateTile(new Vector3(x, 0, z));
                }
            }
        }
    }

    private void InstantiateTile(Vector3 position)
    {
        Transform newTile = Instantiate(
            HexagonTile,
            new Vector3(position.x + (position.z * HexagonTile.localScale.z / 2) * 10, 0, position.z * HexagonTile.localScale.z * 10),
            HexagonTile.localRotation) as Transform;

        newTile.name = position.x + " " + position.z;
        newTile.SetParent(Map);

        TileArray[(int)position.x + BottomEdgeLength - 1, (int)position.z + BottomEdgeLength - 1] = newTile;
    }

    private void Init()
    {
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
