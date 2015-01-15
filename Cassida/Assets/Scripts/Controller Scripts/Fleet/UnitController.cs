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

    public JSONObject ToJSON()
    {
        var jsonObject = JSONObject.obj;
        jsonObject[JSONs.UnitType] = new JSONObject((int)UnitType);
        jsonObject[JSONs.Strength] = new JSONObject(Strength);
        return jsonObject;
    }

    public void FromJSON(JSONObject jsonObject)
    {
        var unitType = (UnitType)(int)jsonObject[JSONs.UnitType];
        var strength = (int)jsonObject[JSONs.Strength];
    }
}

public class Unit : IJSON
{
    private UnitValues unitValues;
    public UnitValues UnitValues
    {
        get { return unitValues; }
        set
        {
            if (value == null) { return; }

            UnitController.InstantiateUnit(value.UnitType, value.Strength, FleetManager.Get().FleetList.Find(f => f.ID == FleetID).Player.Color);
            unitValues = value;
        }
    }

    private int FleetID { get; set; }
    public Transform UnitParent { get; protected set; }
    public UnitController UnitController { get; protected set; }

    public bool AllowAttack { get; set; }

    public Unit(int fleetID, int position, UnitValues unitValues = null)
    {
        FleetID = fleetID;
        UnitParent = UnitController.InstatiateParentObject(fleetID, position);
        UnitController = UnitParent.gameObject.AddComponent<UnitController>(); // TODO change controller to specific unit controller

        UnitValues = unitValues;
        AllowAttack = true;
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
        jsonObject[JSONs.UnitValues] = UnitValues.ToJSON();
        jsonObject[JSONs.FleetID] = new JSONObject(FleetID);
        jsonObject[JSONs.AllowAttack] = new JSONObject(AllowAttack);
        return jsonObject;
    }

    public void FromJSON(JSONObject jsonObject)
    {
        UnitValues.FromJSON(jsonObject[JSONs.UnitValues]);
        AllowAttack = jsonObject[JSONs.AllowAttack];
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
        var fleetParent = FleetManager.Get().FleetList.Find(f => f.ID == fleetID).FleetParent;

        if (fleetParent == null) { return null; }

        fleetParent.rotation = Quaternion.Euler(Vector3.up * (position * -60 - 30));

        var unitParent = new GameObject("Unit: " + position).transform;
        unitParent.position = fleetParent.position + Vector3.forward * 0.6f;
        unitParent.SetParent(fleetParent);

        fleetParent.rotation = Quaternion.identity;

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
