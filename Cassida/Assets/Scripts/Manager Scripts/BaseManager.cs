using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct BaseSettings
{
    [SerializeField]
    private Transform
        _mainBuildingObject;
        
    public Transform MainBuildingObject
    {
        get { return _mainBuildingObject; }
    }
}

[System.Serializable]
public struct BuildingSettings
{
    [SerializeField]
    private Transform
        _baseDefense,
        _engeneeringBay,
        _commandCenter;

    public Transform CommandCenter
    {
        get { return _commandCenter; }
    }
    public Transform EngeneeringBay
    {
        get { return _engeneeringBay; }
    }
    public Transform BaseDefense
    {
        get { return _baseDefense; }
    }
}

[RequireComponent(typeof(PhotonView))]
public class BaseManager : Photon.MonoBehaviour, IJSON
{
    #region Variables
    [SerializeField]
    private BaseSettings _baseSettings;

    [SerializeField]
    private BuildingSettings _buildingSettings;

    public BuildingSettings BuildingSettings
    {
        get { return _buildingSettings; }
    }

    public BaseSettings BaseSettings
    {
        get { return _baseSettings; }
    }

    // Selection
    private bool BaseSelected { get; set; }


    // Lists
    public List<Base> BaseList { get; private set; }
    public Base OwnBase { get { return GetBase(PlayerManager.Get().Player.ID); } }

    // Base ID
    private int _highestBaseID = 0;
    private int HighestBaseID
    {
        get { return _highestBaseID; }
        set { if (PhotonNetwork.isMasterClient) _highestBaseID = value; }
    }
    #endregion

    public void AddNewBuilding(int buildingType)
    {
        if (!BaseSelected || !OwnBase.AllowAddBuilding)
        {
            return;
        }

        OwnBase.AddBuilding((BuildingType)buildingType);
    }
    public void AddNewFleet() // TODO add blueprint info
    {
        if (!BaseSelected)
        {
            return;
        }

        print("add fleet");
    }

    public void InstantiateBasesForAllPlayer()
    {
        foreach (var player in PlayerManager.Get().PlayerList)
        {
            InstantiateNewBase(player.PhotonPlayer);
        }
    }

    private void InstantiateNewBase(PhotonPlayer photonPlayer)
    {
        CheckForNewBase(photonPlayer);
    }

    #region Base selection
    public void SelectIfOwnBase(Base baseo)
    {
        if (baseo == null || baseo.ID != OwnBase.ID)
        {
            return;
        }
        BaseSelected = true;
        CameraController.Get().SetCameraToBasePosition(true);
        print("Base selected");
    }

    public void DeselectBase()
    {
        if (!BaseSelected)
        {
            return;
        }
        BaseSelected = false;
        print("Base deselected");
    }
    #endregion

    #region Add base
    private void CheckForNewBase(PhotonPlayer photonPlayer)
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        // TODO genügend Geld?

        var tileList = TileManager.Get().TileList.FindAll(t => t.ObjectiveType == ObjectiveType.Base
            && !BaseList.Exists(b =>
                b.Position == t.Position
                || PlayerManager.Get().GetPlayer(b.PlayerID).PhotonPlayer == photonPlayer));

        if (tileList == null)
        {
            return;
        }

        var tile = tileList[Random.Range(0, tileList.Count)];

        if (tile == null || tile.ObjectiveType != ObjectiveType.Base)
        {
            return;
        }

        var player = PlayerManager.Get().GetPlayer(photonPlayer);

        HighestBaseID++;

        if (BaseList.Exists(f => f.ID == HighestBaseID))
        {
            return;
        }

        photonView.RPC(RPCs.AddNewBase, PhotonTargets.All, HighestBaseID, player.ID, tile.Position.X, tile.Position.Y);
    }

    [RPC]
    private void AddNewBase(int id, int playerID, int positionX, int positionY, PhotonMessageInfo info)
    {
        if (!info.sender.isMasterClient)
        {
            return;
        }

        var position = new Position(positionX, positionY);

        HighestBaseID = id;

        BaseList.Add(new Base(id, playerID, position));
    }

    public void InstantiateAllExistingBasesAtPlayer(PhotonPlayer photonPlayer)
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        for (int i = BaseList.Count - 1; i > 0; i--)
        {
            var baseo = BaseList[i];
            photonView.RPC(RPCs.AddNewBase, photonPlayer, baseo.ID, baseo.PlayerID, baseo.Position.X, baseo.Position.Y);
        }
    }
    #endregion

    #region Destroy and reset bases
    public void ResetAlreadyBuildBuildingOfOwnBase()
    {
        if (OwnBase == null)
        {
            return;
        }

        OwnBase.ResetAlreadyBuildBuilding();
    }

    public void DestroyBase(Base baseo)
    {
        Destroy(baseo.BaseParent.gameObject);

        TileManager.Get().ResetHighlightedTile();

        BaseList.Remove(baseo);
    }

    public void DestroyBaseOfPlayer(int playerID)
    {
        var baseo = GetBase(playerID);
        if (baseo == null)
        {
            return;
        }

        DestroyBase(baseo);
    }
    public void DestroyAllBases()
    {
        foreach (var baseo in BaseList)
        {
            DestroyBase(baseo);
        }
        BaseList.Clear();
    }
    #endregion

    public Base GetBase(int playerID)
    {
        return BaseList.Find(b => b.PlayerID == playerID);
    }
    public Base GetBase(Position position)
    {
        return BaseList.Find(b => b.Position == position);
    }

    public void AddEndTurnEvents()
    {
        // Add events
        PlayerManager.Get().EndTurnEvent += new EndTurnHandler(ResetAlreadyBuildBuildingOfOwnBase);
    }

    private void OnGUI()
    {
        if (!BaseSelected || !OwnBase.AllowAddBuilding)
        {
            return;
        }

        if(GUI.Button(new Rect(500, 100, 150, 150), "Add Building"))
        {
            AddNewBuilding(0);
        }
        if (GUI.Button(new Rect(200, 0, 100, 20), "Add Fleet"))
        {
            var testUnits = new UnitValues[] { new UnitValues(UnitType.Meele, 2), new UnitValues(UnitType.Range, 1), null, null, null, null };

            var playerBase = BaseManager.Get().GetBase(PlayerManager.Get().Player.ID);
            var fleetPosition = MapManager.Get().FindNearestTileToPosition(Vector3.MoveTowards(playerBase.BaseParent.position, Vector3.zero, 1.5f)).Position;

            FleetManager.Get().InstantiateNewFleet(fleetPosition, FleetType.Slow, testUnits);
        }
    }

    private void Init()
    {
        BaseList = new List<Base>();
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

    private static BaseManager _instance = null;
    public static BaseManager Get()
    {
        if (_instance == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag(Tags.Manager);
            _instance = obj.AddComponent<BaseManager>();
        }

        return _instance;
    }

    public JSONObject ToJSON()
    {
        var jsonObject = JSONObject.obj;
        jsonObject[JSONs.Bases] = JSONObject.CreateList(BaseList);
        return jsonObject;
    }

    public void FromJSON(JSONObject jsonObject)
    {
        DestroyAllBases();

        JSONObject.ReadList<Fleet>(jsonObject[JSONs.Bases]);
    }
}
