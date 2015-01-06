using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PhotonView))]
public class InputManager : Photon.MonoBehaviour
{
    #region Varibales
    // Scripts		
    private MouseController MouseController { get; set; }
    private FleetManager FleetManager { get; set; }
    private MapManager MapManager { get; set; }
    private TileManager TileManager { get; set; }

    // Lists
    private List<Fleet> FleetList { get { return FleetManager.FleetList; } }
    private List<Tile> TileList { get { return TileManager.TileList; } }

    // Tiles
    private Tile CurrentHighlightedTile { get { return TileManager.CurrentHighlightedTile; } }
    private Tile CurrentSelectedTile { get { return TileManager.CurrentSelectedTile; } }
    #endregion

    private void CheckTileSelection()
    {
        var clickedTile = MapManager.NearestTileToMousePosition;

        if (clickedTile == null)
        {
            return;
        }

        if (CurrentSelectedTile == clickedTile)
        {
            RotateFleet(CurrentSelectedTile.Fleet.ID);
        }

        TileManager.SelectTile(MapManager.NearestTileToMousePosition);
    }

    #region Movement and Rotation
    private void CheckFleetMovement()
    {
        var targetTile = MapManager.NearestTileToMousePosition;

        if (targetTile == null || CurrentSelectedTile == null)
        {
            return;
        }

        if (CurrentSelectedTile == targetTile)
        {
            // Rotate Fleet
            RotateFleet(CurrentSelectedTile.Fleet.ID, false);
            //RotateFleetToPosition(CurrentSelectedTile.Fleet.ID, CurrentSelectedTile.Fleet.RotationPosition - 1);
            return;
        }

        if (CurrentSelectedTile != null && targetTile.Fleet != null)
        {
            // Attack 
            AttackEnemyFleet();
            return;
        }

        // Move fleet
        MoveFleet(CurrentSelectedTile.Fleet.ID, targetTile.Position);

        TileManager.SelectTile(targetTile);
    }

    private void RotateFleet(int fleetID, bool rotateRight = true)
    {
        photonView.RPC("NetworkRotateFleet", PhotonTargets.AllBuffered, fleetID, rotateRight);
    }

    //private void RotateFleetToPosition(int fleetID, int rotationPosition)
    //{
    //    photonView.RPC("NetworkRotateFleetToPosition", PhotonTargets.All, fleetID, rotationPosition);
    //}

    [RPC]
    private void NetworkRotateFleet(int fleetID, bool rotateRight)
    {
        var fleet = FleetList.Find(f => f.ID == fleetID);

        fleet.RotateFleet(rotateRight ? 1 : -1);
    }

    //[RPC]
    //private void NetworkRotateFleetToPosition(int fleetID, int rotationPosition)
    //{
    //    var fleet = FleetList.Find(f => f.ID == fleetID);

    //    fleet.RotateFleetToPosition(rotationPosition);
    //}

    private void MoveFleet(int fleetID, Vector2 targetTilePosition)
    {
        photonView.RPC("NetworkMoveFleet", PhotonTargets.AllBuffered, fleetID, targetTilePosition);
    }

    [RPC]
    private void NetworkMoveFleet(int fleetID, Vector2 targetTilePosition)
    {
        var fleet = FleetList.Find(f => f.ID == fleetID);
        if (fleet == null)
        {
            return;
        }

        var currentTile = TileList.Find(t => t.Fleet == fleet);
        var targetTile = TileList.Find(t => t.Position == targetTilePosition);

        targetTile.Fleet = fleet;
        currentTile.Fleet = null;

        fleet.MoveFleet(targetTile.TileParent.position);
    }
    #endregion

    #region Fight
    private void AttackEnemyFleet()
    {
        if (!TileManager.CheckAttackEnemyFleet())
        {
            return;
        }

        var ownUnitDirection = TileManager.GetOwnUnitInDirection();
        var enemyUnitDirection = ownUnitDirection < 3 ? ownUnitDirection + 3 : ownUnitDirection - 3;

        var ownFleetTile = CurrentSelectedTile;
        var enemyFleetTile = CurrentHighlightedTile;

        var ownFleet = ownFleetTile.Fleet;
        var enemyFleet = enemyFleetTile.Fleet;

        var ownUnitStrength = ownFleet.Units[ownUnitDirection].UnitValues.Strength;
        var enemyUnit = enemyFleet.Units[enemyUnitDirection];

        if (enemyUnit == null)
        {
            for (int i = 0; i < enemyFleet.Units.Length; i++)
            {
                AttackUnitOfFleet(enemyFleetTile.Fleet.ID, i, ownUnitStrength);
            }
        }
        else
        {
            AttackUnitOfFleet(ownFleetTile.Fleet.ID, ownUnitDirection, enemyUnit.UnitValues.Strength);
            AttackUnitOfFleet(enemyFleetTile.Fleet.ID, enemyUnitDirection, ownUnitStrength);
        }

        TileManager.ResetHighlightedTile();
    }

    private void AttackUnitOfFleet(int fleetID, int unitPosition, int strength)
    {
        photonView.RPC("NetworkAttackUnitOfFleet", PhotonTargets.AllBuffered, fleetID, unitPosition, strength);
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
    #endregion

    private void AddMouseEvents()
    {
        // Add events
        MouseController.LeftMousecklickEvent += new MouseclickHandler(CheckTileSelection);
        MouseController.RightMouseclickEvent += new MouseclickHandler(CheckFleetMovement);
    }

    private void Init()
    {
        MouseController = GameObject.FindGameObjectWithTag(Tags.GameController).GetComponent<MouseController>();
        FleetManager = GameObject.FindGameObjectWithTag(Tags.Manager).GetComponent<FleetManager>();
        MapManager = GameObject.FindGameObjectWithTag(Tags.Manager).GetComponent<MapManager>();
        TileManager = GameObject.FindGameObjectWithTag(Tags.Manager).GetComponent<TileManager>();

        if (!MouseController || !FleetManager || !MapManager || !TileManager)
        {
            Debug.LogError("MissedComponents!");
        }
    }

    private void Start()
    {
        AddMouseEvents();
    }

    private void Awake()
    {
        Init();
    }

    private void Update()
    {

    }
}
