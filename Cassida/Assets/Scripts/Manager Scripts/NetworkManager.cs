using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public void ConnectToServer()
    {
        PhotonNetwork.offlineMode = false;
        PhotonNetwork.ConnectUsingSettings("Cassida v0.0.2 c");
    }

    public void DisconnectFromServer()
    {
        PhotonNetwork.Disconnect();
    }

    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public void JoinRoom(string name)
    {
        PhotonNetwork.JoinOrCreateRoom(name, null, null);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    private void OnDisconnectedFromPhoton()
    {
        PhotonNetwork.offlineMode = true;
    }

    private void OnPhotonRandomJoinFailed()
    {
        PhotonNetwork.CreateRoom(null);
    }

    private void OnJoinedLobby()
    {
        JoinRandomRoom();
    }

    private void OnGUI()
    {
        if (!PhotonNetwork.offlineMode)
        {
            GUILayout.Label(PhotonNetwork.connectionState.ToString());
            GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
        }

        if (PhotonNetwork.connectionStateDetailed == PeerState.JoinedLobby)
        {
            //if (GUILayout.Button("Join Room"))
            //{
            //    JoinRandomRoom();
            //}
        }

        if (PhotonNetwork.connectionStateDetailed == PeerState.Joined)
        {
            GUILayout.Label(PhotonNetwork.playerList.Length.ToString() + " Player inside Room");

            if (GUILayout.Button("Leave Room"))
            {
                LeaveRoom();
            }
        }

        if (!PhotonNetwork.offlineMode && GUILayout.Button("Go offline"))
        {
            DisconnectFromServer();
        }
        else if (PhotonNetwork.offlineMode && GUILayout.Button("Go online"))
        {
            ConnectToServer();
        }
    }

    private void Init()
    {
        if (!PhotonNetwork.connected)
        {
            PhotonNetwork.offlineMode = true;
        }
    }

    private void Start()
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

        Init();
    }

    private void Update()
    {

    }

    private static NetworkManager _instance = null;
    public static NetworkManager Get()
    {
        if (_instance == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag(Tags.Manager);
            _instance = obj.AddComponent<NetworkManager>();
        }

        return _instance;
    }
}
