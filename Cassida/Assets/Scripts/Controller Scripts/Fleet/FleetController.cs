﻿using UnityEngine;

public enum FleetType
{
    Slow = 1,
    Fast = 2
}

public class Fleet
{
    private FleetType _fleetType;

    public int ID { get; private set; }

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

    public int MovementPointsLeft { get; set; }

    //public int RotationPosition { get; private set; }

    public PhotonPlayer Player { get; private set; }
    
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

    public Fleet(int id, PhotonPlayer player, Transform fleetParent, FleetType fleetType, Unit[] units)
    {
        // Fleet object must be first, then type and units!
        FleetParent = fleetParent;
        FleetType = fleetType;

        ID = id;
        Player = player;
        Units = units;

        ResetMovementPoints();
    }

    public void MoveFleet(Vector3 target)
    {
        Position = target;

        MovementPointsLeft--;
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

        //RotationPosition += rotationDirection;
        //if (RotationPosition > 5)
        //{
        //    RotationPosition = 0;
        //}
    }

    //public void RotateFleetToPosition(int rotationPosition)
    //{
    //    if (rotationPosition < 0)
    //    {
    //        rotationPosition = 5;
    //    }

    //    for (int i = 0; i <= RotationPosition + rotationPosition; i++)
    //    {
    //        RotateFleet(1);
    //    }

    //    RotationPosition = rotationPosition;
    //}
    public void ResetMovementPoints()
    {
        MovementPointsLeft = FleetType.GetHashCode();
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

    private void SetCorrectType()
    {
        FleetController = FleetParent.gameObject.AddComponent<FleetController>();
        FleetController.Type = FleetType;
    }
}

[RequireComponent(typeof (PhotonView))]
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

    public void InstantiateFleetObject(Transform model)
    {
        // Instantiate fleet
        FleetObject = Instantiate(model, transform.position, transform.rotation) as Transform;

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
