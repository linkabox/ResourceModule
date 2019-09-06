using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimationEventTrigger : MonoBehaviour
{
    [Serializable]
    public class AnimIntEvent : UnityEvent<int>
    {

    }

    [Serializable]
    public class AnimFloatEvent : UnityEvent<float>
    {

    }

    [Serializable]
    public class AnimStringEvent : UnityEvent<string>
    {

    }

    [Serializable]
    public class AnimObjectEvent : UnityEvent<UnityEngine.Object>
    {

    }

    public AnimIntEvent onIntEvent = new AnimIntEvent();
    public AnimFloatEvent onFloatEvent = new AnimFloatEvent();
    public AnimObjectEvent onObjEvent = new AnimObjectEvent();
    public AnimStringEvent onStringEvent = new AnimStringEvent();
    // Use this for initialization
    void Start()
    {

    }

    public void TriggerIntEvent(int param)
    {
        //Debug.LogError(param);
        onIntEvent.Invoke(param);
    }

    public void TriggerFloatEvent(float param)
    {
        //Debug.LogError(param);
        onFloatEvent.Invoke(param);
    }

    public void TriggerObjEvent(UnityEngine.Object param)
    {
        //Debug.LogError(param);
        onObjEvent.Invoke(param);
    }

    public void TriggerStringEvent(string param)
    {
        //Debug.LogError(param);
        onStringEvent.Invoke(param);
    }

    public void Dispose()
    {
        onIntEvent.RemoveAllListeners();
        onFloatEvent.RemoveAllListeners();
        onObjEvent.RemoveAllListeners();
        onStringEvent.RemoveAllListeners();
    }

    void OnDestroy()
    {
        Dispose();
    }
}
