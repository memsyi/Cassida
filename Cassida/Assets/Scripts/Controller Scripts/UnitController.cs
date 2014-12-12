using UnityEngine;

public enum UnitType
{
    Meele,
    Range
}

public class Unit
{
    public int Position { get; private set; }
    public Transform UnitObject { get; private set; }
    public UnitType Type { get; private set; }
    public int Strength { get; set; }

    public UnitController Controller { get; private set; }

    public Unit(int position, Transform unitObject, UnitType type, int strength)
    {
        Position = position;
        UnitObject = unitObject;
        Type = type;
        Strength = strength;
    }

    private void SetCorrectType()
    {

    }
}

public class UnitController : MonoBehaviour
{

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
