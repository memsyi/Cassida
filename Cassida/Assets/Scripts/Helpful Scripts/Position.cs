using UnityEngine;
using System.Collections;

public struct Position : IJSON
{
    public int X { get; set; }
    public int Y { get; set; }

    public Position(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Position(JSONObject jsonsObject)
    {
        X = 0;
        Y = 0;
        FromJSON(jsonsObject);
    }

    public static bool operator ==(Position positionA, Position positionB)
    {
        return positionB.X == positionA.X && positionB.Y == positionA.Y;
    }

    public static bool operator !=(Position positionA, Position positionB)
    {
        return !(positionA == positionB);
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
