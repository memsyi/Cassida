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
        _meeleUnitOneObject,
        _meeleUnitTwoObject,
        _meeleUnitThreeObject,
        _rangeUnitOneObject,
        _rangeUnitTwoObject,
        _rangeUnitThreeObject;

    #region Meele
    public Transform MeeleUnitThreeObject
    {
        get { return _meeleUnitThreeObject; }
        set { _meeleUnitThreeObject = value; }
    }

    public Transform MeeleUnitTwoObject
    {
        get { return _meeleUnitTwoObject; }
        set { _meeleUnitTwoObject = value; }
    }

    public Transform MeeleUnitOneObject
    {
        get { return _meeleUnitOneObject; }
        private set { _meeleUnitOneObject = value; }
    }
    #endregion

    #region Range
    public Transform RangeUnitThreeObject
    {
        get { return _rangeUnitThreeObject; }
        set { _rangeUnitThreeObject = value; }
    }

    public Transform RangeUnitTwoObject
    {
        get { return _rangeUnitTwoObject; }
        set { _rangeUnitTwoObject = value; }
    }

    public Transform RangeUnitOneObject
    {
        get { return _rangeUnitOneObject; }
        private set { _rangeUnitOneObject = value; }
    }
    #endregion
}

[RequireComponent(typeof(PhotonView))]
public class FleetManager : Photon.MonoBehaviour, IJSON
{
    #region Variables
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

    // Lists
    public List<Fleet> FleetList { get; private set; }
    private List<Tile> TileList { get { return TileManager.Get().TileList; } }

    // Fleet ID
    private int _highestFleetID = 0;
    private int HighestFleetID
    {
        get { return _highestFleetID; }
        set { if (PhotonNetwork.isMasterClient) _highestFleetID = value; }
    }
    #endregion

    public void InstantiateNewFleet(Position position, FleetType fleetType, UnitValues[] unitValues)
    {
        photonView.RPC(RPCs.AskForNewFleet, PhotonTargets.MasterClient, position.X, position.Y, (int)fleetType);

        for (int i = 0; i < unitValues.Length; i++)
        {
            if (unitValues[i] != null)
            {
                photonView.RPC(RPCs.AskForNewUnit, PhotonTargets.MasterClient, -1, i, (int)unitValues[i].UnitType, unitValues[i].Strength);
            }
        }
    }

    #region Add fleet
    [RPC]
    private void AskForNewFleet(int positionX, int positionY, int fleetType, PhotonMessageInfo info)
    {
        var playerID = PlayerManager.Get().GetPlayer(info.sender).ID;

        if (!PhotonNetwork.isMasterClient || playerID != PlayerManager.Get().CurrentPlayer.ID)
        {
            return;
        }

        // TODO genügend Geld?

        var position = new Position(positionX, positionY);

        var tile = TileList.Find(t => t.Position == position);

        if (tile == null || tile.FleetID > -1)
        {
            return;
        }

        HighestFleetID++;

        if (FleetList.Exists(f => f.ID == HighestFleetID))
        {
            return;
        }

        photonView.RPC(RPCs.AddNewFleet, PhotonTargets.All, HighestFleetID, playerID, positionX, positionY, fleetType);
    }

    [RPC]
    private void AddNewFleet(int id, int playerID, int positionX, int positionY, int fleetType, PhotonMessageInfo info)
    {
        if (!info.sender.isMasterClient)
        {
            return;
        }

        var position = new Position(positionX, positionY);
        var player = PlayerManager.Get().GetPlayer(playerID);

        AddFleet(new Fleet(id, player.ID, position, new FleetValues((FleetType)fleetType)));
    }

    public void AddFleet(Fleet fleet)
    {
        if (FleetList.Exists(f => f.ID == fleet.ID || f.Position == fleet.Position) || !TileList.Exists(t => t.Position == fleet.Position))
        {
            return;
        }

        HighestFleetID = fleet.ID;

        FleetList.Add(fleet);
    }

