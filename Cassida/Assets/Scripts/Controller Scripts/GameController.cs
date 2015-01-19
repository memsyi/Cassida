using UnityEngine;
using System.Collections.Generic;

public struct PlayerProperties
{
    public const string Number = "Number";
    public const string Color = "Color";
}

[RequireComponent(typeof(PhotonView))]
public class GameController : Photon.MonoBehaviour, IJSON
{
    public void StartNewGame()
    {
        if (PhotonNetwork.isMasterClient)
        {
            WorldManager.Get().InitializeWorld();
            PlayerManager.Get().AddPlayerInformation(PhotonNetwork.player, "master", Color.red);
            MapManager.Get().AddBasesToMap();
            CameraController.Get().enabled = true;
        }
    }

    private void InitiatePlayer(PhotonPlayer photonPlayer, string name, Color color)
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        PlayerManager.Get().SetAllExistingPlayerInformationAtPlayer(photonPlayer);
        PlayerManager.Get().AddPlayerInformation(photonPlayer, name, color);
    }

    private void InitiateAllPlayersInRoom()
    {

    }

    private void OnJoinedRoom()
    {
        //StartNewGame();
    }

    #region Player connect
    private void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        InitiatePlayer(newPlayer, "new Player", Color.blue);
    }
    #endregion

    #region Player disconnect
    private void OnPhotonPlayerDisconnected(PhotonPlayer photonPlayer)
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        if (PlayerManager.Get().CurrentPlayer.PhotonPlayer == photonPlayer)
        {
            PlayerManager.Get().EndTurn();
        }

        var player = PlayerManager.Get().GetPlayer(photonPlayer);

        photonView.RPC(RPCs.ClearAndDestroyAllOfDisconnectedPlayer, PhotonTargets.All, player.ID);
    }

    [RPC]
    private void ClearAndDestroyAllOfDisconnectedPlayer(int playerID, PhotonMessageInfo info)
    {
        if (!info.sender.isMasterClient)
        {
            return;
        }

        FleetManager.Get().DestroyAllFleetsOfPlayer(playerID);
    }
    #endregion

    private void OnLeftRoom()
    {
        Application.LoadLevel(Application.loadedLevelName);
    }

    private void OnGUI()
    {
        if (PhotonNetwork.player == PlayerManager.Get().CurrentPlayer.PhotonPlayer)
        {
            if (GUI.Button(new Rect(100, 0, 100, 20), "EndTurn"))
            {
                PlayerManager.Get().EndTurn();
            }
        }

        if (GUI.Button(new Rect(300, 0, 100, 20), "Save"))
        {
            PlayerPrefs.SetString("Game", ToJSON().print());
            print(FleetManager.Get().ToJSON().print());
            print(TileManager.Get().ToJSON().print());
        }
        if (GUI.Button(new Rect(400, 0, 100, 20), "Load"))
        {
            //PlayerPrefs.DeleteAll();
            FleetManager.Get().DestroyAllFleetsOfPlayer(0);

            FromJSON(JSONParser.parse(PlayerPrefs.GetString("Game")));
        }
        if (GUI.Button(new Rect(500, 0, 100, 20), "Clear savepoint"))
        {
            PlayerPrefs.DeleteAll();
        }
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

    private static GameController _instance = null;
    public static GameController Get()
    {
        if (_instance == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag(Tags.Manager);
            _instance = obj.AddComponent<GameController>();
        }

        return _instance;
    }

    public JSONObject ToJSON()
    {
        var jsonObject = JSONObject.obj;
        jsonObject[JSONs.Players] = PlayerManager.Get().ToJSON();
        jsonObject[JSONs.Map] = TileManager.Get().ToJSON();
        jsonObject[JSONs.Army] = FleetManager.Get().ToJSON();
        return jsonObject;
    }

    public void FromJSON(JSONObject jsonObject)
    {
        PlayerManager.Get().FromJSON(jsonObject[JSONs.Players]);
        TileManager.Get().FromJSON(jsonObject[JSONs.Map]);
        FleetManager.Get().FromJSON(jsonObject[JSONs.Army]);
    }
}
