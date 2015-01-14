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
            TryToRotateFleet(CurrentSelectedTile.FleetID);
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
            TryToRotateFleet(CurrentSelectedTile.FleetID, false);
            return;
        }

        if (targetTile.FleetID > -1)
        {
            // Attack 
            TryToAttackFleet(CurrentSelectedTile.FleetID, targetTile.FleetID);
            return;
        }

        if (MoveableTileList.Exists(t => t == targetTile))
        {
            // Move fleet
            TryToMoveFleet(CurrentSelectedTile.FleetID, targetTile.Position);
        }
    }

    #region Movement
    private void TryToMoveFleet(int fleetID, Position targetTilePosition)
    {
        if (!CheckMovement(fleetID, targetTilePosition))
        {
            return;
        }

        if (!PhotonNetwork.isMasterClient)
        {
            MoveFleet(fleetID, targetTilePosition);
        }

        photonView.RPC(RPCs.AskForMoveFleet, PhotonTargets.MasterClient, fleetID, targetTilePosition.X, targetTilePosition.Y);
    }

    private void MoveFleet(int fleetID, Position targetTilePosition)
    {
        var fleet = FleetList.Find(f => f.ID == fleetID);

        if (fleet == null)
        {
            return; // TODO ask for refresh complete data
        }

        var currentTile = TileList.Find(t => t.FleetID == fleet.ID);
        var targetTile = TileList.Find(t => t.Position.IsSameAs(targetTilePosition));

        if (currentTile == null || targetTile == null)
        {
            return; // TODO ask for refresh complete data
        }

        targetTile.FleetID = fleetID;
        currentTile.FleetID = -1;

        fleet.MoveFleet(targetTile.Position);

        if (PlayerManager.Get().CurrentPlayer.PhotonPlayer == PhotonNetwork.player)
        {
            CheckShowMovementArea();

            TileManager.Get().SelectTile(targetTile);
        }
    }

    private bool CheckMovement(int fleetID, Position targetTilePosition)
    {
        if (fleetID < 0)
        {
            return false;
        }

        var fleet = FleetList.Find(f => f.ID == fleetID);

        if (fleet == null || !fleet.AllowMovement || fleet.Player.ID != PlayerManager.Get().CurrentPlayer.ID)
        {
            return false;
        }

        var currentTile = TileList.Find(t => t.FleetID == fleet.ID);
        var targetTile = TileList.Find(t => t.Position.IsSameAs(targetTilePosition));

        if (currentTile == null || targetTile == null || currentTile.FleetID < 0 || targetTile.FleetID > -1
            || Vector3.Distance(currentTile.TileParent.position, targetTile.TileParent.position) > 2f)
        {
            return false;
        }

        return true;
    }

    [RPC]
    private void AskForMoveFleet(int fleetID, int targetTilePositionX, int targetTilePositionY, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.isMasterClient || info.sender != PlayerManager.Get().CurrentPlayer.PhotonPlayer)
        {
            return;
        }

        var targetTilePosition = new Position(targetTilePositionX, targetTilePositionY);

        if (!CheckMovement(fleetID, targetTilePosition))
        {
            return; // TODO refresh complete data at info.sender
        }

        photonView.RPC(RPCs.MoveFleet, PhotonTargets.All, fleetID, targetTilePositionX, targetTilePositionY);
    }

    [RPC]
    private void MoveFleet(int fleetID, int targetTilePositionX, int targetTilePositionY, PhotonMessageInfo info)
    {
        if (!info.sender.isMasterClient)
        {
            return;
        }

        var fleet = FleetList.Find(f => f.ID == fleetID);

        if (fleet == null || (fleet.Position.X == targetTilePositionX && fleet.Position.Y == targetTilePositionY))
        {
            return;
        }

        var targetTilePosition = new Position(targetTilePositionX, targetTilePositionY);

        MoveFleet(fleetID, targetTilePosition);
    }
    #endregion

    #region Rotation
    private void TryToRotateFleet(int fleetID, bool rotateRight = true)
    {
        if (!CheckRotation(fleetID))
        {
            return;
        }

        var rotationTarget = FleetList.Find(f => f.ID == fleetID).Rotation + (rotateRight ? 1 : -1);

        if (!PhotonNetwork.isMasterClient)
        {
            RotateFleet(fleetID, rotationTarget);
        }

        photonView.RPC(RPCs.AskForRotateFleet, PhotonTargets.MasterClient, fleetID, rotationTarget);
    }

    private void RotateFleet(int fleetID, int rotationTarget)
    {
        var fleet = FleetList.Find(f => f.ID == fleetID);

        if (fleet == null)
        {
            return; // TODO ask for refresh complete data
        }

        fleet.RotateFleet(rotationTarget);
    }

    private bool CheckRotation(int fleetID)
    {
        if (fleetID < 0)
        {
            return false;
        }

        var fleet = FleetList.Find(f => f.ID == fleetID);

        if (fleet == null || !fleet.AllowRotation || fleet.Player.ID != PlayerManager.Get().CurrentPlayer.ID)
        {
            return false;
        }

        return true;
    }

    [RPC]
    private void AskForRotateFleet(int fleetID, int rotationTarget, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.isMasterClient || info.sender != PlayerManager.Get().CurrentPlayer.PhotonPlayer)
        {
            return;
        }

        if (!CheckRotation(fleetID))
        {
            return; // TODO refresh complete data at info.sender
        }

        photonView.RPC(RPCs.RotateFleet, PhotonTargets.All, fleetID, rotationTarget);
    }

    [RPC]
    private void RotateFleet(int fleetID, int rotationTarget, PhotonMessageInfo info)
    {
        if (!info.sender.isMasterClient)
        {
            return;
        }

        var fleet = FleetList.Find(f => f.ID == fleetID);

        if (fleet == null || fleet.Rotation == rotationTarget)
        {
            return;
        }

        RotateFleet(fleetID, rotationTarget);
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

        if (fleet == null)
        {
            return;
        }

        if (fleet.AllowMovement)
        {
            SetMovementArea();
        }
    }

    private void SetMovementArea()
    {
        var fleet = FleetList.Find(f => f.ID == CurrentSelectedTile.FleetID);

        if (!fleet.AllowMovement)
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
    private void TryToAttackFleet(int ownFleetID, int enemyFleetID)
    {
        if (!CheckAttack(ownFleetID, enemyFleetID))
        {
            return;
        }

        // TODO start fight animation here

        photonView.RPC(RPCs.AskForAttackFleet, PhotonTargets.MasterClient, ownFleetID, enemyFleetID);
    }

    private void AttackFleet(int ownFleetID, int enemyFleetID)
    {
        var ownFleet = FleetList.Find(f => f.ID == ownFleetID);

        if (ownFleet == null)
        {
            return; // TODO ask for refresh complete data
        }

        ownFleet.AttackWithFleet(enemyFleetID);

        if (PlayerManager.Get().CurrentPlayer.PhotonPlayer == PhotonNetwork.player)
        {
            ResetMovementArea();
            TileManager.Get().ResetHighlightedTile();
        }
    }

    [RPC]
    private void AskForAttackFleet(int ownFleetID, int enemyFleetID, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.isMasterClient || info.sender != PlayerManager.Get().CurrentPlayer.PhotonPlayer)
        {
            return;
        }

        if (!CheckAttack(ownFleetID, enemyFleetID))
        {
            return; // TODO refresh complete data at info.sender
        }

        photonView.RPC(RPCs.AttackFleet, PhotonTargets.All, ownFleetID, enemyFleetID);
    }

    [RPC]
    private void AttackFleet(int ownFleetID, int enemyFleetID, PhotonMessageInfo info)
    {
        if (!info.sender.isMasterClient)
        {
            return;
        }

        // Achtung keine Überprüfung ob jetzt alle gleich syncronisiert sind!

        AttackFleet(ownFleetID, enemyFleetID);
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

        var unitDirection = GetOwnUnitPosition(ownFleet.Position, enemyFleet.Position);

        if (unitDirection < 0)
        {
            return false;
        }

        var ownUnit = ownFleet.Units[unitDirection];

        if (ownUnit == null)
        {
            return false;
        }

        if (ownUnit.UnitController == null || ownUnit.AllowAttack == false) //FUNktionier nicht nicht.. Angriffe funktionieren nicht mehr!!
        {
            return false;
        }

        var distanceBetweenFleets = Vector3.Distance(ownFleet.FleetParent.position, enemyFleet.FleetParent.position);

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

    public int GetOwnUnitPosition(Position ownFleetPosition, Position enemyFleetPosition)
    {
        if (enemyFleetPosition.X == ownFleetPosition.X)
        {
            if (enemyFleetPosition.Y > ownFleetPosition.Y)
            {
                return 0;
            }

            return 3;
        }
        else if (enemyFleetPosition.Y == ownFleetPosition.Y)
        {
            if (enemyFleetPosition.X > ownFleetPosition.X)
            {
                return 1;
            }

            return 4;
        }
        else if(enemyFleetPosition.X + enemyFleetPosition.Y == ownFleetPosition.X + ownFleetPosition.Y)
        {
            if (enemyFleetPosition.X < ownFleetPosition.X
                && enemyFleetPosition.Y > ownFleetPosition.Y)
            {
                return 5;
            }
            else if (enemyFleetPosition.X > ownFleetPosition.X
                && enemyFleetPosition.Y < ownFleetPosition.Y)
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
