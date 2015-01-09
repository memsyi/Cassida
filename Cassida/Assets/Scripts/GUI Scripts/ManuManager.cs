using UnityEngine;
using System.Collections;

public class ManuManager : MonoBehaviour 
{
    public MenuController CurrentMenu;
    public ProfileManager profiles;

    public void Start()
    {
        ShowMenu(CurrentMenu);
        //profiles.ShowProfiles();
    }

    public void ShowMenu(MenuController menu)
    {
        if (CurrentMenu != null) CurrentMenu.IsOpen = false;

        CurrentMenu = menu;
        CurrentMenu.IsOpen = true;
    }

    public void CheckProfile(MenuController menu)
    {
        if (profiles.AccountCreated)
        {
            if (CurrentMenu != null) CurrentMenu.IsOpen = false;

            CurrentMenu = menu;
            CurrentMenu.IsOpen = true;
        }
        else
        {

        }
    }
	
}
