﻿using UnityEngine;
using System.Collections.Generic;

public struct PlayerProperties
{
    public const string Number = "Number";
    public const string Color = "Color";
}

[RequireComponent(typeof(PhotonView))]
public class GameController : Photon.MonoBehaviour, IJSON
{
    private void StartGame()
    {
        if (PhotonNetwork.isMasterClient)
        {
            WorldManager.Get().InitializeWorld();
            PlayerManager.Get().AddPlayerInformation(PhotonNetwork.player, "master", Color.red);
            MapManager.Get().AddBasesToMap();
        }
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

        FleetManager.Get().DestroyAllFleetsOfPlayers(playerID);
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

        if (GUI.Button(new Rect(300, 0, 100, 20), "Save"))
        {
            PlayerPrefs.SetString("Game", ToJSON().print());
            print(FleetManager.Get().ToJSON().print());
            print(TileManager.Get().ToJSON().print());
        }
        if (GUI.Button(new Rect(400, 0, 100, 20), "Load"))
        {
            //PlayerPrefs.DeleteAll();
            FleetManager.Get().DestroyAllFleetsOfPlayers(0);

            FromJSON(JSONParser.parse(PlayerPrefs.GetString("Game")));
        }
        if (GUI.Button(new Rect(500, 0, 100, 20), "Print"))
        {
            print(PlayerPrefs.GetString("Fleets"));
            print(PlayerPrefs.GetString("Tiles"));
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

    public JSONObject ToJSON()
    {
        var jsonObject = JSONObject.obj;
        jsonObject[JSONs.Army] = FleetManager.Get().ToJSON();
        jsonObject[JSONs.Map] = TileManager.Get().ToJSON();
        return jsonObject;
    }

    public void FromJSON(JSONObject jsonObject)
    {
        FleetManager.Get().FromJSON(jsonObject[JSONs.Army]);
        TileManager.Get().FromJSON(jsonObject[JSONs.Map]);
    }
}
