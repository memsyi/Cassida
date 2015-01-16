using UnityEngine;
using System.Collections.Generic;

public enum TerrainType
{
    Empty,
    Asteroids,
    Nebula,
    EnergyField,
    BlackHole
}

public enum ObjectiveType
{
    Empty,
    Base,
    Rubble,
    Village,
    Town,
    TradingStation,
    Outpost
}

public class Tile : IJSON
{
    public Position Position { get; protected set; }
    public int FleetID { get { var fleet = FleetManager.Get().FleetList.Find(f => f.Position == this.Position); return fleet != null ? fleet.ID : -1; } }
    public TerrainType TerrainType { get; private set; }
    public ObjectiveType ObjectiveType { get; private set; }

    public Transform TileParent { get; protected set; }
    public Transform TileObject { get; private set; }

    public TerrainController TerrainController { get; private set; }
    public ObjectiveController ObjectiveController { get; private set; }

    public Tile(Position position, TerrainType terrain, ObjectiveType objective)
    {
        TileParent = MapGenerator.InstatiateParentObject(position);
        TileObject = MapGenerator.Get().InstatiateTileObject(TileParent);

        Position = position;
        TerrainType = terrain;
        ObjectiveType = objective;

        SetCorrectTerrain();
        SetCorrectObjective();
    }

    private void SetCorrectTerrain()
    {
        if (TerrainType == TerrainType.Empty)
        {
            return;
        }

        TerrainController = TileParent.gameObject.AddComponent<TerrainController>();
        TerrainController.InstantiateTerrain(TerrainType);
    }

    private void SetCorrectObjective()
    {
        if (ObjectiveController)
        {
            ObjectiveController.DeletObjective();
            ObjectiveController = null;
        }

        if (ObjectiveType == ObjectiveType.Empty)
        {
            return;
        }

        ObjectiveController = TileParent.gameObject.AddComponent<ObjectiveController>();
        if (ObjectiveType != ObjectiveType.Base)
        {
            ObjectiveController.InstantiateObjective(ObjectiveType);
        }
    }

    public JSONObject ToJSON()
    {
        var jsonObject = JSONObject.obj;
        jsonObject[JSONs.Position] = Position.ToJSON();
        jsonObject[JSONs.TerrainType] = new JSONObject((int)TerrainType);
        jsonObject[JSONs.ObjectiveType] = new JSONObject((int)ObjectiveType);
        return jsonObject;
    }

    public void FromJSON(JSONObject o)
    {
        throw new System.NotImplementedException();
    }
}

[System.Serializable]
public class SettingsTileColor
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

    #region TileColor
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

[System.Serializable]
public class SettingsTileAnimation
{
    [SerializeField]
    private bool
        _allowAnimation = true,
        _smothAnimation = false;

    [SerializeField]
    private float
        _animationSpeed = 1.0f,
        _animationRange = 1.0f,
        _animationFadeOut = 1.0f;

    [SerializeField]
    private int _animatedObjectCount = 2;

    #region TileAnimation
    public float AnimationRange
    {
        get { return _animationRange; }
        set { _animationRange = value; }
    }

    public float AnimationSpeed
    {
        get { return _animationSpeed; }
        set { _animationSpeed = value; }
    }

    public int AnimatedObjectCount
    {
        get { return _animatedObjectCount; }
        set { _animatedObjectCount = value; }
    }

    public bool AllowAnimation
    {
        get { return _allowAnimation; }
        set { _allowAnimation = value; }
    }

    public bool SmothAnimation
    {
        get { return _smothAnimation; }
        set { _smothAnimation = value; }
    }
    public float AnimationFadeOut
    {
        get { return _animationFadeOut; }
        set { _animationFadeOut = value; }
    }
    #endregion

    private bool _backwardAnimation;

    public bool BackwardAnimation
    {
        get
        {
            if (!SmothAnimation) return false;

            return _backwardAnimation;
        }
        set { _backwardAnimation = value; }
    }
    public List<Transform> SelectionObjects { get; set; }

    public SettingsTileAnimation()
    {
        SelectionObjects = new List<Transform>();
    }
}

public class TileManager : MonoBehaviour, IJSON
{
    #region Variables
    [SerializeField]
    SettingsTileColor _tileColor;

    [SerializeField]
    SettingsTileAnimation _tileAnimation;

