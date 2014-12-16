using UnityEngine;

public enum FleetType
{
    Slow,
    Fast
}

public class Fleet
{
    private FleetType _fleetType;

    public Vector3 Position
    {
        get
        {
            return FleetParent.position;
        }
        set
        {
            FleetController.MoveFleet(value);
        }
    }
    public Transform FleetParent { get; private set; }
    public FleetType FleetType
    {
        get
        {
            return _fleetType;
        }
        private set
        {
            _fleetType = value;
            SetCorrectType();
        }
    }
    public Unit[] Units { get; set; }

    public FleetController FleetController { get; private set; }

    public Fleet(Transform fleetParent, FleetType fleetType, Unit[] units)
    {
        // Fleet object must be first, then type and units!
        FleetParent = fleetParent;
        FleetType = fleetType;

        Units = units;
    }

    public void MoveFleet(Vector3 target)
    {
        Position = target;
    }
    public void RotateFleet(int rotationDirection)
    {
        FleetController.RotateFleet(rotationDirection);

        var newUnitPositions = new Unit[6];

        for (int i = 0; i < newUnitPositions.Length; i++)
        {
            if (rotationDirection < 0)
            {
                if (i < 5)
                {
                    newUnitPositions[i] = Units[i + 1];
                }
                else
                {
                    newUnitPositions[5] = Units[0];
                }
            }
            else
            {
                if (i > 0)
                {
                    newUnitPositions[i] = Units[i - 1];
                }
                else
                {
                    newUnitPositions[0] = Units[5];
                }
            }
        }

        Units = newUnitPositions;

        //foreach (var unit in Units)
        //{
        //    if (unit != null)
        //        Debug.Log(unit.Position);
        //}
    }

    public void AttackUnit(int unitPosition, int damage)
    {
        var attackedUnit = Units[unitPosition];

        if (attackedUnit == null)
        {
            return;
        }

        attackedUnit.UnitValues.Strength -= damage;

        if (!attackedUnit.CheckWhetherUnitIsAlive())
        {
            Units[unitPosition] = null;
            return;
        }
    }

    public bool CheckWhetherFleetIsAlive()
    {
        foreach (var unit in Units)
        {
            if (unit != null)
            {
                return true;
            }
        }

        FleetController.DestroyFleetObject(this);
        return false;
    }

    private void SetCorrectType()
    {
        FleetController = FleetParent.gameObject.AddComponent<FleetController>();
        FleetController.Type = FleetType;
    }
}

public class FleetController : MonoBehaviour
{
    #region Object and Instantiation
    FleetType _type;

    public FleetType Type
    {
        get { return _type; }
        set
        {
            _type = value;
            InstantiateFleet();
        }
    }

    private Transform FleetObject { get; set; }

    private FleetManager FleetManager { get; set; }

    private void InstantiateFleet()
    {
        switch (Type)
        {
            case FleetType.Slow:
                InstantiateFleetObject(FleetManager.FleetSettings.SlowFleetObject);
                break;
            case FleetType.Fast:
                InstantiateFleetObject(FleetManager.FleetSettings.FastFleetObject);
                break;
            default:
                break;
        }
    }

    public void InstantiateFleetObject(Transform model)
    {
        // Instantiate fleet
        FleetObject = Instantiate(model, transform.position, model.localRotation) as Transform;

        FleetObject.name = "Fleet Object: " + Type;
        FleetObject.SetParent(transform);
    }
    #endregion

    #region Movement and Rotation
    private Vector3 _movementTarget;

    private Vector3 MovementTarget
    {
        get { return _movementTarget; }
        set
        {
            _movementTarget = value;
            Move = true;
        }
    }

    private Quaternion _rotationTarget;

    private Quaternion RotationTarget
    {
        get { return _rotationTarget; }
        set
        {
            _rotationTarget.eulerAngles += value.eulerAngles;
            Turn = true;
        }
    }

    private bool Move { get; set; }
    private bool Turn { get; set; }

    public void MoveFleet(Vector3 target)
    {
        MovementTarget = target;
    }

    public void RotateFleet(int rotationDirection)
    {
        RotationTarget = Quaternion.AngleAxis(rotationDirection * 60f, Vector3.up);
    }

    private void MoveToTarget()
    {
        if (!Move)
        {
            return;
        }

        transform.position = Vector3.Lerp(transform.position, MovementTarget, Time.deltaTime * 3);

        if (Vector3.Distance(transform.position, MovementTarget) < 0.01f)
        {
            transform.position = MovementTarget;
            Move = false;
        }
    }

    private void RotateToTarget()
    {
        if (!Turn)
        {
            return;
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, RotationTarget, Time.deltaTime * 3);

        if (Quaternion.Angle(transform.rotation, RotationTarget) < 0.01f)
        {
            transform.rotation = RotationTarget;
            Turn = false;
        }
    }
    #endregion

    public void DestroyFleetObject(Fleet fleet)
    {
        Destroy(this.gameObject);
        FleetManager.FleetList.Remove(fleet);
    }

    private void Init()
    {
        FleetManager = GameObject.Find(Tags.Manager).GetComponent<FleetManager>();
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
        MoveToTarget();
        RotateToTarget();
    }
}
