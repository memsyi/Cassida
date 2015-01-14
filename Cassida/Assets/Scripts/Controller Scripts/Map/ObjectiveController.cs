using UnityEngine;

public class ObjectiveController : MonoBehaviour
{
    #region Object and Instantiation
    private Transform ObjectiveObject { get; set; }

    public void InstantiateObjective(ObjectiveType type)
    {
        var mapGenerator = MapGenerator.Get();

        switch (type)
        {
            case ObjectiveType.Base:
                return;
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

    private void InstantiateObjectiveObject(Transform model, ObjectiveType type)
    {
        // Instantiate objective
        ObjectiveObject = Instantiate(model, transform.position, Quaternion.identity) as Transform;

        ObjectiveObject.name = "Objective: " + type;
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

    }

    private void Awake()
    {
        Init();
    }

    private void Update()
    {

    }
}