    public void InstantiateAllExistingFleetsAtPlayer(PhotonPlayer photonPlayer)
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        foreach (var fleet in FleetList)
        {
            photonView.RPC(RPCs.AddNewFleet, photonPlayer, fleet.ID, fleet.PlayerID, fleet.Position.X, fleet.Position.Y, (int)fleet.FleetValues.FleetType);

            foreach (var unit in fleet.UnitList)
            {
                photonView.RPC(RPCs.AddNewUnit, photonPlayer, fleet.ID, unit.Position, (int)unit.UnitValues.UnitType, unit.UnitValues.Strength);
            }
        }
    }
    #endregion

    #region Add unit
    [RPC]
    private void AskForNewUnit(int fleetID, int position, int unitType, int strength, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.isMasterClient || info.sender != PlayerManager.Get().CurrentPlayer.PhotonPlayer)
        {
            return;
        }

        if (fleetID < 0)
        {
            fleetID = HighestFleetID;
        }

        var fleet = FleetList.Find(f => f.ID == fleetID);

        if (fleet == null || fleet.UnitList.Exists(u => u.Position == position) || PlayerManager.Get().PlayerList.Find(p => p.ID == fleet.PlayerID).PhotonPlayer != info.sender)
        {
            return;
        }

        photonView.RPC(RPCs.AddNewUnit, PhotonTargets.All, fleetID, position, unitType, strength);
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

        if (fleet == null || fleet.UnitList.Exists(u => u.Position == position))
        {
            return;
        }

        fleet.UnitList.Add(new Unit(fleet.ID, position, new UnitValues((UnitType)unitType, strength)));
    }
    #endregion

    // TODO !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! by event?!?!?!
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
        Destroy(fleet.FleetParent.gameObject);

        TileManager.Get().ResetHighlightedTile();

        FleetList.Remove(fleet);
    }

    public void DestroyAllFleetsOfPlayer(int playerID)
    {
        for (int i = FleetList.Count - 1; i >= 0; i--)
        {
            if (FleetList[i].PlayerID == playerID)
            {
                DestroyFleet(FleetList[i]);
            }
        }
    }
    public void DestroyAllFleets()
    {
        foreach (var player in PlayerManager.Get().PlayerList)
        {
            DestroyAllFleetsOfPlayer(player.ID);
        }
    }
    #endregion

    public Fleet GetFleet(int id)
    {
        return FleetList.Find(f => f.ID == id);
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(200, 0, 100, 20), "Add Fleet"))
        {
            var testUnits = new UnitValues[] { new UnitValues(UnitType.Meele, 2), new UnitValues(UnitType.Range, 1), null, null, null, null };
            InstantiateNewFleet(new Position(0, 0), FleetType.Slow, testUnits);
        }
    }

    private void Init()
    {
        FleetList = new List<Fleet>();
    }

    private void Start()
    {
        Init();
    }

    private void Awake()
    {
        //Check for Singleton
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Debug.LogError("Second instance!");
            return;
        }
    }

    private void Update()
    {

    }

    private static FleetManager _instance = null;
    public static FleetManager Get()
    {
        if (_instance == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag(Tags.Manager);
            _instance = obj.AddComponent<FleetManager>();
        }

        return _instance;
    }

    public JSONObject ToJSON()
    {
        var jsonObject = JSONObject.obj;
        jsonObject[JSONs.Fleets] = JSONObject.CreateList(FleetList);
        return jsonObject;
    }

    public void FromJSON(JSONObject jsonObject)
    {
        DestroyAllFleets();

        JSONObject.ReadList<Fleet>(jsonObject[JSONs.Fleets]);
    }

    // https://github.com/ChristophPech/servertest/blob/master/src/gamesrv/techtree.cfg
    // https://github.com/omegasrevenge/Project4/blob/master/Assets/Scripts/GameManager.cs
}
