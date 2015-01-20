using UnityEngine;
using System.Collections;

public class MenuController : MonoBehaviour
{
    private Animator _animator;
    private CanvasGroup _canvasGroup;

    [SerializeField]
    private string _menuName;

    public string MenuName
    {
        get { return _menuName; }
        set { _menuName = value; }
    }

    public bool IsOpen
    {
        get { return _animator.GetBool("IsOpen"); }
        set
        {
            _animator.SetBool("IsOpen", value);

            if (!value)
            {
                _canvasGroup.blocksRaycasts = _canvasGroup.interactable = false;
            }
        }
    }

    public void Awake()
    {
        _animator = GetComponent<Animator>();
        _canvasGroup = GetComponent<CanvasGroup>();

        var rect = GetComponent<RectTransform>();
        rect.offsetMax = rect.offsetMin = new Vector2(0, 0);

        _canvasGroup.blocksRaycasts = _canvasGroup.interactable = false;
    }

    public void Update()
    {
        if (!IsOpen)
        {
            return;
        }
        _canvasGroup.blocksRaycasts = _canvasGroup.interactable = true;
    }
}
