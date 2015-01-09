using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour {

	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}


    public void LoadLevel(string levelname)
    {
        if (levelname == "Quit")
        {
            Application.Quit();
        }
        else Application.LoadLevel(levelname);
    }

}
