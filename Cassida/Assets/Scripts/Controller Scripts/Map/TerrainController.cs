using UnityEngine;
using System.Collections;

public class TerrainController : MonoBehaviour
{
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
            InstantiateTerrainObject();
        }
    }

    private void InstantiateTerrainObject()
    {
        switch (Type)
        {
            case TerrainType.Asteroids:
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
