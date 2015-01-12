using UnityEngine;
using System.Collections.Generic;

public struct PlayerProperties
{
    public const string Number = "Number";
    public const string Color = "Color";
}

[RequireComponent(typeof(PhotonView))]
public class GameController : Photon.MonoBehaviour
{
    private void StartGame()
    {
        if (PhotonNetwork.isMasterClient)
        {
            WorldManager.Get().InitializeWorld();
            PlayerManager.Get().AddPlayerInformation(PhotonNetwork.player, "master", Color.red);
        }

        //print(FleetManager.ToJSON().print());
    }

    private void OnJoinedRoom()
    {
        StartGame();
    }

    #region Player connect
    private void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        PlayerManager.Get().SetAllExistingPlayerInformationAtPlayer(newPlayer);
        PlayerManager.Get().AddPlayerInformation(newPlayer, "new player", Color.blue);
    }

    //private JSONObject ToJSON()
    //{
    //    JSONObject x=base.Save();
    //    x["size"]=JSONObject(size);

    //    if(x)

    //    x["FleetMgr"] = FleetManager.Save();

    //    return x;

    //    List<string> strs;
    //    var l=JSONObject.arr;l.Add(strs 

    //    x["Player"]["Levels"][2]["XP"] = 2
    //    (int)x["Player/Levels/2/XP"]
    //}

    //private void FromJSON(JSONObject x)
    //{
    //    size = (float)x["size"];
    //}
    #endregion

    #region Player disconnect
    private void OnPhotonPlayerDisconnected(PhotonPlayer photonPlayer)
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        if (PlayerManager.Get().CurrentPlayer.PhotonPlayer == photonPlayer)
        {
            PlayerManager.Get().EndTurn();
        }

        var player = PlayerManager.Get().PlayerList.Find(p => p.PhotonPlayer == photonPlayer);

        photonView.RPC(RPCs.ClearAndDestroyAllOfDisconnectedPlayer, PhotonTargets.All, player.ID);
    }

    [RPC]
    private void ClearAndDestroyAllOfDisconnectedPlayer(int playerID, PhotonMessageInfo info)
    {
        if (!info.sender.isMasterClient)
        {
            return;
        }

        FleetManager.Get().DestroyAllFleetsOfDisconnectedPlayers(playerID);
    }
    #endregion

    private void OnLeftRoom()
    {
        Application.LoadLevel(Application.loadedLevelName);
    }

    private void OnGUI()
    {
        if (PhotonNetwork.player == PlayerManager.Get().CurrentPlayer.PhotonPlayer)
        {
            if (GUI.Button(new Rect(100, 0, 100, 20), "EndTurn"))
            {
                PlayerManager.Get().EndTurn();
            }
        }
    }

    private void Init()
    {

    }

    private void Start()
    {
        Init();
    }

    private void Awake()
    {
        //Check for Singleton
        if (_instance == null)
        {
            _instance = this;
        }
        else if (_instance != this)
        {
            Debug.LogError("Second instance!");
            return;
        }
    }

    private void Update()
    {

    }

    private static GameController _instance = null;
    public static GameController Get()
    {
        if (_instance == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag(Tags.Manager);
            _instance = obj.AddComponent<GameController>();
        }

        return _instance;
    }
}
