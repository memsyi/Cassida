using UnityEngine;
using System.Collections;

public class ProfileController : MonoBehaviour 
{
    public void SelectThisProfile()
    {
        ProfileManager.Get().SelectProfile(this);

        ActivateSelection();
    }

    public void ActivateSelection()
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
