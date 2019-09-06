//#define UI_DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDragger : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerUpHandler
{
    [Serializable]
    public class DragEvent : UnityEvent<PointerEventData>
    {
    }

    public Transform root;
    public Transform dragObj;
    public Camera uiCamera;
    [SerializeField]
    private float holdTime = 0.2f;

    public bool disableDrag;
    public DragEvent onDragOver = new DragEvent();
    public DragEvent onDragOut = new DragEvent();

    public DragEvent onWillDrag = new DragEvent();
    public DragEvent onBeginDrag = new DragEvent();
    public DragEvent onDrag = new DragEvent();
    public DragEvent onDrop = new DragEvent();
    public DragEvent onEndDrag = new DragEvent();

    public DragEvent onClick = new DragEvent();
    public DragEvent onRelease = new DragEvent();

    private GameObject mGo;
    private bool _ignoreClick;

    public void Start()
    {
        mGo = this.gameObject;
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        if (disableDrag) return;
        if (holdTime > 0)
        {
            StopAllCoroutines();
            StartCoroutine(DelayOnInitializePotentialDrag(eventData));
        }
        else
        {
            DoWillDragTrigger(eventData);
        }
    }

    private IEnumerator DelayOnInitializePotentialDrag(PointerEventData eventData)
    {
        yield return new WaitForSeconds(holdTime);
        _ignoreClick = true;
        DoWillDragTrigger(eventData);
    }

    private void DoWillDragTrigger(PointerEventData eventData)
    {
#if UI_DEBUG
        Debug.LogError("OnInitializePotentialDrag:" + eventData.position);
#endif
        if (root != null)
        {
            root.SetAsLastSibling();
        }
        if (dragObj != null)
        {
            if (uiCamera != null)
                dragObj.position = uiCamera.ScreenToWorldPoint(eventData.position);
            else
                dragObj.position = eventData.position;
        }

        onWillDrag.Invoke(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (disableDrag) return;
        _ignoreClick = true;
#if UI_DEBUG
        Debug.LogError("OnBeginDrag:" + eventData.pointerDrag.transform.parent);
#endif
        onBeginDrag.Invoke(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (disableDrag) return;
#if UI_DEBUG
        Debug.LogError("OnDrag:" + eventData.pointerDrag.transform.parent);
#endif
        if (dragObj != null)
        {
            if (uiCamera != null)
                dragObj.position = uiCamera.ScreenToWorldPoint(eventData.position);
            else
                dragObj.position = eventData.position;
        }
        onDrag.Invoke(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (disableDrag) return;
#if UI_DEBUG
        Debug.LogErrorFormat("OnEndDrag:{0}", eventData.pointerDrag.transform.parent);
#endif
        onEndDrag.Invoke(eventData);
        StopAllCoroutines();
        _ignoreClick = false;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (disableDrag) return;
#if UI_DEBUG
        Debug.LogErrorFormat("OnDrop:{0} {1}", eventData.pointerDrag.transform.parent, eventData.pointerEnter.transform.parent);
#endif
        onDrop.Invoke(eventData);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (disableDrag) return;
        if (eventData.dragging && mGo != eventData.pointerDrag)
        {
#if UI_DEBUG
            Debug.LogErrorFormat("OnDragOver:{0} {1}", eventData.pointerEnter.transform.parent, mGo.transform.parent);
#endif
            onDragOver.Invoke(eventData);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (disableDrag) return;
        if (eventData.dragging && mGo != eventData.pointerDrag)
        {
#if UI_DEBUG
            Debug.LogErrorFormat("OnDragOut:{0} {1}", eventData.pointerEnter.transform.parent, mGo.transform.parent);
#endif
            onDragOut.Invoke(eventData);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_ignoreClick)
        {
#if UI_DEBUG
            Debug.LogErrorFormat("OnClick:{0}", eventData.pointerPress.transform.parent);
#endif
            onClick.Invoke(eventData);
        }
        _ignoreClick = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopAllCoroutines();
        if (!eventData.dragging)
        {
#if UI_DEBUG
            Debug.LogErrorFormat("OnRelease:{0}", eventData.pointerPress.transform.parent);
#endif
            onRelease.Invoke(eventData);
        }
    }

    public void Dispose()
    {
        onDragOver.RemoveAllListeners();
        onDragOut.RemoveAllListeners();
        onWillDrag.RemoveAllListeners();
        onBeginDrag.RemoveAllListeners();
        onDrag.RemoveAllListeners();
        onDrop.RemoveAllListeners();
        onEndDrag.RemoveAllListeners();
        onClick.RemoveAllListeners();
        onRelease.RemoveAllListeners();
    }

    void OnDestroy()
    {
        Dispose();
    }
}
