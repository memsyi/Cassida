using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SettingsTileSettings
{
    [SerializeField]
    private Color
        _defaultColor = Color.cyan,
        _mouseOverColor = Color.blue,
        _selectionColor = Color.white;

    #region Tiles
    public Color SelectionColor
    {
        get { return _selectionColor; }
        set { _selectionColor = value; }
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

    private Tile CurrentHighlightedTile { get; set; }

    private void HighLightNearestTile()
    {
        if (CurrentHighlightedTile != null)
        {
            CurrentHighlightedTile.TileBorder.renderer.material.color = TileSettings.DefaultColor;
        }

        CurrentHighlightedTile = MapController.NearestTileToMousePosition;

        if (CurrentHighlightedTile != null)
        {
            CurrentHighlightedTile.TileBorder.renderer.material.color = TileSettings.MouseOverColor;
        }
    }

    private void GenerateMap()
    {
        var mapGenerator = GameObject.FindGameObjectWithTag(Tags.Generators).GetComponent<MapGenerator>();

        if (!mapGenerator)
        {
            Debug.LogError("Add MapGenerator to Generators object");
            return;
        }

        mapGenerator.GenerateMap(TileList);
    }

    private void Init()
    {
        MapController = GameObject.FindGameObjectWithTag(Tags.Map).GetComponent<MapController>();
        TileList = new List<Tile>();
        GenerateMap();

        TileSettings.DefaultColor = TileList[0].TileBorder.renderer.material.color;
    }

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        HighLightNearestTile();
    }
}
