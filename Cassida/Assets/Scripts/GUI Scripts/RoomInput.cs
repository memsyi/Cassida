using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RoomInput : MonoBehaviour 
{
    //public string roomName;
    public Text roomName;
    public string masterName;
    private int playerCount, maxPlayer;
    private LobbyManager manager;
    private Canvas canvas;
    public Image selection;

	// Use this for initialization
	void Start () 
    {
        manager = FindObjectOfType<LobbyManager>();
        canvas = FindObjectOfType<Canvas>();
        selection.gameObject.SetActive(false);
	}

    public void SetUpRoom(string masterName, int playerCount, int maxPlayer)
    {
        this.masterName = masterName;
        this.playerCount = playerCount;
        this.maxPlayer = maxPlayer;
        roomName.text = masterName + " "+playerCount+"/"+maxPlayer;
        print("room created");
    }

    public void Selected()
    {
        manager.SelectedRoom(this);
    }

    public void ActivateSelection()
    {
        selection.gameObject.SetActive(true);
    }

    public void DeactivateSelection()
    {
        selection.gameObject.SetActive(false);
    }

    public void DeleteRoom()
    {
        Destroy(this.gameObject);
    }

    public void JoinRoom()
    {
        manager.JoinRoom(this);
        canvas.GetComponent<MenuManager>().JoinedRoom();
    }
}
