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

public class WorldManager : MonoBehaviour
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
                    SetTileBorderColor(CurrentHighlightedTile, TileSettings.MouseOverFleetColor);
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

    private void CheckTileSelection(object sender)
    {
        if (CurrentSelectedTile == MapManager.NearestTileToMousePosition)
        {
            RotateFleet(CurrentSelectedTile.Fleet);
        }

        SelectTile(MapManager.NearestTileToMousePosition);
    }

    private void SelectTile(Tile tile)
    {
        if (CurrentSelectedTile != null)
        {
            SetTileBorderColor(CurrentSelectedTile, TileSettings.DefaultColor);
        }

        if (tile.Fleet != null)
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
    private void CheckFleetMovement(object sender)
    {
        var targetTile = MapManager.NearestTileToMousePosition;

        if (CurrentSelectedTile == null)
        {
            return;
        }

        if (CurrentSelectedTile == targetTile)
        {
            // Rotate Fleet
            RotateFleet(CurrentSelectedTile.Fleet, false);
            return;
        }

        if (CurrentSelectedTile != null && targetTile.Fleet != null)
        {
            // Attack 
            AttackEnemyFleet();
            return;
        }

        // Move fleet
        MoveFleet(CurrentSelectedTile.Fleet, targetTile.TileParent.position);

        targetTile.Fleet = CurrentSelectedTile.Fleet;
        CurrentSelectedTile.Fleet = null;

        SelectTile(targetTile);
    }

    private void RotateFleet(Fleet fleet, bool rotateRight = true)
    {
        fleet.RotateFleet(rotateRight ? 1 : -1);
    }

    private void MoveFleet(Fleet fleet, Vector3 target)
    {
        fleet.MoveFleet(target);
    }
    #endregion

    #region Fight
    private bool CheckAttackEnemyFleet()
    {
        if (CurrentHighlightedTile.Fleet == null)
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

        var ownFleet = CurrentSelectedTile.Fleet;
        var enemyFleet = CurrentHighlightedTile.Fleet;

        var ownUnitStrength = ownFleet.Units[ownUnitDirection].UnitValues.Strength;
        var enemyUnit = enemyFleet.Units[enemyUnitDirection];

        if (enemyUnit == null)
        {
            for (int i = 0; i < enemyFleet.Units.Length; i++)
            {
                enemyFleet.AttackUnit(i, ownUnitStrength);

                if (!enemyFleet.CheckWhetherFleetIsAlive())
                {
                    CurrentHighlightedTile.Fleet = null;
                    break;
                }
            }
        }
        else
        {
            ownFleet.AttackUnit(ownUnitDirection, enemyUnit.UnitValues.Strength);
            enemyFleet.AttackUnit(enemyUnitDirection, ownUnitStrength);

            if (!ownFleet.CheckWhetherFleetIsAlive())
            {
                CurrentSelectedTile.Fleet = null;
            }
            if (!enemyFleet.CheckWhetherFleetIsAlive())
            {
                CurrentHighlightedTile.Fleet = null;
            }
        }

        ResetHighlightedTile();
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

    private void Init()
    {
        MapManager = GameObject.FindGameObjectWithTag(Tags.Manager).GetComponent<MapManager>();
        MouseController = GameObject.FindGameObjectWithTag(Tags.GameController).GetComponent<MouseController>();
    }

    private void Start()
    {
        // Generate map
        TileList = new List<Tile>();
        MapManager.GenerateMap(TileList);

        // Set events
        MouseController.LeftMousecklickEvent += new MouseclickHandler(CheckTileSelection);
        MouseController.RightMouseclickEvent += new MouseclickHandler(CheckFleetMovement);

        TileSettings.DefaultColor = TileList[0].TileParent.renderer.material.color;
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
