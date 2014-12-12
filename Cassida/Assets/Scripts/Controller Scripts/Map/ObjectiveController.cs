using UnityEngine;
using System.Collections;

public class ObjectiveController : MonoBehaviour
{
    private ObjectiveType _type;

    public ObjectiveType Type
    {
        get
        {
            return _type;
        }
        set
        {
            _type = value;
            InstantiateObjectiveObject();
        }
    }

    private void InstantiateObjectiveObject()
    {
        switch (Type)
        {
            case ObjectiveType.Rubble:
                break;
            case ObjectiveType.Village:
                break;
            case ObjectiveType.Town:
                break;
            case ObjectiveType.TradingStation:
                break;
            case ObjectiveType.Outpost:
                break;
            default:
                break;
        }
    }

    public void DeletObjective()
    {
        // TODO
    }

    private void Init()
    {

    }

    private void Start()
    {
        Init();
    }

    private void Update()
    {

    }
}
