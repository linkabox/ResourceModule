using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class UIOverlayCam : MonoBehaviour
{
    private Camera mCam;

    void Awake()
    {
        mCam = this.GetComponent<Camera>();
    }

    // Use this for initialization
    void OnEnable()
    {
        ResetCamera();
    }

#if UNITY_EDITOR
    private int last_sw;
    private int last_sh;
    void Update()
    {
        if (last_sw != Screen.width || last_sh != Screen.height)
        {
            last_sw = Screen.width;
            last_sh = Screen.height;
            ResetCamera();
        }
    }
#endif

    [ContextMenu("ResetCamera")]
    public void ResetCamera()
    {
        if (mCam == null) return;

        float sw = Screen.width;
        float sh = Screen.height;

        mCam.orthographicSize = sh / 2f;
        mCam.transform.localPosition = new Vector3(sw / 2f, sh / 2f, 0);
        Debug.LogFormat("Reset UIOverlayCam:({0},{1})  {2}  {3}", sw, sh, mCam.orthographicSize, mCam.transform.localPosition);
    }
}
