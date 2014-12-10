﻿using UnityEngine;
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
        _mouseOverSelectionColor = Color.red;

    #region Tiles
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

    private MapController MapController { get; set; }

    private MouseController MouseController { get; set; }

    private Tile CurrentHighlightedTile { get; set; }

    private Tile CurrentSelectedTile { get; set; }

    private void HighLightNearestTile()
    {
        if (CurrentHighlightedTile == MapController.NearestTileToMousePosition)
        {
            return;
        }

        if (CurrentHighlightedTile != null)
        {
            if (CurrentHighlightedTile == CurrentSelectedTile)
            {
                SetTileBorderColor(CurrentHighlightedTile, TileSettings.SelectionColor);
            }
            else
            {
                SetTileBorderColor(CurrentHighlightedTile, TileSettings.DefaultColor);
            }
        }

        CurrentHighlightedTile = MapController.NearestTileToMousePosition;

        if (CurrentHighlightedTile != null)
        {
            if (CurrentHighlightedTile == CurrentSelectedTile)
            {
                SetTileBorderColor(CurrentHighlightedTile, TileSettings.MouseOverSelectionColor);
            }
            else
            {
                if (CurrentHighlightedTile.Fleet)
                {
                    SetTileBorderColor(CurrentHighlightedTile, TileSettings.MouseOverFleetColor);
                }
                else
                {
                    SetTileBorderColor(CurrentHighlightedTile, TileSettings.MouseOverColor);
                }
            }
        }
    }

    private void CheckTileSelection(object sender)
    {
        if (CurrentSelectedTile == MapController.NearestTileToMousePosition)
        {
            RotateFleet(CurrentSelectedTile.Fleet);
        }

        SelectTile(MapController.NearestTileToMousePosition);
    }

    private void RotateFleet(FleetController fleet, bool turnRight = true)
    {
        fleet.EraseTurnSteps(turnRight ? 1 : -1);
    }

    private void SelectTile(Tile tile)
    {
        if (CurrentSelectedTile != null)
        {
            SetTileBorderColor(CurrentSelectedTile, TileSettings.DefaultColor);
        }

        if (tile.Fleet)
        {
            CurrentSelectedTile = tile;
            SetTileBorderColor(CurrentSelectedTile, TileSettings.MouseOverSelectionColor);
        }
        else
        {
            CurrentSelectedTile = null;
        }
    }

    private void CheckFleetMovement(object sender)
    {
        var targetTile = MapController.NearestTileToMousePosition;

        if (CurrentSelectedTile == null)
            //|| CurrentSelectedTile == targetTile)
        {
            return;
        }

        if (CurrentSelectedTile == targetTile)
        {
            RotateFleet(CurrentSelectedTile.Fleet, false);
            return;
        }

        CurrentSelectedTile.Fleet.MovementTarget = targetTile.TileBorder.position;

        targetTile.Fleet = CurrentSelectedTile.Fleet;
        CurrentSelectedTile.Fleet = null;

        SelectTile(targetTile);
    }

    private void SetTileBorderColor(Tile tile, Color color)
    {
        tile.TileBorder.renderer.material.color = color;
    }

    private void Init()
    {
        MapController = GameObject.FindGameObjectWithTag(Tags.Map).GetComponent<MapController>();
        MouseController = GameObject.FindGameObjectWithTag(Tags.GameController).GetComponent<MouseController>();
    }

    private void Start()
    {
        TileList = new List<Tile>();
        MapController.GenerateMap(TileList);

        MouseController.LeftMousecklickEvent += new MouseclickHandler(CheckTileSelection);
        MouseController.RightMouseclickEvent += new MouseclickHandler(CheckFleetMovement);

        TileSettings.DefaultColor = TileList[0].TileBorder.renderer.material.color;
        TileList.Find(t => t.Position == Vector2.zero).Fleet = GameObject.Find("Fleet").GetComponent<FleetController>();
        print(TileList.Find(t => t.Position == Vector2.zero).Fleet);
    }

    private void Awake()
    {
        Init();
    }

    private void Update()
    {
        HighLightNearestTile();
    }
}