using UnityEngine;
using System.Collections;

public class BuildingValues : IJSON
{
    public BuildingValues()
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

public class Building :IJSON
{

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

public class BuildingController : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
