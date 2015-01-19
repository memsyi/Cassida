using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Profile
{
    public string ProfileName { get; protected set; }
    public int ID { get; protected set; }
    public string PlayerName { get; private set; }

    public ProfileInputController ProfileController { get; private set; }

    public Profile(string profileName, int id, string playerName, ProfileInputController profileController)
    {
        ProfileName = profileName;
        ID = id;
        PlayerName = playerName;

        ProfileController = profileController;
    }
}

public class ProfileManager : MonoBehaviour
{
    public Text errorMassage, profileStyle;
    public Transform profilesPosition;
    public Button deleteProfileButton;
    //private int profilesCount, profileMaxID;

    //public const string PLAYERNAME = "playername", CURRENTPROFILE = "currentprofile", PROFILESCOUNT = "profilecount", MAXID = "maxID";

    public Profile CurrentProfile { get; private set; }
    //private ProfileInputController CurrentSelection { get; set; }

    // Lists
    private List<Profile> ProfileList { get; set; }

    //private bool _accountCreated = false;

    //public bool AccountCreated
    //{
    //    get
    //    {
    //        return _accountCreated;
    //    }
    //    set
    //    {
    //        _accountCreated = value;
    //    }
    //}

    public void GetProfileName(Text text)
    {
        CreateNewProfile(text.text);
    }

    private void CreateNewProfile(string profileName)
    {
        if (profileName.Length < 4)
        {
            errorMassage.text = "Der Name muss mindestens 3 Zeichen lang sein";
            return;
        }
        //print("current profiles: " + profilesCount); /////////////////////

        AddNewProfile(profileName);

        CurrentProfile = ProfileList.Find(p => p.ProfileName == profileName);

        MenuManager.Get().ChangeMenu(GameObject.FindGameObjectWithTag(Tags.ProfileMenu).GetComponent<MenuController>());

        // get a new ID
        //int id = NewID();
        //if (id < 0)
        //{
        //    print("we got empty accounts???");
        //}
        //else
        //{
        

        
        //profileController.SetOwnProfile(newProfile);

        // set up profile stats

        //PlayerPrefs.SetString(PLAYERNAME + id, profileName.GetComponent<ProfileInput>().profileName);
        //profilesCount++;

        //PlayerPrefs.SetInt(PROFILESCOUNT, profilesCount);

        errorMassage.text = "";
        //AccountCreated = true;

        //PlayerPrefs.SetString(CURRENTPROFILE, profileName.text);
        //print("created a profile"); /////////////////////
    }

    private void AddNewProfile(string profileName)
    {
        Text profileObjectText = Instantiate(profileStyle, profilesPosition.position, profilesPosition.rotation) as Text;
        profileObjectText.transform.SetParent(profilesPosition);
        profileObjectText.transform.localScale = profilesPosition.localScale;

        var profileController = profileObjectText.gameObject.GetComponent<ProfileInputController>();
        profileObjectText.text = profileName;

        var newProfile = new Profile("new profile", ProfileList.Count, "Otto", profileController);
        
        ProfileList.Add(newProfile);
        ShowProfile(newProfile);
    }

    //private int NewID()
    //{
    //    int newID = -1;
    //    int count = PlayerPrefs.GetInt(PROFILESCOUNT) + 1;
    //    for (int i = 0; i < count; i++)
    //    {
    //        if (PlayerPrefs.GetString(PLAYERNAME + i) == "")
    //        {
    //            if (i >= profileMaxID)
    //            {
    //                PlayerPrefs.SetInt(MAXID, i);
    //                profileMaxID = i;
    //                print("maxID: " + profileMaxID);
    //            }
    //            return newID = i;
    //        }
    //    }
    //    return newID;
    //}

    public void SelectProfile(ProfileInputController profileController)
    {
        deleteProfileButton.gameObject.SetActive(true);

        if (CurrentProfile != null)
        {
            CurrentProfile.ProfileController.DeactivateSelection();
        }

        CurrentProfile = ProfileList.Find(p => p.ProfileController == profileController);
        //print("selected" + profile.profileName + profile.profileID);
    }

