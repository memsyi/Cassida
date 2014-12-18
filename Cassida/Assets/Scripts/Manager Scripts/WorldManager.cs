using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SettingsTileSettings
{
    [SerializeField]
    private Color
        _defaultColor = Color.grey,
        _mouseOverColor = Color.cyan,
        _mouseOverFleetColor = Color.blue,
        _selectionColor = Color.white,
        _mouseOverSelectionColor = Color.red,
        _mouseOverEnemyFleetColor = Color.magenta,
        _mouseOverCantMoveColor = Color.red;

    #region Tiles
    public Color MouseOverCantMoveColor
    {
        get { return _mouseOverCantMoveColor; }
        set { _mouseOverCantMoveColor = value; }
    }
    public Color MouseOverEnemyFleetColor
    {
        get { return _mouseOverEnemyFleetColor; }
        set { _mouseOverEnemyFleetColor = value; }
    }
    public Color MouseOverSelectionColor
    {
        get { return _mouseOverSelectionColor; }
        set { _mouseOverSelectionColor = value; }
    }
    public Color SelectionColor
    {
        get { return _selectionColor; }
        set { _selectionColor = value; }
    }
    public Color MouseOverFleetColor
    {
        get { return _mouseOverFleetColor; }
        set { _mouseOverFleetColor = value; }
    }
    public Color MouseOverColor
    {
        get { return _mouseOverColor; }
        set { _mouseOverColor = value; }
    }
    public Color DefaultColor
    {
        get { return _defaultColor; }
        set { _defaultColor = value; }
    }
    #endregion
}

[RequireComponent(typeof(PhotonView))]
public class WorldManager : Photon.MonoBehaviour
{
    [SerializeField]
    SettingsTileSettings _tileSettings;

    public SettingsTileSettings TileSettings
    {
        get { return _tileSettings; }
        set { _tileSettings = value; }
    }

    public List<Tile> TileList { get; private set; }

    private MapManager MapManager { get; set; }
    private MouseController MouseController { get; set; }

    private Tile CurrentHighlightedTile { get; set; }
    private Tile CurrentSelectedTile { get; set; }

    #region Highlight tiles
    private void HighLightNearestTile()
    {
        if (CurrentHighlightedTile == MapManager.NearestTileToMousePosition)
        {
            return;
        }

        ResetHighlightedTile();

        CurrentHighlightedTile = MapManager.NearestTileToMousePosition;

        HighlightTile();
    }

    private void HighlightTile()
    {
        // If mouse is over new tile
        if (CurrentHighlightedTile != null)
        {
            // Selected tile
            if (CurrentHighlightedTile == CurrentSelectedTile)
            {
                SetTileBorderColor(CurrentHighlightedTile, TileSettings.MouseOverSelectionColor);
            }
            // Other tile
            else
            {
                // Enemies fleet (own selected)
                if (CurrentSelectedTile != null && CurrentHighlightedTile.Fleet != null)
                {
                    if (CheckAttackEnemyFleet())
                    {
                        SetTileBorderColor(CurrentHighlightedTile, TileSettings.MouseOverEnemyFleetColor);
                    }
                    else
                    {
                        SetTileBorderColor(CurrentHighlightedTile, TileSettings.MouseOverCantMoveColor);
                    }
                }
                // Other fleet
                else if (CurrentHighlightedTile.Fleet != null)
                {
                    if (CurrentHighlightedTile.Fleet.Player == PhotonNetwork.player)
                    {
                        SetTileBorderColor(CurrentHighlightedTile, TileSettings.MouseOverFleetColor);
                    }
                    else
                    {
                        SetTileBorderColor(CurrentHighlightedTile, TileSettings.MouseOverEnemyFleetColor);
                    }
                }
                // Default tile
                else
                {
                    SetTileBorderColor(CurrentHighlightedTile, TileSettings.MouseOverColor);
                }
            }
        }
    }

