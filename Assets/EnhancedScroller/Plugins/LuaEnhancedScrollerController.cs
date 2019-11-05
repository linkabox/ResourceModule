using System;
using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.EventSystems;

public class LuaEnhancedScrollerController : MonoBehaviour, IEnhancedScrollerDelegate, IBeginDragHandler, IEndDragHandler
{
    public delegate int DataCountGetterDelegate(EnhancedScroller scroller);
    public delegate float CellSizeGetterDelegate(EnhancedScroller scroller, int dataIndex);
    public delegate EnhancedScrollerCellView CellDataSetterDelegate(EnhancedScroller scroller, int dataIndex, int cellIndex, EnhancedScrollerCellView cellView);

    public delegate void PullScrollDelegate(float percent, int mode);
    public delegate void PullRefreshDelegate(int pullToRefresh);

    /// <summary>
    /// This is our scroller we will be a delegate for
    /// </summary>
    public EnhancedScroller scroller;
    public EnhancedScrollerCellView cellViewPrefab;

    public int dataCount;
    public DataCountGetterDelegate dataCountHandler;

    public float cellSize = 100f;
    public CellSizeGetterDelegate cellSizeHandler;

    public CellDataSetterDelegate cellDataSetter;

    /// <summary>
    /// The higher the number here, the more we have to pull down to refresh
    /// </summary>
    public float pullDownThreshold = 100f;

    /// <summary>
    /// Whether the scroller is being dragged
    /// </summary>
    private bool _dragging = true;

    /// <summary>
    /// Whether we should refresh after releasing the drag
    /// </summary>
    private int _pullToRefresh = 0;

    public PullScrollDelegate onPullScroll;
    public PullRefreshDelegate onPullRefresh;

    // Use this for initialization
    void Awake()
    {
        // tell the scroller that this script will be its delegate
        scroller.Delegate = this;
        // tell our controller to monitor the scroller's scrolled event.
        scroller.scrollerScrolled = ScrollerScrolled;
    }

    void Start()
    {

    }

    #region EnhancedScroller Handlers
    public int GetNumberOfCells(EnhancedScroller scroller)
    {
        if (dataCountHandler != null)
        {
            return dataCountHandler(scroller);
        }
        return dataCount;
    }

    public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
    {
        if (cellSizeHandler != null)
        {
            return cellSizeHandler(scroller, dataIndex);
        }
        return cellSize;
    }

    public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
    {
        EnhancedScrollerCellView cellView = null;
        //有设置Prefab直接生成
        if (cellViewPrefab != null)
        {
            cellView = scroller.GetCellView(cellViewPrefab);
            cellView.name = "Cell_" + cellIndex + "_" + dataIndex;
        }

        //如果Prefab为空，需要在Lua处理Prefab加载逻辑
        if (cellDataSetter != null)
        {
            cellView = cellDataSetter(scroller, dataIndex, cellIndex, cellView);
        }
        return cellView;
    }
    #endregion

    #region Pull Event

    public void OnBeginDrag(PointerEventData data)
    {
        // we are now dragging.
        // we flag this so that refreshing won't occur if the scroller
        // is scrolling due to inertia. 
        // the user must drag manually in this example.
        _dragging = true;
    }

    /// <summary>
    /// This delegate will fire when the scroller is scrolled
    /// </summary>
    /// <param name="scroller"></param>
    /// <param name="val"></param>
    /// <param name="scrollPosition"></param>
    private void ScrollerScrolled(EnhancedScroller scroller, Vector2 val, float scrollPosition)
    {
        int mode = 0;
        float percent = 0f;
        float scrollSize = scroller.ScrollSize;
        if (_dragging)
        {
            //容器还没撑满,直接判断container的偏移值
            if (scrollSize < 0)
            {
                scrollPosition = scroller.scrollDirection == EnhancedScroller.ScrollDirectionEnum.Vertical
                    ? scroller.ScrollRect.content.anchoredPosition.y
                    : -scroller.ScrollRect.content.anchoredPosition.x;
            }

            if (scrollPosition < 0)
            {
                //滚动到头部
                mode = -1;
                percent = Mathf.Clamp01(scrollPosition / -pullDownThreshold);
            }
            else if (scrollSize > 0 && scrollPosition > scrollSize)
            {
                //滚动到尾部
                mode = 1;
                percent = Mathf.Clamp01((scrollPosition - scrollSize) / pullDownThreshold);
            }
            //设置刷新标记，EndDrag时判断是否进行刷新操作
            _pullToRefresh = percent >= 1 ? mode : 0;

            if (onPullScroll != null)
            {
                onPullScroll(percent, mode);
            }
        }
    }

    public void OnEndDrag(PointerEventData data)
    {
        // no longer dragging
        _dragging = false;
        if (onPullRefresh != null)
        {
            onPullRefresh(_pullToRefresh);
        }
        _pullToRefresh = 0;
    }

    #endregion

    void OnDestroy()
    {
        dataCountHandler = null;
        cellSizeHandler = null;
        cellDataSetter = null;

        onPullScroll = null;
        onPullRefresh = null;
    }
}
