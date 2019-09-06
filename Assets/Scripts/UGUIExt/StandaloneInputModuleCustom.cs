using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StandaloneInputModuleCustom : StandaloneInputModule
{
    public PointerEventData LastPointerEventData
    {
        get
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            return GetLastPointerEventData(PointerInputModule.kMouseLeftId);
#else
            return GetLastPointerEventData(0);
#endif
        }
    }

    public PointerEventData GetLastPointerEvent(int id)
    {
        return GetLastPointerEventData(id);
    }
}