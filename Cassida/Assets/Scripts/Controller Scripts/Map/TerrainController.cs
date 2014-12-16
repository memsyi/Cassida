using UnityEngine;
using System.Collections;

public class TerrainController : MonoBehaviour
{
    #region Object and Instantiation
    private TerrainType _type;

    public TerrainType Type
    {
        get
        {
            return _type;
        }
        set
        {
            _type = value;
            InstantiateTerrain();
        }
    }

    private Transform TerrainObject { get; set; }

    private MapGenerator MapGenerator { get; set; }

    private void InstantiateTerrain()
    {
        switch (Type)
        {
            case TerrainType.Asteroids:
                InstantiateTerrainObject(MapGenerator.AsteroidsTerrain);
                break;
            case TerrainType.Nebula:
                break;
            case TerrainType.EnergyField:
                break;
            case TerrainType.BlackHole:
                break;
            default:
                break;
        }
    }

    public void InstantiateTerrainObject(Transform model)
    {
        // Instantiate terrain
        TerrainObject = Instantiate(model, transform.position, model.localRotation) as Transform;

        TerrainObject.name = "Terrain: " + Type;
        TerrainObject.SetParent(transform);
    } 
    #endregion

    private void Init()
    {
        MapGenerator = GameObject.FindGameObjectWithTag(Tags.Map).GetComponent<MapGenerator>();
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
