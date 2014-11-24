using UnityEngine;
using System.Collections;

public class MapGenerator : MonoBehaviour
{
    [SerializeField]
    private Vector2 _mapSize;

    [SerializeField]
    Transform _hexagonTile;

    #region Variables
    private Transform HexagonTile
    {
        get { return _hexagonTile; }
        set { _hexagonTile = value; }
    }

    private Vector2 MapSize
    {
        get { return _mapSize; }
        set { _mapSize = value; }
    } 
	#endregion

    private Transform[,] TileArray { get; set; }

    void GenerateMap()
    {
        TileArray = new Transform[(int)MapSize.x, (int)MapSize.y];

        for (int x = 0; x < MapSize.x; x++)
        {
            for(int z = 0; z < MapSize.y; z++)
            {
                InstantiateTile(new Vector3(x, 0, z), Vector3.zero);
            }
        }
    }

    void InstantiateTile(Vector3 position, Vector3 positionInfo)
    {
        Transform newTile = Instantiate(
            HexagonTile,
            new Vector3(position.x + (position.z % 2 == 0 ? 0 : HexagonTile.localScale.z / 2) * 10, 0, position.z * HexagonTile.localScale.z * 10), 
            HexagonTile.localRotation) as Transform;

        newTile.name = positionInfo.ToString();

        TileArray[(int)position.x, (int)position.z] = newTile;
    }

    void Init()
    {
        GenerateMap();
    }

    void Start()
    {
        Init();
    }

    void Update()
    {

    }
}
