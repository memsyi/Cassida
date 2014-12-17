using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour
{
    WorldManager WorldManager { get; set; }
    FleetManager FleetManager { get; set; }

    private void StartGame()
    {
        WorldManager.InitializeWorld();

        FleetManager.InstantiateStartFleets();
    }

    private void OnJoinedRoom()
    {
        StartGame();
    }

    private void Init()
    {
        WorldManager = GameObject.FindGameObjectWithTag(Tags.Manager).GetComponent<WorldManager>();
        FleetManager = GameObject.FindGameObjectWithTag(Tags.Manager).GetComponent<FleetManager>();

        if (!WorldManager || !FleetManager)
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
