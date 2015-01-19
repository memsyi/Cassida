using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RoomFoo
{
    public string RoomName { get; private set; }
    public string MasterName { get; private set; }
    public int CurrentPlayerCount { get; private set; }
    public int MaxPlayer { get; set; }
    public string InfoName { get { return MasterName + " " + CurrentPlayerCount + "/" + MaxPlayer; } }

    public RoomController RoomController { get; private set; }

    public RoomFoo(string roomName, string masterName, int playerCount, int maxPlayer, RoomController roomController)
    {
        RoomName = roomName;
        MasterName = masterName;
        CurrentPlayerCount = playerCount;
        MaxPlayer = maxPlayer;

        RoomController = roomController;
    }
}

public class LobbyManager : MonoBehaviour
{
    [SerializeField]
    private GameObject roomStyle;
    private GameObject RoomStyle
    {
        get { return roomStyle; }
    }

    [SerializeField]
    private float refreshTime = 2.0f;
    public float RefreshTime
    {
        get { return refreshTime; }
        set { refreshTime = value; }
    }

    public Transform roomsPosition;
    private float lastUpdateTime;

    private bool developerModus = false;

    private RoomFoo CurrentRoom { get; set; }
    //private RoomInputController CurrentSelection { get; set; }

    //private bool _roomCreated = false;

    // Lists
    private List<RoomInfo> RoomInfoList { get; set; }
    private List<RoomFoo> RoomList { get; set; }

    public void ConnectToServer()
    {
        PhotonNetwork.offlineMode = false;
        if (!developerModus) PhotonNetwork.ConnectUsingSettings("Cassida v0.0.3");
        else PhotonNetwork.ConnectUsingSettings("Cassida v0.0.3c");
    }

    public void DisconnectFromServer()
    {
        PhotonNetwork.Disconnect();
    }

    public void JoinRoom(RoomController roomController)
    {
        SelectRoom(roomController);
        PhotonNetwork.JoinRoom(roomController.OwnRoom.MasterName);
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
        if (!developerModus)
        {
            ShowAllNewExitingRooms();
            print("joined Lobby");
        }
        else JoinRandomRoom();
    }

    private void OnGUI()
    {
        if (PhotonNetwork.offlineMode && GUILayout.Button("Go online"))
        {
            developerModus = true;
            ConnectToServer();
        }
        else if (!PhotonNetwork.offlineMode && GUILayout.Button("Go offline"))
        {
            developerModus = false;
            DisconnectFromServer();
        }

        if (!developerModus)
        {
            return;
        }

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
    }

    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    private void OnPhotonRandomJoinFailed()
    {
        PhotonNetwork.CreateRoom(null);
    }

    public void CreateRoom(string roomName)
    {
        InstantiateRoomObject(roomName);

        RoomOptions defaultSettings = new RoomOptions();
        defaultSettings.isOpen = true;
        defaultSettings.isVisible = true;
        defaultSettings.maxPlayers = CurrentRoom.MaxPlayer;

        PhotonNetwork.CreateRoom(roomName, defaultSettings, null);
        //RoomCreated = true;
        //RoomInfoList = new List<RoomInfo>(PhotonNetwork.GetRoomList());
    }

    private void InstantiateRoomObject(string roomName)
    {
        // initiate the profile at the gui
        GameObject room = Instantiate(roomStyle, roomsPosition.position, roomsPosition.rotation) as GameObject;
        room.transform.SetParent(roomsPosition);
        room.transform.localScale = roomsPosition.localScale;

        var roomController = room.GetComponent<RoomController>();

        CurrentRoom = new RoomFoo(roomName, ProfileManager.Get().CurrentProfile.PlayerName, 0, 2, roomController);
        RoomList.Add(CurrentRoom);
    }


