using UnityEngine;

public enum UnitType
{
    Meele,
    Range
}

public class Unit
{
    public Transform UnitParent { get; private set; }
    public UnitValues UnitValues { get; set; }

    public UnitController UnitController { get; private set; }

    public Unit(Transform unitParent, UnitValues unitValues)
    {
        UnitParent = unitParent;
        UnitValues = unitValues;

        SetCorrectType();
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

    private void SetCorrectType()
    {
        UnitController = UnitParent.gameObject.AddComponent<UnitController>();
        UnitController.Type = UnitValues.UnitType;
    }
}

public class UnitValues
{
    public UnitType UnitType { get; set; }
    public int Strength { get; set; }

    public UnitValues(UnitType unitType, int strength)
    {
        UnitType = unitType;
        Strength = strength;
    }
}

public class UnitController : MonoBehaviour
{
    #region Object and Instantiation
    UnitType _type;

    public UnitType Type
    {
        get { return _type; }
        set
        {
            _type = value;
            InstantiateUnit();
        }
    }

    private Transform UnitObject { get; set; }

    private FleetManager FleetManager { get; set; }

    private void InstantiateUnit()
    {
        switch (Type)
        {
            case UnitType.Meele:
                InstantiateUnitObject(FleetManager.UnitSettings.MeeleUnitObject);
                break;
            case UnitType.Range:
                InstantiateUnitObject(FleetManager.UnitSettings.RangeUnitObject);
                break;
            default:
                break;
        }
    }

    public void InstantiateUnitObject(Transform model)
    {
        // Instantiate unit
        UnitObject = Instantiate(model, transform.position, transform.rotation) as Transform;

        UnitObject.name = "Unit Object: " + Type;
        UnitObject.SetParent(transform);
    }
    #endregion

    public void DestroyUnitObject()
    {
        Destroy(UnitObject.gameObject);
        Destroy(this);
    }

    private void Init()
    {
        FleetManager = GameObject.Find(Tags.Manager).GetComponent<FleetManager>();

        if (!FleetManager)
        {
            Debug.LogError("MissedComponents!");
        }
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
