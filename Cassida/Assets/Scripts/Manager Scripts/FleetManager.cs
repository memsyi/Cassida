using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class FleetSettings
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
public class UnitSettings
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

public class FleetManager : MonoBehaviour
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

    private WorldManager WorldManager { get; set; }

    private void InstantiateNewFleet(Vector2 position, FleetType fleetType, UnitValues[] unitValues)
    {
        var fleetTile = WorldManager.TileList.Find(t => t.Position == position);

        var fleetParent = new GameObject("Fleet: ").transform; // player in namen eintragen ! TODO

        fleetParent.position = fleetTile.TileParent.position;
        fleetParent.SetParent(GameObject.Find(Tags.Fleets).transform);

        #region Instantiate units
        var units = new Unit[6];

        for (int i = 0; i < unitValues.Length; i++)
        {
            var unitParent = new GameObject("Unit: " + i).transform;

            fleetParent.rotation = Quaternion.Euler(Vector3.up * (i * -60 - 30));

            unitParent.position = fleetTile.TileParent.position + Vector3.forward * 0.6f;
            unitParent.SetParent(fleetParent);

            if (unitValues[i] != null)
            {
                units[i] = new Unit(unitParent, unitValues[i]);
            }
        }

        fleetParent.rotation = Quaternion.identity;
        #endregion

        var newFleet = new Fleet(fleetParent, fleetType, units);

        FleetList.Add(newFleet);

        fleetTile.Fleet = newFleet;
    }

    public void AddUnitToFleet(Fleet fleet, int position, UnitValues unitValues)
    {
        fleet.Units[position] =
            new Unit(fleet.FleetParent.FindChild("Unit: " + position), unitValues);
    }

    public void AddStartFleets()
    {
        if (PhotonNetwork.isMasterClient)
        {
            var testUnit = new UnitValues(UnitType.Meele, 1);

            var testUnits = new UnitValues[6] { null, new UnitValues(UnitType.Range, 1), testUnit, testUnit, testUnit, null };

            // Instantiate one fleet at start
            InstantiateNewFleet(Vector2.zero, FleetType.Slow, testUnits);
            InstantiateNewFleet(new Vector2(2, 2), FleetType.Slow, new UnitValues[6]);
            InstantiateNewFleet(new Vector2(2, 1), FleetType.Slow, testUnits);

            AddUnitToFleet(FleetList[0], 0, testUnit);
        }
    }

    private void OnJoinedRoom()
    {
        //AddStartFleets();
    }

    private void Init()
    {
        WorldManager = GameObject.Find(Tags.Manager).GetComponent<WorldManager>();
    }

    private void Start()
    {
        FleetList = new List<Fleet>();

        AddStartFleets();
    }

    private void Awake()
    {
        Init();
    }

    // Update is called once per frame
    private void Update()
    {

    }
}