    // ist not used anymore
    //public void GameCreated()
    //{
    //    if (roomName.text.Length >= 3 && !RoomCreated)
    //    {
    //        print("room :" + roomName.text + " created");
    //        PhotonNetwork.JoinOrCreateRoom(roomName.text, null, null);
    //        //GameObject room = InstantiateRoom();
    //        //room.GetComponent<RoomInput>().SetUpRoom(roomName.text);
    //        errorMassage.text = "";
    //        RoomCreated = true;
    //    }
    //    else if (roomName.text.Length < 3)
    //    {
    //        errorMassage.text = "Der Name muss mindestens 3 Zeichen lang sein";
    //    }
    //}

    private void RefreshRooms()
    {
        RoomInfoList = new List<RoomInfo>(PhotonNetwork.GetRoomList());
        if (RoomInfoList == null || RoomInfoList.Count <= 0)
        {
            return;
        }

        DeleteNotExitingRooms();
        ShowAllNewExitingRooms();
    }

    public void SelectRoom(RoomController roomController)
    {
        if (CurrentRoom != null)
        {
            CurrentRoom.RoomController.DeactivateSelection();
        }
        CurrentRoom = RoomList.Find(r => r.RoomController == roomController);
        print("selected" + roomController.RoomName.text);
    }

    private void ShowRoom(RoomFoo room)
    {
        InstantiateRoomObject(room.RoomName);
        //.OwnRoom = new RoomFoo(roomController.OwnRoom.Name, PlayerManager.Get().Player.Name, roomController.OwnRoom.CurrentPlayerCount, roomController.OwnRoom.MaxPlayer);
    }

    private void ShowAllNewExitingRooms()
    {
        foreach (var roomInfo in RoomInfoList)
        {
            if(RoomList.Exists(r => r.RoomName == roomInfo.name))
            {
                return;
            }

            InstantiateRoomObject(roomInfo.name);

            //var roomController = roomObject.GetComponent<RoomInputController>();
            //.OwnRoom = new RoomFoo(roomController.OwnRoom.Name, PlayerManager.Get().Player.Name, roomController.OwnRoom.CurrentPlayerCount, roomController.OwnRoom.MaxPlayer);
            //roomList.Add(roomObject.GetComponent<RoomInput>());
            //CurrentSelection = roomObject.GetComponent<RoomInput>();
        }
        //if (PhotonNetwork.GetRoomList().Length > 0)
        //{
        //    for (int i = 0; i < PhotonNetwork.GetRoomList().Length; i++)
        //    {
        //        roomsOpen = PhotonNetwork.GetRoomList();
        //        //print("room " + rooms[i].name + " is availible");
        //        GameObject room = InstantiateRoom();
        //        room.GetComponent<RoomInput>().SetUpRoom(roomsOpen[i].name, roomsOpen[i].playerCount, roomsOpen[i].maxPlayers);
        //        roomList.Add(room.GetComponent<RoomInput>());
        //        CurrentSelection = room.GetComponent<RoomInput>();
        //    }
        //}

        //foreach (RoomInfo game in PhotonNetwork.GetRoomList())
        //{
        //    GameObject room = InstantiateRoom();
        //    room.GetComponent<RoomInput>().SetUpRoom(game.name, game.playerCount, game.maxPlayers);
        //    currentSelection = room.GetComponent<RoomInput>();
        //}
    }

    private void DeleteNotExitingRooms()
    {
        foreach (var roomInfo in RoomInfoList)
        {
            var room = RoomList.Find(r => r.RoomName == roomInfo.name);
            if (room != null)
            {
                return;
            }

            room.RoomController.DeleteRoomObject();
            RoomList.Remove(room);
        }
    }

    private void Init()
    {
        RoomList = new List<RoomFoo>();
        PhotonNetwork.offlineMode = true;
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
        if (PhotonNetwork.connected && (lastUpdateTime + refreshTime) < Time.time)
        {
            RefreshRooms();
            //ShowAllRooms();
            lastUpdateTime = Time.time;
        }
    }

    private static LobbyManager _instance = null;
    public static LobbyManager Get()
    {
        if (_instance == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag(Tags.MenuManager);
            _instance = obj.AddComponent<LobbyManager>();
        }

        return _instance;
    }

}
