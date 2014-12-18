using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct FleetSettings
{
    [SerializeField]
    private Transform
        _slowFleetObject,
        _fastFleetObject;

    public Transform SlowFleetObject
    {
        get { return _slowFleetObject; }
        private set { _slowFleetObject = value; }
    }

    public Transform FastFleetObject
    {
        get { return _fastFleetObject; }
        set { _fastFleetObject = value; }
    }
}

[System.Serializable]
public struct UnitSettings
{
    [SerializeField]
    private Transform
        _meeleUnitObject,
        _rangeUnitObject;

    public Transform MeeleUnitObject
    {
        get { return _meeleUnitObject; }
        private set { _meeleUnitObject = value; }
    }

    public Transform RangeUnitObject
    {
        get { return _rangeUnitObject; }
        private set { _rangeUnitObject = value; }
    }
}

public struct BufferedFleet
{
    public Vector2 Position { get; set; }
    public FleetType FleetType { get; set; }
    public PhotonPlayer Player { get; set; }

    public BufferedFleet(Vector2 position, FleetType fleetType, PhotonPlayer player)
    {
        Position = position;
        FleetType = fleetType;
        Player = player;
    }
}

public struct BufferedUnit
{
    public Vector2 FleetPosition { get; set; }
    public int Position { get; set; }
    public UnitType UnitType { get; set; }
    public int Strength { get; set; }

    public BufferedUnit(Vector2 fleetPosition, int position, UnitType unitType, int strength)
    {
        FleetPosition = fleetPosition;
        Position = position;
        UnitType = unitType;
        Strength = strength;
    }
}

[RequireComponent(typeof(PhotonView))]
public class FleetManager : Photon.MonoBehaviour
{
    [SerializeField]
    private FleetSettings _fleetSettings;

    [SerializeField]
    private UnitSettings _unitSettgins;

    public UnitSettings UnitSettings
    {
        get { return _unitSettgins; }
        set { _unitSettgins = value; }
    }

    public FleetSettings FleetSettings
    {
        get { return _fleetSettings; }
        set { _fleetSettings = value; }
    }

    public List<Fleet> FleetList { get; private set; }

    private List<BufferedFleet> BufferedFleetList { get; set; }
    private List<BufferedUnit> BufferedUnitList { get; set; }

    private WorldManager WorldManager { get; set; }

    private void InstantiateNewFleet(Vector2 position, FleetType fleetType, UnitValues[] unitValues)
    {
        photonView.RPC("AddFleetToWorldOverNetwork", PhotonTargets.AllBuffered, position, fleetType.GetHashCode(), PhotonNetwork.player);

        for (int i = 0; i < unitValues.Length; i++)
        {
            if (unitValues[i] != null)
            {
                photonView.RPC("AddUnitToFleetOverNetwork", PhotonTargets.AllBuffered, position, i, unitValues[i].UnitType.GetHashCode(), unitValues[i].Strength);
            }
        }
    }

    [RPC]
    private void AddFleetToWorldOverNetwork(Vector2 position, int fleetType, PhotonPlayer player)
    {
        BufferedFleetList.Add(new BufferedFleet(position, (FleetType)fleetType, player));
    }

    [RPC]
    private void AddUnitToFleetOverNetwork(Vector2 fleetPosition, int position, int unitType, int strength)
    {
        BufferedUnitList.Add(new BufferedUnit(fleetPosition, position, (UnitType)unitType, strength));
    }

    public void AddBufferedFleetsToWorld(BufferedFleet bufferedFleet)
    {
        var fleetTile = WorldManager.TileList.Find(t => t.Position == bufferedFleet.Position);

        var fleetParent = new GameObject("Fleet of: " + bufferedFleet.Player).transform;

        fleetParent.position = fleetTile.TileParent.position;
        fleetParent.SetParent(GameObject.Find(Tags.Fleets).transform);

        #region Instantiate units
        var units = new Unit[6];

        for (int i = 0; i < units.Length; i++)
        {
            var unitParent = new GameObject("Unit: " + i).transform;

            fleetParent.rotation = Quaternion.Euler(Vector3.up * (i * -60 - 30));

            unitParent.position = fleetTile.TileParent.position + Vector3.forward * 0.6f;
            unitParent.SetParent(fleetParent);
        }

        fleetParent.rotation = Quaternion.identity;
        #endregion

        var newFleet = new Fleet(bufferedFleet.Player, fleetParent, (FleetType)bufferedFleet.FleetType, units);

        FleetList.Add(newFleet);

        fleetTile.Fleet = newFleet;
    }

    private void AddBufferedUnitsToFleet(Fleet fleet, BufferedUnit bufferedUnit)
    {
        fleet.Units[bufferedUnit.Position] = new Unit(fleet.FleetParent.FindChild("Unit: " + bufferedUnit.Position), new UnitValues(bufferedUnit.UnitType, bufferedUnit.Strength));
    }

    private void AddFleetsIfTileExists()
    {
        for (int i = 0; i < BufferedFleetList.Count; i++)
        {
            if (WorldManager.TileList.Exists(t => t.Position == BufferedFleetList[i].Position))
            {
                AddBufferedFleetsToWorld(BufferedFleetList[i]);
                BufferedFleetList.RemoveAt(i);
            }
        }
    }

    private void AddUnitsToFleetIfFleetExists()
    {
        for (int i = 0; i < BufferedUnitList.Count; i++)
        {
            var tile = WorldManager.TileList.Find(t => t.Position == BufferedUnitList[i].FleetPosition);

            if (tile == null)
            {
                return;
            }

            var fleet = tile.Fleet;

            if (fleet == null)
            {
                return;
            }

            AddBufferedUnitsToFleet(fleet, BufferedUnitList[i]);
            BufferedUnitList.RemoveAt(i);
        }
    }

    public void InstantiateStartFleets()
    {
        //if (PhotonNetwork.isMasterClient)
        //{
        var testUnit = new UnitValues(UnitType.Meele, 1);

        var testUnits = new UnitValues[] { testUnit, testUnit, null, null, null, null };

        // Instantiate one fleet at start
        //InstantiateNewFleet(Vector2.zero, FleetType.Slow, testUnits);
        for (int i = 0; i < 2; i++)
        {
            InstantiateNewFleet(new Vector2(Random.Range(-2, 2), Random.Range(-3, 3)), FleetType.Slow, testUnits);
        }

        //AddUnitToFleetOverNetwork(FleetList[0], 2, testUnit);
        //}
    }

    private void Init()
    {
        WorldManager = GameObject.Find(Tags.Manager).GetComponent<WorldManager>();

        if (!WorldManager)
        {
            Debug.LogError("MissedComponents!");
        }
    }

    private void Start()
    {
        FleetList = new List<Fleet>();
        BufferedFleetList = new List<BufferedFleet>();
        BufferedUnitList = new List<BufferedUnit>();
    }

    private void Awake()
    {
        Init();
    }

    // Update is called once per frame
    private void Update()
    {
        AddFleetsIfTileExists();
        AddUnitsToFleetIfFleetExists();
    }
}
