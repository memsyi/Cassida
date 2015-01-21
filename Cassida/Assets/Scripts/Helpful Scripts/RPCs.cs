using UnityEngine;
using System.Collections;

public struct RPCs
{
    public const string AskForNewFleet = "AskForNewFleet";
    public const string AskForNewUnit = "AskForNewUnit";

    public const string AddNewFleet = "AddNewFleet";
    public const string AddNewUnit = "AddNewUnit";

    public const string SetPlayerInformation = "SetPlayerInformation";
    public const string SetPlayerColor = "SetPlayerColor";

    public const string StartGame = "StartGame";
    public const string AskMasterToJoinGame = "AskMasterToJoinGame";
    public const string AskMasterToChangeColor = "AskMasterToChangeColor"; 

    public const string AskForEndTurn = "AskForEndTurn";
    public const string SetCurrentPlayer = "SetCurrentPlayer";

    public const string AddTile = "AddTile";

    public const string AddNewBase = "AddNewBase";

    public const string AskForMoveFleet = "AskForMoveFleet";
    public const string MoveFleet = "MoveFleet";
    public const string AskForRotateFleet = "AskForRotateFleet";
    public const string RotateFleet = "RotateFleet";
    public const string AskForAttack = "AskForAttack";
    public const string AttackEnemy = "AttackEnemy";

    public const string ClearAndDestroyAllOfDisconnectedPlayer = "ClearAndDestroyAllOfDisconnectedPlayer";
}
