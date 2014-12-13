using UnityEngine;

public enum UnitType
{
    Meele,
    Range
}

public class Unit
{
    private UnitType _unit;
    public int Position { get; private set; }
    public Transform UnitObject { get; private set; }
    public UnitType UnitType { get; private set; }
    public int Strength { get; set; }

    public UnitController UnitController { get; private set; }

    public Unit(int position, Transform unitObject, UnitType unit, int strength)
    {
        Position = position;
        UnitObject = unitObject;
        UnitType = unit;
        Strength = strength;
    }

    private void SetCorrectType()
    {
        UnitController = UnitObject.gameObject.AddComponent<UnitController>();
        UnitController.Type = UnitType;
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

    private void InstantiateUnit()
    {
        switch (Type)
        {
            case UnitType.Meele:
                InstantiateUnitObject(null);
                break;
            case UnitType.Range:
                break;
            default:
                break;
        }
    }

    public void InstantiateUnitObject(Transform model)
    {
        // Instantiate terrain
        UnitObject = Instantiate(model, transform.position, model.localRotation) as Transform;

        UnitObject.name = "Unit: " + Type;
        UnitObject.SetParent(transform);
    } 
    #endregion

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
