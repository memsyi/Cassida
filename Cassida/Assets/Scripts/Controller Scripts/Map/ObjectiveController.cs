using UnityEngine;
using System.Collections;

public class ObjectiveController : MonoBehaviour
{
    #region Object and Instantiation
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
            InstantiateObjective();
        }
    }

    private Transform ObjectiveObject { get; set; }

    private void InstantiateObjective()
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

    public void InstantiateObjectiveObject(Transform model)
    {
        // Instantiate objective
        ObjectiveObject = Instantiate(model, transform.position, model.localRotation) as Transform;

        ObjectiveObject.name = "Objective: " + Type;
        ObjectiveObject.SetParent(transform);
    }

    public void DeletObjective()
    {
        // TODO
    } 
    #endregion

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
