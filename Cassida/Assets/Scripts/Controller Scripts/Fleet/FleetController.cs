using UnityEngine;

public enum FleetType
{
    Slow = 1,
    Fast = 2
}

public class FleetValues : IJSON
{
    public FleetType FleetType { get; private set; }

    public FleetValues(FleetType fleetType)
    {
        FleetType = fleetType;
    }

    public JSONObject ToJSON()
    {
        throw new System.NotImplementedException();
    }

    public void FromJSON(JSONObject o)
    {
        throw new System.NotImplementedException();
    }
}

public class Fleet : IJSON
{
    public int ID { get; private set; }
    public Player Player { get; private set; }
    public Position Position { get; private set; }
    public int Rotation { get; private set; }
    public FleetValues FleetValues { get; private set; }
    public Unit[] Units { get; private set; }

    public Transform FleetParent { get; private set; }
    public FleetController FleetController { get; private set; }

    public int MovementPointsLeft { get; set; }
    public bool AllowRotation { get; set; }

    public Fleet(int id, Player player, Position position, FleetValues fleetValues)
    {
        // Fleet object and controller must be first!
        var fleetParent = new GameObject("Fleet of: " + player.Name).transform;
        fleetParent.position = TileManager.Get().TileList.Find(t => t.Position == position).TileParent.position;
        fleetParent.SetParent(GameObject.Find(Tags.Fleets).transform);

        FleetParent = fleetParent;
        FleetController = FleetParent.gameObject.AddComponent<FleetController>();
        Units = new Unit[6];

        ID = id;
        Player = player;
        Position = position;
        Rotation = 0;
        FleetValues = fleetValues;

        ResetMovementRotationAndAttack();
        AllowRotation = true;

        FleetController.InstantiateFleet(fleetValues.FleetType);
        FleetController.SetColorOfFleet(player.Color);
    }

    public void MoveFleet(Position target)
    {
        if (MovementPointsLeft <= 0)
        {
            return;
        }

        FleetController.MoveFleet(target);

        MovementPointsLeft--;

        Position = target;
    }
    public void RotateFleet(int rotationTarget)
    {
        if (!AllowRotation)
        {
            return;
        }

        var rotationCount = Mathf.Abs(Rotation - rotationTarget);
        var rotationDirection = (int)Mathf.Sign(Rotation - rotationTarget);

        for (int r = 0; r < rotationCount; r++)
        {
            FleetController.RotateFleet(rotationDirection);

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

        Rotation = rotationTarget;
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

    public JSONObject ToJSON()
    {
        var jsonObject = JSONObject.obj;

        jsonObject[JSONs.ID] = new JSONObject(ID);
        //jsonObject[JSONs.Player] = new JSONObject(Player.ToJSON());
        //jsonObject[JSONs.Position] = new JSONObject(Position.ToJSON());
        jsonObject[JSONs.Rotation] = new JSONObject(Rotation);
        //jsonObject[JSONs.FleetValues] = new JSONObject(FleetValues.ToJSON());
        //jsonObject[JSONs.Units] = new JSONObject(Unit.ToJSON());

        return jsonObject;
    }

    public void FromJSON(JSONObject jsonObject)
    {
        throw new System.NotImplementedException();
    }
}

public class FleetController : MonoBehaviour
{
    #region Object and Instantiation
    private Transform FleetObject { get; set; }

    public void InstantiateFleet(FleetType type)
    {
        var fleetManager = FleetManager.Get();

        switch (type)
        {
            case FleetType.Slow:
                InstantiateFleetObject(fleetManager.FleetSettings.SlowFleetObject, type);
                break;
            case FleetType.Fast:
                InstantiateFleetObject(fleetManager.FleetSettings.FastFleetObject, type);
                break;
            default:
                break;
        }
    }

    private void InstantiateFleetObject(Transform model, FleetType type)
    {
        // Instantiate fleet
        FleetObject = Instantiate(model, transform.position, transform.rotation) as Transform;

        FleetObject.name = "Fleet Object: " + type;
        FleetObject.SetParent(transform);
    }

    public void SetColorOfFleet(Color color)
    {
        FleetObject.GetChild(0).renderer.material.color = color;
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

    public void MoveFleet(Position target)
    {
        MovementTarget = TileManager.Get().TileList.Find(t => t.Position == target).TileParent.position;
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
        FleetManager.Get().DestroyFleet(fleet);
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
        MoveToTarget();
        RotateToTarget();
    }
}
