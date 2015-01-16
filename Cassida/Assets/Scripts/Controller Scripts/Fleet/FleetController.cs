using UnityEngine;
using System.Collections.Generic;

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

    public FleetValues()
    {

    }

    public JSONObject ToJSON()
    {
        var jsonObject = JSONObject.obj;
        jsonObject[JSONs.FleetType] = new JSONObject((int)FleetType);
        return jsonObject;
    }

    public void FromJSON(JSONObject jsonObject)
    {
        var o = jsonObject[JSONs.FleetType];
        int i = (int)o;
        FleetType = (FleetType)i;
        FleetType = (FleetType)(int)jsonObject[JSONs.FleetType];
    }
}

public class Fleet : IJSON
{
    public int ID { get; private set; }
    public Player Player { get; private set; }
    public Position Position { get; private set; }
    public int Rotation { get; private set; }
    public FleetValues FleetValues { get; private set; }
    public List<Unit> Units { get; private set; } // TODO set private

    public Transform FleetParent { get; protected set; }
    public FleetController FleetController { get; protected set; }

    private int MovementPointsLeft { get; set; }
    public bool AllowMovement { get { return MovementPointsLeft > 0; } }
    public bool AllowRotation { get; private set; }

    public Fleet(int id, Player player, Position position, FleetValues fleetValues)
    {
        ID = id;
        Player = player;
        Position = position;
        Rotation = 0;
        FleetValues = fleetValues;

        InitiateValues();
    }

    public Fleet()
    {

    }

    private void InitiateValues()//int id, Player player, Position position, FleetValues fleetValues)
    {
        // Parent object and controller must be first!
        FleetParent = FleetController.InstatiateParentObject(Position, Player.Name);
        FleetController = FleetParent.gameObject.AddComponent<FleetController>();
        Units = new List<Unit>();

        ResetMovementRotationAndAttack();
        AllowRotation = true;

        FleetController.InstantiateFleet(FleetValues.FleetType, Player.Color);
    }

    public void MoveFleet(Position target)
    {
        if (!AllowMovement) { return; }

        FleetController.MoveFleet(target);

        MovementPointsLeft--;

        Position = target;
    }
    public void RotateFleet(int rotationTarget)
    {
        if (!AllowRotation) { return; }

        var rotation = rotationTarget - Rotation;

        FleetController.RotateFleet(rotationTarget);

        foreach (var unit in Units)
        {
            unit.Position += rotation;
        }

        Rotation = rotationTarget % 6;
    }

    public void ResetMovementRotationAndAttack() // TODO add as event (set private)
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

    public void AttackWithFleet(int enemyFleetID)
    {
        var enemyFleet = FleetManager.Get().FleetList.Find(f => f.ID == enemyFleetID);

        if (enemyFleet == null)
        {
            return;
        }

        var unitPosition = InputManager.Get().GetOwnUnitPosition(Position, enemyFleet.Position);
        var unit = Units[unitPosition];

        if (unit == null)
        {
            return;
        }

        var enemyUnitPosition = unitPosition < 3 ? unitPosition + 3 : unitPosition - 3;
        var enemyUnit = enemyFleet.Units[enemyUnitPosition];


        MovementPointsLeft = 0;
        AllowRotation = false;
        unit.AllowAttack = false;

        // damage to all enemy units
        if (enemyUnit == null)
        {
            for (int i = 0; i < enemyFleet.Units.Count; i++)
            {
                AttackUnitOfFleet(enemyFleet, i, unit.UnitValues.Strength);
            }
        }
        else
        {
            var ownStrength = unit.UnitValues.Strength;
            // damage to own unit
            if (unit.UnitValues.UnitType == enemyUnit.UnitValues.UnitType)
            {
                AttackUnitOfFleet(this, unitPosition, enemyUnit.UnitValues.Strength);
            }

            // damage to enemy unit
            AttackUnitOfFleet(enemyFleet, enemyUnitPosition, ownStrength);
        }
    }

    private void AttackUnitOfFleet(Fleet fleet, int unitPosition, int strength)
    {
        if (fleet == null)
        {
            return;
        }

        fleet.BecomeAttacked(unitPosition, strength);
    }

    public void BecomeAttacked(int unitPosition, int damage)
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
            if (unit != null) { return true; }
        }

        FleetController.DestroyFleet(this);
        return false;
    }

    public JSONObject ToJSON()
    {
        var jsonObject = JSONObject.obj;
        jsonObject[JSONs.ID] = new JSONObject(ID);
        jsonObject[JSONs.PlayerID] = new JSONObject(Player.ID);
        jsonObject[JSONs.Position] = Position.ToJSON();
        jsonObject[JSONs.Rotation] = new JSONObject(Rotation);
        jsonObject[JSONs.FleetValues] = FleetValues.ToJSON();
        jsonObject[JSONs.Units] = JSONObject.CreateList(Units);
        jsonObject[JSONs.AllowRotation] = new JSONObject(AllowRotation);
        jsonObject[JSONs.MovementPointsLeft] = new JSONObject(MovementPointsLeft);

        return jsonObject;
    }

    public void FromJSON(JSONObject jsonObject)
    {
        ID = (int)jsonObject[JSONs.ID];
        Player = PlayerManager.Get().GetPlayer((int)jsonObject[JSONs.PlayerID]);
        Position = new Position(jsonObject[JSONs.Position]);
        Rotation = (int)jsonObject[JSONs.Rotation];
        FleetValues = new FleetValues();
        FleetValues.FromJSON(jsonObject[JSONs.FleetValues]);
        AllowRotation = (bool)jsonObject[JSONs.AllowRotation];
        MovementPointsLeft = (int)jsonObject[JSONs.MovementPointsLeft];

        InitiateValues();
        FleetManager.Get().AddFleet(this);

        Units = JSONObject.ReadList<Unit>(jsonObject[JSONs.Units]);
    }
}

public class FleetController : MonoBehaviour
{
    #region Object and Instantiation
    private Transform FleetObject { get; set; }
    private Color color;
    private Color Color { get { return color; } set { SetColorOfFleet(value); color = value; } }

    public static Transform InstatiateParentObject(Position position, string playerName)
    {
        var fleetParent = new GameObject("Fleet of: " + playerName).transform;
        fleetParent.position = TileManager.Get().TileList.Find(t => t.Position == position).TileParent.position;
        fleetParent.SetParent(GameObject.Find(Tags.Fleets).transform);
        return fleetParent;
    }

    public void InstantiateFleet(FleetType type, Color color)
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

        Color = color;
    }

    private void InstantiateFleetObject(Transform model, FleetType type)
    {
        // Destroy old
        if (FleetObject != null)
        {
            Destroy(FleetObject);
        }

        // Instantiate fleet
        FleetObject = Instantiate(model, transform.position, transform.rotation) as Transform;

        FleetObject.name = "Fleet Object: " + type;
        FleetObject.SetParent(transform);
    }

    private void SetColorOfFleet(Color color)
    {
        var colorComponentList = new List<Renderer>(FleetObject.GetComponentsInChildren<Renderer>());
        var colorObjectList = new List<Renderer>(colorComponentList.FindAll(t => t.transform.parent.tag == Tags.PlayerColorObjects));
        foreach (var colorObject in colorObjectList)
        {
            colorObject.renderer.material.color = color;
        }
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
            _rotationTarget.eulerAngles = value.eulerAngles;
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
