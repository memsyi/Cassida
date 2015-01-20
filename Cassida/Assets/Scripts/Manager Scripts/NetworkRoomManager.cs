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

    private bool LoadedGame { get; set; }

    public void StartGame(int bottomEdgeLength, int mapForm)
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

        GameManager.Get().StartNewGame((EdgeLength)bottomEdgeLength, (MapForms)mapForm);
    }

    public void TryToChangeColor(int possibleColor)
    {
        photonView.RPC(RPCs.AskMasterToChangeColor, PhotonTargets.MasterClient, possibleColor);
        ChangeColor(possibleColor);
    }

    public void ChangePlayerCount(int playerCount)
    {
        if(!PhotonNetwork.isMasterClient || PlayerManager.Get().PlayerList.Count > playerCount)
        {
            return;
        }

        PhotonNetwork.room.maxPlayers = playerCount;
    }

    private void ChangeColor(int possibleColor)
    {
        PlayerManager.Get().Player.Color = PlayerColor.GetColor((PossiblePlayerColors)possibleColor);
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
            print(ProfileManager.Get().CurrentProfile);
            AddPlayerInformation(PhotonNetwork.player, ProfileManager.Get().CurrentProfile.PlayerName);
        }
        else
        {
            photonView.RPC(RPCs.AskMasterToJoinGame, PhotonTargets.MasterClient, PhotonNetwork.player, ProfileManager.Get().CurrentProfile.PlayerName);
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