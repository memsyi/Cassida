using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PhotonView))]
public class InputManager : Photon.MonoBehaviour
{
    #region Varibales
    [SerializeField]
    private Transform _moveableTileObject;

    public Transform MoveableTileObject
    {
        get { return _moveableTileObject; }
        private set { _moveableTileObject = value; }
    }

    // Tiles
    private Tile CurrentHighlightedTile { get { return TileManager.Get().CurrentHighlightedTile; } }
    private Tile CurrentSelectedTile { get { return TileManager.Get().CurrentSelectedTile; } }

    // Lists
    private List<Tile> MoveableTileList { get; set; }
    private List<Transform> MoveableTileObjectList { get; set; }
    private List<Fleet> FleetList { get { return FleetManager.Get().FleetList; } }
    private List<Tile> TileList { get { return TileManager.Get().TileList; } }
    #endregion

    private void CheckTileSelection()
    {
        var clickedTile = MapManager.Get().NearestTileToMousePosition;

        if (clickedTile == null)
        {
            return;
        }

        if (CurrentSelectedTile == clickedTile)
        {
            // Rotate Fleet
            RotateFleet(CurrentSelectedTile.FleetID);
            return;
        }

        TileManager.Get().SelectTile(clickedTile);

        CheckShowMovementArea();
    }

    private void CheckFleetAction()
    {
        var targetTile = MapManager.Get().NearestTileToMousePosition;

        if (targetTile == null || CurrentSelectedTile == null)
        {
            return;
        }

        if (CurrentSelectedTile == targetTile)
        {
            // Rotate Fleet
            RotateFleet(CurrentSelectedTile.FleetID, false);
            return;
        }

        if (targetTile.FleetID > -1)
        {
            // Attack 
            AttackEnemyFleet(CurrentSelectedTile.FleetID, targetTile.FleetID);
            return;
        }

        // Move fleet
        if (MoveableTileList.Exists(t => t == targetTile))
        {
            MoveFleet(CurrentSelectedTile.FleetID, targetTile.Position);

            TileManager.Get().SelectTile(targetTile);
        }
    }

    #region Movement
    private void MoveFleet(int fleetID, Vector2 targetTilePosition)
    {
        photonView.RPC(RPCs.AskForMoveFleet, PhotonTargets.MasterClient, fleetID, targetTilePosition);
    }

    private void AskForMoveFleet(int fleetID, Vector2 targetTilePosition, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.isMasterClient || info.sender != PlayerManager.Get().CurrentPlayer.PhotonPlayer)
        {
            return;
        }

        var fleet = FleetList.Find(f => f.ID == fleetID);

        if (fleet != null || fleet.MovementPointsLeft > 0)
        {
            return;
        }

        var currentTile = TileList.Find(t => t.FleetID == fleet.ID);
        var targetTile = TileList.Find(t => t.Position == targetTilePosition);

        if (currentTile == null || targetTile == null || currentTile.FleetID < 0 || targetTile.FleetID > -1)
        {
            return;
        }

