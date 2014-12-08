using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public void ConnectToServer()
    {
        PhotonNetwork.offlineMode = false;
        PhotonNetwork.ConnectUsingSettings("Cassida v0.0.1");
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

    private void OnGUI()
    {
        if (!PhotonNetwork.offlineMode)
        {
            GUILayout.Label(PhotonNetwork.connectionState.ToString());
            GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
        }

        if (PhotonNetwork.connectionStateDetailed == PeerState.JoinedLobby)
        {
            if (GUILayout.Button("Join Room"))
            {
                JoinRandomRoom();
            }
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
        PhotonNetwork.offlineMode = true;
    }

    private void Start()
    {
        Init();
    }

    private void Update()
    {

    }
}
