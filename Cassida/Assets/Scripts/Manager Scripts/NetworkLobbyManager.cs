﻿using UnityEngine;
using System.Collections.Generic;

public class NetworkLobbyManager : MonoBehaviour
{
    [SerializeField]
    private GameObject roomStyle;
    private GameObject RoomStyle
    {
        get { return roomStyle; }
    }

    [SerializeField]
    private float refreshTime = 0.0f;
    private float RefreshTime
    {
        get { return refreshTime; }
        set { refreshTime = value; }
    }

    [SerializeField]
    private Transform roomsPosition;
    private Transform RoomsPosition
    {
        get { return roomsPosition; }
    }

    //private bool DeveloperModus { get; set; }

    private MultiplayerRoom CurrentRoom { get; set; }

    // Lists
    private List<RoomInfo> RoomInfoList { get; set; }
    private List<MultiplayerRoom> RoomList { get; set; }

    public void ConnectToServer()
    {
        //if (!DeveloperModus)
        //{
            PhotonNetwork.ConnectUsingSettings("Cassida v0.0.3");
        //}
        //else
        //{
        //    PhotonNetwork.ConnectUsingSettings("Cassida v0.0.3c");
        //}
    }

    public void DisconnectFromServer()
    {
        PhotonNetwork.Disconnect();
    }

    public void JoinRoom(RoomController roomController)
    {
        if (PhotonNetwork.connectionStateDetailed != PeerState.JoinedLobby)
        {
            return;
        }

        var room = RoomList.Find(r => r.RoomController == roomController);
        if (room == null)
        {
            return;
        }
        PhotonNetwork.JoinRoom(room.RoomName);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    private void OnDisconnectedFromPhoton()
    {
        var mainMenu = GameObject.FindGameObjectWithTag(Tags.MainMenu);
        if (mainMenu != null)
        {
            MenuManager.Get().ShowMenu(mainMenu.GetComponent<MenuController>());
        }
    }

    private void OnJoinedLobby()
    {
        print("asdf");
        var lobbyMenu = GameObject.FindGameObjectWithTag(Tags.MultiplayerLobbyMenu);
        if (lobbyMenu != null)
        {
            MenuManager.Get().ShowMenu(lobbyMenu.GetComponent<MenuController>());
        }
        //if (!DeveloperModus)
        //{
        //    //ShowAllNewExitingRooms();
        //    print("joined Lobby");
        //}
        //else
        //{
        //    JoinRandomRoom();
        //}
    }

    //private void OnGUI()
    //{
        //if (!PhotonNetwork.connected && GUILayout.Button("Go online"))
        //{
        //    //DeveloperModus = true;
        //    ConnectToServer();
        //}
        //else if (PhotonNetwork.connected && GUILayout.Button("Go offline"))
        //{
        //    //DeveloperModus = false;
        //    DisconnectFromServer();
        //}

        //if (!DeveloperModus)
        //{
        //    return;
        //}

        //if (PhotonNetwork.connected)
        //{
        //    GUILayout.Label(PhotonNetwork.connectionState.ToString());
        //    GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
        //    GUILayout.Label(PhotonNetwork.playerList.Length.ToString() + " Player inside Room");
        //}

        //if (PhotonNetwork.connectionStateDetailed == PeerState.Joined)
        //{

        //    if (GUILayout.Button("Leave Room"))
        //    {
        //        LeaveRoom();
        //    }
        //}
    //}

    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    private void OnPhotonRandomJoinFailed()
    {
        RoomOptions defaultRoomOptions = new RoomOptions();
        defaultRoomOptions.maxPlayers = 2;

        PhotonNetwork.CreateRoom(ProfileManager.Get().CurrentProfile.PlayerName, defaultRoomOptions, null);
    }

    public void CreateRoom(string roomName)
    {
        if (PhotonNetwork.connectionStateDetailed != PeerState.JoinedLobby)
        {
            print("cant creat room");
            return;
        }

        if (roomName.Length < 3)
        {
            //errorMassage.text = "Der Name muss mindestens 3 Zeichen lang sein";
            return;
        }

        RoomOptions defaultRoomOptions = new RoomOptions();
        defaultRoomOptions.maxPlayers = 2;

        PhotonNetwork.CreateRoom(roomName, defaultRoomOptions, null);
    }

    private void InstantiateRoomObject(string roomName)
    {
        GameObject roomObject = Instantiate(roomStyle, roomsPosition.position, roomsPosition.rotation) as GameObject;
        roomObject.transform.SetParent(roomsPosition);
        roomObject.transform.localScale = roomsPosition.localScale;

        var roomController = roomObject.GetComponent<RoomController>();

        var room = new MultiplayerRoom(roomName, 1, 2, roomController);
        RoomList.Add(room);
    }

    private void RefreshRooms()
    {
        if (PhotonNetwork.connectionStateDetailed != PeerState.JoinedLobby)
        {
            return;
        }
        RoomInfoList = new List<RoomInfo>(PhotonNetwork.GetRoomList());
        DeleteNotExitingRooms();

        if (RoomInfoList == null || RoomInfoList.Count <= 0)
        {
            return;
        }

        ShowAllNewExitingRooms();
        CorrectDataOfExistingRooms();
    }

    public void SelectRoom(RoomController roomController)
    {
        if (CurrentRoom != null)
        {
            CurrentRoom.RoomController.DeactivateSelection();
        }
        CurrentRoom = RoomList.Find(r => r.RoomController == roomController);
    }

    private void ShowRoom(string roomName)
    {
        InstantiateRoomObject(roomName);
    }

    private void CorrectDataOfExistingRooms()
    {
        for(int i = RoomList.Count -1; i >0; i--)
        {
            var room = RoomList[i];
            var roomInfo = RoomInfoList.Find(r => r.name == room.RoomName);
            room.CurrentPlayerCount = roomInfo.playerCount;
            room.MaxPlayer = roomInfo.maxPlayers;
        }
    }

    private void ShowAllNewExitingRooms()
    {
        for (int i = RoomInfoList.Count - 1; i >= 0; i--)
        {
            if (RoomList.Exists(r => r.RoomName == RoomInfoList[i].name))
            {
                continue;
            }

            ShowRoom(RoomInfoList[i].name);
        }
    }

    private void DeleteNotExitingRooms()
    {
        for(int i = RoomList.Count - 1; i >= 0; i--)
        {
            var roomInfo = RoomInfoList.Find(r => r.name == RoomList[i].RoomName);
            if (roomInfo != null)
            {
                continue;
            }

            RoomList[i].RoomController.DeleteRoomObject();
            RoomList.Remove(RoomList[i]);
        }
    }

    private void Init()
    {
        RoomList = new List<MultiplayerRoom>();

        if (PhotonNetwork.connected)
        {
            return;
        }
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
        RefreshRooms();
    }

    private static NetworkLobbyManager _instance = null;
    public static NetworkLobbyManager Get()
    {
        if (_instance == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag(Tags.Manager);
            _instance = obj.AddComponent<NetworkLobbyManager>();
        }

        return _instance;
    }
}