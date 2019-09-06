using System;
using Coffee.UIExtensions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public static class TweenExt
{
    public static Quaternion LookAt2D(this Transform trans, Transform target, float facing = 0)
    {
        return trans.LookAt2D(target.position, facing);
    }

    public static Quaternion LookAt2D(this Transform trans, Vector3 target, float facing = 0)
    {
        var dir = target - trans.position;
        var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        angle -= facing;
        var rot = Quaternion.AngleAxis(angle, Vector3.forward);
        trans.rotation = rot;
        return rot;
    }

    /// <summary>
    /// 插值跟随移动到目标位置
    /// </summary>
    /// <param name="arrow"></param>
    /// <param name="start"></param>
    /// <param name="target"></param>
    /// <param name="duration"></param>
    /// <param name="follow"></param>
    /// <returns></returns>
    public static Tweener DOFollow2D(this Transform arrow, Vector3 start, Transform target, float duration, bool follow = true)
    {
        return DOTween.To(t =>
        {
            var newPos = Vector3.Lerp(start, target.position, t);
            if (follow && Math.Abs(t - 1) > 0.01f)
                arrow.LookAt2D(newPos);
            arrow.position = newPos;
        }, 0, 1, duration).SetTarget(arrow);
    }

    public static Tweener DOFollow2D(this Transform arrow, Vector3 start, Vector3 targetPos, float duration, bool follow = true)
    {
        return DOTween.To(t =>
        {
            var newPos = Vector3.Lerp(start, targetPos, t);
            if (follow && Math.Abs(t - 1) > 0.01f)
                arrow.LookAt2D(newPos);
            arrow.position = newPos;
        }, 0, 1, duration).SetTarget(arrow);
    }

    public static Tweener DOFollow(this Transform arrow, Vector3 start, Transform target, float duration, bool follow = true)
    {
        return DOTween.To(t =>
        {
            var newPos = Vector3.Lerp(start, target.position, t);
            if (follow && Math.Abs(t - 1) > 0.01f)
                arrow.LookAt(newPos);
            arrow.position = newPos;
        }, 0, 1, duration).SetTarget(arrow);
    }

    public static Tweener DOFollow(this Transform arrow, Vector3 start, Vector3 targetPos, float duration, bool follow = true)
    {
        return DOTween.To(t =>
        {
            var newPos = Vector3.Lerp(start, targetPos, t);
            if (follow && Math.Abs(t - 1) > 0.01f)
                arrow.LookAt(newPos);
            arrow.position = newPos;
        }, 0, 1, duration).SetTarget(arrow);
    }

    /// <summary>
    /// 抛物线插值移动到指定位置点
    /// </summary>
    /// <param name="arrow"></param>
    /// <param name="start"></param>
    /// <param name="targetPos"></param>
    /// <param name="duration"></param>
    /// <param name="follow">x轴跟随切线方向旋转</param>
    /// <param name="parabolaFactor">值越小抛物线越平缓，为0时相当于Lerp</param>
    /// <returns></returns>
    public static Tweener DOParabolaMove2D(this Transform arrow, Vector3 start, Vector3 targetPos, float duration, bool follow = true, float parabolaFactor = 0.5f)
    {
        return DOTween.To(t =>
        {
            var newPos = ParabolaLerp(start, targetPos, t, parabolaFactor);
            //keep follow tangent direction
            if (follow && Math.Abs(t - 1) > 0.01f)
                arrow.LookAt2D(newPos);
            arrow.position = newPos;
        }, 0, 1, duration).SetTarget(arrow);
    }

    public static Tweener DOParabolaLocalMove2D(this Transform arrow, Vector3 start, Vector3 targetPos, float duration, bool follow = true, float parabolaFactor = 0.5f)
    {
        return DOTween.To(t =>
        {
            var newPos = ParabolaLerp(start, targetPos, t, parabolaFactor);
            //keep follow tangent direction
            if (follow && Math.Abs(t - 1) > 0.01f)
            {
                var lookAt = arrow.parent.TransformPoint(newPos);
                arrow.LookAt2D(lookAt);
            }
            arrow.localPosition = newPos;
        }, 0, 1, duration).SetTarget(arrow);
    }

    /// <summary>
    /// 抛物线插值移动到指定Transform位置
    /// </summary>
    /// <param name="arrow"></param>
    /// <param name="start"></param>
    /// <param name="target"></param>
    /// <param name="duration"></param>
    /// <param name="follow">x轴跟随切线方向旋转</param>
    /// <param name="parabolaFactor">值越小抛物线越平缓，为0时相当于Lerp</param>
    /// <returns></returns>
    public static Tweener DOParabolaMove2D(this Transform arrow, Vector3 start, Transform target, float duration, bool follow = true, float parabolaFactor = 0.5f)
    {
        return DOTween.To(t =>
        {
            var newPos = ParabolaLerp(start, target.position, t, parabolaFactor);
            //keep follow tangent direction
            if (follow && Math.Abs(t - 1) > 0.01f)
                arrow.LookAt2D(newPos);
            arrow.position = newPos;
        }, 0, 1, duration).SetTarget(arrow);
    }

    public static Tweener DOParabolaLocalMove2D(this Transform arrow, Vector3 start, Transform target, float duration, bool follow = true, float parabolaFactor = 0.5f)
    {
        return DOTween.To(t =>
        {
            var newPos = ParabolaLerp(start, target.localPosition, t, parabolaFactor);
            //keep follow tangent direction
            if (follow && Math.Abs(t - 1) > 0.01f)
            {
                var lookAt = arrow.parent.TransformPoint(newPos);
                arrow.LookAt2D(lookAt);
            }
            arrow.localPosition = newPos;
        }, 0, 1, duration).SetTarget(arrow);
    }

    public static Tweener DOParabolaMove(this Transform arrow, Vector3 start, Transform target, float duration, bool follow = true, float parabolaFactor = 0.5f)
    {
        return DOTween.To(t =>
        {
            var newPos = ParabolaLerp(start, target.position, t, parabolaFactor);
            //keep follow tangent direction
            if (follow && Math.Abs(t - 1) > 0.01f)
                arrow.LookAt(newPos);
            arrow.position = newPos;
        }, 0, 1, duration).SetTarget(arrow);
    }

    public static Tweener DOParabolaLocalMove(this Transform arrow, Vector3 locStart, Transform target, float duration, bool follow = true, float parabolaFactor = 0.5f)
    {
        return DOTween.To(t =>
        {
            var newPos = ParabolaLerp(locStart, target.localPosition, t, parabolaFactor);
            //keep follow tangent direction
            if (follow && Math.Abs(t - 1) > 0.01f)
            {
                var lookAt = arrow.parent.TransformPoint(newPos);
                arrow.LookAt(lookAt);
            }
            arrow.localPosition = newPos;
        }, 0, 1, duration).SetTarget(arrow);
    }

    public static Tweener DOParabolaMove(this Transform arrow, Vector3 start, Vector3 targetPos, float duration, bool follow = true, float parabolaFactor = 0.5f)
    {
        return DOTween.To(t =>
        {
            var newPos = ParabolaLerp(start, targetPos, t, parabolaFactor);
            //keep follow tangent direction
            if (follow && Math.Abs(t - 1) > 0.01f)
                arrow.LookAt(newPos);
            arrow.position = newPos;
        }, 0, 1, duration).SetTarget(arrow);
    }

    public static Tweener DOParabolaLocalMove(this Transform arrow, Vector3 locStart, Vector3 targetPos, float duration, bool follow = true, float parabolaFactor = 0.5f)
    {
        return DOTween.To(t =>
        {
            var newPos = ParabolaLerp(locStart, targetPos, t, parabolaFactor);
            //keep follow tangent direction
            if (follow && Math.Abs(t - 1) > 0.01f)
            {
                var lookAt = arrow.parent.TransformPoint(newPos);
                arrow.LookAt(lookAt);
            }
            arrow.localPosition = newPos;
        }, 0, 1, duration).SetTarget(arrow);
    }

    public static Vector3 ParabolaLerp(Vector3 start, Vector3 dest, float t, float parabolaFactor = 0.1f)
    {
        float c = Vector3.Distance(start, dest) * parabolaFactor;
        float kk = t - 0.5f;
        float py = c - 4f * c * kk * kk;
        return Vector3.Lerp(start, dest, t) + new Vector3(0, py, 0);
    }

    public static Tweener DOSliderText(this Slider target, float endValue, Text uiText, float endTextValue, float duration, string format = "{0}/{1}")
    {
        var tween = target.DOValue(endValue, duration).OnUpdate(() =>
        {
            var curVal = Mathf.FloorToInt(target.value * endTextValue);
            uiText.text = string.Format(format, curVal, endTextValue);
        });
        tween.SetTarget(target);
        return tween;
    }

    public static Tweener DOSizeDelta(this RectTransform target, float x, float y, float duration, bool snapping = false)
    {
        return DOTweenModuleUI.DOSizeDelta(target, new Vector2(x, y), duration, snapping);
    }

    #region UIEffect

    public static Tweener DOFade(this UIEffect target, float endValue, float duration)
    {
        return DOTween.To(() => target.effectFactor, x => target.effectFactor = x, endValue, duration)
            .SetTarget(target);
    }

    public static Tweener DOFade(this UITransitionEffect target, float endValue, float duration)
    {
        return DOTween.To(() => target.effectFactor, x => target.effectFactor = x, endValue, duration)
            .SetTarget(target);
    }

    public static Tweener DOShiny(this UIShiny target, float endValue, float duration)
    {
        return DOTween.To(() => target.effectFactor, x => target.effectFactor = x, endValue, duration)
            .SetTarget(target);
    }

    public static Tweener DODissolve(this UIDissolve target, float endValue, float duration)
    {
        return DOTween.To(() => target.effectFactor, x => target.effectFactor = x, endValue, duration)
            .SetTarget(target);
    }
    #endregion
}