    private void ShowProfile(Profile profile)
    {
        AddNewProfile(profile.ProfileName);
    }

    private void ShowAllProfiles()
    {
        foreach (var profile in ProfileList)
        {
            ShowProfile(profile);
        }

        //if (PlayerPrefs.GetInt(PROFILESCOUNT) > 0)
        //{
        //    int countTo = PlayerPrefs.GetInt(MAXID);
        //    print("countTo : " + countTo);
        //    for (int i = 0; i <= countTo; i++)
        //    {
        //        if (PlayerPrefs.GetString(PLAYERNAME + i) != "")
        //        {
        //            // initiate the profile at the gui
        //            Text profile = InstantiateProfileObjectText();

        //            // set up profile stats
        //            profileStyle.GetComponent<ProfileInput>().SetUpProfile(i, PlayerPrefs.GetString(PLAYERNAME + i));
        //            profile.text = PlayerPrefs.GetString(PLAYERNAME + i);
        //        }
        //    }
        //}
    }

    //{
    //for (int i = -100; i < 100; i++)
    //{
    //    PlayerPrefs.SetString(PLAYERNAME + i, "");
    //}
    //PlayerPrefs.SetInt(PROFILESCOUNT, 0);
    //PlayerPrefs.SetInt(MAXID, 0);
    //}

    //public void SetUpNewProfile()
    //{
    //    errorMassage.text = "";
    //    //AccountCreated = false;
    //    //newProfile.text = "";
    //}

    public void DeletSelectedProfile()
    {
        DeletProfile(CurrentProfile);
    }

    private void DeletProfile(Profile profile)
    {
        if (profile == null)
        {
            return;
        }

        if (profile.ID == CurrentProfile.ID)
        {
            // TODO change profile to next
            //CurrentProfile = 
        }

        profile.ProfileController.DeleteProfileObject();
        CurrentProfile = null;
        ProfileList.Remove(profile);
        deleteProfileButton.gameObject.SetActive(false);
        // TODO save new profiles
    }

    //public void DeleteProfileObject(Profile profile)
    //{
    //    if (profile == null)
    //    {
    //        return;
    //    }
    //    //SortProfiles(currentSelection.profileID);
    //    //PlayerPrefs.SetString(PLAYERNAME + currentSelection.profileID, "");
    //    //PlayerPrefs.SetInt(PROFILESCOUNT, PlayerPrefs.GetInt(PROFILESCOUNT) - 1);
    //    //if (currentSelection.profileID >= profileMaxID)
    //    //{
    //    //    int newProfileMaxID = 0;
    //    //    for (int i = 0; i < profileMaxID; i++)
    //    //    {
    //    //        if (PlayerPrefs.GetString(PLAYERNAME) != "")
    //    //        {
    //    //            newProfileMaxID = i;
    //    //        }
    //    //    }
    //    //    profileMaxID = newProfileMaxID;
    //    //    PlayerPrefs.SetInt(MAXID, profileMaxID);
    //    //}
        

        
    //}

    private void DeletAllProfiles()
    {
        foreach (var profile in ProfileList)
        {
            DeletProfile(profile);
        }
    }

    private void Init()
    {
        ProfileList = new List<Profile>();
        //profileMaxID = PlayerPrefs.GetInt(MAXID);
        //profilesCount = PlayerPrefs.GetInt(PROFILESCOUNT);
        deleteProfileButton.gameObject.SetActive(false);
        ShowAllProfiles();
    }

    private void Start()
    {
        Init();
        //AddNewProfile("Maxim");
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

    private static ProfileManager _instance = null;
    public static ProfileManager Get()
    {
        if (_instance == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag(Tags.MenuManager);
            _instance = obj.AddComponent<ProfileManager>();
        }

        return _instance;
    }
}