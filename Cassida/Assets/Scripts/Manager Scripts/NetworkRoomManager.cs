﻿using UnityEngine;
using System.Collections.Generic;

public class MultiplayerRoom
{
    public string RoomName { get; private set; }
    public int CurrentPlayerCount { get; set; }
    public int MaxPlayer { get; set; }
    public string InfoName { get { return CurrentPlayerCount + "/" + MaxPlayer; } }

    public RoomController RoomController { get; private set; }

    public MultiplayerRoom(string roomName, int playerCount, int maxPlayer, RoomController roomController)
    {
        RoomName = roomName;
        CurrentPlayerCount = playerCount;
        MaxPlayer = maxPlayer;

        RoomController = roomController;
    }
}

public class NetworkRoomManager : Photon.MonoBehaviour
{
    private int SavePointIndex { get; set; }

    public void StartGame()
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        photonView.RPC(RPCs.StartGame, PhotonTargets.All, SavePointIndex);
    }

    public void ChangePlayerCount(int playerCount)
    {
        if (!PhotonNetwork.isMasterClient || PlayerManager.Get().PlayerList.Count > playerCount)
        {
            return;
        }

        PhotonNetwork.room.maxPlayers = playerCount;
    }

    public void ChangeMapSize(int bottomEdgeLength)
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        GameManager.Get().Game.MapSize = (EdgeLength)bottomEdgeLength;
    }

    public void ChangeMapForm(int mapForm)
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        GameManager.Get().Game.MapForm = (MapForms)mapForm;
    }

    public void TryToChangeColor(int possibleColor)
    {
        photonView.RPC(RPCs.AskMasterToChangeColor, PhotonTargets.MasterClient, possibleColor);
        ChangeColor(possibleColor);
    }

    [RPC]
    private void StartGame(int savePointID, PhotonMessageInfo info)
    {
        if (!info.sender.isMasterClient)
        {
            return;
        }

        if (savePointID >= 0)
        {
            GameManager.Get().LoadGame(savePointID); // TODO load correct
            return;
        }

        GameManager.Get().StartNewGame();
    }
    
    private void ChangeColor(int possibleColor)
    {
        PlayerManager.Get().Player.Color = PlayerColor.GetColor((PossiblePlayerColors)possibleColor);
    }

    private void OnJoinedRoom()
    {
        if (!PhotonNetwork.offlineMode)
        {
            MenuManager.Get().ShowMenu(GameObject.FindGameObjectWithTag(Tags.MultiplayerRoomMenu).GetComponent<MenuController>());
        }

        if (PhotonNetwork.isMasterClient)
        {
            if (SavePointIndex >= 0)
            {
                // TODO change to correct save point
                PlayerManager.Get().FromJSON(JSONParser.parse(PlayerPrefs.GetString("Game"))[JSONs.Players]);
            }

            AddPlayerInformation(PhotonNetwork.player, ProfileManager.Get().CurrentProfile.PlayerName);
        }
        else
        {
            photonView.RPC(RPCs.AskMasterToJoinGame, PhotonTargets.MasterClient, ProfileManager.Get().CurrentProfile.PlayerName);
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
    private void AskMasterToJoinGame(string name, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.isMasterClient || PlayerManager.Get().GetPlayer(info.sender) != null)
        {
            return;
        }

        PlayerManager.Get().SetAllExistingPlayerInformationAtPlayer(info.sender);
        AddPlayerInformation(info.sender, name);

        print("add player");
    }

    [RPC]
    private void AskMasterToChangeColor(int possibleColor, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.isMasterClient || PlayerManager.Get().GetPlayer(info.sender) == null)
        {
            return;
        }

        var color = PlayerColor.GetColor((PossiblePlayerColors)possibleColor);

        if (PlayerManager.Get().PlayerList.Exists(p => p.Color == color))
        {
            PlayerManager.Get().ChangePlayerColor(info.sender, PlayerManager.Get().GetPlayer(info.sender).Color);
            return;
        }

        PlayerManager.Get().ChangePlayerColor(info.sender, color);
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
        SavePointIndex = -1;
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