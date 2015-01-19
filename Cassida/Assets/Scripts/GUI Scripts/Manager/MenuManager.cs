using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    private MenuController CurrentMenu {get; set;}

    public void ShowMenu(MenuController menu)
    {
        ChangeMenu(menu);
    }

    public void ChangeMenu(MenuController menu)
    {

        if (CurrentMenu == null)
        {
            return;
        }

        CurrentMenu.IsOpen = false;
        CurrentMenu = menu;
        CurrentMenu.IsOpen = true;
    }

    private void Init()
    {
        CurrentMenu = GameObject.FindGameObjectWithTag(Tags.MainMenu).GetComponent<MenuController>();
        ChangeMenu(CurrentMenu);
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

    private static MenuManager _instance = null;
    public static MenuManager Get()
    {
        if (_instance == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag(Tags.MenuManager);
            _instance = obj.AddComponent<MenuManager>();
        }

        return _instance;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
