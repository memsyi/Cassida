﻿using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    private MenuController CurrentMenu { get; set; }

    public void ShowMenu(MenuController menu)
    {
        ChangeMenu(menu);
    }

    private void ChangeMenu(MenuController menu)
    {
        print(menu);
        if (menu == null || CurrentMenu == menu)
        {
            return;
        }

        if (CurrentMenu != null)
        {
            CurrentMenu.IsOpen = false;
        }

        
        CurrentMenu = menu;
        CurrentMenu.IsOpen = true;
    }

    public void CloseMenu()
    {
        CurrentMenu.IsOpen = false;
        CurrentMenu = null;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void Init()
    {
        var mainMenu = GameObject.FindGameObjectWithTag(Tags.MainMenu);
        if (mainMenu != null)
        {
            ChangeMenu(mainMenu.GetComponent<MenuController>());
        }
        CameraController.Get().enabled = false;
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
            GameObject obj = GameObject.FindGameObjectWithTag(Tags.Manager);
            _instance = obj.AddComponent<MenuManager>();
        }

        return _instance;
    }
}