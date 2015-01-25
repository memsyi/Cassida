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
    }

    public Transform FastFleetObject
    {
        get { return _fastFleetObject; }
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
    }

    public Transform MeeleUnitTwoObject
    {
        get { return _meeleUnitTwoObject; }
    }

    public Transform MeeleUnitOneObject
    {
        get { return _meeleUnitOneObject; }
    }
    #endregion

    #region Range
    public Transform RangeUnitThreeObject
    {
        get { return _rangeUnitThreeObject; }
    }

    public Transform RangeUnitTwoObject
    {
        get { return _rangeUnitTwoObject; }
    }

    public Transform RangeUnitOneObject
    {
        get { return _rangeUnitOneObject; }
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
    }

    public FleetSettings FleetSettings
    {
        get { return _fleetSettings; }
    }

    // Lists
    public List<Fleet> FleetList { get; private set; }

    // Fleet ID
    private int _highestFleetID = 0;
    private int HighestFleetID
    {
        get { return _highestFleetID; }
        set { if (value > _highestFleetID) _highestFleetID = value; }
    }
    #endregion

    public void InstantiateNewFleet(Position position, FleetType fleetType, UnitValues[] unitValues)
    {
        photonView.RPC(RPCs.AskForNewFleet, PhotonTargets.MasterClient, position.X, position.Y, (int)fleetType); // TODO ?? Network code with json ??

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

        // TODO enough money...?

        if(!IsTileFree(positionX, positionY))
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

        if (!IsTileFree(positionX, positionY))
        {
            return;
        }

        var player = PlayerManager.Get().GetPlayer(playerID);

        AddFleet(new Fleet(id, player.ID, new Position(positionX, positionY), (FleetType)fleetType));
    }

    private bool IsTileFree(int positionX, int positionY)
    {
        var position = new Position(positionX, positionY);

        var tile = TileManager.Get().GetTile(position);

        if (tile == null || tile.FleetID > -1 || tile.BaseID > -1)
        {
            return false;
        }

        return true;
    }

    public void AddFleet(Fleet fleet)
    {
        if (FleetList.Exists(f => f.ID == fleet.ID || f.Position == fleet.Position) || TileManager.Get().GetTile(fleet.Position) == null)
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

        var fleet = GetFleet(fleetID);

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

        // TODO enough money...?

        var fleet = GetFleet(fleetID);

        if (fleet == null || fleet.UnitList.Exists(u => u.Position == position))
        {
            return;
        }

        fleet.UnitList.Add(new Unit(fleet.ID, position, (UnitType)unitType, strength));
    }
    #endregion

    #region Destroy or reset fleets
    public void ResetMovementOfAllFleets()
    {
        foreach (var fleet in FleetList)
        {
            fleet.ResetFleetActions();
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
        foreach(var fleet in FleetList)
        {
            DestroyFleet(fleet);
        }
    }
    #endregion

    public Fleet GetFleet(int id)
    {
        return FleetList.Find(f => f.ID == id);
    }
    public Fleet GetFleet(Position position)
    {
        return FleetList.Find(f => f.Position == position);
    }

    public void AddStartFleets()
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        foreach (var player in PlayerManager.Get().PlayerList)
        {
            HighestFleetID++;
            var playerBase = BaseManager.Get().GetBase(player.ID);
            var fleetPosition = MapManager.Get().FindNearestTileToPosition(Vector3.MoveTowards(playerBase.BaseParent.position, Vector3.zero, 1.5f)).Position;

            photonView.RPC(RPCs.AddNewFleet, PhotonTargets.All, HighestFleetID, player.ID, fleetPosition.X, fleetPosition.Y, (int)FleetType.Slow);
            photonView.RPC(RPCs.AddNewUnit, PhotonTargets.MasterClient, HighestFleetID, 1, (int)UnitType.Meele, 1);
        }
    }

    public void AddEndTurnEvents()
    {
        // Add events
        PlayerManager.Get().EndTurnEvent += new EndTurnHandler(ResetMovementOfAllFleets);
    }

    //public void RemoveEndTurnEvents()
    //{
    //    // Remove events
    //    PlayerManager.Get().EndTurnEvent -= new EndTurnHandler(ResetMovementOfAllFleets);
    //}

    private void Init()
    {
        FleetList = new List<Fleet>();
        AddEndTurnEvents();
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
}
