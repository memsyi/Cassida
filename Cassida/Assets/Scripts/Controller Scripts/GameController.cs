using UnityEngine;
using System.Collections.Generic;

public struct PlayerProperties
{
    public const string Number = "Number";
    public const string Color = "Color";
}

[RequireComponent(typeof(PhotonView))]
public class GameController : Photon.MonoBehaviour
{
    // Scripts
    //WorldManager WorldManager { get; set; }
    private TileManager TileManager { get; set; }
    private FleetManager FleetManager { get; set; }
    private InputManager InputManager { get; set; }

    private List<PhotonPlayer> PlayerList { get; set; }
    private PhotonPlayer TurnOwner { get; set; }
    //private int PlayerNumber { get; set; }

    private void StartGame()
    {
        TileManager.InitializeWorld();

        FleetManager.InstantiateStartFleets();

        if (PhotonNetwork.isMasterClient)
        {
            SetPlayerPropertiesAndAddToList(PhotonNetwork.player, Color.red);
            TurnOwner = PhotonNetwork.player;
            InputManager.AddMouseEvents();
        }
    }

    private void OnJoinedRoom()
    {
        StartGame();
    }

    #region Player connect
    private void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        // Set player number
        SetPlayerPropertiesAndAddToList(newPlayer, Color.blue);

        // Add all players to player list
        foreach (var player in PlayerList)
        {
            photonView.RPC("NetworkAddPlayerToPlayerList", PhotonTargets.All, player);
        }

        // Instantiate map at player
        var mapGenerator = GameObject.FindGameObjectWithTag(Tags.Map).GetComponent<MapGenerator>();
        mapGenerator.InstatiateAllExistingTilesAtPlayer(newPlayer);

        // Instatiate fleets at player
        FleetManager.InstatiateAllExistingFleetsAtPlayer(newPlayer);
    }

    private void SetPlayerPropertiesAndAddToList(PhotonPlayer player, Color color)
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        // Add player number
        var playerProperties = new ExitGames.Client.Photon.Hashtable();
        playerProperties.Add(PlayerProperties.Number, PhotonNetwork.playerList.Length);
        playerProperties.Add(PlayerProperties.Color, new Vector3(color.r, color.g, color.b));

        player.SetCustomProperties(playerProperties);

        // Add to list
        PlayerList.Add(player);
    }

    [RPC]
    private void NetworkAddPlayerToPlayerList(PhotonPlayer player)
    {
        if (PlayerList.Exists(p => p == player))
        {
            return;
        }

        PlayerList.Add(player);
    }
    #endregion

    #region Player disconnect
    private void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        var playerList = new List<PhotonPlayer>(PhotonNetwork.playerList);

        if (!playerList.Exists(p => p == player))
        {
            NetworkEndTurn(player);
        }

        photonView.RPC("NetworkClearAndDestroyAllOfDisconnectedPlayers", PhotonTargets.All, player);
    }

    [RPC]
    private void NetworkClearAndDestroyAllOfDisconnectedPlayers(PhotonPlayer player)
    {
        FleetManager.DestroyAllFleetsOfDisconnectedPlayers(player);
    }
    #endregion

    private void OnLeftRoom()
    {
        Application.LoadLevel(Application.loadedLevelName);
    }

    #region End turn
    public void EndOwnTurn()
    {
        FleetManager.ResetMovementOfAllFleets();
        TileManager.ResetAllTiles();
        InputManager.ResetMovementArea();
        InputManager.RemoveMouseEvents();

        photonView.RPC("NetworkEndTurn", PhotonTargets.MasterClient, PhotonNetwork.player);
    }

    [RPC]
    private void NetworkEndTurn(PhotonPlayer lastPlayer)
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        photonView.RPC("NetworkSetNewTurnOwner", PhotonTargets.All, CheckTurnOwner(lastPlayer));
    }

    [RPC]
    private void NetworkSetNewTurnOwner(PhotonPlayer nextPlayer)
    {
        TurnOwner = nextPlayer;

        if (PhotonNetwork.player != nextPlayer)
        {
            return;
        }

        InputManager.AddMouseEvents();
    }

    private PhotonPlayer CheckTurnOwner(PhotonPlayer lastPlayer)
    {
        PhotonPlayer nextPlayer = null;
        var nextPlayerNumber = (int)lastPlayer.customProperties[PlayerProperties.Number] + 1;

        while (nextPlayer == null)
        {
            if (nextPlayerNumber > PlayerList.Count)
            {
                nextPlayerNumber = 1;
            }

            var playerList = new List<PhotonPlayer>(PhotonNetwork.playerList);

            var possiblePlayer = PlayerList.Find(p => playerList.Exists(pp => pp == p) && (int)p.customProperties[PlayerProperties.Number] == nextPlayerNumber );

            if (possiblePlayer != null)
            {
                nextPlayer = possiblePlayer;
            }

            nextPlayerNumber++;
        }

        return nextPlayer;
    }
    #endregion

    private void OnGUI()
    {
        if (PhotonNetwork.player == TurnOwner)
        {
            if (GUI.Button(new Rect(100, 0, 100, 20), "EndTurn"))
            {
                EndOwnTurn();
            }
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
        PlayerList = new List<PhotonPlayer>();
    }

    private void Awake()
    {
        Init();
    }

    private void Update()
    {

    }
}
