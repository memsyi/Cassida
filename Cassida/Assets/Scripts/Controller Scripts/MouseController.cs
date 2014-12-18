using UnityEngine;

public struct MouseclickInformation
{
    public float DownTime { get; set; }
    public Vector2 DownPosition { get; set; }
}

public delegate void MouseclickHandler();

public class MouseController : MonoBehaviour
{
    [SerializeField]
    private float
        _mouseclickTime = 0.5f,
        _mouseclickDistance = 0.2f;

    public float MouseclickDistance
    {
        get { return _mouseclickDistance; }
        set { _mouseclickDistance = value; }
    }

    public float MouseclickTime
    {
        get { return _mouseclickTime; }
        set { _mouseclickTime = value; }
    }


    public Vector2 MousePositionOnMap { get { return GetMousePositionOnMap(); } }

    private Transform Map { get; set; }

    private MouseclickInformation _mouseclickInformation = new MouseclickInformation();

    #region Events
    public event MouseclickHandler
        LeftMousecklickEvent,
        RightMouseclickEvent,
        MiddleMouseclickEvent;
        
    #endregion

    private Vector2 GetMousePositionOnMap()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;

        var isRaycastColliding = Map.collider.Raycast(ray, out hitInfo, 100f);

        if (!isRaycastColliding)
        {
            return new Vector2(1000, 1000);
        }

        var position = new Vector2(hitInfo.point.x, hitInfo.point.z);

        return position;
    }

    private void CheckForMouseclicks()
    {
        for (int i = 1; i < 4; i++)
        {
            var buttonName = "Fire" + i;

            if (Input.GetButtonDown(buttonName))
            {
                _mouseclickInformation.DownTime = Time.time;
                _mouseclickInformation.DownPosition = MousePositionOnMap;
            }

            if (Input.GetButtonUp(buttonName)
                && Time.time - _mouseclickInformation.DownTime < MouseclickTime
                && Vector2.Distance(MousePositionOnMap, _mouseclickInformation.DownPosition) < MouseclickDistance)
            {
                //print(buttonName + " click!");
                switch (buttonName)
                {
                    case "Fire1":
                        if (LeftMousecklickEvent != null)
                        {
                            LeftMousecklickEvent();
                        }
                        break;
                    case "Fire2":
                        if (RightMouseclickEvent != null)
                        {
                            RightMouseclickEvent();
                        }
                        break;
                    case "Fire3":
                        if (MiddleMouseclickEvent != null)
                        {
                            MiddleMouseclickEvent();
                        }
                        break;
                    default:
                        break;
                }

            }
        }
    }

    private void Init()
    {
        Map = GameObject.FindGameObjectWithTag(Tags.Map).transform;
    }

    private void Start()
    {
        
    }

    private void Awake()
    {
        Init();
    }

    private void Update()
    {
        CheckForMouseclicks();
    }
}
