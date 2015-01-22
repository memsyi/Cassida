using UnityEngine;
using System.Collections;

public class BuildingManager : MonoBehaviour
{

    private void Init()
    {
        
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

    }

    private static BuildingManager _instance = null;
    public static BuildingManager Get()
    {
        if (_instance == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag(Tags.Manager);
            _instance = obj.AddComponent<BuildingManager>();
        }

        return _instance;
    }

    public JSONObject ToJSON()
    {
        var jsonObject = JSONObject.obj;
        //jsonObject[JSONs.Bases] = JSONObject.CreateList(BaseList);
        return jsonObject;
    }

    public void FromJSON(JSONObject jsonObject)
    {
        //DestroyAllBases();

        //JSONObject.ReadList<Fleet>(jsonObject[JSONs.Bases]);
    }
}
