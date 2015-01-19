using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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
        FleetType = (FleetType)(int)jsonObject[JSONs.FleetType];
    }
}

public class Fleet : IJSON
{
    public int ID { get; private set; }
    public int PlayerID { get; private set; }
    public Position Position { get; private set; }
    private int rotation;
    public int Rotation { get { return rotation; } private set { rotation = value; if (rotation < 0) rotation += 6; if (rotation >= 6) rotation -= 6; } }
    public FleetValues FleetValues { get; private set; }
    public List<Unit> UnitList { get; private set; }

    public Transform FleetParent { get; protected set; }
    public FleetController FleetController { get; protected set; }

    private int MovementPointsLeft { get; set; }
    public bool AllowMovement { get { return MovementPointsLeft > 0; } }
    public bool AllowRotation { get; private set; }

    public Fleet(int id, int player, Position position, FleetValues fleetValues)
    {
        ID = id;
        PlayerID = player;
        Position = position;
        Rotation = 0;
        FleetValues = fleetValues;

        InitiateValues();

        ResetMovementRotationAndAttack();
        AllowRotation = true;
    }

    public Fleet()
    {

    }

    private void InitiateValues()
    {
        var player = PlayerManager.Get().GetPlayer(PlayerID);
        FleetParent = FleetController.InstatiateParentObject(Position, player.Name);
        FleetController = FleetParent.gameObject.AddComponent<FleetController>();
        UnitList = new List<Unit>();

        FleetController.InstantiateFleet(FleetValues.FleetType, player.Color);
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

        FleetController.RotateFleet(rotationTarget);

        Rotation = rotationTarget % 6;
    }

    public void ResetMovementRotationAndAttack() // TODO add as event (set private)
    {
        MovementPointsLeft = (int)FleetValues.FleetType;
        AllowRotation = true;

        foreach (var unit in UnitList)
        {
            if (unit != null)
            {
                unit.AllowAttack = true;
            }
        }
    }

    public Unit FindUnit(int position)
    {
        return UnitList.Find(u => (u.Position + Rotation) % 6 == position);
    }

    public void AttackWithFleet(int enemyFleetID)
    {
        var enemyFleet = FleetManager.Get().FleetList.Find(f => f.ID == enemyFleetID);

        if (enemyFleet == null)
        {
            return;
        }

        var unitPosition = InputManager.Get().GetOwnUnitPosition(Position, enemyFleet.Position);
        var unit = FindUnit(unitPosition);

        if (unit == null)
        {
            return;
        }

        var enemyUnitPosition = unitPosition < 3 ? unitPosition + 3 : unitPosition - 3;
        var enemyUnit = enemyFleet.FindUnit(enemyUnitPosition);

        MovementPointsLeft = 0;
        AllowRotation = false;
        unit.AllowAttack = false;

        // damage to all enemy units
        if (enemyUnit == null)
        {
            for (int i = enemyFleet.UnitList.Count - 1; i >= 0; i--)
            {
                AttackUnitOfFleet(enemyFleet, enemyFleet.UnitList[i], unit.UnitValues.Strength);
            }
        }
        else
        {
            var ownStrength = unit.UnitValues.Strength;
            // damage to own unit
            if (unit.UnitValues.UnitType == enemyUnit.UnitValues.UnitType)
            {
                AttackUnitOfFleet(this, unit, enemyUnit.UnitValues.Strength);
            }

            // damage to enemy unit
            AttackUnitOfFleet(enemyFleet, enemyUnit, ownStrength);
        }
    }

    private void AttackUnitOfFleet(Fleet fleet, Unit attackedUnit, int strength)
    {
        if (fleet == null)
        {
            return;
        }

        fleet.BecomeAttacked(attackedUnit, strength);
    }

    public void BecomeAttacked(Unit attackedUnit, int damage)
    {
        if (attackedUnit == null)
        {
            CheckWhetherFleetIsAlive();
            return;
        }

        attackedUnit.UnitValues.Strength -= damage;

        if (!attackedUnit.CheckWhetherUnitIsAlive())
        {
            UnitList.Remove(attackedUnit);
            CheckWhetherFleetIsAlive();
            return;
        }
    }

    private bool CheckWhetherFleetIsAlive()
    {
        foreach (var unit in UnitList)
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
        jsonObject[JSONs.PlayerID] = new JSONObject(PlayerID);
        jsonObject[JSONs.Position] = Position.ToJSON();
        jsonObject[JSONs.Rotation] = new JSONObject(Rotation);
        jsonObject[JSONs.FleetValues] = FleetValues.ToJSON();
        jsonObject[JSONs.Units] = JSONObject.CreateList(UnitList);
        jsonObject[JSONs.AllowRotation] = new JSONObject(AllowRotation);
        jsonObject[JSONs.MovementPointsLeft] = new JSONObject(MovementPointsLeft);

        return jsonObject;
    }

    public void FromJSON(JSONObject jsonObject)
    {
        ID = (int)jsonObject[JSONs.ID];
        PlayerID = (int)jsonObject[JSONs.PlayerID];
        Position = new Position(jsonObject[JSONs.Position]);
        Rotation = (int)jsonObject[JSONs.Rotation];
        FleetValues = new FleetValues();
        FleetValues.FromJSON(jsonObject[JSONs.FleetValues]);
        AllowRotation = (bool)jsonObject[JSONs.AllowRotation];
        MovementPointsLeft = (int)jsonObject[JSONs.MovementPointsLeft];

        InitiateValues();
        FleetManager.Get().AddFleet(this);
        FleetController.RotateFleet(Rotation);

        UnitList = JSONObject.ReadList<Unit>(jsonObject[JSONs.Units]);
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
        //StartCoroutine("MoveToTarget", TileManager.Get().TileList.Find(t => t.Position == target).TileParent.position);
    }

    public void RotateFleet(int rotationDirection)
    {
        RotationTarget = Quaternion.AngleAxis(rotationDirection * 60f, Vector3.up);
    }

    //IEnumerator MoveToTarget(Vector3 target)
    private void MoveToTarget()
    {
        
        //while (true)
        //{
            if (!Move)
            {
                return;
            }

            transform.position = Vector3.Lerp(transform.position, MovementTarget, Time.deltaTime * 3);

            if (Vector3.Distance(transform.position, MovementTarget) < 0.01f)
            {
                transform.position = MovementTarget;
                Move = false;
                //break;
            }
        //}

        //yield return null;
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
