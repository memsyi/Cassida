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
    private Player _player;
    public Player Player
    {
        get { return _player; }
        private set
        {
            _player = value;
            UnitController.Color = value.Color;
        }
    }
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
        UnitValues = unitValues; // Values befor type

        // Flotte instattiieren!!!

        //UnitController.Type = UnitValues.UnitType; // Type befor player
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

    private Color _color;

    public Color Color
    {
        get { return _color; }
        set
        {
            _color = value;
            SetColorOfUnit();
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

    private void SetColorOfUnit()
    {
        UnitObject.GetChild(0).renderer.material.color = Color;
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
