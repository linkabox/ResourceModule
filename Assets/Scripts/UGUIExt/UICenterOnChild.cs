using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;

[RequireComponent(typeof(ScrollRect))]
public class UICenterOnChild : MonoBehaviour, IEndDragHandler
{
    public delegate void OnCenterCallback(int index);

    public OnCenterCallback onCenterStart;
    public OnCenterCallback onCenterEnd;

    public Transform Content;
    public bool snapOnEnable;
    public float snapWatchOffset = 0.5f;
    public float snapTweenTime = 0.2f;
    public Ease easeType;

    public ScrollRect ScrollRect
    {
        get { return _scrollRect; }
    }
    private ScrollRect _scrollRect;
    private float _perstep;

    public int CellNum { get; private set; }

    public int CurCellIndex
    {
        get { return _curCellIndex; }
    }

    private int _curCellIndex = 0;

    public float ScrollPosition
    {
        get
        {
            return _scrollRect.horizontal ? _scrollRect.horizontalNormalizedPosition : _scrollRect.verticalNormalizedPosition;
        }
        set
        {
            value = Mathf.Clamp(value, 0, 1);
            if (_scrollRect.horizontal)
            {
                _scrollRect.horizontalNormalizedPosition = value;
            }
            else
            {
                _scrollRect.verticalNormalizedPosition = value;
            }
        }
    }

    // Use this for initialization
    void Awake()
    {
        _scrollRect = GetComponent<ScrollRect>();
    }

    void OnValidate()
    {
        snapWatchOffset = Mathf.Clamp01(snapWatchOffset);
    }

    void OnEnable()
    {
        ResetItem(this.Content);
        if (snapOnEnable)
            Snap();
    }

    void OnDisable()
    {

    }

    public void ResetItem(Transform content)
    {
        if (content == null) return;
        int itemCount = 0;
        foreach (Transform child in content)
        {
            if (child.gameObject.activeSelf)
            {
                ++itemCount;
            }
        }
        ResetItem(itemCount);
    }

    public void ResetItem(int itemCount)
    {
        CellNum = itemCount;
        _perstep = 1f / (CellNum - 1);
    }

    public int FindCellIndexAtNormalizedPos(float normalizedPos)
    {
        // call the overrloaded method on the entire range of the list
        return _FindCellIndexAtNormalizedPos(normalizedPos, 0, CellNum - 1);
    }

    private int _FindCellIndexAtNormalizedPos(float normalizedPos, int startIndex, int endIndex)
    {
        // if the range is invalid, then we found our index, return the start index
        if (startIndex >= endIndex) return startIndex;

        // determine the middle point of our binary search
        var midIndex = (startIndex + endIndex) / 2;

        // if the middle index is greater than the position, then search the last
        if (GetCellScrollPos(midIndex) + _perstep * snapWatchOffset >= normalizedPos)
            return _FindCellIndexAtNormalizedPos(normalizedPos, startIndex, midIndex);
        else
            return _FindCellIndexAtNormalizedPos(normalizedPos, midIndex + 1, endIndex);
    }

    public float GetCellScrollPos(int cellIndex)
    {
        return _perstep * cellIndex;
    }

    [ContextMenu("MovePrevious")]
    public void MovePrevious()
    {
        JumpTo(_curCellIndex - 1, snapTweenTime, onCenterStart, onCenterEnd);
    }

    [ContextMenu("MoveNext")]
    public void MoveNext()
    {
        JumpTo(_curCellIndex + 1, snapTweenTime, onCenterStart, onCenterEnd);
    }

    [ContextMenu("Snap")]
    public int Snap()
    {
        if (CellNum == 0) return -1;

        // stop the scroller
        //_scrollRect.velocity = Vector2.zero;

        var snapCellIndex = FindCellIndexAtNormalizedPos(ScrollPosition);

        JumpTo(snapCellIndex, snapTweenTime, onCenterStart, onCenterEnd);

        return snapCellIndex;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Snap();
    }

    public void JumpTo(int cellIndex, float tweenTime = 0f, OnCenterCallback onStart = null, OnCenterCallback onEnd = null)
    {
        if (cellIndex < 0 || cellIndex >= CellNum)
            return;

        _curCellIndex = cellIndex;

        float endScrollPos = GetCellScrollPos(cellIndex);
        _scrollRect.DOKill();
        if (tweenTime < 0.001f)
        {
            //Instant tween
            this.ScrollPosition = endScrollPos;
            if (onStart != null) onStart(cellIndex);
            if (onEnd != null) onEnd(cellIndex);
        }
        else
        {
            //use DOTween move scrollRect
            if (onStart != null) onStart(cellIndex);

            TweenCallback onTweenFinish = () =>
            {
                if (onEnd != null)
                    onEnd(cellIndex);
            };

            if (_scrollRect.horizontal)
                _scrollRect.DOHorizontalNormalizedPos(endScrollPos, tweenTime).OnComplete(onTweenFinish).SetEase(easeType);
            else
                _scrollRect.DOVerticalNormalizedPos(endScrollPos, tweenTime).OnComplete(onTweenFinish).SetEase(easeType);
        }
    }

    public void Dispose()
    {
        onCenterStart = null;
        onCenterEnd = null;
    }

    void OnDestroy()
    {
        Dispose();
    }
}