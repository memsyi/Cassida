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

    private void InstantiateTerrain()
    {
        var mapGenerator = GameObject.FindGameObjectWithTag(Tags.Map).GetComponent<MapGenerator>();

        switch (Type)
        {
            case TerrainType.Asteroids:
                InstantiateTerrainObject(mapGenerator.AsteroidsTerrain);
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

    public void InstantiateTerrainObject(string modelName)
    {
        // Instantiate terrain
        TerrainObject = PhotonNetwork.Instantiate("Terrains/" + modelName, transform.position, Quaternion.identity, 0).transform;

        TerrainObject.name = "Terrain: " + Type;
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
