using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class UGUIHUDAnchor : MonoBehaviour
{
    public Transform target;
    public Camera GameCam;
    public Camera UICam;
    public Vector3 offset;

    private RectTransform mTrans;

    void Awake()
    {
        mTrans = this.transform as RectTransform;
    }

    public void SetTarget(Transform target, bool once = false)
    {
        this.target = target;
        this.enabled = !once;
        UpdatePos();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        UpdatePos();
    }

    public void UpdatePos()
    {
        if (target == null || GameCam == null) return;

        Vector3 pos = ConvertPos(GameCam, UICam, target.position);
        mTrans.position = pos;
        mTrans.localPosition += offset;
        //Debug.LogError("UpdatePos:" + pos);
    }

    public static Vector3 ConvertPos(Camera gameCam, Camera uiCam, Vector3 worldPos)
    {
        Vector3 result = gameCam.WorldToScreenPoint(worldPos);
        if (uiCam != null)
        {
            result = uiCam.ScreenToWorldPoint(result);
        }
        return result;
    }
}
