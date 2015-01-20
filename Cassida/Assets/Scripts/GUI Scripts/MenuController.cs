using UnityEngine;
using System.Collections;

public class MenuController : MonoBehaviour
{
    private Animator _animator;
    private CanvasGroup _canvasGroup;

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

    private void Start()
    {
        _canvasGroup.blocksRaycasts = _canvasGroup.interactable = false;
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _canvasGroup = GetComponent<CanvasGroup>();

        var rect = GetComponent<RectTransform>();
        rect.offsetMax = rect.offsetMin = new Vector2(0, 0);
    }

    private void Update()
    {
        if (!IsOpen)
        {
            return;
        }
        _canvasGroup.blocksRaycasts = _canvasGroup.interactable = true;
    }
}
