using UnityEngine;

public enum UnitType
{
    Meele,
    Range
}

public class UnitValues
{
    public UnitType UnitType { get; private set; }
    public int Strength { get; set; }

    public UnitValues(UnitType unitType, int strength)
    {
        UnitType = unitType;
        Strength = strength;
    }
}

public class Unit
{
    public Player Player { get; private set; }
    public UnitValues UnitValues { get; set; }

    public Transform UnitParent { get; private set; }
    public UnitController UnitController { get; private set; }

    public bool AllowAttack { get; set; }

    public Unit(Player player, UnitValues unitValues, Transform unitParent)
    {
        // Fleet object and controller must be first!
        UnitParent = unitParent;
        UnitController = UnitParent.gameObject.AddComponent<UnitController>();

        Player = player;
        UnitValues = unitValues;
        AllowAttack = true;

        UnitController.InstantiateUnit(unitValues.UnitType);
        UnitController.SetColorOfUnit(player.Color);
    }

    public bool CheckWhetherUnitIsAlive()
    {
        if (UnitValues.Strength > 0)
        {
            return true;
        }

        UnitController.DestroyUnitObject();
        return false;
    }
}

public class UnitController : MonoBehaviour
{
    #region Object and Instantiation
    private Transform UnitObject { get; set; }

    public void InstantiateUnit(UnitType type)
    {
        var fleetManager = FleetManager.Get();

        switch (type)
        {
            case UnitType.Meele:
                InstantiateUnitObject(fleetManager.UnitSettings.MeeleUnitObject, type);
                break;
            case UnitType.Range:
                InstantiateUnitObject(fleetManager.UnitSettings.RangeUnitObject, type);
                break;
            default:
                break;
        }
    }

    public void InstantiateUnitObject(Transform model, UnitType type)
    {
        // Instantiate unit
        UnitObject = Instantiate(model, transform.position, transform.rotation) as Transform;

        UnitObject.name = "Unit Object: " + type;
        UnitObject.SetParent(transform);
    }

    public void SetColorOfUnit(Color color)
    {
        UnitObject.GetChild(0).renderer.material.color = color;
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