        photonView.RPC(RPCs.MoveFleet, PhotonTargets.All, fleetID, targetTilePosition);
    }

    [RPC]
    private void MoveFleet(int fleetID, Vector2 targetTilePosition, PhotonMessageInfo info)
    {
        if (!info.sender.isMasterClient)
        {
            return;
        }

        var fleet = FleetList.Find(f => f.ID == fleetID);

        if (fleet == null)
        {
            return;
        }

        var currentTile = TileList.Find(t => t.FleetID == fleet.ID);
        var targetTile = TileList.Find(t => t.Position == targetTilePosition);

        if (currentTile == null || targetTile == null || currentTile.FleetID < 0 || targetTile.FleetID > -1)
        {
            return;
        }

        targetTile.FleetID = fleetID;
        currentTile.FleetID = -1;

        fleet.MoveFleet(targetTile.Position);

        if(PlayerManager.Get().CurrentPlayer.PhotonPlayer == PhotonNetwork.player)
        {
            CheckShowMovementArea();
        }
    } 
    #endregion

    #region Rotation
    private void RotateFleet(int fleetID, bool rotateRight = true)
    {
        photonView.RPC(RPCs.AskForRotateFleet, PhotonTargets.MasterClient, fleetID, rotateRight);
    }

    [RPC]
    private void AskForRotateFleet(int fleetID, bool rotateRight, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.isMasterClient || info.sender != PlayerManager.Get().CurrentPlayer.PhotonPlayer)
        {
            return;
        }

        var fleet = FleetList.Find(f => f.ID == fleetID);

        if (fleet != null || !fleet.AllowRotation)
        {
            return;
        }

        photonView.RPC(RPCs.RotateFleet, PhotonTargets.All, fleetID, rotateRight);
    }

    [RPC]
    private void RotateFleet(int fleetID, bool rotateRight, PhotonMessageInfo info)
    {
        if (!info.sender.isMasterClient)
        {
            return;
        }

        var fleet = FleetList.Find(f => f.ID == fleetID);

        if (fleet == null || !fleet.AllowRotation)
        {
            return;
        }

        fleet.RotateFleet(rotateRight ? 1 : -1);
    }
    #endregion

    #region Movement area
    private void CheckShowMovementArea()
    {
        ResetMovementArea();

        if (CurrentSelectedTile == null)
        {
            return;
        }

        var fleet = FleetList.Find(f => f.ID == CurrentSelectedTile.FleetID);

        if (fleet.MovementPointsLeft > 0)
        {
            ShowMovementArea();
        }
    }

    private void ShowMovementArea()
    {
        var fleet = FleetList.Find(f => f.ID == CurrentSelectedTile.FleetID);

        if (fleet.MovementPointsLeft == 0)
        {
            return;
        }

        MoveableTileList = TileList.FindAll(t => Vector3.Distance(CurrentSelectedTile.TileParent.position, t.TileParent.position) <= 2f);

        foreach (var tile in MoveableTileList)
        {
            MoveableTileObjectList.Add(Instantiate(MoveableTileObject, tile.TileParent.position, tile.TileParent.rotation) as Transform);
        }
    }

    public void ResetMovementArea()
    {
        MoveableTileList.Clear();

        foreach (var tileObject in MoveableTileObjectList)
        {
            Destroy(tileObject.gameObject);
        }

        MoveableTileObjectList.Clear();
    }
    #endregion

    #region Fight
    private void AttackEnemyFleet(int ownFleetID, int enemyFleetID)
    {
        if (CheckAttack(ownFleetID, enemyFleetID))
        {
            return;
        }

        photonView.RPC(RPCs.AskForAttackFleet, PhotonTargets.MasterClient, ownFleetID, enemyFleetID);
    }

    [RPC]
    private void AskForAttackFleet(int ownFleetID, int enemyFleetID, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.isMasterClient || info.sender != PlayerManager.Get().CurrentPlayer.PhotonPlayer)
        {
            return;
        }

        if (CheckAttack(ownFleetID, enemyFleetID))
        {
            return;
        }

        //var ownUnitDirection = TileManager.Get().GetOwnUnitInDirection();
        //var enemyUnitDirection = ownUnitDirection < 3 ? ownUnitDirection + 3 : ownUnitDirection - 3;

        //var ownUnit = FleetList.Find(f => f.ID == ownFleetID).Units[ownUnitDirection];
        //var enemyUnit = FleetList.Find(f => f.ID == enemyFleetID).Units[enemyUnitDirection];

        //if (enemyUnit == null)
        //{
        //    for (int i = 0; i < enemyFleet.Units.Length; i++)
        //    {
        //        AttackUnitOfFleet(enemyFleetID, i, ownUnitStrength);
        //    }
        //}
        //else
        //{
        //    // damage to own
        //    if (ownUnit.UnitValues.UnitType == enemyUnit.UnitValues.UnitType)
        //    {
        //        AttackUnitOfFleet(ownFleetTile.Fleet.ID, ownUnitDirection, enemyUnit.UnitValues.Strength);
        //    }

        //    // damage to enemy
        //    AttackUnitOfFleet(enemyFleetTile.Fleet.ID, enemyUnitDirection, ownUnitStrength);
        //}

        

        photonView.RPC(RPCs.AttackFleet, PhotonTargets.All, ownFleetID, enemyFleetID);
    }

    [RPC]
    private void AttackFleet(int ownFleetID, int enemyFleetID, PhotonMessageInfo info)
    {
        if (!info.sender.isMasterClient)
        {
            return;
        }

        //ownFleet.MovementPointsLeft = 0;
        //ResetMovementArea();
        //ownFleet.AllowRotation = false;
        //ownUnit.AllowAttack = false;

        if (PlayerManager.Get().CurrentPlayer.PhotonPlayer == PhotonNetwork.player)
        {
            TileManager.Get().ResetHighlightedTile();
        }
    }

    private void AttackUnitOfFleet(int fleetID, int unitPosition, int strength)
    {
        photonView.RPC("NetworkAttackUnitOfFleet", PhotonTargets.All, fleetID, unitPosition, strength);
    }

    [RPC]
    private void NetworkAttackUnitOfFleet(int fleetID, int unitPosition, int strength)
    {
        var fleet = FleetList.Find(f => f.ID == fleetID);
        if (fleet == null)
        {
            return;
        }

        fleet.AttackUnit(unitPosition, strength);
    }
    #region Check attack
    public bool CheckAttack(int ownFleetID, int enemyFleetID)
    {
        var ownFleet = FleetList.Find(f => f.ID == ownFleetID);
        var enemyFleet = FleetList.Find(f => f.ID == enemyFleetID);

        if (ownFleet == null || enemyFleet == null || ownFleet.Player.ID != PlayerManager.Get().CurrentPlayer.ID || enemyFleet.Player.ID == PlayerManager.Get().CurrentPlayer.ID)
        {
            return false;
        }

        var unitDirection = GetOwnUnitInDirection(ownFleet.Position, enemyFleet.Position);

        if (unitDirection < 0)
        {
            return false;
        }

        var ownUnit = ownFleet.Units[unitDirection];

        if (ownUnit == null)
        {
            return false;
        }

        if (ownUnit.UnitController == null)// || ownUnit.AllowAttack == false) FUNktionier nicht nicht.. Angriffe funktionieren nicht mehr!!
        {
            return false;
        }

        var distanceBetweenFleets = Vector3.Distance(ownFleet.Position, enemyFleet.Position);

        if (ownUnit.UnitValues.UnitType == UnitType.Meele
            && distanceBetweenFleets <= 2)
        {
            return true;
        }
        else if (ownUnit.UnitValues.UnitType == UnitType.Range
            && distanceBetweenFleets > 2 && distanceBetweenFleets <= 4)
        {
            return true;
        }

        return false;
    }

    public int GetOwnUnitInDirection(Vector2 ownFleetPosition, Vector2 enemyFleetPosition)
    {
        if (enemyFleetPosition.x == ownFleetPosition.x)
        {
            if (enemyFleetPosition.y > ownFleetPosition.y)
            {
                return 0;
            }

            return 3;
        }
        else if (enemyFleetPosition.y == ownFleetPosition.y)
        {
            if (enemyFleetPosition.x > ownFleetPosition.x)
            {
                return 1;
            }

            return 4;
        }
        else
        {
            if (enemyFleetPosition.x < ownFleetPosition.x
                && enemyFleetPosition.y > ownFleetPosition.y
                && (enemyFleetPosition.x - ownFleetPosition.x) - (enemyFleetPosition.y - ownFleetPosition.y) == 0)
            {
                return 5;
            }
            else if (enemyFleetPosition.x > ownFleetPosition.x
                && enemyFleetPosition.y < ownFleetPosition.y
                && (enemyFleetPosition.x + ownFleetPosition.x) - (enemyFleetPosition.y + ownFleetPosition.y) == 0)
            {
                return 2;
            }
        }

        return -1;
    }
    #endregion
    #endregion

    public void AddMouseEvents()
    {
        // Add events
        MouseController.Get().LeftMousecklickEvent += new MouseclickHandler(CheckTileSelection);
        MouseController.Get().RightMouseclickEvent += new MouseclickHandler(CheckFleetAction);
    }

    public void RemoveMouseEvents()
    {
        // Remove events
        MouseController.Get().LeftMousecklickEvent -= new MouseclickHandler(CheckTileSelection);
        MouseController.Get().RightMouseclickEvent -= new MouseclickHandler(CheckFleetAction);
    }

    private void Init()
    {
        MoveableTileList = new List<Tile>();
        MoveableTileObjectList = new List<Transform>();
    }

    private void Start()
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

        Init();
    }

    private void Update()
    {

    }

    private static InputManager _instance = null;
    public static InputManager Get()
    {
        if (_instance == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag(Tags.Manager);
            _instance = obj.AddComponent<InputManager>();
        }

        return _instance;
    }
}
