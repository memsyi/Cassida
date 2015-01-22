using UnityEngine;
using System.Collections.Generic;

public class BaseValues : IJSON
{
    public int HP { get; set; }
    public int MeeleDefense { get; set; }
    public int RangeDefense { get; set; }

    public BaseValues(int hp, int meeleDefense, int rangeDefense)
    {
        HP = hp;
        MeeleDefense = meeleDefense;
        RangeDefense = rangeDefense;
    }

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
    public int ID { get; protected set; }
    public int PlayerID { get; private set; }
    public Position Position { get; private set; }
    public BaseValues BaseValues { get; private set; }
    private List<Building> BuildingList { get; set; }

    public Transform BaseParent { get; protected set; }
    public BaseController BaseController { get; protected set; }
    private List<Transform> ModulPositions { get; set; }

    private bool AlreadyBuildBuilding { get; set; }
    public bool AllowAddBuilding { get { return !AlreadyBuildBuilding && BuildingList.Count < ModulPositions.Count; } }

    public int GoldPerRound { get { return 0; } }

    public Base(int id, int playerID, Position position)
    {
        ID = id;
        PlayerID = playerID;
        Position = position;
        BaseValues = new BaseValues(2, 0, 0);

        AlreadyBuildBuilding = false;

        InitiateValues();
    }

    public Base()
    {

    }

    public void AddBuilding(BuildingType buildingType)
    {
        if (!AllowAddBuilding)
        {
            return;
        }

        BuildingList.Add(new Building(PlayerID, buildingType));
        AlreadyBuildBuilding = true;
    }

    public Transform GetFreeModulPosition()
    {
        if (!AllowAddBuilding)
        {
            return null;
        }

        Transform modulPosition = null;
        while (modulPosition == null)
        {
            var randomModulPosition = Random.Range(0, ModulPositions.Count);
            if (ModulPositions[randomModulPosition].childCount == 0)
            {
                modulPosition = ModulPositions[randomModulPosition];
            }
        }

        return modulPosition;
    }

    public void ResetAlreadyBuildBuilding()
    {
        AlreadyBuildBuilding = false;
    }

    public void BecomeAttacked(int damage)
    {
        BaseValues.HP -= damage;

        CheckWhetherBaseIsAlive();
    }

    private bool CheckWhetherBaseIsAlive()
    {
        if (BaseValues.HP > 0)
        {
            return true;
        }

        // TODO destroy...
        return false;
    }

    private void InitiateValues()
    {
        var player = PlayerManager.Get().GetPlayer(PlayerID);
        BaseParent = BaseController.InstatiateParentObject(Position, player.Name);
        BaseController = BaseParent.gameObject.AddComponent<BaseController>();
        BuildingList = new List<Building>();

        BaseController.InstantiateBase(player.Color);

        ModulPositions = new List<Transform>(BaseParent.GetComponentsInChildren<Transform>()).FindAll(t => t.parent.tag == Tags.ModulPositionObjects);
        Debug.Log(ModulPositions.Count);
    }

    public JSONObject ToJSON()
    {
        var jsonObject = JSONObject.obj;
        jsonObject[JSONs.ID] = new JSONObject(ID);
        jsonObject[JSONs.PlayerID] = new JSONObject(PlayerID);
        jsonObject[JSONs.Position] = Position.ToJSON();
        jsonObject[JSONs.BaseValues] = BaseValues.ToJSON();
        jsonObject[JSONs.Buildings] = JSONObject.CreateList(BuildingList);
        jsonObject[JSONs.AlreadyBuildBuilding] = new JSONObject(AllowAddBuilding);
        return jsonObject;
    }

    public void FromJSON(JSONObject jsonObject)
    {
        ID = (int)jsonObject[JSONs.ID];
        PlayerID = (int)jsonObject[JSONs.PlayerID];
        Position = new Position(jsonObject[JSONs.Position]);
        BaseValues = new BaseValues();
        BaseValues.FromJSON(jsonObject[JSONs.FleetValues]);
        AlreadyBuildBuilding = (bool)jsonObject[JSONs.AlreadyBuildBuilding];

        InitiateValues();
        BaseManager.Get().BaseList.Add(this);

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

    public void InstantiateBase(Color color)
    {
        InstantiateBaseObject(BaseManager.Get().BaseSettings.MainBuildingObject);

        Color = color;
    }

    private void InstantiateBaseObject(Transform model)
    {
        // Instantiate base
        BaseObject = Instantiate(model, transform.position, transform.rotation) as Transform;

        BaseObject.name = "Base Object";
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
