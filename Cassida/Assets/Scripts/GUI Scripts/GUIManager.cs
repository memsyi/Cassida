using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GUIManager : MonoBehaviour 
{
    public Text playerInputText, profileCheckText;
    public GameObject gotProfile, newProfile;
	// Use this for initialization
	void Start () 
    {
        if (PlayerPrefs.GetString("playername") != "")
        {
            profileCheckText.text = "Welcome Lord " + PlayerPrefs.GetString("playername");
        }
        else
        {
            NoProfile();
        }
	}
	
	// Update is called once per frame
	void Update () 
    {

	}

    private void InputPlayerName()
    {
        PlayerPrefs.SetString("playername", playerInputText.text);
        profileCheckText.text = "Welcome Lord " + playerInputText.text;
    }

    public void CheckPlayerName()
    {
        if (playerInputText.text.Length >= 3)
        {
            InputPlayerName();
            StartMenu();
        }
    }
    public void NoProfile()
    {
        gotProfile.SetActive(false);
        newProfile.SetActive(true);
    }

    private void StartMenu()
    {
        gotProfile.SetActive(true);
        newProfile.SetActive(false);
    }

    public void DeleteProfile()
    {
        PlayerPrefs.SetString("playername", "");
        playerInputText.text = "";
        NoProfile();
    }
}
