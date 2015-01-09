using UnityEngine;

public enum FleetType
{
    Slow = 1,
    Fast = 2
}

public class FleetValues
{
    public FleetType FleetType { get; private set; }

    public FleetValues(FleetType fleetType)
    {
        FleetType = fleetType;
    }
}

public class Fleet// : IJSON
{
    public int ID { get; private set; }
    public Player Player { get; private set; }
    public Vector2 Position { get; private set; }
    public FleetValues FleetValues { get; private set; }

    public Transform FleetParent { get; private set; }
    public FleetController FleetController { get; private set; }
    public Unit[] Units { get; private set; }

    public int MovementPointsLeft { get; set; }
    public bool AllowRotation { get; set; }

    //public PhotonPlayer Player
    //{
    //    get { return _player; }
    //    private set
    //    {
    //        _player = value;

    //        var color = (Vector3)value.customProperties[PlayerProperties.Color];
    //        FleetController.Color = new Color(color.x, color.y, color.z);
    //    }
    //}

    //public Vector3 Position
    //{
    //    get
    //    {
    //        return FleetParent.position;
    //    }
    //    set
    //    {
    //        FleetController.MoveFleet(value);
    //    }
    //}

    //public FleetType FleetType
    //{
    //    get
    //    {
    //        return _fleetType;
    //    }
    //    private set
    //    {
    //        _fleetType = value;
    //        FleetController.Type = FleetType;
    //    }
    //}

    public Fleet(int id, Player player, Vector2 position, FleetValues fleetValues, Transform fleetParent)//, Unit[] units)
    {
        // Fleet object and controller must be first!
        FleetParent = fleetParent;
        FleetController = FleetParent.gameObject.AddComponent<FleetController>();
        Units = new Unit[6];

        ID = id;
        Player = player;
        FleetValues = fleetValues; // Flotte instattiieren!!!

        ResetMovementRotationAndAttack();
        AllowRotation = true;
    }

    public void MoveFleet(Vector2 target)
    {
        if (MovementPointsLeft == 0)
        {
            return;
        }

        Position = target;
        // TODO movement an Controller weitergeben

        MovementPointsLeft--;
    }
    public void RotateFleet(int rotationDirection)
    {
        if (!AllowRotation)
        {
            return;
        }

        FleetController.RotateFleet(rotationDirection);

        {
            var newUnitPositions = new Unit[6];
            for (int i = 0; i < newUnitPositions.Length; i++)
            {
                if (rotationDirection < 0)
                {
                    if (i < 5) { newUnitPositions[i] = Units[i + 1]; }
                    else { newUnitPositions[5] = Units[0]; }
                }
                else
                {
                    if (i > 0) { newUnitPositions[i] = Units[i - 1]; }
                    else { newUnitPositions[0] = Units[5]; }
                }
            }
            Units = newUnitPositions;
        }
    }

    public void ResetMovementRotationAndAttack()
    {
        MovementPointsLeft = (int)FleetValues.FleetType;
        AllowRotation = true;

        foreach (var unit in Units)
        {
            if (unit != null)
            {
                unit.AllowAttack = true;
            }
        }
    }

    public void AttackUnit(int unitPosition, int damage)
    {
        var attackedUnit = Units[unitPosition];

        if (attackedUnit == null)
        {
            CheckWhetherFleetIsAlive();
            return;
        }

        attackedUnit.UnitValues.Strength -= damage;

        if (!attackedUnit.CheckWhetherUnitIsAlive())
        {
            Units[unitPosition] = null;
            CheckWhetherFleetIsAlive();
            return;
        }
    }

    private bool CheckWhetherFleetIsAlive()
    {
        foreach (var unit in Units)
        {
            if (unit != null)
            {
                return true;
            }
        }

        FleetController.DestroyFleet(this);
        return false;
    }

    //public JSONObject ToJSON()
    //{
    //    var o = JSONObject.obj;

    //    o["ID"] = new JSONObject(ID);
    //    o["Player"] = new JSONObject(Player.ToJSON());
    //    o["Position"] = Position.ToJSON();
    //    o["Type"] = new JSONObject((int)FleetType);
        
    //    return o;
    //}

    //public void FromJSON(JSONObject o)
    //{
    //    throw new System.NotImplementedException();
    //}
}

public class FleetController : MonoBehaviour
{
    #region Object and Instantiation
    private FleetType _type;

    public FleetType Type
    {
        get { return _type; }
        set
        {
            _type = value;
            InstantiateFleet();
        }
    }

    private Color _color;

    public Color Color
    {
        get { return _color; }
        set
        {
            _color = value;
            SetColorOfFleet();
        }
    }

    private Transform FleetObject { get; set; }

    // Scripts
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

    private void InstantiateFleetObject(Transform model)
    {
        // Instantiate fleet
        FleetObject = Instantiate(model, transform.position, transform.rotation) as Transform;

        FleetObject.name = "Fleet Object: " + Type;
        FleetObject.SetParent(transform);
    }

    private void SetColorOfFleet()
    {
        FleetObject.GetChild(0).renderer.material.color = Color;
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

    public void DestroyFleet(Fleet fleet)
    {
        FleetManager.DestroyFleet(fleet);
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
        MoveToTarget();
        RotateToTarget();
    }
}
