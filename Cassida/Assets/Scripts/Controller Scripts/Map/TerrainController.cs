using UnityEngine;
using System.Collections;

public class TerrainController : MonoBehaviour
{
    #region Object and Instantiation
    private Transform TerrainObject { get; set; }

    public void InstantiateTerrain(TerrainType type)
    {
        var mapGenerator = MapGenerator.Get();

        switch (type)
        {
            case TerrainType.Asteroids:
                InstantiateTerrainObject(mapGenerator.AsteroidsTerrain, type);
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

    private void InstantiateTerrainObject(Transform model, TerrainType type)
    {
        // Instantiate terrain
        TerrainObject = Instantiate(model, transform.position, Quaternion.identity) as Transform;

        TerrainObject.name = "Terrain: " + type;
        TerrainObject.SetParent(transform);
    }
    #endregion

    private void Init()
    {

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
