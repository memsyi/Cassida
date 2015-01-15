using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class WorldManager : MonoBehaviour
{
    #region Variables

    #endregion

    public void InitializeWorld()
    {
        MapManager.Get().InitializeMap();
    }

    private void Init()
    {

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

    private static WorldManager _instance = null;
    public static WorldManager Get()
    {
        if (_instance == null)
        {
            GameObject obj = GameObject.FindGameObjectWithTag(Tags.Manager);
            _instance = obj.AddComponent<WorldManager>();
        }

        return _instance;
    }
}
