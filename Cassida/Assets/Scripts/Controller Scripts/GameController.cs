using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(PhotonView))]
public class GameController : Photon.MonoBehaviour
{
    // Scripts
    //WorldManager WorldManager { get; set; }
    private TileManager TileManager { get; set; }
    private FleetManager FleetManager { get; set; }
    private InputManager InputManager { get; set; }

    private void StartGame()
    {
        TileManager.InitializeWorld();

        FleetManager.InstantiateStartFleets();
    }

    private void OnJoinedRoom()
    {
        StartGame();
    }

    #region Player connect
    private void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        FleetManager.InstatiateAllExistingFleetsAtPlayer(player);

        var mapGenerator = GameObject.FindGameObjectWithTag(Tags.Map).GetComponent<MapGenerator>();
        mapGenerator.InstatiateAllExistingTilesAtPlayer(player);
    }
    #endregion

    #region Player disconnect
    private void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        print("disconnected " + player);

        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        photonView.RPC("NetworkClearAndDestroyAllOfDisconnectedPlayers", PhotonTargets.All);
    }

    void OnMasterClientSwitched(PhotonPlayer newMasterClient)
    {
        print("new master " + newMasterClient.ToString());
        //if (newMasterClient != PhotonNetwork.player)
        //{
        //    return;
        //}

        //photonView.RPC("NetworkClearAndDestroyAllOfDisconnectedPlayers", PhotonTargets.All);
    }

    [RPC]
    private void NetworkClearAndDestroyAllOfDisconnectedPlayers()
    {
        print("delet all objects");
        FleetManager.DestroyAllFleetsOfDisconnectedPlayers();
    }
    #endregion

    private void OnLeftRoom()
    {
        Application.LoadLevel(Application.loadedLevelName);
    }

    public void EndTurn()
    {
        FleetManager.ResetMovementOfAllFleets();
        TileManager.ResetAllTiles();
        InputManager.ResetMovementArea();
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(100, 0, 100, 20), "EndTurn"))
        {
            EndTurn();
        }
    }

    private void Init()
    {
        var managerObject = GameObject.FindGameObjectWithTag(Tags.Manager);
        TileManager = managerObject.GetComponent<TileManager>();
        FleetManager = managerObject.GetComponent<FleetManager>();
        InputManager = managerObject.GetComponent<InputManager>();

        if (!TileManager || !FleetManager || !InputManager)
        {
            Debug.LogError("MissedComponents!");
        }
    }

    private void Start()
    {

    }

    private void Awake()
    {
        Init();
    }

    private void Update()
    {

    }
}
