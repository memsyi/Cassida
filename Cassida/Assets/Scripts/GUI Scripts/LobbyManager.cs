using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour
{
    public GameObject roomStyle, gameCreation;
    public Transform roomsPosition;
    public Text roomName, errorMassage;
    public float refreshTime = 2.0f;
    private float lastUpdateTime;
    private List<RoomInput> rooms;
    private RoomInfo[] roomsOpen;
    private bool defModus = false;

    private RoomInput currentSelection = null;

    private bool _roomCreated = false;

    public bool RoomCreated
    {
        get
        {
            return _roomCreated;
        }
        set
        {
            _roomCreated = value;
        }
    }

    // Use this for initialization
    void Start()
    {
        rooms = new List<RoomInput>();
        PhotonNetwork.offlineMode = true;
        gameCreation.SetActive(false);
        currentSelection = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.connected && rooms != null && (lastUpdateTime + refreshTime) < Time.time)
        {
            roomsOpen = PhotonNetwork.GetRoomList();
            print("refresh");
            RefreshRooms();
            //ShowAllRooms();
            lastUpdateTime = Time.time;
        }
    }

    public void ConnectToServer()
    {
        PhotonNetwork.offlineMode = false;
        if(!defModus) PhotonNetwork.ConnectUsingSettings("Cassida v0.0.2");
        else PhotonNetwork.ConnectUsingSettings("Cassida v0.0.2c");
    }

    public void DisconnectFromServer()
    {
        PhotonNetwork.Disconnect();
    }

    public void JoinRoom(RoomInput room)
    {
        SelectedRoom(room);
        print("try to join " + room.masterName);
        PhotonNetwork.JoinRoom(room.masterName);
    }

    public void StartNewRoom()
    {
        errorMassage.text = "";
        RoomCreated = false;
        roomName.text = "";
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    private void OnDisconnectedFromPhoton()
    {
        PhotonNetwork.offlineMode = true;
    }

    private void OnJoinedLobby()
    {
        if (!defModus)
        {
            ShowAllRooms();
            lastUpdateTime = Time.time;
            print("joined Lobby");
        }
        else JoinRandomRoom();
    }

    private void OnGUI()
    {
        if (!PhotonNetwork.offlineMode)
        {
            GUILayout.Label(PhotonNetwork.connectionState.ToString());
            GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
            GUILayout.Label(PhotonNetwork.playerList.Length.ToString() + " Player inside Room");
        }

        if (PhotonNetwork.connectionStateDetailed == PeerState.Joined)
        {

            if (GUILayout.Button("Leave Room"))
            {
                LeaveRoom();
            }
        }

        if (PhotonNetwork.offlineMode && GUILayout.Button("Go online"))
        {
            defModus = true;
            ConnectToServer();
        }
        else if (!PhotonNetwork.offlineMode && GUILayout.Button("Go offline"))
        {
            defModus = false;
            DisconnectFromServer();
        }
    }

    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    private void OnPhotonRandomJoinFailed()
    {
        PhotonNetwork.CreateRoom(null);
    }

    public void CreateRoom()
    {
        StartNewRoom();

        RoomOptions defaultSettings = new RoomOptions();
        defaultSettings.isOpen = true;
        defaultSettings.isVisible = true;
        defaultSettings.maxPlayers = 2;

        PhotonNetwork.CreateRoom(PlayerPrefs.GetString("currentprofile"), defaultSettings, null);
        RoomCreated = true;
        //gameCreation.SetActive(true);
        roomsOpen = PhotonNetwork.GetRoomList();
    }


    // ist not used anymore
    public void GameCreated()
    {
        if (roomName.text.Length >= 3 && !RoomCreated)
        {
            gameCreation.SetActive(false);
            print("room :" + roomName.text + " created");
            PhotonNetwork.JoinOrCreateRoom(roomName.text, null, null);
            //GameObject room = InstantiateRoom();
            //room.GetComponent<RoomInput>().SetUpRoom(roomName.text);
            errorMassage.text = "";
            RoomCreated = true;
        }
        else if (roomName.text.Length < 3)
        {
            errorMassage.text = "Der Name muss mindestens 3 Zeichen lang sein";
        }
    }

    void ShowAllRooms()
    {
        if (PhotonNetwork.GetRoomList().Length > 0)
        {
            for (int i = 0; i < PhotonNetwork.GetRoomList().Length; i++)
            {
                roomsOpen = PhotonNetwork.GetRoomList();
                //print("room " + rooms[i].name + " is availible");
                GameObject room = InstantiateRoom();
                room.GetComponent<RoomInput>().SetUpRoom(roomsOpen[i].name, roomsOpen[i].playerCount, roomsOpen[i].maxPlayers);
                rooms.Add(room.GetComponent<RoomInput>());
                currentSelection = room.GetComponent<RoomInput>();
            }
        }

        //foreach (RoomInfo game in PhotonNetwork.GetRoomList())
        //{
        //    GameObject room = InstantiateRoom();
        //    room.GetComponent<RoomInput>().SetUpRoom(game.name, game.playerCount, game.maxPlayers);
        //    currentSelection = room.GetComponent<RoomInput>();
        //}
    }

    void RefreshRooms()
    {
        if (PhotonNetwork.GetRoomList().Length > 0 || rooms.Count > 0) DeleteAllRooms();
        ShowAllRooms();
    }

    void DeleteAllRooms()
    {
        foreach (RoomInput room in rooms)
        {
            room.DeleteRoom();
        }
        rooms.Clear();
    }

    private GameObject InstantiateRoom()
    {
        // initiate the profile at the gui
        GameObject room = Instantiate(roomStyle, roomsPosition.position, roomsPosition.rotation) as GameObject;
        room.transform.SetParent(roomsPosition);
        room.transform.localScale = roomsPosition.localScale;
        return room;
    }

    public void SelectedRoom(RoomInput gameRoom)
    {
        if (currentSelection != null)
        {
            currentSelection.DeactivateSelection();
        }
        currentSelection = gameRoom;
        print("selected" + gameRoom.roomName.text);
        currentSelection.ActivateSelection();
    }
}
