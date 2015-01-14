using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    public MenuController CurrentMenu, RoomMenu;
    public ProfileManager profiles;
    public LobbyManager room;

    public void Start()
    {
        ShowMenu(CurrentMenu);
    }

    public void ShowMenu(MenuController menu)
    {
        ChangeMenu(menu);
    }

    public void CheckProfile(MenuController menu)
    {
        if (profiles.AccountCreated)
        {
            ChangeMenu(menu);
        }
    }

    public void CheckCreatedRoom(MenuController menu)
    {
        if (room.RoomCreated)
        {
            ChangeMenu(menu);
        }
    }

    public void JoinedRoom()
    {
        ChangeMenu(RoomMenu);
    }

    private void ChangeMenu(MenuController menu)
    {
        if (CurrentMenu != null) CurrentMenu.IsOpen = false;

        CurrentMenu = menu;
        CurrentMenu.IsOpen = true;
    }
}