    private void ResetHighlightedTile()
    {
        // If mouse is not over tile any more
        if (CurrentHighlightedTile != null)
        {
            // Selected tile
            if (CurrentHighlightedTile == CurrentSelectedTile)
            {
                SetTileBorderColor(CurrentHighlightedTile, TileSettings.SelectionColor);
            }
            // Default tile
            else
            {
                SetTileBorderColor(CurrentHighlightedTile, TileSettings.DefaultColor);
            }
        }

        CurrentHighlightedTile = null;
    }
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
            RotateFleet(CurrentSelectedTile.Position);
        }

        SelectTile(MapManager.NearestTileToMousePosition);
    }

    private void SelectTile(Tile tile)
    {
        if (CurrentSelectedTile != null)
        {
            SetTileBorderColor(CurrentSelectedTile, TileSettings.DefaultColor);
        }

        if (tile.Fleet != null && tile.Fleet.Player == PhotonNetwork.player)
        {
            CurrentSelectedTile = tile;
            SetTileBorderColor(CurrentSelectedTile, TileSettings.MouseOverSelectionColor);
        }
        else
        {
            CurrentSelectedTile = null;
        }
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
            RotateFleet(CurrentSelectedTile.Position, false);
            return;
        }

        if (CurrentSelectedTile != null && targetTile.Fleet != null)
        {
            // Attack 
            AttackEnemyFleet();
            return;
        }

        // Move fleet
        MoveFleet(CurrentSelectedTile.Position, targetTile.Position);

        SelectTile(targetTile);
    }

    private void RotateFleet(Vector2 tilePositionOfFleet, bool rotateRight = true)
    {
        photonView.RPC("RotateFleetOverNetwork", PhotonTargets.AllBuffered, tilePositionOfFleet, rotateRight);
    }

    [RPC]
    private void RotateFleetOverNetwork(Vector2 tilePositionOfFleet, bool rotateRight)
    {
        var fleet = TileList.Find(t => t.Position == tilePositionOfFleet).Fleet;

        fleet.RotateFleet(rotateRight ? 1 : -1);
    }

    private void MoveFleet(Vector2 tilePositionOfFleet, Vector2 targetTilePosition)
    {
        photonView.RPC("MoveFleetOverNetwork", PhotonTargets.AllBuffered, tilePositionOfFleet, targetTilePosition);
    }

    [RPC]
    private void MoveFleetOverNetwork(Vector2 tilePositionOfFleet, Vector2 targetTilePosition)
    {
        var currentTile = TileList.Find(t => t.Position == tilePositionOfFleet);
        var targetTile = TileList.Find(t => t.Position == targetTilePosition);
        var fleet = currentTile.Fleet;

        targetTile.Fleet = fleet;
        currentTile.Fleet = null;

        fleet.MoveFleet(targetTile.TileParent.position);
    }
    #endregion

    #region Fight
    private bool CheckAttackEnemyFleet()
    {
        if (CurrentHighlightedTile == null || CurrentHighlightedTile.Fleet == null || CurrentHighlightedTile.Fleet.Player == PhotonNetwork.player)
        {
            return false;
        }

        var unitDirection = GetOwnUnitInDirection();

        if (unitDirection < 0)
        {
            return false;
        }

        var ownUnit = CurrentSelectedTile.Fleet.Units[unitDirection];

        if (ownUnit != null)
        {
            return ownUnit.UnitController != null;
        }

        return false;
    }

    private void AttackEnemyFleet()
    {
        if (!CheckAttackEnemyFleet())
        {
            return;
        }

        var ownUnitDirection = GetOwnUnitInDirection();
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
                AttackUnitOfFleet(enemyFleetTile.Position, i, ownUnitStrength);
            }
        }
        else
        {
            AttackUnitOfFleet(ownFleetTile.Position, ownUnitDirection, enemyUnit.UnitValues.Strength);
            AttackUnitOfFleet(enemyFleetTile.Position, enemyUnitDirection, ownUnitStrength);
        }

        ResetHighlightedTile();
    }

    private void AttackUnitOfFleet(Vector2 tilePositionOfFleet, int unitPosition, int strength)
    {
        photonView.RPC("AttackUnitOFFleetOverNetwork", PhotonTargets.AllBuffered, tilePositionOfFleet, unitPosition, strength);
    }

    [RPC]
    private void AttackUnitOFFleetOverNetwork(Vector2 tilePositionOfFleet, int unitPosition, int strength)
    {
        var tileOfFleet = TileList.Find(t => t.Position == tilePositionOfFleet);
        var fleet = tileOfFleet.Fleet;

        if (fleet == null)
        {
            return;
        }

        fleet.AttackUnit(unitPosition, strength);

        if (!fleet.CheckWhetherFleetIsAlive())
        {
            tileOfFleet.Fleet = null;
            ResetHighlightedTile();
        }
    }

    private int GetOwnUnitInDirection()
    {
        if (CurrentHighlightedTile.Position.x == CurrentSelectedTile.Position.x)
        {
            if (CurrentHighlightedTile.Position.y > CurrentSelectedTile.Position.y)
            {
                return 0;
            }

            return 3;
        }
        else if (CurrentHighlightedTile.Position.y == CurrentSelectedTile.Position.y)
        {
            if (CurrentHighlightedTile.Position.x > CurrentSelectedTile.Position.x)
            {
                return 1;
            }

            return 4;
        }
        else
        {
            if (CurrentHighlightedTile.Position.x < CurrentSelectedTile.Position.x
                && CurrentHighlightedTile.Position.y > CurrentSelectedTile.Position.y)
            {
                return 5;
            }
            else if (CurrentHighlightedTile.Position.x > CurrentSelectedTile.Position.x
                && CurrentHighlightedTile.Position.y < CurrentSelectedTile.Position.y)
            {
                return 2;
            }
        }

        return -1;
    }
    #endregion

    private void SetTileBorderColor(Tile tile, Color color)
    {
        tile.TileParent.renderer.material.color = color;
    }

    private void AddMouseEvents()
    {
        // Add events
        MouseController.LeftMousecklickEvent += new MouseclickHandler(CheckTileSelection);
        MouseController.RightMouseclickEvent += new MouseclickHandler(CheckFleetMovement);

        MouseController.LeftMousecklickEvent += new MouseclickHandler(ResetHighlightedTile);
        MouseController.RightMouseclickEvent += new MouseclickHandler(ResetHighlightedTile);
    }

    private void InitializeMap()
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        // Generate map
        MapManager.GenerateMap();
    }

    public void InitializeWorld()
    {
        InitializeMap();
    }

    private void Init()
    {
        MapManager = GameObject.FindGameObjectWithTag(Tags.Manager).GetComponent<MapManager>();
        MouseController = GameObject.FindGameObjectWithTag(Tags.GameController).GetComponent<MouseController>();

        if (!MapManager || !MouseController)
        {
            Debug.LogError("MissedComponents!");
        }
    }

    private void Start()
    {
        TileList = new List<Tile>();

        AddMouseEvents();
    }

    private void Awake()
    {
        Init();
    }

    private void Update()
    {
        // Check highlighting
        HighLightNearestTile();
    }
}
