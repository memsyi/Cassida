@@ -0,0 +1,152 @@
﻿using UnityEngine;
using System.Collections.Generic;

public class MultiplayerRoom
{
    public string RoomName { get; private set; }
    public string MasterName { get; private set; }
    public int CurrentPlayerCount { get; private set; }
    public int MaxPlayer { get; set; }
    public string InfoName { get { return MasterName + " " + CurrentPlayerCount + "/" + MaxPlayer; } }

    public RoomController RoomController { get; private set; }

    public MultiplayerRoom(string roomName, string masterName, int playerCount, int maxPlayer, RoomController roomController)
    {
        RoomName = roomName;
        MasterName = masterName;
        CurrentPlayerCount = playerCount;
        MaxPlayer = maxPlayer;

        RoomController = roomController;
    }
}

public class NetworkRoomManager : MonoBehaviour
{

    private bool LoadedGame { get; set; }

    public void StartGame()
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        GameObject.FindGameObjectWithTag(Tags.Menu).SetActive(false);
        CameraController.Get().enabled = true;

        if (LoadedGame)
        {
            GameManager.Get().LoadGame(0); // TODO load correct
            return;
        }

        GameManager.Get().StartNewGame();
    }

    private void OnJoinedRoom()
    {
        print("join room");
        MenuManager.Get().ShowMenu(GameObject.FindGameObjectWithTag(Tags.MultiplayerRoomMenu).GetComponent<MenuController>());

        if (LoadedGame)
        {
            // TODO change to correct save point
            PlayerManager.Get().FromJSON(JSONParser.parse(PlayerPrefs.GetString("Game"))[JSONs.Players]);
        }

        if (PhotonNetwork.isMasterClient)
        {
            AddPlayerInformation(PhotonNetwork.player, ProfileManager.Get().CurrentProfile.PlayerName);
        }
        else
        {
            AskMasterToJoinGame(PhotonNetwork.player, ProfileManager.Get().CurrentProfile.PlayerName);
        }
    }

    //#region Player connect
    //private void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    //{
    //    if (!PhotonNetwork.isMasterClient)
    //    {
    //        return;
    //    }

        
    //}

    [RPC]
    private void AskMasterToJoinGame(PhotonPlayer photonPlayer, string name)
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        PlayerManager.Get().SetAllExistingPlayerInformationAtPlayer(photonPlayer);
        AddPlayerInformation(photonPlayer, name);
    }

    private void AddPlayerInformation(PhotonPlayer photonPlayer, string name)
    {
        var color = FindRandomFreeColor(); 
        PlayerManager.Get().AddPlayerInformation(photonPlayer, name, color);
    }

    private Color FindRandomFreeColor()
    {
        var color = Color.black;

        while (color == Color.black || PlayerManager.Get().PlayerList.Exists(p => p.Color == color))
        {
            var randomColorIndex = Random.Range(0, System.Enum.GetValues(typeof(PossiblePlayerColors)).Length);
            color = PlayerColor.GetColor((PossiblePlayerColors)randomColorIndex);
        }

        return color;
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

    private static NetworkRoomManager _instance = null;
    public static NetworkRoomManager Get()
    {
        if (_instance == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag(Tags.Manager);
            _instance = obj.AddComponent<NetworkRoomManager>();
        }

        return _instance;
    }
}