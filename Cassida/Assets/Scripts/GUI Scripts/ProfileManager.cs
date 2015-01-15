using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ProfileManager : MonoBehaviour
{
    public Text newProfile, errorMassage, profileStyle;
    public Transform profilesPosition;
    public Button deleteProfile;
    private int profilesCount, profileMaxID;

    public const string PLAYERNAME = "playername", CURRENTPROFILE = "currentprofile", PROFILESCOUNT = "profilecount", MAXID = "maxID";

    private ProfileInput currentSelection = null;

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
        profileMaxID = PlayerPrefs.GetInt(MAXID);
        profilesCount = PlayerPrefs.GetInt(PROFILESCOUNT);
        deleteProfile.gameObject.SetActive(false);
        currentSelection = null;
        ShowAllProfiles();
    }

    public void CreateProfile()
    {
        if (newProfile.text.Length >= 3 && !AccountCreated)
        {
            print("current profiles: " + profilesCount); /////////////////////

            Text profile = InstantiateProfile();

            // get a new ID
            int id = NewID();
            if (id < 0)
            {
                print("we got empty accounts???");
            }
            else
            {
                print("id generated: " + id);
                profile.GetComponent<ProfileInput>().SetUpProfile(id, newProfile.text);

                // set up profile stats
                profile.text = profile.GetComponent<ProfileInput>().profileName;
                PlayerPrefs.SetString(PLAYERNAME + id, profile.GetComponent<ProfileInput>().profileName);
                profilesCount++;

                PlayerPrefs.SetInt(PROFILESCOUNT, profilesCount);

                errorMassage.text = "";
                AccountCreated = true;

                PlayerPrefs.SetString(CURRENTPROFILE, profile.text);
                print("created a profile"); /////////////////////
            }
        }
        else if (newProfile.text.Length < 3)
        {
            errorMassage.text = "Der Name muss mindestens 3 Zeichen lang sein";
        }
    }

    private Text InstantiateProfile()
    {
        // initiate the profile at the gui
        Text profile = Instantiate(profileStyle, profilesPosition.position, profilesPosition.rotation) as Text;
        profile.transform.SetParent(profilesPosition);
        profile.transform.localScale = profilesPosition.localScale;
        return profile;
    }

    private int NewID()
    {
        int newID = -1;
        int count = PlayerPrefs.GetInt(PROFILESCOUNT) + 1;
        for (int i = 0; i < count; i++)
        {
            if (PlayerPrefs.GetString(PLAYERNAME + i) == "")
            {
                if (i >= profileMaxID)
                {
                    PlayerPrefs.SetInt(MAXID, i);
                    profileMaxID = i;
                    print("maxID: " + profileMaxID);
                }
                return newID = i;
            }
        }
        return newID;
    }

    public void ShowAllProfiles()
    {
        if (PlayerPrefs.GetInt(PROFILESCOUNT) > 0)
        {
            int countTo = PlayerPrefs.GetInt(MAXID);
            print("countTo : " + countTo);
            for (int i = 0; i <= countTo; i++)
            {
                if (PlayerPrefs.GetString(PLAYERNAME + i) != "")
                {
                    // initiate the profile at the gui
                    Text profile = InstantiateProfile();

                    // set up profile stats
                    profileStyle.GetComponent<ProfileInput>().SetUpProfile(i, PlayerPrefs.GetString(PLAYERNAME + i));
                    profile.text = PlayerPrefs.GetString(PLAYERNAME + i);
                }
            }
        }
    }

    public void ClearAllProfiles()
    {
        for (int i = -100; i < 100; i++)
        {
            PlayerPrefs.SetString(PLAYERNAME + i, "");
        }
        PlayerPrefs.SetInt(PROFILESCOUNT, 0);
        PlayerPrefs.SetInt(MAXID, 0);
    }

    public void StartNewProfile()
    {
        errorMassage.text = "";
        AccountCreated = false;
        newProfile.text = "";
    }

    public void SelectedProfile(ProfileInput profile)
    {
        deleteProfile.gameObject.SetActive(true);
        if (currentSelection != null)
        {
            currentSelection.DeactivateSelection();
        }
        currentSelection = profile;
        print("selected" + profile.profileName + profile.profileID);
        currentSelection.ActivateSelection();
    }

    public void DeleteProfile()
    {
        if (currentSelection != null)
        {
            //SortProfiles(currentSelection.profileID);
            PlayerPrefs.SetString(PLAYERNAME + currentSelection.profileID, "");
            PlayerPrefs.SetInt(PROFILESCOUNT, PlayerPrefs.GetInt(PROFILESCOUNT) - 1);
            if (currentSelection.profileID >= profileMaxID)
            {
                int newProfileMaxID = 0;
                for (int i = 0; i < profileMaxID; i++)
                {
                    if (PlayerPrefs.GetString(PLAYERNAME) != "")
                    {
                        newProfileMaxID = i;
                    }
                }
                profileMaxID = newProfileMaxID;
                PlayerPrefs.SetInt(MAXID, profileMaxID);
            }

            currentSelection.DeleteProfile();
            currentSelection = null;
            deleteProfile.gameObject.SetActive(false);
        }
    }  
}