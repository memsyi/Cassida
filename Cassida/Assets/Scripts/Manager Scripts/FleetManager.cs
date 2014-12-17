using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class FleetSettings
{
    [SerializeField]
    private Transform
        _slowFleetObject = null,
        _fastFleetObject = null;

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
        _meeleUnitObject = null,
        _rangeUnitObject = null;

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

    private WorldManager WorldManager { get; set; }

    private void InstantiateNewFleet(Vector2 position, FleetType fleetType, UnitValues[] unitValues)
    {
        photonView.RPC("InstantiateNewFleetOverNetwork", PhotonTargets.AllBuffered, position, fleetType.GetHashCode(), PhotonNetwork.player);

        for(int i = 0; i < unitValues.Length; i++)
        {
            if (unitValues[i] != null)
            {
                photonView.RPC("AddUnitToFleetOverNetwork", PhotonTargets.AllBuffered, FleetList.Count - 1, i, unitValues[i].UnitType.GetHashCode(), unitValues[i].Strength);
            }
        }
    }

    [RPC]
    private void InstantiateNewFleetOverNetwork(Vector2 position, int fleetType, PhotonPlayer player)
    {
        var fleetTile = WorldManager.TileList.Find(t => t.Position == position);

        var fleetParent = new GameObject("Fleet of: " + player).transform; // player in namen eintragen ! TODO

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

        var newFleet = new Fleet(player, fleetParent, (FleetType)fleetType, units);

        FleetList.Add(newFleet);

        fleetTile.Fleet = newFleet;
    }

    [RPC]
    public void AddUnitToFleetOverNetwork(int fleetInListPosition, int position, int unitType, int strength)
    {
        var fleet = FleetList[fleetInListPosition];

        fleet.Units[position] = new Unit(fleet.FleetParent.FindChild("Unit: " + position), new UnitValues((UnitType)unitType, strength));
    }

    public void InstantiateStartFleets()
    {
        if (PhotonNetwork.isMasterClient)
        {
            var testUnit = new UnitValues(UnitType.Meele, 1);

            var testUnits = new UnitValues[] { testUnit, testUnit, null, null, null, null };

            // Instantiate one fleet at start
            InstantiateNewFleet(Vector2.zero, FleetType.Slow, testUnits);
            InstantiateNewFleet(new Vector2(2, 1), FleetType.Slow, testUnits);

            //AddUnitToFleetOverNetwork(FleetList[0], 2, testUnit);
        }
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
