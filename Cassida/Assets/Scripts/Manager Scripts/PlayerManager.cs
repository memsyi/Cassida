using UnityEngine;
using System.Collections.Generic;

public class Player : IJSON
{
    public int ID { get; private set; }
    private PhotonPlayer photonPlayer = null;
    public PhotonPlayer PhotonPlayer { get { return photonPlayer; } set { if (photonPlayer == null) photonPlayer = value; } }
    public string Name { get; private set; }
    public Color Color { get; private set; }

    public Player(int id, PhotonPlayer photonPlayer, string name, Color color)
        : this()
    {
        ID = id;
        PhotonPlayer = photonPlayer;
        Name = name;
        Color = color;
    }

    public Player()
    {
        
    }

    public JSONObject ToJSON()
    {
        var jsonObject = JSONObject.obj;

        jsonObject[JSONs.ID] = new JSONObject(ID);
        jsonObject[JSONs.Name] = new JSONObject(Name);
        var colorObject = JSONObject.obj;
        colorObject["r"] = new JSONObject(Color.r);
        colorObject["g"] = new JSONObject(Color.g);
        colorObject["b"] = new JSONObject(Color.b);
        jsonObject[JSONs.Color] = colorObject;

        return jsonObject;
    }

    public void FromJSON(JSONObject jsonObject)
    {
        var id = (int)jsonObject[JSONs.ID];
        var name = (string)jsonObject[JSONs.Name];
        var color = new Color((float)jsonObject[JSONs.Color]["r"], (float)jsonObject[JSONs.Color]["g"], (float)jsonObject[JSONs.Color]["b"]);
    }
}

[RequireComponent(typeof(PhotonView))]
public class PlayerManager : Photon.MonoBehaviour
{
    #region Variables
    // Player
    public Player Player { get; private set; }
    public Player CurrentPlayer { get; private set; }

    // Lists
    public List<Player> PlayerList { get; private set; }
    #endregion

    public void AddPlayerInformation(PhotonPlayer player, string name, Color color)
    {
        if (!PhotonNetwork.isMasterClient || PlayerList.Exists(p => p.PhotonPlayer == player))
        {
            return;
        }

        photonView.RPC(RPCs.SetPlayerInformation, PhotonTargets.All, PlayerList.Count, player, name, new Vector3(color.r, color.g, color.b));
    }

    public void SetAllExistingPlayerInformationAtPlayer(PhotonPlayer photonPlayer)
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        foreach (var player in PlayerList)
        {
            photonView.RPC(RPCs.SetPlayerInformation, photonPlayer, player.ID, player.PhotonPlayer, player.Name, new Vector3(player.Color.r, player.Color.g, player.Color.b));
        }
    }

    [RPC]
    private void SetPlayerInformation(int id, PhotonPlayer player, string name, Vector3 color, PhotonMessageInfo info)
    {
        if (!info.sender.isMasterClient || PlayerList.Exists(p => p.PhotonPlayer == player))
        {
            return;
        }

        var newPlayer = new Player(id, player, name, new Color(color.x, color.y, color.z));
        PlayerList.Add(newPlayer);

        if (PhotonNetwork.isMasterClient)
        {
            if (CurrentPlayer.PhotonPlayer == null)
            {
                CurrentPlayer = newPlayer;
            }

            SetCurrentPlayer(CurrentPlayer.PhotonPlayer);

            if (!player.isMasterClient)
            {
                // Instantiate map at player
                MapGenerator.Get().InstatiateAllExistingTilesAtPlayer(player);

                // Instatiate fleets at player
                FleetManager.Get().InstantiateAllExistingFleetsAtPlayer(player);

                // Instantiate bases at player
                BaseManager.Get().InstantiateAllExistingBasesAtPlayer(player);
            }
        }

        if (PhotonNetwork.player != player || Player.PhotonPlayer != null)
        {
            return;
        }

        Player = newPlayer;
    }

    private void SetCurrentPlayer(PhotonPlayer player)
    {
        if (!PhotonNetwork.isMasterClient || !PlayerList.Exists(p => p.PhotonPlayer == player))
        {
            return;
        }

        CurrentPlayer = PlayerList.Find(p => p.PhotonPlayer == player);
        photonView.RPC(RPCs.SetCurrentPlayer, PhotonTargets.All, CurrentPlayer.PhotonPlayer);
    }

    public Player GetPlayer(int id)
    {
        return PlayerList.Find(p => p.ID == id);
    }

    #region End turn
    public void EndTurn()
    {
        if (!PhotonNetwork.isMasterClient)
        {
            if (PhotonNetwork.player != CurrentPlayer.PhotonPlayer)
            {
                return;
            }
        }

        photonView.RPC(RPCs.AskForEndTurn, PhotonTargets.MasterClient);
    }

    [RPC]
    private void AskForEndTurn(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.isMasterClient)
        {
            if (info.sender != CurrentPlayer.PhotonPlayer)
            {
                return;
            }
        }

        photonView.RPC(RPCs.SetCurrentPlayer, PhotonTargets.All, FindNewCurrentPlayer());
    }

    [RPC]
    private void SetCurrentPlayer(PhotonPlayer photonPlayer, PhotonMessageInfo info)
    {
        if (!info.sender.isMasterClient || !PlayerList.Exists(p => p.PhotonPlayer == photonPlayer))
        {
            return;
        }

        if (PhotonNetwork.player == CurrentPlayer.PhotonPlayer)
        {
            FleetManager.Get().ResetMovementOfAllFleets();
            TileManager.Get().ResetAllTiles();
            InputManager.Get().ResetMovementArea();
            InputManager.Get().RemoveMouseEvents();
        }

        CurrentPlayer = PlayerList.Find(p => p.PhotonPlayer == photonPlayer);

        if (PhotonNetwork.player != photonPlayer)
        {
            return;
        }

        InputManager.Get().AddMouseEvents();
    }

    private PhotonPlayer FindNewCurrentPlayer()
    {
        PhotonPlayer nextPlayer = null;
        var nextPlayerID = CurrentPlayer.ID;

        while (nextPlayer == null)
        {
            nextPlayerID++;

            if (nextPlayerID >= PlayerList.Count)
            {
                nextPlayerID = 0;
            }

            var playerList = new List<PhotonPlayer>(PhotonNetwork.playerList);

            var possiblePlayer = PlayerList.Find(p => playerList.Exists(pp => pp == p.PhotonPlayer) && (int)p.ID == nextPlayerID).PhotonPlayer;

            if (possiblePlayer != null)
            {
                nextPlayer = possiblePlayer;
            }
        }

        return nextPlayer;
    }
    #endregion

    private void Init()
    {
        PlayerList = new List<Player>();
        Player = new Player();
        CurrentPlayer = new Player();
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

    private static PlayerManager _instance = null;
    public static PlayerManager Get()
    {
        if (_instance == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag(Tags.Manager);
            _instance = obj.AddComponent<PlayerManager>();
        }

        return _instance;
    }
}
