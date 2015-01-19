using UnityEngine;
using System.Collections.Generic;

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

    public void FromJSON(JSONObject jsonObject)
    {
        throw new System.NotImplementedException();
    }
}

public class Base : IJSON
{
    public int ID { get; private set; }
    public int PlayerID { get; private set; }
    public Position Position { get; private set; }
    public BaseValues BaseValues { get; private set; }
    private List<Building> BuildingList { get; set; }

    public Transform BaseParent { get; protected set; }
    public BaseController BaseController { get; protected set; }

    public bool AllowAddBuilding { get; private set; }

    public Base(int id, int playerID, Position position, BaseValues baseValues)
    {
        ID = id;
        PlayerID = playerID;
        Position = position;
        BaseValues = baseValues;

        AllowAddBuilding = true;

        InitiateValues();
    }

    public Base()
    {

    }

    private void InitiateValues()
    {
        var player = PlayerManager.Get().GetPlayer(PlayerID);
        BaseParent = BaseController.InstatiateParentObject(Position, player.Name);
        BaseController = BaseParent.gameObject.AddComponent<BaseController>();
        BuildingList = new List<Building>();

        BaseController.InstantiateBase(player.Name, player.Color);
    }

    public JSONObject ToJSON()
    {
        var jsonObject = JSONObject.obj;
        jsonObject[JSONs.ID] = new JSONObject(ID);
        jsonObject[JSONs.PlayerID] = new JSONObject(PlayerID);
        jsonObject[JSONs.Position] = Position.ToJSON();
        jsonObject[JSONs.BaseValues] = BaseValues.ToJSON();
        jsonObject[JSONs.Buildings] = JSONObject.CreateList(BuildingList);
        return jsonObject;
    }

    public void FromJSON(JSONObject jsonObject)
    {
        ID = (int)jsonObject[JSONs.ID];
        PlayerID = (int)jsonObject[JSONs.PlayerID];
        Position = new Position(jsonObject[JSONs.Position]);
        BaseValues = new BaseValues();
        BaseValues.FromJSON(jsonObject[JSONs.BaseValues]);

        InitiateValues();
        //BaseManager.Get().AddBase(this);

        BuildingList = JSONObject.ReadList<Building>(jsonObject[JSONs.Buildings]);
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
        fleetParent.position = TileManager.Get().TileList.Find(t => t.Position == position).TileParent.position;
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
        var colorComponentList = new List<Renderer>(BaseObject.GetComponentsInChildren<Renderer>());
        var colorObjectList = new List<Renderer>(colorComponentList.FindAll(t => t.transform.parent.tag == Tags.PlayerColorObjects));
        foreach (var colorObject in colorObjectList)
        {
            colorObject.renderer.material.color = color;
        }
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
