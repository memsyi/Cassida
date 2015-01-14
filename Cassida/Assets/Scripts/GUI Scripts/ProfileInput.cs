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
        profileID = id;
        profileName = name;
        
        print("created a profile with the id: " + id + " and name: " + name);
    }

    public void Selected()
    {
        manager.SelectedProfile(this);
    }

    public void ActivateSelection()
    {
        gameObject.transform.FindChild("Image").gameObject.SetActive(true);
    }

    public void DeactivateSelection()
    {
        gameObject.transform.FindChild("Image").gameObject.SetActive(false);
    }

    public void DeleteProfile()
    {
        Destroy(this.gameObject);
    }
}
