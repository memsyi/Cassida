using UnityEngine;
using System.Collections;

public class FleetController : MonoBehaviour
{
    private Vector3 _movementTarget;

    public Vector3 MovementTarget
    {
        get { return _movementTarget; }
        set
        {
            _movementTarget = value;
            Move = true;
        }
    }

    private Quaternion _turnTarget;

    private Quaternion TurnTarget
    {
        get { return _turnTarget; }
        set 
        {
            _turnTarget.eulerAngles += value.eulerAngles;
            Turn = true;
        }
    }

    public void EraseTurnSteps(int turnDirection)
    {
        TurnTarget = Quaternion.AngleAxis(turnDirection * 60f, Vector3.up);
    }

    private bool Move { get; set; }
    private bool Turn { get; set; }

    private void MoveToTarget()
    {
        if (!Move)
        {
            return;
        }

        transform.position = Vector3.Lerp(transform.position, MovementTarget, Time.deltaTime * 3);

        if (Vector3.Distance(transform.position, MovementTarget) < 0.01f)
        {
            transform.position = MovementTarget;
            Move = false;
        }
    }

    private void TurnToTarget()
    {
        if (!Turn)
        {
            return;
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, TurnTarget, Time.deltaTime * 3);

        if (Quaternion.Angle(transform.rotation, TurnTarget) < 0.01f)
        {
            transform.rotation = TurnTarget;
            Turn = false;
        }
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
        MoveToTarget();
        TurnToTarget();
    }
}