    public SettingsTileColor TileColor
    {
        get { return _tileColor; }
        set { _tileColor = value; }
    }
    public SettingsTileAnimation TileAnimation
    {
        get { return _tileAnimation; }
        set { _tileAnimation = value; }
    }

    // Tiles
    public Tile CurrentHighlightedTile { get; private set; }
    public Tile CurrentSelectedTile { get; private set; }

    // Lists
    public List<Tile> TileList { get; private set; }
    private List<Fleet> FleetList { get { return FleetManager.Get().FleetList; } }
    #endregion

    public void SelectTile(Tile tile)
    {
        if (CurrentSelectedTile != null)
        {
            SetTileBorderColor(CurrentSelectedTile, TileColor.DefaultColor);
            RemoveCurrentSelectionAnimation();
        }

        var fleet = FleetList.Find(f => f.Position == tile.Position);

        if (tile == null || fleet == null || fleet.Player.ID != PlayerManager.Get().Player.ID)
        {
            CurrentSelectedTile = null;
            return;
        }

        CurrentSelectedTile = tile;
        SetTileBorderColor(CurrentSelectedTile, TileColor.MouseOverSelectionColor);

        if (TileAnimation.AllowAnimation)
        {
            InitiateSelectionAnimation(tile);
        }
    }

    public void ResetAllTiles() // TODO event
    {
        ResetSelectedTile();
        ResetHighlightedTile();
        RemoveCurrentSelectionAnimation();
    }

    #region Selected tile animation
    private void InitiateSelectionAnimation(Tile tile)
    {
        foreach (var selectionObject in TileAnimation.SelectionObjects)
        {
            selectionObject.position = tile.TileParent.position;
            selectionObject.renderer.material.color = TileColor.MouseOverColor;
            selectionObject.gameObject.SetActive(true);
        }
    }

    private void RemoveCurrentSelectionAnimation()
    {
        foreach (var selectionObject in TileAnimation.SelectionObjects)
        {
            selectionObject.gameObject.SetActive(false);
        }
    }

    private void AnimateSelection()
    {
        if (TileAnimation.SelectionObjects.Count == 0 || TileAnimation.AnimationRange == 0 || !TileAnimation.AllowAnimation)
        {
            return;
        }

        float currentAnimatedObjectPosition = TileAnimation.SelectionObjects[0].position.y;

        currentAnimatedObjectPosition += Time.deltaTime * TileAnimation.AnimationSpeed * 0.1f * (!TileAnimation.BackwardAnimation ? 1 : -1);

        if (currentAnimatedObjectPosition > TileAnimation.AnimationRange / 10
            || currentAnimatedObjectPosition < 0)
        {
            if (TileAnimation.SmothAnimation && currentAnimatedObjectPosition > TileAnimation.AnimationRange / 10)
            {
                TileAnimation.BackwardAnimation = true;
            }
            else if (TileAnimation.SmothAnimation && currentAnimatedObjectPosition < 0)
            {
                TileAnimation.BackwardAnimation = false;
                foreach (var selectionObject in TileAnimation.SelectionObjects)
                {
                    selectionObject.position = CurrentSelectedTile.TileParent.position;
                    selectionObject.transform.renderer.material.color = TileColor.MouseOverColor;
                }
            }
            else
            {
                foreach (var selectionObject in TileAnimation.SelectionObjects)
                {
                    selectionObject.position = CurrentSelectedTile.TileParent.position;
                    selectionObject.transform.renderer.material.color = TileColor.MouseOverColor;
                }
            }

            return;
        }

        for (int i = 0; i < TileAnimation.SelectionObjects.Count; i++)
        {
            var selectionObject = TileAnimation.SelectionObjects[i];

            selectionObject.position = new Vector3(selectionObject.position.x, currentAnimatedObjectPosition * (i % 2 == 0 ? 1 : -1), selectionObject.position.z);
        }

        if (TileAnimation.BackwardAnimation)
        {
            foreach (var selectionObject in TileAnimation.SelectionObjects)
            {
                selectionObject.transform.renderer.material.color *= new Color(1, 1, 1, 1 + (currentAnimatedObjectPosition / TileAnimation.AnimationRange) * TileAnimation.AnimationFadeOut);
            }
        }
        else
        {
            foreach (var selectionObject in TileAnimation.SelectionObjects)
            {
                selectionObject.transform.renderer.material.color *= new Color(1, 1, 1, 1 - (currentAnimatedObjectPosition / TileAnimation.AnimationRange) * TileAnimation.AnimationFadeOut);
            }
        }
    }
    #endregion

