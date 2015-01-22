using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PhotonView))]
public class InputManager : Photon.MonoBehaviour
{
    #region Varibales
    [SerializeField]
    private Transform
        _moveableTileObject,
        _attackableTileObject;

    public Transform AttackableTileObject
    {
        get { return _attackableTileObject; }
        private set { _attackableTileObject = value; }
    }
    public Transform MoveableTileObject
    {
        get { return _moveableTileObject; }
        private set { _moveableTileObject = value; }
    }

    // Tiles
    private Tile CurrentHighlightedTile { get { return TileManager.Get().CurrentHighlightedTile; } }
    private Tile CurrentSelectedTile { get { return TileManager.Get().CurrentSelectedTile; } }

    // Lists
    //private List<Tile> MoveableTileList { get; set; }
    private List<Transform> MoveableTileObjectList { get; set; }
    private List<Transform> AttackableTileObjectList { get; set; }
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
        }
        else
        {
            TileManager.Get().SelectTile(clickedTile);
        }

        CheckShowActionArea();
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
        }
        else if (targetTile.FleetID > -1 || targetTile.BaseID > -1)
        {
            // Attack
            TryToAttack(CurrentSelectedTile.FleetID, targetTile.Position);
        }
        else if (CheckMovement(CurrentSelectedTile.FleetID, targetTile.Position))
        {
            // Move fleet
            TryToMoveFleet(CurrentSelectedTile.FleetID, targetTile.Position);
        }

        CheckShowActionArea();
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
        var fleet = FleetManager.Get().GetFleet(fleetID);

        if (fleet == null)
        {
            return; // TODO ask for refresh complete data
        }

        var currentTile = TileManager.Get().GetTile(fleetID);
        var targetTile = TileManager.Get().GetTile(targetTilePosition);

        if (currentTile == null || targetTile == null)
        {
            return; // TODO ask for refresh complete data
        }

        fleet.MoveFleet(targetTile.Position);

        if (PlayerManager.Get().CurrentPlayer.PhotonPlayer == PhotonNetwork.player)
        {
            TileManager.Get().SelectTile(targetTile);
        }
    }

    private bool CheckMovement(int fleetID, Position targetTilePosition)
    {
        if (fleetID < 0)
        {
            return false;
        }

        var fleet = FleetManager.Get().GetFleet(fleetID);

        if (fleet == null || !fleet.AllowMovement || fleet.PlayerID != PlayerManager.Get().CurrentPlayer.ID)
        {
            return false;
        }

        var currentTile = TileManager.Get().GetTile(fleetID);
        var targetTile = TileManager.Get().GetTile(targetTilePosition);

        if (currentTile == null || targetTile == null
            || currentTile.FleetID < 0 || targetTile.FleetID > -1
            || targetTile.BaseID > -1
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

        var fleet = FleetManager.Get().GetFleet(fleetID);

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

        var rotationTarget = FleetManager.Get().GetFleet(fleetID).Rotation + (rotateRight ? 1 : -1);

        if (!PhotonNetwork.isMasterClient)
        {
            RotateFleet(fleetID, rotationTarget);
        }

        photonView.RPC(RPCs.AskForRotateFleet, PhotonTargets.MasterClient, fleetID, rotationTarget);
    }

    private void RotateFleet(int fleetID, int rotationTarget)
    {
        var fleet = FleetManager.Get().GetFleet(fleetID);

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

        var fleet = FleetManager.Get().GetFleet(fleetID);

        if (fleet == null || !fleet.AllowRotation || fleet.PlayerID != PlayerManager.Get().CurrentPlayer.ID)
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

        var fleet = FleetManager.Get().GetFleet(fleetID);

        if (fleet == null || fleet.Rotation == rotationTarget)
        {
            return;
        }

        RotateFleet(fleetID, rotationTarget);
    }
    #endregion

    #region Action area
    public void CheckShowActionArea()
    {
        if (PlayerManager.Get().CurrentPlayer.PhotonPlayer != PhotonNetwork.player)
        {
            return;
        }

        ResetActionArea();

        if (CurrentSelectedTile == null)
        {
            return;
        }

        var fleet = FleetManager.Get().GetFleet(CurrentSelectedTile.FleetID);

        if (fleet == null)
        {
            return;
        }

        if (fleet.AllowMovement)
        {
            SetMovementArea(fleet);
        }

        SetAttackArea(fleet);
    }

    private void SetMovementArea(Fleet fleet)
    {
        var moveableTileList = TileManager.Get().TileList.FindAll(t => CheckMovement(fleet.ID, t.Position));

        for (int i = 0; i < moveableTileList.Count; i++)
        {
            var tile = moveableTileList[i];
            var tileObject = MoveableTileObjectList[i];
            tileObject.position = tile.TileParent.position;
            tileObject.rotation = tile.TileParent.rotation;
            tileObject.gameObject.SetActive(true);
        }
    }

    private void SetAttackArea(Fleet fleet)
    {
        var attackableTileList = TileManager.Get().TileList.FindAll(t => CheckAttack(fleet.ID, t.Position));

        for (int i = 0; i < attackableTileList.Count; i++)
        {
            var tile = attackableTileList[i];
            var tileObject = AttackableTileObjectList[i];
            tileObject.position = tile.TileParent.position;
            tileObject.rotation = tile.TileParent.rotation;
            tileObject.gameObject.SetActive(true);
        }
    }

    public void ResetActionArea()
    {
        foreach (var tileObject in MoveableTileObjectList)
        {
            tileObject.gameObject.SetActive(false);
        }
        foreach (var tileObject in AttackableTileObjectList)
        {
            tileObject.gameObject.SetActive(false);
        }
    }
    #endregion

    #region Fight
    private void TryToAttack(int ownFleetID, Position enemyPosition)
    {
        if (!CheckAttack(ownFleetID, enemyPosition))
        {
            return;
        }

        // TODO start fight animation here

        photonView.RPC(RPCs.AskForAttack, PhotonTargets.MasterClient, ownFleetID, enemyPosition.X, enemyPosition.Y);
    }

    private void AttackEnemy(int ownFleetID, int enemyPositionX, int enemyPositionY)
    {
        var ownFleet = FleetManager.Get().GetFleet(ownFleetID);

        if (ownFleet == null)
        {
            return; // TODO ask for refresh complete data
        }

        var enemyTile = TileManager.Get().GetTile(new Position(enemyPositionX, enemyPositionY));

        if (enemyTile.FleetID > -1)
        {
            ownFleet.AttackFleetWithFleet(enemyTile.FleetID);
        }
        else if (enemyTile.BaseID > -1)
        {
            ownFleet.AttackBaseWithFleet(enemyTile.BaseID);
        }

        if (PlayerManager.Get().CurrentPlayer.PhotonPlayer == PhotonNetwork.player)
        {
            ResetActionArea();
            TileManager.Get().ResetHighlightedTile();
        }
    }

    [RPC]
    private void AskForAttack(int ownFleetID, int enemyPositionX, int enemyPositionY, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.isMasterClient || info.sender != PlayerManager.Get().CurrentPlayer.PhotonPlayer)
        {
            return;
        }

        if (!CheckAttack(ownFleetID, enemyPositionX, enemyPositionY))
        {
            return; // TODO refresh complete data at info.sender
        }

        photonView.RPC(RPCs.AttackEnemy, PhotonTargets.All, ownFleetID, enemyPositionX, enemyPositionY);
    }

    [RPC]
    private void AttackEnemy(int ownFleetID, int enemyPositionX, int enemyPositionY, PhotonMessageInfo info)
    {
        if (!info.sender.isMasterClient)
        {
            return;
        }

        // Achtung keine Überprüfung ob jetzt alle gleich syncronisiert sind!

        AttackEnemy(ownFleetID, enemyPositionX, enemyPositionY);
    }
    #region Check attack
    public bool CheckAttack(int ownfleetID, Position enemyPosition)
    {
        return CheckAttack(ownfleetID, enemyPosition.X, enemyPosition.Y);
    }

    public bool CheckAttack(int ownFleetID, int enemyTilePositionX, int enemyTilePositionY)
    {
        var ownFleet = FleetManager.Get().GetFleet(ownFleetID);
        var enemyPosition = new Position(enemyTilePositionX, enemyTilePositionY);
        var enemyFleet = FleetManager.Get().GetFleet(enemyPosition);
        var enemyBase = BaseManager.Get().GetBase(enemyPosition);

        if ((ownFleet == null || ownFleet.PlayerID != PlayerManager.Get().CurrentPlayer.ID)
            || (enemyFleet == null || enemyFleet.PlayerID == PlayerManager.Get().CurrentPlayer.ID)
            && (enemyBase == null || enemyBase.PlayerID == PlayerManager.Get().CurrentPlayer.ID))
        {
            return false;
        }

        var unitDirection = GetOwnUnitPosition(ownFleet.Position, enemyPosition);

        if (unitDirection < 0)
        {
            return false;
        }

        var ownUnit = ownFleet.UnitList.Find(u => (u.Position + ownFleet.Rotation) % 6 == unitDirection); ;

        if (ownUnit == null)
        {
            return false;
        }

        if (ownUnit.UnitController == null || ownUnit.AllowAttack == false)
        {
            return false;
        }

        var enemyObjectPosition = enemyFleet != null ? enemyFleet.FleetParent.position : enemyBase.BaseParent.position;

        var distanceBetweenObjects = Vector3.Distance(TileManager.Get().GetTile(ownFleet.ID).TileParent.position, enemyObjectPosition);

        if (ownUnit.UnitValues.UnitType == UnitType.Meele
            && distanceBetweenObjects <= 2)
        {
            return true;
        }
        else if (ownUnit.UnitValues.UnitType == UnitType.Range
            && distanceBetweenObjects > 2 && distanceBetweenObjects <= 4)
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
        else if (enemyFleetPosition.X + enemyFleetPosition.Y == ownFleetPosition.X + ownFleetPosition.Y)
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

    private void AddEndTurnEvents()
    {
        // Add events
        PlayerManager.Get().EndTurnEvent += new EndTurnHandler(ResetActionArea);
        PlayerManager.Get().EndTurnEvent += new EndTurnHandler(RemoveMouseEvents);
    }

    private void Init()
    {
        MoveableTileObjectList = new List<Transform>();
        for (int i = 0; i < 6; i++)
        {
            var tileObject = Instantiate(MoveableTileObject, Vector3.zero, Quaternion.identity) as Transform;
            tileObject.parent = GameObject.FindGameObjectWithTag(Tags.Map).transform;
            tileObject.gameObject.SetActive(false);
            MoveableTileObjectList.Add(tileObject);
        }
        AttackableTileObjectList = new List<Transform>();
        for (int i = 0; i < 12; i++)
        {
            var tileObject = Instantiate(AttackableTileObject, Vector3.zero, Quaternion.identity) as Transform;
            tileObject.parent = GameObject.FindGameObjectWithTag(Tags.Map).transform;
            tileObject.gameObject.SetActive(false);
            AttackableTileObjectList.Add(tileObject);
        }

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
