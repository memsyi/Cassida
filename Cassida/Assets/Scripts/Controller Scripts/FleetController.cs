using UnityEngine;

public enum FleetType
{
    Slow,
    Fast
}

public class Fleet
{
    private FleetType _fleet;

    private Vector3 _position;

    public Vector3 Position
    {
        get
        {
            return _position;
        }
        set
        {
            _position = value;
            FleetController.MoveFleet(value);
        }
    }
    public Transform FleetObject { get; private set; }
    public FleetType FleetType
    {
        get
        {
            return _fleet;
        }
        private set
        {
            _fleet = value;
            SetCorrectType();
        }
    }
    public Unit[] Units { get; set; }

    public FleetController FleetController { get; private set; }

    public Fleet(Vector3 position, Transform fleetObject, FleetType fleet, Unit[] units)
    {
        // Fleet object must be first, then type!
        FleetObject = fleetObject;
        FleetType = fleet;

        Position = position;
        Units = units;
    }

    public void MoveFleet(Vector3 target)
    {
        Position = target;
    }
    public void RotateFleet(int rotationDirection)
    {
        FleetController.RotateFleet(rotationDirection);

        var newUnitPositions = Units;

        for (int i = 0; i < newUnitPositions.Length; i++)
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

        Units = newUnitPositions;
    }

    private void SetCorrectType()
    {
        FleetController = FleetObject.gameObject.AddComponent<FleetController>();
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

    private Transform UnitObject { get; set; }

    private FleetManager FleetManager { get;  set; }

    private void InstantiateFleet()
    {
        switch (Type)
        {
            case FleetType.Slow:
                InstantiateFleetObject(FleetManager.Fleet);
                break;
            case FleetType.Fast:
                break;
            default:
                break;
        }
    }

    public void InstantiateFleetObject(Transform model)
    {
        // Instantiate terrain
        UnitObject = Instantiate(model, transform.position, model.localRotation) as Transform;

        UnitObject.name = "Fleet: " + Type;
        UnitObject.SetParent(GameObject.Find(Tags.Fleets).transform);
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
