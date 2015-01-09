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

[RequireComponent(typeof(PhotonView))]
public class FleetManager : Photon.MonoBehaviour//, IJSON
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

    //private static FleetManager _inst = null;
    //public static FleetManager Get()
    //{
    //    if (_inst == null) _inst = new FleetManager();
    //    return _inst;
    //}

    //public static GameManager Singleton
    //{
    //    get
    //    {
    //        if (_instance == null)
    //        {
    //            GameObject obj = new GameObject("GameManager");
    //            _instance = obj.AddComponent<GameManager>();
    //        }
    //        return _instance;
    //    }
    //}


    //private void Start()
    //{
    //    //Check for Singleton
    //    if (_instance == null)
    //        _instance = this;
    //    else if (_instance != this)
    //        Debug.LogError("Second instance of GameManager.");
    //}

    // Lists
    public List<Fleet> FleetList { get; private set; }
    private List<Tile> TileList { get { return TileManager.TileList; } }

    // Fleet ID
    private int _highestFleetID = 0;
    private int HighestFleetID
    {
        get { return _highestFleetID; }
        set { if (PhotonNetwork.isMasterClient) _highestFleetID = value; }
    }

    // Scripts
    private PlayerManager PlayerManager { get; set; }
    private TileManager TileManager { get; set; }
    #endregion

    private void InstantiateNewFleet(Player player, Vector2 position, FleetType fleetType, UnitValues[] unitValues)
    {
        photonView.RPC(RPCs.AskForNewFleet, PhotonTargets.MasterClient, position, (int)fleetType, player); // TODO RPCs checken

        for (int i = 0; i < unitValues.Length; i++)
        {
            if (unitValues[i] != null)
            {
                photonView.RPC(RPCs.AskForNewUnit, PhotonTargets.MasterClient, -1, i, (int)unitValues[i].UnitType, unitValues[i].Strength, player); // TODO RPCs checken
            }
        }
    }

    #region Add fleet
    [RPC]
    private void AskForNewFleet(Vector2 position, int fleetType, Player player)
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        // TODO genügend Geld?

        var tile = TileList.Find(t => t.Position == position);

        if (tile == null || tile.FleetID > -1) // TODO player turn and free tile
        {
            return;
        }

        HighestFleetID++;
        photonView.RPC(RPCs.AddNewFleet, PhotonTargets.All, HighestFleetID, player, position, (FleetType)fleetType); // TODO RPCs checken
    }

    [RPC]
    public void AddNewFleet(int ID, Player player, Vector2 position, int fleetType, PhotonMessageInfo info)
    {
        if (!info.sender.isMasterClient)
        {
            return;
        }

        var tile = TileList.Find(t => t.Position == position);

        if (tile == null || FleetList.Exists(f => f.ID == ID))
        {
            return;
        }

        HighestFleetID = ID;

        var fleetParent = new GameObject("Fleet of: " + player.Name).transform;

        fleetParent.position = tile.TileParent.position;
        fleetParent.SetParent(GameObject.Find(Tags.Fleets).transform);

        FleetList.Add(new Fleet(ID, player, position, new FleetValues((FleetType)fleetType), fleetParent));

        tile.FleetID = ID;
    }

    public void InstatiateAllExistingFleetsAtPlayer(PhotonPlayer player)
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        foreach (var fleet in FleetList)
        {
            photonView.RPC(RPCs.AddNewFleet, player, fleet.ID, fleet.Position, (int)fleet.FleetValues.FleetType, fleet.Player); // TODO RPCs checken

            for (int i = 0; i < fleet.Units.Length; i++)
            {
                if (fleet.Units[i] != null)
                {
                    photonView.RPC(RPCs.AddNewUnit, player, fleet.ID, i, (int)fleet.Units[i].UnitValues.UnitType, fleet.Units[i].UnitValues.Strength);
                } // TODO RPCs checken
            }
        }
    }
    #endregion

    #region Add unit
    [RPC]
    private void AskForNewUnit(int fleetID, int position, int unitType, int strength, Player player)
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        if (fleetID < 0)
        {
            fleetID = HighestFleetID;
        }

        var fleet = FleetList.Find(f => f.ID == fleetID);

        if (fleet == null || fleet.Units[position] != null) // player turn
        {
            return;
        }

        photonView.RPC(RPCs.AddNewUnit, PhotonTargets.All, fleetID, position, unitType, strength); // TODO rpc calls
    }

    [RPC]
    private void AddNewUnit(int fleetID, int position, int unitType, int strength, PhotonMessageInfo info)
    {
        if (!info.sender.isMasterClient)
        {
            return;
        }

        // TODO genügend Geld?

        var fleet = FleetList.Find(f => f.ID == fleetID);

        if (fleet == null || fleet.Units[position] != null)
        {
            return;
        }

        var unitParent = new GameObject("Unit: " + position).transform;

        fleet.FleetParent.rotation = Quaternion.Euler(Vector3.up * (position * -60 - 30));

        var tile = TileList.Find(t => t.FleetID == fleetID);

        if (tile == null)
        {
            return;
        }

        unitParent.position = tile.TileParent.position + Vector3.forward * 0.6f;
        unitParent.SetParent(fleet.FleetParent);

        fleet.FleetParent.rotation = Quaternion.identity;

        fleet.Units[position] = new Unit(fleet.Player, new UnitValues((UnitType)unitType, strength), unitParent);
    }
    #endregion

    #region Destroy or reset fleets
    public void ResetMovementOfAllFleets()
    {
        foreach (var fleet in FleetList)
        {
            fleet.ResetMovementRotationAndAttack();
        }
    }

    public void DestroyFleet(Fleet fleet)
    {
        var tile = TileList.Find(t => t.FleetID == fleet.ID);
        tile.FleetID = -1;

        Destroy(fleet.FleetParent.gameObject);

        TileManager.ResetHighlightedTile();

        FleetList.Remove(fleet);
    }

    public void DestroyAllFleetsOfDisconnectedPlayers(int playerID)
    {
        for (int i = FleetList.Count - 1; i >= 0; i--)
        {
            if (FleetList[i].Player.ID == playerID)
            {
                DestroyFleet(FleetList[i]);
            }
        }
    }
    #endregion

    public void InstantiateStartFleets()
    {
        var testUnit = new UnitValues(UnitType.Meele, 2);

        var testUnits = new UnitValues[] { testUnit, new UnitValues(UnitType.Range, 1), null, null, null, null };

        // Instantiate one fleet at start
        //InstantiateNewFleet(Vector2.zero, FleetType.Slow, testUnits);
        for (int i = 0; i < 2; i++)
        {
            var position = new Vector2(Random.Range(-2, 2), Random.Range(-3, 3));

            InstantiateNewFleet(PlayerManager.Player, position, FleetType.Slow, testUnits);
        }
        //InstantiateNewFleet(new Vector2(Random.Range(-2, 2), Random.Range(-3, 3)), FleetType.Fast, testUnits);
    }

    private void Init()
    {
        PlayerManager = GameObject.FindGameObjectWithTag(Tags.Manager).GetComponent<PlayerManager>();
        TileManager = GameObject.FindGameObjectWithTag(Tags.Manager).GetComponent<TileManager>();

        if (!TileManager)
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

    private void Update()
    {

    }

    //public JSONObject ToJSON()
    //{
    //    var o = JSONObject.obj;

    //    //o["FleetList"] = JSONObject.CreateList(FleetList);
    //    o["blah"] = new JSONObject(123);

    //    return o;
    //}

    //public void FromJSON(JSONObject o)
    //{
    //    throw new System.NotImplementedException();
    //}

    // https://github.com/ChristophPech/servertest/blob/master/src/gamesrv/techtree.cfg
    // https://github.com/omegasrevenge/Project4/blob/master/Assets/Scripts/GameManager.cs
}
