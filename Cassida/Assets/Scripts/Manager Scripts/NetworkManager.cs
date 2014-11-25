using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour
{

    void Connect()
    {
        PhotonNetwork.offlineMode = false;
        PhotonNetwork.ConnectUsingSettings("Cassida v0.0.1");
        //PhotonNetwork.ConnectToBestCloudServer("Cassida v0.0.1");
    }

    void OnDisconnectedFromPhoton()
    {
        PhotonNetwork.offlineMode = true;
    }

    void OnPhotonRandomJoinFailed()
    {
        PhotonNetwork.CreateRoom(null);
    }

    void OnGUI()
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
                PhotonNetwork.JoinRandomRoom();
            }
        }

        if (PhotonNetwork.connectionStateDetailed == PeerState.Joined)
        {
            GUILayout.Label(PhotonNetwork.playerList.Length.ToString() + " Player inside Room");

            if (GUILayout.Button("Leave Room"))
            {
                PhotonNetwork.LeaveRoom();
            }
        }

        if (!PhotonNetwork.offlineMode && GUILayout.Button("Go offline"))
        {
            PhotonNetwork.Disconnect();
        }
        else if (PhotonNetwork.offlineMode && GUILayout.Button("Go online"))
        {
            Connect();
        }
    }

    void Init()
    {
        PhotonNetwork.offlineMode = true;
    }

    void Start()
    {
        Init();
    }

    void Update()
    {

    }
}
