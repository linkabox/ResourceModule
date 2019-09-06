using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

[ExecuteInEditMode]
public class HighlightClicker : MonoBehaviour, IPointerClickHandler
{
    [FormerlySerializedAs("onClick")]
    [SerializeField]
    private Button.ButtonClickedEvent m_OnClick = new Button.ButtonClickedEvent();

    public Button.ButtonClickedEvent onClick
    {
        get
        {
            return this.m_OnClick;
        }
        set
        {
            this.m_OnClick = value;
        }
    }

    static readonly Vector2 s_Center = new Vector2(0.5f, 0.5f);

    [Tooltip("Fit graphic's transform to target transform.")]
    [SerializeField] RectTransform m_FitTarget;
    [Tooltip("Fit graphic's transform to target transform on LateUpdate every frame.")]
    [SerializeField] bool m_FitOnLateUpdate;

    private RectTransform m_RectTransform;
    public RectTransform rectTransform
    {
        get
        {
            if (m_RectTransform == null)
            {
                m_RectTransform = this.transform as RectTransform;
            }
            return m_RectTransform;
        }
    }

    /// <summary>
    /// Fit graphic's transform to target transform.
    /// </summary>
    public RectTransform fitTarget
    {
        get { return m_FitTarget; }
        set
        {
            m_FitTarget = value;
            FitTo(m_FitTarget);
        }
    }

    /// <summary>
    /// Fit graphic's transform to target transform on LateUpdate every frame.
    /// </summary>
    public bool fitOnLateUpdate { get { return m_FitOnLateUpdate; } set { m_FitOnLateUpdate = value; } }

    /// <summary>
    /// Fit to target transform.
    /// </summary>
    /// <param name="target">Target transform.</param>
    public void FitTo(RectTransform target)
    {
        if (target == null) return;
        var rt = rectTransform;

        rt.position = target.position;
        rt.rotation = target.rotation;

        var s1 = target.lossyScale;
        var s2 = rt.parent.lossyScale;
        rt.localScale = new Vector3(s1.x / s2.x, s1.y / s2.y, s1.z / s2.z);
        rt.sizeDelta = target.rect.size;
        rt.anchorMax = rt.anchorMin = s_Center;
    }

    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// </summary>
    void OnEnable()
    {
        FitTo(m_FitTarget);
    }

    /// <summary>
    /// LateUpdate is called every frame, if the Behaviour is enabled.
    /// </summary>
    void LateUpdate()
    {
#if UNITY_EDITOR
        if (m_FitTarget && (m_FitOnLateUpdate || !Application.isPlaying))
#else
			if (m_FitTarget && m_FitOnLateUpdate)
#endif
        {
            FitTo(m_FitTarget);
        }
    }

    //监听点击
    public void OnPointerClick(PointerEventData eventData)
    {
        if (m_FitTarget != null)
        {
            var clickGo = m_FitTarget.gameObject;
            this.m_OnClick.Invoke();
            ExecuteEvents.Execute(clickGo, eventData, ExecuteEvents.pointerClickHandler);
        }
    }
}