    #region Highlight tiles
    private void HighLightNearestTile()
    {
        var nearestTile = MapManager.Get().NearestTileToMousePosition;
        if (CurrentHighlightedTile == nearestTile)
        {
            return;
        }

        ResetHighlightedTile();

        CurrentHighlightedTile = nearestTile;

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
                SetTileBorderColor(CurrentHighlightedTile, TileColor.MouseOverSelectionColor);
            }
            // Other tile
            else
            {
                // Enemies fleet (own selected)
                if (CurrentSelectedTile != null && CurrentHighlightedTile.FleetID > -1)
                {
                    if (InputManager.Get().CheckAttack(CurrentSelectedTile.FleetID, CurrentHighlightedTile.FleetID))
                    {
                        SetTileBorderColor(CurrentHighlightedTile, TileColor.MouseOverEnemyFleetColor);
                    }
                    else
                    {
                        SetTileBorderColor(CurrentHighlightedTile, TileColor.MouseOverCantMoveColor);
                    }
                }
                // Other fleet
                else if (CurrentHighlightedTile.FleetID > -1)
                {
                    var fleet = FleetList.Find(f => f.ID == CurrentHighlightedTile.FleetID);
                    if (fleet != null && fleet.Player.ID == PlayerManager.Get().Player.ID)
                    {
                        SetTileBorderColor(CurrentHighlightedTile, TileColor.MouseOverFleetColor);
                    }
                    else
                    {
                        SetTileBorderColor(CurrentHighlightedTile, TileColor.MouseOverEnemyFleetColor);
                    }
                }
                // Default tile
                else
                {
                    SetTileBorderColor(CurrentHighlightedTile, TileColor.MouseOverColor);
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
                SetTileBorderColor(CurrentHighlightedTile, TileColor.SelectionColor);
            }
            // Default tile
            else
            {
                SetTileBorderColor(CurrentHighlightedTile, TileColor.DefaultColor);
            }
        }

        CurrentHighlightedTile = null;
    }

    private void ResetSelectedTile()
    {
        SetTileBorderColor(CurrentSelectedTile, TileColor.DefaultColor);
        CurrentSelectedTile = null;
    }
    #endregion

    private void SetTileBorderColor(Tile tile, Color color)
    {
        if (tile == null) { return; } 
        tile.TileObject.renderer.material.color = color;
    }

    private void AddMouseEvents()
    {
        // Add events
        MouseController.Get().LeftMousecklickEvent += new MouseclickHandler(ResetHighlightedTile);
        MouseController.Get().RightMouseclickEvent += new MouseclickHandler(ResetHighlightedTile);
    }

    private void Init()
    {
        TileList = new List<Tile>();

        for (int i = 0; i < TileAnimation.AnimatedObjectCount; i++)
        {
            var tileAnimationObject = Instantiate(MapGenerator.Get().TileObject) as Transform;

            TileAnimation.SelectionObjects.Add(tileAnimationObject);
            TileAnimation.SelectionObjects[i].gameObject.SetActive(false);
        }
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
        // Check highlighting
        HighLightNearestTile();

        if (CurrentSelectedTile != null)
        {
            AnimateSelection();
            if (!TileAnimation.AllowAnimation && TileAnimation.SelectionObjects[0].gameObject.activeSelf) RemoveCurrentSelectionAnimation();
            else if (TileAnimation.AllowAnimation && !TileAnimation.SelectionObjects[0].gameObject.activeSelf)
            {
                foreach (var selectionObject in TileAnimation.SelectionObjects)
                {
                    selectionObject.gameObject.SetActive(true);
                }
            }
        }
    }

    private static TileManager _instance = null;
    public static TileManager Get()
    {
        if (_instance == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag(Tags.Manager);
            _instance = obj.AddComponent<TileManager>();
        }

        return _instance;
    }

    public JSONObject ToJSON()
    {
        var jsonObject = JSONObject.obj;
        var tileObjects = JSONObject.arr;
        foreach (var tile in TileList)
        {
            tileObjects.Add(tile.ToJSON());
        }
        jsonObject[JSONs.Tiles] = tileObjects;
        return jsonObject;
    }

    public void FromJSON(JSONObject o)
    {
        throw new System.NotImplementedException();
    }
}
