using UnityEngine;
using System.Collections.Generic;

public enum BuildingType
{
    BaseDefense,
    EngeneeringBay,
    CommandCenter,
    SpaceshipFactory,
    EnergieCore,
    WarpField,
    DroneShipEngineer,
    SuicideSwarmFacility,
    BattleShipEngineer,
    SiegeShipEngineer,
    FTLGenerator,
    ShieldCenter,
    AsteroidCollector,
    TaxCollector,
    MerchantGuild,
    SolarCollector
}

public class BuildingValues : IJSON
{
    public BuildingType BuildingType { get; private set; }

    public BuildingValues(BuildingType buildingType)
    {
        BuildingType = buildingType;
    }

    public BuildingValues()
    {

    }

    public JSONObject ToJSON()
    {
        var jsonObject = JSONObject.obj;
        jsonObject[JSONs.BuildingType] = new JSONObject((int)BuildingType);
        return jsonObject;
    }

    public void FromJSON(JSONObject jsonObject)
    {
        BuildingType = (BuildingType)(int)jsonObject[JSONs.BuildingType];
    }
}

public class Building :IJSON
{
    private int PlayerID { get; set; }
    public BuildingValues BuildingValues { get; private set; }

    private Transform BuildingParent { get; set; }
    public BuildingController BuildingController { get; protected set; }

    public Building(int playerID, BuildingType buildingType)
    {
        PlayerID = playerID;
        BuildingValues = new BuildingValues(buildingType);

        InitiateValues();
    }

    public Building()
    {

    }

    private void InitiateValues()
    {
        var baseo = BaseManager.Get().GetBase(PlayerID);
        BuildingParent = BuildingController.InstatiateParentObject(baseo.GetFreeModulPosition(), BuildingValues.BuildingType.ToString());
        BuildingController = BuildingParent.gameObject.AddComponent<BuildingController>();

        BuildingController.InstantiateBuilding(BuildingValues.BuildingType, PlayerManager.Get().Player.Color);
    }

    public JSONObject ToJSON()
    {
        var jsonObject = JSONObject.obj;

        return jsonObject;
    }

    public void FromJSON(JSONObject o)
    {
        throw new System.NotImplementedException();
    }
}

public class BuildingController : MonoBehaviour
{
    #region Object and Instantiation
    private Transform BuildingObject { get; set; }
    private Color color;
    private Color Color { get { return color; } set { SetColorOfBuilding(value); color = value; } }

    public static Transform InstatiateParentObject(Transform position, string buildingType)
    {
        var buildingParent = new GameObject("Building: " + buildingType).transform;
        buildingParent.position = position.position;
        buildingParent.rotation = position.rotation;
        var baseChilds = new List<Transform>(BaseManager.Get().OwnBase.BaseParent.GetComponentsInChildren<Transform>());
        buildingParent.SetParent(baseChilds.Find(t => t.tag == Tags.Buildings));
        return buildingParent;
    }

    public void InstantiateBuilding(BuildingType type, Color color)
    {
        var baseManagerBuildings = BaseManager.Get().BuildingSettings;

        switch (type)
        {
            case BuildingType.BaseDefense:
                InstantiateBuildingObject(baseManagerBuildings.BaseDefense);
                break;
            case BuildingType.EngeneeringBay:
                InstantiateBuildingObject(baseManagerBuildings.EngeneeringBay);
                break;
            case BuildingType.CommandCenter:
                InstantiateBuildingObject(baseManagerBuildings.CommandCenter);
                break;
            case BuildingType.SpaceshipFactory:

                break;
            case BuildingType.EnergieCore:

                break;
            case BuildingType.WarpField:

                break;
            case BuildingType.DroneShipEngineer:

                break;
            case BuildingType.SuicideSwarmFacility:

                break;
            case BuildingType.BattleShipEngineer:

                break;
            case BuildingType.SiegeShipEngineer:

                break;
            case BuildingType.FTLGenerator:

                break;
            case BuildingType.ShieldCenter:

                break;
            case BuildingType.AsteroidCollector:

                break;
            case BuildingType.TaxCollector:

                break;
            case BuildingType.MerchantGuild:

                break;
            case BuildingType.SolarCollector:

                break;
            default:
                break;
        }

        Color = color;
    }

    private void InstantiateBuildingObject(Transform model)
    {
        // Destroy old
        if (BuildingObject != null)
        {
            Destroy(BuildingObject);
        }

        // Instantiate fleet
        BuildingObject = Instantiate(model, transform.position, transform.rotation) as Transform;

        BuildingObject.name = "Building Object";
        BuildingObject.SetParent(transform);
    }

    private void SetColorOfBuilding(Color color)
    {
        var colorComponentList = new List<Renderer>(BuildingObject.GetComponentsInChildren<Renderer>());
        var colorObjectList = new List<Renderer>(colorComponentList.FindAll(t => t.transform.parent.tag == Tags.PlayerColorObjects));
        foreach (var colorObject in colorObjectList)
        {
            colorObject.renderer.material.color = color;
        }
    }
    #endregion
}
