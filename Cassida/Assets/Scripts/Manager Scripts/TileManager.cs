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

public class TileManager : MonoBehaviour
{
    [SerializeField]
    SettingsTileSettings _tileSettings;

    #region Variables
    public SettingsTileSettings TileSettings
    {
        get { return _tileSettings; }
        set { _tileSettings = value; }
    }

    // Scripts
    private MouseController MouseController { get; set; }
    private MapManager MapManager { get; set; }

    // Lists
    public List<Tile> TileList { get; private set; }

    // Tiles
    public Tile CurrentHighlightedTile { get; private set; }
    public Tile CurrentSelectedTile { get; private set; }
    #endregion

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

    public void SelectTile(Tile tile)
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

    public void ResetHighlightedTile()
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

    #region Check Fleets and Units on tiles
    public bool CheckAttackEnemyFleet()
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

    public int GetOwnUnitInDirection()
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
        MouseController.LeftMousecklickEvent += new MouseclickHandler(ResetHighlightedTile);
        MouseController.RightMouseclickEvent += new MouseclickHandler(ResetHighlightedTile);
    }

    private void Init()
    {
        MouseController = GameObject.FindGameObjectWithTag(Tags.GameController).GetComponent<MouseController>();
        MapManager = GameObject.FindGameObjectWithTag(Tags.Manager).GetComponent<MapManager>();

        if (!MouseController || !MapManager)
        {
            Debug.LogError("MissedComponents!");
        }
    }

    private void Start()
    {
        TileList = new List<Tile>();
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
