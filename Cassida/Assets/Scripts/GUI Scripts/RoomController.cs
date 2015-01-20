using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RoomController : MonoBehaviour 
{
    public MultiplayerRoom OwnRoom { get; private set; }

    private Canvas Canvas { get; set; }
    public Text RoomName { get; private set; }
    public Image SelectionImage;

    public void SetOwnProfile(MultiplayerRoom room)
    {
        OwnRoom = room;
        RoomName.text = room.RoomName;
    }

    public void SelectThisRoom()
    {
        NetworkLobbyManager.Get().SelectRoom(this);

        ActivateSelection();
    }

    private void ActivateSelection()
    {
        gameObject.transform.FindChild("Text").gameObject.SetActive(true);
    }

    public void DeactivateSelection()
    {
        gameObject.transform.FindChild("Text").gameObject.SetActive(false);
    }

    public void DeleteRoomObject()
    {
        Destroy(this.gameObject);
    }

    public void JoinRoom()
    {
        NetworkLobbyManager.Get().JoinRoom(this);
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
