using UnityEngine;
using System.Collections;

public class Position :IJSON
{
    public int X { get; set; }
    public int Y { get; set; }

    public Position(int x, int y)
    {
        X = x;
        Y = y;
    }

    public bool IsSameAs(Position position)
    {
        return X == position.X && Y == position.Y;
    }

    public override string ToString()
    {
        return "(" + X + "; " + Y + ")"; 
    }

    public JSONObject ToJSON()
    {
        var jsonObject = JSONObject.obj;
        jsonObject[JSONs.X] = new JSONObject(X);
        jsonObject[JSONs.Y] = new JSONObject(Y);
        return jsonObject;
    }

    public void FromJSON(JSONObject jsonObject)
    {
         X = (int)jsonObject[JSONs.X];
         Y = (int)jsonObject[JSONs.Y];
    }
}
