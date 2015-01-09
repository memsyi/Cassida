using UnityEngine;
using System.Collections;

public class ProfileInput : MonoBehaviour 
{
    public string profileName;
    public int profileID;

    private ProfileManager manager;

	// Use this for initialization
	void Start () 
    {
        manager = FindObjectOfType<ProfileManager>();
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    public void SetUpProfile(int id, string name)
    {
        profileName = name;
        profileID = id;
        print("created a profile with the id: " + id + " and name: " + name);
    }

    public void OnClick()
    {
        manager.SelectedProfile(profileID);
    }
}
