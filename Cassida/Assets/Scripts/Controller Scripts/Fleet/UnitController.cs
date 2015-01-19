using UnityEngine;
using System.Collections.Generic;

public enum UnitType
{
    Meele,
    Range
}

public class UnitValues : IJSON
{
    public UnitType UnitType { get; private set; }
    public int Strength { get; set; }

    public UnitValues(UnitType unitType, int strength)
    {
        UnitType = unitType;
        Strength = strength;
    }

    public UnitValues()
    {

    }

    public JSONObject ToJSON()
    {
        var jsonObject = JSONObject.obj;
        jsonObject[JSONs.UnitType] = new JSONObject((int)UnitType);
        jsonObject[JSONs.Strength] = new JSONObject(Strength);
        return jsonObject;
    }

    public void FromJSON(JSONObject jsonObject)
    {
        UnitType = (UnitType)(int)jsonObject[JSONs.UnitType];
        Strength = (int)jsonObject[JSONs.Strength];
    }
}

public class Unit : IJSON
{
    public UnitValues UnitValues { get; set; }
    private int FleetID { get; set; }
    private int position = 0;
    public int Position { get { return position; } set { position = value; if (position < 0) position += 6; if (position >= 6) position -= 6; } }
    public Transform UnitParent { get; protected set; }
    public UnitController UnitController { get; protected set; }

    public bool AllowAttack { get; set; }

    public Unit(int fleetID, int position, UnitValues unitValues)
    {
        FleetID = fleetID;
        Position = position;
        UnitValues = unitValues;
        AllowAttack = true;

        InitiateValues();
    }

    public Unit()
    {

    }

    private void InitiateValues()
    {
        var fleet = FleetManager.Get().GetFleet(FleetID);
        var player = PlayerManager.Get().GetPlayer(fleet.PlayerID);
        UnitParent = UnitController.InstatiateParentObject(FleetID, Position);
        UnitController = UnitParent.gameObject.AddComponent<UnitController>(); // TODO change controller to specific unit controller

        UnitController.InstantiateUnit(UnitValues.UnitType, UnitValues.Strength, player.Color);
    }

    public bool CheckWhetherUnitIsAlive()
    {
        if (UnitValues.Strength > 0) { return true; }

        UnitController.DestroyUnitObject();
        return false;
    }

    public JSONObject ToJSON()
    {
        var jsonObject = JSONObject.obj;
        jsonObject[JSONs.FleetID] = new JSONObject(FleetID);
        jsonObject[JSONs.Position] = new JSONObject(Position);
        jsonObject[JSONs.UnitValues] = UnitValues.ToJSON();
        jsonObject[JSONs.AllowAttack] = new JSONObject(AllowAttack);
        return jsonObject;
    }

    public void FromJSON(JSONObject jsonObject)
    {
        FleetID = (int)jsonObject[JSONs.FleetID];
        Position = (int)jsonObject[JSONs.Position];
        UnitValues = new UnitValues();
        UnitValues.FromJSON(jsonObject[JSONs.UnitValues]);
        AllowAttack = (bool)jsonObject[JSONs.AllowAttack];
        InitiateValues();
    }
}

public class UnitController : MonoBehaviour
{
    #region Object and Instantiation
    private Transform UnitObject { get; set; }
    private Color color;
    private Color Color { get { return color; } set { SetColorOfUnit(value); color = value; } }

    public static Transform InstatiateParentObject(int fleetID, int position)
    {
        var fleetParent = FleetManager.Get().GetFleet(fleetID).FleetParent;

        if (fleetParent == null) { return null; }

        var unitParent = new GameObject("Unit: " + position).transform;
        unitParent.SetParent(fleetParent);
        var rotation = Quaternion.Euler(Vector3.up * (position * 60 + 30));
        unitParent.localPosition = rotation * Vector3.forward * 0.6f;
        unitParent.localRotation = rotation;

        return unitParent;
    }

    public void InstantiateUnit(UnitType type, int strength, Color color)
    {
        var unitSettings = FleetManager.Get().UnitSettings;

        switch (type)
        {
            case UnitType.Meele:
                switch (strength)
                {
                    case 1:
                        InstantiateUnitObject(unitSettings.MeeleUnitOneObject, type);
                        break;
                    case 2:
                        InstantiateUnitObject(unitSettings.MeeleUnitTwoObject, type);
                        break;
                    case 3:
                        InstantiateUnitObject(unitSettings.MeeleUnitThreeObject, type);
                        break;
                    default:
                        break;
                }
                break;
            case UnitType.Range:
                switch (strength)
                {
                    case 1:
                        InstantiateUnitObject(unitSettings.RangeUnitOneObject, type);
                        break;
                    case 2:
                        InstantiateUnitObject(unitSettings.RangeUnitTwoObject, type);
                        break;
                    case 3:
                        InstantiateUnitObject(unitSettings.RangeUnitThreeObject, type);
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }

        Color = color;
    }

    private void InstantiateUnitObject(Transform model, UnitType type)
    {
        // Destroy old
        if (UnitObject != null)
        {
            Destroy(UnitObject);
        }

        // Instantiate unit
        UnitObject = Instantiate(model, transform.position, transform.rotation) as Transform;

        UnitObject.name = "Unit Object: " + type;
        UnitObject.SetParent(transform);
    }

    private void SetColorOfUnit(Color color)
    {
        var colorComponentList = new List<Renderer>(UnitObject.GetComponentsInChildren<Renderer>());
        var colorObjectList = new List<Renderer>(colorComponentList.FindAll(t => t.transform.parent.tag == Tags.PlayerColorObjects));
        foreach (var colorObject in colorObjectList)
        {
            colorObject.renderer.material.color = color;
        }
    }
    #endregion

    public void DestroyUnitObject()
    {
        Destroy(UnitObject.gameObject);
        Destroy(this);
    }

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
