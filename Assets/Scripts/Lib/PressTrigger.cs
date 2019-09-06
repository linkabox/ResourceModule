using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class PressTrigger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler
{
	[Serializable]
	public class PressEvent : UnityEvent<bool>
	{

	}

	[SerializeField]
	[Tooltip("How long must pointer be down on this object to trigger a long press")]
	private float holdTime = 0.2f;

	public PressEvent onPress = new PressEvent();
	public UnityEvent onClick = new UnityEvent();

	private bool isPointerDown = false;
	private bool longPressed = false;
	private float timePressStarted;

	void Update()
	{
		if (isPointerDown && !longPressed)
		{
			if (Time.time - timePressStarted > holdTime)
			{
				longPressed = true;
				onPress.Invoke(true);
#if UI_DEBUG
				Debug.LogError("OnLongPress:" + this.gameObject);
#endif
			}
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (eventData.button != PointerEventData.InputButton.Left)
			return;

		timePressStarted = Time.time;
		isPointerDown = true;
		longPressed = false;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (eventData.button != PointerEventData.InputButton.Left)
			return;

		isPointerDown = false;
		if (longPressed)
		{
			onPress.Invoke(false);
#if UI_DEBUG
			Debug.LogError("OnRelease:" + eventData.pointerPress);
#endif
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		isPointerDown = false;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!longPressed)
		{
			onClick.Invoke();
#if UI_DEBUG
			Debug.LogError("OnClick:" + eventData.pointerPress);
#endif
		}
	}

	public void Dispose()
	{
		onPress.RemoveAllListeners();
		onClick.RemoveAllListeners();
	}

	void OnDestroy()
	{
		Dispose();
	}
}

