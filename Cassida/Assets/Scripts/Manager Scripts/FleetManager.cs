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
    public int ID { get; private set; }
    public Vector2 Position { get; private set; }
    public FleetType FleetType { get; private set; }
    public PhotonPlayer Player { get; private set; }

    public BufferedFleet(int id, Vector2 position, FleetType fleetType, PhotonPlayer player)
    {
        ID = id;
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

    #region Variables
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

    // Lists
    public List<Fleet> FleetList { get; private set; }
    private List<Tile> TileList { get { return TileManager.TileList; } }

    // Fleet ID
    private int _highestFleetID = 0;
    private int HighestFleetID { get { _highestFleetID++; return _highestFleetID; } }

    // Buffered lists
    private List<BufferedFleet> BufferedFleetList { get; set; }
    private List<BufferedUnit> BufferedUnitList { get; set; }

    // Scripts
    private TileManager TileManager { get; set; } 
    #endregion

    private void InstantiateNewFleet(Vector2 position, FleetType fleetType, UnitValues[] unitValues)
    {
        photonView.RPC("NetworkAddFleetToWorld", PhotonTargets.MasterClient, 0, position, fleetType.GetHashCode(), PhotonNetwork.player);

        for (int i = 0; i < unitValues.Length; i++)
        {
            if (unitValues[i] != null)
            {
                photonView.RPC("NetworkAddUnitToFleet", PhotonTargets.MasterClient, position, i, unitValues[i].UnitType.GetHashCode(), unitValues[i].Strength);
            }
        }
    }

    #region Add fleets to world
    [RPC]
    private void NetworkAddFleetToWorld(int fleetID, Vector2 position, int fleetType, PhotonPlayer player)
    {
        if (PhotonNetwork.isMasterClient)
        {
            fleetID = HighestFleetID;
        }

        BufferedFleetList.Add(new BufferedFleet(fleetID, position, (FleetType)fleetType, player));

        if (!PhotonNetwork.isMasterClient)
        {
            // erase id also on clients
            fleetID = HighestFleetID;
            return;
        }

        photonView.RPC("NetworkAddFleetToWorld", PhotonTargets.Others, fleetID, position, fleetType, player);
    }

    public void InstatiateAllExistingFleetsAtPlayer(PhotonPlayer player)
    {
        foreach (var fleet in FleetList)
        {
            photonView.RPC("NetworkAddFleetToWorld", player, fleet.ID, TileList.Find(t => t.Fleet == fleet).Position, fleet.FleetType.GetHashCode(), fleet.Player);

            for (int i = 0; i < fleet.Units.Length; i++)
            {
                if (fleet.Units[i] != null)
                {
                    photonView.RPC("NetworkAddUnitToFleet", player, TileList.Find(t => t.Fleet == fleet).Position, i, fleet.Units[i].UnitValues.UnitType.GetHashCode(), fleet.Units[i].UnitValues.Strength);
                }
            }
        }
    }

    public void AddBufferedFleetsToWorld(BufferedFleet bufferedFleet)
    {
        var fleetTile = TileList.Find(t => t.Position == bufferedFleet.Position);

        if (fleetTile.Fleet != null)
        {
            return;
        }

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

        var newFleet = new Fleet(bufferedFleet.ID, bufferedFleet.Player, fleetParent, (FleetType)bufferedFleet.FleetType, units);

        FleetList.Add(newFleet);

        fleetTile.Fleet = newFleet;

        BufferedFleetList.Remove(bufferedFleet);
    }

    private void AddFleetsToWorldIfTileExists()
    {
        for (int i = 0; i < BufferedFleetList.Count; i++)
        {
            print("fleet in order");
            if (TileList.Exists(t => t.Position == BufferedFleetList[i].Position))
            {
                AddBufferedFleetsToWorld(BufferedFleetList[i]);
            }
        }
    } 
    #endregion

    #region Add units to fleets
    [RPC]
    private void NetworkAddUnitToFleet(Vector2 fleetPosition, int position, int unitType, int strength)
    {
        BufferedUnitList.Add(new BufferedUnit(fleetPosition, position, (UnitType)unitType, strength));

        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        photonView.RPC("NetworkAddUnitToFleet", PhotonTargets.Others, fleetPosition, position, unitType, strength);
    }

    private void AddBufferedUnitsToFleet(Fleet fleet, BufferedUnit bufferedUnit)
    {
        if (fleet == null || fleet.Units[bufferedUnit.Position] != null)
        {
            return;
        }

        fleet.Units[bufferedUnit.Position] = new Unit(fleet.FleetParent.FindChild("Unit: " + bufferedUnit.Position), new UnitValues(bufferedUnit.UnitType, bufferedUnit.Strength));

        BufferedUnitList.Remove(bufferedUnit);
    }

    private void AddUnitsToFleetIfFleetExists()
    {
        for (int i = 0; i < BufferedUnitList.Count; i++)
        {
            var tile = TileList.Find(t => t.Position == BufferedUnitList[i].FleetPosition);

            if (tile == null)
            {
                return;
            }

            var fleet = tile.Fleet;

            AddBufferedUnitsToFleet(fleet, BufferedUnitList[i]);
        }
    } 
    #endregion

    #region Destroy or reset fleets
    public void ResetMovementOfAllFleets()
    {
        foreach (var fleet in FleetList)
        {
            fleet.ResetMovementPoints();
        }
    }

    public void DestroyFleet(Fleet fleet)
    {
        var tileOfFleet = TileList.Find(t => t.Fleet == fleet);
        tileOfFleet.Fleet = null;

        Destroy(fleet.FleetParent.gameObject);

        TileManager.ResetHighlightedTile();

        FleetList.Remove(fleet);
    }

    [RPC]
    public void DestroyAllFleetsOfDisconnectedPlayers()
    {
        List<PhotonPlayer> playerList = new List<PhotonPlayer>(PhotonNetwork.playerList);

        // TODO kommt im Spiel eigentlich nicht vor... so gibt es einen outofrange fehler bei löschen der flotte
        //if (BufferedFleetList.Count > 0)
        //{
        //    for (int i = BufferedFleetList.Count - 1; i >= 0; i--)
        //    {
        //        if (!playerList.Exists(p => p == BufferedFleetList[i].Player))
        //        {
        //            for (int u = BufferedUnitList.Count - 1; i >= 0; i--)
        //            {
        //                if (BufferedUnitList[u].FleetPosition == BufferedFleetList[i].Position)
        //                {
        //                    BufferedUnitList.Remove(BufferedUnitList[u]);
        //                }
        //            }

        //            BufferedFleetList.Remove(BufferedFleetList[i]); // HIER!!!
        //        }
        //    }
        //}

        for (int i = FleetList.Count - 1; i >= 0; i--)
        {
            if (!playerList.Exists(p => p == FleetList[i].Player))
            {
                DestroyFleet(FleetList[i]);
            }
        }
    } 
    #endregion

    public void InstantiateStartFleets()
    {
        var testUnit = new UnitValues(UnitType.Meele, 1);

        var testUnits = new UnitValues[] { testUnit, testUnit, null, null, null, null };

        // Instantiate one fleet at start
        //InstantiateNewFleet(Vector2.zero, FleetType.Slow, testUnits);
        for (int i = 0; i < 2; i++)
        {
            InstantiateNewFleet(new Vector2(Random.Range(-2, 2), Random.Range(-3, 3)), FleetType.Slow, testUnits);
        }
        InstantiateNewFleet(new Vector2(Random.Range(-2, 2), Random.Range(-3, 3)), FleetType.Fast, testUnits);
    }

    private void Init()
    {
        TileManager = GameObject.Find(Tags.Manager).GetComponent<TileManager>();

        if (!TileManager)
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

    private void Update()
    {
        AddFleetsToWorldIfTileExists();
        AddUnitsToFleetIfFleetExists();
    }
}
