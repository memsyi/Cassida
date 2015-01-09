using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ProfileManager : MonoBehaviour 
{
    public Text newProfile, errorMassage, profileStyle;
    public Transform profilesPosition;
    private int profilesCount;
    
    
    private bool _accountCreated = false;

    public bool AccountCreated
    {
        get
        {
            return _accountCreated;
        }
        set
        {
            _accountCreated = value;
        }
    }

    void Start()
    {
        //ClearAllProfiles();
        ShowAllProfiles();
    }

    public void CreateProfile()
    {
        if (newProfile.text.Length >= 3 && !AccountCreated)
        {
            profilesCount = PlayerPrefs.GetInt("profilecount");

            print("current profiles: " + profilesCount); /////////////////////

            Text profile = Instantiate(profileStyle, profilesPosition.position, profilesPosition.rotation) as Text;
            profile.transform.SetParent(profilesPosition);
            profile.transform.localScale = profilesPosition.localScale;
            profile.text = newProfile.text;
            profile.GetComponent<ProfileInput>().SetUpProfile(profilesCount, profile.text);

            PlayerPrefs.SetString("playername" + profilesCount, newProfile.text);
            profilesCount++;
            PlayerPrefs.SetInt("profilecount", profilesCount);

            errorMassage.text = "";
            AccountCreated = true;

            print("created a profile"); /////////////////////
        }
        else if(newProfile.text.Length < 3)
        {
            errorMassage.text = "Der Name muss mindestens 3 Zeichen lang sein";
        }
    }

    public void ShowAllProfiles()
    {
        profilesCount = PlayerPrefs.GetInt("profilecount");
        for (int i = 0; i < profilesCount; i++)
        {
            Text profile = Instantiate(profileStyle, profilesPosition.position, profilesPosition.rotation) as Text;
            profile.transform.SetParent(profilesPosition);
            profile.transform.localScale = profilesPosition.localScale;
            profile.text = PlayerPrefs.GetString("playername"+i);
        }
    }

    public void ClearAllProfiles()
    {
        for (int i = 0; i < 10; i++)
        {
            PlayerPrefs.SetString("playername" + i, ""); 
        }
        PlayerPrefs.SetInt("profilecount", 0);
    }

    public void StartNewProfile()
    {
        errorMassage.text = "";
        AccountCreated = false;
        newProfile.text = "";
    }

    public void SelectedProfile(int id)
    {
        print("id: " + id);
    }

}
