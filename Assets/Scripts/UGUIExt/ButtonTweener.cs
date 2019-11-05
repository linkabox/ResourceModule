using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonTweener : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    public Animator animator;

    private bool isPress;
    // Use this for initialization
    void Start()
    {
        if (animator == null)
            animator = this.GetComponent<Animator>();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        //Debug.LogError("OnPointerUp:" + Time.frameCount);
        isPress = false;
        animator.SetTrigger("Release");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //Debug.LogError("OnPointerDown:" + Time.frameCount);
        isPress = true;
        animator.SetTrigger("Press");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //Debug.LogError("OnPointerEnter:" + Time.frameCount);
        if (isPress)
            animator.SetTrigger("Press");
    }

    /// <summary>
    /// Evaluate eventData and transition to appropriate state.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        //Debug.LogError("OnPointerExit:" + Time.frameCount);
        if (isPress)
            animator.SetTrigger("Release");
    }
}
