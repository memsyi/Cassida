using UnityEngine;

public class BaseValues : IJSON
{
    public BaseValues()
    {

    }

    public JSONObject ToJSON()
    {
        var jsonObject = JSONObject.obj;

        return jsonObject;
    }

    public void FromJSON(JSONObject o)
    {
        throw new System.NotImplementedException();
    }
}

public class Base : IJSON
{
    public int ID { get; private set; }
    public Player Player { get; private set; }
    public Position Position { get; private set; }
    public BaseValues BaseValues { get; private set; }
    private Building[] Buildings { get; set; }

    public Transform BaseParent { get; protected set; }
    public BaseController BaseController { get; protected set; }

    public bool AllowAddBuilding { get; private set; }

    public Base(int id, Player player, Position position, BaseValues baseValues)
    {
        // Parent object and controller must be first!
        BaseParent = BaseController.InstatiateParentObject(position, player.Name);
        BaseController = BaseParent.gameObject.AddComponent<BaseController>();
        Buildings = new Building[10];

        ID = id;
        Player = player;
        Position = position;
        BaseValues = baseValues;

        AllowAddBuilding = true;

        BaseController.InstantiateBase(player.Name, player.Color);
    }

    public JSONObject ToJSON()
    {
        var jsonObject = JSONObject.obj;

        return jsonObject;
    }

    public void FromJSON(JSONObject o)
    {
        throw new System.NotImplementedException();
    }
}

public class BaseController : MonoBehaviour
{
    #region Object and Instantiation
    private Transform BaseObject { get; set; }
    private Color color;
    private Color Color { get { return color; } set { SetColorOfBase(value); color = value; } }

    public static Transform InstatiateParentObject(Position position, string playerName)
    {
        var fleetParent = new GameObject("Base of: " + playerName).transform;
        fleetParent.position = TileManager.Get().TileList.Find(t => t.Position.IsSameAs(position)).TileParent.position;
        fleetParent.SetParent(GameObject.Find(Tags.Bases).transform);
        return fleetParent;
    }

    public void InstantiateBase(string playerName, Color color)
    {
        InstantiateBaseObject(BaseManager.Get().BaseSettings.MainBuildingObject, playerName);

        Color = color;
    }

    private void InstantiateBaseObject(Transform model, string playerName)
    {
        // Instantiate base
        BaseObject = Instantiate(model, transform.position, transform.rotation) as Transform;

        BaseObject.name = "Base Object: " + playerName;
        BaseObject.SetParent(transform);
    }
    private void SetColorOfBase(Color color)
    {
        BaseObject.GetChild(0).renderer.material.color = color;
    }
    #endregion

    private void Init()
    {

    }

    private void Start()
    {

    }

    private void Awake()
    {
        Init();
    }

    private void Update()
    {
        
    }
}
