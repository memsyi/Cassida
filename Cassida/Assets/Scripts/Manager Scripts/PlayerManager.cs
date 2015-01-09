using UnityEngine;
using System.Collections.Generic;

public struct Player
{
    public int ID { get; private set; }
    public PhotonPlayer PhotonPlayer { get; private set; }
    public string Name { get; private set; }
    public Color Color { get; private set; }

    public Player(int id, PhotonPlayer photonPlayer, string name, Color color)
    {
        ID = id;
        PhotonPlayer = photonPlayer;
        Name = name;
        Color = color;
    }
}

public class PlayerManager : MonoBehaviour
{
    public Player Player { get; private set; }

    // Lists
    public List<Player> PlayerList { get; private set; }

    private void Init()
    {

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
