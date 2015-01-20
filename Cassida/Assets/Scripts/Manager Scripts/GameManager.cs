﻿﻿using UnityEngine;
using System.Collections.Generic;

public struct PlayerProperties
{
    public const string Number = "Number";
    public const string Color = "Color";
}

public class Game : IJSON
{
    public int PlayerCount { get { return PlayerManager.Get().PlayerList.Count; } }
    public float SaveTime { get { return (float)System.DateTime.Now.ToOADate(); } }

    public Game(int bar)
    {
        
    }

    public Game()
    {
        
    }

    public JSONObject ToJSON()
    {
        var jsonObject = JSONObject.obj;
        jsonObject[JSONs.SaveTime] = new JSONObject(SaveTime);
        return jsonObject;
    }

    public void FromJSON(JSONObject jsonObject)
    {
        
    }
}


[RequireComponent(typeof(PhotonView))]
public class GameManager : Photon.MonoBehaviour, IJSON
{
    #region Variables
    private Game Game { get; set; }
    #endregion

    public void StartNewGame()
    {
        if (PhotonNetwork.isMasterClient)
        {
            WorldManager.Get().InitializeWorld();
            MapManager.Get().AddBasesToMap();
        }
    }

    public void LoadGame(int SavePointID)
    {
        // TODO läd noch nicht den richtigen savepoint
        FromJSON(JSONParser.parse(PlayerPrefs.GetString("Game")));
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

    #region Player connect
    private void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        //InitiatePlayer(newPlayer, "new Player", Color.blue);
    }
    #endregion

    #region Player disconnect
    private void OnPhotonPlayerDisconnected(PhotonPlayer photonPlayer)
    {
        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        PlayerPrefs.SetString(Playerprefs.AutoSavePoint, ToJSON().print());

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
            LoadGame(0);
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

    private static GameManager _instance = null;
    public static GameManager Get()
    {
        if (_instance == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag(Tags.Manager);
            _instance = obj.AddComponent<GameManager>();
        }

        return _instance;
    }

    public JSONObject ToJSON()
    {
        var jsonObject = JSONObject.obj;
        jsonObject[JSONs.Game] = Game.ToJSON();
        jsonObject[JSONs.Players] = PlayerManager.Get().ToJSON();
        jsonObject[JSONs.Map] = TileManager.Get().ToJSON();
        jsonObject[JSONs.Army] = FleetManager.Get().ToJSON();
        return jsonObject;
    }

    public void FromJSON(JSONObject jsonObject)
    {
        Game = new Game();
        Game.FromJSON(jsonObject[JSONs.Game]);
        //PlayerManager.Get().FromJSON(jsonObject[JSONs.Players]); // schon beim laden im room
        TileManager.Get().FromJSON(jsonObject[JSONs.Map]);
        FleetManager.Get().FromJSON(jsonObject[JSONs.Army]);
    }
}