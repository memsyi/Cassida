using UnityEngine;
using System.Collections.Generic;

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

    public float AnimationFadeOut
    {
        get { return _animationFadeOut; }
        set { _animationFadeOut = value; }
    }

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

    public bool BackwardAnimation { get; set; }
    public List<Transform> SelectionObjects { get; set; }

    public SettingsTileAnimation()
    {
        SelectionObjects = new List<Transform>();
    }
}

public class TileManager : MonoBehaviour
{
    [SerializeField]
    SettingsTileColor _tileColor;

    [SerializeField]
    SettingsTileAnimation _tileAnimation;

    #region Variables
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

    // Scripts
    private MouseController MouseController { get; set; }
    private MapManager MapManager { get; set; }
    private MapGenerator MapGenerator { get; set; }

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
            SetTileBorderColor(CurrentSelectedTile, TileColor.DefaultColor);

            RemoveCurrentSelectionAnimation();
        }

        if (tile.Fleet != null && tile.Fleet.Player == PhotonNetwork.player)
        {
            CurrentSelectedTile = tile;
            SetTileBorderColor(CurrentSelectedTile, TileColor.MouseOverSelectionColor);

            if (TileAnimation.AllowAnimation)
            {
                InitiateSelectionAnimation(tile);
            }
        }
        else
        {
            CurrentSelectedTile = null;
        }
    }

    public void ResetAllTiles()
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
        if (TileAnimation.SelectionObjects.Count == 0 || TileAnimation.AnimationRange == 0)
        {
            return;
        }

        var currentAnimatedObjectPosition = TileAnimation.SelectionObjects[0].position.y;

        currentAnimatedObjectPosition += Time.deltaTime * TileAnimation.AnimationSpeed * 0.1f * (!TileAnimation.BackwardAnimation ? 1 : -1);

        if (currentAnimatedObjectPosition > TileAnimation.AnimationRange / 10
            || currentAnimatedObjectPosition < 0)
        {
            if (TileAnimation.SmothAnimation)
            {
                TileAnimation.BackwardAnimation = !TileAnimation.BackwardAnimation;
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
        //else if (TileAnimation.BackwardAnimation & currentAnimatedObjectPosition < 0)
        //{
        //    TileAnimation.BackwardAnimation = false;

        //    foreach (var selectionObject in TileAnimation.SelectionObjects)
        //    {
        //        selectionObject.position.Set(selectionObject.position.x, 0, selectionObject.position.z);
        //        selectionObject.renderer.material.color = TileColor.MouseOverColor;
        //    }
        //}

        for (int i = 0; i < TileAnimation.SelectionObjects.Count; i++)
        {
            var selectionObject = TileAnimation.SelectionObjects[i];

            selectionObject.position = new Vector3(selectionObject.position.x, currentAnimatedObjectPosition * (i % 2 == 0 ? 1 : -1), selectionObject.position.z);
        }

        //SelectionOne.transform.position = new Vector3(SelectionOne.transform.position.x, curentAnimationPosition.y, SelectionOne.transform.position.z);
        //SelectionTwo.transform.position = new Vector3(SelectionOne.transform.position.x, -curentAnimationPosition.y, SelectionOne.transform.position.z);

        if (TileAnimation.BackwardAnimation)
        {
            foreach (var selectionObject in TileAnimation.SelectionObjects)
            {
                //selectionObject.renderer.material.color = Color.Lerp(selectionObject.renderer.material.color, TileColor.MouseOverColor, Time.deltaTime);
                //selectionObject.transform.renderer.material.color *= new Color(1, 1, 1, 1 + TileAnimation.AnimationFadeOut / 25);
                selectionObject.transform.renderer.material.color *= TileColor.MouseOverColor * 1 / currentAnimatedObjectPosition / TileAnimation.AnimationRange;
            }

            //SelectionOne.transform.renderer.material.color *= new Color(1, 1, 1, 1 + animationFadeOut / 25);
            //SelectionTwo.transform.renderer.material.color *= new Color(1, 1, 1, 1 + animationFadeOut / 25);
        }
        else
        {
            foreach (var selectionObject in TileAnimation.SelectionObjects)
            {
                //selectionObject.renderer.material.color = Color.Lerp(selectionObject.renderer.material.color, Color.clear, Time.deltaTime);
                selectionObject.transform.renderer.material.color *= new Color(1, 1, 1, 1 - currentAnimatedObjectPosition / TileAnimation.AnimationRange);
                //selectionObject.transform.renderer.material.color *= Color.clear * currentAnimatedObjectPosition / TileAnimation.AnimationRange;
            }

            //SelectionOne.transform.renderer.material.color *= new Color(1, 1, 1, 1 - animationFadeOut / 25);
            //SelectionTwo.transform.renderer.material.color *= new Color(1, 1, 1, 1 - animationFadeOut / 25);
        }
    }
    #endregion

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
                SetTileBorderColor(CurrentHighlightedTile, TileColor.MouseOverSelectionColor);
            }
            // Other tile
            else
            {
                // Enemies fleet (own selected)
                if (CurrentSelectedTile != null && CurrentHighlightedTile.Fleet != null)
                {
                    if (CheckAttackEnemyFleet())
                    {
                        SetTileBorderColor(CurrentHighlightedTile, TileColor.MouseOverEnemyFleetColor);
                    }
                    else
                    {
                        SetTileBorderColor(CurrentHighlightedTile, TileColor.MouseOverCantMoveColor);
                    }
                }
                // Other fleet
                else if (CurrentHighlightedTile.Fleet != null)
                {
                    if (CurrentHighlightedTile.Fleet.Player == PhotonNetwork.player)
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

        var ownFleet = CurrentSelectedTile.Fleet;
        var enemyFleet = CurrentHighlightedTile.Fleet;
        var ownUnit = ownFleet.Units[unitDirection];

        if (ownUnit != null)
        {
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
        }

        return false;
    }

    public int GetOwnUnitInDirection()
    {
        var ownFleetPosition = CurrentSelectedTile.Position;
        var enemyFleetPosition = CurrentHighlightedTile.Position;

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

    private void SetTileBorderColor(Tile tile, Color color)
    {
        if (tile == null)
        {
            return;
        }

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
        MapGenerator = GameObject.FindGameObjectWithTag(Tags.Map).GetComponent<MapGenerator>();

        if (!MouseController || !MapManager || !MapGenerator)
        {
            Debug.LogError("MissedComponents!");
        }
    }

    private void Start()
    {
        TileList = new List<Tile>();

        for (int i = 0; i < TileAnimation.AnimatedObjectCount; i++)
        {
            TileAnimation.SelectionObjects.Add(Instantiate(MapGenerator.TileParent) as Transform);
            TileAnimation.SelectionObjects[i].gameObject.SetActive(false);
        }
    }

    private void Awake()
    {
        Init();
    }

    private void Update()
    {
        // Check highlighting
        HighLightNearestTile();

        if (CurrentSelectedTile != null)
        {
            AnimateSelection();
        }
    }
}
