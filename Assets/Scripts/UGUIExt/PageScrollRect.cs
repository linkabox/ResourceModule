using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    public class PageScrollRect : ScrollRect
    {
        public delegate void ScrollerSnappedDelegate(int cellIndex);
        public ScrollerSnappedDelegate onSnapBegin;
        public ScrollerSnappedDelegate onSnapEnd;

        public int cellNum;

        public bool snapOnEnable;
        public float snapVelocityThreshold = 100f;
        public float snapWatchOffset = 0.5f;
        public float snapTweenTime = 0.2f;
        public Ease easeType;

        private float _perstep;
        private int _curCellIndex;

        public float LinearVelocity
        {
            get
            {
                // return the velocity component depending on which direction this is scrolling
                return (this.horizontal ? this.velocity.x : this.velocity.y);
            }
            set
            {
                // set the appropriate component of the velocity
                this.velocity = this.horizontal ? new Vector2(value, 0) : new Vector2(0, value);
            }
        }

        public float ScrollPosition
        {
            get
            {
                return this.horizontal ? this.horizontalNormalizedPosition : this.verticalNormalizedPosition;
            }
            set
            {
                value = Mathf.Clamp(value, 0, 1);
                if (this.horizontal)
                {
                    this.horizontalNormalizedPosition = value;
                }
                else
                {
                    this.verticalNormalizedPosition = value;
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            ResetItem(this.content);
            if (snapOnEnable)
                Snap();
        }

        public void ResetItem(Transform grid)
        {
            if (grid == null) return;
            int itemCount = 0;
            foreach (Transform child in grid)
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
            cellNum = itemCount;
            _perstep = 1f / (cellNum - 1);
        }

        public float GetCellScrollPos(int cellIndex)
        {
            if (this.vertical)
                return Mathf.Clamp01(_perstep * (cellNum - cellIndex - 1));

            return Mathf.Clamp01(_perstep * cellIndex);
        }

        public int FindCellIndexAtNormalizedPos(float normalizedPos)
        {
            // call the overrloaded method on the entire range of the list
            return _FindCellIndexAtNormalizedPos(normalizedPos, 0, cellNum - 1);
        }

        private int _FindCellIndexAtNormalizedPos(float normalizedPos, int startIndex, int endIndex)
        {
            // if the range is invalid, then we found our index, return the start index
            if (startIndex >= endIndex) return startIndex;

            // determine the middle point of our binary search
            var midIndex = (startIndex + endIndex) / 2;

            if (this.vertical)
            {
                // if the middle index is greater than the position, then search the last
                if (GetCellScrollPos(midIndex) - _perstep * snapWatchOffset < normalizedPos)
                    return _FindCellIndexAtNormalizedPos(normalizedPos, startIndex, midIndex);
                else
                    return _FindCellIndexAtNormalizedPos(normalizedPos, midIndex + 1, endIndex);
            }
            else
            {
                // if the middle index is greater than the position, then search the last
                if (GetCellScrollPos(midIndex) + _perstep * snapWatchOffset >= normalizedPos)
                    return _FindCellIndexAtNormalizedPos(normalizedPos, startIndex, midIndex);
                else
                    return _FindCellIndexAtNormalizedPos(normalizedPos, midIndex + 1, endIndex);
            }
        }

        #region DragEvent

        private Vector2 _lastDragPos;
        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);

            _lastDragPos = eventData.position;
        }

        //public override void OnDrag(PointerEventData eventData)
        //{
        //    base.OnDrag(eventData);
        //}

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            Vector2 dragDelta = _lastDragPos - eventData.position;
            float dragOffset = this.vertical ? dragDelta.y : dragDelta.x;
            //Debug.LogError("drag offset:" + dragOffset);

            if (Mathf.Abs(dragOffset) <= snapVelocityThreshold)
            {
                Snap();
                //Debug.LogError("EndDrag Snap closest:" + Snap());
            }
            else
            {
                int nextIndex = _curCellIndex;
                if (this.horizontal)
                {
                    if (dragOffset < 0)
                        nextIndex -= 1;
                    else
                        nextIndex += 1;
                }
                else
                {
                    if (dragOffset > 0)
                        nextIndex -= 1;
                    else
                        nextIndex += 1;
                }
                JumpTo(nextIndex, snapTweenTime);
                //Debug.LogError("EndDrag JumpToNext:" + nextIndex);
            }
        }
        #endregion

        public int Snap()
        {
            if (cellNum == 0) return -1;

            var snapCellIndex = FindCellIndexAtNormalizedPos(ScrollPosition);
            JumpTo(snapCellIndex, snapTweenTime);
            return snapCellIndex;
        }

        private void SnapJumpComplete()
        {
            if (onSnapEnd != null)
                onSnapEnd(_curCellIndex);
        }

        public void JumpTo(int cellIndex, float tweenTime = 0, TweenCallback jumpComplete = null)
        {
            if (cellIndex < 0 || cellIndex >= cellNum)
                return;

            // stop the scroller
            this.StopMovement();

            _curCellIndex = cellIndex;
            if (onSnapBegin != null)
                onSnapBegin(cellIndex);

            if (jumpComplete == null)
            {
                jumpComplete = SnapJumpComplete;
            }

            float endScrollPos = GetCellScrollPos(cellIndex);
            this.DOKill();
            if (tweenTime < 0.001f)
            {
                //Instant tween
                this.ScrollPosition = endScrollPos;
                jumpComplete();
            }
            else
            {
                //use DOTween move scrollRect
                if (this.horizontal)
                    this.DOHorizontalNormalizedPos(endScrollPos, tweenTime).OnComplete(jumpComplete).SetEase(easeType);
                else
                    this.DOVerticalNormalizedPos(endScrollPos, tweenTime).OnComplete(jumpComplete).SetEase(easeType);
            }
        }
    }
}
