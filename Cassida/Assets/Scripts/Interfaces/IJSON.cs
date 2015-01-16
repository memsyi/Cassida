using UnityEngine;
using System.Collections;

public interface IJSON 
{
    JSONObject ToJSON();
    void FromJSON(JSONObject o);
}
