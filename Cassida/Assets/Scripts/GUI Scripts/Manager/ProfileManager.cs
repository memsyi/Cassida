using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Profile : IJSON
{
    public string ProfileName { get; protected set; }
    public string PlayerName { get; private set; }

    public ProfileController ProfileController { get; private set; }

    public Profile(string profileName, ProfileController profileController)
    {
        ProfileName = profileName;
        PlayerName = "";

        ProfileController = profileController;
    }

    public Profile()
    {

    }

    public JSONObject ToJSON()
    {
        var jsonObject = JSONObject.obj;
        jsonObject[JSONs.Name] = new JSONObject(ProfileName);
        jsonObject[JSONs.PlayerName] = new JSONObject(PlayerName);
        return jsonObject;
    }

    public void FromJSON(JSONObject jsonObject)
    {
        ProfileName = (string)jsonObject[JSONs.Name];
        PlayerName = (string)jsonObject[JSONs.PlayerName];

        ProfileController = ProfileManager.Get().AddProfileObject(ProfileName);
        ProfileManager.Get().ProfileList.Add(this);
    }
}

public class ProfileManager : MonoBehaviour, IJSON
{
    #region Variables
    [SerializeField]
    private Text errorMassage, profileStyle;
    private Text ProfileStyle
    {
        get { return profileStyle; }
    }
    private Text ErrorMassage
    {
        get { return errorMassage; }
    }

    [SerializeField]
    private Transform profilesPosition;
    private Transform ProfilesPosition
    {
        get { return profilesPosition; }
    }

    [SerializeField]
    private Button deleteProfileButton;
    private Button DeleteProfileButton
    {
        get { return deleteProfileButton; }
    }

    public Profile CurrentProfile { get; private set; }

    // Lists
    public List<Profile> ProfileList { get; private set; }
    #endregion

    public void GetProfileName(Text text)
    {
        CreateNewProfile(text.text);
    }

    private void CreateNewProfile(string profileName)
    {
        if (profileName.Length < 3)
        {
            errorMassage.text = "Der Name muss mindestens 3 Zeichen lang sein";
            return;
        }

        if (GetProfile(profileName) != null)
        {
            errorMassage.text = "profile name already exists!";
            return;
        }

        AddNewProfile(profileName);

        CurrentProfile = GetProfile(profileName);

        MenuManager.Get().ChangeMenu(GameObject.FindGameObjectWithTag(Tags.ProfileMenu).GetComponent<MenuController>());

        errorMassage.text = "";
    }

    private void AddNewProfile(string profileName)
    {
        var profileController = AddProfileObject(profileName);

        var newProfile = new Profile(profileName, profileController);

        ProfileList.Add(newProfile);
        SelectProfile(newProfile.ProfileController);
    }

    public ProfileController AddProfileObject(string profileName)
    {
        Text profileObjectText = Instantiate(profileStyle, profilesPosition.position, profilesPosition.rotation) as Text;
        profileObjectText.transform.SetParent(profilesPosition);
        profileObjectText.transform.localScale = profilesPosition.localScale;

        var profileController = profileObjectText.gameObject.GetComponent<ProfileController>();
        profileObjectText.text = profileName;

        return profileController;
    }

    public void SelectProfile(ProfileController profileController)
    {
        if (ProfileList.Count > 1)
        {
            deleteProfileButton.gameObject.SetActive(true);
        }

        if (CurrentProfile != null)
        {
            CurrentProfile.ProfileController.DeactivateSelection();
        }

        CurrentProfile = ProfileList.Find(p => p.ProfileController == profileController);
        CurrentProfile.ProfileController.ActivateSelection();

        SaveAllProfiles();
    }

    public void DeletSelectedProfile()
    {
        DeletProfile(CurrentProfile);
    }

    private void DeletProfile(Profile profile)
    {
        if (profile == null || ProfileList.Count < 2)
        {
            return;
        }

        profile.ProfileController.DeleteProfileObject();
        ProfileList.Remove(profile);
        deleteProfileButton.gameObject.SetActive(false);

        if (profile.ProfileName == CurrentProfile.ProfileName)
        {
            if (ProfileList.Count <= 0)
            {
                CurrentProfile = null;
                return;
            }

            CurrentProfile = ProfileList[0];
            SelectProfile(CurrentProfile.ProfileController);
        }

        SaveAllProfiles();
    }

    private void SaveAllProfiles()
    {
        PlayerPrefs.SetString("Profiles", ToJSON().print());
    }

    private void DeletAllProfiles()
    {
        for(int i = ProfileList.Count -1; i > 0; i--)
        {
            DeletProfile(ProfileList[i]);
        }

        PlayerPrefs.DeleteKey("Profiles");
    }

    private Profile GetProfile(string profileName)
    {
        return ProfileList.Find(p => p.ProfileName == profileName);
    }

    private void Init()
    {
        ProfileList = new List<Profile>();

        if (PlayerPrefs.HasKey("Profiles"))
        {
            FromJSON(JSONParser.parse(PlayerPrefs.GetString("Profiles")));

            if (CurrentProfile != null)
            {
                SelectProfile(CurrentProfile.ProfileController);
            }

            if (ProfileList.Count > 1)
            {
                return;
            }
        }
        
        deleteProfileButton.gameObject.SetActive(false);
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

    public JSONObject ToJSON()
    {
        var jsonObject = JSONObject.obj;
        jsonObject[JSONs.Profiles] = JSONObject.CreateList(ProfileList);
        jsonObject[JSONs.Profile] = new JSONObject(CurrentProfile.ProfileName);
        return jsonObject;
    }

    public void FromJSON(JSONObject jsonObject)
    {
        JSONObject.ReadList<Profile>(jsonObject[JSONs.Profiles]);
        CurrentProfile = GetProfile((string)jsonObject[JSONs.Profile]);
    }
}