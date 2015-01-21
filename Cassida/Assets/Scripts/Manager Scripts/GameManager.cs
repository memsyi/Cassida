﻿using UnityEngine;
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
    public EdgeLength MapSize { get; set; }
    public MapForms MapForm { get; set; }

    public Game(EdgeLength mapSize, MapForms mapForm)
    {
        MapSize = mapSize;
        MapForm = mapForm;
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
    public Game Game { get; set; }
    #endregion

    public void StartNewGame()
    {
        SetUpGameView();
        CheckOfflineMode();

        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }

        WorldManager.Get().InitializeWorld(Game.MapSize, Game.MapForm);

        SetUpGameToAllPlayer();
    }

    public void LoadGame(int SavePointID)
    {
        SetUpGameView();
        CheckOfflineMode();

        if (!PhotonNetwork.isMasterClient)
        {
            return;
        }
        // TODO läd noch nicht den richtigen savepoint
        FromJSON(JSONParser.parse(PlayerPrefs.GetString("Game")));

        SetUpGameToAllPlayer();
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

    public void CheckOfflineMode()
    {
        if (!PhotonNetwork.connected)
        {
            PhotonNetwork.offlineMode = true;
            PhotonNetwork.CreateRoom("singlePlayerRoom");
        }
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

    public void SetUpGameToPlayer(PhotonPlayer photonPlayer)
    {
        // Instantiate map at player
        MapGenerator.Get().InstatiateAllExistingTilesAtPlayer(photonPlayer);

        // Instatiate fleets at player
        FleetManager.Get().InstantiateAllExistingFleetsAtPlayer(photonPlayer);

        // Instantiate bases at player
        BaseManager.Get().InstantiateAllExistingBasesAtPlayer(photonPlayer);
    }

    public void SetUpGameToAllPlayer()
    {
        foreach (var player in PlayerManager.Get().PlayerList)
        {
            SetUpGameToPlayer(player.PhotonPlayer);
        }
    }

    private void SetUpGameView()
    {
        var menu = GameObject.FindGameObjectWithTag(Tags.Menu);
        if (menu != null)
        {
            menu.SetActive(false);
        }
        CameraController.Get().enabled = true;
    }

    private void OnLeftRoom()
    {
        Application.LoadLevel(Application.loadedLevelName);
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(100, 0, 100, 20), "Start game"))
        {
            StartNewGame();
        }
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
        Game = new Game(EdgeLength.Seven, MapForms.Hexagon);
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