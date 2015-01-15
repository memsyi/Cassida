using UnityEngine;
using System.Collections;

public class ProfileInputController : MonoBehaviour 
{
    //public Profile OwnProfile { get; private set; }

    //public void SetOwnProfile(Profile profile)
    //{
    //    OwnProfile = profile;
    //}

    public void SelectThisProfile()
    {
        ProfileManager.Get().SelectProfile(this);

        ActivateSelection();
    }

    private void ActivateSelection()
    {
        gameObject.transform.FindChild("Image").gameObject.SetActive(true);
    }

    public void DeactivateSelection()
    {
        gameObject.transform.FindChild("Image").gameObject.SetActive(false);
    }

    public void DeleteProfileObject()
    {
        Destroy(this.gameObject);
    }

    void Start()
    {

    }

    void Update()
    {

    }
}
