using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RoomController : MonoBehaviour 
{
    public RoomFoo OwnRoom { get; private set; }

    private Canvas Canvas { get; set; }
    public Text RoomName { get; private set; }
    public Image SelectionImage;

    public void SetOwnProfile(RoomFoo room)
    {
        OwnRoom = room;
        RoomName.text = room.Name;
    }

    public void SelectThisRoom()
    {
        LobbyManager.Get().SelectRoom(this);

        ActivateSelection();
    }

    private void ActivateSelection()
    {
        SelectionImage.gameObject.SetActive(true);
    }

    public void DeactivateSelection()
    {
        SelectionImage.gameObject.SetActive(false);
    }

    public void DeleteRoomObject()
    {
        Destroy(this.gameObject);
    }

    public void JoinRoom()
    {
        LobbyManager.Get().JoinRoom(this);
        MenuManager.Get().ChangeMenu(GameObject.FindGameObjectWithTag(Tags.MultiplayerRoomMenu).GetComponent<MenuController>());
    }

    private void Init()
    {
        Canvas = FindObjectOfType<Canvas>();
        SelectionImage.gameObject.SetActive(false);
    }

    private void Start()
    {
        Init();
    }

    private void Update()
    {

    }
}
