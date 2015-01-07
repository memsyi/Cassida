using UnityEngine;

public class GameController : MonoBehaviour
{
    //WorldManager WorldManager { get; set; }
    TileManager TileManager { get; set; }
    FleetManager FleetManager { get; set; }

    private void StartGame()
    {
        TileManager.InitializeWorld();

        FleetManager.InstantiateStartFleets();
    }

    private void OnJoinedRoom()
    {
        StartGame();
    }

    private void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        FleetManager.DestroyAllFleetsOfPlayer(player);
    }

    private void OnLeftRoom()
    {
        Application.LoadLevel(Application.loadedLevelName);
    }

    public void EndTurn()
    {
        FleetManager.ResetMovementOfAllFleets();
        TileManager.ResetAllTiles();
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
        //WorldManager = GameObject.FindGameObjectWithTag(Tags.Manager).GetComponent<WorldManager>();
        TileManager = GameObject.FindGameObjectWithTag(Tags.Manager).GetComponent<TileManager>();
        FleetManager = GameObject.FindGameObjectWithTag(Tags.Manager).GetComponent<FleetManager>();

        if (!TileManager || !FleetManager)
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
